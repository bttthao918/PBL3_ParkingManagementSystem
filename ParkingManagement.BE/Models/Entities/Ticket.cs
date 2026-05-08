using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    // Vé lượt — dùng cho cả khách có tài khoản lẫn khách vãng lai
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        [MaxLength(20)]
        public string TicketId { get; set; } = null!;

        // NULL = khách vãng lai (không có tài khoản)
        [MaxLength(20)]
        public string? CustomerId { get; set; }

        [Required]
        [MaxLength(20)]
        public string VehiclePlate { get; set; } = null!; // Biển số xe (nhập tay khi check-in)

        [Required]
        [MaxLength(20)]
        public string VehicleType { get; set; } = null!; // Lưu lại loại xe tại thời điểm gửi

        [MaxLength(20)]
        public string? SlotId { get; set; }

        [Required]
        public DateTime CheckInTime { get; set; } = DateTime.Now;

        public DateTime? CheckOutTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal Fee { get; set; } = 0; // Tổng phí (tính khi checkout)

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Đang trong bãi"; // Đang trong bãi / Đã ra

        // Navigation
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [ForeignKey("VehiclePlate")]
        public Vehicle? Vehicle { get; set; }

        [ForeignKey("SlotId")]
        public ParkingSlot? ParkingSlot { get; set; }

        public Payment? Payment { get; set; }
    }
}
