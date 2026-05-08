namespace ParkingManagement.BLL.DTOs
{
    // ── Employee Customer Management DTOs ──────────────────
    // UC-EMP-CUST01 - Search customers
    // UC-EMP-CUST02 - Customer detail

    /// <summary>
    /// UC-EMP-CUST01 - Search customers filter
    /// </summary>
    public class EmployeeCustomerSearchFilterDto
    {
        public string SearchKeyword { get; set; } = null!;  // Tên, email, SĐT, biển số
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// UC-EMP-CUST01 - Item trong danh sách tìm kiếm
    /// </summary>
    public class EmployeeCustomerSearchResultDto
    {
        public string CustomerId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool HasActiveMonthlyTicket { get; set; }
        public int TotalTickets { get; set; }
        public DateTime? LastVisit { get; set; }
    }

    /// <summary>
    /// UC-EMP-CUST01 - Danh sách kết quả tìm kiếm
    /// </summary>
    public class ListEmployeeCustomerSearchDto
    {
        public List<EmployeeCustomerSearchResultDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// UC-EMP-CUST02 - Chi tiết khách hàng
    /// </summary>
    public class EmployeeCustomerDetailDto
    {
        public string CustomerId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Monthly Ticket Info
        public bool HasActiveMonthlyTicket { get; set; }
        public string? ActiveMonthlyTicketId { get; set; }
        public DateTime? MonthlyTicketExpiry { get; set; }
        public int? DaysRemainingOnTicket { get; set; }

        // Statistics
        public int TotalTickets { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastVisit { get; set; }
        public DateTime? FirstVisit { get; set; }

        // Favorite Vehicle
        public string? FavoriteVehiclePlate { get; set; }
        public string? FavoriteVehicleType { get; set; }
        public int FavoriteVehicleUsageCount { get; set; }
    }
}
