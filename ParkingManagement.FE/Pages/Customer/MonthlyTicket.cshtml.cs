using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer;

[Authorize(Roles = "Customer")]
public class MonthlyTicketModel : PageModel
{
    private const decimal SilverThreshold = 2_000_000m;
    private const decimal GoldThreshold = 5_000_000m;
    private const decimal DiamondThreshold = 10_000_000m;

    private readonly ICustomerApiService _customerApiService;
    private readonly IPricingService _pricingService;
    private readonly ILogger<MonthlyTicketModel> _logger;

    public MonthlyTicketModel(
        ICustomerApiService customerApiService,
        IPricingService pricingService,
        ILogger<MonthlyTicketModel> logger)
    {
        _customerApiService = customerApiService;
        _pricingService = pricingService;
        _logger = logger;
    }

    public string UserName { get; set; } = "Customer";
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string VipLevel { get; set; } = "Thành viên";
    public int DiscountPercent { get; set; }
    public int VipProgress { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal? AmountToNextLevel { get; set; }
    public MonthlyTicketPaymentVm? PendingPayment { get; set; }
    public List<MonthlyPlanVm> Plans { get; set; } = new();
    public CustomerMonthlyTicketDto? CurrentTicket { get; set; }
    public List<CustomerMonthlyTicketDto> TicketHistories { get; set; } = new();

    [BindProperty]
    public RegisterMonthlyTicketInput RegisterInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["Success"] as string;
        ErrorMessage = TempData["Error"] as string;
        PendingPayment = ReadPendingPayment();
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var result = await _customerApiService.RegisterMonthlyTicketAsync(new RegisterMonthlyTicketRequestDto
        {
            VehiclePlate = RegisterInput.VehiclePlate,
            VehicleType = RegisterInput.VehicleType,
            PackageType = RegisterInput.PackageType,
            PaymentMethod = RegisterInput.PaymentMethod
        });

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToPage();
        }

        if (StorePendingPayment(result.Data, result.Message))
        {
            return RedirectToPage();
        }

