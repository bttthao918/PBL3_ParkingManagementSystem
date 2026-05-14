using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ParkingManagement.FE.Pages.Shared.ParkingSlotManagement
{
        public class ParkingSlotManagementModel : PageModel
        {
            private readonly Services.IParkingSlotService _parkingSlotService;

            public ParkingSlotManagementModel(Services.IParkingSlotService parkingSlotService)
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
                        LastUpdated = s.OccupiedSince ?? DateTime.Now
                    }).ToList();
                }

                SlotsJson = JsonSerializer.Serialize(Slots, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
        }

        public class ParkingSlotViewModel
        {
            public string SlotId { get; set; } = "";
            public string Location { get; set; } = "";
            public string VehicleType { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime LastUpdated { get; set; }
            public string? CurrentOccupant { get; set; }

            public string LastUpdatedText => LastUpdated.ToString("HH:mm");
            public string LastUpdatedFull => LastUpdated.ToString("dd/MM/yyyy HH:mm");
        }
}
