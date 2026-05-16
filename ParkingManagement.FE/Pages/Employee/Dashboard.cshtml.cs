using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class DashboardModel : PageModel
    {
        private readonly Services.IReportService _reportService;
        private readonly Services.ITicketService _ticketService;
        private readonly Services.IWorkLogService _workLogService;
        private readonly Services.IShiftScheduleService _shiftService;

        public DashboardModel(
            Services.IReportService reportService,
            Services.ITicketService ticketService,
            Services.IWorkLogService workLogService,
            Services.IShiftScheduleService shiftService)
        {
            _reportService = reportService;
            _ticketService = ticketService;
            _workLogService = workLogService;
            _shiftService = shiftService;
        }

        public Models.EmployeeDashboardDto Stats { get; set; } = CreateFallbackStats();
        public List<Models.EmployeeTicketListDto> RecentTickets { get; set; } = new();
        public string RevenueChartPoints { get; set; } = BuildSvgPoints(Array.Empty<decimal>());

        // Work Log
        public Services.WorkLogStatusResponse? WorkStatus { get; set; }
        public Services.WorkLogMonthlySummaryResponse? MonthlySummary { get; set; }
        public Services.ShiftTodayResponse? TodayShift { get; set; }
        public List<Services.ShiftMyWeekItem> MyWeekShifts { get; set; } = new();

        [TempData]
        public string? ShiftMessage { get; set; }

        [TempData]
        public bool ShiftSuccess { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Employee";

            var employeeId = User.FindFirst("related_id")?.Value;
            if (!string.IsNullOrEmpty(employeeId))
            {
                var stats = await _reportService.GetEmployeeDashboardAsync(employeeId);
                if (stats != null)
                {
                    Stats = stats;
                }

                var revenueReport = await _reportService.GetEmployeeRevenueReportAsync(employeeId, "7days");
                if (revenueReport != null)
                {
                    RevenueChartPoints = BuildSvgPoints(revenueReport.DailyBreakdown.Select(x => x.TotalRevenue).ToList());
                }
            }

            // Recent tickets
            var searchResult = await _ticketService.SearchTicketsAsync(new Models.EmployeeTicketSearchDto
            {
                PageNumber = 1,
                PageSize = 5
            });

            if (searchResult != null && searchResult.Items != null)
            {
                RecentTickets = searchResult.Items.Take(5).ToList();
            }

            // Work status
            WorkStatus = await _workLogService.GetCurrentStatusAsync();
            MonthlySummary = await _workLogService.GetMonthlySummaryAsync();
            TodayShift = await _shiftService.GetMyTodayShiftAsync();
            MyWeekShifts = await _shiftService.GetMyWeekAsync() ?? new List<Services.ShiftMyWeekItem>();
        }

        public async Task<IActionResult> OnPostStartShiftAsync(string? scheduleId)
        {
            var result = await _workLogService.StartShiftAsync(scheduleId);
            ShiftSuccess = result?.Success ?? false;
            ShiftMessage = result?.Message ?? "Không thể bắt đầu ca.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEndShiftAsync()
        {
            var result = await _workLogService.EndShiftAsync();
            ShiftSuccess = result?.Success ?? false;
            ShiftMessage = result?.Message ?? "Không thể kết thúc ca.";
            return RedirectToPage();
        }

        private static Models.EmployeeDashboardDto CreateFallbackStats()
        {
            return new Models.EmployeeDashboardDto
            {
                TicketsProcessedThisMonth = 0,
                RevenueThisMonth = 0,
                WorkMinutesThisMonth = 0,
                WorkDaysThisMonth = 0,
                AverageRevenuePerTicket = 0,
                AverageTicketsPerDay = 0
            };
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
