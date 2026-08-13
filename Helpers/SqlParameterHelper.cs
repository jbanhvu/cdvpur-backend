using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Helpers;

public static class SqlParameterHelper
{
    public static void SetInt(string name, Dictionary<string, JsonElement> body, int value)
    {
        string? key = body.Keys.FirstOrDefault(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase));
        if (key is not null)
        {
            body.Remove(key);
        }

        body[name] = JsonSerializer.SerializeToElement(value);
    }

    public static SqlParameter Int(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
        {
            return new SqlParameter($"@{name}", number);
        }

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
        {
            return new SqlParameter($"@{name}", number);
        }

        throw new ArgumentException($"Field '{name}' must be an integer.");
    }

    public static SqlParameter NullableInt(string name, IReadOnlyDictionary<string, JsonElement> body, int? defaultValue = null)
    {
        if (!TryGetValue(name, body, out JsonElement value) || IsEmptyValue(value))
        {
            return new SqlParameter($"@{name}", defaultValue ?? (object)DBNull.Value);
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
        {
            return new SqlParameter($"@{name}", number);
        }

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
        {
            return new SqlParameter($"@{name}", number);
        }

        throw new ArgumentException($"Field '{name}' must be an integer.");
    }

    public static SqlParameter BigInt(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long number))
        {
            return new SqlParameter($"@{name}", number);
        }

        if (value.ValueKind == JsonValueKind.String &&
            long.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
        {
            return new SqlParameter($"@{name}", number);
        }

        throw new ArgumentException($"Field '{name}' must be a bigint.");
    }

    public static SqlParameter String(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        object result = value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
            ? DBNull.Value
            : value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();

        return new SqlParameter($"@{name}", result);
    }

    public static SqlParameter NullableString(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        if (!TryGetValue(name, body, out JsonElement value) || IsEmptyValue(value))
        {
            return new SqlParameter($"@{name}", DBNull.Value);
        }

        object result = value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? (object)DBNull.Value
            : value.ToString();

        return new SqlParameter($"@{name}", result);
    }

    public static SqlParameter Decimal(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out decimal number))
        {
            return new SqlParameter($"@{name}", number);
        }

        if (value.ValueKind == JsonValueKind.String &&
            decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number))
        {
            return new SqlParameter($"@{name}", number);
        }

        throw new ArgumentException($"Field '{name}' must be a decimal number.");
    }

    public static SqlParameter NullableDecimal(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        if (!TryGetValue(name, body, out JsonElement value) || IsEmptyValue(value))
        {
            return new SqlParameter($"@{name}", DBNull.Value);
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out decimal number))
        {
            return new SqlParameter($"@{name}", number);
        }

        if (value.ValueKind == JsonValueKind.String &&
            decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number))
        {
            return new SqlParameter($"@{name}", number);
        }

        throw new ArgumentException($"Field '{name}' must be a decimal number.");
    }

    public static SqlParameter Bool(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            return new SqlParameter($"@{name}", value.GetBoolean());
        }

        if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool boolean))
        {
            return new SqlParameter($"@{name}", boolean);
        }

        throw new ArgumentException($"Field '{name}' must be a boolean.");
    }

    public static SqlParameter NullableBool(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        if (!TryGetValue(name, body, out JsonElement value) || IsEmptyValue(value))
        {
            return new SqlParameter($"@{name}", DBNull.Value);
        }

        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            return new SqlParameter($"@{name}", value.GetBoolean());
        }

        if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool boolean))
        {
            return new SqlParameter($"@{name}", boolean);
        }

        throw new ArgumentException($"Field '{name}' must be a boolean.");
    }

    public static SqlParameter Date(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        string text = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
        if (System.DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out System.DateTime date))
        {
            return new SqlParameter($"@{name}", date.Date);
        }

        throw new ArgumentException($"Field '{name}' must be a date.");
    }

    public static SqlParameter NullableDate(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        if (!TryGetValue(name, body, out JsonElement value) || IsEmptyValue(value))
        {
            return new SqlParameter($"@{name}", DBNull.Value);
        }

        string text = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
        if (System.DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out System.DateTime date))
        {
            return new SqlParameter($"@{name}", date.Date);
        }

        throw new ArgumentException($"Field '{name}' must be a date.");
    }

    public static SqlParameter DateTime(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        string text = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
        if (System.DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out System.DateTime dateTime))
        {
            return new SqlParameter($"@{name}", dateTime);
        }

        throw new ArgumentException($"Field '{name}' must be a date and time.");
    }

    public static SqlParameter NullableDateTime(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        if (!TryGetValue(name, body, out JsonElement value) || IsEmptyValue(value))
        {
            return new SqlParameter($"@{name}", DBNull.Value);
        }

        string text = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
        if (System.DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out System.DateTime dateTime))
        {
            return new SqlParameter($"@{name}", dateTime);
        }

        throw new ArgumentException($"Field '{name}' must be a date and time.");
    }

    public static SqlParameter Time(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        JsonElement value = GetValue(name, body);
        string text = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
        if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out TimeSpan time))
        {
            return new SqlParameter($"@{name}", time);
        }

        throw new ArgumentException($"Field '{name}' must be a time.");
    }

    public static SqlParameter NullableTime(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        if (!TryGetValue(name, body, out JsonElement value) || IsEmptyValue(value))
        {
            return new SqlParameter($"@{name}", DBNull.Value);
        }

        string text = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
        if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out TimeSpan time))
        {
            return new SqlParameter($"@{name}", time);
        }

        throw new ArgumentException($"Field '{name}' must be a time.");
    }

    private static JsonElement GetValue(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        if (TryGetValue(name, body, out JsonElement value))
        {
            return value;
        }

        throw new ArgumentException($"Missing required field '{name}'.");
    }

    private static bool TryGetValue(string name, IReadOnlyDictionary<string, JsonElement> body, out JsonElement value)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                value = item.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool IsEmptyValue(JsonElement value)
    {
        return value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
            value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString());
    }
}
