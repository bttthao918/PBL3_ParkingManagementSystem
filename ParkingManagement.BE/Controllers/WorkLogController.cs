using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.BLL.Constants;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.Web.Controllers.Api
{
    [ApiController]
    [Route("api/worklogs")]
    [Authorize(Roles = "Employee")]
    [Produces("application/json")]
    public class WorkLogController : ControllerBase
    {
        private const string ScheduledStatus = "Đã lên lịch";
        private const string WorkingStatus = "Đang làm";
        private const string CompletedStatus = "Hoàn thành";

        private readonly AppDbContext _db;
        private readonly ILogger<WorkLogController> _logger;

        public WorkLogController(AppDbContext db, ILogger<WorkLogController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null)
                return Unauthorized(new { message = "Không xác định được nhân viên" });

            var activeLog = await _db.WorkLogs
                .Include(w => w.ShiftSchedule)
                .Where(w => w.EmployeeId == employeeId && w.Status == WorkingStatus)
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

            var now = DateTime.Now;
            var elapsed = (now - activeLog.StartTime).TotalHours;
            var isNextDay = activeLog.StartTime.Date < now.Date;

            if (elapsed > 12 || isNextDay)
            {
                var autoEndTime = isNextDay
                    ? activeLog.StartTime.Date.AddHours(23).AddMinutes(59)
                    : activeLog.StartTime.AddHours(12);

                var totalMinutes = (int)(autoEndTime - activeLog.StartTime).TotalMinutes;
                activeLog.EndTime = autoEndTime;
                activeLog.TotalMinutes = totalMinutes;
                activeLog.Status = CompletedStatus;
                activeLog.Note = AppendNote(activeLog.Note, "Tự động đóng - quên kết thúc ca");

                if (activeLog.ShiftSchedule != null)
                {
                    activeLog.ShiftSchedule.Status = CompletedStatus;
                }

                await _db.SaveChangesAsync();

                _logger.LogWarning("Auto-closed work log {WorkLogId} for employee {EmployeeId}", activeLog.WorkLogId, employeeId);

                return Ok(new
                {
                    isWorking = false,
                    message = $"Ca trước đã được tự động đóng. Tổng: {totalMinutes / 60}h {totalMinutes % 60}p.",
                    autoClosedShift = new
                    {
                        workLogId = activeLog.WorkLogId,
                        scheduleId = activeLog.ScheduleId,
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
                scheduleId = activeLog.ScheduleId,
                shiftType = activeLog.ShiftSchedule?.ShiftType,
                startTime = activeLog.StartTime,
                durationMinutes = duration,
                note = activeLog.Note,
                message = "Đang trong ca làm việc"
            });
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartShift([FromBody] StartShiftDto? dto)
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null)
                return Unauthorized(new { message = "Không xác định được nhân viên" });

            var activeLog = await _db.WorkLogs
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Status == WorkingStatus);

            if (activeLog != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Bạn đang trong ca làm việc bắt đầu lúc {activeLog.StartTime:HH:mm}. Vui lòng kết thúc ca trước."
                });
            }

            var now = DateTime.Now;
            var schedule = await FindStartableScheduleAsync(employeeId, dto?.ScheduleId, now);
            if (schedule == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Bạn chưa có ca được phân công hôm nay hoặc ca đã hoàn thành."
                });
            }

            if (schedule.Status == WorkingStatus)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ca này đang được tính là đang hoạt động."
                });
            }

            if (schedule.Status == CompletedStatus)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ca này đã hoàn thành, không thể bắt đầu lại."
                });
            }

            if (schedule.Status != ScheduledStatus)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Ca này đang ở trạng thái {schedule.Status}, không thể bắt đầu."
                });
            }

            var window = ShiftConstants.GetEffectiveWindow(schedule.ShiftType, schedule.StartTime, schedule.EndTime);
            if (!ShiftConstants.IsWithinShift(now.TimeOfDay, window.Start, window.End))
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Hiện tại không nằm trong ca {schedule.ShiftType} ({ShiftConstants.FormatWindow(window.Start, window.End)}). Không thể bắt đầu ca lệch giờ; bạn chỉ có thể bắt đầu trong đúng khung giờ được phân công."
                });
            }

            var workLogId = "WL" + now.ToString("yyyyMMddHHmmss") + Random.Shared.Next(100, 999);
            var workLog = new WorkLog
            {
                WorkLogId = workLogId,
                EmployeeId = employeeId,
                ScheduleId = schedule.ScheduleId,
                WorkDate = schedule.WorkDate.Date,
                StartTime = now,
                EndTime = null,
                TotalMinutes = null,
                Note = dto?.Note,
                Status = WorkingStatus
            };

            schedule.Status = WorkingStatus;

            _db.WorkLogs.Add(workLog);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} started schedule {ScheduleId} at {Time}", employeeId, schedule.ScheduleId, now);

            return Ok(new
            {
                success = true,
                message = $"Bắt đầu ca {schedule.ShiftType} lúc {now:HH:mm}",
                workLogId,
                scheduleId = schedule.ScheduleId,
                startTime = now
            });
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndShift([FromBody] EndShiftDto? dto)
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null)
                return Unauthorized(new { message = "Không xác định được nhân viên" });

            var activeLog = await _db.WorkLogs
                .Include(w => w.ShiftSchedule)
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Status == WorkingStatus);

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
            activeLog.Status = CompletedStatus;
            if (!string.IsNullOrWhiteSpace(dto?.Note))
            {
                activeLog.Note = AppendNote(activeLog.Note, dto.Note);
            }

            if (activeLog.ShiftSchedule != null)
            {
                activeLog.ShiftSchedule.Status = CompletedStatus;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} ended work log {WorkLogId} at {Time}, total {Minutes} min", employeeId, activeLog.WorkLogId, now, totalMinutes);

            return Ok(new
            {
                success = true,
                message = $"Kết thúc ca lúc {now:HH:mm}. Tổng: {totalMinutes / 60} giờ {totalMinutes % 60} phút.",
                workLogId = activeLog.WorkLogId,
                scheduleId = activeLog.ScheduleId,
                startTime = activeLog.StartTime,
                endTime = now,
                totalMinutes
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null)
                return Unauthorized(new { message = "Không xác định được nhân viên" });

            var from = fromDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            var to = toDate?.Date ?? DateTime.Now.Date;

            var query = _db.WorkLogs
                .Include(w => w.ShiftSchedule)
                .Where(w => w.EmployeeId == employeeId && w.WorkDate >= from && w.WorkDate <= to)
                .OrderByDescending(w => w.StartTime);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new
                {
                    w.WorkLogId,
                    w.ScheduleId,
                    ShiftType = w.ShiftSchedule != null ? w.ShiftSchedule.ShiftType : null,
                    w.WorkDate,
                    w.StartTime,
                    w.EndTime,
                    w.TotalMinutes,
                    w.Note,
                    w.Status
                })
                .ToListAsync();

            var completedLogs = await _db.WorkLogs
                .Where(w => w.EmployeeId == employeeId && w.WorkDate >= from && w.WorkDate <= to && w.Status == CompletedStatus)
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

        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlySummary()
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null)
                return Unauthorized(new { message = "Không xác định được nhân viên" });

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var today = DateTime.Now.Date;

            var logs = await _db.WorkLogs
                .Where(w => w.EmployeeId == employeeId && w.WorkDate >= firstDayOfMonth && w.WorkDate <= today && w.Status == CompletedStatus)
                .ToListAsync();

            var totalDays = logs.Select(w => w.WorkDate).Distinct().Count();
            var totalMinutes = logs.Sum(w => w.TotalMinutes ?? 0);

            return Ok(new
            {
                totalDays,
                totalMinutes,
                totalHours = totalMinutes / 60,
                averageHoursPerDay = totalDays > 0 ? Math.Round(totalMinutes / 60.0 / totalDays, 1) : 0,
                month = DateTime.Now.Month,
                year = DateTime.Now.Year
            });
        }

        private async Task<ShiftSchedule?> FindStartableScheduleAsync(string employeeId, string? scheduleId, DateTime now)
        {
            var query = _db.ShiftSchedules
                .Where(s => s.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(scheduleId))
            {
                var schedule = await query.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.WorkDate == now.Date);
                return schedule;
            }

            var schedules = await query
                .Where(s => s.WorkDate == now.Date)
                .ToListAsync();

            var startableSchedule = schedules
                .Where(s => s.Status == ScheduledStatus)
                .Select(s =>
                {
                    var window = ShiftConstants.GetEffectiveWindow(s.ShiftType, s.StartTime, s.EndTime);
                    var isCurrentShift = ShiftConstants.IsWithinShift(now.TimeOfDay, window.Start, window.End);
                    return new { Schedule = s, Window = window, IsCurrentShift = isCurrentShift };
                })
                .OrderByDescending(x => x.IsCurrentShift)
                .ThenBy(x => x.Window.Start)
                .FirstOrDefault()
                ?.Schedule;

            if (startableSchedule == null && schedules.Count == 0)
            {
                startableSchedule = await BuildDefaultScheduleAsync(employeeId, now);
            }

            return startableSchedule;
        }

        private async Task<ShiftSchedule?> BuildDefaultScheduleAsync(string employeeId, DateTime now)
        {
            var employee = await _db.Employees.FindAsync(employeeId);
            if (employee == null ||
                string.IsNullOrWhiteSpace(employee.Shift) ||
                !ShiftConstants.TryGetShiftWindow(employee.Shift, out var shiftType, out var startTime, out var endTime))
            {
                return null;
            }

            var schedule = new ShiftSchedule
            {
                ScheduleId = "SCH" + now.ToString("yyyyMMddHHmmss") + Random.Shared.Next(100, 999),
                EmployeeId = employeeId,
                WorkDate = now.Date,
                ShiftType = shiftType,
                StartTime = startTime,
                EndTime = endTime,
                Status = ScheduledStatus,
                Note = "Default employee shift",
                CreatedBy = User.FindFirst("accountId")?.Value ?? employee.AccountId ?? employeeId,
                CreatedAt = now
            };

            _db.ShiftSchedules.Add(schedule);
            return schedule;
        }

        private static string AppendNote(string? current, string note)
        {
            return string.IsNullOrWhiteSpace(current) ? note : $"{current} | {note}";
        }

        private string? GetEmployeeId()
        {
            return User.FindFirst("employeeId")?.Value
                ?? User.FindFirst("related_id")?.Value;
        }
    }

    public class StartShiftDto
    {
        public string? ScheduleId { get; set; }
        public string? Note { get; set; }
    }

    public class EndShiftDto
    {
        public string? Note { get; set; }
    }
}
