using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Customer.Booking
{
    [Authorize(Roles = "Customer")]
    public class CancelModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

