using System.Net.Http.Headers;
using System.Net.Http.Json;
<<<<<<< HEAD
using System.Net;
=======
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
using System.Text.Json;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public interface IReservationService
    {
        Task<ListReservationDto?> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<ListReservationDto?> GetForEmployeeAsync(
            string? searchKeyword = null,
            string? status = null,
            string? vehicleType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<ReservationDetailDto?> GetByIdAsync(string reservationId);
        Task<List<AvailableSlotDto>?> GetAvailableSlotsAsync(string? vehicleType = null);
<<<<<<< HEAD
        Task<ServiceResultDto<ReservationDetailDto>?> CreateAsync(CreateReservationDto dto);
        Task<ServiceResultDto<ReservationDetailDto>?> CreateForEmployeeAsync(CreateReservationDto dto);
=======
        Task<ServiceResultDto<ReservationDetailDto>> CreateAsync(CreateReservationDto dto);
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
        Task<ServiceResultDto?> CancelAsync(string reservationId);
        Task<ServiceResultDto?> CancelForEmployeeAsync(string reservationId);
    }

    public class ReservationService : IReservationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ReservationService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value
                ?? _httpContextAccessor.HttpContext?.Session.GetString("jwt_token")
                ?? _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        public async Task<ListReservationDto?> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/reservations?pageNumber={pageNumber}&pageSize={pageSize}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListReservationDto>();
                }
                _logger.LogWarning("GetAllAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetAllAsync");
                return null;
            }
        }

        public async Task<ListReservationDto?> GetForEmployeeAsync(
            string? searchKeyword = null,
            string? status = null,
            string? vehicleType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                AddAuthorizationHeader();

                var queryParams = new List<string>
                {
                    $"PageNumber={pageNumber}",
                    $"PageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    queryParams.Add($"SearchKeyword={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (!string.IsNullOrWhiteSpace(status))
                    queryParams.Add($"Status={Uri.EscapeDataString(status)}");
                if (!string.IsNullOrWhiteSpace(vehicleType))
                    queryParams.Add($"VehicleType={Uri.EscapeDataString(vehicleType)}");
                if (fromDate.HasValue)
                    queryParams.Add($"FromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
                if (toDate.HasValue)
                    queryParams.Add($"ToDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");

                var response = await _httpClient.GetAsync($"api/reservations/employee?{string.Join("&", queryParams)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListReservationDto>();
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetForEmployeeAsync failed: {StatusCode} {Error}", response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetForEmployeeAsync");
                return null;
            }
        }

        public async Task<ReservationDetailDto?> GetByIdAsync(string reservationId)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/reservations/{Uri.EscapeDataString(reservationId)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ReservationDetailDto>();
                }
                _logger.LogWarning("GetByIdAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetByIdAsync");
                return null;
            }
        }

        public async Task<List<AvailableSlotDto>?> GetAvailableSlotsAsync(string? vehicleType = null)
        {
            try
            {
                AddAuthorizationHeader();
                var url = "api/reservations/available-slots";
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    url += $"?vehicleType={Uri.EscapeDataString(vehicleType)}";
                }
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<AvailableSlotDto>>();
                }
                _logger.LogWarning("GetAvailableSlotsAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetAvailableSlotsAsync");
                return null;
            }
        }

<<<<<<< HEAD
        public async Task<ServiceResultDto<ReservationDetailDto>?> CreateAsync(CreateReservationDto dto)
=======
        public async Task<ServiceResultDto<ReservationDetailDto>> CreateAsync(CreateReservationDto dto)
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("api/reservations", dto);
                if (response.IsSuccessStatusCode)
                {
<<<<<<< HEAD
=======
                    var data = await response.Content.ReadFromJsonAsync<ReservationDetailDto>();
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
                    return new ServiceResultDto<ReservationDetailDto>
                    {
                        Success = true,
                        Message = "Đặt chỗ thành công!",
<<<<<<< HEAD
                        Data = await response.Content.ReadFromJsonAsync<ReservationDetailDto>()
=======
                        Data = data
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
                    };
                }

                var error = await response.Content.ReadAsStringAsync();
<<<<<<< HEAD
                _logger.LogWarning("CreateAsync failed: {StatusCode} {Error}", response.StatusCode, error);
                return ParseErrorResult(response.StatusCode, error, "Không thể đặt chỗ. Vui lòng kiểm tra thông tin xe và thời gian.");
=======
                var message = ExtractApiErrorMessage(error)
                    ?? "Không thể đặt chỗ. Vui lòng thử lại.";

                _logger.LogWarning("CreateAsync failed: {StatusCode} {Message} {Error}", response.StatusCode, message, error);
                return new ServiceResultDto<ReservationDetailDto>
                {
                    Success = false,
                    Message = message
                };
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CreateAsync");
                return new ServiceResultDto<ReservationDetailDto>
                {
                    Success = false,
<<<<<<< HEAD
                    Message = "Không gọi được backend. Kiểm tra BE đã chạy và FE đang trỏ đúng BackendApi:BaseUrl."
=======
                    Message = "Không thể kết nối đến hệ thống đặt chỗ. Vui lòng thử lại."
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
                };
            }
        }

