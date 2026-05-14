using ParkingManagement.FE.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ParkingManagement.FE.Services
{
    public interface ITicketService
    {
        Task<ListEmployeeTicketDto?> SearchTicketsAsync(EmployeeTicketSearchDto search);
        Task<TicketSummaryDto?> GetTicketSummaryAsync();
        Task<TicketDetailDto?> GetTicketDetailAsync(string ticketId);
        Task<CreateTicketResultDto?> CreateTicketAsync(CreateTicketRequestDto input);
        Task<bool> UpdateTicketAsync(string ticketId, UpdateTicketRequestDto input);
        Task<bool> DeleteTicketAsync(string ticketId);
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
            if (search.FromDate.HasValue) queryParams.Add($"FromDate={Uri.EscapeDataString(search.FromDate.Value.ToString("yyyy-MM-dd"))}");
            if (search.ToDate.HasValue) queryParams.Add($"ToDate={Uri.EscapeDataString(search.ToDate.Value.ToString("yyyy-MM-dd"))}");
            queryParams.Add($"PageNumber={search.PageNumber}");
            queryParams.Add($"PageSize={search.PageSize}");

            var queryString = string.Join("&", queryParams);
            var url = $"api/tickets?{queryString}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ListEmployeeTicketDto>();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập API đã hết hạn.");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Call Failed: {response.StatusCode} - {errorContent} - Token: {!string.IsNullOrEmpty(token)} - URL: {url}");
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

        public async Task<TicketDetailDto?> GetTicketDetailAsync(string ticketId)
        {
            ApplyAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/tickets/{Uri.EscapeDataString(ticketId)}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TicketDetailDto>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập API đã hết hạn.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Call Failed: {response.StatusCode} - {errorContent}");
        }

        public async Task<CreateTicketResultDto?> CreateTicketAsync(CreateTicketRequestDto input)
        {
            ApplyAuthorizationHeader(requireToken: false);

            var response = await _httpClient.PostAsJsonAsync("api/tickets", input);
            var responseBody = await response.Content.ReadAsStringAsync();
            CreateTicketResultDto? result = null;

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try
                {
                    result = JsonSerializer.Deserialize<CreateTicketResultDto>(
                        responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "CreateTicketAsync could not parse response body.");
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return result ?? new CreateTicketResultDto { Success = true };
            }

            _logger.LogWarning("CreateTicketAsync failed: {StatusCode} {Body}", response.StatusCode, responseBody);

            return result ?? new CreateTicketResultDto
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(responseBody)
                    ? "Không thể tạo vé mới."
                    : $"Không thể tạo vé mới. Chi tiết: {responseBody}"
            };
        }

        public async Task<bool> UpdateTicketAsync(string ticketId, UpdateTicketRequestDto input)
        {
            ApplyAuthorizationHeader(requireToken: false);

            var response = await _httpClient.PutAsJsonAsync($"api/tickets/{Uri.EscapeDataString(ticketId)}", input);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("UpdateTicketAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
            return false;
        }

        public async Task<bool> DeleteTicketAsync(string ticketId)
        {
            ApplyAuthorizationHeader(requireToken: false);

            var response = await _httpClient.DeleteAsync($"api/tickets/{Uri.EscapeDataString(ticketId)}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("DeleteTicketAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
            return false;
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
