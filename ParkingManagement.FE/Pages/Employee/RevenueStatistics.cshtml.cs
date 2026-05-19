using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class RevenueStatisticsModel : PageModel, IRevenueStatisticsViewModel
    {
        private readonly IReportService _reportService;

        public RevenueStatisticsModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "30days";

        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();
        public RevenueStatisticsChartConfig Charts { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> PaymentMethodBreakdown { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> VehicleTypeBreakdown { get; set; } = new();
        public List<StatisticsRankItemViewModel> Rankings { get; set; } = new();
        public string RankingTitle { get; set; } = "Top ngày doanh thu cao nhất";
        public string LineChartTitle { get; set; } = "Doanh thu cá nhân theo ngày";
        public string DonutTitle { get; set; } = "Cơ cấu doanh thu theo phương thức thanh toán";
        public string BarTitle { get; set; } = "Doanh thu theo loại xe";
        public string ProgressTitle { get; set; } = "Doanh thu theo loại xe";

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Báo cáo doanh thu";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            Period = NormalizePeriod(Period);
            var employeeId = User.FindFirst("related_id")?.Value;
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                SetEmptyData();
                return;
            }

            var report = await _reportService.GetEmployeeRevenueReportAsync(employeeId, Period);
            if (report == null)
            {
                SetEmptyData();
                return;
            }

            Header = new StatisticsHeaderViewModel
            {
                Title = "Báo cáo doanh thu cá nhân",
                Description = "Thống kê doanh thu theo dữ liệu xử lý thực tế",
                DateRangeText = $"{report.PeriodStart:dd/MM/yyyy} - {report.PeriodEnd:dd/MM/yyyy}",
                ActivePeriod = Period
            };

            var changeText = report.RevenueChangePercentage >= 0
                ? $"+{report.RevenueChangePercentage:F1}% so với kỳ trước"
                : $"-{Math.Abs(report.RevenueChangePercentage):F1}% so với kỳ trước";

            Kpis = new()
            {
                new() { Title = "Tổng doanh thu", Value = $"{report.TotalRevenue:N0} đ", ChangeText = changeText, Icon = "fa-solid fa-sack-dollar", ColorClass = "blue" },
                new() { Title = "Tổng số vé", Value = $"{report.TotalTickets:N0} vé", ChangeText = report.Trend, Icon = "fa-solid fa-ticket", ColorClass = "orange" },
                new() { Title = "Trung bình/vé", Value = $"{report.AverageRevenuePerTicket:N0} đ", ChangeText = "Theo vé phát sinh trong kỳ", Icon = "fa-solid fa-calculator", ColorClass = "green" }
            };

            foreach (var item in report.RevenueByVehicleType)
            {
                var ticketCount = report.TicketsByVehicleType.GetValueOrDefault(item.Key, 0);
                Kpis.Add(new StatisticsKpiCardViewModel
                {
                    Title = item.Key,
                    Value = $"{item.Value:N0} đ",
                    ChangeText = $"{ticketCount:N0} vé",
                    Icon = item.Key == "Xe máy" ? "fa-solid fa-motorcycle" : "fa-solid fa-car",
                    ColorClass = item.Key == "Xe máy" ? "cyan" : "purple"
                });
            }

            Table = new StatisticsTableViewModel
            {
                Headers = new() { "Ngày", "Tổng doanh thu", "Số vé", "Trung bình/vé" },
                Rows = report.DailyBreakdown
                    .OrderByDescending(d => d.Date)
                    .Take(10)
                    .Select(day => new List<string>
                    {
                        day.Date.ToString("dd/MM/yyyy"),
                        $"{day.TotalRevenue:N0} đ",
                        $"{day.TicketCount:N0} vé",
                        $"{day.AverageRevenuePerTicket:N0} đ"
                    })
                    .ToList()
            };

            PaymentMethodBreakdown = BuildBreakdown(report.RevenueByPaymentMethod);
            VehicleTypeBreakdown = BuildBreakdown(report.RevenueByVehicleType);
            Rankings = report.TopDays.Select((day, index) => new StatisticsRankItemViewModel
            {
                Rank = (index + 1).ToString(),
                Label = day.Date.ToString("dd/MM/yyyy"),
                Value = $"{day.TotalRevenue:N0} đ",
                Note = $"{day.TicketCount:N0} vé"
            }).ToList();

            Charts = new RevenueStatisticsChartConfig
            {
                Line = new RevenueLineChartConfig
                {
                    Labels = report.DailyBreakdown.OrderBy(x => x.Date).Select(x => x.Date.ToString("dd/MM")).ToList(),
                    Current = report.DailyBreakdown.OrderBy(x => x.Date).Select(x => x.TotalRevenue).ToList()
                },
                Donut = new RevenueSeriesChartConfig
                {
                    Labels = report.RevenueByPaymentMethod.Keys.ToList(),
                    Data = report.RevenueByPaymentMethod.Values.ToList()
                },
                Bar = new RevenueSeriesChartConfig
                {
                    Labels = report.RevenueByVehicleType.Keys.ToList(),
                    Data = report.RevenueByVehicleType.Values.ToList()
                }
            };
        }

        private void SetEmptyData()
        {
            Header = new StatisticsHeaderViewModel
            {
                Title = "Báo cáo doanh thu cá nhân",
                Description = "Không thể tải dữ liệu doanh thu.",
                DateRangeText = "",
                ActivePeriod = Period
            };

            Kpis = new()
            {
                new() { Title = "Tổng doanh thu", Value = "0 đ", ChangeText = "Chưa có dữ liệu", Icon = "fa-solid fa-sack-dollar", ColorClass = "blue" },
                new() { Title = "Tổng số vé", Value = "0 vé", ChangeText = "", Icon = "fa-solid fa-ticket", ColorClass = "orange" },
                new() { Title = "Trung bình/vé", Value = "0 đ", ChangeText = "", Icon = "fa-solid fa-calculator", ColorClass = "green" }
            };

            Table = new StatisticsTableViewModel
            {
                Headers = new() { "Ngày", "Tổng doanh thu", "Số vé", "Trung bình/vé" },
                Rows = new()
            };
            PaymentMethodBreakdown = new();
            VehicleTypeBreakdown = new();
            Rankings = new();
            Charts = new();
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

        private static List<StatisticsBreakdownItemViewModel> BuildBreakdown(Dictionary<string, decimal>? values)
        {
            var total = values?.Values.Sum() ?? 0;
            var colors = new[] { "blue", "green", "purple", "orange", "cyan" };

            return (values ?? new())
                .OrderByDescending(x => x.Value)
                .Select((x, index) => new StatisticsBreakdownItemViewModel
                {
                    Label = x.Key,
                    Value = x.Value,
                    Percentage = total > 0 ? Math.Round(x.Value / total * 100, 1) : 0,
                    ColorClass = colors[index % colors.Length]
                })
                .ToList();
        }
    }
}
