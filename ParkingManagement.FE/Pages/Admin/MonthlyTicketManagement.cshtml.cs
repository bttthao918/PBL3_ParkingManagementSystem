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

        // Pricing
        public List<EmployeeMonthlyTicketPricingItem> Pricing { get; set; } = new();

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
                SmallCarMonthlyOneMonth = GetMonthlyPrice(SmallCarType, 1),
                SmallCarMonthlyThreeMonth = GetMonthlyPrice(SmallCarType, 3),
                SmallCarMonthlySixMonth = GetMonthlyPrice(SmallCarType, 6),
                LargeCarMonthlyOneMonth = GetMonthlyPrice(LargeCarType, 1),
                LargeCarMonthlyThreeMonth = GetMonthlyPrice(LargeCarType, 3),
                LargeCarMonthlySixMonth = GetMonthlyPrice(LargeCarType, 6)
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
                    PricingInput.MotorcycleMonthlyThreeMonth <= 0 ||
                    PricingInput.MotorcycleMonthlySixMonth <= 0 ||
                    PricingInput.SmallCarMonthlyOneMonth <= 0 ||
                    PricingInput.SmallCarMonthlyThreeMonth <= 0 ||
                    PricingInput.SmallCarMonthlySixMonth <= 0 ||
                    PricingInput.LargeCarMonthlyOneMonth <= 0 ||
                    PricingInput.LargeCarMonthlyThreeMonth <= 0 ||
                    PricingInput.LargeCarMonthlySixMonth <= 0)
                {
                    ActionSuccess = false;
                    ActionMessage = "Giá vé tháng phải lớn hơn 0.";
                    return RedirectToPage(new { Search, StatusFilter, VehicleTypeFilter, PageNumber, PageSize });
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
                            ThreeMonth = PricingInput.MotorcycleMonthlyThreeMonth,
                            SixMonth = PricingInput.MotorcycleMonthlySixMonth
                        },
                        [SmallCarType] = new()
                        {
                            OneMonth = PricingInput.SmallCarMonthlyOneMonth,
                            ThreeMonth = PricingInput.SmallCarMonthlyThreeMonth,
                            SixMonth = PricingInput.SmallCarMonthlySixMonth
                        },
                        [LargeCarType] = new()
                        {
                            OneMonth = PricingInput.LargeCarMonthlyOneMonth,
                            ThreeMonth = PricingInput.LargeCarMonthlyThreeMonth,
                            SixMonth = PricingInput.LargeCarMonthlySixMonth
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

            return RedirectToPage(new { Search, StatusFilter, VehicleTypeFilter, PageNumber, PageSize });
        }
    }
}
