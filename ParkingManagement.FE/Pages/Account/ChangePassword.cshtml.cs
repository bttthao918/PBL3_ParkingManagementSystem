using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();
        public AccountViewModel Account { get; set; } = new();

        public void OnGet()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Admin";
            Account.RoleName = role;
        }

        public IActionResult OnPost()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Admin";
            Account.RoleName = role;

            if (!ModelState.IsValid)
                return Page();

            // TODO: kiểm tra mật khẩu hiện tại trong database
            // TODO: mã hóa mật khẩu mới
            // TODO: lưu mật khẩu mới

            TempData["Success"] = "Đổi mật khẩu thành công.";
            return RedirectToPage();
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
