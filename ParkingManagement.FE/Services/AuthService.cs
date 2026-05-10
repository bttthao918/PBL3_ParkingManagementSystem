using System.Net.Http.Json;
using System.Text.Json;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Success, LoginResponse? Data, string Message)> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting login for {Email}", request.Email);

                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    email = request.Email,
                    password = request.Password
                });

                // Đọc response body để debug
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response: Status={Status}, Body={Body}",
                    response.StatusCode, responseBody);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var data = JsonSerializer.Deserialize<LoginResponse>(responseBody, options);

                    if (data?.Token != null)
                    {
                        var session = _httpContextAccessor.HttpContext?.Session;
                        if (session != null)
                        {
                            session.SetString("jwt_token", data.Token);
                            session.SetString("user_role", data.Role);
                            session.SetString("user_name", data.FullName);
                            session.SetString("user_email", data.Email);

                            // ✅ SỬA: RelatedId (không phải ReliedId)
                            if (!string.IsNullOrEmpty(data.RelatedId))
                            {
                                session.SetString(
                                    data.Role == "Customer" ? "CustomerId" : "EmployeeId",
                                    data.RelatedId
                                );
                            }
                        }

                        _logger.LogInformation("Login successful for {Email}", request.Email);
                        return (true, data, "Đăng nhập thành công.");
                    }

                    _logger.LogWarning("No token in response for {Email}", request.Email);
                    return (false, null, "Không nhận được token.");
                }

                // Xử lý lỗi từ backend
                var errorMessage = "Email hoặc mật khẩu không đúng.";
                try
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody);
                    if (error != null && !string.IsNullOrEmpty(error.Message))
                    {
                        errorMessage = error.Message;
                    }
                }
                catch
                {
                    errorMessage = $"Lỗi server: {response.StatusCode}";
                }

                _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, errorMessage);
                return (false, null, errorMessage);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server");
                return (false, null, $"Không thể kết nối đến server. Backend có đang chạy không?");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout for {Email}", request.Email);
                return (false, null, "Request timeout. Backend phản hồi quá chậm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", request.Email);
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

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

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Registration successful for {Email}", request.Email);
                    return (true, "Mã OTP đã được gửi đến email của bạn.");
                }

                var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody);
                var message = error?.Message ?? "Đăng ký thất bại.";
                _logger.LogWarning("Register failed for {Email}: {Message}", request.Email, message);
                return (false, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server during registration");
                return (false, "Không thể kết nối đến server. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error for {Email}", request.Email);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/verify-otp", new
                {
                    email = request.Email,
                    otp = request.Otp
                });

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("OTP verified for {Email}", request.Email);
                    return (true, "Xác thực thành công! Tài khoản đã được tạo.");
                }

                var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody);
                var message = error?.Message ?? "Mã OTP không đúng hoặc đã hết hạn.";
                _logger.LogWarning("OTP verify failed for {Email}: {Message}", request.Email, message);
                return (false, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot connect to server during OTP verification");
                return (false, "Không thể kết nối đến server. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyOtp error for {Email}", request.Email);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        // ✅ CHỈ GIỮ 1 định nghĩa ApiErrorResponse ở cuối file
        private class ApiErrorResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}