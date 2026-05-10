namespace ParkingManagement.FE.Models.MonthlyTicket
{
    public class MonthlyTicketDto
    {
        public string MonthlyTicketId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string PackageType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
    }

    public class RegisterMonthlyTicketRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string PackageType { get; set; } = "1 tháng";
    }

    public class RenewMonthlyTicketRequest
    {
        public string PackageType { get; set; } = "1 tháng";
    }

    public class RegisterMonthlyTicketResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public MonthlyTicketDto? Data { get; set; }
    }

    public class ListMonthlyTicketsResponse
    {
        public List<MonthlyTicketDto> Items { get; set; } = new();
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
    }
}