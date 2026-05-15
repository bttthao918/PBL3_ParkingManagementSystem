using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;

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

        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();

        public async Task OnGetAsync()
        {
            var data = await _reportService.GetManagerCustomerReportAsync();

            if (data != null)
            {
                Header = new StatisticsHeaderViewModel
                {
                    Title = "Thống kê khách hàng",
                    Description = "Báo cáo số lượng và cơ cấu khách hàng",
                    DateRangeText = DateTime.Now.ToString("MM/yyyy")
                };

                Kpis = new()
                {
                    new() { Title = "Tổng khách hàng", Value = $"{data.TotalCustomers:N0}", ChangeText = "", Icon = "fa-solid fa-users", ColorClass = "blue" },
                    new() { Title = "Khách hàng mới (tháng)", Value = $"{data.NewCustomersThisMonth:N0}", ChangeText = "", Icon = "fa-solid fa-user-plus", ColorClass = "green" },
                    new() { Title = "Vé tháng đang active", Value = $"{data.ActiveMonthlyTickets:N0}", ChangeText = "", Icon = "fa-solid fa-id-card", ColorClass = "purple" },
                    new() { Title = "Vé tháng hết hạn", Value = $"{data.ExpiredMonthlyTickets:N0}", ChangeText = "", Icon = "fa-solid fa-user-minus", ColorClass = "red" },
                    new() { Title = "Khách hàng vãng lai", Value = $"{data.OneTimeCustomers:N0}", ChangeText = "", Icon = "fa-solid fa-car", ColorClass = "cyan" }
                };

                Table = new StatisticsTableViewModel
                {
                    Headers = new() { "Khách hàng", "Số điện thoại", "Số lượng vé", "Tổng chi tiêu", "Lần ghé gần nhất" },
                    Rows = data.TopCustomers.Select(x => new List<string>
                    {
                        x.FullName,
                        x.PhoneNumber,
                        $"{x.TicketCount:N0}",
                        $"{x.TotalSpent:N0} đ",
                        x.LastVisit.HasValue ? x.LastVisit.Value.ToString("dd/MM/yyyy HH:mm") : "-"
                    }).ToList()
                };
            }
            else
            {
                Header = new StatisticsHeaderViewModel
                {
                    Title = "Thống kê khách hàng (Dữ liệu lỗi)",
                    Description = "Không thể tải dữ liệu",
                    DateRangeText = ""
                };
            }
        }
    }
}

