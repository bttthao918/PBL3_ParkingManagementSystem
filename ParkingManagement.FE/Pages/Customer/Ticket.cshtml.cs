using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Customer
{
    public class TicketModel : PageModel
    {
        public string UserName { get; set; } = "Customer";

        public List<TicketPriceVm> PriceCards { get; set; } = new();

        public List<TicketVm> Tickets { get; set; } = new();

        public void OnGet()
        {
            LoadPriceCards();
            LoadTickets();
        }

        private void LoadPriceCards()
        {
            PriceCards = new List<TicketPriceVm>
        {
            new()
            {
                VehicleType = "Xe máy",
                Icon = "fa-solid fa-motorcycle",
                ColorClass = "green",
                FirstHourPrice = 5000,
                NextHourPrice = 2000,
                OvernightPrice = 10000
            },
            new()
            {
                VehicleType = "Ô tô nhỏ",
                Icon = "fa-solid fa-car-side",
                ColorClass = "blue",
                FirstHourPrice = 15000,
                NextHourPrice = 5000,
                OvernightPrice = 40000
            },
            new()
            {
                VehicleType = "Ô tô lớn",
                Icon = "fa-solid fa-van-shuttle",
                ColorClass = "purple",
                FirstHourPrice = 25000,
                NextHourPrice = 8000,
                OvernightPrice = 60000
            }
        };
        }

        private void LoadTickets()
        {
            Tickets = new List<TicketVm>
        {
            new()
            {
                Id = 1,
                Code = "VE240501-0001",
                VehiclePlate = "59C1-123.45",
                VehicleType = "Xe máy",
                Icon = "fa-solid fa-motorcycle",
                IconClass = "green",
                CheckInTime = new DateTime(2024, 5, 1, 8, 15, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 10, 45, 0),
                Duration = "2 giờ 30 phút",
                ParkingFee = 10000,
                Discount = 0,
                TotalAmount = 10000,
                Status = "Đã thanh toán",
                PaymentMethod = "Tiền mặt",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            },
            new()
            {
                Id = 2,
                Code = "VE240501-0002",
                VehiclePlate = "51A-567.89",
                VehicleType = "Ô tô nhỏ",
                Icon = "fa-solid fa-car-side",
                IconClass = "blue",
                CheckInTime = new DateTime(2024, 5, 1, 9, 20, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 12, 20, 0),
                Duration = "3 giờ",
                ParkingFee = 30000,
                Discount = 0,
                TotalAmount = 30000,
                Status = "Đã thanh toán",
                PaymentMethod = "Chuyển khoản",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            },
            new()
            {
                Id = 3,
                Code = "VE240501-0003",
                VehiclePlate = "30E-246.80",
                VehicleType = "Ô tô lớn",
                Icon = "fa-solid fa-van-shuttle",
                IconClass = "purple",
                CheckInTime = new DateTime(2024, 5, 1, 10, 5, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 15, 30, 0),
                Duration = "5 giờ 25 phút",
                ParkingFee = 65000,
                Discount = 0,
                TotalAmount = 65000,
                Status = "Đã thanh toán",
                PaymentMethod = "Tiền mặt",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            },
            new()
            {
                Id = 4,
                Code = "VE240501-0004",
                VehiclePlate = "59C2-345.67",
                VehicleType = "Xe máy",
                Icon = "fa-solid fa-motorcycle",
                IconClass = "green",
                CheckInTime = new DateTime(2024, 5, 1, 11, 10, 0),
                CheckOutTime = new DateTime(2024, 5, 1, 11, 50, 0),
                Duration = "40 phút",
                ParkingFee = 5000,
                Discount = 0,
                TotalAmount = 5000,
                Status = "Đã thanh toán",
                PaymentMethod = "Tiền mặt",
                CreatedBy = "Nguyễn Văn A",
                Note = "-"
            }
        };
        }
    }

    public class TicketPriceVm
    {
        public string VehicleType { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string ColorClass { get; set; } = string.Empty;

        public decimal FirstHourPrice { get; set; }

        public decimal NextHourPrice { get; set; }

        public decimal OvernightPrice { get; set; }
    }

    public class TicketVm
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string VehiclePlate { get; set; } = string.Empty;

        public string VehicleType { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string IconClass { get; set; } = string.Empty;

        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string Duration { get; set; } = string.Empty;

        public decimal ParkingFee { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }
}
