using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;
using System.Globalization;
using System.Text.Json;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class CustomerStatisticsModel : PageModel
    {
        private readonly IReportService _reportService;

        public CustomerStatisticsModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "30days";

        [BindProperty(SupportsGet = true)]
        public string? Month { get; set; }

        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();
        public CustomerReportDto Report { get; set; } = new();
        public string ChartConfigJson { get; set; } = "{}";

        public async Task OnGetAsync()
        {
            Period = NormalizePeriod(Period);
            DateTime? fromDate = null;
            DateTime? toDate = null;
            var hasSelectedMonth = TryGetMonthRange(Month, out var monthFrom, out var monthTo);
            if (hasSelectedMonth)
            {
                Period = "month";
                fromDate = monthFrom;
                toDate = monthTo;
            }

            var data = await _reportService.GetManagerCustomerReportAsync(Period, fromDate, toDate);

            if (data == null)
            {
                ChartConfigJson = BuildChartConfig(Report);
                Header = new StatisticsHeaderViewModel
                {
                    Title = "Thống kê khách hàng",
                    Description = "Không thể tải dữ liệu khách hàng từ backend.",
                    DateRangeText = "",
                    ActivePeriod = Period,
                    ShowMonthPicker = true,
                    SelectedMonth = ResolveSelectedMonth(Month, DateTime.Today)
                };

                return;
            }

            Report = data;
            Period = hasSelectedMonth ? "month" : NormalizePeriod(data.Period);

            Header = new StatisticsHeaderViewModel
            {
                Title = "Thống kê khách hàng",
                Description = "Báo cáo số lượng và cơ cấu khách hàng theo dữ liệu thực tế",
                DateRangeText = BuildDateRangeText(data),
                ActivePeriod = Period,
                ShowMonthPicker = true,
                SelectedMonth = ResolveSelectedMonth(hasSelectedMonth ? Month : null, data.From == default ? DateTime.Today : data.From)
            };

            Kpis = new()
            {
                new() { Title = "Tổng khách hàng", Value = $"{data.TotalCustomers:N0}", ChangeText = "Tính đến hiện tại", Icon = "fa-solid fa-users", ColorClass = "blue" },
                new() { Title = $"Khách hàng mới ({BuildPeriodLabel(Period)})", Value = $"{data.NewCustomersInPeriod:N0}", ChangeText = BuildNewCustomerChangeText(Period, data.NewCustomersThisMonth), Icon = "fa-solid fa-user-plus", ColorClass = "green" },
                new() { Title = "Vé tháng đang active", Value = $"{data.ActiveMonthlyTickets:N0}", ChangeText = "Theo trạng thái vé hiện tại", Icon = "fa-solid fa-id-card", ColorClass = "purple" },
                new() { Title = "Vé tháng hết hạn", Value = $"{data.ExpiredMonthlyTickets:N0}", ChangeText = "Đã quá hạn hoặc hết hạn", Icon = "fa-solid fa-user-minus", ColorClass = "red" },
                new() { Title = "Khách vãng lai", Value = $"{data.OneTimeCustomers:N0}", ChangeText = $"{data.WalkInTickets:N0} vé không gắn tài khoản", Icon = "fa-solid fa-car", ColorClass = "cyan" }
            };

            Table = new StatisticsTableViewModel
            {
                Headers = new() { "Khách hàng", "Số điện thoại", "Ngày đăng ký", "Số lượng vé", "Tổng chi tiêu", "Lần ghé gần nhất" },
                Rows = data.NewCustomers.Select(x => new List<string>
                {
                    x.FullName,
                    x.PhoneNumber,
                    x.RegisteredAt.HasValue ? x.RegisteredAt.Value.ToString("dd/MM/yyyy") : "-",
                    $"{x.TicketCount:N0}",
                    $"{x.TotalSpent:N0} đ",
                    x.LastVisit.HasValue ? x.LastVisit.Value.ToString("dd/MM/yyyy HH:mm") : "-"
                }).ToList()
            };

            ChartConfigJson = BuildChartConfig(data);
        }

        public string GetBreakdownColor(int index)
        {
            var colors = new[] { "blue", "green", "orange", "purple" };
            return colors[index % colors.Length];
        }

        public string GetChangeText(decimal percentage)
        {
            return percentage > 0 ? $"+{percentage:N1}%" : $"{percentage:N1}%";
        }

        public string GetChangeClass(decimal percentage)
        {
            return percentage < 0 ? "down" : "";
        }

        public string GetProgressColor(int index)
        {
            var colors = new[] { "blue", "green", "purple", "orange" };
            return colors[index % colors.Length];
        }

        public string GetProgressWidth(decimal percentage)
        {
            return $"{Math.Clamp(percentage, 0, 100):0.#}%";
        }

        private static string NormalizePeriod(string? period)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" => "today",
                "7days" => "7days",
                "month" => "month",
                "30days" => "30days",
                _ => "30days"
            };
        }

        private static string BuildPeriodLabel(string period)
        {
            return period switch
            {
                "today" => "hôm nay",
                "7days" => "7 ngày",
                "month" => "tháng đang chọn",
                _ => "30 ngày"
            };
        }

        private static string BuildNewCustomerChangeText(string period, int newCustomersThisMonth)
        {
            return period == "month"
                ? "Theo tháng đang chọn"
                : $"{newCustomersThisMonth:N0} trong tháng này";
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

        private static string BuildDateRangeText(CustomerReportDto data)
        {
            if (data.From == default || data.To == default)
            {
                return DateTime.Now.ToString("MM/yyyy");
            }

            return data.Period == "today"
                ? data.From.ToString("dd/MM/yyyy")
                : $"{data.From:dd/MM/yyyy} - {data.To:dd/MM/yyyy}";
        }

        private static string BuildChartConfig(CustomerReportDto data)
        {
            var config = new
            {
                type = "customer",
                line = new
                {
                    labels = data.NewCustomerTrend.Select(x => x.Label).ToList(),
                    current = data.NewCustomerTrend.Select(x => x.Count).ToList(),
                    previous = data.PreviousNewCustomerTrend.Select(x => x.Count).ToList()
                },
                donut = new
                {
                    labels = data.GroupBreakdown.Select(x => x.Label).ToList(),
                    data = data.GroupBreakdown.Select(x => x.Count).ToList()
                },
                bar = new
                {
                    labels = data.AreaBreakdown.Select(x => x.Label).ToList(),
                    data = data.AreaBreakdown.Select(x => x.Count).ToList()
                }
            };

            return JsonSerializer.Serialize(config);
        }
    }
}
