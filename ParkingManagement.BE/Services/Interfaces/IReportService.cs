using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IReportService
    {
        // ── 1. Basic Revenue Reports ──
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to);
        Task<List<MonthlyTicketDto>> GetExpiringSoonAsync(int days = 7);
        Task<int> CountActiveVehiclesAsync();

        // ── 2. Manager Dashboard & Reports (from IManagerReportService) ──
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<RevenueReportDto> GetRevenueReportAsync(RevenueReportFilterDto filter);
        Task<CustomerReportDto> GetCustomerReportAsync();

        // ── 3. Employee Reports (from IEmployeeReportService) ──
        Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(string employeeId);
        Task<ShiftAttendanceReportDto> GetShiftAttendanceReportAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<EmployeeRevenueReportDto> GetEmployeeRevenueReportAsync(string employeeId, string period = "month");
    }
}
