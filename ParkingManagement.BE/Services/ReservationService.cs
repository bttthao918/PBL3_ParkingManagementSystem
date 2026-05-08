using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Validators;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        private readonly IParkingSlotRepository _slotRepo;
        private readonly ICustomerRepository _customerRepo;

        public ReservationService(
            IReservationRepository repo,
            IParkingSlotRepository slotRepo,
            ICustomerRepository customerRepo)
        {
            _repo = repo;
            _slotRepo = slotRepo;
            _customerRepo = customerRepo;
        }

        public async Task<List<ReservationDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<List<ReservationDto>> GetByCustomerIdAsync(string customerId)
        {
            var list = await _repo.GetByCustomerIdAsync(customerId);
            return list.Select(MapToDto).ToList();
        }

        public async Task<ServiceResult<ReservationDto>> CreateAsync(CreateReservationDto dto)
        {
            // Validate DTO
            var (isValid, errorMessage) = ReservationValidator.Validate(dto);
            if (!isValid)
                return ServiceResult<ReservationDto>.Fail(errorMessage ?? "Dữ liệu không hợp lệ.");

            var customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                return ServiceResult<ReservationDto>.Fail("Không tìm thấy khách hàng.");

            string? slotId = dto.PreferredSlotId;
            if (!string.IsNullOrEmpty(slotId))
            {
                var preferred = await _slotRepo.GetByIdAsync(slotId);
                if (preferred == null || preferred.Status != "Trống")
                    slotId = null;
            }

            if (string.IsNullOrEmpty(slotId))
            {
                var available = await _slotRepo.GetAvailableAsync(dto.VehicleType);
                if (!available.Any())
                    return ServiceResult<ReservationDto>.Fail("Không còn chỗ trống cho loại xe này.");
                slotId = available.First().SlotId;
            }

            var id = await _repo.GenerateIdAsync();
            var reservation = new Reservation
            {
                ReservationId = id,
                CustomerId = dto.CustomerId,
                VehiclePlate = dto.VehiclePlate,
                SlotId = slotId,
                ExpectedTime = dto.ExpectedTime,
                CreatedAt = DateTime.Now,
                Status = "Chờ"
            };
            await _repo.AddAsync(reservation);

            await _slotRepo.UpdateStatusAsync(slotId, "Đã đặt");

            var result = await _repo.GetByIdAsync(id);
            return ServiceResult<ReservationDto>.Ok(MapToDto(result!), "Đặt chỗ thành công!");
        }

        public async Task<ServiceResult<string>> CancelAsync(string id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return ServiceResult<string>.Fail("Không tìm thấy đặt chỗ.");
            if (r.Status != "Chờ") return ServiceResult<string>.Fail("Đặt chỗ này không thể hủy.");

            r.Status = "Hủy";
            await _repo.UpdateAsync(r);

            if (!string.IsNullOrEmpty(r.SlotId))
                await _slotRepo.UpdateStatusAsync(r.SlotId, "Trống");

            return ServiceResult<string>.Ok(id, "Hủy đặt chỗ thành công.");
        }

        /// <summary>
        /// UC005.1 - Lấy danh sách đặt chỗ của khách hàng với filter và phân trang
        /// Tự động cập nhật status thành "Hết hạn" nếu quá giờ hẹn
        /// </summary>
        public async Task<ListReservationDto> GetByCustomerIdPaginatedAsync(string customerId, FilterReservationDto filter)
        {
            var query = await _repo.GetByCustomerIdAsync(customerId);

            // Auto-expire: Update status to "Hết hạn" if ExpectedTime has passed
            foreach (var res in query.Where(r => r.Status == "Chờ" && r.ExpectedTime < DateTime.Now))
            {
                res.Status = "Hết hạn";
                await _repo.UpdateAsync(res);

                // Release slot
                if (!string.IsNullOrEmpty(res.SlotId))
                    await _slotRepo.UpdateStatusAsync(res.SlotId, "Trống");
            }

            // Refresh query after auto-expire updates
            query = await _repo.GetByCustomerIdAsync(customerId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(r => r.Status == filter.Status).ToList();

            if (!string.IsNullOrWhiteSpace(filter.VehiclePlate))
                query = query.Where(r => r.VehiclePlate.Contains(filter.VehiclePlate, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.CreatedAt.Date >= filter.FromDate.Value.Date).ToList();

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.CreatedAt.Date <= filter.ToDate.Value.Date).ToList();

            // Calculate pagination
            var totalItems = query.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

            // Ensure page number is valid
            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageNumber > totalPages && totalPages > 0) filter.PageNumber = totalPages;

            var skip = (filter.PageNumber - 1) * filter.PageSize;
            var items = query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(filter.PageSize)
                .Select(MapToDto)
                .ToList();

            return new ListReservationDto
            {
                Items = items,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// UC005.2 - Hủy đơn đặt chỗ
        /// Chỉ có thể hủy nếu Status = "Chờ" và chưa quá giờ hẹn
        /// </summary>
        public async Task<ServiceResult<CancelReservationResultDto>> CancelReservationAsync(string customerId, string reservationId)
        {
            // 1. Get reservation
            var reservation = await _repo.GetByIdAsync(reservationId);
            if (reservation == null)
                return ServiceResult<CancelReservationResultDto>.Fail("Không tìm thấy đơn đặt chỗ.");

            // 2. Security check: ensure user owns this reservation
            if (reservation.CustomerId != customerId)
                return ServiceResult<CancelReservationResultDto>.Fail("Bạn không có quyền hủy đơn này.");

            // 3. Auto-expire check: if time has passed, mark as expired
            if (reservation.Status == "Chờ" && reservation.ExpectedTime < DateTime.Now)
            {
                reservation.Status = "Hết hạn";
                await _repo.UpdateAsync(reservation);

                if (!string.IsNullOrEmpty(reservation.SlotId))
                    await _slotRepo.UpdateStatusAsync(reservation.SlotId, "Trống");

                return ServiceResult<CancelReservationResultDto>.Fail(
                    "Thời gian đặt chỗ đã hết hạn, không thể hủy.");
            }

            // 4. Status check: can only cancel if Status = "Chờ"
            if (reservation.Status != "Chờ")
                return ServiceResult<CancelReservationResultDto>.Fail(
                    $"Không thể hủy đơn đặt chỗ với trạng thái: {reservation.Status}");

            // 5. Update status to "Hủy"
            reservation.Status = "Hủy";
            await _repo.UpdateAsync(reservation);

            // 6. Release parking slot
            if (!string.IsNullOrEmpty(reservation.SlotId))
                await _slotRepo.UpdateStatusAsync(reservation.SlotId, "Trống");

            // 7. TODO: Send notification to customer (email, SMS, push notification)
            // await _notificationService.SendReservationCancelledAsync(customerId, reservationId);

            // 8. Return success
            return ServiceResult<CancelReservationResultDto>.Ok(
                new CancelReservationResultDto
                {
                    Success = true,
                    ReservationId = reservationId,
                    NewStatus = "Hủy",
                    Message = "Hủy đặt chỗ thành công!"
                },
                "Hủy đặt chỗ thành công!");
        }

        private static ReservationDto MapToDto(Reservation r) => new()
        {
            ReservationId = r.ReservationId,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.FullName ?? "",
            VehiclePlate = r.VehiclePlate ?? "",
            VehicleType = r.Vehicle?.VehicleType ?? "",
            SlotId = r.SlotId,
            SlotLocation = r.ParkingSlot?.Location,
            ExpectedTime = r.ExpectedTime,
            CreatedAt = r.CreatedAt,
            Status = r.Status
        };
    }
}
