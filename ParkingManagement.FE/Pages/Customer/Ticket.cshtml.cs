using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class TicketModel : PageModel
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly IPricingService _pricingService;
        private readonly ILogger<TicketModel> _logger;

        public TicketModel(
            ICustomerApiService customerApiService,
            IPricingService pricingService,
            ILogger<TicketModel> logger)
        {
            _customerApiService = customerApiService;
            _pricingService = pricingService;
            _logger = logger;
        }

        public string UserName { get; set; } = "Customer";

        public string? ErrorMessage { get; set; }

        public List<TicketPriceVm> PriceCards { get; set; } = new();

        public List<TicketVm> Tickets { get; set; } = new();

        public DateTime PricingUpdatedAt { get; set; } = DateTime.Today;

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            ViewData["Title"] = "Quản lý vé xe";
            ViewData["Role"] = "Khách hàng";

            var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
            UserName = fallbackName;

            try
            {
                var profileTask = _customerApiService.GetProfileAsync();
                var ticketsTask = _customerApiService.GetTicketsAsync(1, 50);
                var pricingTask = LoadPricingAsync();

                await Task.WhenAll(profileTask, ticketsTask, pricingTask);

                var profile = await profileTask;
                var tickets = await ticketsTask ?? new ListCustomerTicketDto();
                var pricing = await pricingTask;

                UserName = string.IsNullOrWhiteSpace(profile?.FullName)
                    ? fallbackName
                    : profile.FullName;

                PricingUpdatedAt = pricing.LastUpdatedAt == default
                    ? DateTime.Today
                    : pricing.LastUpdatedAt;
                PriceCards = BuildPriceCards(pricing);
                Tickets = tickets.Items
                    .OrderByDescending(x => x.CheckInTime)
                    .Select((ticket, index) => MapToTicketVm(ticket, index + 1))
                    .ToList();

                ViewData["UserName"] = UserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load customer ticket data");
                ErrorMessage = "Không tải được dữ liệu vé từ hệ thống. Vui lòng kiểm tra BE và phiên đăng nhập.";
                PriceCards = BuildPriceCards(PricingDisplayDefaults.CreateDefaultPricing());
                ViewData["UserName"] = fallbackName;
            }
        }

        private async Task<PricingDto> LoadPricingAsync()
        {
            try
            {
                return await _pricingService.GetCurrentPricingAsync()
                    ?? PricingDisplayDefaults.CreateDefaultPricing();
            }
            catch
            {
                return PricingDisplayDefaults.CreateDefaultPricing();
            }
        }

        private static List<TicketPriceVm> BuildPriceCards(PricingDto pricing) =>
        [
            new()
            {
                VehicleType = PricingDisplayDefaults.Motorcycle,
                Icon = "fa-solid fa-motorcycle",
                ColorClass = "green",
                HourlyRate = PricingDisplayDefaults.GetHourlyRate(pricing, PricingDisplayDefaults.Motorcycle),
                MaxDailyFee = PricingDisplayDefaults.GetMaxDailyFee(pricing, PricingDisplayDefaults.Motorcycle)
            },
            new()
            {
                VehicleType = PricingDisplayDefaults.SmallCar,
                Icon = "fa-solid fa-car-side",
                ColorClass = "blue",
                HourlyRate = PricingDisplayDefaults.GetHourlyRate(pricing, PricingDisplayDefaults.SmallCar),
                MaxDailyFee = PricingDisplayDefaults.GetMaxDailyFee(pricing, PricingDisplayDefaults.SmallCar)
            },
            new()
            {
                VehicleType = PricingDisplayDefaults.LargeCar,
                Icon = "fa-solid fa-van-shuttle",
                ColorClass = "purple",
                HourlyRate = PricingDisplayDefaults.GetHourlyRate(pricing, PricingDisplayDefaults.LargeCar),
                MaxDailyFee = PricingDisplayDefaults.GetMaxDailyFee(pricing, PricingDisplayDefaults.LargeCar)
            }
        ];

        private static TicketVm MapToTicketVm(CustomerTicketDto ticket, int index)
        {
            var icon = GetVehicleIcon(ticket.VehicleType);
            var iconClass = GetVehicleIconClass(ticket.VehicleType);
            var fee = ticket.Fee ?? 0m;

            return new TicketVm
            {
                Id = index,
                Code = string.IsNullOrWhiteSpace(ticket.TicketId)
                    ? $"VE-{index:D4}"
                    : (ticket.TicketId.Length > 12 ? ticket.TicketId[..12] : ticket.TicketId).ToUpperInvariant(),
                VehiclePlate = ticket.VehiclePlate,
                VehicleType = ticket.VehicleType,
                Icon = icon,
                IconClass = iconClass,
                CheckInTime = ticket.CheckInTime,
                CheckOutTime = ticket.CheckOutTime,
                Duration = CalculateDuration(ticket.CheckInTime, ticket.CheckOutTime),
                ParkingFee = fee,
                Discount = 0m,
                TotalAmount = fee,
                Status = MapStatus(ticket.Status),
                PaymentMethod = fee > 0m ? "Đã thanh toán" : "-",
                CreatedBy = "-",
                Note = "-"
            };
        }

        private static string GetVehicleIcon(string vehicleType)
        {
            var normalized = NormalizeText(vehicleType);
            if (normalized.Contains("may") || normalized.Contains("motorcycle"))
            {
                return "fa-solid fa-motorcycle";
            }

            if (normalized.Contains("lon") || normalized.Contains("large") || normalized.Contains("van"))
            {
                return "fa-solid fa-van-shuttle";
            }

            if (normalized.Contains("o to") || normalized.Contains("car"))
            {
                return "fa-solid fa-car-side";
            }

            return "fa-solid fa-motorcycle";
        }

        private static string GetVehicleIconClass(string vehicleType)
        {
            var normalized = NormalizeText(vehicleType);
            if (normalized.Contains("may") || normalized.Contains("motorcycle"))
            {
                return "green";
            }

            if (normalized.Contains("lon") || normalized.Contains("large") || normalized.Contains("van"))
            {
                return "purple";
            }

            if (normalized.Contains("o to") || normalized.Contains("car"))
            {
                return "blue";
            }

            return "green";
        }

        private static string CalculateDuration(DateTime checkIn, DateTime? checkOut)
        {
            if (checkOut == null)
            {
                return "Đang gửi";
            }

            var span = checkOut.Value - checkIn;
            if (span.TotalMinutes < 1)
            {
                return "Dưới 1 phút";
            }

            if (span.TotalMinutes < 60)
            {
                return $"{(int)Math.Ceiling(span.TotalMinutes)} phút";
            }

            if (span.Minutes == 0)
            {
                return $"{(int)span.TotalHours} giờ";
            }

            return $"{(int)span.TotalHours} giờ {span.Minutes} phút";
        }

        private static string MapStatus(string status)
        {
            var normalized = NormalizeText(status);
            if (normalized.Contains("paid") ||
                normalized.Contains("thanh toan") ||
                normalized.Contains("completed") ||
                normalized.Contains("checked out") ||
                normalized.Contains("checkout") ||
                normalized.Contains("da ra"))
            {
                return "Đã thanh toán";
            }

            if (normalized.Contains("active") ||
                normalized.Contains("checked in") ||
                normalized.Contains("dang") ||
                normalized.Contains("trong bai"))
            {
                return "Đang gửi";
            }

            if (normalized.Contains("unpaid") || normalized.Contains("chua"))
            {
                return "Chưa thanh toán";
            }

            return string.IsNullOrWhiteSpace(status) ? "-" : status;
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd');
        }
    }

    public class TicketPriceVm
    {
        public string VehicleType { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string ColorClass { get; set; } = string.Empty;

        public decimal HourlyRate { get; set; }

        public decimal MaxDailyFee { get; set; }

        public string MinimumDurationText { get; set; } = "15 phút";
    }

    public class TicketVm
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string VehiclePlate { get; set; } = string.Empty;

        public string VehicleType { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string IconClass { get; set; } = string.Empty;

        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string Duration { get; set; } = string.Empty;

        public decimal ParkingFee { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }
}
