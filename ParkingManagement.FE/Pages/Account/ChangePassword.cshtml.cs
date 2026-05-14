using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ParkingManagement.FE.Services;

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
        
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            ViewData["Title"] = "Đổi mật khẩu";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "User";
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            ViewData["Role"] = role switch
            {
                "Employee" => "Nhân viên",
                "Manager" => "Quản lý",
                "Customer" => "Khách hàng",
                _ => role
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var request = new ChangePasswordRequest
            {
                CurrentPassword = Input.CurrentPassword,
                NewPassword = Input.NewPassword,
                ConfirmPassword = Input.ConfirmPassword
            };

            var (success, message) = await _authService.ChangePasswordAsync(request);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = message;
                return Page();
            }
        }
    }

    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }
}
