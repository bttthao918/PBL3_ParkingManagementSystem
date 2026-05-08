using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    /// <summary>
    /// Audit log cho các thay đổi trạng thái chỗ đỗ
    /// UC005.2 - Cấp nhật trạng thái chỗ đỗ
    /// </summary>
    [Table("ParkingSlotAuditLogs")]
    public class ParkingSlotAuditLog
    {
        [Key]
        [MaxLength(20)]
        public string LogId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string SlotId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string EmployeeId { get; set; } = null!; // Nhân viên thực hiện thay đổi

        [Required]
        [MaxLength(30)]
        public string OldStatus { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string NewStatus { get; set; } = null!;

        [MaxLength(500)]
        public string? Note { get; set; } // Ghi chú lý do thay đổi

        public DateTime ChangedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Reason { get; set; } // Lý do tự động (validation message)

        // Navigation
        [ForeignKey("SlotId")]
        public ParkingSlot? ParkingSlot { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
    }
}
