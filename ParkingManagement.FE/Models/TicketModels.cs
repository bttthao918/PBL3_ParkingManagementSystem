namespace ParkingManagement.FE.Models
{
    public class EmployeeTicketSearchDto
    {
        public string? SearchKeyword { get; set; }
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public string? AreaFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class EmployeeTicketListDto
    {
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "";
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
        public string? CustomerName { get; set; }
    }

    public class ListEmployeeTicketDto
    {
        public List<EmployeeTicketListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class TicketSummaryDto
    {
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int CheckedOutTickets { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TicketDetailDto
    {
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string? VehicleType { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? DurationMinutes { get; set; }
        public string Status { get; set; } = "";
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
        public string? MonthlyTicketId { get; set; }
        public bool HasActiveMonthlyTicket { get; set; }
    }

    public class UpdateTicketRequestDto
    {
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "";
        public decimal Fee { get; set; }
        public string? SlotId { get; set; }
    }

    public class CreateTicketRequestDto
    {
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string? SlotId { get; set; }
        public string? CustomerId { get; set; }
    }

    public class CreateTicketResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TicketId { get; set; }
        public string? SlotId { get; set; }
        public DateTime? CheckInTime { get; set; }
    }

    public class PricingDto
    {
        public Dictionary<string, decimal> HourlyRate { get; set; } = new();
        public Dictionary<string, decimal> MaxDailyFee { get; set; } = new();
        public Dictionary<string, MonthlyPricingDto> MonthlyTicketPrice { get; set; } = new();
        public DateTime LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }
    }

    public class MonthlyPricingDto
    {
        public decimal OneMonth { get; set; }
        public decimal ThreeMonth { get; set; }
        public decimal SixMonth { get; set; }
    }

    public class UpdatePricingDto
    {
        public Dictionary<string, decimal>? HourlyRate { get; set; }
        public Dictionary<string, decimal>? MaxDailyFee { get; set; }
        public Dictionary<string, UpdateMonthlyPricingDto>? MonthlyTicketPrice { get; set; }
    }

    public class UpdateMonthlyPricingDto
    {
        public decimal? OneMonth { get; set; }
        public decimal? ThreeMonth { get; set; }
        public decimal? SixMonth { get; set; }
        public decimal? ThreeMonthDiscountPercent { get; set; }
        public decimal? SixMonthDiscountPercent { get; set; }
    }

    public class PricingInputModel
    {
        public decimal MotorcycleHourlyRate { get; set; }
        public decimal MotorcycleMaxDailyFee { get; set; }
        public decimal MotorcycleMonthlyOneMonth { get; set; }
        public decimal MotorcycleMonthlyThreeMonth { get; set; }
        public decimal MotorcycleMonthlySixMonth { get; set; }
        public decimal MotorcycleMonthlyThreeMonthDiscountPercent { get; set; }
        public decimal MotorcycleMonthlySixMonthDiscountPercent { get; set; }
        public decimal SmallCarHourlyRate { get; set; }
        public decimal SmallCarMaxDailyFee { get; set; }
        public decimal SmallCarMonthlyOneMonth { get; set; }
        public decimal SmallCarMonthlyThreeMonth { get; set; }
        public decimal SmallCarMonthlySixMonth { get; set; }
        public decimal SmallCarMonthlyThreeMonthDiscountPercent { get; set; }
        public decimal SmallCarMonthlySixMonthDiscountPercent { get; set; }
        public decimal LargeCarHourlyRate { get; set; }
        public decimal LargeCarMaxDailyFee { get; set; }
        public decimal LargeCarMonthlyOneMonth { get; set; }
        public decimal LargeCarMonthlyThreeMonth { get; set; }
        public decimal LargeCarMonthlySixMonth { get; set; }
        public decimal LargeCarMonthlyThreeMonthDiscountPercent { get; set; }
        public decimal LargeCarMonthlySixMonthDiscountPercent { get; set; }
    }
}
