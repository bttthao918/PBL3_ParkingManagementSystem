// Models/Ticket/TicketDto.cs
namespace ParkingManagement.FE.Models.Ticket
{
    public class CheckInRequest
    {
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string? SlotId { get; set; }
        public string? CustomerId { get; set; }
    }

    public class CheckOutRequest
    {
        public string PaymentMethod { get; set; } = "Tiền mặt";
    }

    public class CheckInResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
    }

    public class CheckOutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public DateTime CheckOutTime { get; set; }
    }

    public class TicketDto
    {
        public string TicketId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string? SlotId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}