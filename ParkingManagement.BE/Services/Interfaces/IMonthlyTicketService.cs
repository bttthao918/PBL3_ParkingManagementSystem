using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IMonthlyTicketService
    {
        Task<List<MonthlyTicketDto>> GetAllAsync();
        Task<List<MonthlyTicketDto>> GetByCustomerIdAsync(string customerId);
        Task<MonthlyTicketDto?> GetByIdAsync(string id);
        Task<ServiceResult<MonthlyTicketDto>> RegisterAsync(RegisterMonthlyTicketDto dto);
        Task<ServiceResult<MonthlyTicketDto>> RenewAsync(string monthlyTicketId, RenewMonthlyTicketDto dto);
        Task<ServiceResult<string>> CancelAsync(string monthlyTicketId);
        Task<List<MonthlyTicketDto>> GetExpiringSoonAsync(int days = 7);
        Task<decimal> CalculateFeeAsync(string vehicleType, string packageType);
    }
}
