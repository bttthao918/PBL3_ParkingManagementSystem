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

        public EmployeeManagementModel(IEmployeeService employeeService, IShiftScheduleService shiftService)
        {
            _employeeService = employeeService;
            _shiftService = shiftService;
        }

        public List<EmployeeViewModel> Employees { get; set; } = new();
        public ManagerEmployeeDetailDto? SelectedEmployee { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
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
        public decimal ActiveRate => TotalEmployees == 0 ? 0 : TotalActive * 100m / TotalEmployees;
        public decimal InactiveRate => TotalEmployees == 0 ? 0 : TotalInactive * 100m / TotalEmployees;

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
                PhoneNumber = CreateEmployeeInput.PhoneNumber.Trim(),
                Password = CreateEmployeeInput.Password,
                ConfirmPassword = CreateEmployeeInput.ConfirmPassword,
                SendInvitationEmail = CreateEmployeeInput.SendInvitationEmail
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

            if (targetStatus == DisabledStatus)
            {
                var deleteResult = await _employeeService.DeleteEmployeeAsync(new DeleteEmployeeDto
                {
                    EmployeeId = employeeId,
                    Reason = reason
                });

                ActionSuccess = deleteResult?.Success == true;
                ActionMessage = deleteResult?.Message ?? (ActionSuccess ? "Đã vô hiệu hóa nhân viên." : "Không thể vô hiệu hóa nhân viên.");
            }
            else
            {
                var updateResult = await _employeeService.UpdateEmployeeAsync(employeeId, new UpdateEmployeeByManagerDto
                {
                    Status = ActiveStatus
                });

                ActionSuccess = updateResult?.Success == true;
                ActionMessage = updateResult?.Message ?? (ActionSuccess ? "Đã kích hoạt nhân viên." : "Không thể kích hoạt nhân viên.");
            }

            return RedirectToPage(BuildRouteValues(employeeId));
        }

        public async Task<IActionResult> OnPostCreateShiftAsync(string employeeId, DateTime workDate, string shiftType)
        {
            var result = await _shiftService.CreateAsync(employeeId, workDate, shiftType);
            ActionSuccess = result?.Success ?? false;
            ActionMessage = result?.Message ?? "Không thể tạo ca.";
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
            }
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

            if (status.Contains("Vô", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("VÃ´", StringComparison.OrdinalIgnoreCase))
                return "disabled";

            if (status.Contains("Hoạt", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Hoáº¡t", StringComparison.OrdinalIgnoreCase))
                return "active";

            return "inactive";
        }
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

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Số điện thoại phải từ 10-15 chữ số.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]).{8,}$",
            ErrorMessage = "Mật khẩu tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool SendInvitationEmail { get; set; } = true;
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
