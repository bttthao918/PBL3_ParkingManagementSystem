using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class RevenueStatisticsModel : PageModel, IRevenueStatisticsViewModel
    {
        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();
        public RevenueStatisticsChartConfig Charts { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> PaymentMethodBreakdown { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> VehicleTypeBreakdown { get; set; } = new();
        public List<StatisticsRankItemViewModel> Rankings { get; set; } = new();
        public string RankingTitle { get; set; } = "Top 5 nhân viên doanh thu cao nhất";

        private readonly IReportService _reportService;

        public RevenueStatisticsModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task OnGetAsync(string period = "month", string? fromDate = null, string? toDate = null)
        {
            var filter = new RevenueReportFilterDto
            {
                Period = period
            };
            
            if (DateTime.TryParse(fromDate, out var from)) filter.FromDate = from;
            if (DateTime.TryParse(toDate, out var to)) filter.ToDate = to;

            var data = await _reportService.GetManagerRevenueReportAsync(filter);
            
            if (data != null)
            {
                Header = new StatisticsHeaderViewModel
                {
                    Title = "Báo cáo doanh thu (Admin)",
                    Description = "Thống kê doanh thu toàn bộ hệ thống",
                    DateRangeText = $"{data.From:dd/MM/yyyy} - {data.To:dd/MM/yyyy}"
                };

                Kpis = new()
                {
                    new() { Title = "Tổng doanh thu", Value = $"{data.TotalRevenue:N0} đ", ChangeText = "", Icon = "fa-solid fa-sack-dollar", ColorClass = "blue" },
                    new() { Title = "Doanh thu vé lượt", Value = $"{data.RevenueFromSingleTickets:N0} đ", ChangeText = "", Icon = "fa-solid fa-money-bill", ColorClass = "green" },
                    new() { Title = "Doanh thu vé tháng", Value = $"{data.RevenueFromMonthlyTickets:N0} đ", ChangeText = "", Icon = "fa-solid fa-credit-card", ColorClass = "purple" },
                    new() { Title = "Tổng số vé", Value = $"{data.TotalTickets:N0} vé", ChangeText = "", Icon = "fa-solid fa-ticket", ColorClass = "orange" },
                    new() { Title = "Vé tháng", Value = $"{data.TotalMonthlyTickets:N0} vé", ChangeText = "", Icon = "fa-solid fa-clock", ColorClass = "cyan" }
                };

                Table = new StatisticsTableViewModel
                {
                    Headers = new() { "Ngày", "Tổng doanh thu", "Số lượng vé" },
                    Rows = data.DailyBreakdown.OrderByDescending(x => x.Date).Select(x => new List<string>
                    {
                        x.Date.ToString("dd/MM/yyyy"),
                        $"{x.Revenue:N0} đ",
                        $"{x.TicketCount:N0}"
                    }).ToList()
                };

                PaymentMethodBreakdown = BuildBreakdown(data.RevenueByPaymentMethod);
                VehicleTypeBreakdown = BuildBreakdown(data.RevenueByVehicleType);
                Rankings = data.TopEmployees.Select((employee, index) => new StatisticsRankItemViewModel
                {
                    Rank = (index + 1).ToString(),
                    Label = employee.EmployeeName,
                    Value = $"{employee.TotalRevenue:N0} đ",
                    Note = $"{employee.PaymentCount:N0} giao dịch"
                }).ToList();

                Charts = new RevenueStatisticsChartConfig
                {
                    Line = new RevenueLineChartConfig
                    {
                        Labels = data.DailyBreakdown.OrderBy(x => x.Date).Select(x => x.Date.ToString("dd/MM")).ToList(),
                        Current = data.DailyBreakdown.OrderBy(x => x.Date).Select(x => x.Revenue).ToList()
                    },
                    Donut = new RevenueSeriesChartConfig
                    {
                        Labels = data.RevenueByPaymentMethod.Keys.ToList(),
                        Data = data.RevenueByPaymentMethod.Values.ToList()
                    },
                    Bar = new RevenueSeriesChartConfig
                    {
                        Labels = data.RevenueByArea.Any() ? data.RevenueByArea.Keys.ToList() : data.RevenueByVehicleType.Keys.ToList(),
                        Data = data.RevenueByArea.Any() ? data.RevenueByArea.Values.ToList() : data.RevenueByVehicleType.Values.ToList()
                    }
                };
            }
            else
            {
                Header = new StatisticsHeaderViewModel
                {
                    Title = "Báo cáo doanh thu (Admin) - Dữ liệu lỗi",
                    Description = "Không thể tải dữ liệu",
                    DateRangeText = ""
                };
            }
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

