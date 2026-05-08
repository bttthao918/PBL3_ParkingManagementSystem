namespace ParkingManagement.BLL.DTOs
{
    public class CreateEmployeeDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }

    public class EmployeeDto
    {
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Shift { get; set; }
        public string? Email { get; set; }
        public bool IsManager { get; set; }
        public string? Department { get; set; }
        public bool IsDeleted { get; set; }
    }
}
