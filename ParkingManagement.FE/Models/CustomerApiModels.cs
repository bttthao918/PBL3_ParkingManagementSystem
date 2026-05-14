namespace ParkingManagement.FE.Models
{
    public class CustomerProfileDto
    {
        public string CustomerId { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeCustomerSearchFilterDto
    {
        public string SearchKeyword { get; set; } = "";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class EmployeeCustomerSearchResultDto
    {
        public string CustomerId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public bool HasActiveMonthlyTicket { get; set; }
        public int TotalTickets { get; set; }
        public DateTime? LastVisit { get; set; }
    }

    public class ListEmployeeCustomerSearchDto
    {
        public List<EmployeeCustomerSearchResultDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class CustomerReservationDto
    {
        public string ReservationId { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string? SlotId { get; set; }
        public string? SlotLocation { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }

    public class ListCustomerReservationDto
    {
        public List<CustomerReservationDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class CustomerTicketDto
    {
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "";
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
    }

    public class ListCustomerTicketDto
    {
        public List<CustomerTicketDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class CustomerMonthlyTicketDto
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

    public class ListCustomerMonthlyTicketDto
    {
        public List<CustomerMonthlyTicketDto> Items { get; set; } = new();
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
    }

    public class RegisterMonthlyTicketRequestDto
    {
        public string VehiclePlate { get; set; } = "";
        public string? VehicleType { get; set; }
        public string PackageType { get; set; } = "";
        public string? PaymentMethod { get; set; }
    }

    public class RegisterMonthlyTicketResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public decimal Fee { get; set; }
        public CustomerMonthlyTicketDto? Data { get; set; }
    }

    public class RenewMonthlyTicketRequestDto
    {
        public string PackageType { get; set; } = "";
        public string? PaymentMethod { get; set; }
    }

    public class RenewMonthlyTicketResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public decimal AdditionalFee { get; set; }
        public CustomerMonthlyTicketDto? Data { get; set; }
    }

    public class BasicApiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

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

    public class ApiActionResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
    }

    public class CustomerPaymentDto
    {
        public string PaymentId { get; set; } = "";
        public string? TicketId { get; set; }
        public string? VehiclePlate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    public class ListCustomerPaymentDto
    {
        public List<CustomerPaymentDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
