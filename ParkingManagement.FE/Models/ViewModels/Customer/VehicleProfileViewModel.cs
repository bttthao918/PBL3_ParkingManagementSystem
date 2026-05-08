namespace ParkingManagement.FE.Models.ViewModels.Customer
{
    public class VehicleProfileViewModel
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string PlateNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
    }
}