<<<<<<< HEAD
        public async Task<ServiceResultDto<ReservationDetailDto>?> CreateForEmployeeAsync(CreateReservationDto dto)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("api/reservations/employee", dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ServiceResultDto<ReservationDetailDto>>();
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateForEmployeeAsync failed: {StatusCode} {Error}", response.StatusCode, errorBody);
                return ParseErrorResult(response.StatusCode, errorBody, "Không thể đặt chỗ. Vui lòng kiểm tra thông tin khách hàng, xe và thời gian.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CreateForEmployeeAsync");
                return new ServiceResultDto<ReservationDetailDto>
                {
                    Success = false,
                    Message = "Không gọi được backend. Kiểm tra BE đã chạy và FE đang trỏ đúng BackendApi:BaseUrl."
                };
            }
        }

        private static ServiceResultDto<ReservationDetailDto> ParseErrorResult(HttpStatusCode statusCode, string errorBody, string fallbackMessage)
        {
            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return new ServiceResultDto<ReservationDetailDto>
                {
                    Success = false,
                    Message = "Phiên đăng nhập backend đã hết hạn hoặc thiếu token. Vui lòng đăng xuất rồi đăng nhập lại."
                };
            }

            if (statusCode == HttpStatusCode.Forbidden)
            {
                return new ServiceResultDto<ReservationDetailDto>
                {
                    Success = false,
                    Message = "Tài khoản hiện tại không có quyền tạo đặt chỗ."
                };
            }

            if (string.IsNullOrWhiteSpace(errorBody))
            {
                return new ServiceResultDto<ReservationDetailDto> { Success = false, Message = fallbackMessage };
            }

            try
            {
                var result = JsonSerializer.Deserialize<ServiceResultDto<ReservationDetailDto>>(
                    errorBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (!string.IsNullOrWhiteSpace(result?.Message))
                {
                    result.Success = false;
                    return result;
                }
            }
            catch
            {
            }

            try
            {
                using var document = JsonDocument.Parse(errorBody);
                if (document.RootElement.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(message.GetString()))
                {
                    return new ServiceResultDto<ReservationDetailDto>
                    {
                        Success = false,
                        Message = message.GetString()!
                    };
                }

                if (TryGetValidationMessage(document.RootElement, out var validationMessage))
                {
                    return new ServiceResultDto<ReservationDetailDto>
                    {
                        Success = false,
                        Message = validationMessage
                    };
                }
            }
            catch
            {
            }

            return new ServiceResultDto<ReservationDetailDto>
            {
                Success = false,
                Message = $"{fallbackMessage} Backend trả về {(int)statusCode}."
            };
        }

        private static bool TryGetValidationMessage(JsonElement element, out string message)
        {
            message = string.Empty;

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("errors", out var errors) &&
                    TryGetValidationMessage(errors, out message))
                {
                    return true;
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in property.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String &&
                                !string.IsNullOrWhiteSpace(item.GetString()))
                            {
                                message = $"{property.Name}: {item.GetString()}";
                                return true;
=======
        private static string? ExtractApiErrorMessage(string? error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(error);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var message)
                        && message.ValueKind == JsonValueKind.String)
                    {
                        return message.GetString();
                    }

                    if (root.TryGetProperty("errors", out var errors)
                        && errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in errors.EnumerateObject())
                        {
                            if (property.Value.ValueKind == JsonValueKind.Array)
                            {
                                var first = property.Value.EnumerateArray().FirstOrDefault();
                                if (first.ValueKind == JsonValueKind.String)
                                    return first.GetString();
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
                            }
                        }
                    }

<<<<<<< HEAD
                    if (property.Value.ValueKind == JsonValueKind.Object &&
                        TryGetValidationMessage(property.Value, out var nestedMessage))
                    {
                        message = nestedMessage;
                        return true;
                    }
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(item.GetString()))
                    {
                        message = item.GetString()!;
                        return true;
                    }
                }
            }

            return false;
=======
                    if (root.TryGetProperty("title", out var title)
                        && title.ValueKind == JsonValueKind.String)
                    {
                        return title.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                return error;
            }

            return null;
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
        }

        public async Task<ServiceResultDto?> CancelAsync(string reservationId)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/reservations/{Uri.EscapeDataString(reservationId)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ServiceResultDto>();
                }
                _logger.LogWarning("CancelAsync failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CancelAsync");
                return null;
            }
        }

        public async Task<ServiceResultDto?> CancelForEmployeeAsync(string reservationId)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/reservations/employee/{Uri.EscapeDataString(reservationId)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ServiceResultDto>();
                }

                var error = await response.Content.ReadFromJsonAsync<ServiceResultDto>();
                _logger.LogWarning("CancelForEmployeeAsync failed: {StatusCode} {Message}", response.StatusCode, error?.Message);
                return error ?? new ServiceResultDto { Success = false, Message = "Không thể hủy đơn đặt chỗ." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CancelForEmployeeAsync");
                return null;
            }
        }
    }
}
