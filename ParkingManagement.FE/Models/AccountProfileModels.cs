namespace ParkingManagement.FE.Models
{
    public class AccountProfileDto
    {
        public string AccountId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string Gender { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateAccountProfileDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
    }

    public class AccountProfileUpdateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public AccountProfileDto? Data { get; set; }
    }
}
