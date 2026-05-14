using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer.Booking
{
    [Authorize(Roles = "Customer")]
    public class CancelModel : PageModel
    {
        private readonly IReservationService _reservationService;

        public CancelModel(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        public ReservationDetailDto? Reservation { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        
        [BindProperty]
        public string? ReservationId { get; set; }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            ViewData["Title"] = "Hủy đặt chỗ";
            ViewData["Role"] = "Khách hàng";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Khách hàng";

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            ReservationId = id;
            Reservation = await _reservationService.GetByIdAsync(id);
            if (Reservation == null)
            {
                ErrorMessage = "Không tìm thấy đặt chỗ.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["Title"] = "Hủy đặt chỗ";
            ViewData["Role"] = "Khách hàng";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Khách hàng";

            if (string.IsNullOrEmpty(ReservationId))
            {
                return RedirectToPage("./Index");
            }

            var result = await _reservationService.CancelAsync(ReservationId);
            if (result?.Success == true)
            {
                TempData["SuccessMessage"] = "Đã hủy đặt chỗ thành công.";
                return RedirectToPage("./Index");
            }
            else
            {
                ErrorMessage = result?.Message ?? "Không thể hủy đặt chỗ.";
                Reservation = await _reservationService.GetByIdAsync(ReservationId);
                return Page();
            }
        }
    }
}

