using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IReportService _reportService;

        public DashboardModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public DashboardSummaryDto Summary { get; set; } = new();

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Admin / Manager";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Manager";

            var data = await _reportService.GetManagerDashboardAsync();
            if (data != null)
            {
                Summary = data;
            }
            else
            {
                Summary = new DashboardSummaryDto
                {
                    TodayRevenue = 0,
                    ThisMonthRevenue = 0,
                    TodayTickets = 0,
                    ThisMonthTickets = 0,
                    TotalSlots = 120,
                    OccupiedSlots = 0,
                    SlotUtilizationRate = 0
                };
            }
        }
    }
}
