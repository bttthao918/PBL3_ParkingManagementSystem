using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.Auth;
using ParkingManagement.FE.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Account
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthService _authService;

        public ChangePasswordModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();

        public AccountViewModel Account { get; set; } = new();

        public void OnGet()
        {
            LoadAccountRole();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadAccountRole();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, message) = await _authService.ChangePasswordAsync(new ChangePasswordRequest
            {
                OldPassword = Input.CurrentPassword,
                NewPassword = Input.NewPassword,
                ConfirmPassword = Input.ConfirmPassword
            });

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToPage();
            }

            TempData["Success"] = message;
            return RedirectToPage();
        }

        private void LoadAccountRole()
        {
            Account.RoleName = User.FindFirst(ClaimTypes.Role)?.Value ?? "Admin";
        }
    }

    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
            ErrorMessage = "Mật khẩu mới phải có chữ hoa, chữ thường, số và ký tự đặc biệt")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }
}
