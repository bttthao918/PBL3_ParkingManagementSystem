namespace ParkingManagement.FE.Models.Auth
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
    }
}
