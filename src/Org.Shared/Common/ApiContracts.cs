namespace Org.Shared.Common;

public sealed record ListResponse<T>(IReadOnlyList<T> Items);

public sealed record ErrorResponse(string Code, string Message, IReadOnlyList<string>? Details = null);
