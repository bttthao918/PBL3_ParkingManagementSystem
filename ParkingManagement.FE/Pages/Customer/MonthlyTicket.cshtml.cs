using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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
    private readonly ILogger<MonthlyTicketModel> _logger;

    public MonthlyTicketModel(ICustomerApiService customerApiService, ILogger<MonthlyTicketModel> logger)
    {
        _customerApiService = customerApiService;
        _logger = logger;
    }

    public string UserName { get; set; } = "Customer";
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public List<MonthlyPlanVm> Plans { get; set; } = new();
    public CustomerMonthlyTicketDto? CurrentTicket { get; set; }
    public List<CustomerMonthlyTicketDto> TicketHistories { get; set; } = new();

    [BindProperty]
    public RegisterMonthlyTicketInput RegisterInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["Success"] as string;
        ErrorMessage = TempData["Error"] as string;
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

        TempData[result.Success ? "Success" : "Error"] = result.Message;
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

    public bool IsActive(CustomerMonthlyTicketDto ticket)
    {
        var status = NormalizeStatus(ticket.Status);
        return ticket.DaysRemaining >= 0 && (status.Contains("hoat dong") || status.Contains("active"));
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

        return "Đã hết hạn";
    }

    private async Task LoadDataAsync()
    {
        ViewData["Title"] = "Vé tháng";
        ViewData["Role"] = "Khách hàng";

        var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
        UserName = fallbackName;

        try
        {
            var profileTask = _customerApiService.GetProfileAsync();
            var ticketsTask = _customerApiService.GetMonthlyTicketsAsync();

            await Task.WhenAll(profileTask, ticketsTask);

            var profile = await profileTask;
            var tickets = await ticketsTask ?? new ListCustomerMonthlyTicketDto();
            UserName = string.IsNullOrWhiteSpace(profile?.FullName) ? fallbackName : profile.FullName;
            ViewData["UserName"] = UserName;

            Plans = BuildPlans();
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
            Plans = BuildPlans();
        }
    }

    private static List<MonthlyPlanVm> BuildPlans() =>
    [
        new MonthlyPlanVm
        {
            VehicleType = "Xe máy",
            PackageType = "1 tháng",
            Name = "Vé xe máy",
            Price = 150000,
            Icon = "fa-solid fa-motorcycle",
            IconClass = "blue",
            IsSelected = true
        },
        new MonthlyPlanVm
        {
            VehicleType = "Ô tô nhỏ",
            PackageType = "1 tháng",
            Name = "Vé ô tô nhỏ",
            Price = 350000,
            Icon = "fa-solid fa-car",
            IconClass = "green"
        },
        new MonthlyPlanVm
        {
            VehicleType = "Ô tô lớn",
            PackageType = "1 tháng",
            Name = "Vé ô tô lớn",
            Price = 600000,
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
}

public class RegisterMonthlyTicketInput
{
    [Required(ErrorMessage = "Vui lòng nhập biển số xe.")]
    [RegularExpression(@"^\d{2}[A-Za-z]-\d{3}\.\d{2}$", ErrorMessage = "Biển số cần đúng định dạng 43A-123.45.")]
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
