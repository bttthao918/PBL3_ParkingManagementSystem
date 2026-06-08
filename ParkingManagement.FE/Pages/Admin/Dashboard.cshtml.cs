using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IReportService _reportService;
        private readonly ITicketService _ticketService;
        private readonly IEmployeeService _employeeService;

        public DashboardModel(
            IReportService reportService,
            ITicketService ticketService,
            IEmployeeService employeeService)
        {
            _reportService = reportService;
            _ticketService = ticketService;
            _employeeService = employeeService;
        }

        public DashboardSummaryDto Summary { get; set; } = new();
        public List<ManagerEmployeeListDto> Employees { get; set; } = new();
        public List<EmployeeTicketListDto> RecentTickets { get; set; } = new();
        public RevenueReportDto? RevenueReport { get; set; }
        public CustomerReportDto? CustomerReport { get; set; }
        public bool IsSummaryView { get; set; }
        public StatisticsHeaderViewModel Header { get; set; } = new();
        public string SummaryChartConfigJson { get; set; } = "{}";

        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "30days";

        [BindProperty(SupportsGet = true)]
        public string? Month { get; set; }

        public async Task OnGetAsync()
        {
            IsSummaryView = Request.Query["view"].ToString().Equals("summary", StringComparison.OrdinalIgnoreCase);
            Period = IsSummaryView ? NormalizePeriod(Period) : "7days";
            DateTime monthFrom = default;
            DateTime monthTo = default;
            var hasSelectedMonth = IsSummaryView && TryGetMonthRange(Month, out monthFrom, out monthTo);

            ViewData["Title"] = IsSummaryView ? "Báo cáo tổng hợp" : "Tổng quan";
            ViewData["Role"] = "Admin / Manager";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Manager";

            var dashboardTask = _reportService.GetManagerDashboardAsync();
            var ticketsTask = _ticketService.SearchTicketsAsync(new EmployeeTicketSearchDto { PageNumber = 1, PageSize = 5 });
            var employeesTask = _employeeService.GetEmployeesAsync(new ManagerEmployeeFilterDto { PageNumber = 1, PageSize = 5 });
            var revenueFilter = new RevenueReportFilterDto { Period = IsSummaryView ? Period : "7days" };
            DateTime? customerFromDate = null;
            DateTime? customerToDate = null;

            if (hasSelectedMonth)
            {
                Period = "month";
                revenueFilter.Period = Period;
                revenueFilter.FromDate = monthFrom;
                revenueFilter.ToDate = monthTo;
                customerFromDate = monthFrom;
                customerToDate = monthTo;
            }

            Task<RevenueReportDto?> revenueTask = _reportService.GetManagerRevenueReportAsync(revenueFilter);
            Task<CustomerReportDto?> customerTask = IsSummaryView
                ? _reportService.GetManagerCustomerReportAsync(Period, customerFromDate, customerToDate)
                : Task.FromResult<CustomerReportDto?>(null);

            await Task.WhenAll(dashboardTask, ticketsTask, employeesTask, revenueTask, customerTask);

            Summary = await dashboardTask ?? new DashboardSummaryDto();
            RevenueReport = await revenueTask;
            CustomerReport = await customerTask;
            SummaryChartConfigJson = BuildSummaryChartConfig(CustomerReport ?? new CustomerReportDto());
            Period = IsSummaryView && hasSelectedMonth ? "month" : NormalizePeriod(RevenueReport?.Period ?? Period);

            if (IsSummaryView)
            {
                BuildSummaryHeader(hasSelectedMonth);
            }

            var tickets = await ticketsTask;
            RecentTickets = tickets?.Items
                .OrderByDescending(x => x.CheckInTime)
                .Take(5)
                .ToList() ?? new List<EmployeeTicketListDto>();

            var employees = await employeesTask;
            Employees = employees?.Items.Take(5).ToList() ?? new List<ManagerEmployeeListDto>();
        }

        public string GetSlotEmptyCount()
        {
            return Math.Max(0, Summary.TotalSlots - Summary.OccupiedSlots).ToString("N0");
        }

        private void BuildSummaryHeader(bool hasSelectedMonth)
        {
            var fallbackMonth = RevenueReport?.From == default
                ? DateTime.Today
                : RevenueReport?.From ?? DateTime.Today;

            Header = new StatisticsHeaderViewModel
            {
                Title = "Báo cáo tổng hợp",
                Description = "Tổng hợp doanh thu, giao dịch, khách hàng và tình trạng chỗ đỗ theo kỳ báo cáo.",
                DateRangeText = GetSummaryDateRangeText(),
                ActivePeriod = Period,
                ShowMonthPicker = true,
                SelectedMonth = ResolveSelectedMonth(hasSelectedMonth ? Month : null, fallbackMonth),
                RouteValues = new Dictionary<string, string>
                {
                    ["view"] = "summary"
                }
            };
        }

        public int GetTotalReportTransactions()
        {
            return (RevenueReport?.TotalTickets ?? Summary.ThisMonthTickets)
                + (RevenueReport?.TotalMonthlyTickets ?? 0);
        }

        public decimal GetTotalReportRevenue()
        {
            return RevenueReport?.TotalRevenue > 0
                ? RevenueReport.TotalRevenue
                : Summary.ThisMonthRevenue;
        }

        public decimal GetSingleTicketRevenue()
        {
            return RevenueReport?.RevenueFromSingleTickets ?? 0;
        }

        public decimal GetMonthlyTicketRevenue()
        {
            return RevenueReport?.RevenueFromMonthlyTickets ?? 0;
        }

        public string GetSummaryDateRangeText()
        {
            if (RevenueReport != null && RevenueReport.From != default && RevenueReport.To != default)
            {
                if (Period == "today")
                {
                    return RevenueReport.From.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                return $"{RevenueReport.From:dd/MM/yyyy} - {RevenueReport.To:dd/MM/yyyy}";
            }

            return DateTime.Today.ToString("MM/yyyy", CultureInfo.InvariantCulture);
        }

        public bool HasRevenueTrendData()
        {
            return RevenueReport?.DailyBreakdown.Any() == true;
        }

        public string GetRevenueTrendRangeText()
        {
            var points = GetRevenueTrendPoints();
            if (points.Count == 0)
            {
                return "7 ngày gần đây";
            }

            return $"{points.First().Date:dd/MM} - {points.Last().Date:dd/MM/yyyy}";
        }

        public string GetRevenueTrendTotalText()
        {
            var total = RevenueReport?.DailyBreakdown.Sum(x => x.Revenue) ?? 0;
            return $"{total:N0} đ";
        }

        public List<DailyRevenueDto> GetRevenueTrendDays()
        {
            var byDate = (RevenueReport?.DailyBreakdown ?? new List<DailyRevenueDto>())
                .GroupBy(item => item.Date.Date)
                .ToDictionary(group => group.Key, group => group.First());
            var end = RevenueReport?.To.Date ?? DateTime.Today;
            var start = end.AddDays(-6);

            return Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = start.AddDays(offset);
                    return byDate.TryGetValue(date, out var value)
                        ? value
                        : new DailyRevenueDto
                        {
                            Date = date,
                            Label = date.ToString("dd/MM", CultureInfo.InvariantCulture),
                            Revenue = 0,
                            TicketCount = 0
                        };
                })
                .ToList();
        }

        public string GetRevenueLinePoints()
        {
            var days = GetRevenueTrendDays();
            if (days.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(" ", GetRevenueChartCoordinates(days)
                .Select(point => $"{FormatNumber(point.X)},{FormatNumber(point.Y)}"));
        }

        public string GetRevenueAreaPoints()
        {
            var line = GetRevenueLinePoints();
            return string.IsNullOrWhiteSpace(line)
                ? string.Empty
                : $"0,170 {line} 700,170";
        }

        public string GetSlotUsageWidth()
        {
            return FormatCssPercent(Summary.SlotUtilizationRate);
        }

        public string GetRateWidth(decimal value, decimal total)
        {
            if (total <= 0)
            {
                return "0%";
            }

            return FormatCssPercent(value / total * 100);
        }

        public string GetRateText(decimal value, decimal total)
        {
            if (total <= 0)
            {
                return "0.0%";
            }

            return $"{value / total * 100:0.0}%";
        }

        public string GetBreakdownWidth(decimal percentage)
        {
            return FormatCssPercent(percentage);
        }

        public List<KeyValuePair<string, decimal>> GetDashboardPaymentMethods(Dictionary<string, decimal>? source)
        {
            if (source == null || source.Count == 0)
            {
                return new List<KeyValuePair<string, decimal>>();
            }

            var methods = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["Chuyển khoản"] = 0,
                ["Tiền mặt"] = 0
            };

            foreach (var item in source)
            {
                methods[NormalizeDashboardPaymentMethod(item.Key)] += item.Value;
            }

            return methods
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ToList();
        }

        public List<CustomerBreakdownDto> GetDashboardCustomerGroups(IEnumerable<CustomerBreakdownDto>? source)
        {
            var items = source?.ToList() ?? new List<CustomerBreakdownDto>();
            var preferredOrder = new[] { "Khách vãng lai", "Thành viên", "Bạc", "Vàng", "Kim Cương" };

            return preferredOrder
                .Select(label => items.FirstOrDefault(item => item.Label.Equals(label, StringComparison.OrdinalIgnoreCase))
                    ?? new CustomerBreakdownDto { Label = label, Count = 0, Percentage = 0 })
                .Where(item => item.Count > 0)
                .ToList();
        }

        public string GetColorClass(int index)
        {
            var colors = new[] { "blue", "green", "orange", "purple", "cyan" };
            return colors[index % colors.Length];
        }

        public string GetSlotPressureClass()
        {
            return Summary.SlotUtilizationRate >= 90 ? "danger"
                : Summary.SlotUtilizationRate >= 70 ? "warning"
                : "good";
        }

        public string GetSlotPressureLabel()
        {
            return Summary.SlotUtilizationRate >= 90 ? "Bãi gần đầy"
                : Summary.SlotUtilizationRate >= 70 ? "Lưu lượng cao"
                : "Ổn định";
        }

        public decimal GetEmployeeCoverageRate()
        {
            return Summary.TotalActiveEmployees <= 0
                ? 0
                : (decimal)Summary.EmployeesOnline / Summary.TotalActiveEmployees * 100;
        }

        public string GetEmployeeCoverageClass()
        {
            var rate = GetEmployeeCoverageRate();
            return rate >= 50 ? "good"
                : rate >= 20 ? "warning"
                : "danger";
        }

        public string GetEmployeeCoverageLabel()
        {
            var rate = GetEmployeeCoverageRate();
            return rate >= 50 ? "Đủ người trực"
                : rate >= 20 ? "Cần theo dõi"
                : "Thiếu nhân sự trực";
        }

        public string GetRevenuePulseClass()
        {
            var monthlyAverage = Summary.ThisMonthRevenue / Math.Max(1, DateTime.Today.Day);
            return Summary.TodayRevenue >= monthlyAverage * 1.1m ? "good"
                : Summary.TodayRevenue < monthlyAverage * 0.7m ? "warning"
                : "neutral";
        }

        public string GetRevenuePulseLabel()
        {
            var monthlyAverage = Summary.ThisMonthRevenue / Math.Max(1, DateTime.Today.Day);
            return Summary.TodayRevenue >= monthlyAverage * 1.1m ? "Vượt nhịp trung bình"
                : Summary.TodayRevenue < monthlyAverage * 0.7m ? "Dưới nhịp trung bình"
                : "Theo nhịp trung bình";
        }

        public string GetTicketStatusClass(string status)
        {
            return status.Contains("Đang", StringComparison.OrdinalIgnoreCase)
                ? "status-warning"
                : "status-success";
        }

        private static string FormatCssPercent(decimal value)
        {
            var clamped = Math.Min(100, Math.Max(0, value));
            return $"{clamped.ToString("0.##", CultureInfo.InvariantCulture)}%";
        }

        private static string NormalizePeriod(string? period)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" or "day" => "today",
                "7days" or "week" => "7days",
                "month" => "month",
                "30days" => "30days",
                _ => "30days"
            };
        }

        private static string NormalizeDashboardPaymentMethod(string? method)
        {
            var value = method?.Trim() ?? string.Empty;

            return value.Contains("tiền mặt", StringComparison.OrdinalIgnoreCase)
                || value.Contains("tien mat", StringComparison.OrdinalIgnoreCase)
                || value.Contains("cash", StringComparison.OrdinalIgnoreCase)
                    ? "Tiền mặt"
                    : "Chuyển khoản";
        }

        private static string BuildSummaryChartConfig(CustomerReportDto data)
        {
            var preferredOrder = new[] { "Khách vãng lai", "Thành viên", "Bạc", "Vàng", "Kim Cương" };
            var groups = preferredOrder
                .Select(label => data.GroupBreakdown.FirstOrDefault(item => item.Label.Equals(label, StringComparison.OrdinalIgnoreCase))
                    ?? new CustomerBreakdownDto { Label = label, Count = 0, Percentage = 0 })
                .Where(item => item.Count > 0)
                .ToList();

            var config = new
            {
                type = "customer",
                donut = new
                {
                    labels = groups.Select(x => x.Label).ToList(),
                    data = groups.Select(x => x.Count).ToList()
                }
            };

            return JsonSerializer.Serialize(config);
        }

        private static bool TryGetMonthRange(string? month, out DateTime from, out DateTime to)
        {
            if (DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                from = new DateTime(parsed.Year, parsed.Month, 1);
                to = from.AddMonths(1).AddDays(-1);
                return true;
            }

            from = default;
            to = default;
            return false;
        }

        private static string ResolveSelectedMonth(string? month, DateTime fallback)
        {
            return DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed.ToString("yyyy-MM", CultureInfo.InvariantCulture)
                : fallback.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        private static string FormatNumber(decimal value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static List<(decimal X, decimal Y)> GetRevenueChartCoordinates(IReadOnlyList<DailyRevenueDto> days)
        {
            const decimal chartWidth = 700m;
            const decimal chartTop = 18m;
            const decimal chartHeight = 132m;
            var maxRevenue = Math.Max(1, days.Max(day => day.Revenue));
            var step = days.Count <= 1 ? 0 : chartWidth / (days.Count - 1);

            return days
                .Select((day, index) =>
                {
                    var x = step * index;
                    var y = chartTop + chartHeight - (day.Revenue / maxRevenue * chartHeight);
                    return (x, y);
                })
                .ToList();
        }

        private List<DailyRevenueDto> GetRevenueTrendPoints()
        {
            return RevenueReport?.DailyBreakdown
                .OrderBy(x => x.Date)
                .ToList() ?? new List<DailyRevenueDto>();
        }
    }
}
