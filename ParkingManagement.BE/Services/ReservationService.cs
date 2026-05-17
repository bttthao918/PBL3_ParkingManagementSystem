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
        private readonly IVehicleRepository _vehicleRepo;

        public ReservationService(
            IReservationRepository repo,
            IParkingSlotRepository slotRepo,
            ICustomerRepository customerRepo,
            IVehicleRepository vehicleRepo)
        {
            _repo = repo;
            _slotRepo = slotRepo;
            _customerRepo = customerRepo;
            _vehicleRepo = vehicleRepo;
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

        public async Task<ListReservationDto> GetAllPaginatedAsync(FilterReservationDto filter)
        {
            NormalizePaging(filter);

            var reservations = await _repo.GetAllAsync();
            await ExpireWaitingReservationsAsync(reservations);
            reservations = await _repo.GetAllAsync();

            var filtered = reservations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.Status))
                filtered = filtered.Where(r => r.Status == filter.Status);

            var keyword = !string.IsNullOrWhiteSpace(filter.SearchKeyword)
                ? filter.SearchKeyword.Trim()
                : filter.VehiclePlate?.Trim();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filtered = filtered.Where(r =>
                    r.ReservationId.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (r.VehiclePlate?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Customer?.FullName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Customer?.PhoneNumber?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.SlotId?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.ParkingSlot?.Location?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(filter.VehicleType))
                filtered = filtered.Where(r => string.Equals(r.Vehicle?.VehicleType, filter.VehicleType, StringComparison.OrdinalIgnoreCase));

            if (filter.FromDate.HasValue)
                filtered = filtered.Where(r => r.ExpectedTime.Date >= filter.FromDate.Value.Date);

            if (filter.ToDate.HasValue)
                filtered = filtered.Where(r => r.ExpectedTime.Date <= filter.ToDate.Value.Date);

            var sorted = filtered
                .OrderBy(r => r.Status == "Chờ" ? 0 : 1)
                .ThenBy(r => r.ExpectedTime)
                .ThenByDescending(r => r.CreatedAt)
                .ToList();

            var totalItems = sorted.Count;
            var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            if (filter.PageNumber > totalPages && totalPages > 0)
                filter.PageNumber = totalPages;

            var items = sorted
                .Skip((filter.PageNumber - 1) * filter.PageSize)
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

        public async Task<ServiceResult<ReservationDto>> CreateAsync(CreateReservationDto dto)
        {
            dto.VehiclePlate = dto.VehiclePlate.Trim().ToUpperInvariant();
            dto.VehicleType = dto.VehicleType?.Trim();

            // Validate DTO
            var (isValid, errorMessage) = ReservationValidator.Validate(dto);
            if (!isValid)
                return ServiceResult<ReservationDto>.Fail(errorMessage ?? "Dữ liệu không hợp lệ.");

<<<<<<< HEAD
            dto.VehiclePlate = dto.VehiclePlate.Trim().ToUpperInvariant();
            dto.VehicleType = dto.VehicleType?.Trim();
            dto.PreferredSlotId = string.IsNullOrWhiteSpace(dto.PreferredSlotId) ? null : dto.PreferredSlotId.Trim();
            var customerId = dto.CustomerId!.Trim();
            var vehicleType = dto.VehicleType!;
            dto.CustomerId = customerId;

=======
            var customerId = dto.CustomerId!;
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
            var customer = await _customerRepo.GetByIdAsync(customerId);
            if (customer == null)
                return ServiceResult<ReservationDto>.Fail("Không tìm thấy khách hàng.");

<<<<<<< HEAD
            var vehicle = await _vehicleRepo.GetByPlateAsync(dto.VehiclePlate);
            if (vehicle == null)
            {
                await _vehicleRepo.AddAsync(new Vehicle
                {
                    VehiclePlate = dto.VehiclePlate,
                    VehicleType = vehicleType,
                    CustomerId = customerId
                });
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(vehicle.CustomerId) &&
                    !string.Equals(vehicle.CustomerId, customerId, StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult<ReservationDto>.Fail("Biển số xe này đã thuộc khách hàng khác.");
                }

                if (!string.Equals(vehicle.VehicleType, vehicleType, StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult<ReservationDto>.Fail("Loại xe không khớp với biển số đã lưu.");
                }

                if (string.IsNullOrWhiteSpace(vehicle.CustomerId))
                {
                    vehicle.CustomerId = customerId;
                    await _vehicleRepo.UpdateAsync(vehicle);
                }
            }

            string? slotId = dto.PreferredSlotId;
            if (!string.IsNullOrEmpty(slotId))
            {
                var preferred = await _slotRepo.GetByIdAsync(slotId);
                if (preferred == null ||
                    preferred.Status != "Trống" ||
                    !string.Equals(preferred.VehicleType, vehicleType, StringComparison.OrdinalIgnoreCase))
                    slotId = null;
=======
var vehicleSyncError = await SyncReservationVehicleAsync(dto);
if (!string.IsNullOrEmpty(vehicleSyncError))
    return ServiceResult<ReservationDto>.Fail(vehicleSyncError);

string? slotId = dto.PreferredSlotId?.Trim();
if (!string.IsNullOrEmpty(slotId))
{
    var preferred = await _slotRepo.GetByIdAsync(slotId);
    if (preferred == null)
        return ServiceResult<ReservationDto>.Fail("Chỗ đỗ đã chọn không tồn tại.");

    if (!string.Equals(preferred.VehicleType, dto.VehicleType, StringComparison.OrdinalIgnoreCase))
        return ServiceResult<ReservationDto>.Fail("Chỗ đỗ đã chọn không phù hợp với loại xe.");

    if (preferred.Status != "Trống")
        return ServiceResult<ReservationDto>.Fail("Chỗ đỗ đã chọn không còn trống. Vui lòng chọn chỗ khác.");
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
            }

            if (string.IsNullOrEmpty(slotId))
            {
<<<<<<< HEAD
                var available = await _slotRepo.GetAvailableAsync(vehicleType);
=======
                var available = await _slotRepo.GetAvailableAsync(dto.VehicleType!);
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
                if (!available.Any())
                    return ServiceResult<ReservationDto>.Fail("Không còn chỗ trống cho loại xe này.");
                slotId = available.First().SlotId;
            }

            var id = await _repo.GenerateIdAsync();
            var reservation = new Reservation
            {
                ReservationId = id,
<<<<<<< HEAD
                CustomerId = customerId,
                VehiclePlate = dto.VehiclePlate,
=======
CustomerId = customerId,
VehiclePlate = dto.VehiclePlate,
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
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

        private async Task<string?> SyncReservationVehicleAsync(CreateReservationDto dto)
        {
            var vehicle = await _vehicleRepo.GetByPlateAsync(dto.VehiclePlate);
            if (vehicle == null)
            {
                await _vehicleRepo.AddAsync(new Vehicle
                {
                    VehiclePlate = dto.VehiclePlate,
                    VehicleType = dto.VehicleType!,
                    CustomerId = dto.CustomerId
                });

                return null;
            }

            if (!string.IsNullOrWhiteSpace(vehicle.CustomerId) && vehicle.CustomerId != dto.CustomerId)
            {
                if (!string.Equals(vehicle.VehicleType, dto.VehicleType, StringComparison.OrdinalIgnoreCase))
                    return "Biển số xe này đã được lưu với loại xe khác.";

                // Reservation is a one-off booking, so it can reference an existing plate
                // without taking ownership away from the customer profile that saved it.
                return null;
            }

            var changed = false;
            if (string.IsNullOrWhiteSpace(vehicle.CustomerId))
            {
                vehicle.CustomerId = dto.CustomerId;
                changed = true;
            }

            if (!string.Equals(vehicle.VehicleType, dto.VehicleType, StringComparison.OrdinalIgnoreCase))
            {
                vehicle.VehicleType = dto.VehicleType!;
                changed = true;
            }

            if (changed)
                await _vehicleRepo.UpdateAsync(vehicle);

            return null;
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

        public async Task<ServiceResult<CancelReservationResultDto>> CancelByEmployeeAsync(string reservationId)
        {
            var reservation = await _repo.GetByIdAsync(reservationId);
            if (reservation == null)
                return ServiceResult<CancelReservationResultDto>.Fail("Không tìm thấy đơn đặt chỗ.");

            if (reservation.Status == "Chờ" && reservation.ExpectedTime < DateTime.Now)
            {
                reservation.Status = "Hết hạn";
                await _repo.UpdateAsync(reservation);

                if (!string.IsNullOrEmpty(reservation.SlotId))
                    await _slotRepo.UpdateStatusAsync(reservation.SlotId, "Trống");

                return ServiceResult<CancelReservationResultDto>.Fail(
                    "Đơn đặt chỗ đã quá thời gian dự kiến, hệ thống đã chuyển sang hết hạn.");
            }

            if (reservation.Status != "Chờ")
                return ServiceResult<CancelReservationResultDto>.Fail(
                    $"Không thể hủy đơn đặt chỗ với trạng thái: {reservation.Status}");

            reservation.Status = "Hủy";
            await _repo.UpdateAsync(reservation);

            if (!string.IsNullOrEmpty(reservation.SlotId))
                await _slotRepo.UpdateStatusAsync(reservation.SlotId, "Trống");

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

        private async Task ExpireWaitingReservationsAsync(IEnumerable<Reservation> reservations)
        {
            foreach (var reservation in reservations.Where(r => r.Status == "Chờ" && r.ExpectedTime < DateTime.Now))
            {
                reservation.Status = "Hết hạn";
                await _repo.UpdateAsync(reservation);

                if (!string.IsNullOrEmpty(reservation.SlotId))
                    await _slotRepo.UpdateStatusAsync(reservation.SlotId, "Trống");
            }
        }

        private static void NormalizePaging(FilterReservationDto filter)
        {
            filter.PageNumber = Math.Max(1, filter.PageNumber);
            filter.PageSize = filter.PageSize <= 0 ? 10 : Math.Min(filter.PageSize, 100);
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
