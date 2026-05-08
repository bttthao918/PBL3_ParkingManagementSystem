using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Otps")]
    public class Otp
    {
        [Key]
        [MaxLength(20)]
        public string OtpId { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(6)]
        public string Code { get; set; } = null!; // 6 chữ số

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; } // Hết hạn 5 phút

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }
    }
}
