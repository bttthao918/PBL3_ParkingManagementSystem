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

        TempData["Success"] = result.Message;
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

            Plans = BuildPlans(pricing);
            TicketHistories = tickets.Items
                .OrderByDescending(IsActive)
                .ThenByDescending(x => x.EndDate)
                .ToList();
            CurrentTicket = TicketHistories.FirstOrDefault(IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load monthly ticket page data");
            ErrorMessage ??= "Chưa tải được dữ liệu vé tháng từ BE. Kiểm tra BE đang chạy và tài khoản còn phiên đăng nhập.";
            ViewData["UserName"] = fallbackName;
            Plans = BuildPlans(PricingDisplayDefaults.CreateDefaultPricing());
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

    private static List<MonthlyPlanVm> BuildPlans(PricingDto pricing) =>
    [
        new MonthlyPlanVm
        {
            VehicleType = PricingDisplayDefaults.Motorcycle,
            PackageType = "1 tháng",
            Name = "Vé xe máy",
            Price = PricingDisplayDefaults.GetMonthlyTicketPrice(pricing, PricingDisplayDefaults.Motorcycle, 1),
            Icon = "fa-solid fa-motorcycle",
            IconClass = "blue",
            IsSelected = true
        },
        new MonthlyPlanVm
        {
            VehicleType = PricingDisplayDefaults.SmallCar,
            PackageType = "1 tháng",
            Name = "Vé ô tô nhỏ",
            Price = PricingDisplayDefaults.GetMonthlyTicketPrice(pricing, PricingDisplayDefaults.SmallCar, 1),
            Icon = "fa-solid fa-car",
            IconClass = "green"
        },
        new MonthlyPlanVm
        {
            VehicleType = PricingDisplayDefaults.LargeCar,
            PackageType = "1 tháng",
            Name = "Vé ô tô lớn",
            Price = PricingDisplayDefaults.GetMonthlyTicketPrice(pricing, PricingDisplayDefaults.LargeCar, 1),
            Icon = "fa-solid fa-van-shuttle",
            IconClass = "purple"
        }
    ];

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

    public string Name { get; set; } = "";
    public decimal Price { get; set; }
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

    public string? QrImageUrl => string.IsNullOrWhiteSpace(QrCode)
        ? null
        : $"https://api.qrserver.com/v1/create-qr-code/?size=280x280&margin=12&data={Uri.EscapeDataString(QrCode)}";
}
