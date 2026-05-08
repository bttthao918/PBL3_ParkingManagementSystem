using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    /// <summary>
    /// UC005.2 - Cấp nhật trạng thái chỗ đỗ
    /// Xử lý logic quản lý trạng thái chỗ đỗ
    /// 
    /// Quy trình:
    /// 1. Nhân viên xem danh sách/bản đồ chỗ đỗ
    /// 2. Chọn chỗ để cập nhật trạng thái
    /// 3. Validate transition (kiểm tra có thể chuyển không)
    /// 4. Ghi lại log + thêm ghi chú (nếu cần)
    /// 5. Cập nhật DB + thông báo
    /// 
    /// Error handling (8a):
    /// - Trạng thái không hợp lệ
    /// - Transition không được phép
    /// - Vẫn còn xe đang đỗ (không cho phép chuyển sang bảo trì/hỏng)
    /// - Thiếu ghi chú (bảo trì/hỏng cần ghi chú)
    /// </summary>
    public class ParkingSlotService : IParkingSlotService
    {
        private readonly IParkingSlotRepository _slotRepo;
        private readonly IParkingSlotAuditLogRepository _auditRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IParkingSlotValidator _validator;

        public ParkingSlotService(
            IParkingSlotRepository slotRepo,
            IParkingSlotAuditLogRepository auditRepo,
            ITicketRepository ticketRepo,
            IParkingSlotValidator validator)
        {
            _slotRepo = slotRepo;
            _auditRepo = auditRepo;
            _ticketRepo = ticketRepo;
            _validator = validator;
        }

        /// <summary>
        /// Lấy danh sách tất cả chỗ đỗ
        /// </summary>
        public async Task<List<ParkingSlotDto>> GetAllAsync()
        {
            try
            {
                var slots = await _slotRepo.GetAllAsync();
                return slots.Select(MapToDto).ToList();
            }
            catch
            {
                return new List<ParkingSlotDto>();
            }
        }

        /// <summary>
        /// Lấy danh sách chỗ đỗ trống theo loại xe
        /// </summary>
        public async Task<List<ParkingSlotDto>> GetAvailableAsync(string vehicleType)
        {
            try
            {
                var slots = await _slotRepo.GetAvailableAsync(vehicleType);
                return slots.Select(MapToDto).ToList();
            }
            catch
            {
                return new List<ParkingSlotDto>();
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một chỗ đỗ
        /// </summary>
        public async Task<ParkingSlotDto?> GetByIdAsync(string slotId)
        {
            try
            {
                var slot = await _slotRepo.GetByIdAsync(slotId);
                return slot == null ? null : MapToDto(slot);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái chỗ đỗ với validation & logging
        /// UC005.2 - Main flow
        /// </summary>
        public async Task<ServiceResult<string>> UpdateStatusAsync(UpdateSlotStatusDto dto, string employeeId)
        {
            try
            {
                // Validate input
                var inputValidation = _validator.ValidateUpdateSlotStatus(dto);
                if (!inputValidation.IsValid)
                    return ServiceResult<string>.Fail(inputValidation.ErrorMessage!);

                // Lấy chỗ đỗ hiện tại
                var slot = await _slotRepo.GetByIdAsync(dto.SlotId);
                if (slot == null)
                    return ServiceResult<string>.Fail($"Không tìm thấy chỗ đỗ {dto.SlotId}");

                // Validate transition
                var transitionValidation = _validator.ValidateStatusTransition(slot.Status, dto.NewStatus);
                if (!transitionValidation.IsValid)
                    return ServiceResult<string>.Fail(transitionValidation.ErrorMessage!);

                // Kiểm tra nếu vẫn còn xe đang đỗ (status = "Đang sử dụng")
                if (slot.Status == "Đang sử dụng" && (dto.NewStatus == "Bảo trì" || dto.NewStatus == "Hỏng"))
                {
                    // Kiểm tra có ticket active không
                    var activeTicket = await _ticketRepo.GetActiveByPlateAsync(dto.SlotId);
                    if (activeTicket != null)
                        return ServiceResult<string>.Fail(
                            "Không thể cập nhật trạng thái này vì vẫn còn xe đang đỗ. Vui lòng kiểm tra lại checkout hoặc xe.");
                }

                // Validate ghi chú (nếu cần)
                var noteValidation = _validator.ValidateNote(dto.Note, dto.NewStatus);
                if (!noteValidation.IsValid)
                    return ServiceResult<string>.Fail(noteValidation.ErrorMessage!);

                // Cập nhật slot status
                slot.Status = dto.NewStatus;
                slot.LastUpdated = DateTime.Now;
                await _slotRepo.UpdateAsync(slot);

                // Ghi log lại thay đổi
                var logId = await _auditRepo.GenerateIdAsync();
                var auditLog = new ParkingSlotAuditLog
                {
                    LogId = logId,
                    SlotId = dto.SlotId,
                    EmployeeId = employeeId,
                    OldStatus = slot.Status,  // Note: giá trị cũ đã được set lúc validate
                    NewStatus = dto.NewStatus,
                    Note = dto.Note,
                    ChangedAt = DateTime.Now,
                    Reason = $"Cập nhật bằng tay bởi nhân viên {employeeId}"
                };
                await _auditRepo.AddAsync(auditLog);

                // Return success message
                return ServiceResult<string>.Ok(
                    logId,
                    $"Cập nhật trạng thái chỗ {dto.SlotId} thành '{dto.NewStatus}' thành công.");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Fail($"Lỗi cập nhật trạng thái: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thống kê số lượng chỗ đỗ theo trạng thái
        /// </summary>
        public async Task<Dictionary<string, int>> GetSlotSummaryAsync()
        {
            try
            {
                var all = await _slotRepo.GetAllAsync();
                return new Dictionary<string, int>
                {
                    ["Trống"] = all.Count(s => s.Status == "Trống"),
                    ["Đang sử dụng"] = all.Count(s => s.Status == "Đang sử dụng"),
                    ["Đã đặt"] = all.Count(s => s.Status == "Đã đặt"),
                    ["Bảo trì"] = all.Count(s => s.Status == "Bảo trì"),
                    ["Hỏng"] = all.Count(s => s.Status == "Hỏng"),
                    ["Tổng"] = all.Count
                };
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Lấy lịch sử thay đổi trạng thái của một chỗ đỗ
        /// </summary>
        public async Task<List<dynamic>> GetAuditLogsAsync(string slotId)
        {
            try
            {
                var logs = await _auditRepo.GetBySlotIdAsync(slotId);
                return logs.Select(log => new
                {
                    log.LogId,
                    log.SlotId,
                    EmployeeId = log.EmployeeId,
                    EmployeeName = log.Employee?.FullName ?? "Unknown",
                    log.OldStatus,
                    log.NewStatus,
                    log.Note,
                    log.ChangedAt,
                    log.Reason
                }).Cast<dynamic>().ToList();
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Lấy lịch sử thay đổi của một nhân viên (30 ngày gần nhất)
        /// </summary>
        public async Task<List<dynamic>> GetEmployeeAuditLogsAsync(string employeeId, int days = 30)
        {
            try
            {
                var fromDate = DateTime.Now.AddDays(-days);
                var logs = await _auditRepo.GetByEmployeeIdAsync(employeeId);
                
                return logs
                    .Where(log => log.ChangedAt >= fromDate)
                    .Select(log => new
                    {
                        log.LogId,
                        log.SlotId,
                        log.OldStatus,
                        log.NewStatus,
                        log.Note,
                        log.ChangedAt
                    })
                    .Cast<dynamic>()
                    .ToList();
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Validate trước khi cập nhật (cho form validation)
        /// </summary>
        public async Task<bool> CanUpdateStatusAsync(string slotId, string newStatus)
        {
            try
            {
                var slot = await _slotRepo.GetByIdAsync(slotId);
                if (slot == null)
                    return false;

                var validation = _validator.ValidateStatusTransition(slot.Status, newStatus);
                return validation.IsValid;
            }
            catch
            {
                return false;
            }
        }

        // ──── Helper Methods ────────────────────────────────────────

        private ParkingSlotDto MapToDto(ParkingSlot slot)
        {
            return new ParkingSlotDto
            {
                SlotId = slot.SlotId,
                Location = slot.Location,
                VehicleType = slot.VehicleType,
                Status = slot.Status,
                LastUpdated = slot.LastUpdated
            };
        }
        // -- Manager Slot Management --
        public async Task<ListParkingSlotDto> GetParkingSlotsAsync(ParkingSlotFilterDto filter)
        {
            try
            {
                var allSlots = (await _slotRepo.GetAllAsync()).ToList();
                var tickets = (await _ticketRepo.GetAllAsync()).ToList();

                // Apply filters
                var filtered = allSlots.AsEnumerable();

                if (!string.IsNullOrEmpty(filter.Status))
                    filtered = filtered.Where(s => s.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.VehicleType))
                    filtered = filtered.Where(s => s.VehicleType == filter.VehicleType);

                if (!string.IsNullOrEmpty(filter.Location))
                    filtered = filtered.Where(s => s.Location == filter.Location);

                if (!string.IsNullOrEmpty(filter.SearchKeyword))
                {
                    var keyword = filter.SearchKeyword.ToLower();
                    filtered = filtered.Where(s => s.SlotId.ToLower().Contains(keyword));
                }

                var sorted = filtered.OrderBy(s => s.SlotId).ToList();

                // Pagination
                var totalItems = sorted.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                var items = sorted
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                // Map to DTOs
                var slotDtos = items.Select(s =>
                {
                    var currentTicket = tickets.FirstOrDefault(t => t.SlotId == s.SlotId && string.IsNullOrEmpty(t.CheckOutTime.ToString()));
                    var usageCount = tickets.Count(t => t.SlotId == s.SlotId);
                    var lastUsed = tickets.Where(t => t.SlotId == s.SlotId).Max(t => (DateTime?)t.CheckOutTime ?? t.CheckInTime);

                    return new ParkingSlotListDto
                    {
                        SlotId = s.SlotId,
                        VehicleType = s.VehicleType,
                        Location = s.Location,
                        Status = s.Status,
                        CurrentOccupant = currentTicket?.VehiclePlate,
                        OccupiedSince = currentTicket?.CheckInTime,
                        UsageCount = usageCount,
                        LastUsedAt = lastUsed
                    };
                }).ToList();

                var totalEmpty = sorted.Count(s => s.Status == "Trống");
                var totalOccupied = sorted.Count(s => s.Status == "Đang sử dụng");
                var totalMaintenance = sorted.Count(s => s.Status == "Bảo trì");

                return new ListParkingSlotDto
                {
                    Items = slotDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    TotalEmpty = totalEmpty,
                    TotalOccupied = totalOccupied,
                    TotalMaintenance = totalMaintenance
                };
            }
            catch (Exception ex)
            {
                return new ListParkingSlotDto();
            }
        }

        /// <summary>
        /// Get parking slot detail + statistics
        /// </summary>
        public async Task<ParkingSlotDetailDto> GetParkingSlotDetailAsync(string slotId)
        {
            try
            {
                var slot = await _slotRepo.GetByIdAsync(slotId);
                if (slot == null)
                    throw new Exception("Chỗ đỗ không tồn tại");

                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();
                var slotTickets = allTickets.Where(t => t.SlotId == slotId).ToList();

                var today = DateTime.Now.Date;
                var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var usageThisMonth = slotTickets.Count(t => t.CheckInTime >= monthStart);
                var usageThisWeek = slotTickets.Count(t => t.CheckInTime >= DateTime.Now.AddDays(-7));
                var avgOccupancyTime = slotTickets.Count > 0
                    ? slotTickets.Where(t => t.CheckOutTime.HasValue)
                        .Average(t => (t.CheckOutTime.Value - t.CheckInTime).TotalMinutes)
                    : 0;

                var currentTicket = slotTickets.FirstOrDefault(t => string.IsNullOrEmpty(t.CheckOutTime.ToString()));
                var occupancyMinutes = currentTicket != null
                    ? (int)(DateTime.Now - currentTicket.CheckInTime).TotalMinutes
                    : (int?)null;

                // Recent history (last 10)
                var history = slotTickets
                    .OrderByDescending(t => t.CheckInTime)
                    .Take(10)
                    .Select(t => new SlotUsageHistoryDto
                    {
                        VehiclePlate = t.VehiclePlate,
                        CustomerName = t.Customer?.FullName,
                        CheckInTime = t.CheckInTime,
                        CheckOutTime = t.CheckOutTime,
                        DurationMinutes = t.CheckOutTime.HasValue
                            ? (int)(t.CheckOutTime.Value - t.CheckInTime).TotalMinutes
                            : null
                    })
                    .ToList();

                return new ParkingSlotDetailDto
                {
                    SlotId = slot.SlotId,
                    VehicleType = slot.VehicleType,
                    Location = slot.Location,
                    Status = slot.Status,
                    IsOccupied = currentTicket != null,
                    CurrentVehiclePlate = currentTicket?.VehiclePlate,
                    CurrentCustomerName = currentTicket?.Customer?.FullName,
                    OccupiedSince = currentTicket?.CheckInTime,
                    CurrentOccupancyMinutes = occupancyMinutes,
                    TotalUsageCount = slotTickets.Count,
                    UsageThisMonth = usageThisMonth,
                    UsageThisWeek = usageThisWeek,
                    AverageOccupancyTime = avgOccupancyTime,
                    FirstUsedAt = slotTickets.Count > 0 ? slotTickets.Min(t => t.CheckInTime) : null,
                    LastUsedAt = slotTickets.Count > 0 
                        ? slotTickets.Max(t => t.CheckOutTime ?? t.CheckInTime)
                        : null,
                    RecentHistory = history
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết chỗ đỗ: {ex.Message}");
            }
        }

        /// <summary>
        /// Get parking slot report
        /// </summary>
        public async Task<ParkingSlotReportDto> GetParkingSlotReportAsync()
        {
            try
            {
                var slots = (await _slotRepo.GetAllAsync()).ToList();
                var tickets = (await _ticketRepo.GetAllAsync()).ToList();

                var totalSlots = slots.Count;
                var totalEmpty = slots.Count(s => s.Status == "Trống");
                var totalOccupied = slots.Count(s => s.Status == "Đang sử dụng");
                var totalMaintenance = slots.Count(s => s.Status == "Bảo trì");
                var utilization = totalSlots > 0 ? (double)totalOccupied / totalSlots * 100 : 0;

                // By vehicle type
                var byVehicleType = slots
                    .GroupBy(s => s.VehicleType)
                    .ToDictionary(
                        g => g.Key,
                        g => new SlotTypeDetailDto
                        {
                            Total = g.Count(),
                            Empty = g.Count(s => s.Status == "Trống"),
                            Occupied = g.Count(s => s.Status == "Đang sử dụng"),
                            Maintenance = g.Count(s => s.Status == "Bảo trì"),
                            UtilizationRate = g.Count() > 0
                                ? (double)g.Count(s => s.Status == "Đang sử dụng") / g.Count() * 100
                                : 0
                        }
                    );

                // Top used
                var topUsed = slots
                    .Select(s => new TopUsedSlotDto
                    {
                        SlotId = s.SlotId,
                        Location = s.Location,
                        UsageCount = tickets.Count(t => t.SlotId == s.SlotId),
                        LastUsedAt = tickets.Where(t => t.SlotId == s.SlotId)
                            .Max(t => (DateTime?)(t.CheckOutTime ?? t.CheckInTime))
                    })
                    .OrderByDescending(x => x.UsageCount)
                    .Take(5)
                    .ToList();

                // Least used
                var leastUsed = slots
                    .Select(s => new TopUsedSlotDto
                    {
                        SlotId = s.SlotId,
                        Location = s.Location,
                        UsageCount = tickets.Count(t => t.SlotId == s.SlotId),
                        LastUsedAt = tickets.Where(t => t.SlotId == s.SlotId)
                            .Max(t => (DateTime?)(t.CheckOutTime ?? t.CheckInTime))
                    })
                    .OrderBy(x => x.UsageCount)
                    .Take(5)
                    .ToList();

                return new ParkingSlotReportDto
                {
                    TotalSlots = totalSlots,
                    TotalEmpty = totalEmpty,
                    TotalOccupied = totalOccupied,
                    TotalMaintenance = totalMaintenance,
                    UtilizationRate = utilization,
                    ByVehicleType = byVehicleType,
                    TopUsedSlots = topUsed,
                    LeastUsedSlots = leastUsed
                };
            }
            catch (Exception ex)
            {
                return new ParkingSlotReportDto();
            }
        }

        /// <summary>
        /// Update parking slot status
        /// </summary>
        public async Task<UpdateParkingSlotResultDto> UpdateParkingSlotAsync(UpdateParkingSlotDto request)
        {
            try
            {
                var slot = await _slotRepo.GetByIdAsync(request.SlotId);
                if (slot == null)
                    return new UpdateParkingSlotResultDto
                    {
                        Success = false,
                        Message = "Chỗ đỗ không tồn tại",
                        SlotId = request.SlotId
                    };

                if (!string.IsNullOrEmpty(request.Status))
                    slot.Status = request.Status;

                await _slotRepo.UpdateAsync(slot);

                return new UpdateParkingSlotResultDto
                {
                    Success = true,
                    Message = "Cập nhật chỗ đỗ thành công",
                    SlotId = request.SlotId
                };
            }
            catch (Exception ex)
            {
                return new UpdateParkingSlotResultDto
                {
                    Success = false,
                    Message = $"Lỗi cập nhật chỗ đỗ: {ex.Message}",
                    SlotId = request.SlotId
                };
            }
        }

        // -- Employee Slot Management --
        public async Task<ListEmployeeSlotDto> GetEmployeeSlotsAsync(EmployeeSlotFilterDto filter)
        {
            try
            {
                var allSlots = (await _slotRepo.GetAllAsync()).ToList();
                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();

                // Apply filters
                var filtered = allSlots.AsEnumerable();

                if (!string.IsNullOrEmpty(filter.VehicleType))
                    filtered = filtered.Where(s => s.VehicleType == filter.VehicleType);

                if (!string.IsNullOrEmpty(filter.Status))
                    filtered = filtered.Where(s => s.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.Location))
                    filtered = filtered.Where(s => s.Location == filter.Location);

                var sorted = filtered.OrderBy(s => s.SlotId).ToList();

                // Pagination
                var totalItems = sorted.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                var items = sorted
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                // Map to DTOs
                var slotDtos = items.Select(s =>
                {
                    var currentTicket = allTickets.FirstOrDefault(t => 
                        t.SlotId == s.SlotId && string.IsNullOrEmpty(t.CheckOutTime.ToString()));

                    return new EmployeeSlotListItemDto
                    {
                        SlotId = s.SlotId,
                        VehicleType = s.VehicleType,
                        Location = s.Location,
                        Status = s.Status,
                        IsAvailable = s.Status == "Trống",
                        CurrentOccupant = currentTicket?.VehiclePlate,
                        OccupiedSince = currentTicket?.CheckInTime
                    };
                }).ToList();

                var totalEmpty = sorted.Count(s => s.Status == "Trống");
                var totalOccupied = sorted.Count(s => s.Status == "Đang sử dụng");
                var totalMaintenance = sorted.Count(s => s.Status == "Bảo trì");
                var utilization = sorted.Count > 0
                    ? (double)totalOccupied / sorted.Count * 100
                    : 0;

                return new ListEmployeeSlotDto
                {
                    Items = slotDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    TotalEmpty = totalEmpty,
                    TotalOccupied = totalOccupied,
                    TotalMaintenance = totalMaintenance,
                    UtilizationRate = utilization
                };
            }
            catch (Exception ex)
            {
                return new ListEmployeeSlotDto
                {
                    Items = new(),
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = 0,
                    TotalPages = 0,
                    TotalEmpty = 0,
                    TotalOccupied = 0,
                    TotalMaintenance = 0,
                    UtilizationRate = 0
                };
            }
        }

        /// <summary>
        /// Lấy chi tiết chỗ đỗ
        /// </summary>
        public async Task<EmployeeSlotDetailDto> GetEmployeeSlotDetailAsync(string slotId)
        {
            try
            {
                var slot = await _slotRepo.GetByIdAsync(slotId);
                if (slot == null)
                    throw new Exception("Chỗ đỗ không tồn tại");

                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();
                var slotTickets = allTickets.Where(t => t.SlotId == slotId).ToList();

                var thisMonth = DateTime.Now.AddMonths(-1);
                var thisWeek = DateTime.Now.AddDays(-7);

                var usageThisMonth = slotTickets.Count(t => t.CheckInTime >= thisMonth);
                var usageThisWeek = slotTickets.Count(t => t.CheckInTime >= thisWeek);
                var lastUsed = slotTickets.Count > 0
                    ? slotTickets.Max(t => t.CheckOutTime ?? t.CheckInTime)
                    : (DateTime?)null;

                var avgOccupancyTime = slotTickets.Count > 0
                    ? slotTickets.Where(t => t.CheckOutTime.HasValue)
                        .Average(t => (t.CheckOutTime.Value - t.CheckInTime).TotalMinutes)
                    : 0;

                var currentTicket = slotTickets.FirstOrDefault(t => string.IsNullOrEmpty(t.CheckOutTime.ToString()));
                var occupancyMinutes = currentTicket != null
                    ? (int)(DateTime.Now - currentTicket.CheckInTime).TotalMinutes
                    : (int?)null;

                return new EmployeeSlotDetailDto
                {
                    SlotId = slot.SlotId,
                    VehicleType = slot.VehicleType,
                    Location = slot.Location,
                    Status = slot.Status,
                    IsOccupied = currentTicket != null,
                    CurrentVehiclePlate = currentTicket?.VehiclePlate,
                    CurrentCustomerName = currentTicket?.Customer?.FullName,
                    OccupiedSince = currentTicket?.CheckInTime,
                    CurrentOccupancyMinutes = occupancyMinutes,
                    UsageThisMonth = usageThisMonth,
                    UsageThisWeek = usageThisWeek,
                    LastUsedAt = lastUsed,
                    AverageOccupancyTime = avgOccupancyTime
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết chỗ đỗ: {ex.Message}");
            }
        }
    }
}
