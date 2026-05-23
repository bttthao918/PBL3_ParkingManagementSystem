namespace ParkingManagement.FE.Models
{
    public class EmployeeDashboardDto
    {
        public int TicketsProcessedToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int WorkMinutesToday { get; set; }

        public int TicketsProcessedThisWeek { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public int WorkMinutesThisWeek { get; set; }
        public int WorkDaysThisWeek { get; set; }

        public int TicketsProcessedThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int WorkMinutesThisMonth { get; set; }
        public int WorkDaysThisMonth { get; set; }

        public decimal AverageRevenuePerTicket { get; set; }
        public double AverageTicketsPerDay { get; set; }
        public string CurrentShift { get; set; } = "";
    }

    public class ShiftAttendanceDetailDto
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

    public class ShiftAttendanceReportDto
    {
        public List<ShiftAttendanceDetailDto> Details { get; set; } = new();
        public int TotalWorkDays { get; set; }
        public int PunctualDays { get; set; }
        public int LateDays { get; set; }
        public int AbsentDays { get; set; }
        public int TotalWorkMinutes { get; set; }
        public int AverageWorkMinutesPerDay { get; set; }
    }

    public class DailyRevenueDetailDto
    {
        public DateTime Date { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerTicket { get; set; }
    }

    public class EmployeeRevenueReportDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTickets { get; set; }
        public decimal AverageRevenuePerTicket { get; set; }
        public Dictionary<string, int> TicketsByVehicleType { get; set; } = new();
        public Dictionary<string, decimal> RevenueByVehicleType { get; set; } = new();
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
        public List<DailyRevenueDetailDto> DailyBreakdown { get; set; } = new();
        public decimal PreviousPeriodRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public string Trend { get; set; } = "";
        public List<DailyRevenueDetailDto> TopDays { get; set; } = new();
    }

    public class DashboardSummaryDto
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal ThisYearRevenue { get; set; }
        public int TodayTickets { get; set; }
        public int ThisMonthTickets { get; set; }
        public decimal SlotUtilizationRate { get; set; }
        public int OccupiedSlots { get; set; }
        public int TotalSlots { get; set; }
        public int TotalActiveEmployees { get; set; }
        public int EmployeesOnline { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveMonthlyTickets { get; set; }
    }

    public class RevenueReportFilterDto
    {
        public string Period { get; set; } = "month";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? VehicleType { get; set; }
    }

    public class RevenueReportDto
    {
        public string Period { get; set; } = "month";
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTickets { get; set; }
        public int TotalMonthlyTickets { get; set; }
        public decimal RevenueFromSingleTickets { get; set; }
        public decimal RevenueFromMonthlyTickets { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
        public List<DailyRevenueDto> PreviousDailyBreakdown { get; set; } = new();
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
        public Dictionary<string, decimal> RevenueByVehicleType { get; set; } = new();
        public Dictionary<string, decimal> RevenueByArea { get; set; } = new();
        public List<EmployeeRevenueSummaryDto> TopEmployees { get; set; } = new();
        public List<RevenueRankDto> TopRevenueDays { get; set; } = new();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = "";
        public decimal Revenue { get; set; }
        public int TicketCount { get; set; }
    }

    public class EmployeeRevenueSummaryDto
    {
        public string? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public decimal TotalRevenue { get; set; }
        public int PaymentCount { get; set; }
    }

    public class RevenueRankDto
    {
        public string Label { get; set; } = "";
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal ChangePercentage { get; set; }
    }

    public class CustomerReportDto
    {
        public string Period { get; set; } = "30days";
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int NewCustomersInPeriod { get; set; }
        public int ActiveMonthlyTickets { get; set; }
        public int ExpiredMonthlyTickets { get; set; }
        public int RegularCustomers { get; set; }
        public int VIPCustomers { get; set; }
        public int OneTimeCustomers { get; set; }
        public int WalkInTickets { get; set; }
        public int ReturningCustomers { get; set; }
        public List<CustomerTrendPointDto> NewCustomerTrend { get; set; } = new();
        public List<CustomerTrendPointDto> PreviousNewCustomerTrend { get; set; } = new();
        public List<CustomerBreakdownDto> GroupBreakdown { get; set; } = new();
        public List<CustomerBreakdownDto> AreaBreakdown { get; set; } = new();
        public List<CustomerReturnBucketDto> ReturnBuckets { get; set; } = new();
        public List<CustomerDetailDto> TopCustomers { get; set; } = new();
        public List<CustomerDetailDto> NewCustomers { get; set; } = new();
    }

    public class CustomerDetailDto
    {
        public string CustomerId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int TicketCount { get; set; }
        public decimal TotalSpent { get; set; }
        public bool HasActiveMonthlyTicket { get; set; }
        public DateTime? LastVisit { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public decimal VisitChangePercentage { get; set; }
    }

    public class CustomerTrendPointDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = "";
        public int Count { get; set; }
    }

    public class CustomerBreakdownDto
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class CustomerReturnBucketDto
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
