using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.Auth;
using ParkingManagement.FE.Services;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Auth
{
    public class AuthenticateModel : PageModel
    {
        private readonly IAuthService _authService;

        public AuthenticateModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginInputModel LoginInput { get; set; } = new();

        [BindProperty]
        public RegisterInputModel RegisterInput { get; set; } = new();

        public string? ActiveTab { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet(string? tab = null)
        {
            ActiveTab = tab ?? "login";
        }

        // ✅ QUAN TRỌNG: Method xử lý login
        public async Task<IActionResult> OnPostLogin()
        {
            if (!ModelState.IsValid)
            {
                ActiveTab = "login";
                return Page();
            }

            var request = new LoginRequest
            {
                Email = LoginInput.Email,
                Password = LoginInput.Password
            };

            var result = await _authService.LoginAsync(request);

            if (!result.Success || result.Data == null)
            {
                ErrorMessage = result.Message;
                ActiveTab = "login";
                return Page();
            }

            // ✅ Tạo Claims cho Cookie Authentication
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, result.Data.FullName ?? ""),
        new Claim(ClaimTypes.Email, result.Data.Email ?? ""),
        new Claim(ClaimTypes.Role, result.Data.Role ?? ""),
        new Claim("AccountId", result.Data.AccountId ?? ""),
        new Claim("RelatedId", result.Data.RelatedId ?? "")
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = LoginInput.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            // ✅ Đăng nhập - tạo cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ✅ Lưu thêm vào Session (nếu cần dùng ở其他地方)
            HttpContext.Session.SetString("user_role", result.Data.Role ?? "");
            HttpContext.Session.SetString("user_name", result.Data.FullName ?? "");
            HttpContext.Session.SetString("user_email", result.Data.Email ?? "");
            if (!string.IsNullOrEmpty(result.Data.RelatedId))
            {
                HttpContext.Session.SetString(
                    result.Data.Role == "Customer" ? "CustomerId" : "EmployeeId",
                    result.Data.RelatedId
                );
            }

            // ✅ Chuyển hướng theo role
            var role = result.Data.Role?.ToLower() ?? "";
            if (role == "manager")
                return RedirectToPage("/Admin/Dashboard");
            else if (role == "employee")
                return RedirectToPage("/Employee/Dashboard");
            else
                return RedirectToPage("/Customer/Dashboard");
        }

        public async Task<IActionResult> OnPostRegister()
        {
            if (!ModelState.IsValid)
            {
                ActiveTab = "register";
                return Page();
            }

            var request = new RegisterRequest
            {
                Email = RegisterInput.Email,
                Password = RegisterInput.Password,
                ConfirmPassword = RegisterInput.ConfirmPassword,
                FullName = RegisterInput.FullName,
                PhoneNumber = RegisterInput.PhoneNumber
            };

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                ActiveTab = "register";
                return Page();
            }

            // Chuyển sang trang xác thực OTP
            return RedirectToPage("/Auth/VerifyOtp", new { email = RegisterInput.Email });
        }
    }

    // Input Models
    public class LoginInputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class RegisterInputModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}