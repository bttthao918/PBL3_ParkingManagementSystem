namespace ParkingManagement.BLL.DTOs
{
    // ════════════════════════════════════════════════════════════
    // UC009 - Check-out (Lấy Xe)
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO cho bước 1: Nhân viên quét mã vé hoặc biển số
    /// </summary>
    public class CheckOutInputDto
    {
        public string VehiclePlateOrTicketId { get; set; } = null!;  // Biển số xe hoặc mã vé
        public string? PaymentMethod { get; set; } = "Tiền mặt";     // Phương thức thanh toán
    }

    /// <summary>
    /// DTO kết quả kiểm tra vé check-out
    /// </summary>
    public class CheckOutValidationDto
    {
        public bool Success { get; set; }
        public string? TicketId { get; set; }                   // Mã vé
        public string? VehiclePlate { get; set; }               // Biển số xe
        public string? VehicleType { get; set; }                // Loại xe
        public string? CustomerName { get; set; }               // Tên khách
        
        // Thông tin vé
        public DateTime? CheckInTime { get; set; }              // Giờ vào
        public DateTime? CurrentTime { get; set; }              // Giờ ra (hiện tại)
        public int DurationMinutes { get; set; }                // Thời gian giữ xe (phút)
        
        // Thông tin phí
        public string? TicketType { get; set; }                 // Loại vé (Vé lượt / Vé tháng)
        public bool IsFreeTicket { get; set; }                  // Miễn phí (vé tháng)?
        public decimal CalculatedFee { get; set; }              // Phí tính toán
        public string? Message { get; set; }                    // Thông báo
    }

    /// <summary>
    /// DTO cho bước 2: Xác nhận check-out
    /// </summary>
    public class ConfirmCheckOutDto
    {
        public string TicketId { get; set; } = null!;           // Mã vé
        public decimal Fee { get; set; }                        // Phí thanh toán
        public string? PaymentMethod { get; set; } = "Tiền mặt"; // Phương thức thanh toán
    }

    /// <summary>
    /// DTO response sau check-out thành công
    /// </summary>
    public class CheckOutResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TicketId { get; set; }                   // Mã vé
        public string? VehiclePlate { get; set; }               // Biển số
        public DateTime? CheckInTime { get; set; }              // Giờ vào
        public DateTime? CheckOutTime { get; set; }             // Giờ ra
        public int DurationMinutes { get; set; }                // Thời gian giữ xe
        public decimal Fee { get; set; }                        // Phí thanh toán
        public bool IsFree { get; set; }                        // Miễn phí?
        public string? PaymentId { get; set; }                  // Mã thanh toán
    }

    /// <summary>
    /// DTO thông tin phí tính toán
    /// </summary>
    public class FeeCalculationDto
    {
        public int DurationMinutes { get; set; }                // Thời gian giữ xe (phút)
        public decimal HourlyRate { get; set; } = 5000;         // Giá/giờ (VND)
        public decimal DailyRate { get; set; } = 50000;         // Giá/ngày (VND)
        public int MinChargeMinutes { get; set; } = 15;         // Tính phí tối thiểu 15 phút
        
        public decimal CalculateFee()
        {
            // Nếu < 15 phút → tính 15 phút
            int chargeMinutes = DurationMinutes < MinChargeMinutes 
                ? MinChargeMinutes 
                : DurationMinutes;

            // Tính theo giờ
            decimal hours = (decimal)chargeMinutes / 60;
            decimal hourlyFee = hours * HourlyRate;

            // Nếu > 1 ngày (1440 phút) → tính theo ngày
            if (DurationMinutes >= 1440)
            {
                int days = DurationMinutes / 1440;
                int remainingMinutes = DurationMinutes % 1440;
                
                decimal fee = (days * DailyRate);
                if (remainingMinutes > 0)
                {
                    fee += ((decimal)remainingMinutes / 60) * HourlyRate;
                }
                return fee;
            }

            return hourlyFee;
        }
    }
}
