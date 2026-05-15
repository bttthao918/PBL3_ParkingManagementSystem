using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class TicketManagementModel : PageModel
    {
        private readonly ITicketService _ticketService;
        private readonly IPricingService _pricingService;
        private const string MotorcycleType = "Xe máy";
        private const string SmallCarType = "Ô tô nhỏ";
        private const string LargeCarType = "Ô tô lớn";

        public TicketManagementModel(ITicketService ticketService, IPricingService pricingService)
        {
            _ticketService = ticketService;
            _pricingService = pricingService;
        }

        public List<TicketViewModel> Tickets { get; set; } = new();
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int CheckedOutTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int ShowingFrom { get; set; }
        public int ShowingTo { get; set; }
        public string? LoadErrorMessage { get; set; }
        public PricingDto Pricing { get; set; } = new();

        [BindProperty]
        public PricingInputModel PricingInput { get; set; } = new();

        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Type { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int StartPage => TotalPages == 0 ? 0 : Math.Max(1, PageNumber - 2);
        public int EndPage => TotalPages == 0 ? 0 : Math.Min(TotalPages, PageNumber + 2);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
        public decimal ActiveRate => TotalTickets == 0 ? 0 : ActiveTickets * 100m / TotalTickets;
        public decimal CheckedOutRate => TotalTickets == 0 ? 0 : CheckedOutTickets * 100m / TotalTickets;

        public async Task<IActionResult> OnGetAsync()
        {
            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            PageSize = PageSize <= 0 ? 10 : Math.Min(PageSize, 100);

            await LoadPricingAsync();

            var searchDto = new EmployeeTicketSearchDto
            {
                SearchKeyword = Keyword,
                Status = Status,
                VehicleType = Type,
                FromDate = FromDate,
                ToDate = ToDate,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            try
            {
                var summary = await _ticketService.GetTicketSummaryAsync();
                if (summary != null)
                {
                    TotalTickets = summary.TotalTickets;
                    ActiveTickets = summary.ActiveTickets;
                    CheckedOutTickets = summary.CheckedOutTickets;
                    TotalRevenue = summary.TotalRevenue;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }
            catch (Exception ex)
            {
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }

            try
            {
                var result = await _ticketService.SearchTicketsAsync(searchDto);
                if (result != null && result.TotalPages > 0 && PageNumber > result.TotalPages)
                {
                    PageNumber = result.TotalPages;
                    searchDto.PageNumber = PageNumber;
                    result = await _ticketService.SearchTicketsAsync(searchDto);
                }
                if (result == null)
                {
                    LoadErrorMessage ??= "Không tải được danh sách vé từ Backend API.";
                    return Page();
                }

                TotalItems = result.TotalItems;
                TotalPages = result.TotalPages;
                PageNumber = result.PageNumber > 0 ? result.PageNumber : PageNumber;
                ShowingFrom = TotalItems == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
                ShowingTo = TotalItems == 0 ? 0 : Math.Min(PageNumber * PageSize, TotalItems);

                Tickets = result.Items.Select(t => new TicketViewModel(
                    t.TicketId,
                    t.VehiclePlate,
                    t.CustomerName ?? "Khách vãng lai",
                    t.VehicleType,
                    t.Fee ?? 0,
                    t.CheckInTime,
                    t.CheckOutTime ?? DateTime.MinValue,
                    t.Status,
                    GetStatusClass(t.Status),
                    t.SlotId ?? ""
                )).ToList();
            }
            catch (UnauthorizedAccessException ex)
            {
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tickets: {ex.Message}");
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePricingAsync()
        {
            try
            {
                if (PricingInput.MotorcycleHourlyRate <= 0 ||
                    PricingInput.MotorcycleMaxDailyFee <= 0 ||
                    PricingInput.MotorcycleMonthlyOneMonth <= 0 ||
                    PricingInput.MotorcycleMonthlyThreeMonth <= 0 ||
                    PricingInput.MotorcycleMonthlySixMonth <= 0 ||
                    PricingInput.SmallCarHourlyRate <= 0 ||
                    PricingInput.SmallCarMaxDailyFee <= 0 ||
                    PricingInput.SmallCarMonthlyOneMonth <= 0 ||
                    PricingInput.SmallCarMonthlyThreeMonth <= 0 ||
                    PricingInput.SmallCarMonthlySixMonth <= 0 ||
                    PricingInput.LargeCarHourlyRate <= 0 ||
                    PricingInput.LargeCarMaxDailyFee <= 0 ||
                    PricingInput.LargeCarMonthlyOneMonth <= 0 ||
                    PricingInput.LargeCarMonthlyThreeMonth <= 0 ||
                    PricingInput.LargeCarMonthlySixMonth <= 0)
                {
                    ActionSuccess = false;
                    ActionMessage = "Giá vé phải lớn hơn 0.";
                    return RedirectToPage(BuildRouteValues());
                }

                var input = new UpdatePricingDto
                {
                    HourlyRate = new Dictionary<string, decimal>
                    {
                        [MotorcycleType] = PricingInput.MotorcycleHourlyRate,
                        [SmallCarType] = PricingInput.SmallCarHourlyRate,
                        [LargeCarType] = PricingInput.LargeCarHourlyRate
                    },
                    MaxDailyFee = new Dictionary<string, decimal>
                    {
                        [MotorcycleType] = PricingInput.MotorcycleMaxDailyFee,
                        [SmallCarType] = PricingInput.SmallCarMaxDailyFee,
                        [LargeCarType] = PricingInput.LargeCarMaxDailyFee
                    },
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
                    ? "Đã cập nhật bảng giá vé."
                    : result?.Message ?? "Không thể cập nhật bảng giá vé.";
            }
            catch (Exception ex)
            {
                ActionSuccess = false;
                ActionMessage = BuildLoadErrorMessage(ex);
            }

            return RedirectToPage(BuildRouteValues());
        }

        public async Task<IActionResult> OnPostCreateTicketAsync(
            string vehiclePlate,
            string vehicleType,
            string? slotId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehiclePlate) || string.IsNullOrWhiteSpace(vehicleType))
                {
                    ActionSuccess = false;
                    ActionMessage = "Vui lòng nhập biển số xe và loại xe.";
                    return RedirectToPage(BuildRouteValues());
                }

                var input = new CreateTicketRequestDto
                {
                    VehiclePlate = vehiclePlate.Trim(),
                    VehicleType = vehicleType,
                    SlotId = string.IsNullOrWhiteSpace(slotId) ? null : slotId.Trim()
                };

                var result = await _ticketService.CreateTicketAsync(input);
                ActionSuccess = result?.Success == true;
                ActionMessage = ActionSuccess
                    ? $"Đã tạo vé {result?.TicketId ?? "mới"}{(string.IsNullOrWhiteSpace(result?.SlotId) ? "" : $" tại vị trí {result.SlotId}")}."
                    : result?.Message ?? "Không thể tạo vé mới.";

                if (ActionSuccess)
                {
                    PageNumber = 1;
                }
            }
            catch (Exception ex)
            {
                ActionSuccess = false;
                ActionMessage = BuildLoadErrorMessage(ex);
            }

            return RedirectToPage(BuildRouteValues());
        }

        public async Task<IActionResult> OnPostUpdateTicketAsync(
            string ticketId,
            string vehiclePlate,
            string vehicleType,
            DateTime checkInTime,
            DateTime? checkOutTime,
            string ticketStatus,
            decimal fee,
            string? slotId)
        {
            try
            {
                var input = new UpdateTicketRequestDto
                {
                    VehiclePlate = vehiclePlate,
                    VehicleType = vehicleType,
                    CheckInTime = checkInTime,
                    CheckOutTime = ticketStatus == "Đang trong bãi" ? null : checkOutTime,
                    Status = ticketStatus,
                    Fee = fee,
                    SlotId = slotId
                };

                ActionSuccess = await _ticketService.UpdateTicketAsync(ticketId, input);
                ActionMessage = ActionSuccess
                    ? $"Đã cập nhật vé {ticketId}."
                    : $"Không thể cập nhật vé {ticketId}.";
            }
            catch (Exception ex)
            {
                ActionSuccess = false;
                ActionMessage = BuildLoadErrorMessage(ex);
            }

            return RedirectToPage(BuildRouteValues());
        }

        public async Task<IActionResult> OnPostDeleteTicketAsync(string deleteTicketId)
        {
            try
            {
                ActionSuccess = await _ticketService.DeleteTicketAsync(deleteTicketId);
                ActionMessage = ActionSuccess
                    ? $"Đã xóa vé {deleteTicketId}."
                    : $"Không thể xóa vé {deleteTicketId}.";
            }
            catch (Exception ex)
            {
                ActionSuccess = false;
                ActionMessage = BuildLoadErrorMessage(ex);
            }

            return RedirectToPage(BuildRouteValues());
        }

        private async Task LoadPricingAsync()
        {
            try
            {
                Pricing = await _pricingService.GetCurrentPricingAsync() ?? CreateDefaultPricing();
            }
            catch (Exception ex)
            {
                LoadErrorMessage ??= BuildLoadErrorMessage(ex);
                Pricing = CreateDefaultPricing();
            }

            PricingInput = new PricingInputModel
            {
                MotorcycleHourlyRate = GetPricingValue(Pricing.HourlyRate, MotorcycleType, 3000m),
                MotorcycleMaxDailyFee = GetPricingValue(Pricing.MaxDailyFee, MotorcycleType, 30000m),
                MotorcycleMonthlyOneMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, MotorcycleType, 1, 150000m),
                MotorcycleMonthlyThreeMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, MotorcycleType, 3, 400000m),
                MotorcycleMonthlySixMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, MotorcycleType, 6, 750000m),
                SmallCarHourlyRate = GetPricingValue(Pricing.HourlyRate, SmallCarType, 5000m),
                SmallCarMaxDailyFee = GetPricingValue(Pricing.MaxDailyFee, SmallCarType, 50000m),
                SmallCarMonthlyOneMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, SmallCarType, 1, 300000m),
                SmallCarMonthlyThreeMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, SmallCarType, 3, 800000m),
                SmallCarMonthlySixMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, SmallCarType, 6, 1500000m),
                LargeCarHourlyRate = GetPricingValue(Pricing.HourlyRate, LargeCarType, 8000m),
                LargeCarMaxDailyFee = GetPricingValue(Pricing.MaxDailyFee, LargeCarType, 80000m),
                LargeCarMonthlyOneMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, LargeCarType, 1, 500000m),
                LargeCarMonthlyThreeMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, LargeCarType, 3, 1300000m),
                LargeCarMonthlySixMonth = GetMonthlyPricingValue(Pricing.MonthlyTicketPrice, LargeCarType, 6, 2500000m)
            };
        }

        private static decimal GetPricingValue(Dictionary<string, decimal> pricing, string vehicleType, decimal fallback)
        {
            if (pricing.TryGetValue(vehicleType, out var value) && value > 0)
                return value;

            var matchedValue = pricing
                .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase))
                .Value;

            return matchedValue > 0 ? matchedValue : fallback;
        }

        private static decimal GetMonthlyPricingValue(
            Dictionary<string, MonthlyPricingDto> pricing,
            string vehicleType,
            int months,
            decimal fallback)
        {
            if (!pricing.TryGetValue(vehicleType, out var monthlyPrice))
            {
                monthlyPrice = pricing
                    .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase))
                    .Value;
            }

            if (monthlyPrice == null)
                return fallback;

            var value = months switch
            {
                1 => monthlyPrice.OneMonth,
                3 => monthlyPrice.ThreeMonth,
                6 => monthlyPrice.SixMonth,
                _ => 0m
            };

            return value > 0 ? value : fallback;
        }

        private static PricingDto CreateDefaultPricing()
        {
            return new PricingDto
            {
                HourlyRate = new Dictionary<string, decimal>
                {
                    [MotorcycleType] = 3000m,
                    [SmallCarType] = 5000m,
                    [LargeCarType] = 8000m
                },
                MaxDailyFee = new Dictionary<string, decimal>
                {
                    [MotorcycleType] = 30000m,
                    [SmallCarType] = 50000m,
                    [LargeCarType] = 80000m
                },
                MonthlyTicketPrice = new Dictionary<string, MonthlyPricingDto>
                {
                    [MotorcycleType] = new() { OneMonth = 150000m, ThreeMonth = 400000m, SixMonth = 750000m },
                    [SmallCarType] = new() { OneMonth = 300000m, ThreeMonth = 800000m, SixMonth = 1500000m },
                    [LargeCarType] = new() { OneMonth = 500000m, ThreeMonth = 1300000m, SixMonth = 2500000m }
                },
                LastUpdatedAt = DateTime.UtcNow
            };
        }

        private object BuildRouteValues()
        {
            return new
            {
                Keyword,
                Status,
                Type,
                FromDate,
                ToDate,
                PageNumber,
                PageSize
            };
        }

        private static string BuildLoadErrorMessage(Exception ex)
        {
            var message = ex.Message;

            if (message.Contains("Connection refused", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
            {
                return "Không tải được dữ liệu vé vì Backend API chưa chạy ở http://localhost:5188.";
            }

            if (message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("401", StringComparison.OrdinalIgnoreCase))
            {
                return "Không tải được dữ liệu vé vì phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            }

            return $"Không tải được dữ liệu vé từ Backend API. Chi tiết: {message}";
        }

        private static string GetStatusClass(string status)
        {
            return status switch
            {
                "Đang trong bãi" => "active",
                "Đã ra" => "paid",
                _ => "expired"
            };
        }
    }

    [Authorize(Roles = "Manager,Admin")]
    public class TicketViewModel
    {
        public string Code { get; set; }
        public string PlateNumber { get; set; }
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; }
        public string SlotId { get; set; }

        public TicketViewModel(
            string code,
            string plateNumber,
            string customerName,
            string type,
            decimal price,
            DateTime checkInTime,
            DateTime checkOutTime,
            string status,
            string statusClass,
            string slotId)
        {
            Code = code;
            PlateNumber = plateNumber;
            CustomerName = customerName;
            Type = type;
            Price = price;
            CheckInTime = checkInTime;
            CheckOutTime = checkOutTime;
            Status = status;
            StatusClass = statusClass;
            SlotId = slotId;
        }
    }
}
