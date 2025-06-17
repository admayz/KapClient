using System.Globalization;
using System.Text.Json;

namespace KapClient.Extender
{
    public static class ValueExtender
    {
        private static readonly string[] DateFormats = { "dd/MM/yyyy HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd" };

        public static object? NormalizeValue(object? rawValue)
        {
            if (rawValue == null)
                return null;

            if (rawValue is string s)
            {
                if (TryParseDate(s, out var dt))
                    return dt;
                return s;
            }

            if (rawValue is JsonElement jsonElement)
            {
                return NormalizeJsonElement(jsonElement);
            }

            return rawValue;
        }

        private static bool TryParseDate(string s, out DateTime dt)
        {
            return DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                   || DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
        }

        private static object? NormalizeJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    var str = element.GetString();
                    if (str != null && TryParseDate(str, out var dt))
                        return dt;
                    return str;

                case JsonValueKind.Array:
                    {
                        var arr = element.EnumerateArray();
                        var list = new List<object?>(element.GetArrayLength());
                        foreach (var item in arr)
                        {
                            list.Add(NormalizeJsonElement(item));
                        }
                        return list;
                    }

                case JsonValueKind.Object:
                    {
                        var obj = new Dictionary<string, object?>(element.GetRawText().Length / 10);
                        foreach (var prop in element.EnumerateObject())
                        {
                            obj[prop.Name] = NormalizeJsonElement(prop.Value);
                        }
                        return obj;
                    }

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long l))
                        return l;
                    if (element.TryGetDouble(out double d))
                        return d;
                    return element.GetRawText();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;

                default:
                    return element.GetRawText();
            }
        }
    }
}