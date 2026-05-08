using System.Net.Http.Json;
using ParkingManagement.FE.Models.Auth;

namespace ParkingManagement.FE.Services
{
    public interface IAuthService
    {
        Task<(bool Success, LoginResponse? Data, string Message)> LoginAsync(LoginRequest request);
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

        private class ApiErrorResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
