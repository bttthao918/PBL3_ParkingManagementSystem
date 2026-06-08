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
        private readonly IParkingSlotService _parkingSlotService;
        private readonly ILogger<BookingModel> _logger;

        public BookingModel(
            ICustomerApiService customerApiService,
            IReservationService reservationService,
            IParkingSlotService parkingSlotService,
            ILogger<BookingModel> logger)
        {
            _customerApiService = customerApiService;
            _reservationService = reservationService;
            _parkingSlotService = parkingSlotService;
            _logger = logger;
        }

        public string UserName { get; set; } = "Customer";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public int TotalBooking { get; set; }
        public int WaitingBooking { get; set; }
        public int ReceivedBooking { get; set; }
        public int ExpiredBooking { get; set; }
        public int CancelledBooking { get; set; }

        public List<BookingVm> Bookings { get; set; } = new();
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
        public List<SavedVehicleOption> SavedVehicles { get; set; } = new();
        public string SavedVehicleStorageKey { get; set; } = "parking.customer.savedVehicles.anonymous";

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
            if (result?.Success == true)
            {
                TempData["Success"] = result.Message ?? "Đặt chỗ thành công!";
            }
            else
            {
                TempData["Error"] = result?.Message ?? "Không thể đặt chỗ. Vui lòng thử lại.";
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
            var fallbackUserKey = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("accountId")?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? fallbackName;
            UserName = fallbackName;
            SavedVehicleStorageKey = BuildSavedVehicleStorageKey(fallbackUserKey);

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

                if (profile != null)
                {
                    var profileStorageKey = FirstNonEmpty(profile.CustomerId, profile.Email, profile.FullName, fallbackUserKey);
                    SavedVehicleStorageKey = BuildSavedVehicleStorageKey(profileStorageKey);
                }

                // Map reservations to BookingVm
                if (reservations?.Items != null)
                {
                    Bookings = reservations.Items.Select((r, index) => MapToBookingVm(r, index + 1)).ToList();

                    TotalBooking = reservations.TotalItems > 0 ? reservations.TotalItems : reservations.Items.Count;
                    WaitingBooking = reservations.Items.Count(r => IsWaiting(r.Status));
                    ReceivedBooking = reservations.Items.Count(r => IsReceived(r.Status));
                    ExpiredBooking = reservations.Items.Count(r => IsExpired(r.Status));
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
            var canCancel = IsWaiting(r.Status) && r.ExpectedTime >= DateTime.Now;

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
            if (IsWaiting(status))
                return "waiting";
            if (IsReceived(status))
                return "received";
            if (IsExpired(status))
                return "expired";
            if (IsCancelled(status))
                return "cancelled";
            return "neutral";
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
            var normalized = NormalizeVehicleType(vehicleType);
            if (normalized == "motorcycle")
                return "green";
            if (normalized == "small-car")
                return "blue";
            if (normalized == "large-car")
                return "purple";
            return "green";
        }

        private static string GetVehicleIcon(string vehicleType)
        {
            return NormalizeVehicleType(vehicleType) switch
            {
                "small-car" => "fa-solid fa-car-side",
                "large-car" => "fa-solid fa-van-shuttle",
                _ => "fa-solid fa-motorcycle"
            };
        }

        private static bool IsWaiting(string status)
        {
            var normalized = NormalizeVietnameseText(status);
            return normalized.Contains("cho") ||
                normalized.Contains("pending") ||
                normalized.Contains("waiting");
        }

        private static bool IsReceived(string status)
        {
            var normalized = NormalizeVietnameseText(status);
            return normalized.Contains("da nhan") ||
                normalized.Contains("received") ||
                normalized.Contains("confirmed") ||
                normalized.Contains("completed") ||
                normalized.Contains("done");
        }

        private static bool IsExpired(string status)
        {
            var normalized = NormalizeVietnameseText(status);
            return normalized.Contains("het han") || normalized.Contains("expired");
        }

        private static bool IsCancelled(string status)
        {
            var normalized = NormalizeVietnameseText(status);
            return normalized.Contains("huy") || normalized.Contains("cancel");
        }

        private static bool IsEmptySlotStatus(string? status)
        {
            var normalized = NormalizeVietnameseText(status);
            return normalized == "trong" || normalized == "empty" || normalized == "available";
        }

        private static bool VehicleTypeMatches(string? slotVehicleType, string? requestedVehicleType)
        {
            if (string.IsNullOrWhiteSpace(requestedVehicleType))
            {
                return true;
            }

            return NormalizeVehicleType(slotVehicleType) == NormalizeVehicleType(requestedVehicleType);
        }

        private static string NormalizeVehicleType(string? value)
        {
            var normalized = NormalizeVietnameseText(value);

            if (normalized.Contains("may") || normalized.Contains("motor"))
            {
                return "motorcycle";
            }

            if (normalized.Contains("nho") || normalized.Contains("small"))
            {
                return "small-car";
            }

            if (normalized.Contains("lon") || normalized.Contains("large") || normalized.Contains("van"))
            {
                return "large-car";
            }

            return normalized;
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "anonymous";
        }

        private static string BuildSavedVehicleStorageKey(string? ownerKey)
        {
            var normalizedOwner = string.IsNullOrWhiteSpace(ownerKey)
                ? "anonymous"
                : ownerKey.Trim().ToLowerInvariant();
            var safeOwner = new string(normalizedOwner
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
                .ToArray())
                .Trim('_');

            if (string.IsNullOrWhiteSpace(safeOwner))
            {
                safeOwner = "anonymous";
            }

            return $"parking.customer.savedVehicles.{safeOwner}";
        }

        private static string NormalizeVietnameseText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .Select(c => c == '\u0111' || c == '\u0110' ? 'd' : c);

            return new string(chars.ToArray()).Normalize(System.Text.NormalizationForm.FormC);
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
