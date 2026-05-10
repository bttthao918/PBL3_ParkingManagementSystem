// Models/Parking/ParkingSlotDto.cs
namespace ParkingManagement.FE.Models.Parking
{
    public class ParkingSlotDto
    {
        public string SlotId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class AvailableSlotDto
    {
        public string SlotId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
    }
}