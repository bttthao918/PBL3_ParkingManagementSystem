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
        private const int CustomerSnapshotPageSize = 1000;

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
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
        public List<SavedVehicleOption> SavedVehicles { get; set; } = new();

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"] as string;
            ErrorMessage = TempData["Error"] as string;
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnGetSlotsAsync(string? vehicleType)
        {
            _logger.LogInformation("OnGetSlotsAsync called with vehicleType: {VehicleType}", vehicleType);
            try
            {
                var slots = await _customerApiService.GetAvailableSlotsAsync(vehicleType, includeUnavailable: true);
                _logger.LogInformation("OnGetSlotsAsync returned {Count} slots", slots?.Count ?? 0);
                return new JsonResult(slots ?? new List<AvailableSlotDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnGetSlotsAsync error");
                return new JsonResult(new List<AvailableSlotDto>());
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string vehiclePlate,
            string vehicleType,
            string? slotId,
            string expectedTime)
        {
            if (string.IsNullOrWhiteSpace(vehiclePlate) || string.IsNullOrWhiteSpace(vehicleType))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin xe.";
                return RedirectToPage();
            }

            if (!DateTime.TryParse(expectedTime, out var parsedTime))
            {
                parsedTime = DateTime.Now.AddMinutes(30);
            }

            var dto = new CreateReservationDto
            {
                VehiclePlate = vehiclePlate,
                VehicleType = vehicleType,
                PreferredSlotId = slotId,
                ExpectedTime = parsedTime
            };

            var result = await _reservationService.CreateAsync(dto);
            if (result.Success)
            {
                TempData["Success"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Đặt chỗ thành công!"
                    : result.Message;
            }
            else
            {
                TempData["Error"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Không thể đặt chỗ. Vui lòng thử lại."
                    : result.Message;
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
                TempData["Success"] = result.Message ?? "Đã hủy đơn đặt chỗ thành công.";
            }
            else
            {
                TempData["Error"] = result?.Message ?? "Không thể hủy đơn đặt chỗ.";
            }

            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
            UserName = fallbackName;

            try
            {
                // Load profile, reservations, and available slots in parallel
                var profileTask = _customerApiService.GetProfileAsync();
                var reservationsTask = _customerApiService.GetReservationsAsync(1, CustomerSnapshotPageSize);
                var slotsTask = _customerApiService.GetAvailableSlotsAsync();

                await Task.WhenAll(profileTask, reservationsTask, slotsTask);

                var profile = await profileTask;
                var reservations = await reservationsTask;
                var slots = await slotsTask;

                // Set user name from profile
                if (profile != null && !string.IsNullOrWhiteSpace(profile.FullName))
                {
                    UserName = profile.FullName;
                }

                // Map reservations to BookingVm
                if (reservations?.Items != null)
                {
                    Bookings = reservations.Items.Select((r, index) => MapToBookingVm(r, index + 1)).ToList();

                    TotalBooking = reservations.TotalItems > 0 ? reservations.TotalItems : reservations.Items.Count;
                    ActiveBooking = reservations.Items.Count(r => IsActive(r.Status));
                    CompletedBooking = reservations.Items.Count(r => IsCompleted(r.Status));
                    CancelledBooking = reservations.Items.Count(r => IsCancelled(r.Status));
                }

                // Available slots for booking wizard
                AvailableSlots = slots ?? new List<AvailableSlotDto>();
                _logger.LogInformation("Available slots loaded: {Count} slots", AvailableSlots.Count);

                // Build saved vehicles from recent reservations
                var vehiclePlates = reservations?.Items
                    .Select(r => new { r.VehiclePlate, r.VehicleType })
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehiclePlate))
                    .DistinctBy(x => x.VehiclePlate)
                    .ToList();

                if (vehiclePlates?.Any() == true)
                {
                    SavedVehicles = vehiclePlates.Select(v => new SavedVehicleOption
                    {
                        Value = $"{v.VehiclePlate}|{v.VehicleType}",
                        Text = $"{v.VehiclePlate} - {v.VehicleType}"
                    }).ToList();
                }

                ViewData["UserName"] = UserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load booking data from BE");
                ErrorMessage = "Không tải được dữ liệu từ hệ thống. Vui lòng kiểm tra kết nối.";
                ViewData["UserName"] = fallbackName;
            }
        }

        private BookingVm MapToBookingVm(CustomerReservationDto r, int index)
        {
            var statusClass = GetStatusClass(r.Status);
            var vehicleClass = GetVehicleClass(r.VehicleType);
            var icon = GetVehicleIcon(r.VehicleType);
            var canCancel = IsActive(r.Status);

            return new BookingVm
            {
                Id = index,
                ReservationId = r.ReservationId,
                Code = r.ReservationId.Length > 8 ? r.ReservationId[..8].ToUpper() : r.ReservationId.ToUpper(),
                ParkingName = "Bãi xe",
                Position = FormatSlotPosition(r.SlotId, r.SlotLocation),
                VehiclePlate = r.VehiclePlate,
                VehicleType = r.VehicleType,
                VehicleClass = vehicleClass,
                Icon = icon,
                BookingTime = r.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                TimeRange = r.ExpectedTime.ToString("dd/MM/yyyy HH:mm"),
                TotalPrice = 0, // BE doesn't return price for reservations
                Status = r.Status,
                StatusClass = statusClass,
                CustomerName = r.CustomerName ?? UserName,
                Phone = "",
                CanCancel = canCancel
            };
        }

        private static string GetStatusClass(string status)
        {
            var normalized = status.ToLower().Trim();
            if (normalized.Contains("chờ") || normalized.Contains("pending") || normalized.Contains("waiting"))
                return "pending";
            if (normalized.Contains("xác nhận") || normalized.Contains("confirmed") || normalized.Contains("active"))
                return "confirmed";
            if (normalized.Contains("hoàn thành") || normalized.Contains("completed") || normalized.Contains("done"))
                return "completed";
            if (normalized.Contains("hủy") || normalized.Contains("cancel"))
                return "cancelled";
            return "pending";
        }

        private static string FormatSlotPosition(string? slotId, string? slotLocation)
        {
            if (string.IsNullOrWhiteSpace(slotId))
                return "Chưa xác định";

            if (string.IsNullOrWhiteSpace(slotLocation))
                return slotId;

            return $"{slotId} - {slotLocation}";
        }

        private static string GetVehicleClass(string vehicleType)
        {
            var normalized = vehicleType.ToLower().Trim();
            if (normalized.Contains("máy") || normalized.Contains("motorcycle"))
                return "green";
            if (normalized.Contains("nhỏ") || normalized.Contains("small") || normalized.Contains("ô tô") && !normalized.Contains("lớn"))
                return "blue";
            if (normalized.Contains("lớn") || normalized.Contains("large") || normalized.Contains("van"))
                return "purple";
            return "green";
        }

        private static string GetVehicleIcon(string vehicleType)
        {
            var normalized = vehicleType.ToLower().Trim();
            if (normalized.Contains("máy") || normalized.Contains("motorcycle"))
                return "fa-solid fa-motorcycle";
            if (normalized.Contains("lớn") || normalized.Contains("large") || normalized.Contains("van"))
                return "fa-solid fa-van-shuttle";
            if (normalized.Contains("ô tô") || normalized.Contains("car"))
                return "fa-solid fa-car-side";
            return "fa-solid fa-motorcycle";
        }

        private static bool IsActive(string status)
        {
            var normalized = status.ToLower().Trim();
            return normalized.Contains("chờ") || normalized.Contains("pending")
                || normalized.Contains("xác nhận") || normalized.Contains("confirmed")
                || normalized.Contains("active") || normalized.Contains("waiting");
        }

        private static bool IsCompleted(string status)
        {
            var normalized = status.ToLower().Trim();
            return normalized.Contains("hoàn thành") || normalized.Contains("completed") || normalized.Contains("done");
        }

        private static bool IsCancelled(string status)
        {
            var normalized = status.ToLower().Trim();
            return normalized.Contains("hủy") || normalized.Contains("cancel");
        }
    }

    public class BookingVm
    {
        public int Id { get; set; }
        public string ReservationId { get; set; } = "";
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
    }

    public class ParkingSlotVm
    {
        public string Code { get; set; } = "";
        public string Position { get; set; } = "";
        public string StatusName { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public bool IsSelectable { get; set; }
    }

    public class SavedVehicleOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
