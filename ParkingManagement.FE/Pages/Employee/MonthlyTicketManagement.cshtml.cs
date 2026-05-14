using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class MonthlyTicketManagementModel : PageModel
    {
        private readonly IEmployeeMonthlyTicketService _service;

        public MonthlyTicketManagementModel(IEmployeeMonthlyTicketService service)
        {
            _service = service;
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

        // Messages
        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Quản lý vé tháng";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

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
            if (pricing != null)
            {
                Pricing = pricing;
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string vehiclePlate, string vehicleType, int durationMonths,
            string? customerId, string? customerPhone, string? paymentMethod)
        {
            var dto = new CreateEmployeeMonthlyTicketRequest
            {
                VehiclePlate = vehiclePlate?.Trim().ToUpper() ?? "",
                VehicleType = vehicleType ?? "Xe máy",
                DurationMonths = durationMonths,
                CustomerId = customerId,
                CustomerPhone = customerPhone,
                PaymentMethod = paymentMethod ?? "Tiền mặt"
            };

            var result = await _service.CreateAsync(dto);
            ActionSuccess = result?.Success ?? false;
            ActionMessage = result?.Message ?? "Đăng ký vé tháng thất bại.";

            return RedirectToPage(new { Search, StatusFilter, VehicleTypeFilter, PageNumber, PageSize });
        }

        public async Task<IActionResult> OnPostRenewAsync(string monthlyTicketId, int monthsToAdd, string? paymentMethod)
        {
            var dto = new RenewEmployeeMonthlyTicketRequest
            {
                MonthsToAdd = monthsToAdd,
                PaymentMethod = paymentMethod ?? "Tiền mặt"
            };

            var result = await _service.RenewAsync(monthlyTicketId, dto);
            ActionSuccess = result?.Success ?? false;
            ActionMessage = result?.Message ?? "Gia hạn vé tháng thất bại.";

            return RedirectToPage(new { Search, StatusFilter, VehicleTypeFilter, PageNumber, PageSize });
        }

        public async Task<IActionResult> OnPostCancelAsync(string monthlyTicketId)
        {
            var result = await _service.CancelAsync(monthlyTicketId);
            ActionSuccess = result?.Success ?? false;
            ActionMessage = result?.Message ?? "Hủy vé tháng thất bại.";

            return RedirectToPage(new { Search, StatusFilter, VehicleTypeFilter, PageNumber, PageSize });
        }
    }
}
