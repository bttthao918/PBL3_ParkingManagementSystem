namespace ParkingManagement.BLL.DTOs
{
    // ── Old DTOs - kept for backward compatibility ────────────────
    public class MonthlyTicketDto
    {
        public string MonthlyTicketId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string PackageType { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = null!;
        public int DaysRemaining { get; set; }
    }

    public class ReservationDto
    {
        public string ReservationId { get; set; } = null!;
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string? SlotId { get; set; }
        public string? SlotLocation { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = null!;    // Chờ / Đã nhận / Hủy / Hết hạn
    }

    /// <summary>
    /// UC005.1 - Filter và phân trang danh sách đặt chỗ
    /// </summary>
    public class FilterReservationDto
    {
        public string? Status { get; set; }
        public string? VehiclePlate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// UC005.1 - Kết quả danh sách đặt chỗ với phân trang
    /// </summary>
    public class ListReservationDto
    {
        public List<ReservationDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// UC005.2 - Kết quả hủy đặt chỗ
    /// </summary>
    public class CancelReservationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string ReservationId { get; set; } = null!;
        public string NewStatus { get; set; } = "Hủy";
    }

    // ── Manager Ticket Management DTOs ────────────────────────
    // UC016 - Xem danh sách vé & chi tiết vé
    // UC017 - Quản lý giá vé

    /// <summary>
    /// UC016.1 - Filter danh sách vé
    /// </summary>
    public class TicketFilterDto
    {
        public string? Status { get; set; }           // "Đang trong bãi", "Đã ra", "Chờ thanh toán"
        public string? VehicleType { get; set; }      // "Xe máy", "Ô tô nhỏ", "Ô tô lớn"
        public DateTime? FromDate { get; set; }       // Từ ngày
        public DateTime? ToDate { get; set; }         // Đến ngày
        public string? SearchKeyword { get; set; }    // Biển số hoặc TicketId
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// UC016.1 - Item trong danh sách vé
    /// </summary>
    public class TicketListDto
    {
        public string TicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal? Fee { get; set; }             // null nếu chưa tính
        public string? SlotId { get; set; }
        public string? CustomerName { get; set; }     // null = vãng lai
    }

    /// <summary>
    /// UC016.1 - Danh sách vé với phân trang
    /// </summary>
    public class ListTicketDto
    {
        public List<TicketListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// UC017.1 - Lấy giá vé hiện tại
    /// </summary>
    public class PricingDto
    {
        // Giá theo giờ (VND/giờ)
        public Dictionary<string, decimal> HourlyRate { get; set; } = new();

        // Giá tối đa theo ngày (VND/ngày)
        public Dictionary<string, decimal> MaxDailyFee { get; set; } = new();

        // Giá vé tháng (VND)
        public Dictionary<string, MonthlyPricingDto> MonthlyTicketPrice { get; set; } = new();

        public DateTime LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }    // ManagerId
    }

    /// <summary>
    /// Thông tin giá vé tháng
    /// </summary>
    public class MonthlyPricingDto
    {
        public decimal OneMonth { get; set; }
        public decimal ThreeMonth { get; set; }
        public decimal SixMonth { get; set; }
    }

    /// <summary>
    /// UC017.2 - Cập nhật giá vé
    /// </summary>
    public class UpdatePricingDto
    {
        public Dictionary<string, decimal>? HourlyRate { get; set; }
        public Dictionary<string, decimal>? MaxDailyFee { get; set; }
        public Dictionary<string, UpdateMonthlyPricingDto>? MonthlyTicketPrice { get; set; }
    }

    /// <summary>
    /// Thông tin cập nhật giá vé tháng
    /// </summary>
    public class UpdateMonthlyPricingDto
    {
        public decimal? OneMonth { get; set; }
        public decimal? ThreeMonth { get; set; }
        public decimal? SixMonth { get; set; }
    }

    // ── Employee Ticket Management DTOs ──────────────────────
    // UC006 - Check-out (Lấy xe, tính tiền)
    // UC008 - Check-in (Gửi xe)
    // UC-EMP01 - Danh sách vé & tìm kiếm

    /// <summary>
    /// UC008 - Check-in Validate Request
    /// </summary>
    public class CheckInValidateRequestDto
    {
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
    }

    /// <summary>
    /// UC008 - Check-in Validate Response
    /// </summary>
    public class CheckInValidateResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? CustomerName { get; set; }     // Nếu là khách hàng đã đăng ký
        public bool HasMonthlyTicket { get; set; }
        public List<ParkingSlotDto> AvailableSlots { get; set; } = new();
    }

    /// <summary>
    /// UC008 - Check-in Confirm Request
    /// </summary>
    public class CheckInConfirmRequestDto
    {
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string SlotId { get; set; } = null!;
        public string? CustomerId { get; set; }
    }

    /// <summary>
    /// UC008 - Check-in Confirm Result
    /// </summary>
    public class CheckInConfirmResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TicketId { get; set; }
        public string? SlotId { get; set; }
        public DateTime CheckInTime { get; set; }
    }

    /// <summary>
    /// UC006 - Check-out Info Request
    /// </summary>
    public class CheckOutInfoRequestDto
    {
        public string VehicleIdentifier { get; set; } = null!;  // Biển số hoặc TicketId
    }

    /// <summary>
    /// UC006 - Check-out Info Response
    /// </summary>
    public class CheckOutInfoDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string TicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Fee { get; set; }
        public bool HasMonthlyTicket { get; set; }
        public string? SlotId { get; set; }
    }

    /// <summary>
    /// UC006 - Check-out Confirm Request
    /// </summary>
    public class CheckOutConfirmRequestDto
    {
        public string TicketId { get; set; } = null!;
    }

    /// <summary>
    /// UC006 - Check-out Confirm Result
    /// </summary>
    public class CheckOutConfirmResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string TicketId { get; set; } = null!;
        public decimal Fee { get; set; }
        public DateTime CheckOutTime { get; set; }
    }

    /// <summary>
    /// UC-EMP01 - List Tickets Filter
    /// </summary>
    public class EmployeeTicketFilterDto
    {
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// UC-EMP01 - List Tickets Item
    /// </summary>
    public class EmployeeTicketListDto
    {
        public string TicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
        public string? CustomerName { get; set; }
    }

    /// <summary>
    /// UC-EMP01 - List Tickets with Pagination
    /// </summary>
    public class ListEmployeeTicketDto
    {
        public List<EmployeeTicketListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// UC-EMP01 - Search Tickets
    /// </summary>
    public class EmployeeTicketSearchDto
    {
        public string SearchKeyword { get; set; } = null!;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ── Customer Ticket Management DTOs ──────────────────────
    // UC-CUST01 - Danh sách vé, chi tiết, lịch sử thanh toán
    // UC-CUST02 - Quản lý vé tháng (xem, gia hạn)
    // UC-CUST03 - Danh sách đặt chỗ

    /// <summary>
    /// UC-CUST01 - Filter danh sách vé của khách hàng
    /// </summary>
    public class CustomerTicketFilterDto
    {
        public string? Status { get; set; }           // "Đang trong bãi", "Đã ra", "Chờ thanh toán"
        public string? VehicleType { get; set; }      // "Xe máy", "Ô tô nhỏ", "Ô tô lớn"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// UC-CUST01 - Item trong danh sách vé
    /// </summary>
    public class CustomerTicketListDto
    {
        public string TicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
    }

    /// <summary>
    /// UC-CUST01 - Danh sách vé với phân trang
    /// </summary>
    public class ListCustomerTicketDto
    {
        public List<CustomerTicketListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// UC-CUST01 - Chi tiết vé của khách hàng
    /// </summary>
    public class CustomerTicketDetailDto
    {
        public string TicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal? Fee { get; set; }
        public string? SlotId { get; set; }
        public int? DurationMinutes { get; set; }

        // Payment info
        public string? PaymentId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? PaymentTime { get; set; }

        // Monthly ticket
        public string? MonthlyTicketId { get; set; }
        public bool HasMonthlyTicket { get; set; }
    }

    /// <summary>
    /// UC-CUST01 - Filter lịch sử thanh toán
    /// </summary>
    public class CustomerPaymentFilterDto
    {
        public string? Status { get; set; }           // "Hoàn tất", "Chờ xác nhận", "Thất bại"
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// UC-CUST01 - Item trong lịch sử thanh toán
    /// </summary>
    public class CustomerPaymentListDto
    {
        public string PaymentId { get; set; } = null!;
        public string TicketId { get; set; } = null!;
        public string? VehiclePlate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    /// <summary>
    /// UC-CUST01 - Danh sách thanh toán với phân trang
    /// </summary>
    public class ListCustomerPaymentDto
    {
        public List<CustomerPaymentListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// UC-CUST02 - Danh sách vé tháng của khách hàng
    /// </summary>
    public class CustomerMonthlyTicketDto
    {
        public string MonthlyTicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string PackageType { get; set; } = null!;       // "1 tháng", "3 tháng", "6 tháng"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = null!;            // "Hoạt động", "Hết hạn", "Chờ thanh toán"
        public int DaysRemaining { get; set; }
    }

    /// <summary>
    /// UC-CUST02 - Danh sách vé tháng với tổng hợp
    /// </summary>
    public class ListCustomerMonthlyTicketDto
    {
        public List<CustomerMonthlyTicketDto> Items { get; set; } = new();
        public int TotalActiveTickets { get; set; }
    }

    /// <summary>
    /// UC-CUST02 - Chi tiết vé tháng
    /// </summary>
    public class CustomerMonthlyTicketDetailDto
    {
        public string MonthlyTicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string PackageType { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalFee { get; set; }
        public string Status { get; set; } = null!;
        public int DaysRemaining { get; set; }
        public int TotalCheckInCount { get; set; }
        public string AvailableCheckIns { get; set; } = "Unlimited";
    }

    /// <summary>
    /// UC-CUST02 - Kết quả gia hạn vé tháng
    /// </summary>
    public class RenewMonthlyTicketResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MonthlyTicketId { get; set; }
        public DateTime NewEndDate { get; set; }
        public decimal NewPrice { get; set; }
    }

    /// <summary>
    /// UC-CUST03 - Filter danh sách đặt chỗ
    /// </summary>
    public class CustomerReservationFilterDto
    {
        public string? Status { get; set; }           // "Chờ", "Đã nhận", "Hủy", "Hết hạn"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// UC-CUST03 - Item trong danh sách đặt chỗ
    /// </summary>
    public class CustomerReservationListDto
    {
        public string ReservationId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string? SlotId { get; set; }
        public DateTime ExpectedTime { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// UC-CUST03 - Danh sách đặt chỗ với phân trang
    /// </summary>
    public class ListCustomerReservationDto
    {
        public List<CustomerReservationListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// UC-CUST03 - Chi tiết đặt chỗ
    /// </summary>
    public class CustomerReservationDetailDto
    {
        public string ReservationId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string? SlotId { get; set; }
        public string? SlotLocation { get; set; }
        public DateTime ExpectedTime { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int TimeUntilExpectedMinutes { get; set; }
    }
}