        if (result.Data?.Data != null && IsActive(result.Data.Data))
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = "Chưa nhận được mã QR thanh toán từ BE. Vui lòng thử lại sau ít phút.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRenewAsync(string ticketId, string packageType, string paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            TempData["Error"] = "Không xác định được vé cần gia hạn.";
            return RedirectToPage();
        }

        var result = await _customerApiService.RenewMonthlyTicketAsync(ticketId, new RenewMonthlyTicketRequestDto
        {
            PackageType = packageType,
            PaymentMethod = paymentMethod
        });

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostShowPaymentAsync(string ticketId)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            TempData["Error"] = "Không xác định được vé cần in lại QR.";
            return RedirectToPage();
        }

        var result = await _customerApiService.CreateMonthlyTicketPaymentLinkAsync(ticketId);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToPage();
        }

        if (StorePendingPayment(result.Data, result.Message))
        {
            return RedirectToPage();
        }

        TempData["Success"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReorderAsync(string vehiclePlate, string? vehicleType, string packageType)
    {
        if (string.IsNullOrWhiteSpace(vehiclePlate))
        {
            TempData["Error"] = "Không xác định được biển số cần đặt lại.";
            return RedirectToPage();
        }

        var result = await _customerApiService.RegisterMonthlyTicketAsync(new RegisterMonthlyTicketRequestDto
        {
            VehiclePlate = vehiclePlate,
            VehicleType = vehicleType,
            PackageType = string.IsNullOrWhiteSpace(packageType) ? "1 tháng" : packageType,
            PaymentMethod = "Chuyển khoản"
        });

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToPage();
        }

        if (StorePendingPayment(result.Data, result.Message))
        {
            return RedirectToPage();
        }

        TempData["Success"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync(string ticketId)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            TempData["Error"] = "Không xác định được vé cần hủy.";
            return RedirectToPage();
        }

        var result = await _customerApiService.CancelMonthlyTicketAsync(ticketId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConfirmPaymentAsync(string monthlyTicketId)
    {
        ApiActionResult<BasicApiResponseDto> result;
        if (string.IsNullOrWhiteSpace(monthlyTicketId))
        {
            result = new ApiActionResult<BasicApiResponseDto>
            {
                Success = false,
                Message = "Không xác định được vé cần kiểm tra thanh toán."
            };
        }
        else
        {
            result = await _customerApiService.ConfirmMonthlyTicketPaymentAsync(monthlyTicketId);
        }

        if (IsAjaxRequest())
        {
            return new JsonResult(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToPage();
    }

    public bool IsActive(CustomerMonthlyTicketDto ticket)
    {
        var status = NormalizeStatus(ticket.Status);
        return ticket.DaysRemaining >= 0 && (status.Contains("hoat dong") || status.Contains("active"));
    }

    public bool IsPending(CustomerMonthlyTicketDto ticket)
    {
        var status = NormalizeStatus(ticket.Status);
        return status.Contains("cho thanh toan") || status.Contains("pending");
    }

    public string GetStatusText(CustomerMonthlyTicketDto ticket)
    {
        if (IsActive(ticket))
        {
            return "Đang hoạt động";
        }

        var status = NormalizeStatus(ticket.Status);
        if (status.Contains("huy") || status.Contains("cancel"))
        {
            return "Đã hủy";
        }

        if (status.Contains("cho thanh toan") || status.Contains("pending"))
        {
            return "Chờ thanh toán";
        }

        return "Đã hết hạn";
    }

    private async Task LoadDataAsync()
    {
        ViewData["Title"] = "Quản lý vé tháng";
        ViewData["Role"] = "Khách hàng";

        var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
        UserName = fallbackName;

        try
        {
            var profileTask = _customerApiService.GetProfileAsync();
            var ticketsTask = _customerApiService.GetMonthlyTicketsAsync();
            var pricingTask = LoadPricingAsync();

            await Task.WhenAll(profileTask, ticketsTask, pricingTask);

            var profile = await profileTask;
            var tickets = await ticketsTask ?? new ListCustomerMonthlyTicketDto();
            var pricing = await pricingTask;
            UserName = string.IsNullOrWhiteSpace(profile?.FullName) ? fallbackName : profile.FullName;
            ViewData["UserName"] = UserName;

            TicketHistories = tickets.Items
                .OrderByDescending(IsActive)
                .ThenByDescending(x => x.EndDate)
                .ToList();
            CurrentTicket = TicketHistories.FirstOrDefault(IsActive);

            ApplyVipSnapshot(profile, TicketHistories);
            Plans = BuildPlans(pricing, DiscountPercent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load monthly ticket page data");
            ErrorMessage ??= "Chưa tải được dữ liệu vé tháng từ BE. Kiểm tra BE đang chạy và tài khoản còn phiên đăng nhập.";
            ViewData["UserName"] = fallbackName;
            Plans = BuildPlans(PricingDisplayDefaults.CreateDefaultPricing(), DiscountPercent);
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

    private static List<MonthlyPlanVm> BuildPlans(PricingDto pricing, int vipDiscountPercent)
    {
        var vehiclePlans = new[]
        {
            new
            {
                VehicleType = PricingDisplayDefaults.Motorcycle,
                Name = "Vé xe máy",
                SubTitle = "Xe máy, scooter",
                Icon = "fa-solid fa-motorcycle",
                IconClass = "blue",
                IsSelected = false
            },
            new
            {
                VehicleType = PricingDisplayDefaults.SmallCar,
                Name = "Vé ô tô nhỏ",
                SubTitle = "Xe dưới 7 chỗ",
                Icon = "fa-solid fa-car",
                IconClass = "green",
                IsSelected = true
            },
            new
            {
                VehicleType = PricingDisplayDefaults.LargeCar,
                Name = "Vé ô tô lớn",
                SubTitle = "Xe từ 7 chỗ trở lên",
                Icon = "fa-solid fa-van-shuttle",
                IconClass = "purple",
                IsSelected = false
            }
        };

        var durations = new[] { 1, 3, 6 };

        return vehiclePlans
            .SelectMany(plan => durations.Select(months =>
            {
                var originalPrice = PricingDisplayDefaults.GetMonthlyTicketPrice(pricing, plan.VehicleType, months);
                var originalOneMonthPrice = PricingDisplayDefaults.GetMonthlyTicketPrice(pricing, plan.VehicleType, 1);

                return new MonthlyPlanVm
                {
                    VehicleType = plan.VehicleType,
                    PackageType = $"{months} tháng",
                    Months = months,
                    Name = plan.Name,
                    SubTitle = plan.SubTitle,
                    OriginalPrice = originalPrice,
                    Price = ApplyVipDiscount(originalPrice, vipDiscountPercent),
                    OneMonthPrice = ApplyVipDiscount(originalOneMonthPrice, vipDiscountPercent),
                    VipDiscountPercent = vipDiscountPercent,
                    Icon = plan.Icon,
                    IconClass = plan.IconClass,
                    IsSelected = plan.IsSelected
                };
            }))
            .ToList();
    }

    private void ApplyVipSnapshot(CustomerProfileDto? profile, List<CustomerMonthlyTicketDto> monthlyTickets)
    {
        var paidMonthlyTotal = monthlyTickets
            .Where(IsPaidMonthlyTicketForVip)
            .Sum(ticket => ticket.TotalFee);
        TotalSpent = Math.Max(profile?.TotalSpent ?? 0, paidMonthlyTotal);
        VipLevel = NormalizeVipLevel(profile?.VipLevel ?? DetermineVipLevel(TotalSpent));
        DiscountPercent = profile?.DiscountPercent ?? GetDiscountPercent(VipLevel);
        VipProgress = Math.Clamp(profile?.VipProgress ?? CalculateVipProgress(TotalSpent), 0, 100);
        AmountToNextLevel = profile?.AmountToNextLevel ?? CalculateAmountToNextLevel(TotalSpent);

        if (profile == null || TotalSpent > profile.TotalSpent)
        {
            VipLevel = DetermineVipLevel(TotalSpent);
            DiscountPercent = GetDiscountPercent(VipLevel);
            VipProgress = CalculateVipProgress(TotalSpent);
            AmountToNextLevel = CalculateAmountToNextLevel(TotalSpent);
        }
    }

    private static decimal ApplyVipDiscount(decimal amount, int discountPercent)
    {
        if (amount <= 0 || discountPercent <= 0)
        {
            return amount;
        }

        return amount - (amount * discountPercent / 100);
    }

    private static bool IsPaidMonthlyTicketForVip(CustomerMonthlyTicketDto ticket)
    {
        if (ticket.TotalFee <= 0)
        {
            return false;
        }

        var status = NormalizeStatus(ticket.Status);
        return status.Contains("hoat dong")
            || status.Contains("active")
            || status.Contains("het han")
            || status.Contains("expired")
            || status.Contains("da huy")
            || status.Contains("cancel");
    }

    private static string NormalizeVipLevel(string? vipLevel)
    {
        if (string.IsNullOrWhiteSpace(vipLevel))
        {
            return "Thành viên";
        }

        return vipLevel.Trim().ToLowerInvariant() switch
        {
            "member" or "normal" or "thanh vien" or "thành viên" => "Thành viên",
            "silver" or "bac" or "bạc" => "Bạc",
            "gold" or "vang" or "vàng" => "Vàng",
            "platinum" or "diamond" or "kim cuong" or "kim cương" => "Kim Cương",
            _ => vipLevel.Trim()
        };
    }

    private static string DetermineVipLevel(decimal totalSpent)
    {
        if (totalSpent >= DiamondThreshold)
        {
            return "Kim Cương";
        }

        if (totalSpent >= GoldThreshold)
        {
            return "Vàng";
        }

        return totalSpent >= SilverThreshold ? "Bạc" : "Thành viên";
    }

    private static int GetDiscountPercent(string vipLevel)
    {
        return NormalizeVipLevel(vipLevel) switch
        {
            "Bạc" => 5,
            "Vàng" => 10,
            "Kim Cương" => 15,
            _ => 0
        };
    }

    private static int CalculateVipProgress(decimal totalSpent)
    {
        var (start, target) = totalSpent switch
        {
            < SilverThreshold => (0m, SilverThreshold),
            < GoldThreshold => (SilverThreshold, GoldThreshold),
            < DiamondThreshold => (GoldThreshold, DiamondThreshold),
            _ => (DiamondThreshold, DiamondThreshold)
        };

        return target <= start
            ? 100
            : Math.Clamp((int)Math.Round((totalSpent - start) / (target - start) * 100), 0, 100);
    }

    private static decimal? CalculateAmountToNextLevel(decimal totalSpent)
    {
        if (totalSpent < SilverThreshold)
        {
            return SilverThreshold - totalSpent;
        }

        if (totalSpent < GoldThreshold)
        {
            return GoldThreshold - totalSpent;
        }

        if (totalSpent < DiamondThreshold)
        {
            return DiamondThreshold - totalSpent;
        }

        return null;
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "";
        }

        return status.Trim().ToLowerInvariant()
            .Replace("đ", "d")
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
            .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");
    }

    private MonthlyTicketPaymentVm? ReadPendingPayment()
    {
        var json = TempData["MonthlyTicketPayment"] as string;
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<MonthlyTicketPaymentVm>(json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not read pending monthly ticket payment data");
            return null;
        }
    }

    private bool IsAjaxRequest() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private bool StorePendingPayment(RegisterMonthlyTicketResponseDto? payment, string? message)
    {
        if (string.IsNullOrWhiteSpace(payment?.CheckoutUrl) &&
            string.IsNullOrWhiteSpace(payment?.QrCode))
        {
            return false;
        }

        TempData["Success"] = message ?? payment.Message;
        TempData["MonthlyTicketPayment"] = JsonSerializer.Serialize(new MonthlyTicketPaymentVm
        {
            MonthlyTicketId = payment.Data?.MonthlyTicketId,
            OrderCode = payment.OrderCode,
            Amount = payment.Fee,
            CheckoutUrl = payment.CheckoutUrl,
            QrCode = payment.QrCode
        });

        return true;
    }
}

public class RegisterMonthlyTicketInput
{
    [Required(ErrorMessage = "Vui lòng nhập biển số xe.")]
    [RegularExpression(@"^\d{2}-?[A-Za-z]\d?-?\d{3}\.\d{2}$", ErrorMessage = "Biển số cần đúng định dạng 43A-123.45 hoặc 43D1-256.31.")]
    public string VehiclePlate { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn loại xe.")]
    public string VehicleType { get; set; } = "Xe máy";

    [Required(ErrorMessage = "Vui lòng chọn gói vé.")]
    public string PackageType { get; set; } = "1 tháng";

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
    public string PaymentMethod { get; set; } = "Chuyển khoản";
}

public class MonthlyPlanVm
{
    public string VehicleType { get; set; } = "";

    public string PackageType { get; set; } = "";

    public int Months { get; set; } = 1;
    public string Name { get; set; } = "";
    public string SubTitle { get; set; } = "";
    public decimal OriginalPrice { get; set; }
    public decimal Price { get; set; }
    public decimal OneMonthPrice { get; set; }
    public int VipDiscountPercent { get; set; }
    public decimal VipDiscountAmount => Math.Max(0, OriginalPrice - Price);
    public string? Discount { get; set; }
    public string Icon { get; set; } = "";
    public string IconClass { get; set; } = "";
    public bool IsSelected { get; set; }
}

public class MonthlyTicketPaymentVm
{
    public string? MonthlyTicketId { get; set; }
    public long? OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrCode { get; set; }

    public string? QrImageUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(QrCode))
            {
                return null;
            }

            if (QrCode.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                QrCode.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                return QrCode;
            }

            return $"https://api.qrserver.com/v1/create-qr-code/?size=280x280&margin=12&data={Uri.EscapeDataString(QrCode)}";
        }
    }
}
