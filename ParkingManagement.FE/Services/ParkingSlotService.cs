// Services/ParkingSlotService.cs
using System.Net.Http.Json;
using System.Text.Json;

namespace ParkingManagement.FE.Services
{
    public interface IParkingSlotService
    {
        // Get slots
        Task<List<ParkingSlotDto>> GetAllSlotsAsync();
        Task<ParkingSlotDto?> GetSlotByIdAsync(string slotId);
        Task<ParkingSlotDetailDto?> GetSlotDetailAsync(string slotId);
        Task<List<ParkingSlotDto>> GetSlotsByStatusAsync(string status);
        Task<List<ParkingSlotDto>> GetSlotsByVehicleTypeAsync(string vehicleType);

        // Available slots
        Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(string? vehicleType = null);
        Task<int> GetAvailableSlotsCountAsync(string? vehicleType = null);

        // Status management
        Task<UpdateSlotStatusResponse?> UpdateSlotStatusAsync(UpdateSlotStatusRequest request);
        Task<bool> ValidateStatusTransitionAsync(string slotId, string newStatus);

        // Audit logs
        Task<List<ParkingSlotAuditLogDto>> GetSlotAuditLogsAsync(string slotId, int? days = null);
        Task<List<ParkingSlotAuditLogDto>> GetEmployeeAuditLogsAsync(string employeeId, int? days = null);

        // Statistics & Reports
        Task<ParkingSlotSummaryDto?> GetSlotSummaryAsync();
        Task<ParkingSlotStatisticsDto?> GetSlotStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ParkingSlotReportDto?> GetSlotReportAsync();

        // Manager operations
        Task<ListParkingSlotResponseDto?> GetParkingSlotsForManagerAsync(ParkingSlotFilterDto filter);
        Task<ParkingSlotDetailForManagerDto?> GetSlotDetailForManagerAsync(string slotId);
        Task<UpdateParkingSlotResponse?> UpdateParkingSlotForManagerAsync(string slotId, UpdateParkingSlotRequest request);

        // Employee operations
        Task<ListEmployeeSlotResponseDto?> GetParkingSlotsForEmployeeAsync(EmployeeSlotFilterDto filter);
        Task<EmployeeSlotDetailDto?> GetSlotDetailForEmployeeAsync(string slotId);

        // Maintenance
        Task<SetMaintenanceResponse?> SetSlotMaintenanceAsync(string slotId, string reason, string? note = null);
        Task<bool> ReleaseSlotFromMaintenanceAsync(string slotId);
    }

    public class ParkingSlotService : BaseHttpService, IParkingSlotService
    {
        private readonly ILogger<ParkingSlotService> _logger;

        public ParkingSlotService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ParkingSlotService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger;
        }

        #region Get Slots

