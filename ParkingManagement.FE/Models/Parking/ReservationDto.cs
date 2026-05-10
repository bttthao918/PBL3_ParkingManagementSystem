// Models/Parking/ReservationDto.cs
namespace ParkingManagement.FE.Models.Parking
{
    public class CreateReservationRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime ExpectedTime { get; set; }
        public string? PreferredSlotId { get; set; }
    }

    public class ReservationDto
    {
        public string ReservationId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string? SlotId { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ReservationDetailDto : ReservationDto
    {
        public string? CustomerName { get; set; }
        public string? ParkingSlotLocation { get; set; }
    }

    public class ListReservationDto
    {
        public List<ReservationDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class FilterReservationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
    }
}