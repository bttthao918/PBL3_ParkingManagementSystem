using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;
using ParkingManagement.FE.Models;
using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class EmployeeManagementModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeManagementModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public List<EmployeeViewModel> Employees { get; set; } = new();
        public int TotalEmployees { get; set; }
        [BindProperty]
        public CreateEmployeeFormInput CreateEmployeeInput { get; set; } = new();
        public string? CreateEmployeeMessage { get; set; }
        public bool CreateEmployeeSuccess { get; set; }
        public bool CreateEmployeePendingVerification { get; set; }

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

        private async Task LoadEmployeesAsync()
        {
            var filter = new ManagerEmployeeFilterDto
            {
                PageNumber = 1,
                PageSize = 100
            };

            var result = await _employeeService.GetEmployeesAsync(filter);

            if (result != null && result.Items != null)
            {
                TotalEmployees = result.TotalItems;
                Employees = result.Items.Select(e => new EmployeeViewModel(
                    e.EmployeeId,
                    e.FullName,
                    "Nhân viên bãi xe", // Temporarily hardcoded or mapped if BE provides it
                    "Bãi xe A", // Temporarily hardcoded
                    e.PhoneNumber,
                    0, // TotalTickets (BE detail endpoint has this, list doesn't)
                    0, // ProcessedTickets
                    e.Status,
                    e.Status == "Hoạt động" ? "active" : "inactive",
                    "/images/avatar-demo.jpg"
                )).ToList();
            }
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

    [Authorize(Roles = "Manager")]
    public class EmployeeViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Phone { get; set; }
        public int TotalTickets { get; set; }
        public int ProcessedTickets { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; }
        public string Avatar { get; set; }

        public EmployeeViewModel(
            string code,
            string name,
            string position,
            string department,
            string phone,
            int totalTickets,
            int processedTickets,
            string status,
            string statusClass,
            string avatar)
        {
            Code = code;
            Name = name;
            Position = position;
            Department = department;
            Phone = phone;
            TotalTickets = totalTickets;
            ProcessedTickets = processedTickets;
            Status = status;
            StatusClass = statusClass;
            Avatar = avatar;
        }
    }
}
