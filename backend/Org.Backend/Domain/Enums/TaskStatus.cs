namespace Org.Backend.Domain.Enums;

/// <summary>
/// Trạng thái task.
/// Storage: string.
/// </summary>
public enum TaskStatus
{
    Todo,
    InProgress,
    Blocked,
    Done,
    Cancelled
}
