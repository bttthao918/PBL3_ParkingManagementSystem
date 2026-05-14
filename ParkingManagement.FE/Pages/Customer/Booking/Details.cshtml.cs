using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer.Booking
{
    [Authorize(Roles = "Customer")]
    public class DetailsModel : PageModel
    {
        private readonly IReservationService _reservationService;

        public DetailsModel(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        public ReservationDetailDto? Reservation { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            ViewData["Title"] = "Chi tiết đặt chỗ";
            ViewData["Role"] = "Khách hàng";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Khách hàng";

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            Reservation = await _reservationService.GetByIdAsync(id);
            if (Reservation == null)
            {
                ErrorMessage = "Không tìm thấy đặt chỗ.";
            }

            return Page();
        }
    }
}

