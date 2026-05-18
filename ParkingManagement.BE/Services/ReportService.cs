using Microsoft.EntityFrameworkCore;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Data;
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
        private readonly AppDbContext _db;

        public ReportService(
            IPaymentRepository paymentRepo,
            IMonthlyTicketRepository monthlyRepo,
            ITicketRepository ticketRepo,
            ICustomerRepository customerRepo,
            IEmployeeRepository employeeRepo,
            IParkingSlotRepository parkingSlotRepo,
            AppDbContext db)
        {
            _paymentRepo = paymentRepo;
            _monthlyRepo = monthlyRepo;
            _ticketRepo = ticketRepo;
            _customerRepo = customerRepo;
            _employeeRepo = employeeRepo;
            _parkingSlotRepo = parkingSlotRepo;
            _db = db;
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

        private static (string Period, DateTime From, DateTime To, DateTime PreviousFrom, DateTime PreviousTo)
            ResolveRevenueReportRange(RevenueReportFilterDto filter)
        {
            var period = NormalizeRevenuePeriod(filter.Period);
            if (filter.FromDate.HasValue || filter.ToDate.HasValue)
            {
                var from = (filter.FromDate ?? DateTime.Now.AddDays(-29)).Date;
                var to = (filter.ToDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);
                if (to < from)
                {
                    (from, to) = (to.Date, from.Date.AddDays(1).AddTicks(-1));
                }

                var days = Math.Max(1, (to.Date - from).Days + 1);
                return ("custom", from, to, from.AddDays(-days), from.AddTicks(-1));
            }

            var today = DateTime.Now.Date;
            DateTime fromDate;

            switch (period)
            {
                case "today":
                case "day":
                    fromDate = today;
                    period = "today";
                    break;
                case "7days":
                case "week":
                    fromDate = today.AddDays(-6);
                    period = "7days";
                    break;
                case "month":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    break;
                default:
                    fromDate = today.AddDays(-29);
                    period = "30days";
                    break;
            }

            var toDate = today.AddDays(1).AddTicks(-1);
            var rangeDays = Math.Max(1, (toDate.Date - fromDate).Days + 1);
            return (period, fromDate, toDate, fromDate.AddDays(-rangeDays), fromDate.AddTicks(-1));
        }

        private static string NormalizeRevenuePeriod(string? period)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" => "today",
                "day" => "day",
                "7days" => "7days",
                "week" => "week",
                "month" => "month",
                "30days" => "30days",
                _ => "30days"
            };
        }

        private async Task<RevenueReportDto> BuildRevenueReportAsync(
            string period,
            DateTime from,
            DateTime to,
            DateTime previousFrom,
            DateTime previousTo)
        {
            var payments = (await _paymentRepo.GetAllAsync())
                .Where(p => IsSuccessfulPaymentStatus(p.Status))
                .ToList();
            var tickets = (await _ticketRepo.GetAllAsync()).ToList();
            var monthlyTickets = (await _monthlyRepo.GetAllAsync()).ToList();
            var slots = (await _parkingSlotRepo.GetAllAsync()).ToList();

            var ticketsById = tickets.ToDictionary(t => t.TicketId);
            var monthlyTicketsById = monthlyTickets.ToDictionary(m => m.MonthlyTicketId);
            var slotAreaById = slots.ToDictionary(s => s.SlotId, s => ResolveAreaLabel(s.Location));

            var periodPayments = payments
                .Where(p => p.PaymentTime >= from && p.PaymentTime <= to)
                .ToList();
            var previousPayments = payments
                .Where(p => p.PaymentTime >= previousFrom && p.PaymentTime <= previousTo)
                .ToList();

            var singlePayments = periodPayments.Where(p => !string.IsNullOrWhiteSpace(p.TicketId)).ToList();
            var monthlyPayments = periodPayments.Where(p => !string.IsNullOrWhiteSpace(p.MonthlyTicketId)).ToList();

            var dailyBreakdown = BuildDailyRevenue(periodPayments, from, to);
            var previousDailyBreakdown = BuildDailyRevenue(previousPayments, previousFrom, previousTo);
            var topRevenueDays = BuildTopRevenueDays(dailyBreakdown, previousDailyBreakdown);

            return new RevenueReportDto
            {
                Period = period,
                From = from,
                To = to,
                TotalRevenue = periodPayments.Sum(p => p.Amount),
                TotalTickets = singlePayments.Count,
                TotalMonthlyTickets = monthlyPayments.Count,
                RevenueFromSingleTickets = singlePayments.Sum(p => p.Amount),
                RevenueFromMonthlyTickets = monthlyPayments.Sum(p => p.Amount),
                DailyBreakdown = dailyBreakdown,
                PreviousDailyBreakdown = previousDailyBreakdown,
                RevenueByPaymentMethod = BuildRevenueBreakdown(
                    periodPayments,
                    p => string.IsNullOrWhiteSpace(p.Method) ? "Không xác định" : p.Method),
                RevenueByVehicleType = BuildRevenueBreakdown(
                    periodPayments,
                    p => ResolvePaymentVehicleType(p, ticketsById, monthlyTicketsById)),
                RevenueByArea = BuildRevenueBreakdown(
                    periodPayments,
                    p => ResolvePaymentArea(p, ticketsById, monthlyTicketsById, slotAreaById)),
                TopRevenueDays = topRevenueDays
            };
        }

        private static List<DailyRevenueDto> BuildDailyRevenue(List<Payment> payments, DateTime from, DateTime to)
        {
            var paymentsByDate = payments
                .GroupBy(p => p.PaymentTime.Date)
                .ToDictionary(g => g.Key, g => new
                {
                    Revenue = g.Sum(p => p.Amount),
                    Count = g.Count()
                });

            var days = Math.Max(1, (to.Date - from.Date).Days + 1);
            return Enumerable.Range(0, days)
                .Select(i =>
                {
                    var date = from.Date.AddDays(i);
                    var value = paymentsByDate.GetValueOrDefault(date);
                    return new DailyRevenueDto
                    {
                        Date = date,
                        Label = date.ToString("dd/MM"),
                        Revenue = value?.Revenue ?? 0,
                        TicketCount = value?.Count ?? 0
                    };
                })
                .ToList();
        }

        private static Dictionary<string, decimal> BuildRevenueBreakdown(
            List<Payment> payments,
            Func<Payment, string> labelSelector)
        {
            if (payments.Count == 0)
            {
                return new Dictionary<string, decimal> { ["Chưa có dữ liệu"] = 0 };
            }

            return payments
                .GroupBy(labelSelector)
                .OrderByDescending(g => g.Sum(p => p.Amount))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
        }

        private static List<RevenueRankDto> BuildTopRevenueDays(
            List<DailyRevenueDto> current,
            List<DailyRevenueDto> previous)
        {
            return current
                .Where(d => d.Revenue > 0 || d.TicketCount > 0)
                .OrderByDescending(d => d.Revenue)
                .Take(5)
                .Select(d =>
                {
                    var index = current.IndexOf(d);
                    var previousRevenue = index >= 0 && index < previous.Count ? previous[index].Revenue : 0;
                    return new RevenueRankDto
                    {
                        Label = d.Date.ToString("dd/MM/yyyy"),
                        Amount = d.Revenue,
                        Count = d.TicketCount,
                        ChangePercentage = CalculateRevenueChangePercentage(d.Revenue, previousRevenue)
                    };
                })
                .ToList();
        }

        private static string ResolvePaymentVehicleType(
            Payment payment,
            Dictionary<string, Ticket> ticketsById,
            Dictionary<string, MonthlyTicket> monthlyTicketsById)
        {
            if (!string.IsNullOrWhiteSpace(payment.TicketId)
                && ticketsById.TryGetValue(payment.TicketId, out var ticket)
                && !string.IsNullOrWhiteSpace(ticket.VehicleType))
            {
                return ticket.VehicleType;
            }

            if (!string.IsNullOrWhiteSpace(payment.MonthlyTicketId)
                && monthlyTicketsById.TryGetValue(payment.MonthlyTicketId, out var monthlyTicket)
                && !string.IsNullOrWhiteSpace(monthlyTicket.VehicleType))
            {
                return monthlyTicket.VehicleType;
            }

            return "Không xác định";
        }

        private static string ResolvePaymentArea(
            Payment payment,
            Dictionary<string, Ticket> ticketsById,
            Dictionary<string, MonthlyTicket> monthlyTicketsById,
            Dictionary<string, string> slotAreaById)
        {
            if (!string.IsNullOrWhiteSpace(payment.TicketId)
                && ticketsById.TryGetValue(payment.TicketId, out var ticket))
            {
                if (!string.IsNullOrWhiteSpace(ticket.SlotId)
                    && slotAreaById.TryGetValue(ticket.SlotId, out var area))
                {
                    return area;
                }

                return "Chưa gán khu vực";
            }

            if (!string.IsNullOrWhiteSpace(payment.MonthlyTicketId)
                && monthlyTicketsById.ContainsKey(payment.MonthlyTicketId))
            {
                return "Vé tháng";
            }

            return "Không xác định";
        }

        private static decimal RevenuePercentage(decimal amount, decimal total)
        {
            return total <= 0 ? 0 : Math.Round(amount * 100m / total, 1);
        }

        private static decimal CalculateRevenueChangePercentage(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }

            return Math.Round((current - previous) * 100m / previous, 1);
        }

        public async Task<CustomerReportDto> GetCustomerReportAsync(string period = "30days")
        {
            try
            {
                var range = ResolveCustomerReportRange(period);
                var today = DateTime.Now.Date;
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var customers = (await _customerRepo.GetAllAsync()).ToList();
                var tickets = (await _ticketRepo.GetAllAsync()).ToList();
                var payments = (await _paymentRepo.GetAllAsync()).ToList();
                var monthlyTickets = (await _monthlyRepo.GetAllAsync()).ToList();
                var slots = (await _parkingSlotRepo.GetAllAsync()).ToList();

                var customersById = customers.ToDictionary(c => c.CustomerId);
                var ticketsById = tickets.ToDictionary(t => t.TicketId);
                var monthlyTicketsById = monthlyTickets.ToDictionary(m => m.MonthlyTicketId);
                var slotAreaById = slots.ToDictionary(s => s.SlotId, s => ResolveAreaLabel(s.Location));

                var periodTickets = tickets
                    .Where(t => t.CheckInTime >= range.From && t.CheckInTime <= range.To)
                    .ToList();

                var previousTickets = tickets
                    .Where(t => t.CheckInTime >= range.PreviousFrom && t.CheckInTime <= range.PreviousTo)
                    .ToList();

                var periodPayments = payments
                    .Where(p => p.PaymentTime >= range.From && p.PaymentTime <= range.To && IsSuccessfulPaymentStatus(p.Status))
                    .ToList();

                var activeMonthlyTickets = monthlyTickets
                    .Where(m => IsActiveMonthlyTicket(m, today))
                    .ToList();

                var expiredMonthlyTickets = monthlyTickets
                    .Where(m => IsExpiredMonthlyTicket(m, today))
                    .ToList();

                var activeMonthlyCustomerIds = activeMonthlyTickets
                    .Select(m => m.CustomerId)
                    .Distinct()
                    .ToHashSet();

                var periodTicketCounts = periodTickets
                    .Where(t => !string.IsNullOrWhiteSpace(t.CustomerId))
                    .GroupBy(t => t.CustomerId!)
                    .ToDictionary(g => g.Key, g => g.Count());

                var previousTicketCounts = previousTickets
                    .Where(t => !string.IsNullOrWhiteSpace(t.CustomerId))
                    .GroupBy(t => t.CustomerId!)
                    .ToDictionary(g => g.Key, g => g.Count());

                var spendingByCustomer = BuildPeriodSpendingByCustomer(
                    periodPayments,
                    periodTickets,
                    ticketsById,
                    monthlyTicketsById);

                var vipCustomerIds = periodTicketCounts
                    .Where(x => x.Value > 20 || activeMonthlyCustomerIds.Contains(x.Key))
                    .Select(x => x.Key)
                    .Concat(activeMonthlyCustomerIds)
                    .Where(customersById.ContainsKey)
                    .Distinct()
                    .ToHashSet();

                var regularCustomers = periodTicketCounts.Count(x => x.Value >= 2 && !vipCustomerIds.Contains(x.Key));
                var oneTimeCustomers = periodTicketCounts.Count(x => x.Value == 1 && !vipCustomerIds.Contains(x.Key));
                var walkInTickets = periodTickets.Count(t => string.IsNullOrWhiteSpace(t.CustomerId));
                var returningCustomers = periodTicketCounts.Count(x => x.Value >= 2);
                var groupTotal = Math.Max(0, walkInTickets + oneTimeCustomers + regularCustomers + vipCustomerIds.Count);

                var groupBreakdown = new List<CustomerBreakdownDto>
                {
                    new()
                    {
                        Label = "Khách vãng lai",
                        Count = walkInTickets + oneTimeCustomers,
                        Percentage = Percentage(walkInTickets + oneTimeCustomers, groupTotal)
                    },
                    new()
                    {
                        Label = "Khách thân thiết",
                        Count = regularCustomers,
                        Percentage = Percentage(regularCustomers, groupTotal)
                    },
                    new()
                    {
                        Label = "Khách VIP",
                        Count = vipCustomerIds.Count,
                        Percentage = Percentage(vipCustomerIds.Count, groupTotal)
                    }
                };

                var topCustomers = periodTicketCounts
                    .Where(x => customersById.ContainsKey(x.Key))
                    .OrderByDescending(x => x.Value)
                    .ThenByDescending(x => spendingByCustomer.GetValueOrDefault(x.Key))
                    .Take(10)
                    .Select(x =>
                    {
                        var customer = customersById[x.Key];
                        var previousCount = previousTicketCounts.GetValueOrDefault(x.Key);

                        return new CustomerDetailDto
                        {
                            CustomerId = customer.CustomerId,
                            FullName = customer.FullName,
                            PhoneNumber = customer.PhoneNumber ?? "",
                            TicketCount = x.Value,
                            TotalSpent = spendingByCustomer.GetValueOrDefault(customer.CustomerId),
                            HasActiveMonthlyTicket = activeMonthlyCustomerIds.Contains(customer.CustomerId),
                            LastVisit = periodTickets
                                .Where(t => t.CustomerId == customer.CustomerId)
                                .Max(t => (DateTime?)t.CheckInTime),
                            RegisteredAt = GetCustomerCreatedAt(customer),
                            VisitChangePercentage = CalculateChangePercentage(x.Value, previousCount)
                        };
                    })
                    .ToList();

                var newCustomers = customers
                    .Where(c =>
                    {
                        var createdAt = GetCustomerCreatedAt(c);
                        return createdAt >= range.From && createdAt <= range.To;
                    })
                    .OrderByDescending(GetCustomerCreatedAt)
                    .Take(10)
                    .Select(c => new CustomerDetailDto
                    {
                        CustomerId = c.CustomerId,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber ?? "",
                        TicketCount = periodTicketCounts.GetValueOrDefault(c.CustomerId),
                        TotalSpent = spendingByCustomer.GetValueOrDefault(c.CustomerId),
                        HasActiveMonthlyTicket = activeMonthlyCustomerIds.Contains(c.CustomerId),
                        LastVisit = periodTickets
                            .Where(t => t.CustomerId == c.CustomerId)
                            .Max(t => (DateTime?)t.CheckInTime),
                        RegisteredAt = GetCustomerCreatedAt(c),
                        VisitChangePercentage = CalculateChangePercentage(
                            periodTicketCounts.GetValueOrDefault(c.CustomerId),
                            previousTicketCounts.GetValueOrDefault(c.CustomerId))
                    })
                    .ToList();

                var currentTrend = BuildNewCustomerTrend(customers, range.From, range.Days);
                var previousTrend = BuildNewCustomerTrend(customers, range.PreviousFrom, range.Days);
                var areaBreakdown = BuildAreaBreakdown(periodTickets, slotAreaById);
                var returnBuckets = BuildReturnBuckets(periodTicketCounts.Values.ToList());

                return new CustomerReportDto
                {
                    Period = range.Period,
                    From = range.From,
                    To = range.To,
                    TotalCustomers = customers.Count,
                    NewCustomersThisMonth = customers.Count(c =>
                    {
                        var createdAt = GetCustomerCreatedAt(c);
                        return createdAt >= monthStart && createdAt <= range.To;
                    }),
                    NewCustomersInPeriod = newCustomers.Count,
                    ActiveMonthlyTickets = activeMonthlyTickets.Count,
                    ExpiredMonthlyTickets = expiredMonthlyTickets.Count,
                    RegularCustomers = regularCustomers,
                    VIPCustomers = vipCustomerIds.Count,
                    OneTimeCustomers = oneTimeCustomers + walkInTickets,
                    WalkInTickets = walkInTickets,
                    ReturningCustomers = returningCustomers,
                    NewCustomerTrend = currentTrend,
                    PreviousNewCustomerTrend = previousTrend,
                    GroupBreakdown = groupBreakdown,
                    AreaBreakdown = areaBreakdown,
                    ReturnBuckets = returnBuckets,
                    TopCustomers = topCustomers,
                    NewCustomers = newCustomers
                };
            }
            catch (Exception)
            {
                return new CustomerReportDto
                {
                    Period = NormalizeCustomerReportPeriod(period)
                };
            }
        }

        private static (string Period, DateTime From, DateTime To, DateTime PreviousFrom, DateTime PreviousTo, int Days)
            ResolveCustomerReportRange(string? period)
        {
            var normalizedPeriod = NormalizeCustomerReportPeriod(period);
            var days = normalizedPeriod switch
            {
                "today" => 1,
                "7days" => 7,
                _ => 30
            };

            var today = DateTime.Now.Date;
            var from = today.AddDays(-(days - 1));
            var to = today.AddDays(1).AddTicks(-1);
            var previousFrom = from.AddDays(-days);
            var previousTo = from.AddTicks(-1);

            return (normalizedPeriod, from, to, previousFrom, previousTo, days);
        }

        private static string NormalizeCustomerReportPeriod(string? period)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" => "today",
                "7days" => "7days",
                "30days" => "30days",
                _ => "30days"
            };
        }

        private static DateTime GetCustomerCreatedAt(Customer customer)
        {
            return customer.Account?.CreatedAt ?? DateTime.MinValue;
        }

        private static List<CustomerTrendPointDto> BuildNewCustomerTrend(
            List<Customer> customers,
            DateTime from,
            int days)
        {
            return Enumerable.Range(0, days)
                .Select(i =>
                {
                    var date = from.Date.AddDays(i);
                    return new CustomerTrendPointDto
                    {
                        Date = date,
                        Label = date.ToString("dd/MM"),
                        Count = customers.Count(c => GetCustomerCreatedAt(c).Date == date)
                    };
                })
                .ToList();
        }

        private static Dictionary<string, decimal> BuildPeriodSpendingByCustomer(
            List<Payment> periodPayments,
            List<Ticket> periodTickets,
            Dictionary<string, Ticket> ticketsById,
            Dictionary<string, MonthlyTicket> monthlyTicketsById)
        {
            var spendingByCustomer = new Dictionary<string, decimal>();
            var paidTicketIds = periodPayments
                .Where(p => !string.IsNullOrWhiteSpace(p.TicketId))
                .Select(p => p.TicketId!)
                .ToHashSet();

            foreach (var payment in periodPayments)
            {
                string? customerId = null;

                if (!string.IsNullOrWhiteSpace(payment.TicketId)
                    && ticketsById.TryGetValue(payment.TicketId, out var ticket))
                {
                    customerId = ticket.CustomerId;
                }
                else if (!string.IsNullOrWhiteSpace(payment.MonthlyTicketId)
                    && monthlyTicketsById.TryGetValue(payment.MonthlyTicketId, out var monthlyTicket))
                {
                    customerId = monthlyTicket.CustomerId;
                }

                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    spendingByCustomer[customerId] = spendingByCustomer.GetValueOrDefault(customerId) + payment.Amount;
                }
            }

            foreach (var ticket in periodTickets)
            {
                if (string.IsNullOrWhiteSpace(ticket.CustomerId) || paidTicketIds.Contains(ticket.TicketId) || ticket.Fee <= 0)
                {
                    continue;
                }

                spendingByCustomer[ticket.CustomerId] = spendingByCustomer.GetValueOrDefault(ticket.CustomerId) + ticket.Fee;
            }

            return spendingByCustomer;
        }

        private static List<CustomerBreakdownDto> BuildAreaBreakdown(
            List<Ticket> periodTickets,
            Dictionary<string, string> slotAreaById)
        {
            var total = periodTickets.Count;
            if (total == 0)
            {
                return new List<CustomerBreakdownDto>
                {
                    new() { Label = "Chưa có dữ liệu", Count = 0, Percentage = 0 }
                };
            }

            return periodTickets
                .GroupBy(t =>
                {
                    if (!string.IsNullOrWhiteSpace(t.SlotId)
                        && slotAreaById.TryGetValue(t.SlotId, out var area)
                        && !string.IsNullOrWhiteSpace(area))
                    {
                        return area;
                    }

                    return "Chưa gán khu vực";
                })
                .OrderByDescending(g => g.Count())
                .Take(6)
                .Select(g => new CustomerBreakdownDto
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Percentage = Percentage(g.Count(), total)
                })
                .ToList();
        }

        private static string ResolveAreaLabel(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return "Chưa gán khu vực";
            }

            var parts = location.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length > 0 ? parts[0] : location.Trim();
        }

        private static List<CustomerReturnBucketDto> BuildReturnBuckets(List<int> ticketCounts)
        {
            var total = ticketCounts.Count;
            var buckets = new List<(string Label, int Count)>
            {
                ("1 - 5 lượt", ticketCounts.Count(x => x >= 1 && x <= 5)),
                ("6 - 10 lượt", ticketCounts.Count(x => x >= 6 && x <= 10)),
                ("11 - 20 lượt", ticketCounts.Count(x => x >= 11 && x <= 20)),
                ("Trên 20 lượt", ticketCounts.Count(x => x > 20))
            };

            return buckets
                .Select(x => new CustomerReturnBucketDto
                {
                    Label = x.Label,
                    Count = x.Count,
                    Percentage = Percentage(x.Count, total)
                })
                .ToList();
        }

        private static bool IsActiveMonthlyTicket(MonthlyTicket monthlyTicket, DateTime today)
        {
            return monthlyTicket.EndDate.Date >= today
                && !IsExpiredMonthlyTicketStatus(monthlyTicket.Status)
                && !IsCanceledMonthlyTicketStatus(monthlyTicket.Status);
        }

        private static bool IsExpiredMonthlyTicket(MonthlyTicket monthlyTicket, DateTime today)
        {
            return monthlyTicket.EndDate.Date < today || IsExpiredMonthlyTicketStatus(monthlyTicket.Status);
        }

        private static bool IsExpiredMonthlyTicketStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            var normalized = status.Trim().ToLowerInvariant();
            return normalized.Contains("hết")
                || normalized.Contains("het")
                || normalized.Contains("expired");
        }

        private static bool IsCanceledMonthlyTicketStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            var normalized = status.Trim().ToLowerInvariant();
            return normalized.Contains("hủy")
                || normalized.Contains("huỷ")
                || normalized.Contains("huy")
                || normalized.Contains("cancel");
        }

        private static decimal CalculateChangePercentage(int current, int previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }

            return Math.Round((current - previous) * 100m / previous, 1);
        }

        private static decimal Percentage(int count, int total)
        {
            return total <= 0 ? 0 : Math.Round(count * 100m / total, 1);
        }

        private static bool IsSuccessfulPaymentStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            return status.Contains("Thành công", StringComparison.OrdinalIgnoreCase)
                || status.Contains("Thanh cong", StringComparison.OrdinalIgnoreCase)
                || status.Contains("Hoàn tất", StringComparison.OrdinalIgnoreCase)
                || status.Contains("Hoan tat", StringComparison.OrdinalIgnoreCase)
                || status.Contains("success", StringComparison.OrdinalIgnoreCase)
                || status.Contains("completed", StringComparison.OrdinalIgnoreCase);
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
