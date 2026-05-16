import re

path = r'd:\pbl3\PBL3_ParkingManagementSystem\ParkingManagement.BE\Services\ReportService.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = re.compile(r'public async Task<ShiftAttendanceReportDto> GetShiftAttendanceReportAsync.*?catch \([^\)]*\)\s*\{\s*return new ShiftAttendanceReportDto\(\);\s*\}\s*\}', re.DOTALL)

replacement = """public async Task<ShiftAttendanceReportDto> GetShiftAttendanceReportAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var from = fromDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
                var to = toDate?.Date ?? DateTime.Now.Date;

                var workLogs = await _db.WorkLogs
                    .Include(w => w.ShiftSchedule)
                    .Where(w => w.EmployeeId == employeeId && w.WorkDate >= from && w.WorkDate <= to)
                    .OrderBy(w => w.WorkDate)
                    .ThenBy(w => w.StartTime)
                    .ToListAsync();

                var allTickets = await _db.Tickets
                    .Where(t => t.CheckInTime.Date >= from && t.CheckInTime.Date <= to)
                    .ToListAsync();

                var details = new List<ShiftAttendanceDetailDto>();

                foreach (var log in workLogs)
                {
                    var shiftEnd = log.EndTime ?? DateTime.Now;
                    
                    var shiftTickets = allTickets.Where(t => 
                        (t.CheckInTime >= log.StartTime && t.CheckInTime <= shiftEnd) ||
                        (t.CheckOutTime.HasValue && t.CheckOutTime.Value >= log.StartTime && t.CheckOutTime.Value <= shiftEnd)
                    ).ToList();

                    var shiftRevenue = shiftTickets
                        .Where(t => t.CheckOutTime.HasValue && t.CheckOutTime.Value >= log.StartTime && t.CheckOutTime.Value <= shiftEnd)
                        .Sum(t => t.Fee);

                    string status = "Đúng giờ";
                    if (!log.EndTime.HasValue) status = "Đang làm";
                    else if (log.ShiftSchedule != null && log.ShiftSchedule.Status == "Vắng") status = "Nghỉ";

                    details.Add(new ShiftAttendanceDetailDto
                    {
                        Date = log.WorkDate,
                        Shift = log.ShiftSchedule?.ShiftType ?? "Ca không xác định",
                        CheckInTime = log.StartTime,
                        CheckOutTime = log.EndTime ?? log.StartTime,
                        WorkMinutes = log.TotalMinutes ?? (int)(DateTime.Now - log.StartTime).TotalMinutes,
                        Status = status,
                        TicketsProcessed = shiftTickets.Count,
                        ShiftRevenue = shiftRevenue
                    });
                }

                var totalWorkDays = details.Count;
                var totalWorkMinutes = details.Sum(d => d.WorkMinutes ?? 0);
                var avgWorkMinutesPerDay = totalWorkDays > 0 ? totalWorkMinutes / totalWorkDays : 0;

                return new ShiftAttendanceReportDto
                {
                    Details = details,
                    TotalWorkDays = totalWorkDays,
                    PunctualDays = details.Count(d => d.Status == "Đúng giờ"),
                    LateDays = details.Count(d => d.Status == "Muộn"),
                    AbsentDays = details.Count(d => d.Status == "Nghỉ"),
                    TotalWorkMinutes = totalWorkMinutes,
                    AverageWorkMinutesPerDay = avgWorkMinutesPerDay,
                    WorkDaysByShift = details.GroupBy(d => d.Shift).ToDictionary(g => g.Key, g => g.Count()),
                    WorkMinutesByShift = details.GroupBy(d => d.Shift).ToDictionary(g => g.Key, g => g.Sum(d => d.WorkMinutes ?? 0))
                };
            }
            catch (Exception)
            {
                return new ShiftAttendanceReportDto();
            }
        }"""

new_content = pattern.sub(replacement, content, count=1)
if new_content == content:
    print('Pattern not found or no change')
else:
    with open(path, 'w', encoding='utf-8') as f:
        f.write(new_content)
    print('Success')
