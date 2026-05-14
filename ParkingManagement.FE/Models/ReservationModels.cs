namespace ParkingManagement.FE.Models
{
    // ── List/Get DTOs ──
    public class ListReservationDto
    {
        public List<ReservationDetailDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class ReservationDetailDto
    {
        public string ReservationId { get; set; } = "";
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string? SlotId { get; set; }
        public string? SlotLocation { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }

    // ── Create DTOs ──
    public class CreateReservationDto
    {
        public string? CustomerId { get; set; }
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "Xe máy";
        public string? PreferredSlotId { get; set; }
        public DateTime ExpectedTime { get; set; }
    }

    // ── Available Slots ──
    public class AvailableSlotDto
    {
        public string SlotId { get; set; } = "";
        public string Location { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
