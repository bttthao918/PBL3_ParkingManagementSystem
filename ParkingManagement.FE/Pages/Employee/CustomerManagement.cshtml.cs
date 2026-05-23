using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class CustomerManagementModel : PageModel
    {
        private static readonly int[] AllowedPageSizes = { 5, 10, 20 };
        private const decimal SilverThreshold = 2_000_000m;
        private const decimal GoldThreshold = 5_000_000m;
        private const decimal DiamondThreshold = 10_000_000m;

        private static readonly IReadOnlyList<VipTierVM> VipTierCatalog = new List<VipTierVM>
        {
            new("Thành viên", "member", "fa-user", 0m, "0 đ", 0),
            new("Bạc", "silver", "fa-medal", SilverThreshold, "2.000.000 đ", 5),
            new("Vàng", "gold", "fa-crown", GoldThreshold, "5.000.000 đ", 10),
            new("Kim Cương", "diamond", "fa-gem", DiamondThreshold, "10.000.000 đ", 15)
        };

        private readonly Services.ICustomerApiService _customerService;

        public CustomerManagementModel(Services.ICustomerApiService customerService)
        {
            _customerService = customerService;
        }

        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int VipCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int TotalPages { get; set; }
        public string? ErrorMessage { get; set; }

        public int FirstItemIndex => TotalCustomers == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
        public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalCustomers);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
        public List<int> VisiblePages { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? VipFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? RegisterDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public List<CustomerItemVM> Customers { get; set; } = new();
        public IReadOnlyList<VipTierVM> VipTiers => VipTierCatalog;

        public CustomerDetailVM? SelectedCustomer { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Quản lý khách hàng";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Nhân viên";

            NormalizePaging();

            var result = await SearchCustomersAsync();
            if (result != null && result.TotalPages > 0 && PageNumber > result.TotalPages)
            {
                PageNumber = result.TotalPages;
                result = await SearchCustomersAsync();
            }

            if (result == null)
            {
                SetEmptyState("Không lấy được danh sách khách hàng. Kiểm tra backend hoặc phiên đăng nhập.");
                return;
            }

            ApplyResult(result);
            await LoadSelectedCustomerAsync();
        }

        private async Task<ListEmployeeCustomerSearchDto?> SearchCustomersAsync()
        {
            var filter = new EmployeeCustomerSearchFilterDto
            {
                SearchKeyword = Search?.Trim() ?? "",
                StatusFilter = StatusFilter,
                VipLevel = VipFilter,
                RegisterDate = RegisterDate?.Date,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            return await _customerService.SearchForEmployeeAsync(filter);
        }

        private void NormalizePaging()
        {
            PageNumber = Math.Max(1, PageNumber);
            if (!AllowedPageSizes.Contains(PageSize))
            {
                PageSize = 10;
            }
        }

        private void ApplyResult(ListEmployeeCustomerSearchDto result)
        {
            TotalCustomers = result.TotalItems;
            TotalPages = result.TotalPages;
            PageNumber = Math.Max(1, result.PageNumber);
            PageSize = result.PageSize > 0 ? result.PageSize : PageSize;
            VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

            Customers = result.Items.Select((c, index) =>
            {
                var rowNumber = ((PageNumber - 1) * PageSize) + index + 1;
                var vipLevel = ResolveVipLevel(c.VipLevel, c.TotalTickets, c.HasActiveMonthlyTicket);
                return new CustomerItemVM
                {
                    Id = rowNumber,
                    FullName = c.FullName,
                    CustomerCode = c.CustomerId,
                    Phone = string.IsNullOrWhiteSpace(c.PhoneNumber) ? "-" : c.PhoneNumber,
                    Email = string.IsNullOrWhiteSpace(c.Email) ? "-" : c.Email,
                    VipLevel = vipLevel,
                    VipClass = GetVipCssClass(vipLevel),
                    VipIcon = GetVipIcon(vipLevel),
                    DiscountPercent = GetDiscountPercent(vipLevel),
                    StatusText = c.IsParking ? "Đang gửi xe" : (c.LastVisit.HasValue ? c.LastVisit.Value.ToString("dd/MM/yyyy") : "Chưa gửi"),
                    StatusClass = c.IsParking ? "parking" : "left",
                    TotalTickets = c.TotalTickets
                };
            }).ToList();

            ActiveCustomers = result.ActiveCustomers;
            VipCustomers = result.VipCustomers;
            NewCustomers = result.NewCustomers;

            if (!string.IsNullOrWhiteSpace(VipFilter))
            {
                Customers = Customers
                    .Where(c => IsVipMatch(c.VipLevel, VipFilter))
                    .ToList();
            }
        }

        private async Task LoadSelectedCustomerAsync()
        {
            var selectedCustomerId = SelectedId ?? Customers.FirstOrDefault()?.Id;
            var selected = Customers.FirstOrDefault(x => x.Id == selectedCustomerId)
                           ?? Customers.FirstOrDefault();

            if (selected == null)
            {
                return;
            }

            var detail = await _customerService.GetEmployeeCustomerDetailAsync(selected.CustomerCode);
            SelectedCustomer = MapDetail(selected, detail);
        }

        private static CustomerDetailVM MapDetail(CustomerItemVM selected, EmployeeCustomerDetailDto? detail)
        {
            var vipLevel = ResolveVipLevel(detail?.VipLevel ?? selected.VipLevel, detail?.TotalTickets ?? selected.TotalTickets, detail?.HasActiveMonthlyTicket ?? false);
            var totalTickets = detail?.TotalTickets ?? selected.TotalTickets;
            var totalSpent = detail?.TotalSpent ?? 0;
            var discountPercent = detail?.DiscountPercent ?? GetDiscountPercent(vipLevel);
            var vipProgress = detail?.VipProgress ?? CalculateVipProgress(totalSpent, vipLevel);
            var amountToNextLevel = detail?.AmountToNextLevel ?? CalculateAmountToNextLevel(totalSpent, vipLevel);
            var nextVipLevel = GetNextVipLevel(totalSpent);

            var histories = detail?.RecentTickets.Select(t => new CustomerParkingHistoryVM
            {
                Date = t.CheckInTime.ToString("dd/MM/yyyy"),
                CheckIn = t.CheckInTime.ToString("HH:mm"),
                CheckOut = t.CheckOutTime?.ToString("HH:mm") ?? "Trong bãi",
                Fee = t.Fee
            }).ToList() ?? new List<CustomerParkingHistoryVM>();

            return new CustomerDetailVM
            {
                Id = selected.Id,
                FullName = detail?.FullName ?? selected.FullName,
                CustomerCode = detail?.CustomerId ?? selected.CustomerCode,
                Phone = detail?.PhoneNumber ?? selected.Phone,
                Email = string.IsNullOrWhiteSpace(detail?.Email) ? selected.Email : detail.Email,
                Gender = detail?.Gender switch { "Male" => "Nam", "Female" => "Nữ", _ => "Chưa cập nhật" },
                RegisterDate = detail?.CreatedAt.ToString("dd/MM/yyyy") ?? "-",
                VipLevel = vipLevel,
                VipClass = GetVipCssClass(vipLevel),
                VipIcon = GetVipIcon(vipLevel),
                TotalSpent = totalSpent,
                TotalTickets = totalTickets,
                DiscountPercent = discountPercent,
                VipProgress = vipProgress,
                AmountToNextLevel = amountToNextLevel,
                NextVipLevel = nextVipLevel ?? "Kim Cương",
                IsMaxVipLevel = nextVipLevel == null,
                Histories = histories
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

        private void SetEmptyState(string message)
        {
            TotalCustomers = 0;
            ActiveCustomers = 0;
            VipCustomers = 0;
            NewCustomers = 0;
            TotalPages = 0;
            Customers = new List<CustomerItemVM>();
            VisiblePages = new List<int>();
            ErrorMessage = message;
        }

        public string GetVipTierState(VipTierVM tier)
        {
            if (SelectedCustomer == null)
            {
                return "locked";
            }

            if (string.Equals(SelectedCustomer.VipLevel, tier.Name, StringComparison.OrdinalIgnoreCase))
            {
                return "current";
            }

            return SelectedCustomer.TotalSpent >= tier.Threshold ? "reached" : "locked";
        }

        private static string ResolveVipLevel(string? apiVipLevel, int totalTickets, bool hasActiveMonthlyTicket)
        {
            if (!string.IsNullOrWhiteSpace(apiVipLevel))
            {
                return NormalizeVipLevel(apiVipLevel);
            }

            if (totalTickets >= 80)
            {
                return "Kim Cương";
            }

            if (totalTickets >= 40)
            {
                return "Vàng";
            }

            if (totalTickets >= 15 || hasActiveMonthlyTicket)
            {
                return "Bạc";
            }

            return "Thành viên";
        }

        private static string NormalizeVipLevel(string vipLevel)
        {
            return vipLevel.Trim().ToLowerInvariant() switch
            {
                "normal" or "member" or "thuong" or "thường" or "thanh vien" or "thành viên" => "Thành viên",
                "silver" or "bac" or "bạc" => "Bạc",
                "gold" or "vang" or "vàng" => "Vàng",
                "platinum" or "diamond" or "kim cuong" or "kim cương" => "Kim Cương",
                _ => vipLevel.Trim()
            };
        }

        private static bool IsVipMatch(string vipLevel, string filter)
        {
            var normalizedFilter = NormalizeVipLevel(filter);
            return string.Equals(vipLevel, normalizedFilter, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNormalVip(string vipLevel)
        {
            return string.Equals(NormalizeVipLevel(vipLevel), "Thành viên", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetDiscountPercent(string vipLevel)
        {
            return NormalizeVipLevel(vipLevel) switch
            {
                "Bạc" => 5,
                "Vàng" => 10,
                "Kim Cương" => 15,
                _ => 0
            };
        }

        private static int CalculateVipProgress(decimal totalSpent, string vipLevel)
        {
            var (start, nextTarget) = GetProgressRange(totalSpent, vipLevel);

            if (nextTarget <= start)
            {
                return 100;
            }

            return Math.Clamp((int)Math.Round((totalSpent - start) / (nextTarget - start) * 100), 0, 100);
        }

        private static decimal CalculateAmountToNextLevel(decimal totalSpent, string vipLevel)
        {
            var (_, nextTarget) = GetProgressRange(totalSpent, vipLevel);

            return Math.Max(0, nextTarget - totalSpent);
        }

        private static (decimal Start, decimal Target) GetProgressRange(decimal totalSpent, string vipLevel)
        {
            var normalizedLevel = NormalizeVipLevel(vipLevel);
            return normalizedLevel switch
            {
                "Thành viên" => (0m, SilverThreshold),
                "Bạc" => (SilverThreshold, GoldThreshold),
                "Vàng" => (GoldThreshold, DiamondThreshold),
                _ => (totalSpent, totalSpent)
            };
        }

        private static string? GetNextVipLevel(decimal totalSpent)
        {
            if (totalSpent < SilverThreshold)
            {
                return "Bạc";
            }

            if (totalSpent < GoldThreshold)
            {
                return "Vàng";
            }

            if (totalSpent < DiamondThreshold)
            {
                return "Kim Cương";
            }

            return null;
        }

        private static string GetVipCssClass(string vipLevel)
        {
            return NormalizeVipLevel(vipLevel) switch
            {
                "Bạc" => "silver",
                "Vàng" => "gold",
                "Kim Cương" => "diamond",
                _ => "member"
            };
        }

        private static string GetVipIcon(string vipLevel)
        {
            return NormalizeVipLevel(vipLevel) switch
            {
                "Bạc" => "fa-medal",
                "Vàng" => "fa-crown",
                "Kim Cương" => "fa-gem",
                _ => "fa-user"
            };
        }
    }

    public class CustomerItemVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string CustomerCode { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string VipLevel { get; set; } = "Thành viên";
        public string VipClass { get; set; } = "member";
        public string VipIcon { get; set; } = "fa-user";
        public int DiscountPercent { get; set; }
        public string StatusText { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public int TotalTickets { get; set; }
    }

    public class CustomerDetailVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string CustomerCode { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Gender { get; set; }
        public string RegisterDate { get; set; } = "";
        public string VipLevel { get; set; } = "Thành viên";
        public string VipClass { get; set; } = "member";
        public string VipIcon { get; set; } = "fa-user";
        public decimal TotalSpent { get; set; }
        public int TotalTickets { get; set; }
        public int DiscountPercent { get; set; }
        public int VipProgress { get; set; }
        public decimal AmountToNextLevel { get; set; }
        public string NextVipLevel { get; set; } = "Bạc";
        public bool IsMaxVipLevel { get; set; }

        public List<CustomerParkingHistoryVM> Histories { get; set; } = new();
    }

    public class CustomerParkingHistoryVM
    {
        public string Date { get; set; } = "";
        public string CheckIn { get; set; } = "";
        public string CheckOut { get; set; } = "";
        public decimal Fee { get; set; }
    }

    public class VipTierVM
    {
        public VipTierVM(string name, string cssClass, string icon, decimal threshold, string thresholdLabel, int discountPercent)
        {
            Name = name;
            CssClass = cssClass;
            Icon = icon;
            Threshold = threshold;
            ThresholdLabel = thresholdLabel;
            DiscountPercent = discountPercent;
        }

        public string Name { get; }
        public string CssClass { get; }
        public string Icon { get; }
        public decimal Threshold { get; }
        public string ThresholdLabel { get; }
        public int DiscountPercent { get; }
    }
}
