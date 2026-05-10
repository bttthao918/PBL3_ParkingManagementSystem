using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;
using ParkingManagement.FE.Models.Employee;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class EmployeeManagementModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeManagementModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public List<EmployeeViewModel> Employees { get; set; } = new();
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }

        public async Task OnGetAsync()
        {
            // Gọi API lấy danh sách nhân viên thật
            var employees = await _employeeService.GetAllEmployeesAsync();

            if (employees != null && employees.Any())
            {
                Employees = employees.Select(e => new EmployeeViewModel
                {
                    Code = e.EmployeeCode,
                    Name = e.FullName,
                    Position = "Nhân viên bãi xe",
                    Department = "Bãi xe A",
                    Phone = e.PhoneNumber ?? "",
                    TotalTickets = 0, // Sẽ tính sau
                    ProcessedTickets = 0,
                    Status = e.IsActive ? "Đang làm việc" : "Vô hiệu hóa",
                    StatusClass = e.IsActive ? "active" : "disabled",
                    Avatar = "/images/avatar-demo.jpg",
                    Email = e.Email
                }).ToList();

                TotalEmployees = Employees.Count;
                ActiveEmployees = Employees.Count(e => e.Status == "Đang làm việc");
                InactiveEmployees = TotalEmployees - ActiveEmployees;
            }
            else
            {
                // Fallback nếu API lỗi
                LoadMockData();
            }
        }

        private void LoadMockData()
        {
            Employees = new List<EmployeeViewModel>
            {
                new("NV0001", "Nguyễn Văn An", "Nhân viên bãi xe", "Bãi xe A", "0901 234 567", 1256, 1198, "Đang làm việc", "active", "/images/avatar-demo.jpg", "an.nguyen@parking.vn"),
                new("NV0002", "Trần Thị Bình", "Nhân viên bãi xe", "Bãi xe B", "0902 345 678", 980, 872, "Đang làm việc", "active", "/images/avatar-demo.jpg", "binh.tran@parking.vn"),
                new("NV0003", "Lê Văn Cường", "Tổ trưởng", "Bãi xe A", "0903 456 789", 1562, 1430, "Đang làm việc", "active", "/images/avatar-demo.jpg", "cuong.le@parking.vn"),
            };
            TotalEmployees = Employees.Count;
            ActiveEmployees = Employees.Count(e => e.Status == "Đang làm việc");
            InactiveEmployees = TotalEmployees - ActiveEmployees;
        }
    }

    public class EmployeeViewModel
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Position { get; set; } = "";
        public string Department { get; set; } = "";
        public string Phone { get; set; } = "";
        public int TotalTickets { get; set; }
        public int ProcessedTickets { get; set; }
        public string Status { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string Email { get; set; } = "";

        public EmployeeViewModel() { }

        public EmployeeViewModel(string code, string name, string position, string department,
            string phone, int totalTickets, int processedTickets, string status,
            string statusClass, string avatar, string email = "")
        {
            Code = code; Name = name; Position = position; Department = department;
            Phone = phone; TotalTickets = totalTickets; ProcessedTickets = processedTickets;
            Status = status; StatusClass = statusClass; Avatar = avatar; Email = email;
        }
    }
}