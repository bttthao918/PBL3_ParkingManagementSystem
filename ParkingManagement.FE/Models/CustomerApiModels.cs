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
        public string? StatusFilter { get; set; }
        public string? VehicleType { get; set; }
        public string? VipLevel { get; set; }
        public DateTime? RegisterDate { get; set; }
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
        public string? MainVehiclePlate { get; set; }
        public string? MainVehicleType { get; set; }
        public int VehicleCount { get; set; }
        public string? VipLevel { get; set; }
    }

    public class ListEmployeeCustomerSearchDto
    {
        public List<EmployeeCustomerSearchResultDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class EmployeeCustomerDetailDto
    {
        public string CustomerId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasActiveMonthlyTicket { get; set; }
        public string? ActiveMonthlyTicketId { get; set; }
        public DateTime? MonthlyTicketExpiry { get; set; }
        public int? DaysRemainingOnTicket { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastVisit { get; set; }
        public DateTime? FirstVisit { get; set; }
        public string? FavoriteVehiclePlate { get; set; }
        public string? FavoriteVehicleType { get; set; }
        public int FavoriteVehicleUsageCount { get; set; }
        public string? VipLevel { get; set; }
        public int? DiscountPercent { get; set; }
        public int? VipProgress { get; set; }
        public decimal? AmountToNextLevel { get; set; }
        public List<EmployeeCustomerVehicleDto> Vehicles { get; set; } = new();
        public List<EmployeeCustomerParkingHistoryDto> RecentTickets { get; set; } = new();
    }

    public class EmployeeCustomerVehicleDto
    {
        public string PlateNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public bool HasActiveMonthlyTicket { get; set; }
        public DateTime? MonthlyTicketExpiry { get; set; }
    }

    public class EmployeeCustomerParkingHistoryDto
    {
        public string TicketId { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; } = "";
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
