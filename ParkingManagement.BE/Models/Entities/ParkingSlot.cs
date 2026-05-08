using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("ParkingSlots")]
    public class ParkingSlot
    {
        [Key]
        [MaxLength(20)]
        public string SlotId { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Location { get; set; } = null!; // VD: "Khu A - Ô 01"

        [Required]
        [MaxLength(20)]
        public string VehicleType { get; set; } = null!; // Xe máy / Ô tô nhỏ / Ô tô lớn

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Trống"; // Trống / Đang sử dụng / Đã đặt / Bảo trì

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
