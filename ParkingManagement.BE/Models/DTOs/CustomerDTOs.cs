namespace ParkingManagement.BLL.DTOs
{
    // ════════════════════════════════════════════════════════════
    // UC019.1 - Tạo/Invite Nhân Viên
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO cho quản lý tạo invite nhân viên
    /// </summary>
    public class CreateEmployeeInviteDto
    {
        public string Email { get; set; } = null!;              // Email nhân viên
        public string FullName { get; set; } = null!;           // Họ tên
        public string PhoneNumber { get; set; } = null!;        // Số điện thoại
        public string? Shift { get; set; }                      // Sáng / Chiều / Tối
    }

    /// <summary>
    /// DTO khi gửi invite (kèm token)
    /// </summary>
    public class EmployeeInviteDto
    {
        public string EmployeeCode { get; set; } = null!;       // Mã nhân viên
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string InviteToken { get; set; } = null!;        // Token xác thực
        public DateTime InviteExpiry { get; set; }              // Hết hạn invite (24h)
    }

    /// <summary>
    /// DTO khi nhân viên đăng ký (sau khi click link invite)
    /// </summary>
    public class EmployeeRegisterDto
    {
        public string InviteToken { get; set; } = null!;        // Token từ link invite
        public string EmployeeCode { get; set; } = null!;       // Mã nhân viên (từ invite)
        public string FullName { get; set; } = null!;           // Nhân viên tự điền
        public string PhoneNumber { get; set; } = null!;        // Nhân viên tự điền
        public string Gender { get; set; } = null!;             // Nhân viên tự điền
        public string? Shift { get; set; }                      // Nhân viên tự điền
        public string Password { get; set; } = null!;           // Nhân viên tự đặt
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// DTO response sau khi đăng ký thành công
    /// </summary>
    public class EmployeeRegistrationResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Email { get; set; }
        public string? EmployeeId { get; set; }
    }

    // ════════════════════════════════════════════════════════════
    // UC003 - Quản Lý Tài Khoản (Khách Hàng)
    // ════════════════════════════════════════════════════════════

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Gender { get; set; }
    }

    public class CustomerProfileDto
    {
        public string CustomerId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ════════════════════════════════════════════════════════════
    // UC003 - Quản Lý Tài Khoản (Nhân Viên)
    // ════════════════════════════════════════════════════════════

    public class EmployeeProfileDto
    {
        public string EmployeeId { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;          // Readonly
        public string? PhoneNumber { get; set; }               // Readonly
        public string? Gender { get; set; }                    // Có thể sửa lần đầu
        public string? Shift { get; set; }                     // Readonly
    }

    public class UpdateEmployeeGenderDto
    {
        public string Gender { get; set; } = null!;            // Male, Female, Other
    }

    // ════════════════════════════════════════════════════════════
    // UC003 - Quản Lý Tài Khoản (Quản Lý)
    // ════════════════════════════════════════════════════════════

    public class ManagerProfileDto
    {
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateManagerProfileDto
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Gender { get; set; }
    }

    // ════════════════════════════════════════════════════════════
    // UC013 - Tìm Kiếm / Quản Lý Khách Hàng
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO cho tìm kiếm chi tiết khách hàng
    /// </summary>
    public class CustomerSearchDto
    {
        public string? FullName { get; set; }                   // Tên khách hàng (tìm gần đúng)
        public string? PhoneNumber { get; set; }                // Số điện thoại
        public string? Email { get; set; }                      // Email
        public string? VehiclePlate { get; set; }               // Biển số xe
    }

    /// <summary>
    /// DTO response cho danh sách kết quả tìm kiếm
    /// </summary>
    public class CustomerSearchResultDto
    {
        public int TotalResults { get; set; }                   // Tổng số kết quả tìm được
        public int DisplayedResults { get; set; }               // Số kết quả hiển thị (max 50)
        public List<CustomerDto> Customers { get; set; } = new();
        public string? Message { get; set; }                    // Thông báo (nếu có)
    }

    // ════════════════════════════════════════════════════════════
    // UC004 - Lịch Sử Giữ Xe
    // ════════════════════════════════════════════════════════════

    public class ParkingHistoryDto
    {
        public string TicketId { get; set; } = null!;
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? Fee { get; set; }
        public string Status { get; set; } = null!; // "Đang trong bãi" / "Đã ra"
    }

    public class CustomerDto
    {
        public string CustomerId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> VehiclePlates { get; set; } = new();
    }
}
