using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;
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

        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();
        public CustomerReportDto Report { get; set; } = new();
        public string ChartConfigJson { get; set; } = "{}";

        public async Task OnGetAsync()
        {
            Period = NormalizePeriod(Period);
            var data = await _reportService.GetManagerCustomerReportAsync(Period);

            if (data == null)
            {
                ChartConfigJson = BuildChartConfig(Report);
                Header = new StatisticsHeaderViewModel
                {
                    Title = "Thống kê khách hàng",
                    Description = "Không thể tải dữ liệu khách hàng từ backend.",
                    DateRangeText = "",
                    ActivePeriod = Period
                };

                return;
            }

            Report = data;
            Period = NormalizePeriod(data.Period);

            Header = new StatisticsHeaderViewModel
            {
                Title = "Thống kê khách hàng",
                Description = "Báo cáo số lượng và cơ cấu khách hàng theo dữ liệu thực tế",
                DateRangeText = BuildDateRangeText(data),
                ActivePeriod = Period
            };

            Kpis = new()
            {
                new() { Title = "Tổng khách hàng", Value = $"{data.TotalCustomers:N0}", ChangeText = "Tính đến hiện tại", Icon = "fa-solid fa-users", ColorClass = "blue" },
                new() { Title = $"Khách hàng mới ({BuildPeriodLabel(Period)})", Value = $"{data.NewCustomersInPeriod:N0}", ChangeText = $"{data.NewCustomersThisMonth:N0} trong tháng này", Icon = "fa-solid fa-user-plus", ColorClass = "green" },
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
                _ => "30 ngày"
            };
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
