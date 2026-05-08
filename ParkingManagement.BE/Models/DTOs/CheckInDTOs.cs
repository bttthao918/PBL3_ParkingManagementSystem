namespace ParkingManagement.BLL.DTOs
{
    // ════════════════════════════════════════════════════════════
    // UC008 - Check-in (Gửi Xe)
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO cho bước 1: Nhân viên nhập biển số xe
    /// </summary>
    public class CheckInInputDto
    {
        public string VehiclePlate { get; set; } = null!;       // Biển số xe (vd: 43A-123.45)
        public string VehicleType { get; set; } = null!;        // Loại xe (Xe máy, Ô tô nhỏ, Ô tô lớn)
        public string? CustomerId { get; set; }                 // ID khách hàng (nếu có)
    }

    /// <summary>
    /// DTO kết quả kiểm tra chỗ trống
    /// Gồm: Kiểm tra vé tháng → Kiểm tra đặt chỗ → Gợi ý chỗ trống
    /// </summary>
    public class CheckInValidationDto
    {
        public bool HasVehicleRecord { get; set; }              // Xe đã được ghi nhận trước đó?
        public string? CustomerId { get; set; }                 // ID khách nếu tìm được
        public string? CustomerName { get; set; }               // Tên khách

        // Kiểm tra vé tháng
        public bool HasMonthlyTicket { get; set; }              // Xe có vé tháng?
        public string? MonthlyTicketId { get; set; }            // ID vé tháng (nếu có)
        public DateTime? MonthlyTicketExpiryDate { get; set; }  // Hạn vé tháng

        // Kiểm tra đặt chỗ
        public bool HasReservation { get; set; }                // Xe có đặt chỗ?
        public string? ReservationId { get; set; }              // ID đặt chỗ
        public string? PreferredSlotId { get; set; }            // Chỗ được ưu tiên (nếu đặt)

        // Gợi ý chỗ trống
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();  // Danh sách chỗ trống
        public string? Message { get; set; }                    // Thông báo
    }

    /// <summary>
    /// DTO thông tin chỗ trống có sẵn
    /// </summary>
    public class AvailableSlotDto
    {
        public string SlotId { get; set; } = null!;             // Mã chỗ (A01, A02, ...)
        public string Location { get; set; } = null!;           // Vị trí (Khu A - Ô 01, ...)
        public string VehicleType { get; set; } = null!;        // Loại xe phù hợp
    }

    /// <summary>
    /// DTO cho bước 2: Xác nhận check-in
    /// </summary>
    public class ConfirmCheckInDto
    {
        public string VehiclePlate { get; set; } = null!;       // Biển số xe
        public string VehicleType { get; set; } = null!;        // Loại xe
        public string SlotId { get; set; } = null!;             // Chỗ được chọn
        public string? CustomerId { get; set; }                 // ID khách (nếu có)
    }

    /// <summary>
    /// DTO response sau check-in thành công
    /// </summary>
    public class CheckInResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TicketId { get; set; }                   // Mã vé
        public string? SlotId { get; set; }                     // Chỗ đã gán
        public DateTime? CheckInTime { get; set; }              // Thời gian check-in
    }
}
