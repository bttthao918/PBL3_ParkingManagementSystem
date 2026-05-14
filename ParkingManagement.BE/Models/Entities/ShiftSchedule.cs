using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    /// <summary>
    /// Lịch ca làm việc — Manager phân công cho nhân viên
    /// </summary>
    [Table("ShiftSchedules")]
    public class ShiftSchedule
    {
        [Key]
        [MaxLength(20)]
        public string ScheduleId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string EmployeeId { get; set; } = null!;

        /// <summary>Ngày làm việc</summary>
        public DateTime WorkDate { get; set; }

        /// <summary>Loại ca: "Sáng", "Chiều", "Tối"</summary>
        [Required]
        [MaxLength(20)]
        public string ShiftType { get; set; } = null!;

        /// <summary>Giờ bắt đầu ca</summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>Giờ kết thúc ca</summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>Trạng thái: "Đã lên lịch", "Đang làm", "Hoàn thành", "Vắng"</summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Đã lên lịch";

        /// <summary>Ghi chú</summary>
        [MaxLength(200)]
        public string? Note { get; set; }

        /// <summary>Manager tạo lịch</summary>
        [MaxLength(20)]
        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;
    }
}
