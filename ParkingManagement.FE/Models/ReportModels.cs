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
        public List<DailyRevenueDetailDto> DailyBreakdown { get; set; } = new();
        public decimal PreviousPeriodRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public string Trend { get; set; } = "";
        public List<DailyRevenueDetailDto> TopDays { get; set; } = new();
    }
}
