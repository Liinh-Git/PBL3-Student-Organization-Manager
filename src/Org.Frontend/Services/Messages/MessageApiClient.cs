namespace Org.Frontend.Services.Messages;

public sealed class MessageApiClient : IMessageService
{
    private static NotSupportedException BuildNotSupported()
        => new("Messaging API/Hub chưa sẵn sàng ở backend. Vui lòng bật mock mode hoặc triển khai API chat trước.");

    public Task<IReadOnlyList<ConversationListItem>> GetConversationsAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<IReadOnlyList<MessageableUserItem>> GetMessageableUsersAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<ConversationOpenResult> GetOrCreateDirectConversationAsync(Guid otherUserId, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<IReadOnlyList<MessageItem>> GetMessagesAsync(Guid conversationId, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<MessageItem> SendMessageAsync(Guid conversationId, string content, CancellationToken ct = default)
        => throw BuildNotSupported();
}
