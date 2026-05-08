using ParkingManagement.FE.Models;
using System.Net.Http.Json;

namespace ParkingManagement.FE.Services
{
    public interface ITicketService
    {
        Task<ListEmployeeTicketDto?> SearchTicketsAsync(EmployeeTicketSearchDto search);
    }

    public class TicketService : ITicketService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TicketService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TicketService(HttpClient httpClient, ILogger<TicketService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ListEmployeeTicketDto?> SearchTicketsAsync(EmployeeTicketSearchDto search)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
            }

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Construct query parameters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search.SearchKeyword)) queryParams.Add($"SearchKeyword={search.SearchKeyword}");
            if (!string.IsNullOrEmpty(search.Status)) queryParams.Add($"Status={search.Status}");
            if (!string.IsNullOrEmpty(search.VehicleType)) queryParams.Add($"VehicleType={search.VehicleType}");
            queryParams.Add($"PageNumber={search.PageNumber}");
            queryParams.Add($"PageSize={search.PageSize}");

            var queryString = string.Join("&", queryParams);
            var url = $"api/tickets?{queryString}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ListEmployeeTicketDto>();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Call Failed: {response.StatusCode} - {errorContent} - Token: {!string.IsNullOrEmpty(token)} - URL: {url}");
        }
    }
}
