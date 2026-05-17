using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class PaymentResultModel : PageModel
    {
        private readonly ILogger<PaymentResultModel> _logger;
        private readonly ICustomerApiService _customerApiService;

        public PaymentResultModel(
            ILogger<PaymentResultModel> logger,
            ICustomerApiService customerApiService)
        {
            _logger = logger;
            _customerApiService = customerApiService;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public string? TxnRef { get; set; }
        public string? TransactionNo { get; set; }
        public decimal Amount { get; set; }
        public string? PayDate { get; set; }
        public string? OrderInfo { get; set; }
        public string UserName { get; set; } = "Customer";

        public async Task OnGetAsync()
        {
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
            ViewData["Title"] = "Kết quả thanh toán";
            ViewData["UserName"] = UserName;
            ViewData["Role"] = "Khách hàng";

            var payOsOrderCode = Request.Query["orderCode"].ToString();
            var payOsStatus = Request.Query["status"].ToString();
            var payOsCode = Request.Query["code"].ToString();
            var payOsPaymentLinkId = Request.Query["id"].ToString();
            var payOsCancelled = string.Equals(Request.Query["cancel"].ToString(), "true", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(payOsOrderCode) ||
                !string.IsNullOrWhiteSpace(payOsStatus) ||
                !string.IsNullOrWhiteSpace(payOsCode))
            {
                TxnRef = payOsOrderCode;
                TransactionNo = payOsPaymentLinkId;
                OrderInfo = "payOS VietQR";
                IsSuccess = !payOsCancelled &&
                    string.Equals(payOsStatus, "PAID", StringComparison.OrdinalIgnoreCase);
                Message = payOsCancelled
                    ? "Bạn đã hủy thanh toán payOS."
                    : IsSuccess
                        ? "Thanh toán payOS đã hoàn tất. Vé tháng sẽ chuyển sang hoạt động sau khi webhook xác nhận giao dịch."
                        : "Thanh toán payOS chưa hoàn tất hoặc đang chờ ngân hàng xác nhận.";

                if (IsSuccess && long.TryParse(payOsOrderCode, out var orderCode))
                {
                    var confirmResult = await _customerApiService.ConfirmPayOsReturnAsync(orderCode);
                    if (confirmResult.Success)
                    {
                        Message = confirmResult.Message;
                    }
                    else
                    {
                        IsSuccess = false;
                        Message = confirmResult.Message;
                    }
                }

                _logger.LogInformation("PayOS Return: Code={Code}, Status={Status}, OrderCode={OrderCode}, Success={Success}",
                    payOsCode,
                    payOsStatus,
                    payOsOrderCode,
                    IsSuccess);
                return;
            }

            // Parse VNPay return parameters
            var vnpResponseCode = Request.Query["vnp_ResponseCode"].ToString();
            var vnpTxnRef = Request.Query["vnp_TxnRef"].ToString();
            var vnpAmount = Request.Query["vnp_Amount"].ToString();
            var vnpTransactionNo = Request.Query["vnp_TransactionNo"].ToString();
            var vnpPayDate = Request.Query["vnp_PayDate"].ToString();
            var vnpOrderInfo = Request.Query["vnp_OrderInfo"].ToString();

            TxnRef = vnpTxnRef;
            TransactionNo = vnpTransactionNo;
            OrderInfo = vnpOrderInfo;

            if (long.TryParse(vnpAmount, out var amountLong))
            {
                Amount = amountLong / 100m;
            }

            if (!string.IsNullOrEmpty(vnpPayDate) && vnpPayDate.Length >= 14)
            {
                PayDate = $"{vnpPayDate[6..8]}/{vnpPayDate[4..6]}/{vnpPayDate[..4]} {vnpPayDate[8..10]}:{vnpPayDate[10..12]}";
            }

            IsSuccess = vnpResponseCode == "00";
            Message = IsSuccess
                ? "Thanh toán thành công!"
                : GetErrorMessage(vnpResponseCode);

            _logger.LogInformation("VNPay Return: Code={Code}, TxnRef={TxnRef}, Success={Success}",
                vnpResponseCode, vnpTxnRef, IsSuccess);
        }

        private static string GetErrorMessage(string responseCode)
        {
            return responseCode switch
            {
                "07" => "Trừ tiền thành công nhưng giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần.",
                "11" => "Đã hết hạn chờ thanh toán. Vui lòng thực hiện lại giao dịch.",
                "12" => "Thẻ/Tài khoản bị khóa.",
                "13" => "Nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Khách hàng hủy giao dịch.",
                "51" => "Tài khoản không đủ số dư để thực hiện giao dịch.",
                "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định.",
                "99" => "Lỗi không xác định.",
                _ => $"Giao dịch không thành công (Mã lỗi: {responseCode})."
            };
        }
    }
}
