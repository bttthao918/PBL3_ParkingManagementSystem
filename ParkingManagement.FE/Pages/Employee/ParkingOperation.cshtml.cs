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
        private readonly ICustomerApiService _customerApiService;
        private readonly IParkingSlotService _parkingSlotService;
        private readonly IEmployeeMonthlyTicketService _monthlyTicketService;
        private readonly IShiftScheduleService _shiftScheduleService;

        public ParkingOperationModel(
            IParkingOperationService service,
            ITicketService ticketService,
            ICustomerApiService customerApiService,
            IParkingSlotService parkingSlotService,
            IEmployeeMonthlyTicketService monthlyTicketService,
            IShiftScheduleService shiftScheduleService)
        {
            _service = service;
            _ticketService = ticketService;
            _customerApiService = customerApiService;
            _parkingSlotService = parkingSlotService;
            _monthlyTicketService = monthlyTicketService;
            _shiftScheduleService = shiftScheduleService;
        }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "checkin";

        [BindProperty]
        public string? VehiclePlate { get; set; }

        [BindProperty]
        public string? VehicleType { get; set; }

        [BindProperty]
        public string? VehiclePlateOrTicketId { get; set; }

        // Check-in state
        public CheckInValidationResponse? CheckInValidation { get; set; }
        public CheckInResultResponse? CheckInResult { get; set; }
        public List<AvailableSlotDto>? AllSlotsForMap { get; set; }

        // Check-out state
        public CheckOutValidationResponse? CheckOutValidation { get; set; }
        public CheckOutResultResponse? CheckOutResult { get; set; }

        // Danh sách xe đang trong bãi (cho tab check-out)
        public List<EmployeeTicketListDto> ActiveTickets { get; set; } = new();
        public int ActiveCount { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int CheckInToday { get; set; }
        public int CheckOutToday { get; set; }
        public int MonthlyTicketActive { get; set; }
        public int MonthlyTicketExpiringSoon { get; set; }
        public string ShiftLabel { get; set; } = "Chưa có ca";
        public List<EmployeeTicketListDto> RecentTickets { get; set; } = new();

        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Quản lý ra vào";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            await LoadSummaryAsync();
            await LoadDashboardDataAsync();

            if (Tab == "checkout")
            {
                await LoadActiveTicketsAsync();
            }
        }

        // Bước 1 Check-in: Validate biển số
        public async Task<IActionResult> OnPostValidateCheckInAsync(string vehiclePlate, string vehicleType)
        {
            VehiclePlate = vehiclePlate;
            VehicleType = vehicleType;

            SetViewData();
            Tab = "checkin";
            await LoadSummaryAsync();
            await LoadDashboardDataAsync();

            if (string.IsNullOrWhiteSpace(vehiclePlate))
            {
                ActionMessage = "Vui lòng nhập biển số xe.";
                ActionSuccess = false;
                return Page();
            }

            CheckInValidation = await _service.ValidateCheckInAsync(vehiclePlate.Trim().ToUpper(), vehicleType ?? "Xe máy");
            if (CheckInValidation != null)
            {
                AllSlotsForMap = await _customerApiService.GetAvailableSlotsAsync(vehicleType ?? "Xe máy", true);
            }
            return Page();
        }

        // Bước 2 Check-in: Xác nhận
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
            await LoadDashboardDataAsync();
            return Page();
        }

        // Bước 1 Check-out: Validate
        public async Task<IActionResult> OnPostValidateCheckOutAsync(string vehiclePlateOrTicketId)
        {
            VehiclePlateOrTicketId = vehiclePlateOrTicketId;

            SetViewData();
            Tab = "checkout";
            await LoadActiveTicketsAsync();
            await LoadSummaryAsync();
            await LoadDashboardDataAsync();

            if (string.IsNullOrWhiteSpace(vehiclePlateOrTicketId))
            {
                ActionMessage = "Vui lòng nhập biển số xe hoặc mã vé.";
                ActionSuccess = false;
                return Page();
            }

            CheckOutValidation = await _service.ValidateCheckOutAsync(vehiclePlateOrTicketId.Trim().ToUpper());
            return Page();
        }

        // Bước 2 Check-out: Xác nhận
        public async Task<IActionResult> OnPostConfirmCheckOutAsync(string ticketId, decimal fee, string? paymentMethod)
        {
            SetViewData();
            Tab = "checkout";

            CheckOutResult = await _service.ConfirmCheckOutAsync(ticketId, fee, paymentMethod ?? "Tiền mặt");

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
            await LoadDashboardDataAsync();
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
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            var today = DateTime.Today;

            var recentResult = await _ticketService.SearchTicketsAsync(new EmployeeTicketSearchDto
            {
                PageNumber = 1,
                PageSize = 8
            });

            if (recentResult?.Items != null)
            {
                RecentTickets = recentResult.Items;
                CheckInToday = recentResult.Items.Count(t => t.CheckInTime.Date == today);
                CheckOutToday = recentResult.Items.Count(t => t.CheckOutTime?.Date == today);
            }

            var todayResult = await _ticketService.SearchTicketsAsync(new EmployeeTicketSearchDto
            {
                FromDate = today,
                ToDate = today,
                PageNumber = 1,
                PageSize = 500
            });

            if (todayResult?.Items != null)
            {
                CheckInToday = todayResult.Items.Count(t => t.CheckInTime.Date == today);
                CheckOutToday = todayResult.Items.Count(t => t.CheckOutTime?.Date == today);
            }

            var slots = await _parkingSlotService.GetEmployeeSlotsAsync(new EmployeeSlotFilterDto
            {
                PageNumber = 1,
                PageSize = 500
            });

            if (slots != null)
            {
                TotalSlots = slots.TotalItems;
                AvailableSlots = slots.TotalEmpty;
            }

            var monthly = await _monthlyTicketService.GetAllAsync(null, null, null, 1, 1);
            if (monthly != null)
            {
                MonthlyTicketActive = monthly.Summary.Active;
                MonthlyTicketExpiringSoon = monthly.Summary.ExpiringSoon;
            }

            var shift = await _shiftScheduleService.GetMyTodayShiftAsync();
            if (shift?.HasShift == true && shift.Shift != null)
            {
                ShiftLabel = $"{shift.Shift.StartTime} - {shift.Shift.EndTime}";
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
