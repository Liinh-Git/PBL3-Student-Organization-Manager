namespace Org.Backend.Domain.Enums;

/// <summary>
/// Trạng thái resource.
/// Storage: string.
/// </summary>
public enum ResourceStatus
{
    Available,
    Reserved,
    InUse,
    Maintenance,
    Unavailable,
    Lost
}
