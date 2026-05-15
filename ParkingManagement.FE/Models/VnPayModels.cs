namespace ParkingManagement.FE.Models
{
    public class CreateVnPayPaymentRequestDto
    {
        public string? TicketId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Chuyển khoản";
        public string Description { get; set; } = "";
    }

    public class CreateVnPayPaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? PaymentUrl { get; set; }
        public string? PaymentId { get; set; }
        public string? TxnRef { get; set; }
    }
}
