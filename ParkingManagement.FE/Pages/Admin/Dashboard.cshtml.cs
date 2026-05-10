using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class DashboardModel : PageModel
    {
        public string UserName { get; set; } = string.Empty;
        public decimal TodayRevenue { get; set; } = 125430000;
        public int TotalTickets { get; set; } = 2345;
        public int ActiveVehicles { get; set; } = 855;
        public int AvailableSlots { get; set; } = 395;
        public int TotalSlots { get; set; } = 1250;

        // ✅ THÊM property này
        public double UtilizationRate => TotalSlots > 0 ? (double)(TotalSlots - AvailableSlots) / TotalSlots * 100 : 0;

        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();

        public void OnGet()
        {
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Manager";
            ViewData["Title"] = "Tổng quan Admin";
            ViewData["UserName"] = UserName;

            RevenueLabels = new List<string> { "01/05", "02/05", "03/05", "04/05", "05/05", "06/05", "07/05" };
            RevenueData = new List<decimal> { 3200000, 4100000, 3800000, 5200000, 4900000, 6100000, 5800000 };
        }
    }
}