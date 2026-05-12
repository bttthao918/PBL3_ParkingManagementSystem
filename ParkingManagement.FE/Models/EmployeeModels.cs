namespace ParkingManagement.FE.Models
{
    public class ManagerEmployeeFilterDto
    {
        public string? Status { get; set; }           
        public string? Shift { get; set; }            
        public string? SearchKeyword { get; set; }    
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ManagerEmployeeListDto
    {
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Shift { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class ListManagerEmployeeDto
    {
        public List<ManagerEmployeeListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
    }
}
