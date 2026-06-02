using System.Security.Claims;
using System.Globalization;
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

        // Work Log
        public Services.WorkLogStatusResponse? WorkStatus { get; set; }
        public Services.WorkLogMonthlySummaryResponse? MonthlySummary { get; set; }
        public Services.ShiftTodayResponse? TodayShift { get; set; }
        public List<Services.ShiftMyWeekItem> MyWeekShifts { get; set; } = new();
        public bool CanStartShift =>
            TodayShift?.HasShift == true &&
            WorkStatus?.IsWorking != true &&
            !IsCompleted(TodayShift.Shift?.Status) &&
            IsNowInShiftWindow();

        public string StartShiftHint
        {
            get
            {
                if (TodayShift?.HasShift != true)
                    return "Hôm nay bạn chưa được phân ca.";

                if (IsCompleted(TodayShift.Shift?.Status))
                    return "Ca hôm nay đã hoàn thành.";

                if (!IsNowInShiftWindow())
                    return $"Chỉ có thể bắt đầu trong khung giờ {TodayShift.Shift?.StartTime} - {TodayShift.Shift?.EndTime}.";

                return "Bấm nút bên dưới để bắt đầu ca làm việc.";
            }
        }

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
            if (MonthlySummary != null &&
                MonthlySummary.TotalDays == 0 &&
                MonthlySummary.TotalMinutes == 0 &&
                (Stats.WorkDaysThisMonth > 0 || Stats.WorkMinutesThisMonth > 0))
            {
                MonthlySummary.TotalDays = Stats.WorkDaysThisMonth;
                MonthlySummary.TotalMinutes = Stats.WorkMinutesThisMonth;
                MonthlySummary.TotalHours = Stats.WorkMinutesThisMonth / 60;
                MonthlySummary.AverageHoursPerDay = Stats.WorkDaysThisMonth > 0
                    ? Math.Round(Stats.WorkMinutesThisMonth / 60.0 / Stats.WorkDaysThisMonth, 1)
                    : 0;
            }
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

        private bool IsNowInShiftWindow()
        {
            var shift = TodayShift?.Shift;
            if (shift == null ||
                !TimeSpan.TryParse(shift.StartTime, CultureInfo.InvariantCulture, out var start) ||
                !TimeSpan.TryParse(shift.EndTime, CultureInfo.InvariantCulture, out var end))
            {
                return false;
            }

            var now = DateTime.Now.TimeOfDay;
            return start <= end
                ? now >= start && now < end
                : now >= start || now < end;
        }

        private static bool IsCompleted(string? status)
        {
            return !string.IsNullOrWhiteSpace(status) &&
                   (status.Contains("Hoàn", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("HoÃ", StringComparison.OrdinalIgnoreCase));
        }
    }
}
