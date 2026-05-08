using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;

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
            [("Xe máy", "1 tháng")] = 150_000,
            [("Xe máy", "3 tháng")] = 400_000,
            [("Xe máy", "6 tháng")] = 750_000,
            [("Ô tô nhỏ", "1 tháng")] = 350_000,
            [("Ô tô nhỏ", "3 tháng")] = 650_000,
            [("Ô tô nhỏ", "6 tháng")] = 900_000,
            [("Ô tô lớn", "1 tháng")] = 500_000,
            [("Ô tô lớn", "3 tháng")] = 900_000,
            [("Ô tô lớn", "6 tháng")] = 1_600_000,
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
            var m = await _repo.GetByIdAsync(id);
            return m == null ? null : MapToDto(m);
        }

        public async Task<ServiceResult<MonthlyTicketDto>> RegisterAsync(RegisterMonthlyTicketDto dto)
        {
            // Validate DTO
            var (isValid, errorMessage) = MonthlyTicketValidator.Validate(dto);
            if (!isValid)
                return ServiceResult<MonthlyTicketDto>.Fail(errorMessage ?? "Dữ liệu không hợp lệ.");

            var existing = await _repo.GetActiveByPlateAsync(dto.VehiclePlate);
            if (existing != null)
                return ServiceResult<MonthlyTicketDto>.Fail("Biển số xe này đã có vé tháng đang sử dụng. Vui lòng hủy vé cũ trước.");

            var customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                return ServiceResult<MonthlyTicketDto>.Fail("Không tìm thấy khách hàng.");

            decimal fee = CalculateFee(dto.VehicleType, dto.PackageType);
            if (fee == 0)
                return ServiceResult<MonthlyTicketDto>.Fail("Gói vé tháng không hợp lệ.");

            var start = DateTime.Today;
            int months = dto.PackageType switch
            {
                "1 tháng" => 1,
                "3 tháng" => 3,
                "6 tháng" => 6,
                _ => 0
            };
            if (months == 0) return ServiceResult<MonthlyTicketDto>.Fail("Gói thời gian không hợp lệ.");
            var end = start.AddMonths(months).AddDays(-1);

            if (!await _vehicleRepo.ExistsAsync(dto.VehiclePlate))
            {
                await _vehicleRepo.AddAsync(new Vehicle
                {
                    VehiclePlate = dto.VehiclePlate,
                    VehicleType = dto.VehicleType,
                    CustomerId = dto.CustomerId
                });
            }

            var id = await _repo.GenerateIdAsync();
            var monthly = new MonthlyTicket
            {
                MonthlyTicketId = id,
                CustomerId = dto.CustomerId,
                VehiclePlate = dto.VehiclePlate,
                VehicleType = dto.VehicleType,
                StartDate = start,
                EndDate = end,
                PackageType = dto.PackageType,
                TotalFee = fee,
                Status = "Hoạt động",
                CreatedAt = DateTime.Now
            };
            await _repo.AddAsync(monthly);

            var paymentId = await _paymentRepo.GenerateIdAsync();
            await _paymentRepo.AddAsync(new Payment
            {
                PaymentId = paymentId,
                TicketId = null,
                MonthlyTicketId = id,
                Amount = fee,
                Method = dto.PaymentMethod,
                PaymentTime = DateTime.Now,
                Status = "Thành công"
            });

            return ServiceResult<MonthlyTicketDto>.Ok(MapToDto(monthly), "Đăng ký vé tháng thành công!");
        }

        public async Task<ServiceResult<string>> CancelAsync(string id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return ServiceResult<string>.Fail("Không tìm thấy vé tháng.");
            if (m.Status != "Hoạt động") return ServiceResult<string>.Fail("Vé tháng đã không còn hoạt động.");

            m.Status = "Đã hủy";
            await _repo.UpdateAsync(m);
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

        private static MonthlyTicketDto MapToDto(MonthlyTicket m) => new()
        {
            MonthlyTicketId = m.MonthlyTicketId,
            CustomerName = m.Customer?.FullName ?? "",
            VehiclePlate = m.VehiclePlate,
            VehicleType = m.VehicleType,
            PackageType = m.PackageType,
            StartDate = m.StartDate,
            EndDate = m.EndDate,
            TotalFee = m.TotalFee,
            Status = m.Status,
            DaysRemaining = m.Status == "Hoạt động"
                ? Math.Max(0, (int)(m.EndDate - DateTime.Today).TotalDays)
                : 0
        };
    }
}
