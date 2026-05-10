// Services/PaymentService.cs
using System.Net.Http.Json;
using ParkingManagement.FE.Models.Payment;

namespace ParkingManagement.FE.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse?> PayForTicketAsync(string ticketId, PaymentRequest request);
        Task<PaymentResponse?> PayForMonthlyTicketAsync(string monthlyTicketId, PaymentRequest request);
        Task<List<PaymentResponse>> GetPaymentHistoryAsync();
    }

    public class PaymentService : BaseHttpService, IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<PaymentService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<PaymentResponse?> PayForTicketAsync(string ticketId, PaymentRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PostAsJsonAsync($"api/payments/tickets/{ticketId}", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaymentResponse>();
                }

                _logger.LogWarning("Payment failed for ticket: {TicketId}", ticketId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for ticket: {TicketId}", ticketId);
                return null;
            }
        }

        public async Task<PaymentResponse?> PayForMonthlyTicketAsync(string monthlyTicketId, PaymentRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PostAsJsonAsync($"api/payments/monthly-tickets/{monthlyTicketId}", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaymentResponse>();
                }

                _logger.LogWarning("Payment failed for monthly ticket: {MonthlyTicketId}", monthlyTicketId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for monthly ticket: {MonthlyTicketId}", monthlyTicketId);
                return null;
            }
        }

        public async Task<List<PaymentResponse>> GetPaymentHistoryAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/payments");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PaymentResponse>>() ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history");
                return new();
            }
        }
    }
}