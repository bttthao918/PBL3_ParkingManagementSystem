using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Validators
{
    /// <summary>
    /// Validator cho Check-in logic
    /// Tách riêng validation theo SRP (Single Responsibility Principle)
    /// </summary>
    public interface ICheckInValidator
    {
        /// <summary>
        /// Kiểm tra dữ liệu đầu vào có hợp lệ
        /// </summary>
        ValidationResult ValidateInput(CheckInInputDto input);

        /// <summary>
        /// Kiểm tra biển số xe có hợp lệ
        /// </summary>
        ValidationResult ValidateVehiclePlate(string vehiclePlate);

        /// <summary>
        /// Kiểm tra loại xe có hợp lệ
        /// </summary>
        ValidationResult ValidateVehicleType(string vehicleType);
    }

    /// <summary>
    /// Result của validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
    }

    /// <summary>
    /// Implementation của CheckInValidator
    /// </summary>
    public class CheckInValidator : ICheckInValidator
    {
        private static readonly List<string> ValidVehicleTypes = new()
        {
            "Xe máy",
            "Ô tô nhỏ",
            "Ô tô lớn"
        };

        // Regex cho biển số xe Việt Nam: XX-XXX.XX (vd: 43A-123.45)
        private const string VehiclePlatePattern = @"^\d{2}[A-Z]-\d{3}\.\d{2}$";

        public ValidationResult ValidateInput(CheckInInputDto input)
        {
            if (string.IsNullOrWhiteSpace(input.VehiclePlate))
                return ValidationResult.Fail("Vui lòng nhập biển số xe.");

            if (string.IsNullOrWhiteSpace(input.VehicleType))
                return ValidationResult.Fail("Vui lòng chọn loại xe.");

            var plateValidation = ValidateVehiclePlate(input.VehiclePlate);
            if (!plateValidation.IsValid)
                return plateValidation;

            var typeValidation = ValidateVehicleType(input.VehicleType);
            if (!typeValidation.IsValid)
                return typeValidation;

            return ValidationResult.Success();
        }

        public ValidationResult ValidateVehiclePlate(string vehiclePlate)
        {
            if (string.IsNullOrWhiteSpace(vehiclePlate))
                return ValidationResult.Fail("Biển số xe không được để trống.");

            var plate = vehiclePlate.Trim().ToUpper();

            // Kiểm tra format biển số (VD: 43A-123.45)
            if (!System.Text.RegularExpressions.Regex.IsMatch(plate, VehiclePlatePattern))
                return ValidationResult.Fail("Biển số xe không hợp lệ. Định dạng: XX-XXX.XX (vd: 43A-123.45)");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateVehicleType(string vehicleType)
        {
            if (string.IsNullOrWhiteSpace(vehicleType))
                return ValidationResult.Fail("Loại xe không được để trống.");

            if (!ValidVehicleTypes.Contains(vehicleType.Trim()))
                return ValidationResult.Fail($"Loại xe không hợp lệ. Chỉ chấp nhận: {string.Join(", ", ValidVehicleTypes)}");

            return ValidationResult.Success();
        }
    }
}
