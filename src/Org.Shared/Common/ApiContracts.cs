// ---- Kiểu phản hồi chung cho danh sách và lỗi API ----
namespace Org.Shared.Common;

// ---- Wrapper cho phản hồi danh sách: bọc Items vào object để FE dễ xử lý ----
public sealed record ListResponse<T>(IReadOnlyList<T> Items);

// ---- Phản hồi lỗi chuẩn: Code (mã lỗi), Message (mô tả), Details (chi tiết bổ sung nếu có) ----
public sealed record ErrorResponse(string Code, string Message, IReadOnlyList<string>? Details = null);
