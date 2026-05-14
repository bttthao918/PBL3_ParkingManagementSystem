using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IMonthlyTicketService
    {
        Task<ListMonthlyTicketDto?> GetAllAsync();
        Task<MonthlyTicketDetailDto?> GetByIdAsync(string monthlyTicketId);
        Task<RegisterMonthlyTicketResponseDto?> RegisterAsync(RegisterMonthlyTicketDto dto);
        Task<RenewMonthlyTicketResponseDto?> RenewAsync(string monthlyTicketId, RenewMonthlyTicketDto dto);
        Task<ServiceResultDto?> CancelAsync(string monthlyTicketId);
        Task<MonthlyTicketPricingDto?> GetPricingAsync();
    }

    public class MonthlyTicketService : IMonthlyTicketService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MonthlyTicketService> _logger;

        public MonthlyTicketService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MonthlyTicketService> logger)
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

        public async Task<ListMonthlyTicketDto?> GetAllAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/monthly-tickets");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListMonthlyTicketDto>();
                }
                _logger.LogWarning("GetAllAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetAllAsync");
                return null;
            }
        }

        public async Task<MonthlyTicketDetailDto?> GetByIdAsync(string monthlyTicketId)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/monthly-tickets/{Uri.EscapeDataString(monthlyTicketId)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MonthlyTicketDetailDto>();
                }
                _logger.LogWarning("GetByIdAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetByIdAsync");
                return null;
            }
        }

        public async Task<RegisterMonthlyTicketResponseDto?> RegisterAsync(RegisterMonthlyTicketDto dto)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("api/monthly-tickets", dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RegisterMonthlyTicketResponseDto>();
                }
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("RegisterAsync failed: {StatusCode} {Error}", response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling RegisterAsync");
                return null;
            }
        }

        public async Task<RenewMonthlyTicketResponseDto?> RenewAsync(string monthlyTicketId, RenewMonthlyTicketDto dto)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync($"api/monthly-tickets/{Uri.EscapeDataString(monthlyTicketId)}/renew", dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RenewMonthlyTicketResponseDto>();
                }
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("RenewAsync failed: {StatusCode} {Error}", response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling RenewAsync");
                return null;
            }
        }

        public async Task<ServiceResultDto?> CancelAsync(string monthlyTicketId)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/monthly-tickets/{Uri.EscapeDataString(monthlyTicketId)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ServiceResultDto>();
                }
                _logger.LogWarning("CancelAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CancelAsync");
                return null;
            }
        }

        public async Task<MonthlyTicketPricingDto?> GetPricingAsync()
        {
            try
            {
                // This endpoint doesn't require auth
                var response = await _httpClient.GetAsync("api/monthly-tickets/pricing");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MonthlyTicketPricingDto>();
                }
                _logger.LogWarning("GetPricingAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetPricingAsync");
                return null;
            }
        }
    }
}
