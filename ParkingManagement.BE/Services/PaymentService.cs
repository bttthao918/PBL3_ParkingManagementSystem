using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.BLL.Constants;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;
using System.Text;

namespace ParkingManagement.BLL.Services.Implementations
{
    /// <summary>
    /// UC010 - Payment Service
    /// Xử lý logic thanh toán cho vé lượt & vé tháng
    /// 
    /// Quy trình thanh toán:
    /// 1. Chọn phương thức thanh toán
    /// 2. Lấy thông tin thanh toán (ngân hàng/ví/...)
    /// 3. Khách quét QR Code hoặc thanh toán trực tiếp
    /// 4. Xác nhận thanh toán
    /// 5. Cập nhật trạng thái vé
    /// 
    /// Các phương thức:
    /// - Tiền mặt: tính tiền thừa
    /// - Chuyển khoản: hiển thị QR Code
    /// - Ví điện tử: hiển thị QR Code + link thanh toán
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IMonthlyTicketRepository _monthlyTicketRepo;
        private readonly IPaymentValidator _validator;

        public PaymentService(
            IPaymentRepository paymentRepo,
            ITicketRepository ticketRepo,
            IMonthlyTicketRepository monthlyTicketRepo,
            IPaymentValidator validator)
        {
            _paymentRepo = paymentRepo;
            _ticketRepo = ticketRepo;
            _monthlyTicketRepo = monthlyTicketRepo;
            _validator = validator;
        }

