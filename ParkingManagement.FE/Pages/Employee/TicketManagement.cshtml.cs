using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class TicketManagementModel : PageModel
    {
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int PaidTickets { get; set; }
        public int ExpiredTickets { get; set; }
        public int CancelledTickets { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? VehicleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? AreaFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CreatedDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        public List<TicketItemVM> Tickets { get; set; } = new List<TicketItemVM>();

        public TicketDetailVM? SelectedTicket { get; set; }
        public void OnGet()
        {
            Tickets = new List<TicketItemVM>
    {
        new TicketItemVM
        {
            Id = 1,
            TicketCode = "VE20240520001",
            CreatedAt = "20/05/2024 08:10",
            CustomerName = "Nguyễn Văn A",
            Phone = "0905 123 456",
            PlateNumber = "43A-12345",
            VehicleType = "Ô tô",
            Area = "B1-15",
            AreaClass = "b",
            CheckInTime = "08:10",
            CheckInDate = "20/05/2024",
            StatusText = "Đang gửi",
            StatusClass = "active",
            TotalPrice = null
        },
        new TicketItemVM
        {
            Id = 2,
            TicketCode = "VE20240520002",
            CreatedAt = "20/05/2024 07:45",
            CustomerName = "Trần Thị B",
            Phone = "0912 345 678",
            PlateNumber = "43B-67890",
            VehicleType = "Xe máy",
            Area = "A2-08",
            AreaClass = "a",
            CheckInTime = "07:45",
            CheckInDate = "20/05/2024",
            StatusText = "Đang gửi",
            StatusClass = "active",
            TotalPrice = null
        },
        new TicketItemVM
        {
            Id = 3,
            TicketCode = "VE20240519045",
            CreatedAt = "19/05/2024 18:20",
            CustomerName = "Phạm Thị D",
            Phone = "0987 654 321",
            PlateNumber = "43C-24680",
            VehicleType = "Xe máy",
            Area = "A1-05",
            AreaClass = "c",
            CheckInTime = "18:20",
            CheckInDate = "19/05/2024",
            StatusText = "Đã thanh toán",
            StatusClass = "paid",
            TotalPrice = 25000
        },
        new TicketItemVM
        {
            Id = 4,
            TicketCode = "VE20240519043",
            CreatedAt = "19/05/2024 16:05",
            CustomerName = "Đỗ Thị F",
            Phone = "0901 234 567",
            PlateNumber = "43B-22222",
            VehicleType = "Xe máy",
            Area = "A2-10",
            AreaClass = "a",
            CheckInTime = "16:05",
            CheckInDate = "19/05/2024",
            StatusText = "Quá hạn",
            StatusClass = "expired",
            TotalPrice = 30000
        }
    };

            TotalTickets = 1248;
            ActiveTickets = 320;
            PaidTickets = 856;
            ExpiredTickets = 72;
            CancelledTickets = 28;

            if (SelectedId.HasValue)
            {
                var selected = Tickets.FirstOrDefault(x => x.Id == SelectedId.Value);

                if (selected != null)
                {
                    SelectedTicket = new TicketDetailVM
                    {
                        Id = selected.Id,
                        TicketCode = selected.TicketCode,
                        CreatedAt = selected.CreatedAt,
                        CustomerName = selected.CustomerName,
                        Phone = selected.Phone,
                        PlateNumber = selected.PlateNumber,
                        VehicleType = selected.VehicleType,
                        Area = "B1 - Tầng hầm 1",
                        AreaClass = selected.AreaClass,
                        Position = selected.Area,
                        CheckInTime = selected.CheckInTime,
                        CheckInDate = selected.CheckInDate,
                        StatusText = selected.StatusText,
                        StatusClass = selected.StatusClass,
                        EmployeeName = "Nguyễn Văn An",
                        VehicleColor = "Trắng",
                        Brand = "Toyota",
                        CheckInFull = "20/05/2024 08:10",
                        DurationText = "2 giờ 35 phút",
                        BasePrice = 15000,
                        ServiceFee = 2000,
                        PaymentTotal = 17000
                    };
                }
            }
        }

        [Authorize(Roles = "Employee")]
    public class TicketItemVM
        {
            public int Id { get; set; }
            public string TicketCode { get; set; } = "";
            public string CreatedAt { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public string Phone { get; set; } = "";
            public string PlateNumber { get; set; } = "";
            public string VehicleType { get; set; } = "";
            public string Area { get; set; } = "";
            public string AreaClass { get; set; } = "";
            public string CheckInTime { get; set; } = "";
            public string CheckInDate { get; set; } = "";
            public string StatusText { get; set; } = "";
            public string StatusClass { get; set; } = "";
            public decimal? TotalPrice { get; set; }
        }

        [Authorize(Roles = "Employee")]
    public class TicketDetailVM : TicketItemVM
        {
            public string EmployeeName { get; set; } = "";
            public string VehicleColor { get; set; } = "";
            public string Brand { get; set; } = "";
            public string Position { get; set; } = "";
            public string CheckInFull { get; set; } = "";
            public string DurationText { get; set; } = "";
            public decimal BasePrice { get; set; }
            public decimal ServiceFee { get; set; }
            public decimal PaymentTotal { get; set; }
        }
    }
}

