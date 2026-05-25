using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class MonthlyTicketManagementModel : PageModel
    {
        private readonly IEmployeeMonthlyTicketService _service;
        private readonly IPricingService _pricingService;
        private const string MotorcycleType = "Xe máy";
        private const string SmallCarType = "Ô tô nhỏ";
        private const string LargeCarType = "Ô tô lớn";

        public MonthlyTicketManagementModel(IEmployeeMonthlyTicketService service, IPricingService pricingService)
        {
            _service = service;
            _pricingService = pricingService;
        }

        // KPI
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int ExpiredTickets { get; set; }
        public int ExpiringSoonTickets { get; set; }

        // Filter & Paging
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? VehicleTypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        // Data
        public List<EmployeeMonthlyTicketItem> Tickets { get; set; } = new();
        public EmployeeMonthlyTicketDetailResponse? SelectedTicket { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowPricingPanel { get; set; }

        // Pricing
        public List<EmployeeMonthlyTicketPricingItem> Pricing { get; set; } = new();
        public DateTime? PricingLastUpdatedAt { get; set; }

        [BindProperty]
        public PricingInputModel PricingInput { get; set; } = new();

        // Messages
        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Quản lý vé tháng";
            ViewData["Role"] = "Quản lý";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Quản lý";

            PageNumber = Math.Max(1, PageNumber);

            var result = await _service.GetAllAsync(Search, StatusFilter, VehicleTypeFilter, PageNumber, PageSize);
            if (result != null)
            {
                Tickets = result.Items;
                TotalItems = result.TotalItems;
                TotalPages = result.TotalPages;
                TotalTickets = result.Summary.Total;
                ActiveTickets = result.Summary.Active;
                ExpiredTickets = result.Summary.Expired;
                ExpiringSoonTickets = result.Summary.ExpiringSoon;
            }

            // Load selected ticket detail
            if (!string.IsNullOrEmpty(SelectedId))
            {
                SelectedTicket = await _service.GetDetailAsync(SelectedId);
            }

            var currentPricing = await _pricingService.GetCurrentPricingAsync();
            PricingLastUpdatedAt = currentPricing?.LastUpdatedAt;

            // Load pricing
            var pricing = await _service.GetPricingAsync();
            if (pricing != null && pricing.Any())
            {
                Pricing = pricing;
            }
            else
            {
                Pricing = CreateDefaultPricing();
            }

            LoadPricingInput();
        }

        private void LoadPricingInput()
        {
            PricingInput = new PricingInputModel
            {
                MotorcycleMonthlyOneMonth = GetMonthlyPrice(MotorcycleType, 1),
                MotorcycleMonthlyThreeMonth = GetMonthlyPrice(MotorcycleType, 3),
                MotorcycleMonthlySixMonth = GetMonthlyPrice(MotorcycleType, 6),
                MotorcycleMonthlyThreeMonthDiscountPercent = GetMonthlyDiscountPercent(MotorcycleType, 3),
                MotorcycleMonthlySixMonthDiscountPercent = GetMonthlyDiscountPercent(MotorcycleType, 6),
                SmallCarMonthlyOneMonth = GetMonthlyPrice(SmallCarType, 1),
                SmallCarMonthlyThreeMonth = GetMonthlyPrice(SmallCarType, 3),
                SmallCarMonthlySixMonth = GetMonthlyPrice(SmallCarType, 6),
                SmallCarMonthlyThreeMonthDiscountPercent = GetMonthlyDiscountPercent(SmallCarType, 3),
                SmallCarMonthlySixMonthDiscountPercent = GetMonthlyDiscountPercent(SmallCarType, 6),
                LargeCarMonthlyOneMonth = GetMonthlyPrice(LargeCarType, 1),
                LargeCarMonthlyThreeMonth = GetMonthlyPrice(LargeCarType, 3),
                LargeCarMonthlySixMonth = GetMonthlyPrice(LargeCarType, 6),
                LargeCarMonthlyThreeMonthDiscountPercent = GetMonthlyDiscountPercent(LargeCarType, 3),
                LargeCarMonthlySixMonthDiscountPercent = GetMonthlyDiscountPercent(LargeCarType, 6)
            };
        }

        public decimal GetMonthlyPrice(string vehicleType, int months)
        {
            var price = Pricing
                .FirstOrDefault(item =>
                    item.Months == months &&
                    string.Equals(item.VehicleType, vehicleType, StringComparison.OrdinalIgnoreCase))
                ?.Price ?? 0m;

            if (price > 0)
                return price;

            return CreateDefaultPricing()
                .FirstOrDefault(item =>
                    item.Months == months &&
                    string.Equals(item.VehicleType, vehicleType, StringComparison.OrdinalIgnoreCase))
                ?.Price ?? 0m;
        }

        public decimal GetMonthlySaving(string vehicleType, int months)
        {
            if (months <= 1)
                return 0m;

            var oneMonthPrice = GetMonthlyPrice(vehicleType, 1);
            var packagePrice = GetMonthlyPrice(vehicleType, months);
            var saving = (oneMonthPrice * months) - packagePrice;

            return saving > 0 ? saving : 0m;
        }

        public decimal GetMonthlyDiscountPercent(string vehicleType, int months)
        {
            if (months <= 1)
                return 0m;

            var oneMonthPrice = GetMonthlyPrice(vehicleType, 1);
            var packagePrice = GetMonthlyPrice(vehicleType, months);
            var fullPrice = oneMonthPrice * months;

            if (oneMonthPrice <= 0 || packagePrice <= 0 || fullPrice <= 0)
                return 0m;

            var discount = (1m - packagePrice / fullPrice) * 100m;
            return Math.Round(Math.Max(0m, discount), 2, MidpointRounding.AwayFromZero);
        }

        private static bool HasInvalidDiscount(decimal discountPercent)
            => discountPercent < 0 || discountPercent >= 100;

        private static List<EmployeeMonthlyTicketPricingItem> CreateDefaultPricing()
        {
            var defaultPricing = PricingDisplayDefaults.CreateDefaultPricing();
            var vehicleTypes = new[] { MotorcycleType, SmallCarType, LargeCarType };
            var months = new[] { 1, 3, 6 };

            return vehicleTypes
                .SelectMany(vehicleType => months.Select(month => new EmployeeMonthlyTicketPricingItem
                {
                    VehicleType = vehicleType,
                    Months = month,
                    PackageType = $"{month} tháng",
                    Price = PricingDisplayDefaults.GetMonthlyTicketPrice(defaultPricing, vehicleType, month)
                }))
                .ToList();
        }



        public async Task<IActionResult> OnPostUpdatePricingAsync()
        {
            try
            {
                if (PricingInput.MotorcycleMonthlyOneMonth <= 0 ||
                    PricingInput.SmallCarMonthlyOneMonth <= 0 ||
                    PricingInput.LargeCarMonthlyOneMonth <= 0)
                {
                    ActionSuccess = false;
                    ActionMessage = "Giá vé 1 tháng phải lớn hơn 0.";
                    return RedirectToPage(BuildRouteValues(showPricingPanel: true));
                }

                if (HasInvalidDiscount(PricingInput.MotorcycleMonthlyThreeMonthDiscountPercent) ||
                    HasInvalidDiscount(PricingInput.MotorcycleMonthlySixMonthDiscountPercent) ||
                    HasInvalidDiscount(PricingInput.SmallCarMonthlyThreeMonthDiscountPercent) ||
                    HasInvalidDiscount(PricingInput.SmallCarMonthlySixMonthDiscountPercent) ||
                    HasInvalidDiscount(PricingInput.LargeCarMonthlyThreeMonthDiscountPercent) ||
                    HasInvalidDiscount(PricingInput.LargeCarMonthlySixMonthDiscountPercent))
                {
                    ActionSuccess = false;
                    ActionMessage = "Phần trăm giảm phải từ 0 đến dưới 100.";
                    return RedirectToPage(BuildRouteValues(showPricingPanel: true));
                }

                var currentPricing = await _pricingService.GetCurrentPricingAsync();
                
                var input = new UpdatePricingDto
                {
                    HourlyRate = currentPricing?.HourlyRate ?? new Dictionary<string, decimal>(),
                    MaxDailyFee = currentPricing?.MaxDailyFee ?? new Dictionary<string, decimal>(),
                    MonthlyTicketPrice = new Dictionary<string, UpdateMonthlyPricingDto>
                    {
                        [MotorcycleType] = new()
                        {
                            OneMonth = PricingInput.MotorcycleMonthlyOneMonth,
                            ThreeMonthDiscountPercent = PricingInput.MotorcycleMonthlyThreeMonthDiscountPercent,
                            SixMonthDiscountPercent = PricingInput.MotorcycleMonthlySixMonthDiscountPercent
                        },
                        [SmallCarType] = new()
                        {
                            OneMonth = PricingInput.SmallCarMonthlyOneMonth,
                            ThreeMonthDiscountPercent = PricingInput.SmallCarMonthlyThreeMonthDiscountPercent,
                            SixMonthDiscountPercent = PricingInput.SmallCarMonthlySixMonthDiscountPercent
                        },
                        [LargeCarType] = new()
                        {
                            OneMonth = PricingInput.LargeCarMonthlyOneMonth,
                            ThreeMonthDiscountPercent = PricingInput.LargeCarMonthlyThreeMonthDiscountPercent,
                            SixMonthDiscountPercent = PricingInput.LargeCarMonthlySixMonthDiscountPercent
                        }
                    }
                };

                var result = await _pricingService.UpdatePricingAsync(input);
                ActionSuccess = result?.Success == true;
                ActionMessage = ActionSuccess
                    ? "Đã cập nhật bảng giá vé tháng."
                    : result?.Message ?? "Không thể cập nhật bảng giá vé tháng.";
            }
            catch (Exception ex)
            {
                ActionSuccess = false;
                ActionMessage = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage(BuildRouteValues(showPricingPanel: true));
        }

        private object BuildRouteValues(bool showPricingPanel = false)
        {
            return new
            {
                Search,
                StatusFilter,
                VehicleTypeFilter,
                PageNumber,
                PageSize,
                ShowPricingPanel = showPricingPanel
            };
        }
    }
}
