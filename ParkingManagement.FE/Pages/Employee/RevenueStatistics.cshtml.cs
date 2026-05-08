using Microsoft.AspNetCore.Authorization;
namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class RevenueStatisticsModel : ParkingManagement.FE.Pages.Shared.Statistics.RevenueStatisticsModel
    {
    }
}

