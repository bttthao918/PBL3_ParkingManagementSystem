using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.FE.Models.ViewModels.Customer
{
    public class BookingCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập biển số xe")]
        public string PlateNumber { get; set; } = "";

        [Required]
        public string VehicleType { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn vị trí đỗ")]
        public string ParkingSlot { get; set; } = "";

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Required]
        public DateTime EndTime { get; set; } = DateTime.Now.AddHours(2);

        public decimal TotalPrice { get; set; }
    }
}
