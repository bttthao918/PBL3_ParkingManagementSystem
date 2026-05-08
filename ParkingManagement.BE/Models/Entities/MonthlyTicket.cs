using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    // Vé tháng — hoàn toàn tách biệt với vé lượt (Ticket)
    // Khách đến thì nhân viên chỉ chỗ trống, không gắn slot cố định
    [Table("MonthlyTickets")]
    public class MonthlyTicket
    {
        [Key]
        [MaxLength(20)]
        public string MonthlyTicketId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string CustomerId { get; set; } = null!; // Vé tháng bắt buộc phải có tài khoản

        [Required]
        [MaxLength(20)]
        public string VehiclePlate { get; set; } = null!; // Biển số xe đăng ký

        [Required]
        [MaxLength(20)]
        public string VehicleType { get; set; } = null!; // Xe máy / Ô tô nhỏ / Ô tô lớn

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string PackageType { get; set; } = null!; // 1 tháng / 3 tháng / 6 tháng

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal TotalFee { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Hoạt động"; // Hoạt động / Hết hạn / Đã hủy

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = null!;

        [ForeignKey("VehiclePlate")]
        public Vehicle Vehicle { get; set; } = null!;

        public Payment? Payment { get; set; }
    }
}
