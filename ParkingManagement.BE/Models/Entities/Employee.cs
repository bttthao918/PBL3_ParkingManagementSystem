using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [MaxLength(20)]
        public string EmployeeId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string EmployeeCode { get; set; } = null!; // Mã nhân viên (VD: EMP001, EMP002...)

        [Required]
        [MaxLength(20)]
        public string AccountId { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; } // Male, Female, Other

        [MaxLength(20)]
        public string? Shift { get; set; } // Sáng / Chiều / Tối

        [MaxLength(20)]
        public string? ManagerId { get; set; } // FK tới Manager

        public bool IsDeleted { get; set; } = false;

        // Navigation
        [ForeignKey("AccountId")]
        public Account Account { get; set; } = null!;

        [ForeignKey("ManagerId")]
        public Manager? Manager { get; set; }
    }
}
