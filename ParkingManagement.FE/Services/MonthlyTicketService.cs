using System.Net.Http.Json;
using ParkingManagement.FE.Models.MonthlyTicket;

namespace ParkingManagement.FE.Services
{
    public interface IMonthlyTicketService
    {
        Task<List<MonthlyTicketDto>> GetMyMonthlyTicketsAsync();
        Task<MonthlyTicketDto?> GetMonthlyTicketByIdAsync(string id);
        Task<(bool Success, string Message, string? TicketId)> RegisterAsync(RegisterMonthlyTicketRequest request);
        Task<(bool Success, string Message)> RenewAsync(string monthlyTicketId, RenewMonthlyTicketRequest request);
        Task<(bool Success, string Message)> CancelAsync(string monthlyTicketId);
    }

    public class MonthlyTicketService : BaseHttpService, IMonthlyTicketService
    {
        private readonly ILogger<MonthlyTicketService> _logger;

        public MonthlyTicketService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<MonthlyTicketService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<List<MonthlyTicketDto>> GetMyMonthlyTicketsAsync()
        {
            try
            {
                var result = await GetFromJsonAsync<ListMonthlyTicketsResponse>("api/monthly-tickets");
                return result?.Items ?? new List<MonthlyTicketDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly tickets");
                return new List<MonthlyTicketDto>();
            }
        }

        public async Task<MonthlyTicketDto?> GetMonthlyTicketByIdAsync(string id)
        {
            try
            {
                return await GetFromJsonAsync<MonthlyTicketDto>($"api/monthly-tickets/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly ticket: {Id}", id);
                return null;
            }
        }

        public async Task<(bool Success, string Message, string? TicketId)> RegisterAsync(RegisterMonthlyTicketRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monthly-tickets", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RegisterMonthlyTicketResponse>();
                    return (true, "Đăng ký vé tháng thành công!", result?.Data?.MonthlyTicketId);
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Đăng ký thất bại", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering monthly ticket");
                return (false, "Lỗi hệ thống. Vui lòng thử lại.", null);
            }
        }

        public async Task<(bool Success, string Message)> RenewAsync(string monthlyTicketId, RenewMonthlyTicketRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/monthly-tickets/{monthlyTicketId}/renew", request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Gia hạn vé tháng thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Gia hạn thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing monthly ticket: {Id}", monthlyTicketId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string Message)> CancelAsync(string monthlyTicketId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/monthly-tickets/{monthlyTicketId}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Hủy vé tháng thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Hủy vé thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling monthly ticket: {Id}", monthlyTicketId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }
    }
}