using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class RevenueStatisticsModel : PageModel, IRevenueStatisticsViewModel
    {
        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();

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
    }
}

