namespace ParkingManagement.FE.Models.Employee
{
    public class EmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Shift { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public string Shift { get; set; } = "Sáng";
    }
}