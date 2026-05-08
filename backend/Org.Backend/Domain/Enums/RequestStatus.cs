namespace Org.Backend.Domain.Enums;

/// <summary>
/// Trạng thái review request.
/// Storage: string.
/// </summary>
public enum RequestStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled,
    Closed
}
