using ParkingManagement.BLL.DTOs;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.BLL.Strategies
{
    /// <summary>
    /// Strategy Pattern: Tìm chỗ trống theo chiến lược
    /// Giúp dễ mở rộng nếu sau này có chiến lược tìm chỗ khác (VD: ưu tiên gần cửa, ...)
    /// </summary>
    public interface IParkingSlotStrategy
    {
        /// <summary>
        /// Tìm chỗ trống phù hợp
        /// </summary>
        Task<List<AvailableSlotDto>> FindAvailableSlotsAsync(string vehicleType);
    }

    /// <summary>
    /// Chiến lược mặc định: Tìm chỗ trống đầu tiên
    /// </summary>
    public class DefaultParkingSlotStrategy : IParkingSlotStrategy
    {
        private readonly DAL.Interfaces.IParkingSlotRepository _slotRepo;

        public DefaultParkingSlotStrategy(DAL.Interfaces.IParkingSlotRepository slotRepo)
        {
            _slotRepo = slotRepo;
        }

        public async Task<List<AvailableSlotDto>> FindAvailableSlotsAsync(string vehicleType)
        {
            // Lấy tất cả chỗ trống phù hợp loại xe
            var availableSlots = await _slotRepo.GetAvailableAsync(vehicleType);

            // Chuyển đổi sang DTO
            return availableSlots.Select(s => new AvailableSlotDto
            {
                SlotId = s.SlotId,
                Location = s.Location,
                VehicleType = s.VehicleType
            }).ToList();
        }
    }
}
