using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;
using ParkingManagement.FE.Pages.Shared.ParkingSlotManagement;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class ParkingSlotManagementModel : PageModel
    {
        private readonly IParkingSlotService _parkingSlotService;

        public ParkingSlotManagementModel(IParkingSlotService parkingSlotService)
        {
            _parkingSlotService = parkingSlotService;
        }

        public List<ParkingSlotViewModel> Slots { get; set; } = new();
        public string SlotsJson { get; set; } = "[]";

        public int AvailableCount => Slots.Count(x => x.Status == "Trống");
        public int UsingCount => Slots.Count(x => x.Status == "Đang sử dụng");
        public int BookedCount => Slots.Count(x => x.Status == "Đã đặt");

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Quản lý chỗ đỗ";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            var result = await _parkingSlotService.GetEmployeeSlotsAsync(new Models.EmployeeSlotFilterDto
            {
                PageNumber = 1,
                PageSize = 200
            });

            if (result != null && result.Items != null && result.Items.Any())
            {
                Slots = result.Items.Select(s => new ParkingSlotViewModel
                {
                    SlotId = s.SlotId,
                    Location = s.Location,
                    VehicleType = s.VehicleType,
                    Status = s.Status,
                    LastUpdated = s.OccupiedSince ?? DateTime.Now,
                    CurrentOccupant = s.CurrentOccupant
                }).ToList();
            }

            SlotsJson = JsonSerializer.Serialize(Slots, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
