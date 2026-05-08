using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Validators
{
    /// <summary>
    /// Validator cho Parking Slot logic
    /// UC005.2 - Cấp nhật trạng thái chỗ đỗ
    /// </summary>
    public interface IParkingSlotValidator
    {
        /// <summary>
        /// Kiểm tra UpdateSlotStatusDto có hợp lệ
        /// </summary>
        ValidationResult ValidateUpdateSlotStatus(UpdateSlotStatusDto dto);

        /// <summary>
        /// Kiểm tra ID chỗ đỗ
        /// </summary>
        ValidationResult ValidateSlotId(string slotId);

        /// <summary>
        /// Kiểm tra trạng thái chỗ đỗ
        /// </summary>
        ValidationResult ValidateSlotStatus(string status);

        /// <summary>
        /// Kiểm tra có thể chuyển sang trạng thái mới không
        /// </summary>
        ValidationResult ValidateStatusTransition(string currentStatus, string newStatus);

        /// <summary>
        /// Kiểm tra ghi chú (nếu cần)
        /// </summary>
        ValidationResult ValidateNote(string? note, string targetStatus);
    }

    /// <summary>
    /// Implementation của ParkingSlotValidator
    /// </summary>
    public class ParkingSlotValidator : IParkingSlotValidator
    {
        // Valid statuses cho chỗ đỗ
        private static readonly List<string> ValidStatuses = new()
        {
            "Trống",              // Empty
            "Đang sử dụng",       // In use
            "Đã đặt",             // Reserved
            "Bảo trì",            // Maintenance
            "Hỏng"                // Broken
        };

        // Transitions mà không cần ghi chú
        private static readonly List<(string from, string to)> NoNoteAllowed = new()
        {
            ("Trống", "Đang sử dụng"),
            ("Đang sử dụng", "Trống"),
            ("Trống", "Đã đặt"),
            ("Đã đặt", "Trống")
        };

        public ValidationResult ValidateUpdateSlotStatus(UpdateSlotStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SlotId))
                return ValidationResult.Fail("ID chỗ đỗ không được để trống");

            var slotValidation = ValidateSlotId(dto.SlotId);
            if (!slotValidation.IsValid)
                return slotValidation;

            if (string.IsNullOrWhiteSpace(dto.NewStatus))
                return ValidationResult.Fail("Trạng thái mới không được để trống");

            var statusValidation = ValidateSlotStatus(dto.NewStatus);
            if (!statusValidation.IsValid)
                return statusValidation;

            return ValidationResult.Success();
        }

        public ValidationResult ValidateSlotId(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId))
                return ValidationResult.Fail("ID chỗ đỗ không được để trống");

            // Format: A01, B05, C10, etc.
            if (!System.Text.RegularExpressions.Regex.IsMatch(slotId, @"^[A-Z]\d{2}$"))
                return ValidationResult.Fail("Format ID chỗ đỗ không hợp lệ (vd: A01, B05)");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateSlotStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return ValidationResult.Fail("Trạng thái không được để trống");

            if (!ValidStatuses.Contains(status))
                return ValidationResult.Fail($"Trạng thái '{status}' không hợp lệ. Chỉ chấp nhận: {string.Join(", ", ValidStatuses)}");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateStatusTransition(string currentStatus, string newStatus)
        {
            // Kiểm tra trạng thái hiện tại
            var currentValidation = ValidateSlotStatus(currentStatus);
            if (!currentValidation.IsValid)
                return currentValidation;

            // Kiểm tra trạng thái mới
            var newValidation = ValidateSlotStatus(newStatus);
            if (!newValidation.IsValid)
                return newValidation;

            // Không cho phép chuyển sang trạng thái giống hiện tại
            if (currentStatus == newStatus)
                return ValidationResult.Fail($"Trạng thái hiện tại đã là '{newStatus}'");

            // Kiểm tra transition hợp lệ
            // Ví dụ: Từ "Đang sử dụng" không thể chuyển sang "Đã đặt" trực tiếp
            // Phải về "Trống" trước
            if (currentStatus == "Đang sử dụng" && newStatus == "Đã đặt")
                return ValidationResult.Fail("Không thể chuyển từ 'Đang sử dụng' sang 'Đã đặt'. Cần về 'Trống' trước.");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateNote(string? note, string targetStatus)
        {
            // Nếu chuyển sang "Bảo trì" hoặc "Hỏng", bắt buộc phải có ghi chú
            if ((targetStatus == "Bảo trì" || targetStatus == "Hỏng") && string.IsNullOrWhiteSpace(note))
                return ValidationResult.Fail($"Vui lòng nhập lý do khi chuyển sang trạng thái '{targetStatus}'");

            // Nếu có ghi chú, kiểm tra độ dài
            if (!string.IsNullOrWhiteSpace(note) && note.Length > 500)
                return ValidationResult.Fail("Ghi chú không được quá 500 ký tự");

            return ValidationResult.Success();
        }
    }
}
