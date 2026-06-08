using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ParkingManagement.FE.Services
{
    public interface IWorkLogService
    {
        Task<WorkLogStatusResponse?> GetCurrentStatusAsync();
        Task<WorkLogActionResponse?> StartShiftAsync(string? scheduleId = null, string? note = null);
        Task<WorkLogActionResponse?> EndShiftAsync(string? note = null);
        Task<WorkLogMonthlySummaryResponse?> GetMonthlySummaryAsync(int? year = null, int? month = null);
    }

    public class WorkLogService : IWorkLogService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<WorkLogService> _logger;

        public WorkLogService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<WorkLogService> logger)
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

        public async Task<WorkLogStatusResponse?> GetCurrentStatusAsync()
        {
            try
            {
                AddAuth();
                var response = await _httpClient.GetAsync("api/worklogs/current");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<WorkLogStatusResponse>();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCurrentStatus error");
                return null;
            }
        }

        public async Task<WorkLogActionResponse?> StartShiftAsync(string? scheduleId = null, string? note = null)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/worklogs/start", new { scheduleId, note });
                return await response.Content.ReadFromJsonAsync<WorkLogActionResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StartShift error");
                return null;
            }
        }

        public async Task<WorkLogActionResponse?> EndShiftAsync(string? note = null)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/worklogs/end", new { note });
                return await response.Content.ReadFromJsonAsync<WorkLogActionResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EndShift error");
                return null;
            }
        }

        public async Task<WorkLogMonthlySummaryResponse?> GetMonthlySummaryAsync(int? year = null, int? month = null)
        {
            try
            {
                AddAuth();
                var queryParams = new List<string>();
                if (year.HasValue) queryParams.Add($"year={year.Value}");
                if (month.HasValue) queryParams.Add($"month={month.Value}");
                var url = "api/worklogs/monthly-summary";
                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<WorkLogMonthlySummaryResponse>();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMonthlySummary error");
                return null;
            }
        }
    }

    // ── Response DTOs ──
    public class WorkLogStatusResponse
    {
        public bool IsWorking { get; set; }
        public string? WorkLogId { get; set; }
        public string? ScheduleId { get; set; }
        public string? ShiftType { get; set; }
        public DateTime? StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Note { get; set; }
        public string? Message { get; set; }
        public WorkLogAutoClosedShift? AutoClosedShift { get; set; }
    }

    public class WorkLogAutoClosedShift
    {
        public string? WorkLogId { get; set; }
        public string? ScheduleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalMinutes { get; set; }
    }

    public class WorkLogActionResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? WorkLogId { get; set; }
        public string? ScheduleId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? TotalMinutes { get; set; }
    }

    public class WorkLogMonthlySummaryResponse
    {
        public int TotalDays { get; set; }
        public int TotalMinutes { get; set; }
        public int TotalHours { get; set; }
        public double AverageHoursPerDay { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
