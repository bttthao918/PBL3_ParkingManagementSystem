using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.Globalization;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class TicketManagementModel : PageModel
    {
        private const string ActiveStatus = "Đang trong bãi";
        private const string CheckedOutStatus = "Đã ra";
        private static readonly int[] AllowedPageSizes = { 10, 20, 50 };

        private readonly ITicketService _ticketService;

        public TicketManagementModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int PaidTickets { get; set; }
        public int FilteredTickets { get; set; }
        public int TotalPages { get; set; }

        public int FirstItemIndex => FilteredTickets == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
        public int LastItemIndex => Math.Min(PageNumber * PageSize, FilteredTickets);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
        public List<int> VisiblePages { get; set; } = new();

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

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public List<TicketItemVM> Tickets { get; set; } = new();

        public TicketDetailVM? SelectedTicket { get; set; }

        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Quản lý vé";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            // Fallback: nếu model binding không parse được CreatedDate (do format dd/MM/yyyy),
            // thử parse thủ công từ query string
            if (!CreatedDate.HasValue && Request.Query.ContainsKey("CreatedDate"))
            {
                var raw = Request.Query["CreatedDate"].ToString();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    // Thử parse nhiều format: yyyy-MM-dd, dd/MM/yyyy, MM/dd/yyyy
                    var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy" };
                    if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    {
                        CreatedDate = parsed;
                    }
                }
            }

            NormalizePaging();

            var summary = await _ticketService.GetTicketSummaryAsync();
            ApplySummary(summary);

            var result = await SearchTicketsAsync();
            if (result != null && result.TotalPages > 0 && PageNumber > result.TotalPages)
            {
                PageNumber = result.TotalPages;
                result = await SearchTicketsAsync();
            }

            ApplyResult(result);
            LoadSelectedTicket();
        }

        public async Task<IActionResult> OnPostCheckOutAsync(string ticketId, decimal fee, int? selectedId)
        {
            var ok = await _ticketService.CheckOutAsync(ticketId, fee);
            ActionSuccess = ok;
            ActionMessage = ok ? $"Đã check-out vé {ticketId}." : $"Check-out thất bại cho vé {ticketId}.";

            return RedirectToPage(new
            {
                Search,
                StatusFilter,
                VehicleFilter,
                AreaFilter,
                CreatedDate,
                PageNumber,
                PageSize,
                SelectedId = selectedId
            });
        }

        private Task<ListEmployeeTicketDto?> SearchTicketsAsync()
        {
            var searchDto = new EmployeeTicketSearchDto
            {
                SearchKeyword = Search?.Trim(),
                Status = StatusFilter,
                VehicleType = VehicleFilter,
                AreaFilter = AreaFilter,
                FromDate = CreatedDate?.Date,
                ToDate = CreatedDate?.Date,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            return _ticketService.SearchTicketsAsync(searchDto);
        }

        private void NormalizePaging()
        {
            PageNumber = Math.Max(1, PageNumber);
            if (!AllowedPageSizes.Contains(PageSize))
            {
                PageSize = 10;
            }
        }

        private void ApplySummary(TicketSummaryDto? summary)
        {
            TotalTickets = summary?.TotalTickets ?? 0;
            ActiveTickets = summary?.ActiveTickets ?? 0;
            PaidTickets = summary?.CheckedOutTickets ?? 0;
        }

        private void ApplyResult(ListEmployeeTicketDto? result)
        {
            if (result == null)
            {
                Tickets = new List<TicketItemVM>();
                FilteredTickets = 0;
                TotalPages = 0;
                VisiblePages = new List<int>();
                return;
            }

            FilteredTickets = result.TotalItems;
            TotalPages = result.TotalPages;
            PageNumber = Math.Max(1, result.PageNumber);
            PageSize = result.PageSize > 0 ? result.PageSize : PageSize;
            VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

            Tickets = result.Items.Select((ticket, index) =>
            {
                var rowNumber = ((PageNumber - 1) * PageSize) + index + 1;
                var isActive = IsActive(ticket.Status);
                var slotId = string.IsNullOrWhiteSpace(ticket.SlotId) ? "Chưa xếp chỗ" : ticket.SlotId;

                return new TicketItemVM
                {
                    Id = rowNumber,
                    TicketCode = ticket.TicketId,
                    CreatedAt = ticket.CheckInTime.ToString("dd/MM/yyyy HH:mm"),
                    CustomerName = string.IsNullOrWhiteSpace(ticket.CustomerName) ? "Khách vãng lai" : ticket.CustomerName,
                    Phone = "",
                    PlateNumber = ticket.VehiclePlate,
                    VehicleType = ticket.VehicleType,
                    Area = slotId,
                    AreaClass = ResolveAreaClass(ticket.SlotId),
                    CheckInTime = ticket.CheckInTime.ToString("HH:mm"),
                    CheckInDate = ticket.CheckInTime.ToString("dd/MM/yyyy"),
                    CheckInAt = ticket.CheckInTime,
                    CheckOutAt = ticket.CheckOutTime,
                    StatusText = ticket.Status,
                    StatusClass = ResolveStatusClass(ticket.Status),
                    TotalPrice = isActive ? null : ticket.Fee,
                    CanCheckOut = isActive
                };
            }).ToList();
        }

        private void LoadSelectedTicket()
        {
            if (!SelectedId.HasValue)
            {
                return;
            }

            var selected = Tickets.FirstOrDefault(x => x.Id == SelectedId.Value);
            if (selected == null)
            {
                return;
            }

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
                CheckInAt = selected.CheckInAt,
                CheckOutAt = selected.CheckOutAt,
                StatusText = selected.StatusText,
                StatusClass = selected.StatusClass,
                TotalPrice = selected.TotalPrice,
                CanCheckOut = selected.CanCheckOut,
                EmployeeName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên",
                VehicleColor = "Chưa cập nhật",
                Brand = "Chưa cập nhật",
                CheckInFull = $"{selected.CheckInDate} {selected.CheckInTime}",
                DurationText = FormatDuration(selected.CheckInAt, selected.CheckOutAt),
                BasePrice = selected.TotalPrice ?? 0,
                ServiceFee = 0,
                PaymentTotal = selected.TotalPrice ?? 0
            };
        }

        private static List<int> BuildVisiblePages(int currentPage, int totalPages)
        {
            if (totalPages <= 0)
            {
                return new List<int>();
            }

            var start = Math.Max(1, currentPage - 2);
            var end = Math.Min(totalPages, currentPage + 2);

            if (currentPage <= 2)
            {
                end = Math.Min(totalPages, 5);
            }
            else if (currentPage >= totalPages - 1)
            {
                start = Math.Max(1, totalPages - 4);
            }

            return Enumerable.Range(start, end - start + 1).ToList();
        }

        private static bool IsActive(string? status)
        {
            return string.Equals(status, ActiveStatus, StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveStatusClass(string? status)
        {
            return status switch
            {
                ActiveStatus => "active",
                CheckedOutStatus => "paid",
                "Đã hủy" => "cancelled",
                _ => "expired"
            };
        }

        private static string ResolveAreaClass(string? slotId)
        {
            return string.IsNullOrWhiteSpace(slotId)
                ? ""
                : slotId[..1].ToLowerInvariant();
        }

        private static string FormatDuration(DateTime checkIn, DateTime? checkOut)
        {
            var end = checkOut ?? DateTime.Now;
            var duration = end - checkIn;
            if (duration.TotalMinutes < 1)
            {
                return "Dưới 1 phút";
            }

            var days = duration.Days;
            var hours = duration.Hours;
            var minutes = duration.Minutes;

            if (days > 0)
            {
                return $"{days} ngày {hours} giờ";
            }

            if (hours > 0)
            {
                return $"{hours} giờ {minutes} phút";
            }

            return $"{minutes} phút";
        }
    }

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
        public DateTime CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public string StatusText { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public decimal? TotalPrice { get; set; }
        public bool CanCheckOut { get; set; }
        public string AvatarLetter => string.IsNullOrWhiteSpace(CustomerName) ? "?" : CustomerName[..1].ToUpperInvariant();
    }

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
