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

    [BindProperty]
    public ForgotPasswordInputModel ForgotPasswordInput { get; set; } = new();

    [BindProperty]
    public ResetPasswordInputModel ResetPasswordInput { get; set; } = new();

    [TempData] public string? ErrorMessage { get; set; }
    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ActiveTab { get; set; }
    [TempData] public string? ForgotStep { get; set; }
    [TempData] public string? ForgotEmail { get; set; }
    [TempData] public string? LoginEmail { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Redirect(GetDashboardPath(role ?? string.Empty));
        }

        ActiveTab ??= "login";
        ForgotStep ??= "email";
        LoginInput.Email = LoginEmail ?? string.Empty;
        LoginInput.Password = string.Empty;

        if (!string.IsNullOrWhiteSpace(ForgotEmail))
        {
            ForgotPasswordInput.Email = ForgotEmail;
            ResetPasswordInput.Email = ForgotEmail;
        }

        return Page();
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
            await ClearAuthenticationStateAsync();
            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            ErrorMessage = firstError ?? "Vui lòng kiểm tra lại thông tin.";
            ActiveTab = "login";
            LoginEmail = LoginInput.Email;
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
            await ClearAuthenticationStateAsync();
            ErrorMessage = message;
            ActiveTab = "login";
            LoginEmail = LoginInput.Email;
            return RedirectToPage();
        }

        // Lưu JWT vào Session
        HttpContext.Session.SetString("jwt_token", data.Token);
        HttpContext.Session.SetString("account_id", data.AccountId);
        HttpContext.Session.SetString("role", data.Role);
        HttpContext.Session.SetString("full_name", data.FullName);
        HttpContext.Session.SetString("related_id", data.RelatedId ?? "");
        HttpContext.Session.SetString("email", data.Email);

        HttpContext.Response.Cookies.Append("jwt_token", data.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = LoginInput.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddHours(24)
        });

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

    public async Task<IActionResult> OnPostForgotPasswordAsync()
    {
        BindForgotPasswordInputFromForm();
        ModelState.Clear();
        TryValidateModel(ForgotPasswordInput, nameof(ForgotPasswordInput));

        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            ErrorMessage = firstError ?? "Vui lòng kiểm tra lại email.";
            ActiveTab = "forgot";
            ForgotStep = "email";
            ForgotEmail = ForgotPasswordInput.Email;
            return RedirectToPage();
        }

        var email = ForgotPasswordInput.Email.Trim();
        var (success, message) = await _authService.RequestPasswordResetAsync(new ForgotPasswordRequest
        {
            Email = email
        });

        if (!success)
        {
            ErrorMessage = message;
            ActiveTab = "forgot";
            ForgotStep = "email";
            ForgotEmail = email;
            return RedirectToPage();
        }

        SuccessMessage = message;
        ActiveTab = "forgot";
        ForgotStep = "reset";
        ForgotEmail = email;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync()
    {
        BindResetPasswordInputFromForm();
        ModelState.Clear();
        TryValidateModel(ResetPasswordInput, nameof(ResetPasswordInput));

        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            ErrorMessage = firstError ?? "Vui lòng kiểm tra lại thông tin.";
            ActiveTab = "forgot";
            ForgotStep = "reset";
            ForgotEmail = ResetPasswordInput.Email;
            return RedirectToPage();
        }

        var (success, message) = await _authService.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = ResetPasswordInput.Email.Trim(),
            Otp = ResetPasswordInput.Otp.Trim(),
            NewPassword = ResetPasswordInput.NewPassword,
            ConfirmPassword = ResetPasswordInput.ConfirmPassword
        });

        if (!success)
        {
            ErrorMessage = message;
            ActiveTab = "forgot";
            ForgotStep = "reset";
            ForgotEmail = ResetPasswordInput.Email;
            return RedirectToPage();
        }

        SuccessMessage = message;
        ActiveTab = "login";
        ForgotStep = "email";
        ForgotEmail = null;
        return RedirectToPage();
    }

    private static string GetDashboardPath(string role) => role switch
    {
        "Admin" => "/Admin/Dashboard",
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

    private void BindForgotPasswordInputFromForm()
    {
        ForgotPasswordInput.Email = Request.Form["ForgotPasswordInput.Email"].ToString().Trim();
    }

    private void BindResetPasswordInputFromForm()
    {
        ResetPasswordInput.Email = Request.Form["ResetPasswordInput.Email"].ToString().Trim();
        ResetPasswordInput.Otp = Request.Form["ResetPasswordInput.Otp"].ToString().Trim();
        ResetPasswordInput.NewPassword = Request.Form["ResetPasswordInput.NewPassword"].ToString();
        ResetPasswordInput.ConfirmPassword = Request.Form["ResetPasswordInput.ConfirmPassword"].ToString();
    }

    private async Task ClearAuthenticationStateAsync()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("jwt_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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

    public class ForgotPasswordInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordInputModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP gồm 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP chỉ gồm 6 chữ số")]
        public string Otp { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
