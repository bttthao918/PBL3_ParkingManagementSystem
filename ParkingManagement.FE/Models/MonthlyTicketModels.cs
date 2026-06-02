namespace ParkingManagement.FE.Models
{
    // ── List/Get DTOs ──
    public class ListMonthlyTicketDto
    {
        public List<MonthlyTicketDetailDto> Items { get; set; } = new();
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
    }

    public class MonthlyTicketDetailDto
    {
        public string MonthlyTicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string? VehicleType { get; set; }
        public string PackageType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = "";
        public int DaysRemaining { get; set; }
        public bool AutoRenew { get; set; }
    }

    // ── Register DTOs ──
    public class RegisterMonthlyTicketDto
    {
        public string? CustomerId { get; set; }
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "Xe máy";
        public string PackageType { get; set; } = "1 tháng"; // "1 tháng", "3 tháng", "6 tháng"
    }

    // ── Renew DTOs ──
    public class RenewMonthlyTicketDto
    {
        public string PackageType { get; set; } = "1 tháng";
    }
}
