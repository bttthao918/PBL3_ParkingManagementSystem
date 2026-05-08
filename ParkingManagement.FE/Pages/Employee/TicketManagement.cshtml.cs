using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class TicketManagementModel : PageModel
    {
        private readonly ITicketService _ticketService;

        public TicketManagementModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

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
        public async Task OnGetAsync()
        {
            var searchDto = new EmployeeTicketSearchDto
            {
                SearchKeyword = Search,
                Status = StatusFilter,
                VehicleType = VehicleFilter,
                PageNumber = 1,
                PageSize = 50
            };

            var result = await _ticketService.SearchTicketsAsync(searchDto);

            if (result != null && result.Items != null)
            {
                TotalTickets = result.TotalItems;
                ActiveTickets = result.Items.Count(t => t.Status == "Đang trong bãi");
                PaidTickets = result.Items.Count(t => t.Status == "Đã ra");
                ExpiredTickets = 0;
                CancelledTickets = 0;

                Tickets = result.Items.Select((t, index) => new TicketItemVM
                {
                    Id = index + 1, // ID tạm thời cho FE
                    TicketCode = t.TicketId,
                    CreatedAt = t.CheckInTime.ToString("dd/MM/yyyy HH:mm"),
                    CustomerName = t.CustomerName ?? "Khách vãng lai",
                    Phone = "", 
                    PlateNumber = t.VehiclePlate,
                    VehicleType = t.VehicleType,
                    Area = t.SlotId ?? "Chưa xếp chỗ",
                    AreaClass = string.IsNullOrEmpty(t.SlotId) ? "" : t.SlotId.Substring(0, 1).ToLower(),
                    CheckInTime = t.CheckInTime.ToString("HH:mm"),
                    CheckInDate = t.CheckInTime.ToString("dd/MM/yyyy"),
                    StatusText = t.Status,
                    StatusClass = t.Status == "Đang trong bãi" ? "active" : (t.Status == "Đã ra" ? "paid" : "expired"),
                    TotalPrice = t.Fee
                }).ToList();
            }
            else
            {
                Tickets = new List<TicketItemVM>();
                TotalTickets = 0;
                ActiveTickets = 0;
                PaidTickets = 0;
                ExpiredTickets = 0;
                CancelledTickets = 0;
            }

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
                        Area = selected.Area,
                        AreaClass = selected.AreaClass,
                        Position = selected.Area,
                        CheckInTime = selected.CheckInTime,
                        CheckInDate = selected.CheckInDate,
                        StatusText = selected.StatusText,
                        StatusClass = selected.StatusClass,
                        EmployeeName = User.Identity?.Name ?? "Employee",
                        VehicleColor = "N/A",
                        Brand = "N/A",
                        CheckInFull = selected.CheckInDate + " " + selected.CheckInTime,
                        DurationText = "",
                        BasePrice = selected.TotalPrice ?? 0,
                        ServiceFee = 0,
                        PaymentTotal = selected.TotalPrice ?? 0
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

