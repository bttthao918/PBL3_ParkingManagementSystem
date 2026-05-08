using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class EmployeeManagementModel : PageModel
    {
        public List<EmployeeViewModel> Employees { get; set; } = new();

        public void OnGet()
        {
            Employees = new List<EmployeeViewModel>
            {
                new("NV0001", "Nguyễn Văn An", "Nhân viên bãi xe", "Bãi xe A", "0901 234 567", 1256, 1198, "Đang làm việc", "active", "/images/avatar-demo.jpg"),
                new("NV0002", "Trần Thị Bình", "Nhân viên bãi xe", "Bãi xe B", "0902 345 678", 980, 872, "Đang làm việc", "active", "/images/avatar-demo.jpg"),
                new("NV0003", "Lê Văn Cường", "Tổ trưởng", "Bãi xe A", "0903 456 789", 1562, 1430, "Đang làm việc", "active", "/images/avatar-demo.jpg"),
                new("NV0004", "Phạm Thị Dung", "Nhân viên bãi xe", "Bãi xe C", "0904 567 890", 753, 690, "Nghỉ phép", "leave", "/images/avatar-demo.jpg"),
                new("NV0005", "Hoàng Văn Em", "Nhân viên bãi xe", "Bãi xe B", "0905 678 901", 612, 580, "Đang làm việc", "active", "/images/avatar-demo.jpg"),
                new("NV0006", "Đỗ Thị Hương", "Nhân viên bãi xe", "Bãi xe A", "0906 789 012", 1102, 1002, "Tạm nghỉ", "inactive", "/images/avatar-demo.jpg"),
                new("NV0007", "Vũ Văn Kiên", "Tổ trưởng", "Bãi xe C", "0907 890 123", 890, 812, "Đang làm việc", "active", "/images/avatar-demo.jpg"),
                new("NV0008", "Bùi Thị Lan", "Nhân viên bãi xe", "Bãi xe A", "0908 901 234", 567, 498, "Đang làm việc", "active", "/images/avatar-demo.jpg")
            };
        }
    }

    [Authorize(Roles = "Manager")]
    public class EmployeeViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Phone { get; set; }
        public int TotalTickets { get; set; }
        public int ProcessedTickets { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; }
        public string Avatar { get; set; }

        public EmployeeViewModel(
            string code,
            string name,
            string position,
            string department,
            string phone,
            int totalTickets,
            int processedTickets,
            string status,
            string statusClass,
            string avatar)
        {
            Code = code;
            Name = name;
            Position = position;
            Department = department;
            Phone = phone;
            TotalTickets = totalTickets;
            ProcessedTickets = processedTickets;
            Status = status;
            StatusClass = statusClass;
            Avatar = avatar;
        }
    }
}
