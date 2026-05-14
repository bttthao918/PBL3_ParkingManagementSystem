using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IPaymentService
    {
        Task<ListPaymentDto?> GetHistoryAsync(int pageNumber = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null);
        Task<PaymentDetailDto?> GetByIdAsync(string paymentId);
        Task<PaymentResponseDto?> PayForTicketAsync(string ticketId, ProcessPaymentDto dto);
        Task<PaymentResponseDto?> PayForMonthlyTicketAsync(string monthlyTicketId, ProcessPaymentDto dto);
        Task<PaymentSummaryDto?> GetSummaryAsync(DateTime from, DateTime to);
    }

    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PaymentService> logger)
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

        public async Task<ListPaymentDto?> GetHistoryAsync(int pageNumber = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AddAuthorizationHeader();
                var url = $"api/payments?pageNumber={pageNumber}&pageSize={pageSize}";
                if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
                if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListPaymentDto>();
                }
                _logger.LogWarning("GetHistoryAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetHistoryAsync");
                return null;
            }
        }

        public async Task<PaymentDetailDto?> GetByIdAsync(string paymentId)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/payments/{Uri.EscapeDataString(paymentId)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaymentDetailDto>();
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

        public async Task<PaymentResponseDto?> PayForTicketAsync(string ticketId, ProcessPaymentDto dto)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync($"api/payments/tickets/{Uri.EscapeDataString(ticketId)}", dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
                }
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("PayForTicketAsync failed: {StatusCode} {Error}", response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PayForTicketAsync");
                return null;
            }
        }

        public async Task<PaymentResponseDto?> PayForMonthlyTicketAsync(string monthlyTicketId, ProcessPaymentDto dto)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync($"api/payments/monthly-tickets/{Uri.EscapeDataString(monthlyTicketId)}", dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
                }
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("PayForMonthlyTicketAsync failed: {StatusCode} {Error}", response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PayForMonthlyTicketAsync");
                return null;
            }
        }

        public async Task<PaymentSummaryDto?> GetSummaryAsync(DateTime from, DateTime to)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/payments/summary?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaymentSummaryDto>();
                }
                _logger.LogWarning("GetSummaryAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetSummaryAsync");
                return null;
            }
        }
    }
}
