using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models.Auth;

namespace ParkingManagement.FE.Services
{
    public interface IAuthService
    {
        Task<(bool Success, LoginResponse? Data, string Message)> LoginAsync(LoginRequest request);
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request);
        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordRequest request);
        Task<CurrentUserDto?> GetCurrentUserAsync();
        Task<(bool Success, CurrentUserDto? Data, string Message)> UpdateCurrentUserAsync(UpdateCurrentUserRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value
                ?? _httpContextAccessor.HttpContext?.Session.GetString("jwt_token")
                ?? _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        /// <summary>Gọi POST /api/auth/login</summary>
        public async Task<(bool Success, LoginResponse? Data, string Message)> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    email = request.Email,
                    password = request.Password
                });

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return (true, data, "Đăng nhập thành công.");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                var message = error?.Message ?? "Email hoặc mật khẩu không đúng.";
                _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, message);
                return (false, null, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server.");
                return (false, null, "Không thể kết nối đến server. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return (false, null, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        /// <summary>Gọi POST /api/auth/register → gửi OTP email</summary>
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
                {
                    email = request.Email,
                    password = request.Password,
                    confirmPassword = request.ConfirmPassword,
                    fullName = request.FullName,
                    phoneNumber = request.PhoneNumber
                });

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Mã OTP đã được gửi đến email của bạn.");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                var message = error?.Message ?? "Đăng ký thất bại.";
                _logger.LogWarning("Register failed for {Email}: {Message}", request.Email, message);
                return (false, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server.");
                return (false, "Không thể kết nối đến server. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error");
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        /// <summary>Gọi POST /api/auth/verify-otp → xác nhận OTP, tạo tài khoản</summary>
        public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/verify-otp", new
                {
                    email = request.Email,
                    otp = request.Otp
                });

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Xác thực thành công! Tài khoản đã được tạo.");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                var message = error?.Message ?? "Mã OTP không đúng hoặc đã hết hạn.";
                _logger.LogWarning("OTP verify failed for {Email}: {Message}", request.Email, message);
                return (false, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server.");
                return (false, "Không thể kết nối đến server. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyOtp error");
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        /// <summary>Gọi POST /api/auth/change-password</summary>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", new
                {
                    oldPassword = request.CurrentPassword,
                    newPassword = request.NewPassword,
                    confirmPassword = request.ConfirmPassword
                });

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đổi mật khẩu thành công.");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                var message = error?.Message ?? "Đổi mật khẩu thất bại.";
                _logger.LogWarning("ChangePassword failed: {Message}", message);
                return (false, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server.");
                return (false, "Không thể kết nối đến server. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePassword error");
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        /// <summary>Gọi GET /api/auth/me</summary>
        public async Task<CurrentUserDto?> GetCurrentUserAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/auth/me");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CurrentUserDto>();
                }

                _logger.LogWarning("GetCurrentUser failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCurrentUser error");
                return null;
            }
        }

        /// <summary>Gọi PUT /api/auth/me</summary>
        public async Task<(bool Success, CurrentUserDto? Data, string Message)> UpdateCurrentUserAsync(UpdateCurrentUserRequest request)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync("api/auth/me", new
                {
                    fullName = request.FullName,
                    phoneNumber = request.PhoneNumber,
                    gender = request.Gender
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UpdateCurrentUserResponse>();
                    return (true, result?.Data, result?.Message ?? "Cập nhật thông tin thành công.");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                var message = error?.Message ?? "Cập nhật thông tin thất bại.";
                _logger.LogWarning("UpdateCurrentUser failed: {Message}", message);
                return (false, null, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server.");
                return (false, null, "Không thể kết nối đến server. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCurrentUser error");
                return (false, null, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        private class ApiErrorResponse
        {
            public string Message { get; set; } = string.Empty;
        }

        private class UpdateCurrentUserResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public CurrentUserDto? Data { get; set; }
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }

    public class CurrentUserDto
    {
        public string AccountId { get; set; } = "";
        public string Role { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RelatedId { get; set; }
    }

    public class UpdateCurrentUserRequest
    {
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Gender { get; set; } = "";
    }
}
