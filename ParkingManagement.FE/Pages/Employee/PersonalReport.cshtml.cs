using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class PersonalReportModel : PageModel
    {
        public string FromDate { get; set; } = "01/05/2024";
        public string ToDate { get; set; } = "20/05/2024";

        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public string TotalWorkingHours { get; set; } = "";
        public int TotalShifts { get; set; }
        public double AverageHoursPerShift { get; set; }
        public decimal AverageRevenuePerHour { get; set; }

        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> RevenueChartData { get; set; } = new List<decimal>();
        public List<int> TicketChartData { get; set; } = new List<int>();

        public List<ShiftReportVM> Shifts { get; set; } = new List<ShiftReportVM>();
        public List<CalendarDayVM> CalendarDays { get; set; } = new List<CalendarDayVM>();

        public void OnGet()
        {
            TotalTickets = 1248;
            TotalRevenue = 45680000;
            TotalWorkingHours = "176 giờ 30 phút";
            TotalShifts = 24;
            AverageHoursPerShift = 7.4;
            AverageRevenuePerHour = 259250;

            Shifts = new List<ShiftReportVM>
            {
                new ShiftReportVM
                {
                    WorkDate = "20/05/2024",
                    DayName = "Thứ hai",
                    ShiftName = "Ca sáng",
                    StartTime = "07:00",
                    EndTime = "15:00",
                    TotalHours = "8 giờ",
                    TicketCount = 48,
                    Revenue = 2450000,
                    Status = "Đã hoàn thành"
                },
                new ShiftReportVM
                {
                    WorkDate = "19/05/2024",
                    DayName = "Chủ nhật",
                    ShiftName = "Ca tối",
                    StartTime = "15:00",
                    EndTime = "23:00",
                    TotalHours = "8 giờ",
                    TicketCount = 52,
                    Revenue = 2780000,
                    Status = "Đã hoàn thành"
                },
                new ShiftReportVM
                {
                    WorkDate = "18/05/2024",
                    DayName = "Thứ bảy",
                    ShiftName = "Ca sáng",
                    StartTime = "07:00",
                    EndTime = "15:00",
                    TotalHours = "8 giờ",
                    TicketCount = 46,
                    Revenue = 2320000,
                    Status = "Đã hoàn thành"
                },
                new ShiftReportVM
                {
                    WorkDate = "17/05/2024",
                    DayName = "Thứ sáu",
                    ShiftName = "Ca tối",
                    StartTime = "15:00",
                    EndTime = "23:00",
                    TotalHours = "8 giờ",
                    TicketCount = 50,
                    Revenue = 2650000,
                    Status = "Đã hoàn thành"
                },
                new ShiftReportVM
                {
                    WorkDate = "16/05/2024",
                    DayName = "Thứ năm",
                    ShiftName = "Ca sáng",
                    StartTime = "07:00",
                    EndTime = "15:00",
                    TotalHours = "8 giờ",
                    TicketCount = 45,
                    Revenue = 2270000,
                    Status = "Đã hoàn thành"
                }
            };

            CalendarDays = Enumerable.Range(1, 31)
                .Select(day => new CalendarDayVM
                {
                    Day = day,
                    IsToday = day == 20,
                    HasWorked = day >= 1 && day <= 20 && day != 3 && day != 4 && day != 11
                })
                .ToList();

            ChartLabels = new List<string>
{
    "01/05", "02/05", "03/05", "04/05", "05/05",
    "06/05", "07/05", "08/05", "09/05", "10/05",
    "11/05", "12/05", "13/05", "14/05", "15/05",
    "16/05", "17/05", "18/05", "19/05", "20/05"
};

            RevenueChartData = new List<decimal>
{
    2000000, 4100000, 3000000, 4500000, 5200000,
    4000000, 4700000, 5300000, 3900000, 6250000,
    4300000, 5100000, 4900000, 6000000, 3700000,
    4800000, 3200000, 3900000, 4700000, 3300000
};

            TicketChartData = new List<int>
{
    25, 52, 39, 47, 58,
    60, 49, 38, 45, 68,
    50, 53, 60, 41, 55,
    60, 57, 40, 48, 39
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
