// Services/ReportService.cs
using System.Net.Http.Json;
using System.Text.Json;

namespace ParkingManagement.FE.Services
{
    public interface IReportService
    {
        // Dashboard
        Task<DashboardSummaryDto?> GetDashboardAsync();
        Task<ManagerDashboardDto?> GetManagerDashboardAsync();
        Task<EmployeeDashboardDto?> GetEmployeeDashboardAsync(string employeeId);

        // Revenue Reports
        Task<RevenueReportDto?> GetRevenueReportAsync(DateTime from, DateTime to);
        Task<RevenueReportDto?> GetRevenueReportAsync(RevenueReportFilterDto filter);
        Task<DetailedRevenueReportDto?> GetDetailedRevenueReportAsync(DateTime from, DateTime to);

        // Customer Reports
        Task<CustomerReportDto?> GetCustomerReportAsync();
        Task<CustomerStatisticsDto?> GetCustomerStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Parking Slot Reports
        Task<ParkingSlotReportDto?> GetParkingSlotReportAsync();
        Task<ParkingSlotSummaryDto?> GetParkingSlotSummaryAsync();

        // Employee Reports
        Task<EmployeePerformanceDto?> GetEmployeePerformanceAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<EmployeeShiftReportDto>?> GetEmployeeShiftReportAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null);

        // Expiring Tickets
        Task<List<ExpiringTicketDto>?> GetExpiringMonthlyTicketsAsync(int days = 7);

