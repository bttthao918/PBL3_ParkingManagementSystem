// Services/TicketService.cs
using System.Net.Http.Json;
using System.Text.Json;

namespace ParkingManagement.FE.Services
{
    public interface ITicketService
    {
        // Check-in / Check-out
        Task<CheckInResponse?> CheckInAsync(CheckInRequest request);
        Task<CheckInValidationResponse?> ValidateCheckInAsync(CheckInValidateRequest request);
        Task<CheckOutResponse?> CheckOutAsync(string ticketId, CheckOutRequest request);
        Task<CheckOutInfoResponse?> GetCheckOutInfoAsync(string vehicleIdentifier);

        // Get tickets
        Task<TicketDto?> GetTicketByIdAsync(string ticketId);
        Task<TicketDetailDto?> GetTicketDetailAsync(string ticketId);
        Task<ListTicketResponseDto?> GetTicketsAsync(TicketFilterDto filter);
        Task<List<TicketDto>> GetMyTicketsAsync();
        Task<List<TicketDto>> GetCustomerTicketsAsync(string customerId);

        // Search
        Task<ListTicketResponseDto?> SearchTicketsAsync(TicketSearchDto search);

        // Statistics
        Task<TicketStatisticsDto?> GetTicketStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<DailyTicketStatsDto?> GetTodayTicketStatsAsync();

        // Payment history for customer
        Task<ListPaymentHistoryDto?> GetPaymentHistoryAsync(string customerId, PaymentHistoryFilterDto filter);

        // Monthly tickets
        Task<MonthlyTicketDto?> GetMonthlyTicketByIdAsync(string monthlyTicketId);
        Task<List<MonthlyTicketDto>> GetCustomerMonthlyTicketsAsync(string customerId);
        Task<RegisterMonthlyTicketResponse?> RegisterMonthlyTicketAsync(RegisterMonthlyTicketRequest request);
        Task<RenewMonthlyTicketResponse?> RenewMonthlyTicketAsync(string monthlyTicketId, RenewMonthlyTicketRequest request);
        Task<bool> CancelMonthlyTicketAsync(string monthlyTicketId);
    }

    public class TicketService : BaseHttpService, ITicketService
    {
        private readonly ILogger<TicketService> _logger;

        public TicketService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<TicketService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        #region Check-in / Check-out

        public async Task<CheckInResponse?> CheckInAsync(CheckInRequest request)
        {
            try
            {
                AttachToken();
                _logger.LogInformation("Check-in request for vehicle: {VehiclePlate}", request.VehiclePlate);

                var response = await _httpClient.PostAsJsonAsync("api/tickets/checkin", request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<CheckInResponse>(responseBody, options);
                }

                _logger.LogWarning("Check-in failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in");
                return null;
            }
        }

        public async Task<CheckInValidationResponse?> ValidateCheckInAsync(CheckInValidateRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PostAsJsonAsync("api/tickets/checkin/validate", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CheckInValidationResponse>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating check-in");
                return null;
            }
        }

        public async Task<CheckOutResponse?> CheckOutAsync(string ticketId, CheckOutRequest request)
        {
            try
            {
                AttachToken();
                _logger.LogInformation("Check-out request for ticket: {TicketId}", ticketId);

                var response = await _httpClient.PostAsJsonAsync($"api/tickets/{ticketId}/checkout", request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<CheckOutResponse>(responseBody, options);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-out for ticket: {TicketId}", ticketId);
                return null;
            }
        }

        public async Task<CheckOutInfoResponse?> GetCheckOutInfoAsync(string vehicleIdentifier)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/tickets/checkout/info?identifier={Uri.EscapeDataString(vehicleIdentifier)}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CheckOutInfoResponse>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting check-out info");
                return null;
            }
        }

        #endregion

        #region Get Tickets

        public async Task<TicketDto?> GetTicketByIdAsync(string ticketId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/tickets/{ticketId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TicketDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket: {TicketId}", ticketId);
                return null;
            }
        }

