using System.Text;

namespace CurrencyApp.Application.Common.Pagination;

public static class CursorTokenCodec
{
    public static string Encode(int id)
    {
        var raw = Encoding.UTF8.GetBytes(id.ToString());
        var base64 = Convert.ToBase64String(raw);

        return base64
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static bool TryDecode(string token, out int id)
    {
        id = 0;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        var normalized = token
            .Trim()
            .Replace('-', '+')
            .Replace('_', '/');

        var remainder = normalized.Length % 4;
        if (remainder > 0)
            normalized = normalized.PadRight(normalized.Length + (4 - remainder), '=');

        try
        {
            var bytes = Convert.FromBase64String(normalized);
            var value = Encoding.UTF8.GetString(bytes);
            return int.TryParse(value, out id);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
