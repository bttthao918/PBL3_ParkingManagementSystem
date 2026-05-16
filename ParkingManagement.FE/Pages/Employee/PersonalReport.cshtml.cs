using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public string FromDate { get; set; } = DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy");
        public string ToDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");

        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public string TotalWorkingHours { get; set; } = "0 giờ 0 phút";
        public int TotalShifts { get; set; }
        public double AverageHoursPerShift { get; set; }
        public decimal AverageRevenuePerHour { get; set; }

        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> RevenueChartData { get; set; } = new List<decimal>();
        public List<int> TicketChartData { get; set; } = new List<int>();

        public List<ShiftReportVM> Shifts { get; set; } = new List<ShiftReportVM>();
        public List<CalendarDayVM> CalendarDays { get; set; } = new List<CalendarDayVM>();

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Báo cáo cá nhân";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            var employeeId = User.FindFirst("related_id")?.Value;
            if (!string.IsNullOrEmpty(employeeId))
            {
                Period = string.IsNullOrWhiteSpace(Period) ? "month" : Period.ToLower();
                var revenueReport = await _reportService.GetEmployeeRevenueReportAsync(employeeId, Period);
                
                if (revenueReport != null)
                {
                    var attendanceReport = await _reportService.GetShiftAttendanceReportAsync(employeeId, revenueReport.PeriodStart, revenueReport.PeriodEnd);

                    FromDate = revenueReport.PeriodStart.ToString("dd/MM/yyyy");
                    ToDate = revenueReport.PeriodEnd.ToString("dd/MM/yyyy");
                    
                    TotalTickets = revenueReport.TotalTickets;
                    TotalRevenue = revenueReport.TotalRevenue;
                    
                    if (revenueReport.DailyBreakdown != null)
                    {
                        foreach (var day in revenueReport.DailyBreakdown.OrderBy(d => d.Date).TakeLast(10))
                        {
                            ChartLabels.Add(day.Date.ToString("dd/MM"));
                            RevenueChartData.Add(day.TotalRevenue);
                            TicketChartData.Add(day.TicketCount);
                        }
                    }

                    if (attendanceReport != null)
                    {
                        TotalShifts = attendanceReport.TotalWorkDays;
                    int hours = attendanceReport.TotalWorkMinutes / 60;
                    int mins = attendanceReport.TotalWorkMinutes % 60;
                    TotalWorkingHours = $"{hours} giờ {mins} phút";
                    
                    AverageHoursPerShift = attendanceReport.TotalWorkDays > 0 
                        ? Math.Round(attendanceReport.TotalWorkMinutes / 60.0 / attendanceReport.TotalWorkDays, 1) 
                        : 0;

                    if (TotalTickets > 0 && attendanceReport.TotalWorkMinutes > 0)
                    {
                        AverageRevenuePerHour = TotalRevenue / (decimal)(attendanceReport.TotalWorkMinutes / 60.0);
                    }

                    if (attendanceReport.Details != null)
                    {
                        Shifts = attendanceReport.Details.OrderByDescending(d => d.Date).Select(d => new ShiftReportVM
                        {
                            WorkDate = d.Date.ToString("dd/MM/yyyy"),
                            DayName = GetDayName(d.Date.DayOfWeek),
                            ShiftName = "Ca " + d.Shift,
                            StartTime = d.CheckInTime?.ToString("HH:mm") ?? "-",
                            EndTime = d.CheckOutTime?.ToString("HH:mm") ?? "-",
                            TotalHours = d.WorkMinutes.HasValue ? $"{d.WorkMinutes.Value / 60} giờ {d.WorkMinutes.Value % 60} phút" : "0 giờ",
                            TicketCount = d.TicketsProcessed,
                            Revenue = d.ShiftRevenue,
                            Status = d.Status
                        }).ToList();
                    }
                }
            }
            }

            // Calendar Days
            CalendarDays = Enumerable.Range(1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
                .Select(d => new CalendarDayVM
                {
                    Day = d,
                    IsToday = d == DateTime.Now.Day,
                    HasWorked = Shifts.Any(s => s.WorkDate.StartsWith($"{d:00}/"))
                }).ToList();
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
