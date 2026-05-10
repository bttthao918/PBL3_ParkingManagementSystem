// Services/CommonDto.cs
namespace ParkingManagement.FE.Services
{
    #region Common

    public class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Title { get; set; }
        public int? Status { get; set; }
    }

    #endregion

    #region Available Slot

    public class AvailableSlotDto
    {
        public string SlotId { get; set; } = "";
        public string Location { get; set; } = "";
        public string VehicleType { get; set; } = "";
    }

    public class AvailableCountResponse
    {
        public int Count { get; set; }
    }

    #endregion

    #region Parking Slot Base

    public class ParkingSlotDto
    {
        public string SlotId { get; set; } = "";
        public string Location { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime LastUpdated { get; set; }
    }

    public class ParkingSlotDetailDto : ParkingSlotDto
    {
        public bool IsOccupied { get; set; }
        public string? CurrentVehiclePlate { get; set; }
        public string? CurrentCustomerName { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public int? CurrentOccupancyMinutes { get; set; }
        public int TotalUsageCount { get; set; }
        public int UsageThisMonth { get; set; }
        public int UsageThisWeek { get; set; }
        public double AverageOccupancyTime { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public List<SlotUsageHistoryDto> RecentHistory { get; set; } = new();
    }

    public class SlotUsageHistoryDto
    {
        public string VehiclePlate { get; set; } = "";
        public string? CustomerName { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? DurationMinutes { get; set; }
    }

    #endregion

    #region Status Management

    public class UpdateSlotStatusRequest
    {
        public string SlotId { get; set; } = "";
        public string NewStatus { get; set; } = "";
        public string? Note { get; set; }
        public string? EmployeeId { get; set; }
    }

    public class UpdateSlotStatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? SlotId { get; set; }
        public string? NewStatus { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ValidateTransitionResponse
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }

    #endregion

    #region Audit Logs

    public class ParkingSlotAuditLogDto
    {
        public string LogId { get; set; } = "";
        public string SlotId { get; set; } = "";
        public string EmployeeId { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string OldStatus { get; set; } = "";
        public string NewStatus { get; set; } = "";
        public string? Note { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }

    #endregion

    #region Statistics

    public class ParkingSlotSummaryDto
    {
        public int TotalSlots { get; set; }
        public int TotalEmpty { get; set; }
        public int TotalOccupied { get; set; }
        public int TotalBooked { get; set; }
        public int TotalMaintenance { get; set; }
        public int TotalError { get; set; }
        public double UtilizationRate { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public Dictionary<string, int> ByVehicleType { get; set; } = new();
    }

    public class ParkingSlotStatisticsDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalCheckOuts { get; set; }
        public double AverageOccupancyTimeMinutes { get; set; }
        public List<DailySlotStatsDto> DailyStats { get; set; } = new();
        public List<TopUsedSlotDto> TopUsedSlots { get; set; } = new();
        public Dictionary<string, double> AverageOccupancyByVehicleType { get; set; } = new();
    }

    public class DailySlotStatsDto
    {
        public DateTime Date { get; set; }
        public int CheckInCount { get; set; }
        public int CheckOutCount { get; set; }
        public int PeakOccupancy { get; set; }
        public double AverageOccupancyMinutes { get; set; }
    }

    public class TopUsedSlotDto
    {
        public string SlotId { get; set; } = "";
        public string Location { get; set; } = "";
        public int UsageCount { get; set; }
        public double AverageOccupancyMinutes { get; set; }
        public DateTime? LastUsedAt { get; set; }
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

    #endregion

    #region Manager

    public class ParkingSlotFilterDto
    {
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public string? Location { get; set; }
        public string? SearchKeyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ListParkingSlotResponseDto
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

    public class ParkingSlotListDto
    {
        public string SlotId { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Location { get; set; } = "";
        public string Status { get; set; } = "";
        public string? CurrentOccupant { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class ParkingSlotDetailForManagerDto : ParkingSlotDetailDto
    {
        public decimal TotalRevenueFromSlot { get; set; }
        public int MonthlyRevenueTarget { get; set; }
        public int PerformanceScore { get; set; }
        public List<string> MaintenanceHistory { get; set; } = new();
    }

    public class UpdateParkingSlotRequest
    {
        public string SlotId { get; set; } = "";
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateParkingSlotResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string SlotId { get; set; } = "";
    }

    #endregion

    #region Employee

    public class EmployeeSlotFilterDto
    {
        public string? VehicleType { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ListEmployeeSlotResponseDto
    {
        public List<EmployeeSlotListItemDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int TotalEmpty { get; set; }
        public int TotalOccupied { get; set; }
        public int TotalMaintenance { get; set; }
        public double UtilizationRate { get; set; }
    }

    public class EmployeeSlotListItemDto
    {
        public string SlotId { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Location { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsAvailable { get; set; }
        public string? CurrentOccupant { get; set; }
        public DateTime? OccupiedSince { get; set; }
    }

    public class EmployeeSlotDetailDto
    {
        public string SlotId { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Location { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsOccupied { get; set; }
        public string? CurrentVehiclePlate { get; set; }
        public string? CurrentCustomerName { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public int? CurrentOccupancyMinutes { get; set; }
        public int UsageThisMonth { get; set; }
        public int UsageThisWeek { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    #endregion

    #region Maintenance

    public class SetMaintenanceRequest
    {
        public string SlotId { get; set; } = "";
        public string Reason { get; set; } = "";
        public string? Note { get; set; }
    }

    public class SetMaintenanceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? SlotId { get; set; }
        public DateTime? MaintenanceUntil { get; set; }
    }

    #endregion
}