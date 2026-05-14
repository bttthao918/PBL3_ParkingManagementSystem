using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// API quản lý ca làm việc
    /// Manager: tạo/sửa/xóa lịch ca
    /// Employee: xem ca của mình
    /// </summary>
    [ApiController]
    [Route("api/shifts")]
    [Authorize]
    [Produces("application/json")]
    public class ShiftScheduleController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ShiftScheduleController> _logger;

        // Ca mặc định
        private static readonly Dictionary<string, (TimeSpan Start, TimeSpan End)> DefaultShifts = new()
        {
            ["Sáng"] = (new TimeSpan(6, 0, 0), new TimeSpan(14, 0, 0)),
            ["Chiều"] = (new TimeSpan(14, 0, 0), new TimeSpan(22, 0, 0)),
            ["Tối"] = (new TimeSpan(22, 0, 0), new TimeSpan(6, 0, 0))
        };

        public ShiftScheduleController(AppDbContext db, ILogger<ShiftScheduleController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Lấy lịch ca theo tuần (Manager)
        /// </summary>
        [HttpGet("week")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetWeekSchedule([FromQuery] DateTime? startDate)
        {
            var start = startDate?.Date ?? GetMondayOfWeek(DateTime.Today);
            var end = start.AddDays(6);

            var schedules = await _db.ShiftSchedules
                .Include(s => s.Employee)
                .Where(s => s.WorkDate >= start && s.WorkDate <= end)
                .OrderBy(s => s.WorkDate)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    s.ScheduleId,
                    s.EmployeeId,
                    EmployeeName = s.Employee.FullName,
                    s.WorkDate,
                    s.ShiftType,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                    s.Status,
                    s.Note
                })
                .ToListAsync();

            var employees = await _db.Employees
                .Where(e => !e.IsDeleted)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            return Ok(new
            {
                weekStart = start,
                weekEnd = end,
                schedules,
                employees
            });
        }

        /// <summary>
        /// Tạo lịch ca (Manager)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateShiftDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.EmployeeId) || string.IsNullOrWhiteSpace(dto.ShiftType))
                return BadRequest(new { success = false, message = "Thiếu thông tin nhân viên hoặc loại ca" });

            if (!DefaultShifts.ContainsKey(dto.ShiftType))
                return BadRequest(new { success = false, message = "Loại ca không hợp lệ. Chọn: Sáng, Chiều, Tối" });

            var employee = await _db.Employees.FindAsync(dto.EmployeeId);
            if (employee == null || employee.IsDeleted)
                return BadRequest(new { success = false, message = "Nhân viên không tồn tại" });

            // Kiểm tra trùng ca
            var existing = await _db.ShiftSchedules
                .AnyAsync(s => s.EmployeeId == dto.EmployeeId && s.WorkDate == dto.WorkDate.Date && s.ShiftType == dto.ShiftType);
            if (existing)
                return BadRequest(new { success = false, message = $"Nhân viên đã có ca {dto.ShiftType} ngày {dto.WorkDate:dd/MM}" });

            var shift = DefaultShifts[dto.ShiftType];
            var scheduleId = "SCH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

            var schedule = new ShiftSchedule
            {
                ScheduleId = scheduleId,
                EmployeeId = dto.EmployeeId,
                WorkDate = dto.WorkDate.Date,
                ShiftType = dto.ShiftType,
                StartTime = shift.Start,
                EndTime = shift.End,
                Status = "Đã lên lịch",
                Note = dto.Note,
                CreatedBy = User.FindFirst("accountId")?.Value ?? "MGR001",
                CreatedAt = DateTime.Now
            };

            _db.ShiftSchedules.Add(schedule);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Shift created: {ScheduleId} for {EmployeeId} on {Date}", scheduleId, dto.EmployeeId, dto.WorkDate);

            return Ok(new { success = true, message = $"Đã tạo ca {dto.ShiftType} cho {employee.FullName} ngày {dto.WorkDate:dd/MM}", scheduleId });
        }

        /// <summary>
        /// Xóa lịch ca (Manager)
        /// </summary>
        [HttpDelete("{scheduleId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteSchedule(string scheduleId)
        {
            var schedule = await _db.ShiftSchedules.FindAsync(scheduleId);
            if (schedule == null)
                return NotFound(new { success = false, message = "Không tìm thấy lịch ca" });

            if (schedule.Status == "Đang làm" || schedule.Status == "Hoàn thành")
                return BadRequest(new { success = false, message = "Không thể xóa ca đang làm hoặc đã hoàn thành" });

            _db.ShiftSchedules.Remove(schedule);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa lịch ca" });
        }

        /// <summary>
        /// Tạo lịch ca hàng loạt cho cả tuần (Manager)
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateShiftDto dto)
        {
            if (dto.Assignments == null || !dto.Assignments.Any())
                return BadRequest(new { success = false, message = "Không có dữ liệu phân ca" });

            var created = 0;
            var skipped = 0;

            foreach (var assignment in dto.Assignments)
            {
                if (!DefaultShifts.ContainsKey(assignment.ShiftType)) { skipped++; continue; }

                var exists = await _db.ShiftSchedules
                    .AnyAsync(s => s.EmployeeId == assignment.EmployeeId && s.WorkDate == assignment.WorkDate.Date && s.ShiftType == assignment.ShiftType);
                if (exists) { skipped++; continue; }

                var shift = DefaultShifts[assignment.ShiftType];
                var scheduleId = "SCH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

                _db.ShiftSchedules.Add(new ShiftSchedule
                {
                    ScheduleId = scheduleId,
                    EmployeeId = assignment.EmployeeId,
                    WorkDate = assignment.WorkDate.Date,
                    ShiftType = assignment.ShiftType,
                    StartTime = shift.Start,
                    EndTime = shift.End,
                    Status = "Đã lên lịch",
                    Note = assignment.Note,
                    CreatedBy = User.FindFirst("accountId")?.Value ?? "MGR001",
                    CreatedAt = DateTime.Now
                });
                created++;
            }

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã tạo {created} ca, bỏ qua {skipped} ca trùng", created, skipped });
        }

        /// <summary>
        /// Lấy ca hôm nay của nhân viên hiện tại (Employee)
        /// </summary>
        [HttpGet("my-today")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyTodayShift()
        {
            var employeeId = User.FindFirst("employeeId")?.Value ?? User.FindFirst("related_id")?.Value;
            if (string.IsNullOrEmpty(employeeId))
                return Ok(new { hasShift = false, message = "Không xác định được nhân viên" });

            var today = DateTime.Today;
            var todayShift = await _db.ShiftSchedules
                .Where(s => s.EmployeeId == employeeId && s.WorkDate == today)
                .OrderBy(s => s.StartTime)
                .Select(s => new
                {
                    s.ScheduleId,
                    s.ShiftType,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                    s.Status,
                    s.Note
                })
                .FirstOrDefaultAsync();

            if (todayShift == null)
                return Ok(new { hasShift = false, message = "Hôm nay bạn không có ca" });

            return Ok(new { hasShift = true, shift = todayShift });
        }

        /// <summary>
        /// Lấy lịch ca tuần của nhân viên hiện tại (Employee)
        /// </summary>
        [HttpGet("my-week")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyWeekSchedule()
        {
            var employeeId = User.FindFirst("employeeId")?.Value ?? User.FindFirst("related_id")?.Value;
            if (string.IsNullOrEmpty(employeeId))
                return Ok(new List<object>());

            var start = GetMondayOfWeek(DateTime.Today);
            var end = start.AddDays(6);

            var schedules = await _db.ShiftSchedules
                .Where(s => s.EmployeeId == employeeId && s.WorkDate >= start && s.WorkDate <= end)
                .OrderBy(s => s.WorkDate)
                .Select(s => new
                {
                    s.WorkDate,
                    s.ShiftType,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                    s.Status
                })
                .ToListAsync();

            return Ok(schedules);
        }

        // Helper
        private static DateTime GetMondayOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }

    // DTOs
    public class CreateShiftDto
    {
        public string EmployeeId { get; set; } = null!;
        public DateTime WorkDate { get; set; }
        public string ShiftType { get; set; } = null!; // "Sáng", "Chiều", "Tối"
        public string? Note { get; set; }
    }

    public class BulkCreateShiftDto
    {
        public List<CreateShiftDto> Assignments { get; set; } = new();
    }
}
