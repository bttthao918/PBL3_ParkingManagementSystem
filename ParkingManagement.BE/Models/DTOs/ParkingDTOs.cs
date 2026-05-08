namespace ParkingManagement.BLL.DTOs
{
    // ── ParkingSlot DTOs ──────────────────────────────────────
    public class UpdateSlotStatusDto
    {
        public string SlotId { get; set; } = null!;
        public string NewStatus { get; set; } = null!;
        public string? Note { get; set; }
    }

    public class ParkingSlotDto
    {
        public string SlotId { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime LastUpdated { get; set; }
    }

    // ── Report DTOs ───────────────────────────────────────────
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
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TicketCount { get; set; }
    }

    // ── Manager Dashboard DTOs ────────────────────────────────
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

    public class CustomerReportDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveMonthlyTickets { get; set; }
        public int ExpiredMonthlyTickets { get; set; }
        public int RegularCustomers { get; set; }
        public int VIPCustomers { get; set; }
        public int OneTimeCustomers { get; set; }
        public List<CustomerDetailDto> TopCustomers { get; set; } = new();
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
    }

    public class TopCustomerDto
    {
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public int TicketCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    // ── Parking Slot Management DTOs ──────────────────────────
    public class ParkingSlotFilterDto
    {
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public string? Location { get; set; }
        public string? SearchKeyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ParkingSlotListDto
    {
        public string SlotId { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? CurrentOccupant { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class ListParkingSlotDto
    {
        public List<ParkingSlotListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int TotalEmpty { get; set; }
        public int TotalOccupied { get; set; }
        public int TotalMaintenance { get; set; }
    }

    public class ParkingSlotDetailDto
    {
        public string SlotId { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool IsOccupied { get; set; }
        public string? CurrentVehiclePlate { get; set; }
        public string? CurrentCustomerName { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public int? CurrentOccupancyMinutes { get; set; }
        public int TotalUsageCount { get; set; }
        public int UsageThisMonth { get; set; }
        public int UsageThisWeek { get; set; }
        public double AverageOccupancyTime { get; set; }
        public double AverageEmptyTime { get; set; }
        public DateTime? FirstUsedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public List<SlotUsageHistoryDto> RecentHistory { get; set; } = new();
    }

    public class SlotUsageHistoryDto
    {
        public string VehiclePlate { get; set; } = null!;
        public string? CustomerName { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? DurationMinutes { get; set; }
    }

    public class ParkingSlotReportDto
    {
        public int TotalSlots { get; set; }
        public int TotalEmpty { get; set; }
        public int TotalOccupied { get; set; }
        public int TotalMaintenance { get; set; }
        public double UtilizationRate { get; set; }
        public Dictionary<string, SlotTypeDetailDto> ByVehicleType { get; set; } = new();
        public List<TopUsedSlotDto> TopUsedSlots { get; set; } = new();
        public List<TopUsedSlotDto> LeastUsedSlots { get; set; } = new();
    }

    public class SlotTypeDetailDto
    {
        public int Total { get; set; }
        public int Empty { get; set; }
        public int Occupied { get; set; }
        public int Maintenance { get; set; }
        public double UtilizationRate { get; set; }
    }

    public class TopUsedSlotDto
    {
        public string SlotId { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int UsageCount { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class UpdateParkingSlotDto
    {
        public string SlotId { get; set; } = null!;
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateParkingSlotResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string SlotId { get; set; } = null!;
    }

    public record CheckInResult(
        int TicketId,
        string LicensePlate,
        string AreaName,
        DateTime EntryTime
    );

    public record CheckOutResult(
        int TicketId,
        string LicensePlate,
        string AreaName,
        DateTime EntryTime,
        DateTime ExitTime,
        double DurationHours,
        decimal Price
    );
}
