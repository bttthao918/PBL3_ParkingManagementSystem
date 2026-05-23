using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class EmployeeManagementModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IShiftScheduleService _shiftService;
        private const string ActiveStatus = "Hoạt động";
        private const string DisabledStatus = "Vô hiệu hóa";
        private const string DeletedStatus = "Đã xóa";

        public EmployeeManagementModel(IEmployeeService employeeService, IShiftScheduleService shiftService)
        {
            _employeeService = employeeService;
            _shiftService = shiftService;
        }

        public List<EmployeeViewModel> Employees { get; set; } = new();
        public ManagerEmployeeDetailDto? SelectedEmployee { get; set; }
        public List<EmployeeShiftDayView> SelectedEmployeeWeekShifts { get; set; } = new();
        public int TotalEmployees { get; set; }
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
        public int TotalDeleted { get; set; }
        public int TotalAllEmployees { get; set; }
        public int TotalPages { get; set; }
        public int ShowingFrom { get; set; }
        public int ShowingTo { get; set; }
        public string? LoadErrorMessage { get; set; }

        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        [BindProperty]
        public CreateEmployeeFormInput CreateEmployeeInput { get; set; } = new();

        public string? CreateEmployeeMessage { get; set; }
        public bool CreateEmployeeSuccess { get; set; }
        public bool CreateEmployeePendingVerification { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Shift { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? SelectedEmployeeId { get; set; }

        public int StartPage => TotalPages == 0 ? 0 : Math.Max(1, PageNumber - 2);
        public int EndPage => TotalPages == 0 ? 0 : Math.Min(TotalPages, PageNumber + 2);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
        public bool IsDeletedFolder => IsDeletedStatus(Status);
        public DateTime CurrentWeekStart => GetMondayOfWeek(DateTime.Today);
        public DateTime CurrentWeekEnd => CurrentWeekStart.AddDays(6);
        public decimal ActiveRate => TotalAllEmployees == 0 ? 0 : TotalActive * 100m / TotalAllEmployees;
        public decimal InactiveRate => TotalAllEmployees == 0 ? 0 : TotalInactive * 100m / TotalAllEmployees;
        public decimal DeletedRate => TotalAllEmployees == 0 ? 0 : TotalDeleted * 100m / TotalAllEmployees;

        public async Task OnGetAsync()
        {
            await LoadEmployeesAsync();
        }

        public async Task<IActionResult> OnPostCreateEmployeeAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadEmployeesAsync();
                CreateEmployeeSuccess = false;
                CreateEmployeeMessage = "Vui lòng kiểm tra lại dữ liệu nhập.";
                return Page();
            }

            var request = new CreateEmployeeInviteByManagerDto
            {
                FullName = CreateEmployeeInput.FullName.Trim(),
                Email = CreateEmployeeInput.Email.Trim(),
                SendInvitationEmail = true
            };

            var result = await _employeeService.CreateEmployeeInviteAsync(request);
            CreateEmployeeSuccess = result?.Success == true;
            CreateEmployeeMessage = result?.Message ?? "Không thể tạo nhân viên.";
            CreateEmployeePendingVerification = CreateEmployeeSuccess && result?.InviteExpiry != null;

            if (CreateEmployeeSuccess)
            {
                ModelState.Clear();
                CreateEmployeeInput = new CreateEmployeeFormInput();
            }

            await LoadEmployeesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateEmployeeAsync(
            string employeeId,
            string fullName,
            string phoneNumber,
            string? employeeShift,
            string employeeStatus)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                ActionSuccess = false;
                ActionMessage = "Không tìm thấy nhân viên cần cập nhật.";
                return RedirectToPage(BuildRouteValues());
            }

            var result = await _employeeService.UpdateEmployeeAsync(employeeId, new UpdateEmployeeByManagerDto
            {
                FullName = fullName?.Trim(),
                PhoneNumber = phoneNumber?.Trim(),
                Shift = string.IsNullOrWhiteSpace(employeeShift) ? null : employeeShift.Trim(),
                Status = employeeStatus
            });

            ActionSuccess = result?.Success == true;
            ActionMessage = result?.Message ?? (ActionSuccess ? "Đã cập nhật nhân viên." : "Không thể cập nhật nhân viên.");

            if (ActionSuccess)
            {
                if (IsDeletedStatus(employeeStatus))
                    Status = DeletedStatus;
                else if (IsDisabledStatus(employeeStatus))
                    Status = DisabledStatus;
                else
                    Status = null;

                PageNumber = 1;
                SelectedEmployeeId = employeeId;
            }

            return RedirectToPage(BuildRouteValues(employeeId));
        }

        public async Task<IActionResult> OnPostSetEmployeeStatusAsync(string employeeId, string targetStatus, string? reason)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                ActionSuccess = false;
                ActionMessage = "Không tìm thấy nhân viên cần thao tác.";
                return RedirectToPage(BuildRouteValues());
            }

            if (targetStatus == DeletedStatus)
            {
                var deleteResult = await _employeeService.DeleteEmployeeAsync(new DeleteEmployeeDto
                {
                    EmployeeId = employeeId,
                    Reason = reason
                });

                ActionSuccess = deleteResult?.Success == true;
                ActionMessage = deleteResult?.Message ?? (ActionSuccess ? "Đã xóa nhân viên." : "Không thể xóa nhân viên.");
                if (ActionSuccess)
                {
                    Status = DeletedStatus;
                    PageNumber = 1;
                    SelectedEmployeeId = employeeId;
                }
            }
            else if (targetStatus == DisabledStatus)
            {
                var updateResult = await _employeeService.UpdateEmployeeAsync(employeeId, new UpdateEmployeeByManagerDto
                {
                    Status = DisabledStatus
                });

                ActionSuccess = updateResult?.Success == true;
                ActionMessage = updateResult?.Message ?? (ActionSuccess ? "Đã vô hiệu hóa nhân viên." : "Không thể vô hiệu hóa nhân viên.");
                if (ActionSuccess)
                {
                    Status = DisabledStatus;
                    PageNumber = 1;
                    SelectedEmployeeId = employeeId;
                }
            }
            else
            {
                var updateResult = await _employeeService.UpdateEmployeeAsync(employeeId, new UpdateEmployeeByManagerDto
                {
                    Status = ActiveStatus
                });

                ActionSuccess = updateResult?.Success == true;
                ActionMessage = updateResult?.Message ?? (ActionSuccess ? "Đã kích hoạt nhân viên." : "Không thể kích hoạt nhân viên.");
                if (ActionSuccess)
                {
                    Status = null;
                    PageNumber = 1;
                    SelectedEmployeeId = employeeId;
                }
            }

            return RedirectToPage(BuildRouteValues(employeeId));
        }

        public async Task<IActionResult> OnPostRestoreEmployeeAsync(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                ActionSuccess = false;
                ActionMessage = "Không tìm thấy nhân viên cần khôi phục.";
                return RedirectToPage(BuildRouteValues());
            }

            var result = await _employeeService.RestoreEmployeeAsync(employeeId);
            ActionSuccess = result?.Success == true;
            ActionMessage = result?.Message ?? (ActionSuccess ? "Đã khôi phục nhân viên." : "Không thể khôi phục nhân viên.");

            if (ActionSuccess)
            {
                Status = null;
                PageNumber = 1;
                SelectedEmployeeId = employeeId;
            }

            return RedirectToPage(BuildRouteValues(employeeId));
        }

        public async Task<IActionResult> OnPostCreateShiftAsync(string employeeId, DateTime workDate, string shiftType, string? note)
        {
            var result = await _shiftService.CreateAsync(employeeId, workDate, shiftType, note);
            ActionSuccess = result?.Success ?? false;
            ActionMessage = result?.Message ?? "Không thể tạo ca.";
            return RedirectToPage(BuildRouteValues(employeeId));
        }

        public async Task<IActionResult> OnPostDeleteShiftAsync(string employeeId, string scheduleId)
        {
            if (string.IsNullOrWhiteSpace(employeeId) || string.IsNullOrWhiteSpace(scheduleId))
            {
                ActionSuccess = false;
                ActionMessage = "Không tìm thấy ca cần xóa.";
                return RedirectToPage(BuildRouteValues(employeeId));
            }

            var result = await _shiftService.DeleteAsync(scheduleId);
            ActionSuccess = result?.Success ?? false;
            ActionMessage = result?.Message ?? "Không thể xóa ca.";
            return RedirectToPage(BuildRouteValues(employeeId));
        }

        private async Task LoadEmployeesAsync()
        {
            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            PageSize = PageSize <= 0 ? 10 : Math.Min(PageSize, 100);

            var filter = new ManagerEmployeeFilterDto
            {
                SearchKeyword = Keyword,
                Status = Status,
                Shift = Shift,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var result = await _employeeService.GetEmployeesAsync(filter);
            if (result != null && result.TotalPages > 0 && PageNumber > result.TotalPages)
            {
                PageNumber = result.TotalPages;
                filter.PageNumber = PageNumber;
                result = await _employeeService.GetEmployeesAsync(filter);
            }

            if (result == null)
            {
                LoadErrorMessage = _employeeService.LastRequestUnauthorized
                    ? "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                    : "Không tải được danh sách nhân viên từ Backend API.";
                return;
            }

            TotalEmployees = result.TotalItems;
            TotalActive = result.TotalActive;
            TotalInactive = result.TotalInactive;
            TotalDeleted = result.TotalDeleted;
            TotalAllEmployees = TotalActive + TotalInactive + TotalDeleted;
            TotalPages = result.TotalPages;
            PageNumber = result.PageNumber > 0 ? result.PageNumber : PageNumber;
            PageSize = result.PageSize > 0 ? result.PageSize : PageSize;
            ShowingFrom = TotalEmployees == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
            ShowingTo = TotalEmployees == 0 ? 0 : Math.Min(PageNumber * PageSize, TotalEmployees);

            Employees = result.Items.Select(e => new EmployeeViewModel(
                e.EmployeeId,
                string.IsNullOrWhiteSpace(e.EmployeeCode) ? e.EmployeeId : e.EmployeeCode,
                e.FullName,
                e.Email,
                "Nhân viên bãi xe",
                e.Shift ?? "Chưa phân ca",
                e.PhoneNumber,
                0,
                0,
                e.Status,
                GetStatusClass(e.Status),
                "/images/avatar-demo.jpg",
                e.CreatedAt
            )).ToList();

            if (string.IsNullOrWhiteSpace(SelectedEmployeeId))
            {
                SelectedEmployeeId = Employees.FirstOrDefault()?.Id;
            }

            if (!string.IsNullOrWhiteSpace(SelectedEmployeeId))
            {
                SelectedEmployee = await _employeeService.GetEmployeeDetailAsync(SelectedEmployeeId);
                await LoadSelectedEmployeeWeekShiftsAsync(SelectedEmployeeId);
            }
        }

        private async Task LoadSelectedEmployeeWeekShiftsAsync(string employeeId)
        {
            var week = await _shiftService.GetWeekScheduleAsync(CurrentWeekStart);
            var schedules = week?.Schedules
                .Where(s => s.EmployeeId == employeeId)
                .ToList() ?? new List<ShiftScheduleItem>();

            SelectedEmployeeWeekShifts = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = CurrentWeekStart.AddDays(offset).Date;
                    var daySchedules = schedules
                        .Where(s => s.WorkDate.Date == date)
                        .OrderBy(s => ParseTime(s.StartTime))
                        .ToList();

                    return new EmployeeShiftDayView
                    {
                        WorkDate = date,
                        DayLabel = GetVietnameseDayLabel(date),
                        IsToday = date == DateTime.Today,
                        Schedules = daySchedules
                    };
                })
                .ToList();
        }

        private object BuildRouteValues(string? selectedEmployeeId = null)
        {
            return new
            {
                Keyword,
                Status,
                Shift,
                PageNumber,
                PageSize,
                SelectedEmployeeId = selectedEmployeeId ?? SelectedEmployeeId
            };
        }

        public static string GetStatusClass(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "inactive";

            if (IsDeletedStatus(status))
                return "deleted";

            if (IsDisabledStatus(status))
                return "disabled";

            if (status.Contains("Hoạt", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Hoáº¡t", StringComparison.OrdinalIgnoreCase))
                return "active";

            return "inactive";
        }

        public static bool IsDeletedStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            return status.Contains("Đã", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("xóa", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("xoa", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("deleted", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDisabledStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            return status.Contains("Vô", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("VÃ´", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("vo", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("disabled", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetShiftBadgeClass(string? shiftType)
        {
            if (string.IsNullOrWhiteSpace(shiftType))
                return "none";

            if (shiftType.Contains("SÃ¡ng", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("Sáng", StringComparison.OrdinalIgnoreCase))
                return "morning";

            if (shiftType.Contains("Chiá»u", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("Chiều", StringComparison.OrdinalIgnoreCase))
                return "afternoon";

            if (shiftType.Contains("Tá»‘i", StringComparison.OrdinalIgnoreCase) ||
                shiftType.Contains("Tối", StringComparison.OrdinalIgnoreCase))
                return "night";

            return "none";
        }

        public static string GetScheduleStatusClass(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "planned";

            if (status.Contains("Äang", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Đang", StringComparison.OrdinalIgnoreCase))
                return "working";

            if (status.Contains("HoÃ n", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Hoàn", StringComparison.OrdinalIgnoreCase))
                return "done";

            return "planned";
        }

        public static bool CanDeleteSchedule(string? status)
        {
            var statusClass = GetScheduleStatusClass(status);
            return statusClass != "working" && statusClass != "done";
        }

        private static DateTime GetMondayOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private static TimeSpan ParseTime(string? value)
        {
            return TimeSpan.TryParse(value, out var time) ? time : TimeSpan.Zero;
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
    }

    public class EmployeeShiftDayView
    {
        public DateTime WorkDate { get; set; }
        public string DayLabel { get; set; } = "";
        public bool IsToday { get; set; }
        public List<ShiftScheduleItem> Schedules { get; set; } = new();
    }

    public class CreateEmployeeFormInput
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [MinLength(3)]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

    }

    public class EmployeeViewModel
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Phone { get; set; }
        public int TotalTickets { get; set; }
        public int ProcessedTickets { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; }
        public string Avatar { get; set; }
        public string Initials { get; set; }
        public DateTime CreatedAt { get; set; }

        public EmployeeViewModel(
            string id,
            string code,
            string name,
            string email,
            string position,
            string department,
            string phone,
            int totalTickets,
            int processedTickets,
            string status,
            string statusClass,
            string avatar,
            DateTime createdAt)
        {
            Id = id;
            Code = code;
            Name = name;
            Email = email;
            Position = position;
            Department = department;
            Phone = phone;
            TotalTickets = totalTickets;
            ProcessedTickets = processedTickets;
            Status = status;
            StatusClass = statusClass;
            Avatar = avatar;
            Initials = BuildInitials(name);
            CreatedAt = createdAt;
        }

        private static string BuildInitials(string name)
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
                return "NV";

            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpperInvariant();

            return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
        }
    }
}
