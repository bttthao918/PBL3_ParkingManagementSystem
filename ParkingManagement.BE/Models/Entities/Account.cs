using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Accounts")]
    public class Account
    {
        [Key]
        [MaxLength(20)]
        public string AccountId { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = null!; // Customer / Employee / Manager

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public bool RequirePasswordChange { get; set; } = false; // Flag: Bắt buộc đổi password lần đầu

        // Navigation
        public Customer? Customer { get; set; }
        public Employee? Employee { get; set; }
        public Manager? Manager { get; set; }
    }
}
