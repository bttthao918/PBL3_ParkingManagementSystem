using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class DashboardModel : PageModel
    {
        private const int CustomerSnapshotPageSize = 1000;
        private const decimal SilverThreshold = 2_000_000m;
        private const decimal GoldThreshold = 5_000_000m;
        private const decimal DiamondThreshold = 10_000_000m;

        private readonly ICustomerApiService _customerApiService;
        private readonly ILogger<DashboardModel> _logger;

        public CustomerDashboardViewModel Dashboard { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public DashboardModel(ICustomerApiService customerApiService, ILogger<DashboardModel> logger)
        {
            _customerApiService = customerApiService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Khách hàng";

            var fallbackName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
            Dashboard.UserName = fallbackName;

            try
            {
                var profileTask = _customerApiService.GetProfileAsync();
                var reservationsTask = _customerApiService.GetReservationsAsync(1, CustomerSnapshotPageSize);
                var ticketsTask = _customerApiService.GetTicketsAsync(1, CustomerSnapshotPageSize);
                var monthlyTicketsTask = _customerApiService.GetMonthlyTicketsAsync();
                var paymentsTask = _customerApiService.GetPaymentHistoryAsync(1, CustomerSnapshotPageSize);

                await Task.WhenAll(profileTask, reservationsTask, ticketsTask, monthlyTicketsTask, paymentsTask);

                var profile = await profileTask;
                var reservations = await reservationsTask ?? new ListCustomerReservationDto();
                var tickets = await ticketsTask ?? new ListCustomerTicketDto();
                var monthlyTickets = await monthlyTicketsTask ?? new ListCustomerMonthlyTicketDto();
                var payments = await paymentsTask ?? new ListCustomerPaymentDto();
                var monthlyTicketSpent = monthlyTickets.Items
                    .Where(IsPaidMonthlyTicketForVip)
                    .Sum(x => x.TotalFee);

                Dashboard.UserName = string.IsNullOrWhiteSpace(profile?.FullName) ? fallbackName : profile.FullName;
                if (profile != null)
                {
                    Dashboard.VipLevel = profile.VipLevel ?? "Thành viên";
                    Dashboard.VipProgress = profile.VipProgress ?? 0;
                    Dashboard.DiscountPercent = profile.DiscountPercent ?? 0;
                    Dashboard.AmountToNextLevel = profile.AmountToNextLevel;
                    Dashboard.TotalSpending = Math.Max(profile.TotalSpent, monthlyTicketSpent);
                    if (Dashboard.TotalSpending > profile.TotalSpent)
                    {
                        Dashboard.VipLevel = DetermineVipLevel(Dashboard.TotalSpending);
                        Dashboard.DiscountPercent = GetDiscountPercent(Dashboard.VipLevel);
                        Dashboard.VipProgress = CalculateVipProgress(Dashboard.TotalSpending);
                        Dashboard.AmountToNextLevel = CalculateAmountToNextLevel(Dashboard.TotalSpending);
                    }
                }
                Dashboard.ReservationTotal = reservations.TotalItems > 0 ? reservations.TotalItems : reservations.Items.Count;
                Dashboard.ActiveReservationCount = reservations.Items.Count(IsActiveReservation);
                Dashboard.TicketTotal = tickets.TotalItems > 0 ? tickets.TotalItems : tickets.Items.Count;
                Dashboard.MonthlyTicketCount = monthlyTickets.ActiveCount > 0
                    ? monthlyTickets.ActiveCount
                    : monthlyTickets.Items.Count(IsActiveMonthlyTicket);
                Dashboard.CurrentMonthlyTicket = monthlyTickets.Items
                    .OrderByDescending(IsActiveMonthlyTicket)
                    .ThenBy(x => x.DaysRemaining < 0)
                    .ThenBy(x => x.DaysRemaining)
                    .FirstOrDefault();
                if (profile == null)
                {
                    var paymentSpent = payments.Items
                        .Where(IsSuccessfulPayment)
                        .Sum(x => x.Amount);
                    Dashboard.TotalSpending = Math.Max(paymentSpent, monthlyTicketSpent);
                    Dashboard.VipLevel = DetermineVipLevel(Dashboard.TotalSpending);
                    Dashboard.DiscountPercent = GetDiscountPercent(Dashboard.VipLevel);
                    Dashboard.VipProgress = CalculateVipProgress(Dashboard.TotalSpending);
                    Dashboard.AmountToNextLevel = CalculateAmountToNextLevel(Dashboard.TotalSpending);
                }
                Dashboard.RecentReservations = reservations.Items
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(3)
                    .ToList();
                Dashboard.RecentTickets = tickets.Items
                    .OrderByDescending(x => x.CheckInTime)
                    .Take(3)
                    .ToList();
                Dashboard.PaymentChartPoints = BuildPaymentChartPoints(payments.Items);

                ViewData["UserName"] = Dashboard.UserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load customer dashboard data");
                ErrorMessage = "Chưa tải được dữ liệu thật từ BE. Kiểm tra BE đang chạy và tài khoản còn token đăng nhập.";
                ViewData["UserName"] = fallbackName;
            }
        }

        public string MonthlyTicketBadge
        {
            get
            {
                var monthlyTicket = Dashboard.CurrentMonthlyTicket;
                if (monthlyTicket == null)
                {
                    return "Chưa đăng ký";
                }

                if (monthlyTicket.DaysRemaining < 0)
                {
                    return "Đã hết hạn";
                }

                return monthlyTicket.DaysRemaining <= 7 ? "Sắp hết hạn" : "Đang hoạt động";
            }
        }

        public string GetReservationStatusClass(string status)
            => IsCancelledStatus(status) ? "status-danger" : "status-success";

        public string GetMonthlyTicketBorderColor()
        {
            var monthlyTicket = Dashboard.CurrentMonthlyTicket;
            if (monthlyTicket == null || monthlyTicket.DaysRemaining < 0)
            {
                return "#ef4444";
            }

            return monthlyTicket.DaysRemaining <= 7 ? "#ef4444" : "#22c55e";
        }

        private static bool IsActiveReservation(CustomerReservationDto reservation)
        {
            var status = NormalizeStatus(reservation.Status);
            return status.Contains("cho") || status.Contains("nhan") || status.Contains("confirmed") || status.Contains("active");
        }

        private static bool IsActiveMonthlyTicket(CustomerMonthlyTicketDto monthlyTicket)
        {
            var status = NormalizeStatus(monthlyTicket.Status);
            return monthlyTicket.DaysRemaining >= 0 && (status.Contains("hoat") || status.Contains("active"));
        }

        private static bool IsPaidMonthlyTicketForVip(CustomerMonthlyTicketDto monthlyTicket)
        {
            if (monthlyTicket.TotalFee <= 0)
            {
                return false;
            }

            var status = NormalizeStatus(monthlyTicket.Status);
            return status.Contains("hoat")
                || status.Contains("active")
                || status.Contains("het han")
                || status.Contains("expired")
                || status.Contains("huy")
                || status.Contains("cancel");
        }

        private static bool IsSuccessfulPayment(CustomerPaymentDto payment)
        {
            var status = NormalizeStatus(payment.Status);
            return status.Contains("hoan tat")
                || status.Contains("thanh cong")
                || status.Contains("success")
                || status.Contains("completed");
        }

        private static bool IsCancelledStatus(string status)
        {
            var normalized = NormalizeStatus(status);
            return normalized.Contains("huy")
                || normalized.Contains("cancel")
                || normalized.Contains("het han")
                || normalized.Contains("expired")
                || normalized.Contains("fail");
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return "";
            }

            return status.Trim().ToLowerInvariant()
                .Replace("đ", "d")
                .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");
        }

        private static string BuildPaymentChartPoints(List<CustomerPaymentDto> payments)
        {
            var monthKeys = Enumerable.Range(0, 6)
                .Select(i => DateTime.Today.AddMonths(i - 5))
                .Select(d => new DateTime(d.Year, d.Month, 1))
                .ToList();

            var monthlyAmounts = monthKeys
                .Select(month => payments
                    .Where(IsSuccessfulPayment)
                    .Where(payment => payment.CreatedAt.Year == month.Year && payment.CreatedAt.Month == month.Month)
                    .Sum(payment => payment.Amount))
                .ToList();

            if (monthlyAmounts.All(amount => amount <= 0))
            {
                return "0,210 200,210 400,210 600,210 800,210 1000,210";
            }

            var maxAmount = monthlyAmounts.Max();
            var points = monthlyAmounts.Select((amount, index) =>
            {
                var x = index * 200;
                var y = 220 - (double)(amount / maxAmount) * 130;
                return $"{x},{Math.Round(y)}";
            });

            return string.Join(" ", points);
        }

        private static string DetermineVipLevel(decimal totalSpent)
        {
            if (totalSpent >= DiamondThreshold)
            {
                return "Kim Cương";
            }

            if (totalSpent >= GoldThreshold)
            {
                return "Vàng";
            }

            return totalSpent >= SilverThreshold ? "Bạc" : "Thành viên";
        }

        private static int GetDiscountPercent(string vipLevel)
        {
            return vipLevel switch
            {
                "Bạc" => 5,
                "Vàng" => 10,
                "Kim Cương" => 15,
                _ => 0
            };
        }

        private static int CalculateVipProgress(decimal totalSpent)
        {
            var (start, target) = totalSpent switch
            {
                < SilverThreshold => (0m, SilverThreshold),
                < GoldThreshold => (SilverThreshold, GoldThreshold),
                < DiamondThreshold => (GoldThreshold, DiamondThreshold),
                _ => (DiamondThreshold, DiamondThreshold)
            };

            return target <= start
                ? 100
                : Math.Clamp((int)Math.Round((totalSpent - start) / (target - start) * 100), 0, 100);
        }

        private static decimal? CalculateAmountToNextLevel(decimal totalSpent)
        {
            if (totalSpent < SilverThreshold)
            {
                return SilverThreshold - totalSpent;
            }

            if (totalSpent < GoldThreshold)
            {
                return GoldThreshold - totalSpent;
            }

            return totalSpent < DiamondThreshold ? DiamondThreshold - totalSpent : null;
        }
    }

    public class CustomerDashboardViewModel
    {
        public string UserName { get; set; } = "Customer"; public string VipLevel { get; set; } = "Thành viên"; public int VipProgress { get; set; } = 0; public int DiscountPercent { get; set; } = 0; public decimal? AmountToNextLevel { get; set; }
        public int ReservationTotal { get; set; }
        public int ActiveReservationCount { get; set; }
        public int TicketTotal { get; set; }
        public int MonthlyTicketCount { get; set; }
        public decimal TotalSpending { get; set; }
        public string PaymentChartPoints { get; set; } = "0,210 200,210 400,210 600,210 800,210 1000,210";
        public CustomerMonthlyTicketDto? CurrentMonthlyTicket { get; set; }
        public List<CustomerReservationDto> RecentReservations { get; set; } = new();
        public List<CustomerTicketDto> RecentTickets { get; set; } = new();
    }
}

