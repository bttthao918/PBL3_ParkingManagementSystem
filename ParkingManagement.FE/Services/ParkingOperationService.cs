using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IParkingOperationService
    {
        Task<CheckInValidationResponse?> ValidateCheckInAsync(string vehiclePlate, string vehicleType, string? customerId = null);
        Task<CheckInResultResponse?> ConfirmCheckInAsync(string vehiclePlate, string vehicleType, string slotId, string? customerId = null);
        Task<CheckOutValidationResponse?> ValidateCheckOutAsync(string vehiclePlateOrTicketId);
        Task<CheckOutResultResponse?> ConfirmCheckOutAsync(string ticketId, decimal fee, string paymentMethod = "Tiền mặt");
    }

    public class ParkingOperationService : IParkingOperationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ParkingOperationService> _logger;

        public ParkingOperationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ParkingOperationService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuth()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value
                ?? _httpContextAccessor.HttpContext?.Session.GetString("jwt_token")
                ?? _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token) : null;
        }

        public async Task<CheckInValidationResponse?> ValidateCheckInAsync(string vehiclePlate, string vehicleType, string? customerId = null)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/tickets/checkin/validate", new
                {
                    vehiclePlate,
                    vehicleType,
                    customerId
                });
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<CheckInValidationResponse>();

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("ValidateCheckIn failed: {Status} {Error}", response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateCheckIn error");
                return null;
            }
        }

        public async Task<CheckInResultResponse?> ConfirmCheckInAsync(string vehiclePlate, string vehicleType, string slotId, string? customerId = null)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/tickets/checkin", new
                {
                    vehiclePlate,
                    vehicleType,
                    slotId,
                    customerId
                });
                return await response.Content.ReadFromJsonAsync<CheckInResultResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmCheckIn error");
                return null;
            }
        }

        public async Task<CheckOutValidationResponse?> ValidateCheckOutAsync(string vehiclePlateOrTicketId)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/tickets/checkout/validate", new
                {
                    vehiclePlateOrTicketId
                });
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<CheckOutValidationResponse>();

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("ValidateCheckOut failed: {Status} {Error}", response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateCheckOut error");
                return null;
            }
        }

        public async Task<CheckOutResultResponse?> ConfirmCheckOutAsync(string ticketId, decimal fee, string paymentMethod = "Tiền mặt")
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync($"api/tickets/{Uri.EscapeDataString(ticketId)}/checkout", new
                {
                    ticketId,
                    fee,
                    paymentMethod
                });
                return await response.Content.ReadFromJsonAsync<CheckOutResultResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmCheckOut error");
                return null;
            }
        }
    }
}
