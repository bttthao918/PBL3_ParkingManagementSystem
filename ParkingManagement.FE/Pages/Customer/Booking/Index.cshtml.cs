using Microsoft.AspNetCore.Authorization;
using global::ParkingManagement.FE.Models.ViewModels.Customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Helpers;

namespace ParkingManagement.FE.Pages.Customer.Booking
{

    [Authorize(Roles = "Customer")]
    public class IndexModel : PageModel
    {
        public List<BookingViewModel> Bookings { get; set; } = new();

        public void OnGet()
        {
            Bookings = CustomerBookingFakeData.Bookings
                .OrderByDescending(x => x.StartTime)
                .ToList();
        }

        public IActionResult OnPostCancel(int id)
        {
            var booking = CustomerBookingFakeData.Bookings.FirstOrDefault(x => x.Id == id);

            if (booking != null)
            {
                booking.Status = "Đã hủy";
            }

            return RedirectToPage();
        }
    }
}

