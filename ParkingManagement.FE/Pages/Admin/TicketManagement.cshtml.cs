using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class TicketManagementModel : PageModel
    {
        private readonly ITicketService _ticketService;

        public TicketManagementModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public List<TicketViewModel> Tickets { get; set; } = new();

        public async Task OnGetAsync()
        {
            var searchDto = new EmployeeTicketSearchDto
            {
                PageNumber = 1,
                PageSize = 100
            };

            try
            {
                var result = await _ticketService.SearchTicketsAsync(searchDto);

                if (result != null && result.Items != null)
                {
                    Tickets = result.Items.Select(t => new TicketViewModel(
                        t.TicketId,
                        t.VehiclePlate,
                        t.CustomerName ?? "Khách vãng lai",
                        t.VehicleType,
                        t.Fee ?? 0,
                        t.CheckInTime,
                        t.CheckOutTime ?? DateTime.MinValue,
                        t.Status,
                        t.Status == "Đang trong bãi" ? "active" : (t.Status == "Đã ra" ? "paid" : "expired")
                    )).ToList();
                }
            }
            catch (Exception ex)
            {
                // Optionally handle exception and keep list empty
                Console.WriteLine($"Error fetching tickets: {ex.Message}");
            }
        }
    }

    [Authorize(Roles = "Manager")]
    public class TicketViewModel
    {
        public string Code { get; set; }
        public string PlateNumber { get; set; }
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; }

        public TicketViewModel(
            string code,
            string plateNumber,
            string customerName,
            string type,
            decimal price,
            DateTime checkInTime,
            DateTime checkOutTime,
            string status,
            string statusClass)
        {
            Code = code;
            PlateNumber = plateNumber;
            CustomerName = customerName;
            Type = type;
            Price = price;
            CheckInTime = checkInTime;
            CheckOutTime = checkOutTime;
            Status = status;
            StatusClass = statusClass;
        }
    }
}
