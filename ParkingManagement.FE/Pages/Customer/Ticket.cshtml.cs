using System.Security.Claims;
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
        public string? PricingUpdatedDate { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
            UserName = fallbackName;

            try
            {
                // Load profile, tickets, and pricing in parallel
                var profileTask = _customerApiService.GetProfileAsync();
                var ticketsTask = _customerApiService.GetTicketsAsync(1, 50);
                var pricingTask = _pricingService.GetCurrentPricingAsync();

                await Task.WhenAll(profileTask, ticketsTask, pricingTask);

                var profile = await profileTask;
                var tickets = await ticketsTask;
                var pricing = await pricingTask;

                // Set user name
                if (profile != null && !string.IsNullOrWhiteSpace(profile.FullName))
                {
                    UserName = profile.FullName;
                }

                // Map pricing to PriceCards
                if (pricing != null)
                {
                    PriceCards = BuildPriceCards(pricing);
                    PricingUpdatedDate = pricing.LastUpdatedAt != default
                        ? pricing.LastUpdatedAt.ToString("dd/MM/yyyy")
                        : null;
                }
                else
                {
                    // Fallback pricing if BE unavailable
                    LoadFallbackPriceCards();
                }

                // Map tickets
                if (tickets?.Items != null)
                {
                    Tickets = tickets.Items.Select((t, index) => MapToTicketVm(t, index + 1)).ToList();
                }

                ViewData["UserName"] = UserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load ticket data from BE");
                ErrorMessage = "Không tải được dữ liệu từ hệ thống. Vui lòng kiểm tra kết nối.";
                LoadFallbackPriceCards();
                ViewData["UserName"] = fallbackName;
            }
        }

        private List<TicketPriceVm> BuildPriceCards(PricingDto pricing)
        {
            var cards = new List<TicketPriceVm>();

            // Xe máy
            var motorcycleHourly = pricing.HourlyRate.GetValueOrDefault("Xe máy", 5000);
            var motorcycleMax = pricing.MaxDailyFee.GetValueOrDefault("Xe máy", 10000);
            cards.Add(new TicketPriceVm
            {
                VehicleType = "Xe máy",
                Icon = "fa-solid fa-motorcycle",
                ColorClass = "green",
                FirstHourPrice = motorcycleHourly,
                NextHourPrice = motorcycleHourly,
                OvernightPrice = motorcycleMax
            });

            // Ô tô nhỏ
            var smallCarHourly = pricing.HourlyRate.GetValueOrDefault("Ô tô nhỏ", 15000);
            var smallCarMax = pricing.MaxDailyFee.GetValueOrDefault("Ô tô nhỏ", 40000);
            cards.Add(new TicketPriceVm
            {
                VehicleType = "Ô tô nhỏ",
                Icon = "fa-solid fa-car-side",
                ColorClass = "blue",
                FirstHourPrice = smallCarHourly,
                NextHourPrice = smallCarHourly,
                OvernightPrice = smallCarMax
            });

            // Ô tô lớn
            var largeCarHourly = pricing.HourlyRate.GetValueOrDefault("Ô tô lớn", 25000);
            var largeCarMax = pricing.MaxDailyFee.GetValueOrDefault("Ô tô lớn", 60000);
            cards.Add(new TicketPriceVm
            {
                VehicleType = "Ô tô lớn",
                Icon = "fa-solid fa-van-shuttle",
                ColorClass = "purple",
                FirstHourPrice = largeCarHourly,
                NextHourPrice = largeCarHourly,
                OvernightPrice = largeCarMax
            });

            return cards;
        }

        private void LoadFallbackPriceCards()
        {
            PriceCards = new List<TicketPriceVm>
            {
                new()
                {
                    VehicleType = "Xe máy",
                    Icon = "fa-solid fa-motorcycle",
                    ColorClass = "green",
                    FirstHourPrice = 5000,
                    NextHourPrice = 2000,
                    OvernightPrice = 10000
                },
                new()
                {
                    VehicleType = "Ô tô nhỏ",
                    Icon = "fa-solid fa-car-side",
                    ColorClass = "blue",
                    FirstHourPrice = 15000,
                    NextHourPrice = 5000,
                    OvernightPrice = 40000
                },
                new()
                {
                    VehicleType = "Ô tô lớn",
                    Icon = "fa-solid fa-van-shuttle",
                    ColorClass = "purple",
                    FirstHourPrice = 25000,
                    NextHourPrice = 8000,
                    OvernightPrice = 60000
                }
            };
        }

        private TicketVm MapToTicketVm(CustomerTicketDto t, int index)
        {
            var icon = GetVehicleIcon(t.VehicleType);
            var iconClass = GetVehicleIconClass(t.VehicleType);
            var duration = CalculateDuration(t.CheckInTime, t.CheckOutTime);
            var fee = t.Fee ?? 0;
            var statusText = MapStatus(t.Status);

            return new TicketVm
            {
                Id = index,
                Code = t.TicketId.Length > 12 ? t.TicketId[..12].ToUpper() : t.TicketId.ToUpper(),
                VehiclePlate = t.VehiclePlate,
                VehicleType = t.VehicleType,
                Icon = icon,
                IconClass = iconClass,
                CheckInTime = t.CheckInTime,
                CheckOutTime = t.CheckOutTime,
                Duration = duration,
                ParkingFee = fee,
                Discount = 0,
                TotalAmount = fee,
                Status = statusText,
                PaymentMethod = fee > 0 ? "Đã thanh toán" : "-",
                CreatedBy = "-",
                Note = "-"
            };
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

        private static string GetVehicleIconClass(string vehicleType)
        {
            var normalized = vehicleType.ToLower().Trim();
            if (normalized.Contains("máy") || normalized.Contains("motorcycle"))
                return "green";
            if (normalized.Contains("lớn") || normalized.Contains("large") || normalized.Contains("van"))
                return "purple";
            if (normalized.Contains("ô tô") || normalized.Contains("car"))
                return "blue";
            return "green";
        }

        private static string CalculateDuration(DateTime checkIn, DateTime? checkOut)
        {
            if (checkOut == null)
                return "Đang gửi";

            var span = checkOut.Value - checkIn;
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} phút";
            if (span.Minutes == 0)
                return $"{(int)span.TotalHours} giờ";
            return $"{(int)span.TotalHours} giờ {span.Minutes} phút";
        }

        private static string MapStatus(string status)
        {
            var normalized = status.ToLower().Trim();
            if (normalized.Contains("paid") || normalized.Contains("thanh toán") || normalized.Contains("completed"))
                return "Đã thanh toán";
            if (normalized.Contains("active") || normalized.Contains("đang") || normalized.Contains("checked in"))
                return "Đang gửi";
            if (normalized.Contains("unpaid") || normalized.Contains("chưa"))
                return "Chưa thanh toán";
            return status;
        }
    }

    public class TicketPriceVm
    {
        public string VehicleType { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
        public decimal FirstHourPrice { get; set; }
        public decimal NextHourPrice { get; set; }
        public decimal OvernightPrice { get; set; }
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
