using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Interfaces;
using ParkingManagement.DAL.Entities;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.BLL.Services;

public class ParkingService(IUnitOfWork uow) : IParkingService
{
    // ──────────────────────────────────────────────────────────────────────────
    // Bảng giá (đồng/giờ)
    // ──────────────────────────────────────────────────────────────────────────
    private static readonly Dictionary<VehicleType, decimal> _pricePerHour = new()
    {
        [VehicleType.Motorcycle] = 5_000m,
        [VehicleType.Bicycle]    = 5_000m,
        [VehicleType.Car]        = 20_000m,
        [VehicleType.Truck]      = 20_000m,
    };

    // ──────────────────────────────────────────────────────────────────────────
    // CheckIn
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<CheckInResult> CheckInAsync(
        string licensePlate,
        string vehicleType,
        int areaId)
    {
        // 1. Validate biển số
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new ArgumentException("Biển số xe không được để trống.", nameof(licensePlate));

        // 2. Parse loại xe
        if (!Enum.TryParse<VehicleType>(vehicleType, ignoreCase: true, out var parsedType))
            throw new ArgumentException($"Loại xe '{vehicleType}' không hợp lệ.", nameof(vehicleType));

        // 3. Kiểm tra khu bãi
        var areaRepo = uow.Repository<ParkingArea>();
        var area = await areaRepo.GetByIdAsync(areaId)
            ?? throw new InvalidOperationException($"Không tìm thấy khu bãi với ID = {areaId}.");

        if (area.AvailableSlots <= 0)
            throw new InvalidOperationException($"Khu '{area.Name}' đã hết chỗ trống.");

        // 4. Tìm hoặc tạo Vehicle
        var vehicleRepo = uow.Repository<Vehicle>();
        var vehicle = await vehicleRepo.FirstOrDefaultAsync(
            v => v.LicensePlate == licensePlate.ToUpper().Trim());

        if (vehicle is null)
        {
            vehicle = new Vehicle
            {
                LicensePlate = licensePlate.ToUpper().Trim(),
                VehicleType  = parsedType
            };
            await vehicleRepo.AddAsync(vehicle);
            await uow.SaveChangesAsync(); // cần Id của vehicle cho Ticket
        }

        // 5. Kiểm tra xe chưa đang gửi (tránh checkin 2 lần)
        var ticketRepo = uow.Repository<Ticket>();
        var existing = await ticketRepo.FirstOrDefaultAsync(
            t => t.VehicleId == vehicle.Id && t.Status == TicketStatus.Active);

        if (existing is not null)
            throw new InvalidOperationException(
                $"Xe '{licensePlate}' đang có vé gửi chưa checkout (Ticket #{existing.Id}).");

        // 6. Tạo Ticket mới
        var ticket = new Ticket
        {
            VehicleId     = vehicle.Id,
            ParkingAreaId = area.Id,
            EntryTime     = DateTime.Now,
            Status        = TicketStatus.Active
        };
        await ticketRepo.AddAsync(ticket);

        // 7. Giảm số chỗ trống
        area.AvailableSlots--;
        areaRepo.Update(area);

        await uow.SaveChangesAsync();

        return new CheckInResult(ticket.Id, vehicle.LicensePlate, area.Name, ticket.EntryTime);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CheckOut
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<CheckOutResult> CheckOutAsync(int ticketId)
    {
        // 1. Tìm Ticket (kèm Vehicle và ParkingArea)
        var ticketRepo = uow.Repository<Ticket>();
        var tickets = await ticketRepo.FindAsync(
            t => t.Id == ticketId);

        var ticket = tickets.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Không tìm thấy Ticket với ID = {ticketId}.");

        if (ticket.Status != TicketStatus.Active)
            throw new InvalidOperationException(
                $"Ticket #{ticketId} đang ở trạng thái '{ticket.Status}', không thể checkout.");

        // 2. Tải Vehicle và ParkingArea
        var vehicle = await uow.Repository<Vehicle>().GetByIdAsync(ticket.VehicleId)
            ?? throw new InvalidOperationException("Không tìm thấy thông tin xe.");

        var area = await uow.Repository<ParkingArea>().GetByIdAsync(ticket.ParkingAreaId)
            ?? throw new InvalidOperationException("Không tìm thấy thông tin khu bãi.");

        // 3. Tính thời gian và số tiền
        var exitTime     = DateTime.Now;
        var rawHours     = (exitTime - ticket.EntryTime).TotalHours;
        var billedHours  = (int)Math.Ceiling(rawHours == 0 ? 1 : rawHours); // tối thiểu 1 giờ
        var rate         = _pricePerHour.GetValueOrDefault(vehicle.VehicleType, 5_000m);
        var price        = billedHours * rate;

        // 4. Cập nhật Ticket
        ticket.ExitTime = exitTime;
        ticket.Price    = price;
        ticket.Status   = TicketStatus.Completed;
        ticketRepo.Update(ticket);

        // 5. Tăng số chỗ trống
        area.AvailableSlots++;
        uow.Repository<ParkingArea>().Update(area);

        await uow.SaveChangesAsync();

        return new CheckOutResult(
            ticket.Id,
            vehicle.LicensePlate,
            area.Name,
            ticket.EntryTime,
            exitTime,
            billedHours,
            price);
    }
}
