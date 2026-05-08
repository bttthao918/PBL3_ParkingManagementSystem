using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IParkingSlotService
    {
        // ── 1. General Slot Management ──
        Task<List<ParkingSlotDto>> GetAllAsync();
        Task<List<ParkingSlotDto>> GetAvailableAsync(string vehicleType);
        Task<ParkingSlotDto?> GetByIdAsync(string slotId);
        Task<ServiceResult<string>> UpdateStatusAsync(UpdateSlotStatusDto dto, string employeeId);
        Task<Dictionary<string, int>> GetSlotSummaryAsync();
        Task<List<dynamic>> GetAuditLogsAsync(string slotId);
        Task<List<dynamic>> GetEmployeeAuditLogsAsync(string employeeId, int days = 30);
        Task<bool> CanUpdateStatusAsync(string slotId, string newStatus);

        // ── 2. Manager Slot Management ──
        Task<ListParkingSlotDto> GetParkingSlotsAsync(ParkingSlotFilterDto filter);
        Task<ParkingSlotDetailDto> GetParkingSlotDetailAsync(string slotId);
        Task<ParkingSlotReportDto> GetParkingSlotReportAsync();
        Task<UpdateParkingSlotResultDto> UpdateParkingSlotAsync(UpdateParkingSlotDto request);

        // ── 3. Employee Slot Management (from IEmployeeSlotManagementService) ──
        Task<ListEmployeeSlotDto> GetEmployeeSlotsAsync(EmployeeSlotFilterDto filter);
        Task<EmployeeSlotDetailDto> GetEmployeeSlotDetailAsync(string slotId);
    }
}
