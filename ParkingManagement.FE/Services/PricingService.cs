using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IPricingService
    {
        Task<PricingDto?> GetCurrentPricingAsync();
        Task<ServiceResultDto<PricingDto>?> UpdatePricingAsync(UpdatePricingDto input);
    }

    public class PricingService : IPricingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PricingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PricingService(
            HttpClient httpClient,
            ILogger<PricingService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PricingDto?> GetCurrentPricingAsync()
        {
            ApplyAuthorizationHeader(requireToken: false);

            var response = await _httpClient.GetAsync("api/pricing");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PricingDto>();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phien dang nhap API da het han.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("GetCurrentPricingAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
            return null;
        }

        public async Task<ServiceResultDto<PricingDto>?> UpdatePricingAsync(UpdatePricingDto input)
        {
            ApplyAuthorizationHeader(requireToken: true);

            var response = await _httpClient.PutAsJsonAsync("api/pricing", input);
            var responseBody = await response.Content.ReadAsStringAsync();
            ServiceResultDto<PricingDto>? result = null;

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try
                {
                    result = JsonSerializer.Deserialize<ServiceResultDto<PricingDto>>(
                        responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "UpdatePricingAsync could not parse response body.");
                }
            }

            if (response.IsSuccessStatusCode)
                return result ?? new ServiceResultDto<PricingDto> { Success = true };

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phien dang nhap API da het han.");
            }

            _logger.LogWarning("UpdatePricingAsync failed: {StatusCode} {Body}", response.StatusCode, responseBody);

            return result ?? new ServiceResultDto<PricingDto>
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(responseBody)
                    ? "Khong the cap nhat bang gia ve."
                    : $"Khong the cap nhat bang gia ve. Chi tiet: {responseBody}"
            };
        }

        private string? ApplyAuthorizationHeader(bool requireToken)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
            }
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            }

            if (string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                if (requireToken)
                {
                    throw new UnauthorizedAccessException("Khong tim thay JWT de goi Backend API.");
                }

                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return token;
        }
    }
}
