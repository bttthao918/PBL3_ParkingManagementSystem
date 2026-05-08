using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.Auth;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Auth
{
    public class VerifyOtpModel : PageModel
    {
        private readonly IAuthService _authService;

        public VerifyOtpModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public OtpInputModel Input { get; set; } = new();

        /// <summary>Email nhận từ query string sau khi đăng ký</summary>
        public string Email { get; set; } = string.Empty;

        [TempData]
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("/Auth/Authenticate");

            Email = email;
            Input.Email = email;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Email = Input.Email;

            if (!ModelState.IsValid)
                return Page();

            var request = new VerifyOtpRequest
            {
                Email = Input.Email.Trim(),
                Otp = Input.Otp.Trim()
            };

            var (success, message) = await _authService.VerifyOtpAsync(request);

            if (!success)
            {
                ErrorMessage = message;
                return Page();
            }

            // OTP đúng → redirect về login với thông báo thành công
            TempData["SuccessMessage"] = "Tài khoản đã được xác thực! Vui lòng đăng nhập.";
            TempData["ActiveTab"] = "login";
            return RedirectToPage("/Auth/Authenticate");
        }

        public class OtpInputModel
        {
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP gồm 6 chữ số")]
            [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP chỉ gồm 6 chữ số")]
            public string Otp { get; set; } = string.Empty;
        }
    }
}
