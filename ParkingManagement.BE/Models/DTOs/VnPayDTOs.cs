namespace ParkingManagement.BLL.DTOs
{
    /// <summary>
    /// DTO để tạo URL thanh toán VNPay
    /// </summary>
    public class VnPayCreateUrlDto
    {
        public string TxnRef { get; set; } = "";
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = "";
        public string? OrderType { get; set; } = "other";
    }

    /// <summary>
    /// DTO kết quả validate response từ VNPay
    /// </summary>
    public class VnPayResponseDto
    {
        public bool Success { get; set; }
        public bool IsValidHash { get; set; }
        public string TxnRef { get; set; } = "";
        public string ResponseCode { get; set; } = "";
        public string TransactionNo { get; set; } = "";
        public string OrderInfo { get; set; } = "";
        public string PayDate { get; set; } = "";
        public decimal Amount { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Request tạo thanh toán VNPay từ FE
    /// </summary>
    public class CreateVnPayPaymentRequest
    {
        public string? TicketId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Chuyển khoản";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Response trả về cho FE khi tạo VNPay payment
    /// </summary>
    public class CreateVnPayPaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? PaymentUrl { get; set; }
        public string? PaymentId { get; set; }
        public string? TxnRef { get; set; }
    }
}
