using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMonthlyTicketRepository _monthlyTicketRepository;

        public CustomerService(
            ICustomerRepository repo,
            ITicketRepository ticketRepository,
            IMonthlyTicketRepository monthlyTicketRepository)
        {
            _repo = repo;
            _ticketRepository = ticketRepository;
            _monthlyTicketRepository = monthlyTicketRepository;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<CustomerDto?> GetByIdAsync(string id)
        {
            var c = await _repo.GetByIdAsync(id);
            return c == null ? null : MapToDto(c);
        }

        public async Task<List<CustomerDto>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return await GetAllAsync();
            var list = await _repo.SearchAsync(keyword.Trim());
            return list.Select(MapToDto).ToList();
        }

        /// <summary>
        /// UC013 - Tìm kiếm chi tiết khách hàng
        /// Hỗ trợ tìm theo: Tên, SĐT, Email, Biển số xe
        /// Giới hạn 50 kết quả, hỗ trợ tìm gần đúng, không phân biệt hoa/thường
        /// </summary>
        public async Task<CustomerSearchResultDto> SearchAdvancedAsync(CustomerSearchDto searchDto)
        {
            // Kiểm tra xem có ít nhất một tiêu chí tìm kiếm
            if (string.IsNullOrWhiteSpace(searchDto.FullName) &&
                string.IsNullOrWhiteSpace(searchDto.PhoneNumber) &&
                string.IsNullOrWhiteSpace(searchDto.Email) &&
                string.IsNullOrWhiteSpace(searchDto.VehiclePlate))
            {
                return new CustomerSearchResultDto
                {
                    TotalResults = 0,
                    DisplayedResults = 0,
                    Customers = new(),
                    Message = "Vui lòng nhập ít nhất một tiêu chí tìm kiếm (Tên, SĐT, Email, hoặc Biển số xe)."
                };
            }

            // Chuẩn hóa dữ liệu tìm kiếm (trim, lowercase)
            var fullName = searchDto.FullName?.Trim().ToLower();
            var phoneNumber = searchDto.PhoneNumber?.Trim().ToLower();
            var email = searchDto.Email?.Trim().ToLower();
            var vehiclePlate = searchDto.VehiclePlate?.Trim().ToUpper();

            // Tìm kiếm trong database (giới hạn 50 kết quả)
            var results = await _repo.SearchAdvancedAsync(fullName, phoneNumber, email, vehiclePlate, maxResults: 50);

            if (results.Count == 0)
            {
                return new CustomerSearchResultDto
                {
                    TotalResults = 0,
                    DisplayedResults = 0,
                    Customers = new(),
                    Message = "Không tìm thấy khách hàng phù hợp. Vui lòng kiểm tra lại thông tin hoặc thoát chức năng."
                };
            }

            // Nếu có quá nhiều kết quả (đúng 50 = có thể còn nhiều hơn)
            var message = "";
            if (results.Count == 50)
            {
                message = "Có quá nhiều kết quả. Vui lòng nhập thêm tiêu chí để lọc chính xác hơn. Hiển thị 20 kết quả đầu tiên.";
                results = results.Take(20).ToList();
            }

            var customerDtos = results.Select(MapToDto).ToList();

            return new CustomerSearchResultDto
            {
                TotalResults = results.Count,
                DisplayedResults = customerDtos.Count,
                Customers = customerDtos,
                Message = message
            };
        }

        public async Task<ServiceResult<string>> SoftDeleteAsync(string id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return ServiceResult<string>.Fail("Không tìm thấy khách hàng.");
            await _repo.SoftDeleteAsync(id);
            return ServiceResult<string>.Ok(id, "Xóa khách hàng thành công.");
        }

        public async Task<List<CustomerDto>> GetDeletedAsync()
        {
            var list = await _repo.GetDeletedAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<ServiceResult<string>> RestoreAsync(string id)
        {
            await _repo.RestoreAsync(id);
            return ServiceResult<string>.Ok(id, "Khôi phục thành công.");
        }

        private static CustomerDto MapToDto(DAL.Models.Customer c) => new()
        {
            CustomerId = c.CustomerId,
            FullName = c.FullName,
            PhoneNumber = c.PhoneNumber,
            Email = c.Account?.Email,
            IsDeleted = c.IsDeleted,
            VehiclePlates = c.Vehicles.Select(v => v.VehiclePlate).ToList()
        };

        // ════════════════════════════════════════════════════════════
        // Advanced Features (Previously in EmployeeCustomerService)
        // ════════════════════════════════════════════════════════════

        public async Task<ListEmployeeCustomerSearchDto> SearchCustomersAsync(EmployeeCustomerSearchFilterDto filter)
        {
            try
            {
                var allCustomers = (await _repo.GetAllAsync()).ToList();
                var allTickets = (await _ticketRepository.GetAllAsync()).ToList();
                var allMonthlyTickets = (await _monthlyTicketRepository.GetAllAsync()).ToList();

                // Search by keyword
                var keyword = filter.SearchKeyword?.ToLower().Trim() ?? "";
                var filtered = allCustomers
                    .Where(c =>
                        string.IsNullOrEmpty(keyword) ||
                        c.FullName.ToLower().Contains(keyword) ||
                        (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(keyword)) ||
                        (c.Account?.Email != null && c.Account.Email.ToLower().Contains(keyword)) ||
                        allTickets.Any(t => t.CustomerId == c.CustomerId && t.VehiclePlate.ToLower().Contains(keyword))
                    )
                    .ToList();

                var sorted = filtered.OrderBy(c => c.FullName).ToList();

                // Pagination
                var totalItems = sorted.Count;
                var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                var items = sorted
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                // Map to DTOs
                var resultDtos = items.Select(c =>
                {
                    var customerTickets = allTickets.Where(t => t.CustomerId == c.CustomerId).ToList();
                    var hasActiveMonthly = allMonthlyTickets.Any(m => 
                        m.CustomerId == c.CustomerId && m.Status == "Hoạt động");
                    var lastVisit = customerTickets.Count > 0 ? customerTickets.Max(t => (DateTime?)t.CheckInTime) : null;

                    return new EmployeeCustomerSearchResultDto
                    {
                        CustomerId = c.CustomerId,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber ?? "",
                        Email = c.Account?.Email ?? "",
                        HasActiveMonthlyTicket = hasActiveMonthly,
                        TotalTickets = customerTickets.Count,
                        LastVisit = lastVisit
                    };
                }).ToList();

                return new ListEmployeeCustomerSearchDto
                {
                    Items = resultDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            }
            catch (Exception)
            {
                return new ListEmployeeCustomerSearchDto
                {
                    Items = new(),
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
        }

        public async Task<EmployeeCustomerDetailDto> GetCustomerDetailAsync(string customerId)
        {
            try
            {
                var customer = await _repo.GetByIdAsync(customerId);
                if (customer == null)
                    throw new Exception("Khách hàng không tồn tại");

                var tickets = (await _ticketRepository.GetAllAsync())
                    .Where(t => t.CustomerId == customerId)
                    .ToList();

                var monthlyTickets = (await _monthlyTicketRepository.GetAllAsync())
                    .Where(m => m.CustomerId == customerId)
                    .ToList();

                var activeMonthly = monthlyTickets.FirstOrDefault(m => m.Status == "Hoạt động");
                var daysRemaining = activeMonthly != null
                    ? (int)(activeMonthly.EndDate - DateTime.Now).TotalDays
                    : (int?)null;

                // Favorite vehicle
                var vehicleUsage = tickets
                    .GroupBy(t => t.VehiclePlate)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                var totalSpent = tickets
                    .Sum(t => t.Fee);

                return new EmployeeCustomerDetailDto
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber ?? "",
                    Email = customer.Account?.Email ?? "",
                    CreatedAt = customer.Account?.CreatedAt ?? DateTime.Now,
                    HasActiveMonthlyTicket = activeMonthly != null,
                    ActiveMonthlyTicketId = activeMonthly?.MonthlyTicketId,
                    MonthlyTicketExpiry = activeMonthly?.EndDate,
                    DaysRemainingOnTicket = daysRemaining,
                    TotalTickets = tickets.Count,
                    TotalSpent = totalSpent,
                    LastVisit = tickets.Count > 0 ? tickets.Max(t => t.CheckInTime) : null,
                    FirstVisit = tickets.Count > 0 ? tickets.Min(t => t.CheckInTime) : null,
                    FavoriteVehiclePlate = vehicleUsage?.Key,
                    FavoriteVehicleType = vehicleUsage?.FirstOrDefault()?.VehicleType,
                    FavoriteVehicleUsageCount = vehicleUsage?.Count() ?? 0
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết khách hàng: {ex.Message}");
            }
        }
    }
}
