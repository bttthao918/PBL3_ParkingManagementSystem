using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ParkingManagement.FE.Services
{
    public interface IShiftScheduleService
    {
        Task<ShiftWeekResponse?> GetWeekScheduleAsync(DateTime? startDate = null);
        Task<ShiftActionResponse?> CreateAsync(string employeeId, DateTime workDate, string shiftType, string? note = null);
        Task<ShiftActionResponse?> BulkCreateAsync(List<ShiftAssignment> assignments);
        Task<ShiftActionResponse?> DeleteAsync(string scheduleId);
        Task<ShiftTodayResponse?> GetMyTodayShiftAsync();
        Task<List<ShiftMyWeekItem>?> GetMyWeekAsync();
    }

    public class ShiftScheduleService : IShiftScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ShiftScheduleService> _logger;

        public ShiftScheduleService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ShiftScheduleService> logger)
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

        public async Task<ShiftWeekResponse?> GetWeekScheduleAsync(DateTime? startDate = null)
        {
            try
            {
                AddAuth();
                var url = "api/shifts/week";
                if (startDate.HasValue) url += $"?startDate={startDate.Value:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ShiftWeekResponse>();
                return null;
            }
            catch (Exception ex) { _logger.LogError(ex, "GetWeekSchedule error"); return null; }
        }

        public async Task<ShiftActionResponse?> CreateAsync(string employeeId, DateTime workDate, string shiftType, string? note = null)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/shifts", new { employeeId, workDate, shiftType, note });
                return await response.Content.ReadFromJsonAsync<ShiftActionResponse>();
            }
            catch (Exception ex) { _logger.LogError(ex, "CreateShift error"); return null; }
        }

        public async Task<ShiftActionResponse?> BulkCreateAsync(List<ShiftAssignment> assignments)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.PostAsJsonAsync("api/shifts/bulk", new { assignments });
                return await response.Content.ReadFromJsonAsync<ShiftActionResponse>();
            }
            catch (Exception ex) { _logger.LogError(ex, "BulkCreate error"); return null; }
        }

        public async Task<ShiftActionResponse?> DeleteAsync(string scheduleId)
        {
            try
            {
                AddAuth();
                var response = await _httpClient.DeleteAsync($"api/shifts/{scheduleId}");
                return await response.Content.ReadFromJsonAsync<ShiftActionResponse>();
            }
            catch (Exception ex) { _logger.LogError(ex, "DeleteShift error"); return null; }
        }

        public async Task<ShiftTodayResponse?> GetMyTodayShiftAsync()
        {
            try
            {
                AddAuth();
                var response = await _httpClient.GetAsync("api/shifts/my-today");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ShiftTodayResponse>();
                return null;
            }
            catch (Exception ex) { _logger.LogError(ex, "GetMyTodayShift error"); return null; }
        }

        public async Task<List<ShiftMyWeekItem>?> GetMyWeekAsync()
        {
            try
            {
                AddAuth();
                var response = await _httpClient.GetAsync("api/shifts/my-week");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<ShiftMyWeekItem>>();
                return null;
            }
            catch (Exception ex) { _logger.LogError(ex, "GetMyWeek error"); return null; }
        }
    }

    // Response DTOs
    public class ShiftWeekResponse
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<ShiftScheduleItem> Schedules { get; set; } = new();
        public List<ShiftEmployeeItem> Employees { get; set; } = new();
    }

    public class ShiftScheduleItem
    {
        public string ScheduleId { get; set; } = "";
        public string EmployeeId { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public DateTime WorkDate { get; set; }
        public string ShiftType { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Note { get; set; }
        public int WorkMinutes { get; set; }
    }

    public class ShiftEmployeeItem
    {
        public string EmployeeId { get; set; } = "";
        public string FullName { get; set; } = "";
    }

    public class ShiftActionResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ScheduleId { get; set; }
        public int Created { get; set; }
        public int Skipped { get; set; }
    }

    public class ShiftTodayResponse
    {
        public bool HasShift { get; set; }
        public string? Message { get; set; }
        public ShiftTodayDetail? Shift { get; set; }
    }

    public class ShiftTodayDetail
    {
        public string ScheduleId { get; set; } = "";
        public string ShiftType { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Note { get; set; }
    }

    public class ShiftMyWeekItem
    {
        public DateTime WorkDate { get; set; }
        public string ShiftType { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class ShiftAssignment
    {
        public string EmployeeId { get; set; } = "";
        public DateTime WorkDate { get; set; }
        public string ShiftType { get; set; } = "";
        public string? Note { get; set; }
    }
}
