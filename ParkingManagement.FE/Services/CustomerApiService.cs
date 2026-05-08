using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface ICustomerApiService
    {
        Task<CustomerProfileDto?> GetProfileAsync();
        Task<ListCustomerReservationDto?> GetReservationsAsync(int pageNumber = 1, int pageSize = 5);
        Task<ListCustomerTicketDto?> GetTicketsAsync(int pageNumber = 1, int pageSize = 5, string? status = null);
        Task<ListCustomerMonthlyTicketDto?> GetMonthlyTicketsAsync();
        Task<ListCustomerPaymentDto?> GetPaymentHistoryAsync(int pageNumber = 1, int pageSize = 50);
        Task<ListEmployeeCustomerSearchDto?> SearchForEmployeeAsync(EmployeeCustomerSearchFilterDto filter);
    }

    public class CustomerApiService : ICustomerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CustomerApiService> _logger;

        public CustomerApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CustomerApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Task<CustomerProfileDto?> GetProfileAsync()
            => GetAsync<CustomerProfileDto>("api/customers/me");

        public Task<ListCustomerReservationDto?> GetReservationsAsync(int pageNumber = 1, int pageSize = 5)
            => GetAsync<ListCustomerReservationDto>($"api/customers/reservations?pageNumber={pageNumber}&pageSize={pageSize}");

        public Task<ListCustomerTicketDto?> GetTicketsAsync(int pageNumber = 1, int pageSize = 5, string? status = null)
        {
            var url = $"api/customers/tickets?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += $"&status={Uri.EscapeDataString(status)}";
            }

            return GetAsync<ListCustomerTicketDto>(url);
        }

        public Task<ListCustomerMonthlyTicketDto?> GetMonthlyTicketsAsync()
            => GetAsync<ListCustomerMonthlyTicketDto>("api/customers/monthly-tickets");

        public Task<ListCustomerPaymentDto?> GetPaymentHistoryAsync(int pageNumber = 1, int pageSize = 50)
            => GetAsync<ListCustomerPaymentDto>($"api/customers/payment-history?pageNumber={pageNumber}&pageSize={pageSize}");

        public Task<ListEmployeeCustomerSearchDto?> SearchForEmployeeAsync(EmployeeCustomerSearchFilterDto filter)
        {
            var url = $"api/customers/employee/search?PageNumber={filter.PageNumber}&PageSize={filter.PageSize}";
            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                url += $"&SearchKeyword={Uri.EscapeDataString(filter.SearchKeyword)}";
            }
            return GetAsync<ListEmployeeCustomerSearchDto>(url);
        }

        private async Task<T?> GetAsync<T>(string url)
        {
            AttachBearerToken();

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Customer API call failed: {StatusCode} {Url} {Body}", response.StatusCode, url, errorContent);
            return default;
        }

        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
            }

            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
