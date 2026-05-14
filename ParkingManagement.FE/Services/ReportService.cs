using System.Net.Http.Headers;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IReportService
    {
        Task<EmployeeDashboardDto?> GetEmployeeDashboardAsync(string employeeId);
        Task<ShiftAttendanceReportDto?> GetShiftAttendanceReportAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<EmployeeRevenueReportDto?> GetEmployeeRevenueReportAsync(string employeeId, string period = "month");
        Task<DashboardSummaryDto?> GetManagerDashboardAsync();
        Task<RevenueReportDto?> GetManagerRevenueReportAsync(RevenueReportFilterDto filter);
        Task<CustomerReportDto?> GetManagerCustomerReportAsync();
    }

    public class ReportService : IReportService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReportService> _logger;

        public ReportService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ReportService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuthorizationHeader()
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

            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        public async Task<EmployeeDashboardDto?> GetEmployeeDashboardAsync(string employeeId)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/reports/employee/{employeeId}/dashboard");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeeDashboardDto>();
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"GetEmployeeDashboardAsync failed: {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetEmployeeDashboardAsync");
                return null;
            }
        }

        public async Task<ShiftAttendanceReportDto?> GetShiftAttendanceReportAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AddAuthorizationHeader();
                var url = $"api/reports/employee/{employeeId}/attendance";
                var queryParams = new List<string>();
                if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ShiftAttendanceReportDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetShiftAttendanceReportAsync");
                return null;
            }
        }

        public async Task<EmployeeRevenueReportDto?> GetEmployeeRevenueReportAsync(string employeeId, string period = "month")
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/reports/employee/{employeeId}/revenue?period={period}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeeRevenueReportDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetEmployeeRevenueReportAsync");
                return null;
            }
        }

        public async Task<DashboardSummaryDto?> GetManagerDashboardAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/reports/manager/dashboard");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetManagerDashboardAsync");
                return null;
            }
        }

        public async Task<RevenueReportDto?> GetManagerRevenueReportAsync(RevenueReportFilterDto filter)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("api/reports/manager/revenue", filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RevenueReportDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetManagerRevenueReportAsync");
                return null;
            }
        }

        public async Task<CustomerReportDto?> GetManagerCustomerReportAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/reports/manager/customers");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CustomerReportDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetManagerCustomerReportAsync");
                return null;
            }
        }
    }
}