        public async Task<List<ParkingSlotDto>> GetAllSlotsAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/parking-slots");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ParkingSlotDto>>() ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all slots");
                return new();
            }
        }

        public async Task<ParkingSlotDto?> GetSlotByIdAsync(string slotId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/parking-slots/{slotId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot by id: {SlotId}", slotId);
                return null;
            }
        }

        public async Task<ParkingSlotDetailDto?> GetSlotDetailAsync(string slotId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/parking-slots/{slotId}/detail");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotDetailDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot detail: {SlotId}", slotId);
                return null;
            }
        }

        public async Task<List<ParkingSlotDto>> GetSlotsByStatusAsync(string status)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/parking-slots?status={Uri.EscapeDataString(status)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ParkingSlotDto>>() ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slots by status: {Status}", status);
                return new();
            }
        }

        public async Task<List<ParkingSlotDto>> GetSlotsByVehicleTypeAsync(string vehicleType)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/parking-slots?vehicleType={Uri.EscapeDataString(vehicleType)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ParkingSlotDto>>() ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slots by vehicle type: {VehicleType}", vehicleType);
                return new();
            }
        }

        #endregion

        #region Available Slots

        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(string? vehicleType = null)
        {
            try
            {
                AttachToken();
                var url = "api/parking-slots/available";
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    url += $"?vehicleType={Uri.EscapeDataString(vehicleType)}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var slots = await response.Content.ReadFromJsonAsync<List<ParkingSlotDto>>();
                    return slots?.Select(s => new AvailableSlotDto
                    {
                        SlotId = s.SlotId,
                        Location = s.Location,
                        VehicleType = s.VehicleType
                    }).ToList() ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available slots");
                return new();
            }
        }

        public async Task<int> GetAvailableSlotsCountAsync(string? vehicleType = null)
        {
            try
            {
                AttachToken();
                var url = "api/parking-slots/available/count";
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    url += $"?vehicleType={Uri.EscapeDataString(vehicleType)}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AvailableCountResponse>();
                    return result?.Count ?? 0;
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available slots count");
                return 0;
            }
        }

        #endregion

        #region Status Management

        public async Task<UpdateSlotStatusResponse?> UpdateSlotStatusAsync(UpdateSlotStatusRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PatchAsJsonAsync($"api/parking-slots/{request.SlotId}/status", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UpdateSlotStatusResponse>();
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new UpdateSlotStatusResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Cập nhật trạng thái thất bại"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slot status for {SlotId}", request.SlotId);
                return new UpdateSlotStatusResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống. Vui lòng thử lại."
                };
            }
        }

        public async Task<bool> ValidateStatusTransitionAsync(string slotId, string newStatus)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/parking-slots/{slotId}/validate-transition?newStatus={Uri.EscapeDataString(newStatus)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ValidateTransitionResponse>();
                    return result?.IsValid ?? false;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating status transition");
                return false;
            }
        }

        #endregion

        #region Audit Logs

        public async Task<List<ParkingSlotAuditLogDto>> GetSlotAuditLogsAsync(string slotId, int? days = null)
        {
            try
            {
                AttachToken();
                var url = $"api/parking-slots/{slotId}/audit-logs";
                if (days.HasValue) url += $"?days={days.Value}";

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ParkingSlotAuditLogDto>>() ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for slot {SlotId}", slotId);
                return new();
            }
        }

        public async Task<List<ParkingSlotAuditLogDto>> GetEmployeeAuditLogsAsync(string employeeId, int? days = null)
        {
            try
            {
                AttachToken();
                var url = $"api/parking-slots/audit-logs/employee/{employeeId}";
                if (days.HasValue) url += $"?days={days.Value}";

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ParkingSlotAuditLogDto>>() ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for employee {EmployeeId}", employeeId);
                return new();
            }
        }

        #endregion

        #region Statistics & Reports

        public async Task<ParkingSlotSummaryDto?> GetSlotSummaryAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/parking-slots/summary");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotSummaryDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot summary");
                return null;
            }
        }

        public async Task<ParkingSlotStatisticsDto?> GetSlotStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                AttachToken();
                var queryParams = new List<string>();
                if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

                var url = "api/parking-slots/statistics";
                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotStatisticsDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot statistics");
                return null;
            }
        }

        public async Task<ParkingSlotReportDto?> GetSlotReportAsync()
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync("api/reports/parking-slots");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotReportDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot report");
                return null;
            }
        }

        #endregion

        #region Manager Operations

        public async Task<ListParkingSlotResponseDto?> GetParkingSlotsForManagerAsync(ParkingSlotFilterDto filter)
        {
            try
            {
                AttachToken();
                var queryString = BuildSlotFilterQuery(filter);
                var response = await _httpClient.GetAsync($"api/parking-slots/manager/list{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListParkingSlotResponseDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parking slots for manager");
                return null;
            }
        }

        public async Task<ParkingSlotDetailForManagerDto?> GetSlotDetailForManagerAsync(string slotId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/parking-slots/manager/{slotId}/detail");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ParkingSlotDetailForManagerDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot detail for manager: {SlotId}", slotId);
                return null;
            }
        }

        public async Task<UpdateParkingSlotResponse?> UpdateParkingSlotForManagerAsync(string slotId, UpdateParkingSlotRequest request)
        {
            try
            {
                AttachToken();
                request.SlotId = slotId;
                var response = await _httpClient.PutAsJsonAsync($"api/parking-slots/manager/{slotId}", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UpdateParkingSlotResponse>();
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new UpdateParkingSlotResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Cập nhật chỗ đỗ thất bại"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parking slot for manager: {SlotId}", slotId);
                return new UpdateParkingSlotResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống. Vui lòng thử lại."
                };
            }
        }

        #endregion

        #region Employee Operations

        public async Task<ListEmployeeSlotResponseDto?> GetParkingSlotsForEmployeeAsync(EmployeeSlotFilterDto filter)
        {
            try
            {
                AttachToken();
                var queryString = BuildEmployeeSlotFilterQuery(filter);
                var response = await _httpClient.GetAsync($"api/parking-slots/employee/list{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListEmployeeSlotResponseDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parking slots for employee");
                return null;
            }
        }

        public async Task<EmployeeSlotDetailDto?> GetSlotDetailForEmployeeAsync(string slotId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.GetAsync($"api/parking-slots/employee/{slotId}/detail");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeeSlotDetailDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot detail for employee: {SlotId}", slotId);
                return null;
            }
        }

        #endregion

        #region Maintenance

        public async Task<SetMaintenanceResponse?> SetSlotMaintenanceAsync(string slotId, string reason, string? note = null)
        {
            try
            {
                AttachToken();
                var request = new SetMaintenanceRequest
                {
                    SlotId = slotId,
                    Reason = reason,
                    Note = note
                };

                var response = await _httpClient.PostAsJsonAsync($"api/parking-slots/{slotId}/maintenance", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<SetMaintenanceResponse>();
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new SetMaintenanceResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Đặt chế độ bảo trì thất bại"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting slot maintenance for {SlotId}", slotId);
                return new SetMaintenanceResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống. Vui lòng thử lại."
                };
            }
        }

        public async Task<bool> ReleaseSlotFromMaintenanceAsync(string slotId)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.DeleteAsync($"api/parking-slots/{slotId}/maintenance");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing slot from maintenance: {SlotId}", slotId);
                return false;
            }
        }

        #endregion

        #region Private Helpers

        private string BuildSlotFilterQuery(ParkingSlotFilterDto filter)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(filter.Status)) queryParams.Add($"status={Uri.EscapeDataString(filter.Status)}");
            if (!string.IsNullOrEmpty(filter.VehicleType)) queryParams.Add($"vehicleType={Uri.EscapeDataString(filter.VehicleType)}");
            if (!string.IsNullOrEmpty(filter.Location)) queryParams.Add($"location={Uri.EscapeDataString(filter.Location)}");
            if (!string.IsNullOrEmpty(filter.SearchKeyword)) queryParams.Add($"search={Uri.EscapeDataString(filter.SearchKeyword)}");
            queryParams.Add($"pageNumber={filter.PageNumber}");
            queryParams.Add($"pageSize={filter.PageSize}");
            return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        }

        private string BuildEmployeeSlotFilterQuery(EmployeeSlotFilterDto filter)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(filter.VehicleType)) queryParams.Add($"vehicleType={Uri.EscapeDataString(filter.VehicleType)}");
            if (!string.IsNullOrEmpty(filter.Status)) queryParams.Add($"status={Uri.EscapeDataString(filter.Status)}");
            if (!string.IsNullOrEmpty(filter.Location)) queryParams.Add($"location={Uri.EscapeDataString(filter.Location)}");
            queryParams.Add($"pageNumber={filter.PageNumber}");
            queryParams.Add($"pageSize={filter.PageSize}");
            return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        }

        #endregion
    }
}