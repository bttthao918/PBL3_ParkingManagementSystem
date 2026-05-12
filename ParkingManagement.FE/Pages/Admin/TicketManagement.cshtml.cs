using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager,Admin")]
    public class TicketManagementModel : PageModel
    {
        private readonly ITicketService _ticketService;

        public TicketManagementModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public List<TicketViewModel> Tickets { get; set; } = new();
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int CheckedOutTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int ShowingFrom { get; set; }
        public int ShowingTo { get; set; }
        public int PageSize { get; } = 10;
        public string? LoadErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Type { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int StartPage => TotalPages == 0 ? 0 : Math.Max(1, PageNumber - 2);
        public int EndPage => TotalPages == 0 ? 0 : Math.Min(TotalPages, PageNumber + 2);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
        public decimal ActiveRate => TotalTickets == 0 ? 0 : ActiveTickets * 100m / TotalTickets;
        public decimal CheckedOutRate => TotalTickets == 0 ? 0 : CheckedOutTickets * 100m / TotalTickets;

        public async Task<IActionResult> OnGetAsync()
        {
            PageNumber = PageNumber < 1 ? 1 : PageNumber;

            var searchDto = new EmployeeTicketSearchDto
            {
                SearchKeyword = Keyword,
                Status = Status,
                VehicleType = Type,
                FromDate = FromDate,
                ToDate = ToDate,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            try
            {
                var summary = await _ticketService.GetTicketSummaryAsync();
                if (summary != null)
                {
                    TotalTickets = summary.TotalTickets;
                    ActiveTickets = summary.ActiveTickets;
                    CheckedOutTickets = summary.CheckedOutTickets;
                    TotalRevenue = summary.TotalRevenue;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }
            catch (Exception ex)
            {
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }

            try
            {
                var result = await _ticketService.SearchTicketsAsync(searchDto);
                if (result == null)
                {
                    LoadErrorMessage ??= "Không tải được danh sách vé từ Backend API.";
                    return Page();
                }

                TotalItems = result.TotalItems;
                TotalPages = result.TotalPages;
                PageNumber = result.PageNumber > 0 ? result.PageNumber : PageNumber;
                ShowingFrom = TotalItems == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
                ShowingTo = TotalItems == 0 ? 0 : Math.Min(PageNumber * PageSize, TotalItems);

                Tickets = result.Items.Select(t => new TicketViewModel(
                    t.TicketId,
                    t.VehiclePlate,
                    t.CustomerName ?? "Khách vãng lai",
                    t.VehicleType,
                    t.Fee ?? 0,
                    t.CheckInTime,
                    t.CheckOutTime ?? DateTime.MinValue,
                    t.Status,
                    GetStatusClass(t.Status)
                )).ToList();
            }
            catch (UnauthorizedAccessException ex)
            {
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tickets: {ex.Message}");
                LoadErrorMessage = BuildLoadErrorMessage(ex);
            }

            return Page();
        }

        private static string BuildLoadErrorMessage(Exception ex)
        {
            var message = ex.Message;

            if (message.Contains("Connection refused", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
            {
                return "Không tải được dữ liệu vé vì Backend API chưa chạy ở http://localhost:5188.";
            }

            if (message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("401", StringComparison.OrdinalIgnoreCase))
            {
                return "Không tải được dữ liệu vé vì phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            }

            return $"Không tải được dữ liệu vé từ Backend API. Chi tiết: {message}";
        }

        private static string GetStatusClass(string status)
        {
            return status switch
            {
                "Đang trong bãi" => "active",
                "Đã ra" => "paid",
                _ => "expired"
            };
        }
    }

    [Authorize(Roles = "Manager,Admin")]
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
