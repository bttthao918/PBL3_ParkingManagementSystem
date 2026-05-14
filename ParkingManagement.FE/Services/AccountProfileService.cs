using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IAccountProfileService
    {
        bool LastRequestUnauthorized { get; }
        Task<AccountProfileDto?> GetProfileAsync();
        Task<AccountProfileUpdateResponseDto?> UpdateProfileAsync(UpdateAccountProfileDto dto);
    }

    public class AccountProfileService : IAccountProfileService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountProfileService> _logger;

        public bool LastRequestUnauthorized { get; private set; }

        public AccountProfileService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AccountProfileService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<AccountProfileDto?> GetProfileAsync()
        {
            try
            {
                LastRequestUnauthorized = false;
                var accountId = GetAccountId();
                AttachBearerToken();

                var response = await _httpClient.GetAsync(BuildProfileUrl(accountId));
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AccountProfileDto>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Get account profile failed: {StatusCode} {Body}", response.StatusCode, body);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling account profile API");
                return null;
            }
        }

        public async Task<AccountProfileUpdateResponseDto?> UpdateProfileAsync(UpdateAccountProfileDto dto)
        {
            try
            {
                LastRequestUnauthorized = false;
                var accountId = GetAccountId();
                AttachBearerToken();

                var response = await _httpClient.PutAsJsonAsync(BuildProfileUrl(accountId), dto);
                AccountProfileUpdateResponseDto? result = null;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<AccountProfileUpdateResponseDto>();
                }
                catch
                {
                    // Fallback when API returns a non-JSON error response.
                }

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new AccountProfileUpdateResponseDto { Success = true, Message = "Cập nhật thông tin thành công." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Update account profile failed: {StatusCode} {Body}", response.StatusCode, body);
                return result ?? new AccountProfileUpdateResponseDto { Success = false, Message = "Không thể cập nhật thông tin tài khoản." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account profile");
                return new AccountProfileUpdateResponseDto { Success = false, Message = "Lỗi kết nối đến máy chủ." };
            }
        }

        private static string BuildProfileUrl(string? accountId)
        {
            return string.IsNullOrWhiteSpace(accountId)
                ? "api/auth/profile"
                : $"api/auth/profile?accountId={Uri.EscapeDataString(accountId)}";
        }

        private string? GetAccountId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User.FindFirst("accountId")?.Value
                ?? httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContext?.Session.GetString("account_id");
        }

        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            }

            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
