using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface ICustomerService
    {
        // Basic CRUD & Search
        Task<List<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(string customerId);
        Task<List<CustomerDto>> SearchAsync(string keyword);
        Task<CustomerSearchResultDto> SearchAdvancedAsync(CustomerSearchDto searchDto);
        Task<ServiceResult<string>> SoftDeleteAsync(string customerId);
        Task<List<CustomerDto>> GetDeletedAsync();
        Task<ServiceResult<string>> RestoreAsync(string customerId);

        // Advanced Features (Previously in EmployeeCustomerService)
        Task<ListEmployeeCustomerSearchDto> SearchCustomersAsync(EmployeeCustomerSearchFilterDto filter);
        Task<EmployeeCustomerDetailDto> GetCustomerDetailAsync(string customerId);
    }
}
