namespace ParkingManagement.FE.Models
{
    public class EmployeeSlotFilterDto
    {
        public string? VehicleType { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 200; // Get all basically
    }

    public class EmployeeSlotListItemDto
    {
        public string SlotId { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Location { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsAvailable { get; set; }
        public string? CurrentOccupant { get; set; }
        public DateTime? OccupiedSince { get; set; }
    }

    public class ListEmployeeSlotDto
    {
        public List<EmployeeSlotListItemDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        
        public int TotalEmpty { get; set; }
        public int TotalOccupied { get; set; }
        public int TotalMaintenance { get; set; }
        public double UtilizationRate { get; set; }
    }
}
