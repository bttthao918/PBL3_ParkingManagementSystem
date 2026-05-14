using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.DAL.Interfaces;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class MonthlyTicketService : IMonthlyTicketService
    {
        private readonly IMonthlyTicketRepository _repo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IPaymentRepository _paymentRepo;

        private static readonly Dictionary<(string vehicleType, string package), decimal> Pricing = new()
        {
            [("Xe máy", "1 tháng")] = 400_000,
            [("Xe máy", "3 tháng")] = 1_100_000,
            [("Ô tô nhỏ", "1 tháng")] = 1_200_000,
            [("Ô tô nhỏ", "3 tháng")] = 3_200_000,
            [("Ô tô lớn", "1 tháng")] = 2_000_000,
            [("Ô tô lớn", "3 tháng")] = 5_500_000,
        };

        public MonthlyTicketService(
            IMonthlyTicketRepository repo,
            ICustomerRepository customerRepo,
            IVehicleRepository vehicleRepo,
            IPaymentRepository paymentRepo)
        {
            _repo = repo;
            _customerRepo = customerRepo;
            _vehicleRepo = vehicleRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<List<MonthlyTicketDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<List<MonthlyTicketDto>> GetByCustomerIdAsync(string customerId)
        {
            var list = await _repo.GetByCustomerIdAsync(customerId);
            return list.Select(MapToDto).ToList();
        }

        public async Task<MonthlyTicketDto?> GetByIdAsync(string id)
        {
            var ticket = await _repo.GetByIdAsync(id);
            return ticket == null ? null : MapToDto(ticket);
        }

        public async Task<ServiceResult<MonthlyTicketDto>> RegisterAsync(RegisterMonthlyTicketDto dto)
        {
            dto.VehiclePlate = dto.VehiclePlate.Trim().ToUpperInvariant();

            var (isValid, errorMessage) = MonthlyTicketValidator.Validate(dto);
            if (!isValid)
                return ServiceResult<MonthlyTicketDto>.Fail(errorMessage ?? "Dữ liệu không hợp lệ.");

            var existing = await _repo.GetActiveByPlateAsync(dto.VehiclePlate);
            if (existing != null)
                return ServiceResult<MonthlyTicketDto>.Fail("Biển số xe này đã có vé tháng đang sử dụng. Vui lòng hủy vé cũ trước.");

            var customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                return ServiceResult<MonthlyTicketDto>.Fail("Không tìm thấy khách hàng.");

            var fee = CalculateFee(dto.VehicleType!, dto.PackageType);
            if (fee == 0)
                return ServiceResult<MonthlyTicketDto>.Fail("Gói vé tháng không hợp lệ.");

            var start = DateTime.Today;
            var end = start.AddMonths(GetPackageMonths(dto.PackageType)).AddDays(-1);

            if (!await _vehicleRepo.ExistsAsync(dto.VehiclePlate))
            {
                await _vehicleRepo.AddAsync(new Vehicle
                {
                    VehiclePlate = dto.VehiclePlate,
                    VehicleType = dto.VehicleType!,
                    CustomerId = dto.CustomerId
                });
            }

            var id = await _repo.GenerateIdAsync();
            var monthly = new MonthlyTicket
            {
                MonthlyTicketId = id,
                CustomerId = dto.CustomerId,
                VehiclePlate = dto.VehiclePlate,
                VehicleType = dto.VehicleType!,
                StartDate = start,
                EndDate = end,
                PackageType = dto.PackageType,
                TotalFee = fee,
                Status = "Hoạt động",
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(monthly);
            await AddPaymentAsync(id, fee, dto.PaymentMethod);

            return ServiceResult<MonthlyTicketDto>.Ok(MapToDto(monthly), "Đăng ký vé tháng thành công!");
        }

        public async Task<ServiceResult<MonthlyTicketDto>> RenewAsync(string monthlyTicketId, RenewMonthlyTicketDto dto)
        {
            var ticket = await _repo.GetByIdAsync(monthlyTicketId);
            if (ticket == null)
                return ServiceResult<MonthlyTicketDto>.Fail("Không tìm thấy vé tháng.");

            if (ticket.Status == "Đã hủy")
                return ServiceResult<MonthlyTicketDto>.Fail("Vé tháng đã hủy không thể gia hạn.");

            if (string.IsNullOrWhiteSpace(dto.PackageType))
                return ServiceResult<MonthlyTicketDto>.Fail("Gói vé tháng không được để trống.");

            var fee = CalculateFee(ticket.VehicleType, dto.PackageType);
            if (fee == 0)
                return ServiceResult<MonthlyTicketDto>.Fail("Gói vé tháng không hợp lệ.");

            var extensionStart = ticket.EndDate.Date >= DateTime.Today
                ? ticket.EndDate.Date.AddDays(1)
                : DateTime.Today;

            ticket.EndDate = extensionStart.AddMonths(GetPackageMonths(dto.PackageType)).AddDays(-1);
            ticket.PackageType = dto.PackageType;
            ticket.TotalFee += fee;
            ticket.Status = "Hoạt động";

            await _repo.UpdateAsync(ticket);
            await AddPaymentAsync(ticket.MonthlyTicketId, fee, dto.PaymentMethod);

            return ServiceResult<MonthlyTicketDto>.Ok(MapToDto(ticket), "Gia hạn vé tháng thành công!");
        }

        public async Task<ServiceResult<string>> CancelAsync(string id)
        {
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket == null)
                return ServiceResult<string>.Fail("Không tìm thấy vé tháng.");

            if (ticket.Status != "Hoạt động")
                return ServiceResult<string>.Fail("Vé tháng đã không còn hoạt động.");

            ticket.Status = "Đã hủy";
            await _repo.UpdateAsync(ticket);

            return ServiceResult<string>.Ok(id, "Hủy vé tháng thành công.");
        }

        public async Task<List<MonthlyTicketDto>> GetExpiringSoonAsync(int days = 7)
        {
            var list = await _repo.GetExpiringSoonAsync(days);
            return list.Select(MapToDto).ToList();
        }

        public decimal CalculateFee(string vehicleType, string packageType)
        {
            Pricing.TryGetValue((vehicleType, packageType), out var fee);
            return fee;
        }

        private async Task AddPaymentAsync(string monthlyTicketId, decimal fee, string? paymentMethod)
        {
            var paymentId = await _paymentRepo.GenerateIdAsync();
            await _paymentRepo.AddAsync(new Payment
            {
                PaymentId = paymentId,
                TicketId = null,
                MonthlyTicketId = monthlyTicketId,
                Amount = fee,
                Method = string.IsNullOrWhiteSpace(paymentMethod) ? "Chuyển khoản" : paymentMethod,
                PaymentTime = DateTime.Now,
                Status = "Thành công"
            });
        }

        private static int GetPackageMonths(string packageType) => packageType switch
        {
            "1 tháng" => 1,
            "3 tháng" => 3,
            "6 tháng" => 6,
            _ => 0
        };

        private static MonthlyTicketDto MapToDto(MonthlyTicket ticket) => new()
        {
            MonthlyTicketId = ticket.MonthlyTicketId,
            CustomerName = ticket.Customer?.FullName ?? "",
            VehiclePlate = ticket.VehiclePlate,
            VehicleType = ticket.VehicleType,
            PackageType = ticket.PackageType,
            StartDate = ticket.StartDate,
            EndDate = ticket.EndDate,
            TotalFee = ticket.TotalFee,
            Status = ticket.Status,
            DaysRemaining = ticket.Status == "Hoạt động"
                ? Math.Max(0, (int)(ticket.EndDate.Date - DateTime.Today).TotalDays)
                : 0
        };
    }
}
