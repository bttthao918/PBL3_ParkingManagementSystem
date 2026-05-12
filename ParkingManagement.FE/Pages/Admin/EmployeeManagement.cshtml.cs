using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;
using ParkingManagement.FE.Models;

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

        public async Task OnGetAsync()
        {
            var filter = new ManagerEmployeeFilterDto
            {
                PageNumber = 1,
                PageSize = 100
            };

            var result = await _employeeService.GetEmployeesAsync(filter);

            if (result != null && result.Items != null)
            {
                Employees = result.Items.Select(e => new EmployeeViewModel(
                    e.EmployeeId,
                    e.FullName,
                    "Nhân viên bãi xe", // Temporarily hardcoded or mapped if BE provides it
                    "Bãi xe A", // Temporarily hardcoded
                    e.PhoneNumber,
                    0, // TotalTickets (BE detail endpoint has this, list doesn't)
                    0, // ProcessedTickets
                    e.Status,
                    e.Status == "Hoạt động" ? "active" : "inactive",
                    "/images/avatar-demo.jpg"
                )).ToList();
            }
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
