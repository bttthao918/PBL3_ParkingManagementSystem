using System.Net.Http.Headers;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IParkingSlotService
    {
        Task<ListEmployeeSlotDto?> GetEmployeeSlotsAsync(EmployeeSlotFilterDto filter);
    }

    public class ParkingSlotService : IParkingSlotService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ParkingSlotService> _logger;

        public ParkingSlotService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ParkingSlotService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuthorizationHeader()
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

            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        public async Task<ListEmployeeSlotDto?> GetEmployeeSlotsAsync(EmployeeSlotFilterDto filter)
        {
            try
            {
                AddAuthorizationHeader();
                var url = "api/parking-slots/employee/list";
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.VehicleType)) queryParams.Add($"VehicleType={Uri.EscapeDataString(filter.VehicleType)}");
                if (!string.IsNullOrEmpty(filter.Status)) queryParams.Add($"Status={Uri.EscapeDataString(filter.Status)}");
                if (!string.IsNullOrEmpty(filter.Location)) queryParams.Add($"Location={Uri.EscapeDataString(filter.Location)}");
                queryParams.Add($"PageNumber={filter.PageNumber}");
                queryParams.Add($"PageSize={filter.PageSize}");

                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListEmployeeSlotDto>();
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"GetEmployeeSlotsAsync failed: {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetEmployeeSlotsAsync");
                return null;
            }
        }
    }
}
