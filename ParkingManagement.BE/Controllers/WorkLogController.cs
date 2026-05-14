using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// API chấm công cho nhân viên
    /// Bắt đầu ca / Kết thúc ca / Xem lịch sử
    /// </summary>
    [ApiController]
    [Route("api/worklogs")]
    [Authorize(Roles = "Employee")]
    [Produces("application/json")]
    public class WorkLogController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<WorkLogController> _logger;

        public WorkLogController(AppDbContext db, ILogger<WorkLogController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Lấy trạng thái ca hiện tại của nhân viên (đang trong ca hay không)
        /// Tự động đóng ca nếu quá 12 giờ hoặc qua ngày mới
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null) return Unauthorized(new { message = "Không xác định được nhân viên" });

            var activeLog = await _db.WorkLogs
                .Where(w => w.EmployeeId == employeeId && w.Status == "Đang làm")
                .OrderByDescending(w => w.StartTime)
                .FirstOrDefaultAsync();

            if (activeLog == null)
            {
                return Ok(new
                {
                    isWorking = false,
                    message = "Chưa bắt đầu ca"
                });
            }

            // Auto-close: nếu ca kéo dài quá 12 giờ hoặc qua ngày mới
            var now = DateTime.Now;
            var elapsed = (now - activeLog.StartTime).TotalHours;
            var isNextDay = activeLog.StartTime.Date < now.Date;

            if (elapsed > 12 || isNextDay)
            {
                // Tự động đóng ca lúc 23:59 ngày bắt đầu (hoặc sau 12h)
                var autoEndTime = isNextDay
                    ? activeLog.StartTime.Date.AddHours(23).AddMinutes(59)
                    : activeLog.StartTime.AddHours(12);

                var totalMinutes = (int)(autoEndTime - activeLog.StartTime).TotalMinutes;
                activeLog.EndTime = autoEndTime;
                activeLog.TotalMinutes = totalMinutes;
                activeLog.Status = "Hoàn thành";
                activeLog.Note = (activeLog.Note ?? "") + " [Tự động đóng - quên kết thúc ca]";
                await _db.SaveChangesAsync();

                _logger.LogWarning("Auto-closed shift {WorkLogId} for employee {EmployeeId} (forgot to end)", activeLog.WorkLogId, employeeId);

                return Ok(new
                {
                    isWorking = false,
                    message = $"Ca trước đã được tự động đóng (quên bấm kết thúc). Tổng: {totalMinutes / 60}h {totalMinutes % 60}p.",
                    autoClosedShift = new
                    {
                        workLogId = activeLog.WorkLogId,
                        startTime = activeLog.StartTime,
                        endTime = autoEndTime,
                        totalMinutes
                    }
                });
            }

            var duration = (int)(now - activeLog.StartTime).TotalMinutes;

            return Ok(new
            {
                isWorking = true,
                workLogId = activeLog.WorkLogId,
                startTime = activeLog.StartTime,
                durationMinutes = duration,
                note = activeLog.Note,
                message = "Đang trong ca làm việc"
            });
        }

        /// <summary>
        /// Bắt đầu ca làm việc
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartShift([FromBody] StartShiftDto? dto)
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null) return Unauthorized(new { message = "Không xác định được nhân viên" });

            // Kiểm tra đã có ca đang mở chưa
            var activeLog = await _db.WorkLogs
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Status == "Đang làm");

            if (activeLog != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Bạn đang trong ca làm việc (bắt đầu lúc {activeLog.StartTime:HH:mm}). Vui lòng kết thúc ca trước."
                });
            }

            var now = DateTime.Now;
            var workLogId = "WL" + now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

            var workLog = new WorkLog
            {
                WorkLogId = workLogId,
                EmployeeId = employeeId,
                WorkDate = now.Date,
                StartTime = now,
                EndTime = null,
                TotalMinutes = null,
                Note = dto?.Note,
                Status = "Đang làm"
            };

            _db.WorkLogs.Add(workLog);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} started shift at {Time}", employeeId, now);

            return Ok(new
            {
                success = true,
                message = $"Bắt đầu ca lúc {now:HH:mm}",
                workLogId,
                startTime = now
            });
        }

        /// <summary>
        /// Kết thúc ca làm việc
        /// </summary>
        [HttpPost("end")]
        public async Task<IActionResult> EndShift([FromBody] EndShiftDto? dto)
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null) return Unauthorized(new { message = "Không xác định được nhân viên" });

            var activeLog = await _db.WorkLogs
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Status == "Đang làm");

            if (activeLog == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Bạn chưa bắt đầu ca nào."
                });
            }

            var now = DateTime.Now;
            var totalMinutes = (int)(now - activeLog.StartTime).TotalMinutes;

            activeLog.EndTime = now;
            activeLog.TotalMinutes = totalMinutes;
            activeLog.Status = "Hoàn thành";
            if (!string.IsNullOrWhiteSpace(dto?.Note))
            {
                activeLog.Note = (activeLog.Note ?? "") + " | " + dto.Note;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} ended shift at {Time}, total {Minutes} min", employeeId, now, totalMinutes);

            var hours = totalMinutes / 60;
            var mins = totalMinutes % 60;

            return Ok(new
            {
                success = true,
                message = $"Kết thúc ca lúc {now:HH:mm}. Tổng: {hours} giờ {mins} phút.",
                workLogId = activeLog.WorkLogId,
                startTime = activeLog.StartTime,
                endTime = now,
                totalMinutes
            });
        }

        /// <summary>
        /// Lấy lịch sử chấm công của nhân viên
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null) return Unauthorized(new { message = "Không xác định được nhân viên" });

            var from = fromDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            var to = toDate?.Date ?? DateTime.Now.Date;

            var query = _db.WorkLogs
                .Where(w => w.EmployeeId == employeeId && w.WorkDate >= from && w.WorkDate <= to)
                .OrderByDescending(w => w.StartTime);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new
                {
                    w.WorkLogId,
                    w.WorkDate,
                    w.StartTime,
                    w.EndTime,
                    w.TotalMinutes,
                    w.Note,
                    w.Status
                })
                .ToListAsync();

            // Thống kê
            var completedLogs = await _db.WorkLogs
                .Where(w => w.EmployeeId == employeeId && w.WorkDate >= from && w.WorkDate <= to && w.Status == "Hoàn thành")
                .ToListAsync();

            var totalWorkDays = completedLogs.Select(w => w.WorkDate).Distinct().Count();
            var totalWorkMinutes = completedLogs.Sum(w => w.TotalMinutes ?? 0);

            return Ok(new
            {
                items,
                totalItems = total,
                totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                page,
                pageSize,
                summary = new
                {
                    totalWorkDays,
                    totalWorkMinutes,
                    averageMinutesPerDay = totalWorkDays > 0 ? totalWorkMinutes / totalWorkDays : 0
                }
            });
        }

        /// <summary>
        /// Lấy thống kê tháng hiện tại
        /// </summary>
        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlySummary()
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null) return Unauthorized(new { message = "Không xác định được nhân viên" });

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var today = DateTime.Now.Date;

            var logs = await _db.WorkLogs
                .Where(w => w.EmployeeId == employeeId && w.WorkDate >= firstDayOfMonth && w.WorkDate <= today && w.Status == "Hoàn thành")
                .ToListAsync();

            var totalDays = logs.Select(w => w.WorkDate).Distinct().Count();
            var totalMinutes = logs.Sum(w => w.TotalMinutes ?? 0);
            var totalHours = totalMinutes / 60;
            var avgHoursPerDay = totalDays > 0 ? Math.Round(totalMinutes / 60.0 / totalDays, 1) : 0;

            return Ok(new
            {
                totalDays,
                totalMinutes,
                totalHours,
                averageHoursPerDay = avgHoursPerDay,
                month = DateTime.Now.Month,
                year = DateTime.Now.Year
            });
        }

        // ── Helper ──
        private string? GetEmployeeId()
        {
            // JWT token dùng claim "employeeId", cookie dùng "related_id"
            return User.FindFirst("employeeId")?.Value
                ?? User.FindFirst("related_id")?.Value;
        }
    }

    // ── DTOs ──
    public class StartShiftDto
    {
        public string? Note { get; set; }
    }

    public class EndShiftDto
    {
        public string? Note { get; set; }
    }
}
