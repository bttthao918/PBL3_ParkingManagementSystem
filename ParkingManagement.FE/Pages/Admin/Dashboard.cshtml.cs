using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IReportService _reportService;
        private readonly ITicketService _ticketService;
        private readonly IEmployeeService _employeeService;

        public DashboardModel(
            IReportService reportService,
            ITicketService ticketService,
            IEmployeeService employeeService)
        {
            _reportService = reportService;
            _ticketService = ticketService;
            _employeeService = employeeService;
        }

        public DashboardSummaryDto Summary { get; set; } = new();
        public List<ManagerEmployeeListDto> Employees { get; set; } = new();
        public List<EmployeeTicketListDto> RecentTickets { get; set; } = new();

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Admin / Manager";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Manager";

            var dashboardTask = _reportService.GetManagerDashboardAsync();
            var ticketsTask = _ticketService.SearchTicketsAsync(new EmployeeTicketSearchDto { PageNumber = 1, PageSize = 5 });
            var employeesTask = _employeeService.GetEmployeesAsync(new ManagerEmployeeFilterDto { PageNumber = 1, PageSize = 5 });

            await Task.WhenAll(dashboardTask, ticketsTask, employeesTask);

            Summary = await dashboardTask ?? new DashboardSummaryDto();

            var tickets = await ticketsTask;
            RecentTickets = tickets?.Items
                .OrderByDescending(x => x.CheckInTime)
                .Take(5)
                .ToList() ?? new List<EmployeeTicketListDto>();

            var employees = await employeesTask;
            Employees = employees?.Items.Take(5).ToList() ?? new List<ManagerEmployeeListDto>();
        }

        public string GetSlotEmptyCount()
        {
            return Math.Max(0, Summary.TotalSlots - Summary.OccupiedSlots).ToString("N0");
        }

        public string GetTicketStatusClass(string status)
        {
            return status.Contains("Đang", StringComparison.OrdinalIgnoreCase)
                ? "status-warning"
                : "status-success";
        }
    }
}
