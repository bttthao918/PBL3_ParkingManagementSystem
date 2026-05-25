using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class ShiftManagementModel : PageModel
    {
        private readonly Services.IWorkLogService _workLogService;
        private readonly Services.IShiftScheduleService _shiftService;

        public ShiftManagementModel(
            Services.IWorkLogService workLogService,
            Services.IShiftScheduleService shiftService)
        {
            _workLogService = workLogService;
            _shiftService = shiftService;
        }

        public Services.WorkLogStatusResponse? WorkStatus { get; set; }
        public Services.WorkLogMonthlySummaryResponse? MonthlySummary { get; set; }
        public Services.ShiftTodayResponse? TodayShift { get; set; }
        public List<Services.ShiftMyWeekItem> MyWeekShifts { get; set; } = new();
        public List<EmployeeShiftDayView> WeekDays { get; set; } = new();

        public string EmployeeName { get; set; } = "Nhân viên";
        public string EmployeeCode { get; set; } = "";
        public DateTime Today { get; set; } = DateTime.Today;

        public string CurrentShiftName => TodayShift?.HasShift == true
            ? TodayShift.Shift?.ShiftType ?? "Ca hôm nay"
            : "Chưa có ca";

        public string CurrentShiftTime => TodayShift?.HasShift == true
            ? $"{TodayShift.Shift?.StartTime} - {TodayShift.Shift?.EndTime}"
            : "Chưa được phân ca";

        public string CurrentShiftStatusClass
        {
            get
            {
                if (WorkStatus?.IsWorking == true)
                    return "working";

                var status = TodayShift?.Shift?.Status ?? "";
                if (status.Contains("Hoàn", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("HoÃ", StringComparison.OrdinalIgnoreCase))
                    return "done";

                return TodayShift?.HasShift == true ? "planned" : "none";
            }
        }

        public bool CanStartShift =>
            TodayShift?.HasShift == true &&
            WorkStatus?.IsWorking != true &&
            CurrentShiftStatusClass == "planned" &&
            IsNowInShiftWindow();
        public bool CanEndShift => WorkStatus?.IsWorking == true;

        public string StartShiftHint
        {
            get
            {
                if (WorkStatus?.IsWorking == true)
                    return "Bạn đang trong ca làm việc.";

                if (TodayShift?.HasShift != true)
                    return "Hôm nay bạn chưa được phân ca.";

                if (CurrentShiftStatusClass == "done")
                    return "Ca hôm nay đã hoàn thành.";

                if (!IsNowInShiftWindow())
                    return $"Chỉ có thể bắt đầu trong khung giờ {CurrentShiftTime}.";

                return "Ca này chỉ được tính hoạt động sau khi bạn bấm Bắt đầu ca.";
            }
        }

        [TempData]
        public string? ShiftMessage { get; set; }

        [TempData]
        public bool ShiftSuccess { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Ca làm việc";
            ViewData["Role"] = "Nhân viên";

            EmployeeName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";
            EmployeeCode = User.FindFirst("related_id")?.Value ?? User.FindFirst("employeeId")?.Value ?? "";
            ViewData["UserName"] = EmployeeName;

            WorkStatus = await _workLogService.GetCurrentStatusAsync();
            MonthlySummary = await _workLogService.GetMonthlySummaryAsync();
            TodayShift = await _shiftService.GetMyTodayShiftAsync();
            MyWeekShifts = await _shiftService.GetMyWeekAsync() ?? new List<Services.ShiftMyWeekItem>();
            WeekDays = BuildWeekDays(MyWeekShifts);
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

        public string GetShiftInitial(string? shiftType)
        {
            if (string.IsNullOrWhiteSpace(shiftType))
                return "-";

            if (shiftType.Contains("Sáng", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("SÃ", StringComparison.OrdinalIgnoreCase))
                return "S";

            if (shiftType.Contains("Chiều", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("Chi", StringComparison.OrdinalIgnoreCase))
                return "C";

            if (shiftType.Contains("Tối", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("Tá", StringComparison.OrdinalIgnoreCase))
                return "T";

            return shiftType[..1].ToUpperInvariant();
        }

        public string GetShiftClass(string? shiftType, string? status)
        {
            if (IsCompleted(status))
                return "done";

            if (string.IsNullOrWhiteSpace(shiftType))
                return "off";

            if (shiftType.Contains("Sáng", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("SÃ", StringComparison.OrdinalIgnoreCase))
                return "morning";

            if (shiftType.Contains("Chiều", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("Chi", StringComparison.OrdinalIgnoreCase))
                return "afternoon";

            if (shiftType.Contains("Tối", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("Tá", StringComparison.OrdinalIgnoreCase))
                return "night";

            return "morning";
        }

        public bool IsCompleted(string? status)
        {
            return !string.IsNullOrWhiteSpace(status) &&
                   (status.Contains("Hoàn", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("HoÃ", StringComparison.OrdinalIgnoreCase));
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

        private static List<EmployeeShiftDayView> BuildWeekDays(List<Services.ShiftMyWeekItem> shifts)
        {
            var start = GetMondayOfWeek(DateTime.Today);
            return Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = start.AddDays(offset).Date;
                    var shift = shifts.FirstOrDefault(s => s.WorkDate.Date == date);
                    return new EmployeeShiftDayView
                    {
                        WorkDate = date,
                        DayLabel = GetVietnameseDayLabel(date),
                        Shift = shift
                    };
                })
                .ToList();
        }

        private static DateTime GetMondayOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private static string GetVietnameseDayLabel(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                _ => "CN"
            };
        }

        public string FormatVietnameseDate(DateTime date)
        {
            return date.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN"));
        }
    }

    public class EmployeeShiftDayView
    {
        public DateTime WorkDate { get; set; }
        public string DayLabel { get; set; } = "";
        public Services.ShiftMyWeekItem? Shift { get; set; }
    }
}
