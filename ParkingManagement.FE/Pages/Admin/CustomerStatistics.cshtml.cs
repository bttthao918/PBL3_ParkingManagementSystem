using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.ViewModels;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class CustomerStatisticsModel : PageModel
    {
        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();

        public void OnGet()
        {
            Header = new StatisticsHeaderViewModel
            {
                Title = "Thống kê khách hàng",
                Description = "Báo cáo tăng trưởng và cơ cấu khách hàng",
                DateRangeText = "01/05/2026 - 10/05/2026"
            };

            Kpis = new()
            {
                new() { Title = "Tổng khách hàng", Value = "2,450", ChangeText = "↑ 12.5% so với kỳ trước", Icon = "fa-solid fa-users", ColorClass = "blue" },
                new() { Title = "Khách hàng mới", Value = "320", ChangeText = "↑ 18.6% so với kỳ trước", Icon = "fa-solid fa-user-plus", ColorClass = "green" },
                new() { Title = "Khách hàng thân thiết", Value = "680", ChangeText = "↑ 8.7% so với kỳ trước", Icon = "fa-solid fa-user-check", ColorClass = "purple" },
                new() { Title = "Tỷ lệ khách quay lại", Value = "72.4%", ChangeText = "↑ 5.3% so với kỳ trước", Icon = "fa-solid fa-star", ColorClass = "orange" },
                new() { Title = "Lượt gửi xe TB/khách", Value = "8.6 lượt", ChangeText = "↑ 6.4% so với kỳ trước", Icon = "fa-solid fa-clock", ColorClass = "cyan" }
            };

            Table = new StatisticsTableViewModel
            {
                Headers = new() { "Ngày", "Khách hàng mới", "Khách thân thiết", "Khách VIP", "Tỷ lệ quay lại", "Tổng lượt gửi xe" },
                Rows = new()
                {
                    new() { "10/05/2026", "52", "14", "6", "73.6%", "486" },
                    new() { "09/05/2026", "38", "12", "5", "71.2%", "452" },
                    new() { "08/05/2026", "45", "16", "7", "74.8%", "512" }
                }
            };
        }
    }
}

