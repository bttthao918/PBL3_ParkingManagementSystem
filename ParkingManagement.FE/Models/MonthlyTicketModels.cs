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
    }

    // ── Register DTOs ──
    public class RegisterMonthlyTicketDto
    {
        public string? CustomerId { get; set; }
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "Xe máy";
        public string PackageType { get; set; } = "1 tháng"; // "1 tháng", "3 tháng", "6 tháng"
    }

    public class RegisterMonthlyTicketResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public decimal Fee { get; set; }
        public MonthlyTicketDetailDto? Data { get; set; }
    }

    // ── Renew DTOs ──
    public class RenewMonthlyTicketDto
    {
        public string PackageType { get; set; } = "1 tháng";
    }

    public class RenewMonthlyTicketResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public decimal AdditionalFee { get; set; }
        public MonthlyTicketDetailDto? Data { get; set; }
    }

    // ── Pricing DTOs ──
    public class MonthlyTicketPricingDto
    {
        public List<PackagePriceDto> Packages { get; set; } = new();
    }

    public class PackagePriceDto
    {
        public string Package { get; set; } = "";
        public decimal Price { get; set; }
        public string? Discount { get; set; }
    }
}