        public async Task<TicketDetailDto?> GetTicketDetailAsync(string ticketId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/tickets/{ticketId}/detail");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TicketDetailDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket detail: {TicketId}", ticketId);
                return null;
            }
        }

        public async Task<ListTicketResponseDto?> GetTicketsAsync(TicketFilterDto filter)
        {
            try
            {
                AttachToken();
                var queryString = BuildTicketFilterQuery(filter);
                var response = await _httpClient.GetAsync($"api/tickets{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListTicketResponseDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets");
                return null;
            }
        }

        public async Task<List<TicketDto>> GetMyTicketsAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/customers/tickets");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ListCustomerTicketResponse>();
                    return result?.Items?.Select(x => new TicketDto
                    {
                        TicketId = x.TicketId,
                        VehiclePlate = x.VehiclePlate,
                        VehicleType = x.VehicleType,
                        CheckInTime = x.CheckInTime,
                        CheckOutTime = x.CheckOutTime,
                        Fee = x.Fee ?? 0,
                        Status = x.Status,
                        SlotId = x.SlotId
                    }).ToList() ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my tickets");
                return new();
            }
        }

        public async Task<List<TicketDto>> GetCustomerTicketsAsync(string customerId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/customers/{customerId}/tickets");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<TicketDto>>() ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer tickets");
                return new();
            }
        }

        #endregion

        #region Search Tickets

        public async Task<ListTicketResponseDto?> SearchTicketsAsync(TicketSearchDto search)
        {
            try
            {
                AttachToken();
                var queryString = $"api/tickets/search?keyword={Uri.EscapeDataString(search.Keyword)}";
                if (search.PageNumber > 0) queryString += $"&pageNumber={search.PageNumber}";
                if (search.PageSize > 0) queryString += $"&pageSize={search.PageSize}";
                if (!string.IsNullOrEmpty(search.Status)) queryString += $"&status={Uri.EscapeDataString(search.Status)}";

                var response = await _httpClient.GetAsync(queryString);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListTicketResponseDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets");
                return null;
            }
        }

        #endregion

        #region Statistics

        public async Task<TicketStatisticsDto?> GetTicketStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AttachToken();
                var queryString = "";
                if (fromDate.HasValue) queryString += $"?fromDate={fromDate.Value:yyyy-MM-dd}";
                if (toDate.HasValue) queryString += queryString.Contains("?") ? $"&toDate={toDate.Value:yyyy-MM-dd}" : $"?toDate={toDate.Value:yyyy-MM-dd}";

                var response = await _httpClient.GetAsync($"api/tickets/statistics{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TicketStatisticsDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket statistics");
                return null;
            }
        }

        public async Task<DailyTicketStatsDto?> GetTodayTicketStatsAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/tickets/today-stats");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DailyTicketStatsDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today ticket stats");
                return null;
            }
        }

        #endregion

        #region Payment History

        public async Task<ListPaymentHistoryDto?> GetPaymentHistoryAsync(string customerId, PaymentHistoryFilterDto filter)
        {
            try
            {
                AttachToken();
                var queryString = $"api/customers/{customerId}/payments?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
                if (!string.IsNullOrEmpty(filter.Status)) queryString += $"&status={Uri.EscapeDataString(filter.Status)}";
                if (filter.FromDate.HasValue) queryString += $"&fromDate={filter.FromDate.Value:yyyy-MM-dd}";
                if (filter.ToDate.HasValue) queryString += $"&toDate={filter.ToDate.Value:yyyy-MM-dd}";

                var response = await _httpClient.GetAsync(queryString);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListPaymentHistoryDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history");
                return null;
            }
        }

        #endregion

        #region Monthly Tickets

        public async Task<MonthlyTicketDto?> GetMonthlyTicketByIdAsync(string monthlyTicketId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/monthly-tickets/{monthlyTicketId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MonthlyTicketDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly ticket: {MonthlyTicketId}", monthlyTicketId);
                return null;
            }
        }

        public async Task<List<MonthlyTicketDto>> GetCustomerMonthlyTicketsAsync(string customerId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/customers/{customerId}/monthly-tickets");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ListMonthlyTicketsResponse>();
                    return result?.Items ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer monthly tickets");
                return new();
            }
        }

        public async Task<RegisterMonthlyTicketResponse?> RegisterMonthlyTicketAsync(RegisterMonthlyTicketRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PostAsJsonAsync("api/monthly-tickets", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RegisterMonthlyTicketResponse>();
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new RegisterMonthlyTicketResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Đăng ký vé tháng thất bại"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering monthly ticket");
                return new RegisterMonthlyTicketResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống. Vui lòng thử lại."
                };
            }
        }

        public async Task<RenewMonthlyTicketResponse?> RenewMonthlyTicketAsync(string monthlyTicketId, RenewMonthlyTicketRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PostAsJsonAsync($"api/monthly-tickets/{monthlyTicketId}/renew", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RenewMonthlyTicketResponse>();
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new RenewMonthlyTicketResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Gia hạn vé tháng thất bại"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing monthly ticket");
                return new RenewMonthlyTicketResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống. Vui lòng thử lại."
                };
            }
        }

        public async Task<bool> CancelMonthlyTicketAsync(string monthlyTicketId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.DeleteAsync($"api/monthly-tickets/{monthlyTicketId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling monthly ticket");
                return false;
            }
        }

        #endregion

        #region Private Helpers

        private string BuildTicketFilterQuery(TicketFilterDto filter)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(filter.Status))
                queryParams.Add($"status={Uri.EscapeDataString(filter.Status)}");
            if (!string.IsNullOrEmpty(filter.VehicleType))
                queryParams.Add($"vehicleType={Uri.EscapeDataString(filter.VehicleType)}");
            if (!string.IsNullOrEmpty(filter.SearchKeyword))
                queryParams.Add($"search={Uri.EscapeDataString(filter.SearchKeyword)}");
            if (filter.FromDate.HasValue)
                queryParams.Add($"fromDate={filter.FromDate.Value:yyyy-MM-dd}");
            if (filter.ToDate.HasValue)
                queryParams.Add($"toDate={filter.ToDate.Value:yyyy-MM-dd}");

            queryParams.Add($"pageNumber={filter.PageNumber}");
            queryParams.Add($"pageSize={filter.PageSize}");

            return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        }

        #endregion
    }

    #region Request/Response DTOs for Ticket

    // Check-in / Check-out DTOs
    public class CheckInRequest
    {
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string? SlotId { get; set; }
        public string? CustomerId { get; set; }
    }

    public class CheckInResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public string SlotId { get; set; } = "";
    }

    public class CheckInValidateRequest
    {
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
    }

    public class CheckInValidationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? CustomerName { get; set; }
        public bool HasMonthlyTicket { get; set; }
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
    }

    public class CheckOutRequest
    {
        public string PaymentMethod { get; set; } = "Tiền mặt";
        public decimal? ReceivedAmount { get; set; }
    }

    public class CheckOutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string TicketId { get; set; } = "";
        public decimal Fee { get; set; }
        public DateTime CheckOutTime { get; set; }
        public decimal? Change { get; set; }
        public string PaymentId { get; set; } = "";
    }

    public class CheckOutInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Fee { get; set; }
        public bool HasMonthlyTicket { get; set; }
        public string? SlotId { get; set; }
    }

    // Ticket DTOs
    public class TicketDto
    {
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string? SlotId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; } = "";
    }

    public class TicketDetailDto : TicketDto
    {
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public int? DurationMinutes { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentTime { get; set; }
        public string? MonthlyTicketId { get; set; }
        public bool HasActiveMonthlyTicket { get; set; }
    }

    public class TicketFilterDto
    {
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchKeyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TicketSearchDto
    {
        public string Keyword { get; set; } = "";
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ListTicketResponseDto
    {
        public List<TicketListItemDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class TicketListItemDto
    {
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "";
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
        public string? CustomerName { get; set; }
    }

    public class ListCustomerTicketResponse
    {
        public List<CustomerTicketItemDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class CustomerTicketItemDto
    {
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "";
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
    }

    // Payment History DTOs
    public class PaymentHistoryFilterDto
    {
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ListPaymentHistoryDto
    {
        public List<PaymentHistoryItemDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class PaymentHistoryItemDto
    {
        public string PaymentId { get; set; } = "";
        public string TicketId { get; set; } = "";
        public string? VehiclePlate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // Statistics DTOs
    public class TicketStatisticsDto
    {
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int CompletedTickets { get; set; }
        public int CancelledTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerTicket { get; set; }
        public Dictionary<string, int> TicketsByVehicleType { get; set; } = new();
        public List<DailyTicketStatsDto> DailyStats { get; set; } = new();
    }

    public class DailyTicketStatsDto
    {
        public DateTime Date { get; set; }
        public int TicketCount { get; set; }
        public decimal Revenue { get; set; }
        public int ActiveCount { get; set; }
    }

    // Monthly Ticket DTOs
    public class MonthlyTicketDto
    {
        public string MonthlyTicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string PackageType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = "";
        public int DaysRemaining { get; set; }
    }

    public class ListMonthlyTicketsResponse
    {
        public List<MonthlyTicketDto> Items { get; set; } = new();
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
    }

    public class RegisterMonthlyTicketRequest
    {
        public string CustomerId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string PackageType { get; set; } = "1 tháng";
        public string? PaymentMethod { get; set; }
    }

    public class RegisterMonthlyTicketResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public decimal Fee { get; set; }
        public MonthlyTicketDto? Data { get; set; }
    }

    public class RenewMonthlyTicketRequest
    {
        public string PackageType { get; set; } = "1 tháng";
    }

    public class RenewMonthlyTicketResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public decimal AdditionalFee { get; set; }
        public MonthlyTicketDto? Data { get; set; }
    }

    #endregion
}