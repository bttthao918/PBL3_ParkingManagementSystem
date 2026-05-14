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
                    StatusText = c.LastVisit.HasValue ? c.LastVisit.Value.ToString("dd/MM/yyyy") : "Chưa gửi",
                    StatusClass = c.LastVisit.HasValue ? "parking" : "left",
                    TotalTickets = c.TotalTickets
                };
            }).ToList();

            ActiveCustomers = Customers.Count(c => c.StatusClass == "parking");
            VipCustomers = Customers.Count(c => !IsNormalVip(c.VipLevel));
            NewCustomers = Customers.Count(c => c.TotalTickets == 0);

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
                TotalSpent = totalSpent,
                TotalTickets = totalTickets,
                DiscountPercent = discountPercent,
                VipProgress = vipProgress,
                AmountToNextLevel = amountToNextLevel,
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

        private static string ResolveVipLevel(string? apiVipLevel, int totalTickets, bool hasActiveMonthlyTicket)
        {
            if (!string.IsNullOrWhiteSpace(apiVipLevel))
            {
                return NormalizeVipLevel(apiVipLevel);
            }

            if (totalTickets >= 80)
            {
                return "Platinum";
            }

            if (totalTickets >= 40)
            {
                return "Gold";
            }

            if (totalTickets >= 15 || hasActiveMonthlyTicket)
            {
                return "Silver";
            }

            return "Thường";
        }

        private static string NormalizeVipLevel(string vipLevel)
        {
            return vipLevel.Trim().ToLowerInvariant() switch
            {
                "normal" or "thuong" or "thường" => "Thường",
                "silver" => "Silver",
                "gold" => "Gold",
                "platinum" => "Platinum",
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
            return string.Equals(vipLevel, "Thường", StringComparison.OrdinalIgnoreCase)
                || string.Equals(vipLevel, "Normal", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetDiscountPercent(string vipLevel)
        {
            return vipLevel switch
            {
                "Silver" => 3,
                "Gold" => 5,
                "Platinum" => 10,
                _ => 0
            };
        }

        private static int CalculateVipProgress(decimal totalSpent, string vipLevel)
        {
            var nextTarget = vipLevel switch
            {
                "Thường" => 1_000_000m,
                "Silver" => 3_000_000m,
                "Gold" => 6_000_000m,
                _ => totalSpent
            };

            if (nextTarget <= 0)
            {
                return 100;
            }

            return Math.Clamp((int)Math.Round(totalSpent / nextTarget * 100), 0, 100);
        }

        private static decimal CalculateAmountToNextLevel(decimal totalSpent, string vipLevel)
        {
            var nextTarget = vipLevel switch
            {
                "Thường" => 1_000_000m,
                "Silver" => 3_000_000m,
                "Gold" => 6_000_000m,
                _ => totalSpent
            };

            return Math.Max(0, nextTarget - totalSpent);
        }
    }

    public class CustomerItemVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string CustomerCode { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string VipLevel { get; set; } = "Thường";
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
        public string VipLevel { get; set; } = "Thường";
        public decimal TotalSpent { get; set; }
        public int TotalTickets { get; set; }
        public int DiscountPercent { get; set; }
        public int VipProgress { get; set; }
        public decimal AmountToNextLevel { get; set; }

        public List<CustomerParkingHistoryVM> Histories { get; set; } = new();
    }

    public class CustomerParkingHistoryVM
    {
        public string Date { get; set; } = "";
        public string CheckIn { get; set; } = "";
        public string CheckOut { get; set; } = "";
        public decimal Fee { get; set; }
    }
}
