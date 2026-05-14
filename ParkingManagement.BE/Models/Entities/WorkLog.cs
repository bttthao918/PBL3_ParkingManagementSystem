using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    /// <summary>
    /// Bảng chấm công — ghi nhận giờ vào/ra của nhân viên mỗi ca
    /// </summary>
    [Table("WorkLogs")]
    public class WorkLog
    {
        [Key]
        [MaxLength(20)]
        public string WorkLogId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string EmployeeId { get; set; } = null!;

        /// <summary>Ngày làm việc</summary>
        public DateTime WorkDate { get; set; }

        /// <summary>Giờ bắt đầu ca</summary>
        public DateTime StartTime { get; set; }

        /// <summary>Giờ kết thúc ca (null = đang trong ca)</summary>
        public DateTime? EndTime { get; set; }

        /// <summary>Tổng phút làm việc (tính khi kết thúc ca)</summary>
        public int? TotalMinutes { get; set; }

        /// <summary>Ghi chú (VD: "Ca sáng", "Tăng ca"...)</summary>
        [MaxLength(200)]
        public string? Note { get; set; }

        /// <summary>Trạng thái: "Đang làm", "Hoàn thành"</summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Đang làm";

        // Navigation
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;
    }
}
