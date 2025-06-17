using System.Globalization;

namespace KapClient.Extender
{
    public static class DatetimeExtender
    {
        public static DateTime? ToNullableDateTime(this string? input, string format = "dd/MM/yyyy HH:mm:ss")
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (DateTime.TryParseExact(
                    input,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
            {
                return result;
            }

            return null;
        }
    }
}