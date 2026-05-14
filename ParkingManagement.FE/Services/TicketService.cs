using ParkingManagement.FE.Models;
using System.Net;
using System.Net.Http.Json;

namespace ParkingManagement.FE.Services
{
    public interface ITicketService
    {
        Task<ListEmployeeTicketDto?> SearchTicketsAsync(EmployeeTicketSearchDto search);
        Task<TicketSummaryDto?> GetTicketSummaryAsync();
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
            var token = ApplyAuthorizationHeader(requireToken: false);

            // Construct query parameters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search.SearchKeyword)) queryParams.Add($"SearchKeyword={Uri.EscapeDataString(search.SearchKeyword)}");
            if (!string.IsNullOrEmpty(search.Status)) queryParams.Add($"Status={Uri.EscapeDataString(search.Status)}");
            if (!string.IsNullOrEmpty(search.VehicleType)) queryParams.Add($"VehicleType={Uri.EscapeDataString(search.VehicleType)}");
            if (!string.IsNullOrEmpty(search.AreaFilter)) queryParams.Add($"AreaFilter={Uri.EscapeDataString(search.AreaFilter)}");
            if (search.FromDate.HasValue) queryParams.Add($"FromDate={Uri.EscapeDataString(search.FromDate.Value.ToString("yyyy-MM-dd"))}");
            if (search.ToDate.HasValue) queryParams.Add($"ToDate={Uri.EscapeDataString(search.ToDate.Value.ToString("yyyy-MM-dd"))}");
            queryParams.Add($"PageNumber={search.PageNumber}");
            queryParams.Add($"PageSize={search.PageSize}");

            var queryString = string.Join("&", queryParams);
            // Sử dụng api/tickets (endpoint chung) - BE trả về ListTicketDto có cùng cấu trúc với ListEmployeeTicketDto
            var url = $"api/tickets?{queryString}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                // BE trả về ListTicketDto nhưng có cùng cấu trúc với ListEmployeeTicketDto
                // Deserialize trực tiếp vì JSON property names giống nhau
                return await response.Content.ReadFromJsonAsync<ListEmployeeTicketDto>();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập API đã hết hạn.");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("SearchTicketsAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
            return null;
        }

        public async Task<TicketSummaryDto?> GetTicketSummaryAsync()
        {
            ApplyAuthorizationHeader(requireToken: false);

            var response = await _httpClient.GetAsync("api/tickets/summary");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TicketSummaryDto>();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập API đã hết hạn.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("GetTicketSummaryAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
            return null;
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

        private string? ApplyAuthorizationHeader(bool requireToken = true)
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
                    throw new UnauthorizedAccessException("Không tìm thấy JWT để gọi Backend API.");
                }

                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return token;
        }
    }
}
