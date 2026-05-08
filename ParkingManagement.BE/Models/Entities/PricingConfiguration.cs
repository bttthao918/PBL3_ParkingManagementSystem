using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("PricingConfigurations")]
    public class PricingConfiguration
    {
        [Key]
        public string PricingId { get; set; } = null!;

        /// <summary>
        /// Loại xe: "Xe máy", "Ô tô nhỏ", "Ô tô lớn"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string VehicleType { get; set; } = null!;

        /// <summary>
        /// Loại giá: "HourlyRate", "MaxDailyFee", "Monthly1M", "Monthly3M", "Monthly6M"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RateType { get; set; } = null!;

        /// <summary>
        /// Số tiền (VND)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Thời gian cập nhật
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Người cập nhật (ManagerId)
        /// </summary>
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }
    }
}