        // Active Vehicles
        Task<int> GetActiveVehiclesCountAsync();
    }

    public class ReportService : BaseHttpService, IReportService
    {
        private readonly ILogger<ReportService> _logger;

        public ReportService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ReportService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        #region Dashboard

        public async Task<DashboardSummaryDto?> GetDashboardAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/reports/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
                }

                _logger.LogWarning("GetDashboardAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard");
                return null;
            }
        }

        public async Task<ManagerDashboardDto?> GetManagerDashboardAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/reports/manager/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ManagerDashboardDto>();
                }

                _logger.LogWarning("GetManagerDashboardAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manager dashboard");
                return null;
            }
        }

        public async Task<EmployeeDashboardDto?> GetEmployeeDashboardAsync(string employeeId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/reports/employee/{employeeId}/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeeDashboardDto>();
                }

                _logger.LogWarning("GetEmployeeDashboardAsync failed for {EmployeeId}: {StatusCode}", employeeId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee dashboard for {EmployeeId}", employeeId);
                return null;
            }
        }

        #endregion

        #region Revenue Reports

        public async Task<RevenueReportDto?> GetRevenueReportAsync(DateTime from, DateTime to)
        {
            try
            {
                AttachToken();
                var url = $"api/reports/revenue?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RevenueReportDto>();
                }

                _logger.LogWarning("GetRevenueReportAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue report");
                return null;
            }
        }

        public async Task<RevenueReportDto?> GetRevenueReportAsync(RevenueReportFilterDto filter)
        {
            try
            {
                AttachToken();
                var queryParams = new List<string>();

                if (filter.FromDate.HasValue)
                    queryParams.Add($"from={filter.FromDate.Value:yyyy-MM-dd}");
                if (filter.ToDate.HasValue)
                    queryParams.Add($"to={filter.ToDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(filter.Period))
                    queryParams.Add($"period={Uri.EscapeDataString(filter.Period)}");
                if (!string.IsNullOrEmpty(filter.VehicleType))
                    queryParams.Add($"vehicleType={Uri.EscapeDataString(filter.VehicleType)}");
                if (!string.IsNullOrEmpty(filter.PaymentMethod))
                    queryParams.Add($"paymentMethod={Uri.EscapeDataString(filter.PaymentMethod)}");

                var url = "api/reports/revenue";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RevenueReportDto>();
                }

                _logger.LogWarning("GetRevenueReportAsync with filter failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue report with filter");
                return null;
            }
        }

        public async Task<DetailedRevenueReportDto?> GetDetailedRevenueReportAsync(DateTime from, DateTime to)
        {
            try
            {
                AttachToken();
                var url = $"api/reports/revenue/detailed?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DetailedRevenueReportDto>();
                }

                _logger.LogWarning("GetDetailedRevenueReportAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed revenue report");
                return null;
            }
        }

        #endregion

        #region Customer Reports

        public async Task<CustomerReportDto?> GetCustomerReportAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/reports/manager/customers");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CustomerReportDto>();
                }

                _logger.LogWarning("GetCustomerReportAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer report");
                return null;
            }
        }

        public async Task<CustomerStatisticsDto?> GetCustomerStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AttachToken();
                var queryParams = new List<string>();

                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

                var url = "api/reports/customer-statistics";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CustomerStatisticsDto>();
                }

                _logger.LogWarning("GetCustomerStatisticsAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics");
                return null;
            }
        }

        #endregion

        #region Parking Slot Reports

        public async Task<ParkingSlotReportDto?> GetParkingSlotReportAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/reports/parking-slots");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotReportDto>();
                }

                _logger.LogWarning("GetParkingSlotReportAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parking slot report");
                return null;
            }
        }

        public async Task<ParkingSlotSummaryDto?> GetParkingSlotSummaryAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/parking-slots/summary");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotSummaryDto>();
                }

                _logger.LogWarning("GetParkingSlotSummaryAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parking slot summary");
                return null;
            }
        }

        #endregion

        #region Employee Reports

        public async Task<EmployeePerformanceDto?> GetEmployeePerformanceAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AttachToken();
                var queryParams = new List<string>();

                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

                var url = $"api/reports/employee/{employeeId}/performance";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeePerformanceDto>();
                }

                _logger.LogWarning("GetEmployeePerformanceAsync failed for {EmployeeId}: {StatusCode}", employeeId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee performance for {EmployeeId}", employeeId);
                return null;
            }
        }

        public async Task<List<EmployeeShiftReportDto>?> GetEmployeeShiftReportAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AttachToken();
                var queryParams = new List<string>();

                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

                var url = $"api/reports/employee/{employeeId}/shifts";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<EmployeeShiftReportDto>>();
                }

                _logger.LogWarning("GetEmployeeShiftReportAsync failed for {EmployeeId}: {StatusCode}", employeeId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee shift report for {EmployeeId}", employeeId);
                return null;
            }
        }

        #endregion

        #region Expiring Tickets

        public async Task<List<ExpiringTicketDto>?> GetExpiringMonthlyTicketsAsync(int days = 7)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/reports/expiring-tickets?days={days}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ExpiringTicketDto>>();
                }

                _logger.LogWarning("GetExpiringMonthlyTicketsAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring monthly tickets");
                return null;
            }
        }

        #endregion

        #region Active Vehicles

        public async Task<int> GetActiveVehiclesCountAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/reports/active-vehicles");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ActiveVehiclesResponse>();
                    return result?.ActiveVehicles ?? 0;
                }

                _logger.LogWarning("GetActiveVehiclesCountAsync failed: {StatusCode}", response.StatusCode);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active vehicles count");
                return 0;
            }
        }

        #endregion
    }

    #region DTOs

    public class DashboardSummaryDto
    {
        public decimal TodayRevenue { get; set; }
        public int TodayTickets { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int OccupiedSlots => TotalSlots - AvailableSlots;
        public double UtilizationRate => TotalSlots > 0 ? (double)OccupiedSlots / TotalSlots * 100 : 0;
        public int ActiveMonthlyTickets { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomersToday { get; set; }
    }

    public class ManagerDashboardDto
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal ThisYearRevenue { get; set; }
        public int TodayTickets { get; set; }
        public int ThisMonthTickets { get; set; }
        public double SlotUtilizationRate { get; set; }
        public int OccupiedSlots { get; set; }
        public int TotalSlots { get; set; }
        public int TotalActiveEmployees { get; set; }
        public int EmployeesOnline { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveMonthlyTickets { get; set; }
        public List<DailyRevenueDto> RecentRevenue { get; set; } = new();
    }

    public class EmployeeDashboardDto
    {
        public int TicketsProcessedToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int WorkMinutesToday { get; set; }
        public int TicketsProcessedThisWeek { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public int WorkMinutesThisWeek { get; set; }
        public int TicketsProcessedThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int WorkMinutesThisMonth { get; set; }
        public decimal AverageRevenuePerTicket { get; set; }
        public double AverageTicketsPerDay { get; set; }
        public string CurrentShift { get; set; } = "";
    }

    public class RevenueReportDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTickets { get; set; }
        public int TotalMonthlyTickets { get; set; }
        public decimal RevenueFromSingleTickets { get; set; }
        public decimal RevenueFromMonthlyTickets { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
        public Dictionary<string, decimal> RevenueByVehicleType { get; set; } = new();
    }

    public class DetailedRevenueReportDto : RevenueReportDto
    {
        public List<TopEmployeeRevenueDto> TopEmployees { get; set; } = new();
        public List<TopCustomerRevenueDto> TopCustomers { get; set; } = new();
        public Dictionary<int, decimal> RevenueByHour { get; set; } = new();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TicketCount { get; set; }
        public string DateText => Date.ToString("dd/MM/yyyy");
    }

    public class TopEmployeeRevenueDto
    {
        public string EmployeeId { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public int TicketCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopCustomerRevenueDto
    {
        public string CustomerId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public int TicketCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class CustomerReportDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveMonthlyTickets { get; set; }
        public int ExpiredMonthlyTickets { get; set; }
        public int RegularCustomers { get; set; }
        public int VipCustomers { get; set; }
        public int OneTimeCustomers { get; set; }
        public List<CustomerDetailDto> TopCustomers { get; set; } = new();
    }

    public class CustomerDetailDto
    {
        public string CustomerId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public int TicketCount { get; set; }
        public decimal TotalSpent { get; set; }
        public bool HasActiveMonthlyTicket { get; set; }
        public DateTime? LastVisit { get; set; }
    }

    public class CustomerStatisticsDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int LoyalCustomers { get; set; }
        public int VipCustomers { get; set; }
        public double ReturnRate { get; set; }
        public List<DailyCustomerDto> DailyNewCustomers { get; set; } = new();
        public Dictionary<string, int> CustomerByVehicleType { get; set; } = new();
        public Dictionary<string, int> CustomerByArea { get; set; } = new();
    }

    public class DailyCustomerDto
    {
        public DateTime Date { get; set; }
        public int NewCustomers { get; set; }
        public int TotalVisits { get; set; }
    }

    public class EmployeePerformanceDto
    {
        public string EmployeeId { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public int TotalTicketsProcessed { get; set; }
        public int TicketsProcessedToday { get; set; }
        public int TicketsProcessedThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int TotalWorkDays { get; set; }
        public int TotalWorkHours { get; set; }
        public double AverageTicketsPerDay { get; set; }
        public decimal AverageRevenuePerDay { get; set; }
        public DateTime? FirstWorkDay { get; set; }
        public DateTime? LastWorkDay { get; set; }
        public List<DailyPerformanceDto> DailyPerformance { get; set; } = new();
    }

    public class DailyPerformanceDto
    {
        public DateTime Date { get; set; }
        public int TicketsProcessed { get; set; }
        public decimal Revenue { get; set; }
        public int WorkMinutes { get; set; }
        public string Shift { get; set; } = "";
    }

    public class EmployeeShiftReportDto
    {
        public DateTime Date { get; set; }
        public string Shift { get; set; } = "";
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? WorkMinutes { get; set; }
        public string Status { get; set; } = "";
        public int TicketsProcessed { get; set; }
        public decimal ShiftRevenue { get; set; }
    }

    public class ExpiringTicketDto
    {
        public string MonthlyTicketId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class RevenueReportFilterDto
    {
        public string Period { get; set; } = "month"; // day, week, month, year, custom
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? VehicleType { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class ActiveVehiclesResponse
    {
        public int ActiveVehicles { get; set; }
    }

    #endregion
}