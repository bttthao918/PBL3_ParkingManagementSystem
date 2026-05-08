using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Validators
{
    /// <summary>
    /// Validator cho CreateReservationDto (UC005.1 - Đặt chỗ gửi xe mới)
    /// </summary>
    public class ReservationValidator
    {
        public static (bool isValid, string? errorMessage) Validate(CreateReservationDto dto)
        {
            // Kiểm tra CustomerId
            if (string.IsNullOrWhiteSpace(dto.CustomerId))
                return (false, "CustomerId không được để trống.");

            // Kiểm tra VehiclePlate
            if (string.IsNullOrWhiteSpace(dto.VehiclePlate))
                return (false, "Biển số xe không được để trống.");

            if (dto.VehiclePlate.Length > 20)
                return (false, "Biển số xe tối đa 20 ký tự.");

            // Kiểm tra VehicleType
            if (string.IsNullOrWhiteSpace(dto.VehicleType))
                return (false, "Loại xe không được để trống.");

            var validTypes = new[] { "Xe máy", "Ô tô nhỏ", "Ô tô lớn" };
            if (!validTypes.Contains(dto.VehicleType))
                return (false, "Loại xe không hợp lệ. Vui lòng chọn: Xe máy, Ô tô nhỏ, Ô tô lớn.");

            // Kiểm tra ExpectedTime
            if (dto.ExpectedTime == default)
                return (false, "Thời gian đặt chỗ không hợp lệ.");

            if (dto.ExpectedTime < DateTime.Now)
                return (false, "Thời gian đặt chỗ phải từ hiện tại trở đi.");

            // Kiểm tra PreferredSlotId (nếu có)
            if (!string.IsNullOrWhiteSpace(dto.PreferredSlotId) && dto.PreferredSlotId.Length > 20)
                return (false, "SlotId tối đa 20 ký tự.");

            return (true, null);
        }
    }
}
