namespace ParkingManagement.BLL.DTOs
{
    // ── Manager Employee Management DTOs ─────────────────────
    // UC-MGR-EMP01 - Xem danh sách nhân viên
    // UC-MGR-EMP02 - Xem chi tiết + thống kê nhân viên
    // UC-MGR-EMP03 - Thêm nhân viên (invite)
    // UC-MGR-EMP04 - Cập nhật nhân viên
    // UC-MGR-EMP05 - Xóa nhân viên

    /// <summary>
    /// UC-MGR-EMP01 - Filter danh sách nhân viên
    /// </summary>
    public class ManagerEmployeeFilterDto
    {
        public string? Status { get; set; }           // "Hoạt động", "Vô hiệu hóa"
        public string? Shift { get; set; }            // "Sáng", "Chiều", "Tối"
        public string? SearchKeyword { get; set; }    // Tên hoặc email
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// UC-MGR-EMP01 - Item trong danh sách nhân viên
    /// </summary>
    public class ManagerEmployeeListDto
    {
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Shift { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// UC-MGR-EMP01 - Danh sách nhân viên với phân trang
    /// </summary>
    public class ListManagerEmployeeDto
    {
        public List<ManagerEmployeeListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
    }

    /// <summary>
    /// UC-MGR-EMP02 - Chi tiết nhân viên + thống kê
    /// </summary>
    public class ManagerEmployeeDetailDto
    {
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Shift { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Thống kê công việc
        public int TotalTicketsProcessed { get; set; }  // Tổng vé xử lý
        public int TicketsProcessedToday { get; set; }  // Vé xử lý hôm nay
        public int TicketsProcessedThisMonth { get; set; }  // Vé xử lý tháng này
        public DateTime? FirstWorkDay { get; set; }  // Ngày đầu làm việc
        public int WorkDaysCount { get; set; }      // Tổng ngày làm việc
    }

    /// <summary>
    /// UC-MGR-EMP03 - Tạo invite nhân viên
    /// </summary>
    public class CreateEmployeeInviteByManagerDto
    {
        public string Email { get; set; } = null!;
        public string? Shift { get; set; }
    }

    /// <summary>
    /// UC-MGR-EMP03 - Kết quả tạo invite
    /// </summary>
    public class CreateEmployeeInviteResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? EmployeeCode { get; set; }
        public string? InviteToken { get; set; }
        public DateTime? InviteExpiry { get; set; }
    }

    /// <summary>
    /// UC-MGR-EMP04 - Cập nhật thông tin nhân viên
    /// </summary>
    public class UpdateEmployeeByManagerDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Shift { get; set; }
        public string? Status { get; set; }  // "Hoạt động", "Vô hiệu hóa"
    }

    /// <summary>
    /// UC-MGR-EMP04 - Kết quả cập nhật
    /// </summary>
    public class UpdateEmployeeResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// UC-MGR-EMP05 - Xóa (vô hiệu hóa) nhân viên
    /// </summary>
    public class DeleteEmployeeDto
    {
        public string EmployeeId { get; set; } = null!;
        public string? Reason { get; set; }
    }

    /// <summary>
    /// UC-MGR-EMP05 - Kết quả xóa
    /// </summary>
    public class DeleteEmployeeResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string EmployeeId { get; set; } = null!;
        public string NewStatus { get; set; } = "Vô hiệu hóa";
    }

    // ── Employee Invite Confirm DTOs ───────────────────────
    /// <summary>
    /// Employee xác nhận invite: nhập thông tin cá nhân
    /// </summary>
    public class ConfirmEmployeeInviteDto
    {
        public string InviteToken { get; set; } = null!;
        public string FullName { get; set; } = null!;     // Min 3, max 100
        public string PhoneNumber { get; set; } = null!;  // 10+ digits
        public string Password { get; set; } = null!;     // Min 6, letters+numbers+special
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// Kết quả xác nhận invite
    /// </summary>
    public class ConfirmEmployeeInviteResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
    }
}
