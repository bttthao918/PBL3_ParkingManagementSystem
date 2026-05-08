using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ParkingManagement.FE.Pages.Shared.ParkingSlotManagement
{
        public class ParkingSlotManagementModel : PageModel
        {
            public List<ParkingSlotViewModel> Slots { get; set; } = new();

            public string SlotsJson { get; set; } = "[]";

            public int AvailableCount => Slots.Count(x => x.Status == "Trống");
            public int UsingCount => Slots.Count(x => x.Status == "Đang sử dụng");
            public int BookedCount => Slots.Count(x => x.Status == "Đã đặt");

            public void OnGet()
            {
                Slots = GenerateSlots();

                SlotsJson = JsonSerializer.Serialize(Slots, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            private List<ParkingSlotViewModel> GenerateSlots()
            {
                var slots = new List<ParkingSlotViewModel>();

                for (int i = 1; i <= 50; i++)
                {
                    var id = $"A{i:00}";

                    slots.Add(new ParkingSlotViewModel
                    {
                        SlotId = id,
                        Location = $"Khu A - Ô {i:00}",
                        VehicleType = "Xe máy",
                        Status = id switch
                        {
                            "A01" => "Đang sử dụng",
                            "A04" => "Đã đặt",
                            _ => "Trống"
                        },
                        LastUpdated = id switch
                        {
                            "A01" => new DateTime(2026, 5, 10, 8, 23, 0),
                            "A02" => new DateTime(2026, 5, 9, 17, 30, 0),
                            "A03" => new DateTime(2026, 5, 8, 12, 0, 0),
                            "A04" => new DateTime(2026, 5, 10, 10, 0, 0),
                            _ => new DateTime(2026, 5, 10, 0, 0, 0)
                        }
                    });
                }

                for (int i = 1; i <= 50; i++)
                {
                    var id = $"B{i:00}";

                    slots.Add(new ParkingSlotViewModel
                    {
                        SlotId = id,
                        Location = $"Khu B - Ô {i:00}",
                        VehicleType = "Ô tô nhỏ",
                        Status = id switch
                        {
                            "B01" => "Đang sử dụng",
                            _ => "Trống"
                        },
                        LastUpdated = id switch
                        {
                            "B01" => new DateTime(2026, 5, 10, 9, 5, 0),
                            _ => new DateTime(2026, 5, 10, 0, 0, 0)
                        }
                    });
                }

                for (int i = 1; i <= 20; i++)
                {
                    var id = $"C{i:00}";

                    slots.Add(new ParkingSlotViewModel
                    {
                        SlotId = id,
                        Location = $"Khu C - Ô {i:00}",
                        VehicleType = "Ô tô lớn",
                        Status = id switch
                        {
                            "C01" => "Đang sử dụng",
                            _ => "Trống"
                        },
                        LastUpdated = id switch
                        {
                            "C01" => new DateTime(2026, 5, 1, 0, 0, 0),
                            _ => new DateTime(2026, 5, 10, 0, 0, 0)
                        }
                    });
                }

                return slots;
            }
        }

        public class ParkingSlotViewModel
        {
            public string SlotId { get; set; } = "";
            public string Location { get; set; } = "";
            public string VehicleType { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime LastUpdated { get; set; }

            public string LastUpdatedText => LastUpdated.ToString("HH:mm");
            public string LastUpdatedFull => LastUpdated.ToString("dd/MM/yyyy HH:mm");
        }
}
