using ParkingManagement.BLL.DTOs;
using System.Text.RegularExpressions;

namespace ParkingManagement.BLL.Validators
{
    public class MonthlyTicketValidator
    {
        public static (bool isValid, string? errorMessage) Validate(RegisterMonthlyTicketDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerId))
                return (false, "CustomerId không được để trống.");

            if (string.IsNullOrWhiteSpace(dto.VehiclePlate))
                return (false, "Biển số xe không được để trống.");

            if (dto.VehiclePlate.Length > 20)
                return (false, "Biển số xe tối đa 20 ký tự.");

            if (!IsValidVehiclePlate(dto.VehiclePlate))
                return (false, "Biển số xe không hợp lệ. Định dạng: 43A-123.45");

            if (string.IsNullOrWhiteSpace(dto.VehicleType))
                return (false, "Loại xe không được để trống.");

            var validTypes = new[] { "Xe máy", "Ô tô nhỏ", "Ô tô lớn" };
            if (!validTypes.Contains(dto.VehicleType))
                return (false, "Loại xe không hợp lệ. Vui lòng chọn: Xe máy, Ô tô nhỏ, Ô tô lớn.");

            if (string.IsNullOrWhiteSpace(dto.PackageType))
                return (false, "Gói vé tháng không được để trống.");

            var validPackages = new[] { "1 tháng", "3 tháng", "6 tháng" };
            if (!validPackages.Contains(dto.PackageType))
                return (false, "Gói vé tháng không hợp lệ. Vui lòng chọn: 1 tháng, 3 tháng, 6 tháng.");

            if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
                return (false, "Phương thức thanh toán không được để trống.");

            var validMethods = new[] { "Chuyển khoản", "Ví điện tử" };
            if (!validMethods.Contains(dto.PaymentMethod))
                return (false, "Vé tháng chỉ hỗ trợ: Chuyển khoản, Ví điện tử. Không hỗ trợ thanh toán tiền mặt.");

            return (true, null);
        }

        private static bool IsValidVehiclePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return false;

            return Regex.IsMatch(plate.Trim().ToUpperInvariant(), @"^\d{2}[A-Z]-\d{3}\.\d{2}$");
        }
    }
}
