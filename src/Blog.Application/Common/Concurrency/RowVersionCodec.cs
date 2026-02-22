namespace Blog.Application.Common.Concurrency;

public static class RowVersionCodec
{
    public static string ToBase64(byte[] rowVersion)
    {
        return Convert.ToBase64String(rowVersion);
    }

    public static bool TryDecode(string? input, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim();

        if (normalized.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[2..];
        }

        normalized = normalized.Trim('"');

        try
        {
            bytes = Convert.FromBase64String(normalized);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string ToETag(byte[] rowVersion)
    {
        return $"\"{ToBase64(rowVersion)}\"";
    }
}
