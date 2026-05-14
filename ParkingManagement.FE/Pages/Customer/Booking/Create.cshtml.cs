using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.FE.Pages.Customer.Booking
{
    [Authorize(Roles = "Customer")]
    public class CreateModel : PageModel
    {
        private readonly IReservationService _reservationService;
        private readonly ICustomerApiService _customerApiService;

        public CreateModel(IReservationService reservationService, ICustomerApiService customerApiService)
        {
            _reservationService = reservationService;
            _customerApiService = customerApiService;
        }

        [BindProperty]
        public CreateReservationInput Input { get; set; } = new();

        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
        public CustomerProfileDto? Profile { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Đặt chỗ mới";
            ViewData["Role"] = "Khách hàng";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Khách hàng";

            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["Title"] = "Đặt chỗ mới";
            ViewData["Role"] = "Khách hàng";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Khách hàng";

            await LoadDataAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.ExpectedTime <= DateTime.Now)
            {
                ErrorMessage = "Thời gian dự kiến phải trong tương lai.";
                return Page();
            }

            var dto = new CreateReservationDto
            {
                VehiclePlate = Input.VehiclePlate.Trim().ToUpper(),
                VehicleType = Input.VehicleType,
                PreferredSlotId = Input.SlotId,
                ExpectedTime = Input.ExpectedTime
            };

            var result = await _reservationService.CreateAsync(dto);
            if (result != null)
            {
                TempData["SuccessMessage"] = $"Đặt chỗ thành công! Mã đặt chỗ: {result.ReservationId}";
                return RedirectToPage("./Index");
            }
            else
            {
                ErrorMessage = "Không thể tạo đặt chỗ. Vui lòng thử lại.";
                return Page();
            }
        }

        private async Task LoadDataAsync()
        {
            Profile = await _customerApiService.GetProfileAsync();
            AvailableSlots = await _reservationService.GetAvailableSlotsAsync(Input.VehicleType) ?? new();
        }
    }

    public class CreateReservationInput
    {
        [Required(ErrorMessage = "Vui lòng nhập biển số xe")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{3,5}\.[0-9]{2}$|^[0-9]{2}[A-Z]{1,2}-[0-9]{5}$", 
            ErrorMessage = "Biển số xe không đúng định dạng (VD: 43A-123.45)")]
        public string VehiclePlate { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn loại xe")]
        public string VehicleType { get; set; } = "Xe máy";

        public string? SlotId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian dự kiến")]
        public DateTime ExpectedTime { get; set; } = DateTime.Now.AddHours(1);
    }
}

