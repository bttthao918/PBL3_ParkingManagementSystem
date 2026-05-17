using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Interfaces;
using ParkingManagement.DAL.Models;

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
            var range = NormalizeDateRange(from, to);
            var payments = await _paymentRepo.GetAllAsync();
            var tickets = await _ticketRepo.GetAllAsync();
            var monthlyTickets = await _monthlyRepo.GetAllAsync();
            var employees = await _employeeRepo.GetAllAsync();

            return BuildRevenueReport(payments, tickets, monthlyTickets, employees, range.From, range.To);
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
                    .Where(p => p.PaymentTime.Date == today && PaymentStatuses.IsSuccessful(p.Status))
                    .Sum(p => p.Amount);

                var thisMonthRevenue = payments
                    .Where(p => p.PaymentTime >= monthStart && PaymentStatuses.IsSuccessful(p.Status))
                    .Sum(p => p.Amount);

                var thisYearRevenue = payments
                    .Where(p => p.PaymentTime >= yearStart && PaymentStatuses.IsSuccessful(p.Status))
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
                var monthlyTickets = (await _monthlyRepo.GetAllAsync()).ToList();
                var payments = (await _paymentRepo.GetAllAsync()).ToList();
                var employees = (await _employeeRepo.GetAllAsync()).ToList();

                var range = NormalizeDateRange(filter.FromDate, filter.ToDate, filter.Period);
                return BuildRevenueReport(
                    payments,
                    tickets,
                    monthlyTickets,
                    employees,
                    range.From,
                    range.To,
                    vehicleType: filter.VehicleType);
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
                var employeePayments = allPayments
                    .Where(p => string.Equals(p.CollectedByEmployeeId, employeeId, StringComparison.OrdinalIgnoreCase))
                    .Where(p => PaymentStatuses.IsSuccessful(p.Status))
                    .ToList();

                var today = DateTime.Now.Date;
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var thisMonthStart = new DateTime(today.Year, today.Month, 1);

                var ticketsToday = employeePayments.Count(p => p.PaymentTime.Date == today);
                var revenueToday = employeePayments
                    .Where(p => p.PaymentTime.Date == today)
                    .Sum(p => p.Amount);

                var workMinutesToday = allTickets
                    .Where(t => t.CheckInTime.Date == today && t.CheckOutTime.HasValue)
                    .Sum(t => (int)(t.CheckOutTime.Value - t.CheckInTime).TotalMinutes);

                var ticketsThisWeek = employeePayments.Count(p => p.PaymentTime.Date >= thisWeekStart && p.PaymentTime.Date <= today);
                var revenueThisWeek = employeePayments
                    .Where(p => p.PaymentTime.Date >= thisWeekStart && p.PaymentTime.Date <= today)
                    .Sum(p => p.Amount);

                var workMinutesThisWeek = allTickets
                    .Where(t => t.CheckInTime.Date >= thisWeekStart && t.CheckInTime.Date <= today && t.CheckOutTime.HasValue)
                    .Sum(t => (int)(t.CheckOutTime.Value - t.CheckInTime).TotalMinutes);

                var workDaysThisWeek = allTickets
                    .Where(t => t.CheckInTime.Date >= thisWeekStart && t.CheckInTime.Date <= today)
                    .Select(t => t.CheckInTime.Date)
                    .Distinct()
                    .Count();

                var ticketsThisMonth = employeePayments.Count(p => p.PaymentTime >= thisMonthStart && p.PaymentTime <= today.AddDays(1).AddTicks(-1));
                var revenueThisMonth = employeePayments
                    .Where(p => p.PaymentTime >= thisMonthStart && p.PaymentTime <= today.AddDays(1).AddTicks(-1))
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
                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();
                var allMonthlyTickets = (await _monthlyRepo.GetAllAsync()).ToList();
                var allPayments = (await _paymentRepo.GetAllAsync()).ToList();
                var employees = (await _employeeRepo.GetAllAsync()).ToList();
                var range = NormalizeDateRange(null, null, period);

                var report = BuildRevenueReport(
                    allPayments,
                    allTickets,
                    allMonthlyTickets,
                    employees,
                    range.From,
                    range.To,
                    employeeId);

                var paymentsInPeriod = allPayments
                    .Where(p => p.PaymentTime >= range.From && p.PaymentTime <= range.To)
                    .Where(p => PaymentStatuses.IsSuccessful(p.Status))
                    .Where(p => string.Equals(p.CollectedByEmployeeId, employeeId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var dailyBreakdown = paymentsInPeriod
                    .GroupBy(p => p.PaymentTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DailyRevenueDetailDto
                    {
                        Date = g.Key,
                        TicketCount = g.Count(),
                        TotalRevenue = g.Sum(p => p.Amount),
                        AverageRevenuePerTicket = g.Count() > 0 ? g.Sum(p => p.Amount) / g.Count() : 0
                    })
                    .ToList();

                var previousRange = period switch
                {
                    "day" => (From: range.From.AddDays(-1), To: range.From.AddTicks(-1)),
                    "week" => (From: range.From.AddDays(-7), To: range.From.AddTicks(-1)),
                    "year" => (From: range.From.AddYears(-1), To: range.From.AddTicks(-1)),
                    _ => (From: range.From.AddMonths(-1), To: range.From.AddTicks(-1))
                };

                var prevPayments = allPayments
                    .Where(p => p.PaymentTime >= previousRange.From && p.PaymentTime <= previousRange.To)
                    .Where(p => PaymentStatuses.IsSuccessful(p.Status))
                    .Where(p => string.Equals(p.CollectedByEmployeeId, employeeId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var prevRevenue = prevPayments.Sum(p => p.Amount);

                var revenueChange = prevRevenue > 0 ? ((report.TotalRevenue - prevRevenue) / prevRevenue) * 100 : 0;
                var trend = revenueChange > 5 ? "↑ Tăng" : revenueChange < -5 ? "↓ Giảm" : "→ Ổn định";

                var topDays = dailyBreakdown.OrderByDescending(d => d.TotalRevenue).Take(5).ToList();

                return new EmployeeRevenueReportDto
                {
                    PeriodStart = report.From,
                    PeriodEnd = report.To,
                    TotalRevenue = report.TotalRevenue,
                    TotalTickets = paymentsInPeriod.Count,
                    AverageRevenuePerTicket = paymentsInPeriod.Count > 0 ? report.TotalRevenue / paymentsInPeriod.Count : 0,
                    TicketsByVehicleType = BuildTicketCountsByVehicle(paymentsInPeriod, allTickets, allMonthlyTickets),
                    RevenueByVehicleType = report.RevenueByVehicleType,
                    RevenueByPaymentMethod = report.RevenueByPaymentMethod,
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

        private static (DateTime From, DateTime To) NormalizeDateRange(DateTime? fromDate, DateTime? toDate, string period = "month")
        {
            var now = DateTime.Now;
            DateTime from;
            DateTime to;

            if (fromDate.HasValue || toDate.HasValue)
            {
                from = fromDate?.Date ?? now.AddMonths(-1).Date;
                to = toDate?.Date ?? now.Date;
            }
            else
            {
                to = now.Date;
                from = period switch
                {
                    "day" => to,
                    "week" => to.AddDays(-(int)to.DayOfWeek),
                    "year" => new DateTime(to.Year, 1, 1),
                    _ => new DateTime(to.Year, to.Month, 1)
                };
            }

            if (to < from)
                (from, to) = (to, from);

            return (from, to.Date.AddDays(1).AddTicks(-1));
        }

        private static RevenueReportDto BuildRevenueReport(
            IEnumerable<Payment> allPayments,
            IEnumerable<Ticket> allTickets,
            IEnumerable<MonthlyTicket> allMonthlyTickets,
            IEnumerable<Employee> allEmployees,
            DateTime from,
            DateTime to,
            string? employeeId = null,
            string? vehicleType = null)
        {
            var ticketLookup = allTickets.ToDictionary(t => t.TicketId);
            var monthlyLookup = allMonthlyTickets.ToDictionary(m => m.MonthlyTicketId);
            var employeeLookup = allEmployees.ToDictionary(e => e.EmployeeId);

            var payments = allPayments
                .Where(p => p.PaymentTime >= from && p.PaymentTime <= to)
                .Where(p => PaymentStatuses.IsSuccessful(p.Status))
                .ToList();

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                payments = payments
                    .Where(p => string.Equals(p.CollectedByEmployeeId, employeeId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(vehicleType))
            {
                payments = payments
                    .Where(p => string.Equals(GetVehicleType(p, ticketLookup, monthlyLookup), vehicleType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var singlePayments = payments.Where(p => p.TicketId != null).ToList();
            var monthlyPayments = payments.Where(p => p.MonthlyTicketId != null).ToList();

            return new RevenueReportDto
            {
                From = from.Date,
                To = to.Date,
                TotalRevenue = payments.Sum(p => p.Amount),
                TotalTickets = singlePayments.Count,
                TotalMonthlyTickets = monthlyPayments.Count,
                RevenueFromSingleTickets = singlePayments.Sum(p => p.Amount),
                RevenueFromMonthlyTickets = monthlyPayments.Sum(p => p.Amount),
                DailyBreakdown = payments
                    .GroupBy(p => p.PaymentTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DailyRevenueDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(p => p.Amount),
                        TicketCount = g.Count()
                    })
                    .ToList(),
                RevenueByPaymentMethod = payments
                    .GroupBy(p => PaymentMethods.Normalize(p.Method))
                    .OrderByDescending(g => g.Sum(p => p.Amount))
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                RevenueByVehicleType = payments
                    .GroupBy(p => GetVehicleType(p, ticketLookup, monthlyLookup) ?? "Không xác định")
                    .OrderByDescending(g => g.Sum(p => p.Amount))
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                RevenueByArea = payments
                    .Where(p => p.TicketId != null)
                    .GroupBy(p => GetAreaName(p, ticketLookup))
                    .OrderByDescending(g => g.Sum(p => p.Amount))
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                TopEmployees = payments
                    .Where(p => !string.IsNullOrWhiteSpace(p.CollectedByEmployeeId))
                    .GroupBy(p => p.CollectedByEmployeeId!)
                    .OrderByDescending(g => g.Sum(p => p.Amount))
                    .Take(5)
                    .Select(g => new EmployeeRevenueSummaryDto
                    {
                        EmployeeId = g.Key,
                        EmployeeName = employeeLookup.TryGetValue(g.Key, out var employee)
                            ? employee.FullName
                            : $"Nhân viên {g.Key}",
                        TotalRevenue = g.Sum(p => p.Amount),
                        PaymentCount = g.Count()
                    })
                    .ToList()
            };
        }

        private static string? GetVehicleType(
            Payment payment,
            IReadOnlyDictionary<string, Ticket> ticketLookup,
            IReadOnlyDictionary<string, MonthlyTicket> monthlyLookup)
        {
            if (!string.IsNullOrWhiteSpace(payment.TicketId) &&
                ticketLookup.TryGetValue(payment.TicketId, out var ticket))
                return ticket.VehicleType;

            if (!string.IsNullOrWhiteSpace(payment.MonthlyTicketId) &&
                monthlyLookup.TryGetValue(payment.MonthlyTicketId, out var monthlyTicket))
                return monthlyTicket.VehicleType;

            return null;
        }

        private static Dictionary<string, int> BuildTicketCountsByVehicle(
            IEnumerable<Payment> payments,
            IEnumerable<Ticket> allTickets,
            IEnumerable<MonthlyTicket> allMonthlyTickets)
        {
            var ticketLookup = allTickets.ToDictionary(t => t.TicketId);
            var monthlyLookup = allMonthlyTickets.ToDictionary(m => m.MonthlyTicketId);

            return payments
                .GroupBy(p => GetVehicleType(p, ticketLookup, monthlyLookup) ?? "Không xác định")
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private static string GetAreaName(Payment payment, IReadOnlyDictionary<string, Ticket> ticketLookup)
        {
            if (string.IsNullOrWhiteSpace(payment.TicketId) ||
                !ticketLookup.TryGetValue(payment.TicketId, out var ticket) ||
                string.IsNullOrWhiteSpace(ticket.SlotId))
                return "Không xác định";

            return char.ToUpperInvariant(ticket.SlotId[0]) switch
            {
                'A' => "Khu A",
                'B' => "Khu B",
                'C' => "Khu C",
                _ => "Khu khác"
            };
        }
    }
}
