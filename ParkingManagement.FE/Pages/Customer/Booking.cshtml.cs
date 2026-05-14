using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Customer
{

    public class BookingModel : PageModel
    {
        public string UserName { get; set; } = "Nguyễn Văn Nam";

        public int TotalBooking { get; set; } = 128;
        public int ActiveBooking { get; set; } = 32;
        public int CompletedBooking { get; set; } = 85;
        public int CancelledBooking { get; set; } = 11;

        public List<BookingVm> Bookings { get; set; } = new();
        public List<ParkingSlotVm> ParkingSlots { get; set; } = new();

        public void OnGet()
        {
            Bookings =
            [
                new()
            {
                Id = 1,
                Code = "BK250516-0001",
                ParkingName = "Central Park",
                Position = "Tầng B2 - Khu B - Ô số 12",
                VehiclePlate = "59C1-123.45",
                VehicleType = "Xe máy",
                VehicleClass = "green",
                Icon = "fa-solid fa-motorcycle",
                BookingTime = "16/05/2026 07:30",
                TimeRange = "16/05/2026 08:00 - 16/05/2026 12:00",
                TotalPrice = 20000,
                Status = "Đã xác nhận",
                StatusClass = "confirmed",
                CustomerName = "Nguyễn Văn A",
                Phone = "0901 234 567",
                CanCancel = true
            },
            new()
            {
                Id = 2,
                Code = "BK250516-0002",
                ParkingName = "Times City",
                Position = "Tầng B1 - Khu A - Ô số 04",
                VehiclePlate = "51A-567.89",
                VehicleType = "Ô tô nhỏ",
                VehicleClass = "blue",
                Icon = "fa-solid fa-car-side",
                BookingTime = "16/05/2026 06:45",
                TimeRange = "16/05/2026 09:00 - 16/05/2026 11:00",
                TotalPrice = 30000,
                Status = "Đang chờ",
                StatusClass = "pending",
                CustomerName = "Nguyễn Văn A",
                Phone = "0901 234 567",
                CanCancel = true
            },
            new()
            {
                Id = 3,
                Code = "BK250515-0009",
                ParkingName = "Central Park",
                Position = "Tầng B2 - Khu B - Ô số 09",
                VehiclePlate = "30F-246.80",
                VehicleType = "Ô tô lớn",
                VehicleClass = "purple",
                Icon = "fa-solid fa-van-shuttle",
                BookingTime = "15/05/2026 20:10",
                TimeRange = "15/05/2026 18:00 - 15/05/2026 22:00",
                TotalPrice = 65000,
                Status = "Đã hoàn thành",
                StatusClass = "completed",
                CustomerName = "Nguyễn Văn A",
                Phone = "0901 234 567",
                CanCancel = false
            },
            new()
            {
                Id = 4,
                Code = "BK250515-0008",
                ParkingName = "EcoPark",
                Position = "Tầng G - Khu C - Ô số 02",
                VehiclePlate = "29H-987.65",
                VehicleType = "Xe máy",
                VehicleClass = "green",
                Icon = "fa-solid fa-motorcycle",
                BookingTime = "15/05/2026 14:22",
                TimeRange = "15/05/2026 15:00 - 15/05/2026 17:00",
                TotalPrice = 10000,
                Status = "Đã hủy",
                StatusClass = "cancelled",
                CustomerName = "Nguyễn Văn A",
                Phone = "0901 234 567",
                CanCancel = false
            }
            ];

            ParkingSlots =
            [
                new() { Code = "A01", Position = "Khu A - Ô 01", StatusName = "Đang dùng", StatusClass = "using", IsSelectable = false },
            new() { Code = "A02", Position = "Khu A - Ô 02", StatusName = "Trống", StatusClass = "empty", IsSelectable = true },
            new() { Code = "A03", Position = "Khu A - Ô 03", StatusName = "Trống", StatusClass = "empty", IsSelectable = true },
            new() { Code = "A04", Position = "Khu A - Ô 04", StatusName = "Đã đặt", StatusClass = "reserved", IsSelectable = false },
            new() { Code = "A05", Position = "Khu A - Ô 05", StatusName = "Đang dùng", StatusClass = "using", IsSelectable = false },
            new() { Code = "A06", Position = "Khu A - Ô 06", StatusName = "Bảo trì", StatusClass = "maintenance", IsSelectable = false },
            new() { Code = "A07", Position = "Khu A - Ô 07", StatusName = "Trống", StatusClass = "empty", IsSelectable = true },
            new() { Code = "A08", Position = "Khu A - Ô 08", StatusName = "Sự cố", StatusClass = "error", IsSelectable = false }
            ];
        }
    }

    public class BookingVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string ParkingName { get; set; } = "";
        public string Position { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string VehicleClass { get; set; } = "";
        public string Icon { get; set; } = "";
        public string BookingTime { get; set; } = "";
        public string TimeRange { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public bool CanCancel { get; set; }
    }

    public class ParkingSlotVm
    {
        public string Code { get; set; } = "";
        public string Position { get; set; } = "";
        public string StatusName { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public bool IsSelectable { get; set; }
    }
}
