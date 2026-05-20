namespace Org.Backend.Domain.Enums;

/// <summary>
/// Hierarchy canonical/default mapping, không phải role custom của Role.
/// Storage: logic enum only trong v1; nếu sau này cần persist riêng thì dùng string.
/// </summary>
public enum MemberRole
{
    Member,
    Manager,
    VicePresident,
    President
}
