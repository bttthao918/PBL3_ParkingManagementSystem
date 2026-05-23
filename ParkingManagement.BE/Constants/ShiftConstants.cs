using System.Globalization;
using System.Text;

namespace ParkingManagement.BLL.Constants
{
    public static class ShiftConstants
    {
        public const string Morning = "Sáng";
        public const string Afternoon = "Chiều";
        public const string Evening = "Tối";

        public const string ScheduledStatus = "Đã lên lịch";
        public const string WorkingStatus = "Đang làm";
        public const string CompletedStatus = "Hoàn thành";
        public const string AbsentStatus = "Vắng";

        private static readonly Dictionary<string, (TimeSpan Start, TimeSpan End)> DefaultShifts = new()
        {
            [Morning] = (new TimeSpan(7, 0, 0), new TimeSpan(12, 0, 0)),
            [Afternoon] = (new TimeSpan(12, 0, 0), new TimeSpan(17, 0, 0)),
            [Evening] = (new TimeSpan(17, 0, 0), new TimeSpan(22, 0, 0))
        };

        public static bool TryGetShiftWindow(string? shiftType, out string normalizedShiftType, out TimeSpan start, out TimeSpan end)
        {
            normalizedShiftType = NormalizeShiftType(shiftType);
            if (DefaultShifts.TryGetValue(normalizedShiftType, out var window))
            {
                start = window.Start;
                end = window.End;
                return true;
            }

            start = default;
            end = default;
            return false;
        }

        public static (TimeSpan Start, TimeSpan End) GetEffectiveWindow(string? shiftType, TimeSpan storedStart, TimeSpan storedEnd)
        {
            return TryGetShiftWindow(shiftType, out _, out var start, out var end)
                ? (start, end)
                : (storedStart, storedEnd);
        }

        public static bool IsWithinShift(TimeSpan currentTime, TimeSpan start, TimeSpan end)
        {
            if (start <= end)
                return currentTime >= start && currentTime < end;

            return currentTime >= start || currentTime < end;
        }

        public static bool AllowsEmployeeOperation(string? status)
        {
            return string.Equals(status, ScheduledStatus, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, WorkingStatus, StringComparison.OrdinalIgnoreCase);
        }

        public static string FormatWindow(TimeSpan start, TimeSpan end)
        {
            return $"{start:hh\\:mm}-{end:hh\\:mm}";
        }

        private static string NormalizeShiftType(string? shiftType)
        {
            var normalized = RemoveDiacritics(shiftType ?? string.Empty).Trim().ToLowerInvariant();

            if (normalized.Contains("sang"))
                return Morning;

            if (normalized.Contains("chieu") || normalized.Contains("chi"))
                return Afternoon;

            if (normalized.Contains("toi") || normalized.Contains("to"))
                return Evening;

            return shiftType?.Trim() ?? string.Empty;
        }

        private static string RemoveDiacritics(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
