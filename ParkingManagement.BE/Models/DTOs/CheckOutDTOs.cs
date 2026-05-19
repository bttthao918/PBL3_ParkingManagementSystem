namespace ParkingManagement.BLL.DTOs
{
    public class CheckOutInputDto
    {
        public string VehiclePlateOrTicketId { get; set; } = null!;
        public string? PaymentMethod { get; set; } = "Cash";
    }

    public class CheckOutValidationDto
    {
        public bool Success { get; set; }
        public string? TicketId { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleType { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CurrentTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? TicketType { get; set; }
        public bool IsFreeTicket { get; set; }
        public decimal CalculatedFee { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountHolder { get; set; }
        public string? BankTransferContent { get; set; }
        public string? BankTransferQrUrl { get; set; }
        public string? Message { get; set; }
    }

    public class ConfirmCheckOutDto
    {
        public string TicketId { get; set; } = null!;
        public decimal Fee { get; set; }
        public string? PaymentMethod { get; set; } = "Cash";
        public bool PaymentReceivedConfirmed { get; set; }
        public string? BankTransferRef { get; set; }
        public string? CollectedByEmployeeId { get; set; }
    }

    public class CheckOutResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TicketId { get; set; }
        public string? VehiclePlate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Fee { get; set; }
        public bool IsFree { get; set; }
        public string? PaymentId { get; set; }
    }

    public class FeeCalculationDto
    {
        public int DurationMinutes { get; set; }
        public decimal HourlyRate { get; set; } = 5000;
        public decimal DailyRate { get; set; } = 50000;
        public int MinChargeMinutes { get; set; } = 15;

        public decimal CalculateFee()
        {
            var chargeMinutes = DurationMinutes < MinChargeMinutes
                ? MinChargeMinutes
                : DurationMinutes;

            if (DurationMinutes >= 1440)
            {
                var days = DurationMinutes / 1440;
                var remainingMinutes = DurationMinutes % 1440;

                var fee = days * DailyRate;
                if (remainingMinutes > 0)
                    fee += ((decimal)remainingMinutes / 60) * HourlyRate;

                return fee;
            }

            return ((decimal)chargeMinutes / 60) * HourlyRate;
        }
    }
}
