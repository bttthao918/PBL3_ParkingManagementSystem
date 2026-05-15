using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class ParkingSlotManagementModel : ParkingManagement.FE.Pages.Shared.ParkingSlotManagement.ParkingSlotManagementModel
    {
        public ParkingSlotManagementModel(IParkingSlotService parkingSlotService)
            : base(parkingSlotService)
        {
        }
    }
}

