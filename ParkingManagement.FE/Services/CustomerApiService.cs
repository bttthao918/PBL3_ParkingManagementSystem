using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface ICustomerApiService
    {
        Task<CustomerProfileDto?> GetProfileAsync();
        Task<ListCustomerReservationDto?> GetReservationsAsync(int pageNumber = 1, int pageSize = 5);
        Task<ListCustomerTicketDto?> GetTicketsAsync(int pageNumber = 1, int pageSize = 5, string? status = null);
        Task<ListCustomerMonthlyTicketDto?> GetMonthlyTicketsAsync();
        Task<MonthlyTicketPricingDto?> GetMonthlyTicketPricingAsync();
        Task<ApiActionResult<RegisterMonthlyTicketResponseDto>> RegisterMonthlyTicketAsync(RegisterMonthlyTicketRequestDto request);
        Task<ApiActionResult<RegisterMonthlyTicketResponseDto>> CreateMonthlyTicketPaymentLinkAsync(string monthlyTicketId);
        Task<ApiActionResult<RenewMonthlyTicketResponseDto>> RenewMonthlyTicketAsync(string monthlyTicketId, RenewMonthlyTicketRequestDto request);
        Task<ApiActionResult<BasicApiResponseDto>> CancelMonthlyTicketAsync(string monthlyTicketId);
        Task<ApiActionResult<BasicApiResponseDto>> ConfirmPayOsReturnAsync(long orderCode);
        Task<ApiActionResult<BasicApiResponseDto>> ConfirmMonthlyTicketPaymentAsync(string monthlyTicketId);
        Task<ListCustomerPaymentDto?> GetPaymentHistoryAsync(int pageNumber = 1, int pageSize = 50);
        Task<List<AvailableSlotDto>?> GetAvailableSlotsAsync(string? vehicleType = null, bool includeUnavailable = false);
        Task<CreateVnPayPaymentResponseDto?> CreateVnPayPaymentAsync(CreateVnPayPaymentRequestDto request);
        Task<ListEmployeeCustomerSearchDto?> SearchForEmployeeAsync(EmployeeCustomerSearchFilterDto filter);
        Task<EmployeeCustomerDetailDto?> GetEmployeeCustomerDetailAsync(string customerId);
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
            => GetAsync<ListCustomerMonthlyTicketDto>("api/monthly-tickets");

        public Task<MonthlyTicketPricingDto?> GetMonthlyTicketPricingAsync()
            => GetAsync<MonthlyTicketPricingDto>("api/monthly-tickets/pricing");

        public Task<ApiActionResult<RegisterMonthlyTicketResponseDto>> RegisterMonthlyTicketAsync(RegisterMonthlyTicketRequestDto request)
            => SendAsync<RegisterMonthlyTicketResponseDto>(() => _httpClient.PostAsJsonAsync("api/monthly-tickets", request), "api/monthly-tickets");

        public Task<ApiActionResult<RegisterMonthlyTicketResponseDto>> CreateMonthlyTicketPaymentLinkAsync(string monthlyTicketId)
            => SendAsync<RegisterMonthlyTicketResponseDto>(
                () => _httpClient.PostAsync($"api/monthly-tickets/{Uri.EscapeDataString(monthlyTicketId)}/payment-link", null),
                $"api/monthly-tickets/{monthlyTicketId}/payment-link");

        public Task<ApiActionResult<RenewMonthlyTicketResponseDto>> RenewMonthlyTicketAsync(string monthlyTicketId, RenewMonthlyTicketRequestDto request)
            => SendAsync<RenewMonthlyTicketResponseDto>(
                () => _httpClient.PostAsJsonAsync($"api/monthly-tickets/{Uri.EscapeDataString(monthlyTicketId)}/renew", request),
                $"api/monthly-tickets/{monthlyTicketId}/renew");

        public Task<ApiActionResult<BasicApiResponseDto>> CancelMonthlyTicketAsync(string monthlyTicketId)
            => SendAsync<BasicApiResponseDto>(
                () => _httpClient.DeleteAsync($"api/monthly-tickets/{Uri.EscapeDataString(monthlyTicketId)}"),
                $"api/monthly-tickets/{monthlyTicketId}");

        public Task<ApiActionResult<BasicApiResponseDto>> ConfirmPayOsReturnAsync(long orderCode)
            => SendAsync<BasicApiResponseDto>(
                () => _httpClient.PostAsync($"api/payments/payos/confirm-return/{orderCode}", null),
                $"api/payments/payos/confirm-return/{orderCode}");

        public Task<ApiActionResult<BasicApiResponseDto>> ConfirmMonthlyTicketPaymentAsync(string monthlyTicketId)
            => SendAsync<BasicApiResponseDto>(
                () => _httpClient.PostAsync($"api/payments/payos/confirm-monthly-ticket/{Uri.EscapeDataString(monthlyTicketId)}", null),
                $"api/payments/payos/confirm-monthly-ticket/{monthlyTicketId}");

        public Task<ListCustomerPaymentDto?> GetPaymentHistoryAsync(int pageNumber = 1, int pageSize = 50)
            => GetAsync<ListCustomerPaymentDto>($"api/customers/payment-history?pageNumber={pageNumber}&pageSize={pageSize}");

        public Task<ListEmployeeCustomerSearchDto?> SearchForEmployeeAsync(EmployeeCustomerSearchFilterDto filter)
        {
            var query = new List<string>
            {
                $"PageNumber={filter.PageNumber}",
                $"PageSize={filter.PageSize}"
            };

            AddQuery(query, "SearchKeyword", filter.SearchKeyword);
            AddQuery(query, "StatusFilter", filter.StatusFilter);
            AddQuery(query, "VehicleType", filter.VehicleType);
            AddQuery(query, "VipLevel", filter.VipLevel);
            if (filter.RegisterDate.HasValue)
            {
                AddQuery(query, "RegisterDate", filter.RegisterDate.Value.ToString("yyyy-MM-dd"));
            }

            return GetAsync<ListEmployeeCustomerSearchDto>($"api/customers/employee/search?{string.Join("&", query)}");
        }

        public Task<EmployeeCustomerDetailDto?> GetEmployeeCustomerDetailAsync(string customerId)
            => GetAsync<EmployeeCustomerDetailDto>($"api/customers/employee/{Uri.EscapeDataString(customerId)}/detail");

        public Task<List<AvailableSlotDto>?> GetAvailableSlotsAsync(string? vehicleType = null, bool includeUnavailable = false)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(vehicleType))
            {
                query.Add($"vehicleType={Uri.EscapeDataString(vehicleType)}");
            }

            if (includeUnavailable)
            {
                query.Add("includeUnavailable=true");
            }

            var url = "api/reservations/available-slots";
            if (query.Count > 0)
            {
                url += $"?{string.Join("&", query)}";
            }

            return GetAsync<List<AvailableSlotDto>>(url);
        }

        public async Task<CreateVnPayPaymentResponseDto?> CreateVnPayPaymentAsync(CreateVnPayPaymentRequestDto request)
        {
            AttachBearerToken();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/vnpay/create-payment", request);
                return await response.Content.ReadFromJsonAsync<CreateVnPayPaymentResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateVnPayPaymentAsync failed");
                return null;
            }
        }

        private async Task<T?> GetAsync<T>(string url)
        {
            AttachBearerToken();

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Customer API call failed: {StatusCode} {Url} {Body}", response.StatusCode, url, errorContent);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Customer API call exception: {Url}", url);
                return default;
            }
        }

        private async Task<ApiActionResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> send, string url)
        {
            AttachBearerToken();

            var response = await send();
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = string.IsNullOrWhiteSpace(body)
                    ? default
                    : JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return new ApiActionResult<T>
                {
                    Success = true,
                    Message = ExtractMessage(body) ?? "Thao tác thành công.",
                    Data = data
                };
            }

            var message = ExtractMessage(body) ?? $"BE trả về lỗi {(int)response.StatusCode}.";
            _logger.LogWarning("Customer API mutation failed: {StatusCode} {Url} {Body}", response.StatusCode, url, body);

            return new ApiActionResult<T>
            {
                Success = false,
                Message = message
            };
        }

        private static string? ExtractMessage(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString();
                }

                if (root.TryGetProperty("Message", out message) && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString();
                }

                if (TryGetValidationMessage(root, out var validationMessage))
                {
                    return validationMessage;
                }
            }
            catch (JsonException)
            {
                return body;
            }

            return null;
        }

        private static bool TryGetValidationMessage(JsonElement element, out string message)
        {
            message = string.Empty;

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("errors", out var errors) &&
                    TryGetValidationMessage(errors, out message))
                {
                    return true;
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in property.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String &&
                                !string.IsNullOrWhiteSpace(item.GetString()))
                            {
                                message = $"{property.Name}: {item.GetString()}";
                                return true;
                            }
                        }
                    }

                    if (property.Value.ValueKind == JsonValueKind.Object &&
                        TryGetValidationMessage(property.Value, out var nestedMessage))
                    {
                        message = nestedMessage;
                        return true;
                    }
                }
            }

            return false;
        }

        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            }

            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        private static void AddQuery(List<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }
    }
}
