using ParkingManagement.BLL.DTOs;
using System.Text.RegularExpressions;

namespace ParkingManagement.BLL.Validators
{
    /// <summary>
    /// Validator cho RegisterMonthlyTicketDto (UC007.1 - Đăng Ký Vé Tháng)
    /// </summary>
    public class MonthlyTicketValidator
    {
        public static (bool isValid, string? errorMessage) Validate(RegisterMonthlyTicketDto dto)
        {
            // Kiểm tra CustomerId
            if (string.IsNullOrWhiteSpace(dto.CustomerId))
                return (false, "CustomerId không được để trống.");

            // Kiểm tra VehiclePlate
            if (string.IsNullOrWhiteSpace(dto.VehiclePlate))
                return (false, "Biển số xe không được để trống.");

            if (dto.VehiclePlate.Length > 20)
                return (false, "Biển số xe tối đa 20 ký tự.");

            // Validate format biển số (VD: 43A-123.45)
            if (!IsValidVehiclePlate(dto.VehiclePlate))
                return (false, "Biển số xe không hợp lệ. Định dạng: 43A-123.45");

            // Kiểm tra VehicleType
            if (string.IsNullOrWhiteSpace(dto.VehicleType))
                return (false, "Loại xe không được để trống.");

            var validTypes = new[] { "Xe máy", "Ô tô nhỏ", "Ô tô lớn" };
            if (!validTypes.Contains(dto.VehicleType))
                return (false, "Loại xe không hợp lệ. Vui lòng chọn: Xe máy, Ô tô nhỏ, Ô tô lớn.");

            // Kiểm tra PackageType
            if (string.IsNullOrWhiteSpace(dto.PackageType))
                return (false, "Gói vé tháng không được để trống.");

            var validPackages = new[] { "1 tháng", "3 tháng", "6 tháng" };
            if (!validPackages.Contains(dto.PackageType))
                return (false, "Gói vé tháng không hợp lệ. Vui lòng chọn: 1 tháng, 3 tháng, 6 tháng.");

            // Kiểm tra PaymentMethod
            if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
                return (false, "Phương thức thanh toán không được để trống.");

            // MonthlyTicket chỉ hỗ trợ: Chuyển khoản & Ví điện tử
            // Không hỗ trợ: Tiền mặt
            var validMethods = new[] { "Chuyển khoản", "Ví điện tử" };
            if (!validMethods.Contains(dto.PaymentMethod))
                return (false, "Vé tháng chỉ hỗ trợ: Chuyển khoản, Ví điện tử. Không hỗ trợ thanh toán tiền mặt.");

            return (true, null);
        }

        /// <summary>
        /// Validate format biển số xe (VD: 43A-123.45)
        /// </summary>
        private static bool IsValidVehiclePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return false;

            // Format: XX[A-Z]-\d{3}\.\d{2}
            // VD: 43A-123.45, 51B-456.78
            var pattern = @"^\d{2}[A-Z]-\d{3}\.\d{2}$";
            return Regex.IsMatch(plate, pattern);
        }
    }
}
