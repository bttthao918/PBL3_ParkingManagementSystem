using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Managers")]
    public class Manager
    {
        [Key]
        [MaxLength(20)]
        public string ManagerId { get; set; } = null!;

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

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
