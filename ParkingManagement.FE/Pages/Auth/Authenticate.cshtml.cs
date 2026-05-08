using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.Auth;
using ParkingManagement.FE.Services;

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

    [TempData] public string? ErrorMessage { get; set; }
    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ActiveTab { get; set; }

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            Response.Redirect(GetDashboardPath(role));
        }
    }

    /// <summary>Xử lý đăng nhập</summary>
    public async Task<IActionResult> OnPostLoginAsync()
    {
        BindLoginInputFromForm();
        ModelState.Clear();
        TryValidateModel(LoginInput, nameof(LoginInput));

        // ✅ FIX: Chỉ validate LoginInput, bỏ qua RegisterInput
        foreach (var key in ModelState.Keys.Where(k => k.StartsWith("RegisterInput")).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            ErrorMessage = firstError ?? "Vui lòng kiểm tra lại thông tin.";
            ActiveTab = "login";
            return RedirectToPage();
        }

        var request = new LoginRequest
        {
            Email = LoginInput.Email.Trim(),
            Password = LoginInput.Password
        };

        var (success, data, message) = await _authService.LoginAsync(request);

        if (!success || data == null)
        {
            ErrorMessage = message;
            ActiveTab = "login";
            return RedirectToPage();
        }

        // Lưu JWT vào Session
        HttpContext.Session.SetString("jwt_token", data.Token);
        HttpContext.Session.SetString("account_id", data.AccountId);
        HttpContext.Session.SetString("role", data.Role);
        HttpContext.Session.SetString("full_name", data.FullName);
        HttpContext.Session.SetString("related_id", data.RelatedId ?? "");
        HttpContext.Session.SetString("email", data.Email);

        // Tạo Cookie Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, data.AccountId),
            new(ClaimTypes.Email, data.Email),
            new(ClaimTypes.Name, data.FullName),
            new(ClaimTypes.Role, data.Role),
            new("related_id", data.RelatedId ?? ""),
            new("jwt_token", data.Token)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = LoginInput.RememberMe,
            ExpiresUtc = LoginInput.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddHours(24)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        return Redirect(GetDashboardPath(data.Role));
    }

    /// <summary>Xử lý đăng ký → gửi OTP email</summary>
    public async Task<IActionResult> OnPostRegisterAsync()
    {
        BindRegisterInputFromForm();
        ModelState.Clear();
        TryValidateModel(RegisterInput, nameof(RegisterInput));

        // ✅ FIX: Chỉ validate RegisterInput, bỏ qua LoginInput
        foreach (var key in ModelState.Keys.Where(k => k.StartsWith("LoginInput")).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            ErrorMessage = firstError ?? "Vui lòng kiểm tra lại thông tin.";
            ActiveTab = "register";
            return RedirectToPage();
        }

        var request = new RegisterRequest
        {
            Email = RegisterInput.Email.Trim(),
            Password = RegisterInput.Password,
            ConfirmPassword = RegisterInput.ConfirmPassword,
            FullName = RegisterInput.FullName.Trim(),
            PhoneNumber = RegisterInput.PhoneNumber.Trim()
        };

        var (success, message) = await _authService.RegisterAsync(request);

        if (!success)
        {
            ErrorMessage = message;
            ActiveTab = "register";
            return RedirectToPage();
        }

        // Đăng ký thành công → chuyển sang trang xác thực OTP
        return RedirectToPage("/Auth/VerifyOtp", new { email = RegisterInput.Email.Trim() });
    }

    private static string GetDashboardPath(string role) => role switch
    {
        "Manager" => "/Admin/Dashboard",
        "Employee" => "/Employee/Dashboard",
        "Customer" => "/Customer/Dashboard",
        _ => "/Auth/Authenticate"
    };

    private void BindLoginInputFromForm()
    {
        LoginInput.Email = Request.Form["LoginInput.Email"].ToString().Trim();
        LoginInput.Password = Request.Form["LoginInput.Password"].ToString();
        LoginInput.RememberMe = Request.Form["LoginInput.RememberMe"]
            .Any(value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));
    }

    private void BindRegisterInputFromForm()
    {
        RegisterInput.FullName = Request.Form["RegisterInput.FullName"].ToString().Trim();
        RegisterInput.Email = Request.Form["RegisterInput.Email"].ToString().Trim();
        RegisterInput.PhoneNumber = Request.Form["RegisterInput.PhoneNumber"].ToString().Trim();
        RegisterInput.Password = Request.Form["RegisterInput.Password"].ToString();
        RegisterInput.ConfirmPassword = Request.Form["RegisterInput.ConfirmPassword"].ToString();
    }

    // ── Input Models ──────────────────────────────────────────────

    public class LoginInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
