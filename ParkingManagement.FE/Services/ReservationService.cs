// Services/ReservationService.cs
using System.Net.Http.Json;
using System.Text.Json;
using ParkingManagement.FE.Models.Parking;

namespace ParkingManagement.FE.Services
{
    public interface IReservationService
    {
        // Get reservations
        Task<ListReservationDto?> GetMyReservationsAsync(FilterReservationRequest filter);
        Task<ReservationDetailDto?> GetReservationByIdAsync(string id);
        Task<List<ReservationDto>> GetAllReservationsAsync(string? customerId = null);

        // Create reservation
        Task<(bool Success, string Message, string? ReservationId)> CreateReservationAsync(CreateReservationRequest request);

        // Cancel reservation
        Task<(bool Success, string Message)> CancelReservationAsync(string reservationId);

        // Update reservation
        Task<(bool Success, string Message)> UpdateReservationStatusAsync(string reservationId, string status);

        // Check-in using reservation
        Task<(bool Success, string Message, string? TicketId)> CheckInFromReservationAsync(string reservationId);

        // Statistics
        Task<ReservationStatisticsDto?> GetReservationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class ReservationService : BaseHttpService, IReservationService
    {
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ReservationService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        #region Get Reservations

        public async Task<ListReservationDto?> GetMyReservationsAsync(FilterReservationRequest filter)
        {
            try
            {
                AttachToken();
                var url = $"api/reservations?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    url += $"&status={Uri.EscapeDataString(filter.Status)}";
                }
                if (!string.IsNullOrEmpty(filter.VehiclePlate))
                {
                    url += $"&vehiclePlate={Uri.EscapeDataString(filter.VehiclePlate)}";
                }
                if (filter.FromDate.HasValue)
                {
                    url += $"&fromDate={filter.FromDate.Value:yyyy-MM-dd}";
                }
                if (filter.ToDate.HasValue)
                {
                    url += $"&toDate={filter.ToDate.Value:yyyy-MM-dd}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListReservationDto>();
                }

                _logger.LogWarning("GetMyReservationsAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my reservations");
                return null;
            }
        }

        public async Task<ReservationDetailDto?> GetReservationByIdAsync(string id)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/reservations/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ReservationDetailDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation by id: {Id}", id);
                return null;
            }
        }

        public async Task<List<ReservationDto>> GetAllReservationsAsync(string? customerId = null)
        {
            try
            {
                AttachToken();
                var url = "api/reservations/all";
                if (!string.IsNullOrEmpty(customerId))
                {
                    url += $"?customerId={customerId}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ReservationDto>>() ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reservations");
                return new();
            }
        }

        #endregion

        #region Create Reservation

        public async Task<(bool Success, string Message, string? ReservationId)> CreateReservationAsync(CreateReservationRequest request)
        {
            try
            {
                AttachToken();
                _logger.LogInformation("Creating reservation for customer {CustomerId}, vehicle {VehiclePlate}",
                    request.CustomerId, request.VehiclePlate);

                var response = await _httpClient.PostAsJsonAsync("api/reservations", request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var result = JsonSerializer.Deserialize<ReservationDetailDto>(responseBody, options);
                    return (true, "Đặt chỗ thành công!", result?.ReservationId);
                }

                var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return (false, error?.Message ?? "Đặt chỗ thất bại", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation for customer {CustomerId}", request.CustomerId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.", null);
            }
        }

        #endregion

        #region Cancel Reservation

        public async Task<(bool Success, string Message)> CancelReservationAsync(string reservationId)
        {
            try
            {
                AttachToken();
                _logger.LogInformation("Cancelling reservation: {ReservationId}", reservationId);

                var response = await _httpClient.DeleteAsync($"api/reservations/{reservationId}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Hủy đặt chỗ thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Hủy đặt chỗ thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling reservation: {ReservationId}", reservationId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        #endregion

        #region Update Reservation Status

        public async Task<(bool Success, string Message)> UpdateReservationStatusAsync(string reservationId, string status)
        {
            try
            {
                AttachToken();
                var request = new UpdateReservationStatusRequest
                {
                    ReservationId = reservationId,
                    Status = status
                };

                var response = await _httpClient.PatchAsJsonAsync($"api/reservations/{reservationId}/status", request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, $"Cập nhật trạng thái đặt chỗ thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Cập nhật trạng thái thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation status: {ReservationId}", reservationId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        #endregion

        #region Check-in from Reservation

        public async Task<(bool Success, string Message, string? TicketId)> CheckInFromReservationAsync(string reservationId)
        {
            try
            {
                AttachToken();
                _logger.LogInformation("Checking in from reservation: {ReservationId}", reservationId);

                var response = await _httpClient.PostAsync($"api/reservations/{reservationId}/checkin", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CheckInFromReservationResponse>();
                    return (true, "Check-in thành công!", result?.TicketId);
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Check-in thất bại", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in from reservation: {ReservationId}", reservationId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.", null);
            }
        }

        #endregion

        #region Statistics

        public async Task<ReservationStatisticsDto?> GetReservationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AttachToken();
                var queryParams = new List<string>();

                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

                var url = "api/reservations/statistics";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ReservationStatisticsDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation statistics");
                return null;
            }
        }

        #endregion
    }

    #region Request/Response DTOs for Reservation

    public class FilterReservationRequest
    {
        public string? Status { get; set; }
        public string? VehiclePlate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ListReservationDto
    {
        public List<ReservationDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class ReservationDto
    {
        public string ReservationId { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public string? CustomerName { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleType { get; set; }
        public string? SlotId { get; set; }
        public string? SlotLocation { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }

    public class ReservationDetailDto : ReservationDto
    {
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public int TimeUntilExpectedMinutes { get; set; }
        public bool IsExpired { get; set; }
    }

    public class CreateReservationRequest
    {
        public string CustomerId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string? VehicleType { get; set; }
        public string? PreferredSlotId { get; set; }
        public DateTime ExpectedTime { get; set; }
    }

    public class UpdateReservationStatusRequest
    {
        public string ReservationId { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class CheckInFromReservationResponse
    {
        public bool Success { get; set; }
        public string? TicketId { get; set; }
        public string? Message { get; set; }
    }

    public class ReservationStatisticsDto
    {
        public int TotalReservations { get; set; }
        public int PendingReservations { get; set; }
        public int CompletedReservations { get; set; }
        public int CancelledReservations { get; set; }
        public int ExpiredReservations { get; set; }
        public double CompletionRate { get; set; }
        public Dictionary<string, int> ReservationsByVehicleType { get; set; } = new();
        public List<DailyReservationStatsDto> DailyStats { get; set; } = new();
    }

    public class DailyReservationStatsDto
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    #endregion
}