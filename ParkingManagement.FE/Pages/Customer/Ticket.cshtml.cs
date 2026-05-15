using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer
{
    public class TicketModel : PageModel
    {
        private readonly IPricingService _pricingService;

        public TicketModel(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        public string UserName { get; set; } = "Customer";

        public List<TicketPriceVm> PriceCards { get; set; } = new();

        public List<TicketVm> Tickets { get; set; } = new();

        public DateTime PricingUpdatedAt { get; set; } = DateTime.Today;

        public async Task OnGetAsync()
        {
            var pricing = await LoadPricingAsync();
            PricingUpdatedAt = pricing.LastUpdatedAt == default ? DateTime.Today : pricing.LastUpdatedAt;
            LoadPriceCards(pricing);
            LoadTickets();
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

        private void LoadPriceCards(PricingDto pricing)
        {
            PriceCards = new List<TicketPriceVm>
        {
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
        };
        }

        private void LoadTickets()
        {
            Tickets = new List<TicketVm>
        {
            new()
            {
                Id = 1,
                Code = "VE240501-0001",
                VehiclePlate = "59C1-123.45",
                VehicleType = "Xe máy",
                Icon = "fa-solid fa-motorcycle",
                IconClass = "green",
                CheckInTime = new DateTime(2024, 5, 1, 8, 15, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 10, 45, 0),
                Duration = "2 giờ 30 phút",
                ParkingFee = 10000,
                Discount = 0,
                TotalAmount = 10000,
                Status = "Đã thanh toán",
                PaymentMethod = "Tiền mặt",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            },
            new()
            {
                Id = 2,
                Code = "VE240501-0002",
                VehiclePlate = "51A-567.89",
                VehicleType = "Ô tô nhỏ",
                Icon = "fa-solid fa-car-side",
                IconClass = "blue",
                CheckInTime = new DateTime(2024, 5, 1, 9, 20, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 12, 20, 0),
                Duration = "3 giờ",
                ParkingFee = 30000,
                Discount = 0,
                TotalAmount = 30000,
                Status = "Đã thanh toán",
                PaymentMethod = "Chuyển khoản",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            },
            new()
            {
                Id = 3,
                Code = "VE240501-0003",
                VehiclePlate = "30E-246.80",
                VehicleType = "Ô tô lớn",
                Icon = "fa-solid fa-van-shuttle",
                IconClass = "purple",
                CheckInTime = new DateTime(2024, 5, 1, 10, 5, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 15, 30, 0),
                Duration = "5 giờ 25 phút",
                ParkingFee = 65000,
                Discount = 0,
                TotalAmount = 65000,
                Status = "Đã thanh toán",
                PaymentMethod = "Tiền mặt",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            },
            new()
            {
                Id = 4,
                Code = "VE240501-0004",
                VehiclePlate = "59C2-345.67",
                VehicleType = "Xe máy",
                Icon = "fa-solid fa-motorcycle",
                IconClass = "green",
                CheckInTime = new DateTime(2024, 5, 1, 11, 10, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 11, 50, 0),
                Duration = "40 phút",
                ParkingFee = 5000,
                Discount = 0,
                TotalAmount = 5000,
                Status = "Đã thanh toán",
                PaymentMethod = "Tiền mặt",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            }
        };
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
