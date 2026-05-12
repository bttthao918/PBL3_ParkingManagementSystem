using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IEmployeeService
    {
        Task<ListManagerEmployeeDto?> GetEmployeesAsync(ManagerEmployeeFilterDto filter);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EmployeeService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ListManagerEmployeeDto?> GetEmployeesAsync(ManagerEmployeeFilterDto filter)
        {
            try
            {
                AttachBearerToken();
                
                var url = $"api/employees/manager/list?PageNumber={filter.PageNumber}&PageSize={filter.PageSize}";
                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                {
                    url += $"&SearchKeyword={Uri.EscapeDataString(filter.SearchKeyword)}";
                }
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    url += $"&Status={Uri.EscapeDataString(filter.Status)}";
                }
                if (!string.IsNullOrWhiteSpace(filter.Shift))
                {
                    url += $"&Shift={Uri.EscapeDataString(filter.Shift)}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListManagerEmployeeDto>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Employee API call failed: {StatusCode} {Url} {Body}", response.StatusCode, url, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetEmployeesAsync");
                return null;
            }
        }

        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
