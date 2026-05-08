using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        [MaxLength(20)]
        public string ReservationId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string CustomerId { get; set; } = null!;

        [MaxLength(20)]
        public string? VehiclePlate { get; set; }

        [MaxLength(20)]
        public string? SlotId { get; set; }

        [Required]
        public DateTime ExpectedTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Chờ"; // Chờ / Đã nhận / Hủy / Hết hạn

        // Navigation
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = null!;

        [ForeignKey("VehiclePlate")]
        public Vehicle? Vehicle { get; set; }

        [ForeignKey("SlotId")]
        public ParkingSlot? ParkingSlot { get; set; }
    }
}
