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

        // Navigation
        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }

        [ForeignKey("MonthlyTicketId")]
        public MonthlyTicket? MonthlyTicket { get; set; }
    }
}

