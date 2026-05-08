using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Vehicles")]
    public class Vehicle
    {
        [Key]
        [MaxLength(20)]
        public string VehiclePlate { get; set; } = null!; // Biển số xe là PK

        [Required]
        [MaxLength(20)]
        public string VehicleType { get; set; } = null!; // Xe máy / Ô tô nhỏ / Ô tô lớn

        [MaxLength(20)]
        public string? CustomerId { get; set; }

        // Navigation
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
