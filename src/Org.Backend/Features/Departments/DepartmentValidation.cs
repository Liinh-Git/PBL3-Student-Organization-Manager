namespace Org.Backend.Features.Departments;

internal static class DepartmentValidation
{
    public static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        return normalized.Length >= 2 ? normalized : null;
    }

    public static string NormalizeCode(string? code, string? fallbackName)
    {
        var source = string.IsNullOrWhiteSpace(code) ? fallbackName : code;

        if (string.IsNullOrWhiteSpace(source))
            return "DEPT";

        var compact = new string(source.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(compact))
            return "DEPT";

        var normalized = compact.Length <= 8 ? compact : compact[..8];
        return normalized.ToUpperInvariant();
    }

    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static int? ParsePositiveInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value, out var parsed) && parsed > 0
            ? parsed
            : null;
    }

    public static bool? ParseNullableBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToLowerInvariant() switch
        {
            "true"  or "1" => true,
            "false" or "0" => false,
            _              => null
        };
    }
}
