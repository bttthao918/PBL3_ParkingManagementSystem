using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class ParkingSlotManagementModel : ParkingManagement.FE.Pages.Shared.ParkingSlotManagement.ParkingSlotManagementModel
    {
    }
}

