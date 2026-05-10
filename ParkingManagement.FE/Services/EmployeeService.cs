using System.Net.Http.Json;
using ParkingManagement.FE.Models.Employee;

namespace ParkingManagement.FE.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(string employeeId);
        Task<(bool Success, string Message)> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<(bool Success, string Message)> UpdateEmployeeAsync(string employeeId, UpdateEmployeeRequest request);
        Task<(bool Success, string Message)> DeleteEmployeeAsync(string employeeId);
        Task<(bool Success, string Message)> RestoreEmployeeAsync(string employeeId);
    }

    public class EmployeeService : BaseHttpService, IEmployeeService
    {
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<EmployeeService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
        {
            try
            {
                return await GetListAsync<EmployeeDto>("api/employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees");
                return new List<EmployeeDto>();
            }
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(string employeeId)
        {
            try
            {
                return await GetFromJsonAsync<EmployeeDto>($"api/employees/{employeeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee: {Id}", employeeId);
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/employees", request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Thêm nhân viên thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Thêm nhân viên thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateEmployeeAsync(string employeeId, UpdateEmployeeRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/employees/manager/{employeeId}", request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Cập nhật nhân viên thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Cập nhật thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee: {Id}", employeeId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string Message)> DeleteEmployeeAsync(string employeeId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/employees/{employeeId}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Xóa nhân viên thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Xóa nhân viên thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee: {Id}", employeeId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string Message)> RestoreEmployeeAsync(string employeeId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, $"api/employees/{employeeId}/restore");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Khôi phục nhân viên thành công!");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Message ?? "Khôi phục thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring employee: {Id}", employeeId);
                return (false, "Lỗi hệ thống. Vui lòng thử lại.");
            }
        }
    }
}