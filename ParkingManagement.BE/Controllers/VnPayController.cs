using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Interfaces;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.Web.Controllers.Api
{
    [ApiController]
    [Route("api/vnpay")]
    [Produces("application/json")]
    public class VnPayController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IMonthlyTicketRepository _monthlyTicketRepo;
        private readonly ILogger<VnPayController> _logger;

        public VnPayController(
            IVnPayService vnPayService,
            IPaymentRepository paymentRepo,
            ITicketRepository ticketRepo,
            IMonthlyTicketRepository monthlyTicketRepo,
            ILogger<VnPayController> logger)
        {
            _vnPayService = vnPayService;
            _paymentRepo = paymentRepo;
            _ticketRepo = ticketRepo;
            _monthlyTicketRepo = monthlyTicketRepo;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        [HttpPost("create-payment")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] CreateVnPayPaymentRequest request)
        {
            try
            {
                if (!_vnPayService.IsConfigured)
                    return BadRequest(new CreateVnPayPaymentResponse
                    {
                        Success = false,
                        Message = "VNPay chưa được cấu hình trên hệ thống."
                    });

                if (request.Amount <= 0)
                    return BadRequest(new CreateVnPayPaymentResponse
                    {
                        Success = false,
                        Message = "Số tiền không hợp lệ."
                    });

                // Generate unique TxnRef
                var txnRef = $"{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";

                // Create pending payment record
                var paymentId = await _paymentRepo.GenerateIdAsync();
                var payment = new Payment
                {
                    PaymentId = paymentId,
                    TicketId = request.TicketId,
                    MonthlyTicketId = request.MonthlyTicketId,
                    Amount = request.Amount,
                    Method = request.PaymentMethod,
                    PaymentTime = DateTime.Now,
                    Status = PaymentStatuses.PENDING,
                    VnpTxnRef = txnRef
                };

                await _paymentRepo.AddAsync(payment);

                // Build order info
                var orderInfo = !string.IsNullOrWhiteSpace(request.Description)
                    ? request.Description
                    : request.MonthlyTicketId != null
                        ? $"Thanh toan ve thang {request.MonthlyTicketId}"
                        : $"Thanh toan ve {request.TicketId}";

                // Create VNPay URL
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var paymentUrl = _vnPayService.CreatePaymentUrl(new VnPayCreateUrlDto
                {
                    TxnRef = txnRef,
                    Amount = request.Amount,
                    OrderInfo = orderInfo,
                    OrderType = "billpayment"
                }, clientIp);

                _logger.LogInformation("VNPay payment created: PaymentId={PaymentId}, TxnRef={TxnRef}", paymentId, txnRef);

                return Ok(new CreateVnPayPaymentResponse
                {
                    Success = true,
                    Message = "Tạo thanh toán thành công.",
                    PaymentUrl = paymentUrl,
                    PaymentId = paymentId,
                    TxnRef = txnRef
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreatePayment error");
                return StatusCode(500, new CreateVnPayPaymentResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống khi tạo thanh toán."
                });
            }
        }

        /// <summary>
        /// IPN callback từ VNPay (không cần auth)
        /// VNPay gọi endpoint này khi giao dịch hoàn tất
        /// </summary>
        [HttpGet("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> IpnCallback()
        {
            try
            {
                var response = _vnPayService.ValidateResponse(Request.Query);

                if (!response.IsValidHash)
                {
                    _logger.LogWarning("VNPay IPN: Invalid checksum");
                    return Ok(new { RspCode = "97", Message = "Invalid Checksum" });
                }

                // Find payment by TxnRef
                var payment = await _paymentRepo.GetByVnpTxnRefAsync(response.TxnRef);
                if (payment == null)
                {
                    _logger.LogWarning("VNPay IPN: Order not found. TxnRef={TxnRef}", response.TxnRef);
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                // Check if already confirmed
                if (payment.Status != PaymentStatuses.PENDING)
                {
                    _logger.LogInformation("VNPay IPN: Order already confirmed. TxnRef={TxnRef}", response.TxnRef);
                    return Ok(new { RspCode = "02", Message = "Order already confirmed" });
                }

                // Validate amount
                if (payment.Amount != response.Amount)
                {
                    _logger.LogWarning("VNPay IPN: Amount mismatch. Expected={Expected}, Got={Got}", payment.Amount, response.Amount);
                    return Ok(new { RspCode = "04", Message = "Invalid Amount" });
                }

                // Update payment status
                if (response.ResponseCode == "00")
                {
                    payment.Status = PaymentStatuses.COMPLETED;
                    payment.PaymentTime = DateTime.Now;
                    _logger.LogInformation("VNPay IPN: Payment confirmed. TxnRef={TxnRef}", response.TxnRef);
                }
                else
                {
                    payment.Status = PaymentStatuses.FAILED;
                    _logger.LogInformation("VNPay IPN: Payment failed. TxnRef={TxnRef}, Code={Code}", response.TxnRef, response.ResponseCode);
                }

                await _paymentRepo.UpdateAsync(payment);

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VNPay IPN error");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        /// <summary>
        /// Kiểm tra VNPay đã cấu hình chưa
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        public IActionResult GetStatus()
        {
            return Ok(new { configured = _vnPayService.IsConfigured });
        }
    }
}
