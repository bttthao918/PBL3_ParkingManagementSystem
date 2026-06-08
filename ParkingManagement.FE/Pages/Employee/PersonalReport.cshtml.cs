using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class PersonalReportModel : PageModel
    {
        private readonly Services.IReportService _reportService;

        public PersonalReportModel(Services.IReportService reportService)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "month";

        [BindProperty(SupportsGet = true)]
        public string? Month { get; set; }

        public string FromDate { get; set; } = DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy");
        public string ToDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public string SelectedMonth { get; set; } = DateTime.Now.ToString("yyyy-MM");
        public string? DetailMonth => Period == "month" ? SelectedMonth : null;
        public bool HasSelectedMonthFilter { get; set; }

        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public string TotalWorkingHours { get; set; } = "0 giờ";
        public int TotalShifts { get; set; }
        public double AverageHoursPerShift { get; set; }
        public decimal AverageRevenuePerHour { get; set; }

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> RevenueChartData { get; set; } = new();
        public List<int> TicketChartData { get; set; } = new();
        public List<ShiftReportVM> Shifts { get; set; } = new();
        public List<CalendarDayVM> CalendarDays { get; set; } = new();

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Báo cáo cá nhân";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            Period = NormalizePeriod(Period);
            DateTime? fromDate = null;
            DateTime? toDate = null;
            var calendarMonth = DateTime.Now;
            var hasSelectedMonth = TryGetMonthRange(Month, out var monthFrom, out var monthTo);
            if (hasSelectedMonth)
            {
                Period = "month";
                fromDate = monthFrom;
                toDate = monthTo;
                calendarMonth = monthFrom;
            }

            HasSelectedMonthFilter = hasSelectedMonth;
            SelectedMonth = ResolveSelectedMonth(hasSelectedMonth ? Month : null, calendarMonth);

            var employeeId = User.FindFirst("related_id")?.Value;
            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                var revenueReport = await _reportService.GetEmployeeRevenueReportAsync(employeeId, Period, fromDate, toDate);
                if (revenueReport != null)
                {
                    var attendanceReport = await _reportService.GetShiftAttendanceReportAsync(
                        employeeId,
                        revenueReport.PeriodStart,
                        revenueReport.PeriodEnd);

                    FromDate = revenueReport.PeriodStart.ToString("dd/MM/yyyy");
                    ToDate = revenueReport.PeriodEnd.ToString("dd/MM/yyyy");
                    calendarMonth = revenueReport.PeriodStart == default ? calendarMonth : revenueReport.PeriodStart;
                    SelectedMonth = ResolveSelectedMonth(hasSelectedMonth ? Month : null, calendarMonth);
                    TotalTickets = revenueReport.TotalTickets;
                    TotalRevenue = revenueReport.TotalRevenue;

                    foreach (var day in revenueReport.DailyBreakdown.OrderBy(d => d.Date).TakeLast(10))
                    {
                        ChartLabels.Add(day.Date.ToString("dd/MM"));
                        RevenueChartData.Add(day.TotalRevenue);
                        TicketChartData.Add(day.TicketCount);
                    }

                    if (attendanceReport != null)
                    {
                        TotalShifts = attendanceReport.TotalWorkDays;
                        TotalWorkingHours = FormatDuration(attendanceReport.TotalWorkMinutes);
                        AverageHoursPerShift = attendanceReport.TotalWorkDays > 0
                            ? Math.Round(attendanceReport.TotalWorkMinutes / 60.0 / attendanceReport.TotalWorkDays, 1)
                            : 0;
                        AverageRevenuePerHour = attendanceReport.TotalWorkMinutes > 0
                            ? TotalRevenue / (decimal)(attendanceReport.TotalWorkMinutes / 60.0)
                            : 0;

                        Shifts = attendanceReport.Details
                            .OrderByDescending(d => d.Date)
                            .ThenByDescending(d => d.CheckInTime)
                            .Select(d => new ShiftReportVM
                            {
                                WorkDate = d.Date.ToString("dd/MM/yyyy"),
                                DayName = GetDayName(d.Date.DayOfWeek),
                                ShiftName = FormatShiftName(d.Shift),
                                StartTime = d.CheckInTime?.ToString("HH:mm") ?? "-",
                                EndTime = d.CheckOutTime?.ToString("HH:mm") ?? "Đang làm",
                                TotalHours = FormatDuration(d.WorkMinutes ?? 0),
                                TicketCount = d.TicketsProcessed,
                                Revenue = d.ShiftRevenue,
                                Status = d.Status
                            })
                            .ToList();
                    }
                }
            }

            CalendarDays = Enumerable.Range(1, DateTime.DaysInMonth(calendarMonth.Year, calendarMonth.Month))
                .Select(day => new CalendarDayVM
                {
                    Day = day,
                    IsToday = new DateTime(calendarMonth.Year, calendarMonth.Month, day).Date == DateTime.Today,
                    HasWorked = Shifts.Any(shift => shift.WorkDate.StartsWith($"{day:00}/{calendarMonth.Month:00}/"))
                })
                .ToList();
        }

        private static string NormalizePeriod(string? period)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" or "day" => "today",
                "7days" or "week" => "7days",
                "month" => "month",
                _ => "month"
            };
        }

        private static bool TryGetMonthRange(string? month, out DateTime from, out DateTime to)
        {
            if (DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                from = new DateTime(parsed.Year, parsed.Month, 1);
                to = from.AddMonths(1).AddDays(-1);
                return true;
            }

            from = default;
            to = default;
            return false;
        }

        private static string ResolveSelectedMonth(string? month, DateTime fallback)
        {
            return DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed.ToString("yyyy-MM", CultureInfo.InvariantCulture)
                : fallback.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        private static string GetDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ hai",
                DayOfWeek.Tuesday => "Thứ ba",
                DayOfWeek.Wednesday => "Thứ tư",
                DayOfWeek.Thursday => "Thứ năm",
                DayOfWeek.Friday => "Thứ sáu",
                DayOfWeek.Saturday => "Thứ bảy",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => ""
            };
        }

        private static string FormatShiftName(string? shift)
        {
            if (string.IsNullOrWhiteSpace(shift))
            {
                return "Ca không xác định";
            }

            var value = shift.Trim();
            return value.StartsWith("Ca ", StringComparison.OrdinalIgnoreCase)
                ? value
                : $"Ca {value}";
        }

        private static string FormatDuration(int totalMinutes)
        {
            var safeMinutes = Math.Max(0, totalMinutes);
            var hours = safeMinutes / 60;
            var minutes = safeMinutes % 60;
            return minutes == 0 ? $"{hours} giờ" : $"{hours} giờ {minutes} phút";
        }
    }

    [Authorize(Roles = "Employee")]
    public class ShiftReportVM
    {
        public string WorkDate { get; set; } = "";
        public string DayName { get; set; } = "";
        public string ShiftName { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string TotalHours { get; set; } = "";
        public int TicketCount { get; set; }
        public decimal Revenue { get; set; }
        public string Status { get; set; } = "";
    }

    [Authorize(Roles = "Employee")]
    public class CalendarDayVM
    {
        public int Day { get; set; }
        public bool IsToday { get; set; }
        public bool HasWorked { get; set; }
    }
}
