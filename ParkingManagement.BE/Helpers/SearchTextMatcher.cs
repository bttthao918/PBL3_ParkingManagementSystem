using System.Globalization;
using System.Text;

namespace ParkingManagement.BLL.Helpers
{
    public static class SearchTextMatcher
    {
        public static bool Matches(string? keyword, params object?[] values)
        {
            var normalizedKeyword = Normalize(keyword);
            if (normalizedKeyword.Length == 0)
                return true;

            var compactKeyword = Compact(normalizedKeyword);

            return values.Any(value =>
            {
                var normalizedValue = Normalize(ToSearchText(value));
                return normalizedValue.Contains(normalizedKeyword, StringComparison.Ordinal) ||
                       (compactKeyword.Length > 0 &&
                        Compact(normalizedValue).Contains(compactKeyword, StringComparison.Ordinal));
            });
        }

        private static string ToSearchText(object? value)
        {
            return value switch
            {
                null => string.Empty,
                DateTime dateTime => string.Join(' ',
                    dateTime.ToString("dd/MM/yyyy HH:mm"),
                    dateTime.ToString("yyyy-MM-dd HH:mm"),
                    dateTime.ToString("dd/MM/yyyy")),
                DateTimeOffset dateTimeOffset => string.Join(' ',
                    dateTimeOffset.ToString("dd/MM/yyyy HH:mm"),
                    dateTimeOffset.ToString("yyyy-MM-dd HH:mm"),
                    dateTimeOffset.ToString("dd/MM/yyyy")),
                IFormattable formattable => formattable.ToString(null, CultureInfo.GetCultureInfo("vi-VN")) ?? string.Empty,
                _ => value.ToString() ?? string.Empty
            };
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var characters = value.Trim().ToLowerInvariant()
                .Normalize(NormalizationForm.FormD)
                .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                .Select(character => character == 'đ' ? 'd' : character)
                .ToArray();

            return new string(characters).Normalize(NormalizationForm.FormC);
        }

        private static string Compact(string value) =>
            new(value.Where(char.IsLetterOrDigit).ToArray());
    }
}
