using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IEmployeeMonthlyTicketService
    {
        Task<EmployeeMonthlyTicketListResponse?> GetAllAsync(string? search, string? status, string? vehicleType, int page = 1, int pageSize = 10);
        Task<EmployeeMonthlyTicketDetailResponse?> GetDetailAsync(string id);
        Task<ApiResultResponse?> CreateAsync(CreateEmployeeMonthlyTicketRequest dto);
        Task<ApiResultResponse?> RenewAsync(string id, RenewEmployeeMonthlyTicketRequest dto);
        Task<ApiResultResponse?> CancelAsync(string id);
        Task<List<EmployeeMonthlyTicketPricingItem>?> GetPricingAsync();
    }

    public class EmployeeMonthlyTicketService : IEmployeeMonthlyTicketService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmployeeMonthlyTicketService> _logger;

        public EmployeeMonthlyTicketService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EmployeeMonthlyTicketService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuth()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value
                ?? _httpContextAccessor.HttpContext?.Session.GetString("jwt_token")
                ?? _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        public async Task<EmployeeMonthlyTicketListResponse?> GetAllAsync(string? search, string? status, string? vehicleType, int page = 1, int pageSize = 10)
        {
            try
            {
                AddAuth();
                var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
                if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (!string.IsNullOrWhiteSpace(vehicleType)) queryParams.Add($"vehicleType={Uri.EscapeDataString(vehicleType)}");

                var url = $"api/employee/monthly-tickets?{string.Join("&", queryParams)}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<EmployeeMonthlyTicketListResponse>();

                _logger.LogWarning("GetAllAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllAsync");
                return null;
            }
        }

        public async Task<EmployeeMonthlyTicketDetailResponse?> GetDetailAsync(string id)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.GetAsync($"api/employee/monthly-tickets/{Uri.EscapeDataString(id)}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<EmployeeMonthlyTicketDetailResponse>();

                _logger.LogWarning("GetDetailAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetailAsync");
                return null;
            }
        }

        public async Task<ApiResultResponse?> CreateAsync(CreateEmployeeMonthlyTicketRequest dto)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/employee/monthly-tickets", dto);
                return await response.Content.ReadFromJsonAsync<ApiResultResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAsync");
                return null;
            }
        }

        public async Task<ApiResultResponse?> RenewAsync(string id, RenewEmployeeMonthlyTicketRequest dto)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync($"api/employee/monthly-tickets/{Uri.EscapeDataString(id)}/renew", dto);
                return await response.Content.ReadFromJsonAsync<ApiResultResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RenewAsync");
                return null;
            }
        }

        public async Task<ApiResultResponse?> CancelAsync(string id)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync($"api/employee/monthly-tickets/{Uri.EscapeDataString(id)}/cancel", new { });
                return await response.Content.ReadFromJsonAsync<ApiResultResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CancelAsync");
                return null;
            }
        }

        public async Task<List<EmployeeMonthlyTicketPricingItem>?> GetPricingAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/employee/monthly-tickets/pricing");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<EmployeeMonthlyTicketPricingItem>>();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPricingAsync");
                return null;
            }
        }
    }
}
