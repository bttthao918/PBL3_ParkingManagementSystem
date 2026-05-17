using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class ParkingOperationModel : PageModel
    {
        private readonly IParkingOperationService _service;
        private readonly ITicketService _ticketService;

        public ParkingOperationModel(IParkingOperationService service, ITicketService ticketService)
        {
            _service = service;
            _ticketService = ticketService;
        }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "checkin";

        public CheckInValidationResponse? CheckInValidation { get; set; }
        public CheckInResultResponse? CheckInResult { get; set; }
        public CheckOutValidationResponse? CheckOutValidation { get; set; }
        public CheckOutResultResponse? CheckOutResult { get; set; }

        public List<EmployeeTicketListDto> ActiveTickets { get; set; } = new();
        public int ActiveCount { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }

        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        public async Task OnGetAsync()
        {
            SetViewData();
            await LoadSummaryAsync();

            if (Tab == "checkout")
                await LoadActiveTicketsAsync();
        }

        public async Task<IActionResult> OnPostValidateCheckInAsync(string vehiclePlate, string vehicleType)
        {
            SetViewData();
            Tab = "checkin";
            await LoadSummaryAsync();

            if (string.IsNullOrWhiteSpace(vehiclePlate))
            {
                ActionMessage = "Vui lòng nhập biển số xe.";
                ActionSuccess = false;
                return Page();
            }

            CheckInValidation = await _service.ValidateCheckInAsync(vehiclePlate.Trim().ToUpper(), vehicleType ?? "Xe máy");
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmCheckInAsync(string vehiclePlate, string vehicleType, string slotId, string? customerId)
        {
            SetViewData();
            Tab = "checkin";

            CheckInResult = await _service.ConfirmCheckInAsync(vehiclePlate.Trim().ToUpper(), vehicleType, slotId, customerId);

            if (CheckInResult?.Success == true)
            {
                ActionSuccess = true;
                ActionMessage = $"✓ Check-in thành công! Vé: {CheckInResult.TicketId} — Chỗ: {CheckInResult.SlotId}";
                return RedirectToPage(new { Tab = "checkin" });
            }

            ActionMessage = CheckInResult?.Message ?? "Check-in thất bại.";
            ActionSuccess = false;
            await LoadSummaryAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostValidateCheckOutAsync(string vehiclePlateOrTicketId)
        {
            SetViewData();
            Tab = "checkout";
            await LoadActiveTicketsAsync();
            await LoadSummaryAsync();

            if (string.IsNullOrWhiteSpace(vehiclePlateOrTicketId))
            {
                ActionMessage = "Vui lòng nhập biển số xe hoặc mã vé.";
                ActionSuccess = false;
                return Page();
            }

            CheckOutValidation = await _service.ValidateCheckOutAsync(vehiclePlateOrTicketId.Trim().ToUpper());
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmCheckOutAsync(
            string ticketId,
            decimal fee,
            string? paymentMethod,
            bool paymentReceivedConfirmed,
            string? bankTransferRef)
        {
            SetViewData();
            Tab = "checkout";

            CheckOutResult = await _service.ConfirmCheckOutAsync(
                ticketId,
                fee,
                paymentMethod ?? "Cash",
                paymentReceivedConfirmed,
                bankTransferRef);

            if (CheckOutResult?.Success == true)
            {
                ActionSuccess = true;
                ActionMessage = CheckOutResult.IsFree
                    ? $"✓ Check-out thành công! Vé: {CheckOutResult.TicketId} — Miễn phí (vé tháng)"
                    : $"✓ Check-out thành công! Vé: {CheckOutResult.TicketId} — Thu: {CheckOutResult.Fee:N0}đ";
                return RedirectToPage(new { Tab = "checkout" });
            }

            ActionMessage = CheckOutResult?.Message ?? "Check-out thất bại.";
            ActionSuccess = false;
            await LoadActiveTicketsAsync();
            await LoadSummaryAsync();
            return Page();
        }

        private void SetViewData()
        {
            ViewData["Title"] = "Quản lý ra vào";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";
        }

        private async Task LoadSummaryAsync()
        {
            var summary = await _ticketService.GetTicketSummaryAsync();
            if (summary != null)
            {
                ActiveCount = summary.ActiveTickets;
                TotalSlots = summary.TotalTickets;
            }
        }

        private async Task LoadActiveTicketsAsync()
        {
            var result = await _ticketService.SearchTicketsAsync(new EmployeeTicketSearchDto
            {
                Status = "Đang trong bãi",
                PageNumber = 1,
                PageSize = 50
            });

            if (result?.Items != null)
            {
                ActiveTickets = result.Items;
                ActiveCount = result.TotalItems;
            }
        }
    }
}
