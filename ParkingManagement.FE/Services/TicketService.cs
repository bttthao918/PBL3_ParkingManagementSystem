using ParkingManagement.FE.Models;
using System.Net.Http.Json;

namespace ParkingManagement.FE.Services
{
    public interface ITicketService
    {
        Task<ListEmployeeTicketDto?> SearchTicketsAsync(EmployeeTicketSearchDto search);
        Task<bool> CheckOutAsync(string ticketId, decimal fee, string paymentMethod = "Tiền mặt");
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
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            }

            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
                : null;

            // Construct query parameters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search.SearchKeyword)) queryParams.Add($"SearchKeyword={Uri.EscapeDataString(search.SearchKeyword)}");
            if (!string.IsNullOrEmpty(search.Status)) queryParams.Add($"Status={Uri.EscapeDataString(search.Status)}");
            if (!string.IsNullOrEmpty(search.VehicleType)) queryParams.Add($"VehicleType={Uri.EscapeDataString(search.VehicleType)}");
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

        public async Task<bool> CheckOutAsync(string ticketId, decimal fee, string paymentMethod = "Tiền mặt")
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value
                ?? _httpContextAccessor.HttpContext?.Session.GetString("jwt_token")
                ?? _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
                : null;

            var response = await _httpClient.PostAsJsonAsync($"api/tickets/{Uri.EscapeDataString(ticketId)}/checkout", new
            {
                ticketId,
                fee,
                paymentMethod
            });

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("CheckOutAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
            return false;
        }
    }
}
