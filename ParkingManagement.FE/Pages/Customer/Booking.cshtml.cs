using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class BookingModel : PageModel
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly IReservationService _reservationService;
        private readonly ILogger<BookingModel> _logger;

        public BookingModel(
            ICustomerApiService customerApiService,
            IReservationService reservationService,
            ILogger<BookingModel> logger)
        {
            _customerApiService = customerApiService;
            _reservationService = reservationService;
            _logger = logger;
        }

        public string UserName { get; set; } = "Customer";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public int TotalBooking { get; set; }
        public int ActiveBooking { get; set; }
        public int CompletedBooking { get; set; }
        public int CancelledBooking { get; set; }

        public List<BookingVm> Bookings { get; set; } = new();
        public List<ParkingSlotVm> ParkingSlots { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> SavedVehicles { get; set; } = new();

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"] as string;
            ErrorMessage = TempData["Error"] as string;

            var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
            UserName = fallbackName;

            try
            {
                // Load profile for username
                var profileTask = _customerApiService.GetProfileAsync();
                // Load reservations
                var reservationsTask = _customerApiService.GetReservationsAsync(1, 100);
                // Load available slots for the wizard
                var slotsTask = _reservationService.GetAvailableSlotsAsync();

                await Task.WhenAll(profileTask, reservationsTask, slotsTask);

                var profile = await profileTask;
                var reservations = await reservationsTask;
                var slots = await slotsTask;

                if (profile != null && !string.IsNullOrWhiteSpace(profile.FullName))
                    UserName = profile.FullName;

                // Map reservations to BookingVm
                if (reservations?.Items != null)
                {
                    var items = reservations.Items;
                    TotalBooking = reservations.TotalItems > 0 ? reservations.TotalItems : items.Count;
                    ActiveBooking = items.Count(r => IsActive(r.Status));
                    CompletedBooking = items.Count(r => IsCompleted(r.Status));
                    CancelledBooking = items.Count(r => IsCancelled(r.Status));

                    Bookings = items.Select((r, idx) => new BookingVm
                    {
                        Id = idx + 1,
                        Code = r.ReservationId,
                        ParkingName = "Bãi đỗ xe",
                        Position = r.SlotLocation ?? r.SlotId ?? "Chưa xác định",
                        VehiclePlate = r.VehiclePlate,
                        VehicleType = r.VehicleType,
                        VehicleClass = GetVehicleClass(r.VehicleType),
                        Icon = GetVehicleIcon(r.VehicleType),
                        BookingTime = r.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        TimeRange = r.ExpectedTime.ToString("dd/MM/yyyy HH:mm"),
                        TotalPrice = 0,
                        Status = r.Status,
                        StatusClass = GetStatusClass(r.Status),
                        CustomerName = r.CustomerName ?? UserName,
                        Phone = profile?.PhoneNumber ?? "",
                        CanCancel = IsActive(r.Status),
                        ReservationId = r.ReservationId
                    }).ToList();

                    var uniqueVehicles = items
                        .Where(x => !string.IsNullOrWhiteSpace(x.VehiclePlate))
                        .GroupBy(x => x.VehiclePlate)
                        .Select(g => g.First())
                        .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = $"{x.VehiclePlate}|{x.VehicleType}|{x.CustomerName ?? UserName}|{profile?.PhoneNumber ?? ""}",
                            Text = $"{x.VehiclePlate} - {x.VehicleType}"
                        }).ToList();
                    SavedVehicles = uniqueVehicles;
                }

                // Map available slots
                if (slots != null)
                {
                    ParkingSlots = slots.Select(s => new ParkingSlotVm
                    {
                        Code = s.SlotId,
                        Position = s.Location,
                        StatusName = s.Status == "Trống" ? "Trống" : s.Status,
                        StatusClass = GetSlotStatusClass(s.Status),
                        IsSelectable = s.Status == "Trống"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load booking data from BE");
                ErrorMessage = "Không thể tải dữ liệu đặt chỗ. Vui lòng kiểm tra kết nối BE.";
            }

            ViewData["UserName"] = UserName;
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string vehiclePlate,
            string vehicleType,
            string? slotId,
            string? expectedTime)
        {
            if (string.IsNullOrWhiteSpace(vehiclePlate))
            {
                TempData["Error"] = "Vui lòng nhập biển số xe.";
                return RedirectToPage();
            }

            var parsedTime = DateTime.TryParse(expectedTime, out var time) ? time : DateTime.Now.AddHours(1);

            var dto = new CreateReservationDto
            {
                VehiclePlate = vehiclePlate,
                VehicleType = string.IsNullOrWhiteSpace(vehicleType) ? "Xe máy" : vehicleType,
                PreferredSlotId = slotId,
                ExpectedTime = parsedTime
            };

            var result = await _reservationService.CreateAsync(dto);

            if (result != null)
            {
                TempData["Success"] = "Đặt chỗ thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể đặt chỗ. Vui lòng thử lại.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(string reservationId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
            {
                TempData["Error"] = "Không xác định được đơn cần hủy.";
                return RedirectToPage();
            }

            var result = await _reservationService.CancelAsync(reservationId);

            if (result?.Success == true)
            {
                TempData["Success"] = "Đã hủy đơn đặt chỗ thành công.";
            }
            else
            {
                TempData["Error"] = result?.Message ?? "Không thể hủy đơn đặt chỗ.";
            }

            return RedirectToPage();
        }

        // ── Helpers ──

        private static bool IsActive(string status)
        {
            var s = status.ToLower();
            return s.Contains("chờ") || s.Contains("xác nhận") || s.Contains("pending") || s.Contains("confirmed") || s.Contains("active");
        }

        private static bool IsCompleted(string status)
        {
            var s = status.ToLower();
            return s.Contains("hoàn thành") || s.Contains("completed") || s.Contains("done") || s.Contains("used");
        }

        private static bool IsCancelled(string status)
        {
            var s = status.ToLower();
            return s.Contains("hủy") || s.Contains("cancelled") || s.Contains("canceled");
        }

        private static string GetVehicleClass(string vehicleType) => vehicleType switch
        {
            "Xe máy" => "green",
            "Ô tô nhỏ" => "blue",
            "Ô tô lớn" => "purple",
            _ => "green"
        };

        private static string GetVehicleIcon(string vehicleType) => vehicleType switch
        {
            "Xe máy" => "fa-solid fa-motorcycle",
            "Ô tô nhỏ" => "fa-solid fa-car-side",
            "Ô tô lớn" => "fa-solid fa-van-shuttle",
            _ => "fa-solid fa-motorcycle"
        };

        private static string GetStatusClass(string status)
        {
            var s = status.ToLower();
            if (s.Contains("chờ") || s.Contains("pending")) return "pending";
            if (s.Contains("xác nhận") || s.Contains("confirmed")) return "confirmed";
            if (s.Contains("hoàn thành") || s.Contains("completed") || s.Contains("done")) return "completed";
            if (s.Contains("hủy") || s.Contains("cancel")) return "cancelled";
            return "pending";
        }

        private static string GetSlotStatusClass(string status)
        {
            var s = status.ToLower();
            if (s.Contains("trống") || s.Contains("available") || s.Contains("empty")) return "empty";
            if (s.Contains("đang") || s.Contains("occupied") || s.Contains("using")) return "using";
            if (s.Contains("đặt") || s.Contains("reserved")) return "reserved";
            if (s.Contains("bảo trì") || s.Contains("maintenance")) return "maintenance";
            return "error";
        }
    }

    public class BookingVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string ParkingName { get; set; } = "";
        public string Position { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string VehicleClass { get; set; } = "";
        public string Icon { get; set; } = "";
        public string BookingTime { get; set; } = "";
        public string TimeRange { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public bool CanCancel { get; set; }
        public string ReservationId { get; set; } = "";
    }

    public class ParkingSlotVm
    {
        public string Code { get; set; } = "";
        public string Position { get; set; } = "";
        public string StatusName { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public bool IsSelectable { get; set; }
    }
}
