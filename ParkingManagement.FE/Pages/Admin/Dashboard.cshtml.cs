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
        public string RevenueChartPoints { get; set; } = BuildSvgPoints(Array.Empty<decimal>());
        public List<ManagerEmployeeListDto> Employees { get; set; } = new();
        public List<EmployeeTicketListDto> RecentTickets { get; set; } = new();

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Admin / Manager";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Manager";

            var dashboardTask = _reportService.GetManagerDashboardAsync();
            var revenueTask = _reportService.GetManagerRevenueReportAsync(new RevenueReportFilterDto { Period = "7days" });
            var ticketsTask = _ticketService.SearchTicketsAsync(new EmployeeTicketSearchDto { PageNumber = 1, PageSize = 5 });
            var employeesTask = _employeeService.GetEmployeesAsync(new ManagerEmployeeFilterDto { PageNumber = 1, PageSize = 5 });

            await Task.WhenAll(dashboardTask, revenueTask, ticketsTask, employeesTask);

            Summary = await dashboardTask ?? new DashboardSummaryDto();

            var revenue = await revenueTask;
            if (revenue != null)
            {
                RevenueChartPoints = BuildSvgPoints(revenue.DailyBreakdown.Select(x => x.Revenue).ToList());
            }

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

        private static string BuildSvgPoints(IReadOnlyCollection<decimal> values)
        {
            var series = values.Count == 0 ? new List<decimal> { 0, 0, 0, 0, 0, 0, 0 } : values.ToList();
            if (series.Count == 1)
            {
                series.Add(series[0]);
            }

            var max = series.Max();
            var step = 1000d / Math.Max(1, series.Count - 1);
            return string.Join(" ", series.Select((value, index) =>
            {
                var x = Math.Round(index * step);
                var y = max <= 0 ? 210 : 220 - (double)(value / max) * 150;
                return $"{x},{Math.Round(y)}";
            }));
        }
    }
}
