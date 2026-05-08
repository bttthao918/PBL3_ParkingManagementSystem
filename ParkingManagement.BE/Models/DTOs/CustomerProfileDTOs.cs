namespace ParkingManagement.BLL.DTOs
{
    // ── UPDATE PROFILE ────────────────────────────────────────
    public class UpdateCustomerProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
    }

    public class UpdateProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CustomerProfileDto? Data { get; set; }
    }

    // ── RESERVATION MANAGEMENT ────────────────────────────────────────
    public class ReservationDetailDto
    {
        public string ReservationId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string? VehicleType { get; set; }
        public string? SlotId { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;  // "Chờ", "Đã nhận", "Hủy", "Hết hạn"
    }

    public class ListReservationsDto
    {
        public List<ReservationDetailDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class CreateReservationDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string? VehicleType { get; set; }
        public string? PreferredSlotId { get; set; }
        public DateTime ExpectedTime { get; set; }
    }

    public class CancelReservationDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ── TICKET MANAGEMENT ────────────────────────────────────────
    public class TicketDetailDto
    {
        public string TicketId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string? VehicleType { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public bool HasActiveMonthlyTicket { get; set; }
    }

    public class ListTicketsDto
    {
        public List<TicketDetailDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
    }

    // ── MONTHLY TICKET MANAGEMENT ────────────────────────────────────────
    public class MonthlyTicketDetailDto
    {
        public string MonthlyTicketId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string? VehicleType { get; set; }
        public string PackageType { get; set; } = string.Empty;  // "1 tháng", "3 tháng", "6 tháng"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = string.Empty;       // "Hoạt động", "Hết hạn", "Chờ thanh toán"
        public int DaysRemaining { get; set; }
    }

    public class ListMonthlyTicketsDto
    {
        public List<MonthlyTicketDetailDto> Items { get; set; } = new();
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
    }

    public class RegisterMonthlyTicketDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string? VehicleType { get; set; }
        public string PackageType { get; set; } = string.Empty;  // "1 tháng", "3 tháng", "6 tháng"
        public string? PaymentMethod { get; set; }
    }

    public class RegisterMonthlyTicketResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public MonthlyTicketDetailDto? Data { get; set; }
    }

    public class RenewMonthlyTicketDto
    {
        public string PackageType { get; set; } = string.Empty;
    }

    public class RenewMonthlyTicketResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal AdditionalFee { get; set; }
        public MonthlyTicketDetailDto? Data { get; set; }
    }

    // ── PAYMENT MANAGEMENT ────────────────────────────────────────
    public class PaymentHistoryDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string? VehiclePlate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ListPaymentHistoryDto
    {
        public List<PaymentHistoryDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class ProcessPaymentDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public string? TicketId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime PaymentTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public int TicketPayments { get; set; }
        public int MonthlyTicketPayments { get; set; }
        public int Count { get; set; }
    }
}
