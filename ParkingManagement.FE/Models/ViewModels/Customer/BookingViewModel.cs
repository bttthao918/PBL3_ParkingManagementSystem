namespace ParkingManagement.FE.Models.ViewModels.Customer
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string PlateNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string ParkingSlot { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
    }
}
