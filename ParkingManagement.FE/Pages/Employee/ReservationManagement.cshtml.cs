using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class ReservationManagementModel : PageModel
    {
        private const string WaitingStatus = "Chờ";
        private const string ReceivedStatus = "Đã nhận";
        private const string CancelledStatus = "Hủy";
        private const string ExpiredStatus = "Hết hạn";
        private static readonly int[] AllowedPageSizes = { 10, 20, 50 };

        private readonly IReservationService _reservationService;

        public ReservationManagementModel(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        public int TotalReservations { get; set; }
        public int WaitingReservations { get; set; }
        public int TodayReservations { get; set; }
        public int ReceivedReservations { get; set; }
        public int ExpiredReservations { get; set; }
        public int CancelledReservations { get; set; }
        public int FilteredReservations { get; set; }
        public int TotalPages { get; set; }

        public int FirstItemIndex => FilteredReservations == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
        public int LastItemIndex => Math.Min(PageNumber * PageSize, FilteredReservations);
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
        public DateTime? ExpectedDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public List<ReservationItemVM> Reservations { get; set; } = new();
        public ReservationDetailVM? SelectedReservation { get; set; }

        [TempData]
        public string? ActionMessage { get; set; }

        [TempData]
        public bool ActionSuccess { get; set; }

        public async Task OnGetAsync()
        {
            SetViewData();
            NormalizePaging();

            await LoadSummaryAsync();

            var result = await SearchReservationsAsync();
            if (result != null && result.TotalPages > 0 && PageNumber > result.TotalPages)
            {
                PageNumber = result.TotalPages;
                result = await SearchReservationsAsync();
            }

            ApplyResult(result);
            LoadSelectedReservation();
        }

        public async Task<IActionResult> OnPostCancelAsync(string reservationId)
        {
            var result = await _reservationService.CancelForEmployeeAsync(reservationId);
            ActionSuccess = result?.Success == true;
            ActionMessage = result?.Message ?? "Không thể hủy đơn đặt chỗ.";

            return RedirectToPage(new
            {
                Search,
                StatusFilter,
                VehicleFilter,
                ExpectedDate = ExpectedDate?.ToString("yyyy-MM-dd"),
                PageNumber,
                PageSize,
                SelectedId
            });
        }

        private void SetViewData()
        {
            ViewData["Title"] = "Quản lý đặt chỗ";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";
        }

        private Task<ListReservationDto?> SearchReservationsAsync()
        {
            return _reservationService.GetForEmployeeAsync(
                Search,
                StatusFilter,
                VehicleFilter,
                ExpectedDate?.Date,
                ExpectedDate?.Date,
                PageNumber,
                PageSize);
        }

        private async Task LoadSummaryAsync()
        {
            var total = await _reservationService.GetForEmployeeAsync(pageNumber: 1, pageSize: 1);
            var waiting = await _reservationService.GetForEmployeeAsync(status: WaitingStatus, pageNumber: 1, pageSize: 1);
            var today = await _reservationService.GetForEmployeeAsync(fromDate: DateTime.Today, toDate: DateTime.Today, pageNumber: 1, pageSize: 1);
            var received = await _reservationService.GetForEmployeeAsync(status: ReceivedStatus, pageNumber: 1, pageSize: 1);
            var expired = await _reservationService.GetForEmployeeAsync(status: ExpiredStatus, pageNumber: 1, pageSize: 1);
            var cancelled = await _reservationService.GetForEmployeeAsync(status: CancelledStatus, pageNumber: 1, pageSize: 1);

            TotalReservations = total?.TotalItems ?? 0;
            WaitingReservations = waiting?.TotalItems ?? 0;
            TodayReservations = today?.TotalItems ?? 0;
            ReceivedReservations = received?.TotalItems ?? 0;
            ExpiredReservations = expired?.TotalItems ?? 0;
            CancelledReservations = cancelled?.TotalItems ?? 0;
        }

        private void NormalizePaging()
        {
            PageNumber = Math.Max(1, PageNumber);
            if (!AllowedPageSizes.Contains(PageSize))
            {
                PageSize = 10;
            }
        }

        private void ApplyResult(ListReservationDto? result)
        {
            if (result == null)
            {
                Reservations = new List<ReservationItemVM>();
                FilteredReservations = 0;
                TotalPages = 0;
                VisiblePages = new List<int>();
                ActionSuccess = false;
                ActionMessage ??= "Không lấy được danh sách đặt chỗ. Kiểm tra backend hoặc phiên đăng nhập.";
                return;
            }

            FilteredReservations = result.TotalItems;
            TotalPages = result.TotalPages;
            PageNumber = Math.Max(1, result.PageNumber);
            PageSize = result.PageSize > 0 ? result.PageSize : PageSize;
            VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

            Reservations = result.Items.Select((reservation, index) =>
            {
                var rowNumber = ((PageNumber - 1) * PageSize) + index + 1;
                var slotText = !string.IsNullOrWhiteSpace(reservation.SlotLocation)
                    ? reservation.SlotLocation
                    : reservation.SlotId ?? "Tự động";
                var customerName = string.IsNullOrWhiteSpace(reservation.CustomerName)
                    ? "Khách hàng"
                    : reservation.CustomerName;

                return new ReservationItemVM
                {
                    Id = rowNumber,
                    ReservationId = reservation.ReservationId,
                    CustomerId = reservation.CustomerId ?? "",
                    CustomerName = customerName,
                    VehiclePlate = reservation.VehiclePlate,
                    VehicleType = string.IsNullOrWhiteSpace(reservation.VehicleType) ? "Chưa rõ" : reservation.VehicleType,
                    SlotId = reservation.SlotId,
                    SlotText = slotText,
                    ExpectedTime = reservation.ExpectedTime,
                    ExpectedDateText = reservation.ExpectedTime.ToString("dd/MM/yyyy"),
                    ExpectedTimeText = reservation.ExpectedTime.ToString("HH:mm"),
                    CreatedAt = reservation.CreatedAt,
                    CreatedAtText = reservation.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    Status = reservation.Status,
                    StatusClass = ResolveStatusClass(reservation.Status),
                    TimeLabel = BuildTimeLabel(reservation.ExpectedTime, reservation.Status),
                    CanCancel = CanCancel(reservation),
                    CanCheckIn = IsWaiting(reservation.Status)
                };
            }).ToList();
        }

        private void LoadSelectedReservation()
        {
            if (!SelectedId.HasValue)
            {
                return;
            }

            var selected = Reservations.FirstOrDefault(x => x.Id == SelectedId.Value);
            if (selected == null)
            {
                return;
            }

            SelectedReservation = new ReservationDetailVM
            {
                Id = selected.Id,
                ReservationId = selected.ReservationId,
                CustomerId = selected.CustomerId,
                CustomerName = selected.CustomerName,
                VehiclePlate = selected.VehiclePlate,
                VehicleType = selected.VehicleType,
                SlotId = selected.SlotId,
                SlotText = selected.SlotText,
                ExpectedTime = selected.ExpectedTime,
                ExpectedDateText = selected.ExpectedDateText,
                ExpectedTimeText = selected.ExpectedTimeText,
                CreatedAt = selected.CreatedAt,
                CreatedAtText = selected.CreatedAtText,
                Status = selected.Status,
                StatusClass = selected.StatusClass,
                TimeLabel = selected.TimeLabel,
                CanCancel = selected.CanCancel,
                CanCheckIn = selected.CanCheckIn
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

        private static bool IsWaiting(string? status)
        {
            return string.Equals(status, WaitingStatus, StringComparison.OrdinalIgnoreCase);
        }

        private static bool CanCancel(ReservationDetailDto reservation)
        {
            return IsWaiting(reservation.Status) && reservation.ExpectedTime >= DateTime.Now;
        }

        private static string ResolveStatusClass(string? status)
        {
            return status switch
            {
                WaitingStatus => "waiting",
                ReceivedStatus => "received",
                CancelledStatus => "cancelled",
                ExpiredStatus => "expired",
                _ => "neutral"
            };
        }

        private static string BuildTimeLabel(DateTime expectedTime, string status)
        {
            if (!IsWaiting(status))
            {
                return status;
            }

            var remaining = expectedTime - DateTime.Now;
            if (remaining.TotalMinutes < 0)
            {
                return "Quá hẹn";
            }

            if (remaining.TotalMinutes < 60)
            {
                return $"{Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes))} phút nữa";
            }

            if (remaining.TotalHours < 24)
            {
                return $"{Math.Max(1, (int)Math.Ceiling(remaining.TotalHours))} giờ nữa";
            }

            return $"{Math.Max(1, (int)Math.Ceiling(remaining.TotalDays))} ngày nữa";
        }
    }

    public class ReservationItemVM
    {
        public int Id { get; set; }
        public string ReservationId { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string VehiclePlate { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string? SlotId { get; set; }
        public string SlotText { get; set; } = "";
        public DateTime ExpectedTime { get; set; }
        public string ExpectedDateText { get; set; } = "";
        public string ExpectedTimeText { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public string TimeLabel { get; set; } = "";
        public bool CanCancel { get; set; }
        public bool CanCheckIn { get; set; }
        public string AvatarLetter => string.IsNullOrWhiteSpace(CustomerName) ? "K" : CustomerName[..1].ToUpperInvariant();
    }

    public class ReservationDetailVM : ReservationItemVM
    {
    }
}
