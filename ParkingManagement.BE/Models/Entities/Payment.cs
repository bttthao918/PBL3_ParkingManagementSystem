using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [MaxLength(20)]
        public string PaymentId { get; set; } = null!;

        // Một trong hai phải có giá trị (vé lượt HOẶC vé tháng)
        [MaxLength(20)]
        public string? TicketId { get; set; }

        [MaxLength(20)]
        public string? MonthlyTicketId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Method { get; set; } = null!; // Tiền mặt / Chuyển khoản / Ví điện tử

        public DateTime PaymentTime { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Thành công"; // Thành công / Thất bại

        /// <summary>
        /// Nhân viên thu tiền/xác nhận thanh toán. Null nghĩa là thanh toán tự động hoặc dữ liệu cũ.
        /// </summary>
        [MaxLength(20)]
        public string? CollectedByEmployeeId { get; set; }

        /// <summary>
        /// VNPay transaction reference (dùng để map callback từ VNPay)
        /// </summary>
        [MaxLength(50)]
        public string? VnpTxnRef { get; set; }

        // Navigation
        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }

        [ForeignKey("MonthlyTicketId")]
        public MonthlyTicket? MonthlyTicket { get; set; }

        [ForeignKey("CollectedByEmployeeId")]
        public Employee? CollectedByEmployee { get; set; }
    }
}

