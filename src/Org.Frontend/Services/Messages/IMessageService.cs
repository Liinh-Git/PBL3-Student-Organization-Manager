namespace Org.Frontend.Services.Messages;

public interface IMessageService
{
    Task<IReadOnlyList<ConversationListItem>> GetConversationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MessageableUserItem>> GetMessageableUsersAsync(CancellationToken ct = default);
    Task<ConversationOpenResult> GetOrCreateDirectConversationAsync(Guid otherUserId, CancellationToken ct = default);
    Task<IReadOnlyList<MessageItem>> GetMessagesAsync(Guid conversationId, CancellationToken ct = default);
    Task<MessageItem> SendMessageAsync(Guid conversationId, string content, CancellationToken ct = default);
}

public sealed record ConversationListItem(
    Guid ConversationId,
    string Title,
    bool IsGroup,
    string? AvatarUrl,
    string? Subtitle,
    Guid? DirectUserId,
    string LastMessagePreview,
    DateTime LastMessageAt);

public sealed record MessageableUserItem(
    Guid UserId,
    string FullName,
    string? AvatarUrl,
    string? Subtitle,
    bool HasConversation,
    Guid? ExistingConversationId);

public sealed record ConversationOpenResult(
    Guid ConversationId,
    bool CreatedNew);

public sealed record MessageItem(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string? SenderAvatarUrl,
    string Content,
    DateTime SentAt,
    string MessageType,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<Guid> ReadByUserIds);
