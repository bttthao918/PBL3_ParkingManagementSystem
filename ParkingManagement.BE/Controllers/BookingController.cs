using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IParkingSlotService _slotService;

        public BookingController(IReservationService reservationService, IParkingSlotService slotService)
        {
            _reservationService = reservationService;
            _slotService = slotService;
        }

        // GET: /Booking
        // Tương đương Index cũ của FE — hiển thị form đặt chỗ
        [HttpGet]
        public async Task<IActionResult> Index(string? slotId)
        {
            // Lấy danh sách slot trống để hiển thị (thay thế ParkingSpots cũ)
            var allSlots = await _slotService.GetAllAsync();
            var availableSlots = allSlots.Where(s => s.Status == "Available" || s.Status == "Trống" || s.Status == "Empty").ToList();
            ViewBag.AvailableSlots = availableSlots;

            if (!string.IsNullOrEmpty(slotId))
            {
                ViewBag.SelectedSlotId = slotId;
            }

            // Lấy customerId từ session (sau khi đăng nhập)
            var customerId = HttpContext.Session.GetString("CustomerId");
            ViewBag.CustomerId = customerId;

            return View();
        }

        // POST: /Booking/Create
        // Tương đương Create cũ — tạo đặt chỗ mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string vehiclePlate,
            string vehicleType,
            DateTime expectedTime,
            string? preferredSlotId)
        {
            // Lấy customerId từ session
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để đặt chỗ!";
                return RedirectToAction("Login", "Auth");
            }

            var dto = new CreateReservationDto
            {
                CustomerId = customerId,
                VehiclePlate = vehiclePlate,
                VehicleType = vehicleType,
                ExpectedTime = expectedTime,
                PreferredSlotId = preferredSlotId
            };

            var result = await _reservationService.CreateAsync(dto);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = $"Đặt chỗ thành công! Mã đặt chỗ: {result.Data!.ReservationId}";
            return RedirectToAction(nameof(Confirmation), new { id = result.Data.ReservationId });
        }

        // GET: /Booking/Confirmation/{id}
        [HttpGet]
        public async Task<IActionResult> Confirmation(string id)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
                return RedirectToAction("Login", "Auth");

            // Lấy danh sách của customer rồi tìm đúng reservation
            var list = await _reservationService.GetByCustomerIdAsync(customerId);
            var reservation = list.FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            return View(reservation); // View nhận ReservationDto
        }

        // GET: /Booking/MyBookings
        // Tương đương MyBookings cũ
        [HttpGet]
        public async Task<IActionResult> MyBookings(string? status, int page = 1)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
                return RedirectToAction("Login", "Auth");

            var filter = new FilterReservationDto
            {
                Status = status,
                PageNumber = page,
                PageSize = 10
            };

            var result = await _reservationService.GetByCustomerIdPaginatedAsync(customerId, filter);
            return View(result); // View nhận ListReservationDto
        }

        // POST: /Booking/Cancel/{id}
        [HttpPost]
        public async Task<IActionResult> Cancel(string id)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
                return RedirectToAction("Login", "Auth");

            var result = await _reservationService.CancelReservationAsync(customerId, id);

            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = "Hủy đặt chỗ thành công!";

            return RedirectToAction(nameof(MyBookings));
        }
    }
}
