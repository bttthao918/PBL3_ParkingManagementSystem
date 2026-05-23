using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IEmployeeService
    {
        bool LastRequestUnauthorized { get; }
        Task<ListManagerEmployeeDto?> GetEmployeesAsync(ManagerEmployeeFilterDto filter);
        Task<ManagerEmployeeDetailDto?> GetEmployeeDetailAsync(string employeeId);
        Task<CreateEmployeeInviteResultDto?> CreateEmployeeInviteAsync(CreateEmployeeInviteByManagerDto dto);
        Task<EmployeeInviteApiResultDto?> GetEmployeeInviteAsync(string token);
        Task<ConfirmEmployeeInviteResultDto?> ConfirmEmployeeInviteAsync(ConfirmEmployeeInviteDto dto);
        Task<UpdateEmployeeResultDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeByManagerDto dto);
        Task<DeleteEmployeeResultDto?> DeleteEmployeeAsync(DeleteEmployeeDto dto);
        Task<UpdateEmployeeResultDto?> RestoreEmployeeAsync(string employeeId);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmployeeService> _logger;
        public bool LastRequestUnauthorized { get; private set; }

        public EmployeeService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EmployeeService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ListManagerEmployeeDto?> GetEmployeesAsync(ManagerEmployeeFilterDto filter)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();
                
                var url = $"api/employees/manager/list?PageNumber={filter.PageNumber}&PageSize={filter.PageSize}";
                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                {
                    url += $"&SearchKeyword={Uri.EscapeDataString(filter.SearchKeyword)}";
                }
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    url += $"&Status={Uri.EscapeDataString(filter.Status)}";
                }
                if (!string.IsNullOrWhiteSpace(filter.Shift))
                {
                    url += $"&Shift={Uri.EscapeDataString(filter.Shift)}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListManagerEmployeeDto>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Employee API call failed: {StatusCode} {Url} {Body}", response.StatusCode, url, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetEmployeesAsync");
                return null;
            }
        }

        public async Task<CreateEmployeeInviteResultDto?> CreateEmployeeInviteAsync(CreateEmployeeInviteByManagerDto dto)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();
                var response = await _httpClient.PostAsJsonAsync("api/employees/manager/invite", dto);
                CreateEmployeeInviteResultDto? result = null;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<CreateEmployeeInviteResultDto>();
                }
                catch
                {
                    // Fallback when API returns non-JSON body (401/403/500 middleware responses)
                }

                if (response.IsSuccessStatusCode)
                {
                    return result;
                }

                _logger.LogWarning("CreateEmployeeInviteAsync failed: {StatusCode}", response.StatusCode);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new CreateEmployeeInviteResultDto { Success = false, Message = "Bạn không có quyền thêm nhân viên." };
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new CreateEmployeeInviteResultDto { Success = false, Message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." };
                }
                return result ?? new CreateEmployeeInviteResultDto
                {
                    Success = false,
                    Message = $"Không thể tạo nhân viên. Mã lỗi: {(int)response.StatusCode}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CreateEmployeeInviteAsync");
                return new CreateEmployeeInviteResultDto
                {
                    Success = false,
                    Message = "Lỗi kết nối đến máy chủ."
                };
            }
        }

        public async Task<ManagerEmployeeDetailDto?> GetEmployeeDetailAsync(string employeeId)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();

                var response = await _httpClient.GetAsync($"api/employees/manager/{Uri.EscapeDataString(employeeId)}/detail");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ManagerEmployeeDetailDto>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetEmployeeDetailAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetEmployeeDetailAsync");
                return null;
            }
        }

        public async Task<EmployeeInviteApiResultDto?> GetEmployeeInviteAsync(string token)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();

                var response = await _httpClient.GetAsync($"api/employees/invite/{Uri.EscapeDataString(token)}");
                EmployeeInviteApiResultDto? result = null;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<EmployeeInviteApiResultDto>();
                }
                catch
                {
                    // Fallback when API returns non-JSON body.
                }

                if (response.IsSuccessStatusCode)
                {
                    return result;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetEmployeeInviteAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
                return result ?? new EmployeeInviteApiResultDto
                {
                    Success = false,
                    Message = "Link mời không hợp lệ hoặc đã hết hạn."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetEmployeeInviteAsync");
                return new EmployeeInviteApiResultDto
                {
                    Success = false,
                    Message = "Lỗi kết nối đến máy chủ."
                };
            }
        }

        public async Task<ConfirmEmployeeInviteResultDto?> ConfirmEmployeeInviteAsync(ConfirmEmployeeInviteDto dto)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();

                var response = await _httpClient.PostAsJsonAsync("api/employees/invite/confirm", dto);
                ConfirmEmployeeInviteResultDto? result = null;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<ConfirmEmployeeInviteResultDto>();
                }
                catch
                {
                    // Fallback when API returns non-JSON body.
                }

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new ConfirmEmployeeInviteResultDto
                    {
                        Success = true,
                        Message = "Hoàn tất tài khoản thành công."
                    };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("ConfirmEmployeeInviteAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
                return result ?? new ConfirmEmployeeInviteResultDto
                {
                    Success = false,
                    Message = "Không thể hoàn tất tài khoản."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ConfirmEmployeeInviteAsync");
                return new ConfirmEmployeeInviteResultDto
                {
                    Success = false,
                    Message = "Lỗi kết nối đến máy chủ."
                };
            }
        }

        public async Task<UpdateEmployeeResultDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeByManagerDto dto)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();

                var response = await _httpClient.PutAsJsonAsync($"api/employees/manager/{Uri.EscapeDataString(employeeId)}", dto);
                UpdateEmployeeResultDto? result = null;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<UpdateEmployeeResultDto>();
                }
                catch
                {
                    // Fallback when API returns non-JSON body.
                }

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new UpdateEmployeeResultDto { Success = true, Message = "Đã cập nhật nhân viên." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("UpdateEmployeeAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
                return result ?? new UpdateEmployeeResultDto { Success = false, Message = "Không thể cập nhật nhân viên." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling UpdateEmployeeAsync");
                return new UpdateEmployeeResultDto { Success = false, Message = "Lỗi kết nối đến máy chủ." };
            }
        }

        public async Task<DeleteEmployeeResultDto?> DeleteEmployeeAsync(DeleteEmployeeDto dto)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();

                var response = await _httpClient.PostAsJsonAsync("api/employees/manager/delete", dto);
                DeleteEmployeeResultDto? result = null;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<DeleteEmployeeResultDto>();
                }
                catch
                {
                    // Fallback when API returns non-JSON body.
                }

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new DeleteEmployeeResultDto { Success = true, EmployeeId = dto.EmployeeId, Message = "Đã xóa nhân viên." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("DeleteEmployeeAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
                return result ?? new DeleteEmployeeResultDto { Success = false, EmployeeId = dto.EmployeeId, Message = "Không thể xóa nhân viên." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling DeleteEmployeeAsync");
                return new DeleteEmployeeResultDto { Success = false, EmployeeId = dto.EmployeeId, Message = "Lỗi kết nối đến máy chủ." };
            }
        }

        public async Task<UpdateEmployeeResultDto?> RestoreEmployeeAsync(string employeeId)
        {
            try
            {
                LastRequestUnauthorized = false;
                AttachBearerToken();

                var response = await _httpClient.PostAsync($"api/employees/manager/{Uri.EscapeDataString(employeeId)}/restore", null);
                UpdateEmployeeResultDto? result = null;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<UpdateEmployeeResultDto>();
                }
                catch
                {
                    // Fallback when API returns non-JSON body.
                }

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new UpdateEmployeeResultDto { Success = true, Message = "Đã khôi phục nhân viên." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LastRequestUnauthorized = true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("RestoreEmployeeAsync failed: {StatusCode} {Body}", response.StatusCode, errorContent);
                return result ?? new UpdateEmployeeResultDto { Success = false, Message = "Không thể khôi phục nhân viên." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling RestoreEmployeeAsync");
                return new UpdateEmployeeResultDto { Success = false, Message = "Lỗi kết nối đến máy chủ." };
            }
        }

        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
