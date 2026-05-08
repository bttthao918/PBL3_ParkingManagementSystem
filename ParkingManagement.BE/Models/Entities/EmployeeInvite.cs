using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("EmployeeInvites")]
    public class EmployeeInvite
    {
        [Key]
        [MaxLength(50)]
        public string InviteToken { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string EmployeeCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string? Shift { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiryTime { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
