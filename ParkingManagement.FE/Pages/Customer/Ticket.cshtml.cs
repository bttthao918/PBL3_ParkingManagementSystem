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
        public string? PricingUpdatedDate { get; set; }

        public List<TicketPriceVm> PriceCards { get; set; } = new();
        public List<TicketVm> Tickets { get; set; } = new();

        public async Task OnGetAsync()
        {
            var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
            UserName = fallbackName;

            try
            {
                var profileTask = _customerApiService.GetProfileAsync();
                var ticketsTask = _customerApiService.GetTicketsAsync(1, 100);
                var pricingTask = _pricingService.GetCurrentPricingAsync();

                await Task.WhenAll(profileTask, ticketsTask, pricingTask);

                var profile = await profileTask;
                var tickets = await ticketsTask;
                var pricing = await pricingTask;

                if (profile != null && !string.IsNullOrWhiteSpace(profile.FullName))
                    UserName = profile.FullName;

                // Map pricing to PriceCards
                if (pricing != null)
                {
                    PricingUpdatedDate = pricing.LastUpdatedAt.ToString("dd/MM/yyyy");
                    PriceCards = BuildPriceCards(pricing);
                }
                else
                {
                    // Fallback pricing
                    PricingUpdatedDate = DateTime.Now.ToString("dd/MM/yyyy");
                    PriceCards = GetFallbackPriceCards();
                }

                // Map tickets
                if (tickets?.Items != null)
                {
                    Tickets = tickets.Items.Select((t, idx) => new TicketVm
                    {
                        Id = idx + 1,
                        Code = t.TicketId,
                        VehiclePlate = t.VehiclePlate,
                        VehicleType = t.VehicleType,
                        Icon = GetVehicleIcon(t.VehicleType),
                        IconClass = GetVehicleClass(t.VehicleType),
                        CheckInTime = t.CheckInTime,
                        CheckOutTime = t.CheckOutTime,
                        Duration = CalculateDuration(t.CheckInTime, t.CheckOutTime),
                        ParkingFee = t.Fee ?? 0,
                        Discount = 0,
                        TotalAmount = t.Fee ?? 0,
                        Status = MapTicketStatus(t.Status),
                        PaymentMethod = "",
                        CreatedBy = "",
                        Note = "-"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load ticket data from BE");
                ErrorMessage = "Không thể tải dữ liệu vé. Vui lòng kiểm tra kết nối BE.";
                PriceCards = GetFallbackPriceCards();
                PricingUpdatedDate = DateTime.Now.ToString("dd/MM/yyyy");
            }

            ViewData["UserName"] = UserName;
        }

        // ── Helpers ──

        private static List<TicketPriceVm> BuildPriceCards(PricingDto pricing)
        {
            var cards = new List<TicketPriceVm>();

            var vehicleTypes = new[]
            {
                ("Xe máy", "fa-solid fa-motorcycle", "green"),
                ("Ô tô nhỏ", "fa-solid fa-car-side", "blue"),
                ("Ô tô lớn", "fa-solid fa-van-shuttle", "purple")
            };

            foreach (var (type, icon, color) in vehicleTypes)
            {
                var hourlyRate = pricing.HourlyRate.GetValueOrDefault(type, 0);
                var maxDaily = pricing.MaxDailyFee.GetValueOrDefault(type, 0);

                cards.Add(new TicketPriceVm
                {
                    VehicleType = type,
                    Icon = icon,
                    ColorClass = color,
                    FirstHourPrice = hourlyRate,
                    NextHourPrice = hourlyRate,
                    OvernightPrice = maxDaily
                });
            }

            return cards;
        }

        private static List<TicketPriceVm> GetFallbackPriceCards() => new()
        {
            new() { VehicleType = "Xe máy", Icon = "fa-solid fa-motorcycle", ColorClass = "green", FirstHourPrice = 5000, NextHourPrice = 2000, OvernightPrice = 10000 },
            new() { VehicleType = "Ô tô nhỏ", Icon = "fa-solid fa-car-side", ColorClass = "blue", FirstHourPrice = 15000, NextHourPrice = 5000, OvernightPrice = 40000 },
            new() { VehicleType = "Ô tô lớn", Icon = "fa-solid fa-van-shuttle", ColorClass = "purple", FirstHourPrice = 25000, NextHourPrice = 8000, OvernightPrice = 60000 }
        };

        private static string CalculateDuration(DateTime checkIn, DateTime? checkOut)
        {
            if (checkOut == null) return "Đang gửi";

            var duration = checkOut.Value - checkIn;
            if (duration.TotalMinutes < 60)
                return $"{(int)duration.TotalMinutes} phút";

            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            return minutes > 0 ? $"{hours} giờ {minutes} phút" : $"{hours} giờ";
        }

        private static string MapTicketStatus(string status)
        {
            var s = status.ToLower();
            if (s.Contains("paid") || s.Contains("thanh toán") || s.Contains("completed")) return "Đã thanh toán";
            if (s.Contains("active") || s.Contains("checked in") || s.Contains("đang")) return "Đang gửi";
            if (s.Contains("unpaid") || s.Contains("chưa")) return "Chưa thanh toán";
            return status;
        }

        private static string GetVehicleIcon(string vehicleType) => vehicleType switch
        {
            "Xe máy" => "fa-solid fa-motorcycle",
            "Ô tô nhỏ" => "fa-solid fa-car-side",
            "Ô tô lớn" => "fa-solid fa-van-shuttle",
            _ => "fa-solid fa-motorcycle"
        };

        private static string GetVehicleClass(string vehicleType) => vehicleType switch
        {
            "Xe máy" => "green",
            "Ô tô nhỏ" => "blue",
            "Ô tô lớn" => "purple",
            _ => "green"
        };
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