        /// <summary>
        /// Lấy thông tin phương thức thanh toán
        /// </summary>
        public async Task<PaymentMethodInfoDto> GetPaymentMethodInfoAsync(string paymentMethod, decimal amount)
        {
            // Validate phương thức
            var methodValidation = _validator.ValidatePaymentMethod(paymentMethod);
            if (!methodValidation.IsValid)
                return new PaymentMethodInfoDto { Success = false, Message = methodValidation.ErrorMessage };

            // Validate số tiền
            var amountValidation = _validator.ValidateAmount(amount);
            if (!amountValidation.IsValid)
                return new PaymentMethodInfoDto { Success = false, Message = amountValidation.ErrorMessage };

            try
            {
                var response = new PaymentMethodInfoDto
                {
                    Success = true,
                    PaymentMethod = paymentMethod
                };

                if (paymentMethod == PaymentMethods.CASH)
                {
                    response.CashPayment = new CashPaymentDto
                    {
                        Amount = amount,
                        ReceivedAmount = 0,
                        Change = 0
                    };
                }
                else if (paymentMethod == PaymentMethods.BANK_TRANSFER)
                {
                    var qrCode = await GenerateQRCodeForBankTransferAsync(amount);
                    response.BankTransfer = new BankTransferPaymentDto
                    {
                        BankName = BankInfo.BANK_NAME,
                        BankAccount = BankInfo.BANK_ACCOUNT,
                        AccountHolder = BankInfo.ACCOUNT_HOLDER,
                        Amount = amount,
                        TransferContent = $"Thanh toan ve {DateTime.Now:ddMMyyy}",
                        QRCodeBase64 = qrCode,
                        QRCodeUrl = $"https://api.vietqr.io/merged/{BankInfo.BANK_ACCOUNT}/{BankInfo.ACCOUNT_HOLDER.Replace(" ", "%20")}/{amount}"
                    };
                }
                else if (paymentMethod == PaymentMethods.E_WALLET)
                {
                    var momoQRCode = await GenerateQRCodeForEWalletAsync("Momo", amount);
                    var momoLink = await GeneratePaymentLinkAsync("Momo", amount);

                    response.EWallet = new EWalletPaymentDto
                    {
                        WalletType = "Momo",
                        PhoneNumber = EWalletInfo.MOMO_PHONE,
                        Amount = amount,
                        QRCodeBase64 = momoQRCode,
                        PaymentLink = momoLink
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                return new PaymentMethodInfoDto
                {
                    Success = false,
                    Message = $"Lỗi lấy thông tin thanh toán: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xác nhận thanh toán
        /// </summary>
        public async Task<ServiceResult<PaymentCompletedDto>> ConfirmPaymentAsync(ConfirmPaymentDto confirmDto)
        {
            try
            {
                // Validate input
                var validation = _validator.ValidateConfirmPayment(confirmDto);
                if (!validation.IsValid)
                    return ServiceResult<PaymentCompletedDto>.Fail(validation.ErrorMessage!);

                // Kiểm tra vé tồn tại
                var ticket = await _ticketRepo.GetByIdAsync(confirmDto.TicketId);
                if (ticket == null)
                    return ServiceResult<PaymentCompletedDto>.Fail("Vé không tồn tại");

                // Kiểm tra vé chưa thanh toán
                var existingPayment = await _paymentRepo.GetByTicketIdAsync(confirmDto.TicketId);
                if (existingPayment != null)
                    return ServiceResult<PaymentCompletedDto>.Fail("Vé này đã thanh toán rồi");

                // Tạo bản ghi Payment
                var paymentId = await _paymentRepo.GenerateIdAsync();
                var payment = new Payment
                {
                    PaymentId = paymentId,
                    TicketId = confirmDto.TicketId,
                    Amount = confirmDto.Amount,
                    Method = confirmDto.PaymentMethod,
                    PaymentTime = DateTime.Now,
                    Status = PaymentStatuses.COMPLETED
                };

                await _paymentRepo.AddAsync(payment);

                // Trả về kết quả
                var result = new PaymentCompletedDto
                {
                    Success = true,
                    PaymentId = paymentId,
                    TicketId = confirmDto.TicketId,
                    PaymentMethod = confirmDto.PaymentMethod,
                    Amount = confirmDto.Amount,
                    PaymentTime = payment.PaymentTime,
                    Change = confirmDto.PaymentMethod == PaymentMethods.CASH && confirmDto.ReceivedAmount.HasValue
                        ? confirmDto.ReceivedAmount.Value - confirmDto.Amount
                        : null
                };

                return ServiceResult<PaymentCompletedDto>.Ok(result, $"Thanh toán {paymentId} thành công");
            }
            catch (Exception ex)
            {
                return ServiceResult<PaymentCompletedDto>.Fail($"Lỗi xác nhận thanh toán: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy chi tiết thanh toán
        /// </summary>
        public async Task<PaymentCompletedDto?> GetPaymentByIdAsync(string paymentId)
        {
            try
            {
                var payment = await _paymentRepo.GetByTicketIdAsync(paymentId);
                if (payment == null)
                    return null;

                return MapToPaymentCompletedDto(payment);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Lấy danh sách thanh toán của vé
        /// </summary>
        public async Task<List<PaymentCompletedDto>> GetPaymentsByTicketAsync(string ticketId)
        {
            try
            {
                var payment = await _paymentRepo.GetByTicketIdAsync(ticketId);
                if (payment == null)
                    return new List<PaymentCompletedDto>();

                return new List<PaymentCompletedDto> { MapToPaymentCompletedDto(payment) };
            }
            catch
            {
                return new List<PaymentCompletedDto>();
            }
        }

        /// <summary>
        /// Lấy danh sách thanh toán của vé tháng
        /// </summary>
        public async Task<List<PaymentCompletedDto>> GetPaymentsByMonthlyTicketAsync(string monthlyTicketId)
        {
            try
            {
                var payment = await _paymentRepo.GetByMonthlyTicketIdAsync(monthlyTicketId);
                if (payment == null)
                    return new List<PaymentCompletedDto>();

                return new List<PaymentCompletedDto> { MapToPaymentCompletedDto(payment) };
            }
            catch
            {
                return new List<PaymentCompletedDto>();
            }
        }

        /// <summary>
        /// Lấy tất cả thanh toán
        /// </summary>
        public async Task<List<PaymentCompletedDto>> GetAllPaymentsAsync()
        {
            try
            {
                var payments = await _paymentRepo.GetAllAsync();
                return payments.Select(MapToPaymentCompletedDto).ToList();
            }
            catch
            {
                return new List<PaymentCompletedDto>();
            }
        }

        /// <summary>
        /// Lấy thanh toán trong khoảng thời gian
        /// </summary>
        public async Task<List<PaymentCompletedDto>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
        {
            try
            {
                var payments = await _paymentRepo.GetByDateRangeAsync(from, to);
                return payments.Select(MapToPaymentCompletedDto).ToList();
            }
            catch
            {
                return new List<PaymentCompletedDto>();
            }
        }

        /// <summary>
        /// Tạo QR Code cho chuyển khoản
        /// Sử dụng VietQR API (mock data)
        /// </summary>
        public async Task<string> GenerateQRCodeForBankTransferAsync(decimal amount)
        {
            try
            {
                // Mock: Generate base64 QR code
                // Trong thực tế, anh sẽ tích hợp VietQR API hoặc QRCodeGenerator library
                var qrContent = $"00020126380014vn.com.vietqr01051000020570802012300000000{amount}263042100312{DateTime.Now:yyyyMMddHHmmss}630406450000";
                var qrBytes = Encoding.UTF8.GetBytes(qrContent);
                return Convert.ToBase64String(qrBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Tạo QR Code cho ví điện tử
        /// </summary>
        public async Task<string> GenerateQRCodeForEWalletAsync(string walletType, decimal amount)
        {
            try
            {
                // Mock: Generate base64 QR code cho Momo hoặc ZaloPay
                var phoneNumber = walletType == "Momo" ? EWalletInfo.MOMO_PHONE : EWalletInfo.ZALOPAY_PHONE;
                var qrContent = $"{walletType}|{phoneNumber}|{amount}|{DateTime.Now:yyyyMMddHHmmss}";
                var qrBytes = Encoding.UTF8.GetBytes(qrContent);
                return Convert.ToBase64String(qrBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Tạo link thanh toán cho ví điện tử
        /// </summary>
        public async Task<string> GeneratePaymentLinkAsync(string walletType, decimal amount)
        {
            try
            {
                if (walletType == "Momo")
                {
                    // Momo payment link format
                    return $"https://momo.vn/qr/{EWalletInfo.MOMO_PHONE}?amount={amount}";
                }
                else if (walletType == "ZaloPay")
                {
                    // ZaloPay payment link format
                    return $"https://zalopay.vn/qr/{EWalletInfo.ZALOPAY_PHONE}?amount={amount}";
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // ──── Helper Methods ────────────────────────────────────────

        private PaymentCompletedDto MapToPaymentCompletedDto(Payment payment)
        {
            return new PaymentCompletedDto
            {
                Success = true,
                PaymentId = payment.PaymentId,
                TicketId = payment.TicketId,
                Amount = payment.Amount,
                PaymentMethod = payment.Method,
                PaymentTime = payment.PaymentTime,
                Message = $"Thanh toán {payment.Status.ToLower()}"
            };
        }
    }
}
