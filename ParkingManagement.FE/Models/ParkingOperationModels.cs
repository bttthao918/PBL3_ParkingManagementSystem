namespace ParkingManagement.FE.Models
{
    // ── Check-in Validation Response ──
    public class CheckInValidationResponse
    {
        public bool HasVehicleRecord { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public bool HasMonthlyTicket { get; set; }
        public string? MonthlyTicketId { get; set; }
        public DateTime? MonthlyTicketExpiryDate { get; set; }
        public bool HasReservation { get; set; }
        public string? ReservationId { get; set; }
        public string? PreferredSlotId { get; set; }
        public List<AvailableSlotItem> AvailableSlots { get; set; } = new();
        public string? Message { get; set; }
    }

    public class AvailableSlotItem
    {
        public string SlotId { get; set; } = "";
        public string Location { get; set; } = "";
        public string VehicleType { get; set; } = "";
    }

    // ── Check-in Result Response ──
    public class CheckInResultResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TicketId { get; set; }
        public string? SlotId { get; set; }
        public DateTime? CheckInTime { get; set; }
    }

    // ── Check-out Validation Response ──
    public class CheckOutValidationResponse
    {
        public bool Success { get; set; }
        public string? TicketId { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleType { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CurrentTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? TicketType { get; set; }
        public bool IsFreeTicket { get; set; }
        public decimal CalculatedFee { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountHolder { get; set; }
        public string? BankTransferContent { get; set; }
        public string? BankTransferQrUrl { get; set; }
        public string? Message { get; set; }
    }

    // ── Check-out Result Response ──
    public class CheckOutResultResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TicketId { get; set; }
        public string? VehiclePlate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Fee { get; set; }
        public bool IsFree { get; set; }
        public string? PaymentId { get; set; }
    }
}
