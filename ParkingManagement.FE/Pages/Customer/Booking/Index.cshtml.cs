using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer.Booking
{
    [Authorize(Roles = "Customer")]
    public class IndexModel : PageModel
    {
        private readonly IReservationService _reservationService;

        public IndexModel(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        public List<ReservationDetailDto> Reservations { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Danh sách đặt chỗ";
            ViewData["Role"] = "Khách hàng";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Khách hàng";

            var result = await _reservationService.GetAllAsync(PageNumber, 10);
            if (result != null)
            {
                Reservations = result.Items.OrderByDescending(x => x.CreatedAt).ToList();
                TotalItems = result.TotalItems;
                TotalPages = result.TotalPages;
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(string reservationId)
        {
            var result = await _reservationService.CancelAsync(reservationId);
            if (result?.Success == true)
            {
                TempData["SuccessMessage"] = "Đã hủy đặt chỗ thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = result?.Message ?? "Không thể hủy đặt chỗ.";
            }

            return RedirectToPage();
        }
    }
}

