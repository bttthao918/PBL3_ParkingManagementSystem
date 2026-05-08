namespace ParkingManagement.BLL.DTOs
{
    // ── Employee Report DTOs ──────────────────────────────────
    // UC-EMP-RPT01 - Dashboard
    // UC-EMP-RPT02 - Shift Attendance
    // UC-EMP-RPT03 - Revenue Report

    /// <summary>
    /// UC-EMP-RPT01 - Dashboard cá nhân (My Performance)
    /// </summary>
    public class EmployeeDashboardDto
    {
        // Today
        public int TicketsProcessedToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int WorkMinutesToday { get; set; }

        // This Week
        public int TicketsProcessedThisWeek { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public int WorkMinutesThisWeek { get; set; }
        public int WorkDaysThisWeek { get; set; }

        // This Month
        public int TicketsProcessedThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int WorkMinutesThisMonth { get; set; }
        public int WorkDaysThisMonth { get; set; }

        // Overall
        public decimal AverageRevenuePerTicket { get; set; }
        public double AverageTicketsPerDay { get; set; }
        public string CurrentShift { get; set; } = null!;
    }

    /// <summary>
    /// UC-EMP-RPT02 - Báo cáo điểm danh ca làm
    /// </summary>
    public class ShiftAttendanceDetailDto
    {
        public DateTime Date { get; set; }
        public string Shift { get; set; } = null!;  // "Sáng", "Chiều", "Tối"
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? WorkMinutes { get; set; }
        public string Status { get; set; } = null!;  // "Đúng giờ", "Muộn", "Sớm", "Nghỉ"
        public int TicketsProcessed { get; set; }
        public decimal ShiftRevenue { get; set; }
    }

    /// <summary>
    /// UC-EMP-RPT02 - Báo cáo điểm danh (summary)
    /// </summary>
    public class ShiftAttendanceReportDto
    {
        public List<ShiftAttendanceDetailDto> Details { get; set; } = new();

        // Summary
        public int TotalWorkDays { get; set; }
        public int PunctualDays { get; set; }
        public int LateDays { get; set; }
        public int AbsentDays { get; set; }
        public int TotalWorkMinutes { get; set; }
        public int AverageWorkMinutesPerDay { get; set; }

        // By Shift
        public Dictionary<string, int> WorkDaysByShift { get; set; } = new();
        public Dictionary<string, int> WorkMinutesByShift { get; set; } = new();
    }

    /// <summary>
    /// UC-EMP-RPT03 - Báo cáo doanh thu
    /// </summary>
    public class DailyRevenueDetailDto
    {
        public DateTime Date { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerTicket { get; set; }
    }

    /// <summary>
    /// UC-EMP-RPT03 - Báo cáo doanh thu (summary)
    /// </summary>
    public class EmployeeRevenueReportDto
    {
        // Period summary
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTickets { get; set; }
        public decimal AverageRevenuePerTicket { get; set; }

        // By vehicle type
        public Dictionary<string, int> TicketsByVehicleType { get; set; } = new();
        public Dictionary<string, decimal> RevenueByVehicleType { get; set; } = new();

        // Daily breakdown
        public List<DailyRevenueDetailDto> DailyBreakdown { get; set; } = new();

        // Trend
        public decimal PreviousPeriodRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public string Trend { get; set; } = "";  // "↑ Tăng", "↓ Giảm", "→ Ổn định"

        // Top days
        public List<DailyRevenueDetailDto> TopDays { get; set; } = new();
    }
}
