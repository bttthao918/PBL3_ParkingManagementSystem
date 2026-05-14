namespace ParkingManagement.FE.Models
{
    // ── Response từ API GET /api/employee/monthly-tickets ──
    public class EmployeeMonthlyTicketListResponse
    {
        public List<EmployeeMonthlyTicketItem> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public EmployeeMonthlyTicketSummary Summary { get; set; } = new();
    }

    public class EmployeeMonthlyTicketItem
    {
        public string MonthlyTicketId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public string VehiclePlate { get; set; } = "";
        public string? VehicleType { get; set; }
        public string PackageType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
        public int DaysRemaining { get; set; }
        public decimal TotalFee { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeMonthlyTicketSummary
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Expired { get; set; }
        public int ExpiringSoon { get; set; }
    }

    // ── Response từ API GET /api/employee/monthly-tickets/{id} ──
    public class EmployeeMonthlyTicketDetailResponse
    {
        public string MonthlyTicketId { get; set; } = "";
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerId { get; set; }
        public string VehiclePlate { get; set; } = "";
        public string? VehicleType { get; set; }
        public string PackageType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
        public int DaysRemaining { get; set; }
        public decimal TotalFee { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<EmployeeMonthlyTicketPayment> Payments { get; set; } = new();
    }

    public class EmployeeMonthlyTicketPayment
    {
        public string PaymentId { get; set; } = "";
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public DateTime PaymentTime { get; set; }
        public string Status { get; set; } = "";
    }

    // ── Request DTOs ──
    public class CreateEmployeeMonthlyTicketRequest
    {
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "Xe máy";
        public int DurationMonths { get; set; } = 1;
        public string? CustomerId { get; set; }
        public string? CustomerPhone { get; set; }
        public string? PaymentMethod { get; set; } = "Tiền mặt";
    }

    public class RenewEmployeeMonthlyTicketRequest
    {
        public int MonthsToAdd { get; set; } = 1;
        public string? PaymentMethod { get; set; } = "Tiền mặt";
    }

    // ── Generic API result ──
    public class ApiResultResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    // ── Pricing ──
    public class EmployeeMonthlyTicketPricingItem
    {
        public string VehicleType { get; set; } = "";
        public int Months { get; set; }
        public string PackageType { get; set; } = "";
        public decimal Price { get; set; }
    }
}
