namespace ParkingManagement.BLL.DTOs
{
    // ── Employee Parking Slot Management DTOs ──────────────
    // UC-EMP-SLOT01 - View available slots
    // UC-EMP-SLOT02 - Slot detail + status

    /// <summary>
    /// UC-EMP-SLOT01 - Filter slots (khi check-in)
    /// </summary>
    public class EmployeeSlotFilterDto
    {
        public string? VehicleType { get; set; }  // "Xe máy", "Ô tô nhỏ", "Ô tô lớn"
        public string? Status { get; set; }       // "Trống", "Đang sử dụng", "Bảo trì"
        public string? Location { get; set; }     // Khu vực
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// UC-EMP-SLOT01 - Item trong danh sách chỗ đỗ
    /// </summary>
    public class EmployeeSlotListItemDto
    {
        public string SlotId { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public string? CurrentOccupant { get; set; }
        public DateTime? OccupiedSince { get; set; }
    }

    /// <summary>
    /// UC-EMP-SLOT01 - Danh sách chỗ đỗ
    /// </summary>
    public class ListEmployeeSlotDto
    {
        public List<EmployeeSlotListItemDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        
        // Summary
        public int TotalEmpty { get; set; }
        public int TotalOccupied { get; set; }
        public int TotalMaintenance { get; set; }
        public double UtilizationRate { get; set; }  // %
    }

    /// <summary>
    /// UC-EMP-SLOT02 - Chi tiết chỗ đỗ
    /// </summary>
    public class EmployeeSlotDetailDto
    {
        public string SlotId { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Status { get; set; } = null!;
        
        // Current Status
        public bool IsOccupied { get; set; }
        public string? CurrentVehiclePlate { get; set; }
        public string? CurrentCustomerName { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public int? CurrentOccupancyMinutes { get; set; }
        
        // Statistics
        public int UsageThisMonth { get; set; }
        public int UsageThisWeek { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public double AverageOccupancyTime { get; set; }  // Minutes
    }
}
