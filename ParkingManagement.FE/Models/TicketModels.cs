namespace ParkingManagement.FE.Models
{
    public class EmployeeTicketSearchDto
    {
        public string? SearchKeyword { get; set; }
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
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
}
