// Models/Customer/CustomerDto.cs
namespace ParkingManagement.FE.Models.Customer
{
    public class CustomerProfileDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateCustomerProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
    }

    public class VehicleDto
    {
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
    }
}