namespace Org.Backend.Domain.Enums;

/// <summary>
/// Trạng thái tham dự.
/// Storage: string.
/// </summary>
public enum AttendeeStatus
{
    Registered,
    CheckInPending,
    CheckedIn,
    Cancelled,
    NoShow
}
