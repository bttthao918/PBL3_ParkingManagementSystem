using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    /// <summary>
    /// Consolidated Report Service
    /// Combines: ReportService + ManagerReportService + EmployeeReportService
    /// Provides permission-based data filtering based on caller role (handled at controller level)
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IMonthlyTicketRepository _monthlyRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IParkingSlotRepository _parkingSlotRepo;

        public ReportService(
            IPaymentRepository paymentRepo,
            IMonthlyTicketRepository monthlyRepo,
            ITicketRepository ticketRepo,
            ICustomerRepository customerRepo,
            IEmployeeRepository employeeRepo,
            IParkingSlotRepository parkingSlotRepo)
        {
            _paymentRepo = paymentRepo;
            _monthlyRepo = monthlyRepo;
            _ticketRepo = ticketRepo;
            _customerRepo = customerRepo;
            _employeeRepo = employeeRepo;
            _parkingSlotRepo = parkingSlotRepo;
        }

        // ── 1. Basic Revenue Reports ──
        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to)
        {
            var payments = await _paymentRepo.GetByDateRangeAsync(from, to);

            var singlePayments = payments.Where(p => p.TicketId != null).ToList();
            var monthlyPayments = payments.Where(p => p.MonthlyTicketId != null).ToList();

            var daily = payments
                .GroupBy(p => p.PaymentTime.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    TicketCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new RevenueReportDto
            {
                From = from,
                To = to,
                TotalRevenue = payments.Sum(p => p.Amount),
                TotalTickets = singlePayments.Count,
                TotalMonthlyTickets = monthlyPayments.Count,
                RevenueFromSingleTickets = singlePayments.Sum(p => p.Amount),
                RevenueFromMonthlyTickets = monthlyPayments.Sum(p => p.Amount),
                DailyBreakdown = daily
            };
        }

        public async Task<List<MonthlyTicketDto>> GetExpiringSoonAsync(int days = 7)
        {
            var list = await _monthlyRepo.GetExpiringSoonAsync(days);
            return list.Select(m => new MonthlyTicketDto
            {
                MonthlyTicketId = m.MonthlyTicketId,
                CustomerName = m.Customer?.FullName ?? "",
                VehiclePlate = m.VehiclePlate,
                VehicleType = m.VehicleType,
                PackageType = m.PackageType,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                TotalFee = m.TotalFee,
                Status = m.Status,
                DaysRemaining = Math.Max(0, (int)(m.EndDate - DateTime.Today).TotalDays)
            }).ToList();
        }

        public async Task<int> CountActiveVehiclesAsync()
        {
            var tickets = await _ticketRepo.GetActiveTicketsAsync();
            return tickets.Count;
        }

        // ── 2. Manager Dashboard & Reports ──
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            try
            {
                var today = DateTime.Now.Date;
                var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var yearStart = new DateTime(DateTime.Now.Year, 1, 1);

                var tickets = (await _ticketRepo.GetAllAsync()).ToList();
                var payments = (await _paymentRepo.GetAllAsync()).ToList();
                var customers = (await _customerRepo.GetAllAsync()).ToList();
                var employees = (await _employeeRepo.GetAllAsync()).Where(e => !e.IsDeleted).ToList();
                var slots = (await _parkingSlotRepo.GetAllAsync()).ToList();
                var monthlyTickets = (await _monthlyRepo.GetAllAsync()).ToList();

                var todayRevenue = payments
                    .Where(p => p.PaymentTime.Date == today && p.Status == "Thành công")
                    .Sum(p => p.Amount);

                var thisMonthRevenue = payments
                    .Where(p => p.PaymentTime >= monthStart && p.Status == "Thành công")
                    .Sum(p => p.Amount);

                var thisYearRevenue = payments
                    .Where(p => p.PaymentTime >= yearStart && p.Status == "Thành công")
                    .Sum(p => p.Amount);

                var todayTickets = tickets.Count(t => t.CheckInTime.Date == today);
                var thisMonthTickets = tickets.Count(t => t.CheckInTime >= monthStart);

                var occupiedSlots = slots.Count(s => s.Status == "Đang sử dụng");
                var totalSlots = slots.Count;
                var slotUtilization = totalSlots > 0 ? (double)occupiedSlots / totalSlots * 100 : 0;

                var employeesOnline = employees.Count(e => e.Account?.IsActive == true);
                var activeMonthlyTickets = monthlyTickets.Count(m => m.Status == "Hoạt động");

                return new DashboardSummaryDto
                {
                    TodayRevenue = todayRevenue,
                    ThisMonthRevenue = thisMonthRevenue,
                    ThisYearRevenue = thisYearRevenue,
                    TodayTickets = todayTickets,
                    ThisMonthTickets = thisMonthTickets,
                    SlotUtilizationRate = (decimal)slotUtilization,
                    OccupiedSlots = occupiedSlots,
                    TotalSlots = totalSlots,
                    TotalActiveEmployees = employees.Count,
                    EmployeesOnline = employeesOnline,
                    TotalCustomers = customers.Count,
                    ActiveMonthlyTickets = activeMonthlyTickets
                };
            }
            catch (Exception)
            {
                return new DashboardSummaryDto();
            }
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(RevenueReportFilterDto filter)
        {
            try
            {
                var tickets = (await _ticketRepo.GetAllAsync()).ToList();
                var payments = (await _paymentRepo.GetAllAsync()).ToList();

                var from = filter.FromDate ?? DateTime.Now.AddMonths(-1);
                var to = filter.ToDate ?? DateTime.Now;

                var periodTickets = tickets.Where(t => t.CheckInTime >= from && t.CheckInTime <= to).ToList();
                var periodPayments = payments
                    .Where(p => p.PaymentTime >= from && p.PaymentTime <= to && p.Status == "Thành công")
                    .ToList();

                var totalRevenue = periodPayments.Sum(p => p.Amount);
                var ticketCount = periodTickets.Count;
                var avgRevenuePerTicket = ticketCount > 0 ? totalRevenue / ticketCount : 0;

                var revenueByVehicle = periodTickets
                    .GroupBy(t => t.VehicleType)
                    .ToDictionary(
                        g => g.Key,
                        g => (decimal)periodPayments.Where(p => p.TicketId != null).Sum(p => p.Amount)
                    );

                var ticketsByVehicle = periodTickets
                    .GroupBy(t => t.VehicleType)
                    .ToDictionary(g => g.Key, g => g.Count());

                var revenueByMethod = periodPayments
                    .GroupBy(p => p.Method)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

                var topCustomers = periodTickets
                    .GroupBy(t => t.CustomerId)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new TopCustomerDto
                    {
                        CustomerId = g.Key,
                        CustomerName = g.First().Customer?.FullName ?? "Unknown",
                        TicketCount = g.Count(),
                        TotalSpent = (decimal)periodPayments.Where(p => p.TicketId != null).Sum(p => p.Amount)
                    })
                    .ToList();

                return new RevenueReportDto
                {
                    From = from,
                    To = to,
                    TotalRevenue = totalRevenue,
                    TotalTickets = ticketCount,
                    TotalMonthlyTickets = 0,
                    RevenueFromSingleTickets = totalRevenue,
                    RevenueFromMonthlyTickets = 0,
                    DailyBreakdown = new()
                };
            }
            catch (Exception)
            {
                return new RevenueReportDto();
            }
        }

        public async Task<CustomerReportDto> GetCustomerReportAsync()
        {
            try
            {
                var customers = (await _customerRepo.GetAllAsync()).ToList();
                var tickets = (await _ticketRepo.GetAllAsync()).ToList();
                var monthlyTickets = (await _monthlyRepo.GetAllAsync()).ToList();

                var regularCustomers = customers.Count(c => 
                    tickets.Count(t => t.CustomerId == c.CustomerId) > 10);
                var vipCustomers = monthlyTickets.Count(m => m.Status == "Hoạt động");
                var oneTimeCustomers = customers.Count(c => 
                    tickets.Count(t => t.CustomerId == c.CustomerId) == 1);

                var topCustomers = customers
                    .Select(c => new CustomerDetailDto
                    {
                        CustomerId = c.CustomerId,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber ?? "",
                        TicketCount = tickets.Count(t => t.CustomerId == c.CustomerId),
                        TotalSpent = 0,
                        HasActiveMonthlyTicket = monthlyTickets.Any(m => m.CustomerId == c.CustomerId && m.Status == "Hoạt động"),
                        LastVisit = tickets.Where(t => t.CustomerId == c.CustomerId).Max(t => (DateTime?)t.CheckInTime)
                    })
                    .OrderByDescending(c => c.TicketCount)
                    .Take(10)
                    .ToList();

                return new CustomerReportDto
                {
                    TotalCustomers = customers.Count,
                    NewCustomersThisMonth = customers.Count(),
                    ActiveMonthlyTickets = monthlyTickets.Count(m => m.Status == "Hoạt động"),
                    ExpiredMonthlyTickets = monthlyTickets.Count(m => m.Status == "Hết hạn"),
                    RegularCustomers = regularCustomers,
                    VIPCustomers = vipCustomers,
                    OneTimeCustomers = oneTimeCustomers,
                    TopCustomers = topCustomers
                };
            }
            catch (Exception)
            {
                return new CustomerReportDto();
            }
        }

        // ── 3. Employee Reports ──
        public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(string employeeId)
        {
            try
            {
                var employee = await _employeeRepo.GetByIdAsync(employeeId);
                if (employee == null)
                    throw new Exception("Nhân viên không tồn tại");

                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();
                var allPayments = (await _paymentRepo.GetAllAsync()).ToList();

                var today = DateTime.Now.Date;
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var thisMonthStart = new DateTime(today.Year, today.Month, 1);

                var ticketsToday = allTickets.Count(t => t.CheckInTime.Date == today);
                var revenueToday = allPayments
                    .Where(p => p.PaymentTime.Date == today && p.Status == "Thành công")
                    .Sum(p => p.Amount);

                var workMinutesToday = allTickets
                    .Where(t => t.CheckInTime.Date == today && t.CheckOutTime.HasValue)
                    .Sum(t => (int)(t.CheckOutTime.Value - t.CheckInTime).TotalMinutes);

                var ticketsThisWeek = allTickets.Count(t => t.CheckInTime.Date >= thisWeekStart && t.CheckInTime.Date <= today);
                var revenueThisWeek = allPayments
                    .Where(p => p.PaymentTime.Date >= thisWeekStart && p.PaymentTime.Date <= today && p.Status == "Thành công")
                    .Sum(p => p.Amount);

                var workMinutesThisWeek = allTickets
                    .Where(t => t.CheckInTime.Date >= thisWeekStart && t.CheckInTime.Date <= today && t.CheckOutTime.HasValue)
                    .Sum(t => (int)(t.CheckOutTime.Value - t.CheckInTime).TotalMinutes);

                var workDaysThisWeek = allTickets
                    .Where(t => t.CheckInTime.Date >= thisWeekStart && t.CheckInTime.Date <= today)
                    .Select(t => t.CheckInTime.Date)
                    .Distinct()
                    .Count();

                var ticketsThisMonth = allTickets.Count(t => t.CheckInTime >= thisMonthStart && t.CheckInTime <= today);
                var revenueThisMonth = allPayments
                    .Where(p => p.PaymentTime >= thisMonthStart && p.PaymentTime <= today && p.Status == "Thành công")
                    .Sum(p => p.Amount);

                var workMinutesThisMonth = allTickets
                    .Where(t => t.CheckInTime >= thisMonthStart && t.CheckInTime <= today && t.CheckOutTime.HasValue)
                    .Sum(t => (int)(t.CheckOutTime.Value - t.CheckInTime).TotalMinutes);

                var workDaysThisMonth = allTickets
                    .Where(t => t.CheckInTime >= thisMonthStart && t.CheckInTime <= today)
                    .Select(t => t.CheckInTime.Date)
                    .Distinct()
                    .Count();

                var avgRevenuePerTicket = ticketsThisMonth > 0 ? revenueThisMonth / ticketsThisMonth : 0;
                var avgTicketsPerDay = workDaysThisMonth > 0 ? (double)ticketsThisMonth / workDaysThisMonth : 0;

                return new EmployeeDashboardDto
                {
                    TicketsProcessedToday = ticketsToday,
                    RevenueToday = revenueToday,
                    WorkMinutesToday = workMinutesToday,
                    TicketsProcessedThisWeek = ticketsThisWeek,
                    RevenueThisWeek = revenueThisWeek,
                    WorkMinutesThisWeek = workMinutesThisWeek,
                    WorkDaysThisWeek = workDaysThisWeek,
                    TicketsProcessedThisMonth = ticketsThisMonth,
                    RevenueThisMonth = revenueThisMonth,
                    WorkMinutesThisMonth = workMinutesThisMonth,
                    WorkDaysThisMonth = workDaysThisMonth,
                    AverageRevenuePerTicket = avgRevenuePerTicket,
                    AverageTicketsPerDay = avgTicketsPerDay,
                    CurrentShift = employee.Shift ?? "Không xác định"
                };
            }
            catch (Exception)
            {
                return new EmployeeDashboardDto();
            }
        }

        public async Task<ShiftAttendanceReportDto> GetShiftAttendanceReportAsync(string employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var from = fromDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
                var to = toDate?.Date ?? DateTime.Now.Date;

                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();
                var ticketsInRange = allTickets
                    .Where(t => t.CheckInTime.Date >= from && t.CheckInTime.Date <= to)
                    .ToList();

                var dailyStats = ticketsInRange
                    .GroupBy(t => t.CheckInTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var dayTickets = g.ToList();
                        var workMinutes = dayTickets
                            .Where(t => t.CheckOutTime.HasValue)
                            .Sum(t => (int)(t.CheckOutTime.Value - t.CheckInTime).TotalMinutes);

                        var shiftRevenue = dayTickets.Sum(t => t.Fee);

                        return new ShiftAttendanceDetailDto
                        {
                            Date = g.Key,
                            Shift = "Không xác định",
                            CheckInTime = dayTickets.Min(t => t.CheckInTime),
                            CheckOutTime = dayTickets.Max(t => (DateTime?)(t.CheckOutTime ?? t.CheckInTime)),
                            WorkMinutes = workMinutes,
                            Status = "Đúng giờ",
                            TicketsProcessed = dayTickets.Count,
                            ShiftRevenue = shiftRevenue
                        };
                    })
                    .ToList();

                var totalWorkDays = dailyStats.Count;
                var totalWorkMinutes = dailyStats.Sum(d => d.WorkMinutes ?? 0);
                var avgWorkMinutesPerDay = totalWorkDays > 0 ? totalWorkMinutes / totalWorkDays : 0;

                var workDaysByShift = dailyStats
                    .GroupBy(d => d.Shift)
                    .ToDictionary(g => g.Key, g => g.Count());

                var workMinutesByShift = dailyStats
                    .GroupBy(d => d.Shift)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.WorkMinutes ?? 0));

                return new ShiftAttendanceReportDto
                {
                    Details = dailyStats,
                    TotalWorkDays = totalWorkDays,
                    PunctualDays = dailyStats.Count(d => d.Status == "Đúng giờ"),
                    LateDays = dailyStats.Count(d => d.Status == "Muộn"),
                    AbsentDays = dailyStats.Count(d => d.Status == "Nghỉ"),
                    TotalWorkMinutes = totalWorkMinutes,
                    AverageWorkMinutesPerDay = avgWorkMinutesPerDay,
                    WorkDaysByShift = workDaysByShift,
                    WorkMinutesByShift = workMinutesByShift
                };
            }
            catch (Exception)
            {
                return new ShiftAttendanceReportDto();
            }
        }

        public async Task<EmployeeRevenueReportDto> GetEmployeeRevenueReportAsync(string employeeId, string period = "month")
        {
            try
            {
                var today = DateTime.Now.Date;
                DateTime from;

                if (period == "day")
                    from = today;
                else if (period == "week")
                    from = today.AddDays(-(int)today.DayOfWeek);
                else
                    from = new DateTime(today.Year, today.Month, 1);

                var to = today;

                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();
                var allPayments = (await _paymentRepo.GetAllAsync()).ToList();

                var ticketsInPeriod = allTickets
                    .Where(t => t.CheckInTime.Date >= from && t.CheckInTime.Date <= to)
                    .ToList();

                var paymentsInPeriod = allPayments
                    .Where(p => p.PaymentTime.Date >= from && p.PaymentTime.Date <= to && p.Status == "Thành công")
                    .ToList();

                var totalRevenue = paymentsInPeriod.Sum(p => p.Amount);
                var totalTickets = ticketsInPeriod.Count;
                var avgRevenuePerTicket = totalTickets > 0 ? totalRevenue / totalTickets : 0;

                var ticketsByVehicle = ticketsInPeriod
                    .GroupBy(t => t.VehicleType)
                    .ToDictionary(g => g.Key, g => g.Count());

                var revenueByVehicle = new Dictionary<string, decimal>();
                foreach (var vehicleType in ticketsByVehicle.Keys)
                {
                    var revenue = ticketsInPeriod
                        .Where(t => t.VehicleType == vehicleType)
                        .Sum(t => t.Fee);
                    revenueByVehicle[vehicleType] = revenue;
                }

                var dailyBreakdown = ticketsInPeriod
                    .GroupBy(t => t.CheckInTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DailyRevenueDetailDto
                    {
                        Date = g.Key,
                        TicketCount = g.Count(),
                        TotalRevenue = g.Sum(t => t.Fee),
                        AverageRevenuePerTicket = g.Count() > 0 ? g.Sum(t => t.Fee) / g.Count() : 0
                    })
                    .ToList();

                var prevFrom = period == "day" ? from.AddDays(-1)
                             : period == "week" ? from.AddDays(-7)
                             : new DateTime(from.Year, from.Month, 1).AddMonths(-1);
                var prevTickets = allTickets.Where(t => t.CheckInTime.Date >= prevFrom && t.CheckInTime.Date < from).ToList();
                var prevPayments = allPayments.Where(p => p.PaymentTime.Date >= prevFrom && p.PaymentTime.Date < from && p.Status == "Thành công").ToList();
                var prevRevenue = prevPayments.Sum(p => p.Amount);

                var revenueChange = prevRevenue > 0 ? ((totalRevenue - prevRevenue) / prevRevenue) * 100 : 0;
                var trend = revenueChange > 5 ? "↑ Tăng" : revenueChange < -5 ? "↓ Giảm" : "→ Ổn định";

                var topDays = dailyBreakdown.OrderByDescending(d => d.TotalRevenue).Take(5).ToList();

                return new EmployeeRevenueReportDto
                {
                    PeriodStart = from,
                    PeriodEnd = to,
                    TotalRevenue = totalRevenue,
                    TotalTickets = totalTickets,
                    AverageRevenuePerTicket = avgRevenuePerTicket,
                    TicketsByVehicleType = ticketsByVehicle,
                    RevenueByVehicleType = revenueByVehicle,
                    DailyBreakdown = dailyBreakdown,
                    PreviousPeriodRevenue = prevRevenue,
                    RevenueChangePercentage = (decimal)revenueChange,
                    Trend = trend,
                    TopDays = topDays
                };
            }
            catch (Exception)
            {
                return new EmployeeRevenueReportDto();
            }
        }
    }
}
