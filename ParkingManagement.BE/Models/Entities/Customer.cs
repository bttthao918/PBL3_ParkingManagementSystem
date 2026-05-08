using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        [MaxLength(20)]
        public string CustomerId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string AccountId { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; } // Male, Female, Other

        public bool IsDeleted { get; set; } = false;

        // Navigation
        [ForeignKey("AccountId")]
        public Account Account { get; set; } = null!;

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
