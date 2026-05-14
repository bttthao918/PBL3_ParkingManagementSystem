namespace ParkingManagement.FE.Models
{
    // ── List/Get DTOs ──
    public class ListPaymentDto
    {
        public List<PaymentItemDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaymentItemDto
    {
        public string PaymentId { get; set; } = "";
        public string? TicketId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public string? VehiclePlate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime PaymentTime { get; set; }
    }

    public class PaymentDetailDto
    {
        public string PaymentId { get; set; } = "";
        public string? TicketId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public string? VehiclePlate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime PaymentTime { get; set; }
        public string? CustomerName { get; set; }
        public string? EmployeeName { get; set; }
    }

    // ── Process Payment DTOs ──
    public class ProcessPaymentDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt"; // "Tiền mặt", "Chuyển khoản", "Ví điện tử"
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string PaymentId { get; set; } = "";
        public string? TicketId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public DateTime PaymentTime { get; set; }
        public string Status { get; set; } = "";
    }

    // ── Summary DTOs ──
    public class PaymentSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public int TicketPayments { get; set; }
        public int MonthlyTicketPayments { get; set; }
        public int Count { get; set; }
    }
}
