// Models/Payment/PaymentDto.cs
namespace ParkingManagement.FE.Models.Payment
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt";
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime PaymentTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}