using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class RevenueStatisticsModel : ParkingManagement.FE.Pages.Shared.Statistics.RevenueStatisticsModel
    {
    }
}

