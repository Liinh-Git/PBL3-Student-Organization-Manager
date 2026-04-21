// ---- Helper validate tên và các tham số đầu vào cho module sự kiện ----
namespace Org.Backend.Features.Events;

internal static class EventValidation
{
    // ---- Chuẩn hóa tên sự kiện: bỏ khoảng trắng đầu/cuối, null nếu < 2 ký tự ----
    public static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        return normalized.Length >= 2 ? normalized : null;
    }

    // ---- Chuẩn hóa trường tùy chọn: null nếu rỗng, trim khoảng trắng nếu có ----
    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
