using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Auth
{
    [AllowAnonymous]
    public class EmployeeInviteModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeInviteModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [BindProperty(SupportsGet = true, Name = "token")]
        public string? Token { get; set; }

        [BindProperty]
        public ConfirmInviteInput Input { get; set; } = new();

        public EmployeeInviteDto? Invite { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public bool Completed { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadInviteAsync(Token);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Token = Input.InviteToken;

            if (!ModelState.IsValid)
            {
                ErrorMessage = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?.ErrorMessage ?? "Vui lòng kiểm tra lại thông tin.";

                await LoadInviteAsync(Input.InviteToken, setError: false);
                return Page();
            }

            var result = await _employeeService.ConfirmEmployeeInviteAsync(new ConfirmEmployeeInviteDto
            {
                InviteToken = Input.InviteToken,
                PhoneNumber = Input.PhoneNumber,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword
            });

            if (result?.Success == true)
            {
                Completed = true;
                SuccessMessage = result.Message ?? "Hoàn tất tài khoản thành công. Bạn có thể đăng nhập ngay.";
                return Page();
            }

            ErrorMessage = result?.Message ?? "Không thể hoàn tất tài khoản.";
            await LoadInviteAsync(Input.InviteToken, setError: false);
            return Page();
        }

        private async Task LoadInviteAsync(string? token, bool setError = true)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                if (setError)
                {
                    ErrorMessage = "Link mời không hợp lệ hoặc thiếu token.";
                }
                return;
            }

            var result = await _employeeService.GetEmployeeInviteAsync(token);
            if (result?.Success == true && result.Data != null)
            {
                Invite = result.Data;
                Input.InviteToken = result.Data.InviteToken;
                return;
            }

            if (setError)
            {
                ErrorMessage = result?.Message ?? "Link mời không hợp lệ hoặc đã hết hạn.";
            }
        }
    }

    public class ConfirmInviteInput
    {
        [Required]
        public string InviteToken { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Số điện thoại phải có 10 đến 15 chữ số.")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$", ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
