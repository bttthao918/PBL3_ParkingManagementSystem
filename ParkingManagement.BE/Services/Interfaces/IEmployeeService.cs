using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IEmployeeService
    {
        // ── 1. Basic Employee CRUD ──
        Task<List<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto?> GetByIdAsync(string employeeId);
        Task<List<EmployeeDto>> SearchAsync(string keyword);
        Task<ServiceResult<string>> CreateAsync(CreateEmployeeDto dto);
        Task<ServiceResult<string>> SoftDeleteAsync(string employeeId);
        Task<List<EmployeeDto>> GetDeletedAsync();
        Task<ServiceResult<string>> RestoreAsync(string employeeId);

        // ── 2. Manager Employee Management ──
        Task<ListManagerEmployeeDto> GetEmployeesAsync(ManagerEmployeeFilterDto filter);
        Task<ManagerEmployeeDetailDto> GetEmployeeDetailAsync(string employeeId);
        Task<CreateEmployeeInviteResultDto> CreateEmployeeInviteAsync(CreateEmployeeInviteByManagerDto request);
        Task<UpdateEmployeeResultDto> UpdateEmployeeAsync(string employeeId, UpdateEmployeeByManagerDto request);
        Task<DeleteEmployeeResultDto> DeleteEmployeeAsync(DeleteEmployeeDto request);

        // ── 3. Employee Invite Processing (from IEmployeeInviteService) ──
        Task<ServiceResult<EmployeeInviteDto>> CreateInviteAsync(CreateEmployeeInviteDto dto);
        Task<ServiceResult<EmployeeInviteDto>> GetInviteByTokenAsync(string token);
        Task<ConfirmEmployeeInviteResultDto> ConfirmInviteAsync(ConfirmEmployeeInviteDto request);
    }
}
