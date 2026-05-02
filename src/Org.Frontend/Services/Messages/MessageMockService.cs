using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;

namespace Org.Frontend.Services.Messages;

public sealed class MessageMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IMessageService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<IReadOnlyList<ConversationListItem>> GetConversationsAsync(CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            var joinedConversationIds = data.ConversationParticipants
                .Where(x => x.UserId == currentUserId)
                .Select(x => x.ConversationId)
                .ToHashSet();

            var result = new List<ConversationListItem>();
            foreach (var conversation in data.Conversations.Where(x => joinedConversationIds.Contains(x.Id)))
            {
                var participants = data.ConversationParticipants
                    .Where(x => x.ConversationId == conversation.Id)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToList();

                var isGroup = IsGroupConversation(conversation, participants.Count);
                var otherUserId = !isGroup
                    ? participants.FirstOrDefault(x => x != currentUserId)
                    : Guid.Empty;

                var otherUser = otherUserId != Guid.Empty
                    ? data.Users.FirstOrDefault(x => x.Id == otherUserId)
                    : null;

                var lastMessage = data.Messages
                    .Where(x => x.ConversationId == conversation.Id)
                    .OrderByDescending(x => x.SentAt)
                    .FirstOrDefault();

                result.Add(new ConversationListItem(
                    conversation.Id,
                    BuildConversationTitle(conversation, otherUser),
                    isGroup,
                    otherUser?.AvatarUrl,
                    BuildConversationSubtitle(conversation, isGroup, otherUser),
                    otherUserId == Guid.Empty ? null : otherUserId,
                    lastMessage?.Content ?? "Chưa có tin nhắn",
                    lastMessage?.SentAt ?? conversation.UpdatedAt));
            }

            return result
                .OrderByDescending(x => x.LastMessageAt)
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task<IReadOnlyList<MessageableUserItem>> GetMessageableUsersAsync(CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            var existingDirectMap = data.Conversations
                .Where(conversation =>
                {
                    var participants = data.ConversationParticipants
                        .Where(x => x.ConversationId == conversation.Id)
                        .Select(x => x.UserId)
                        .Distinct()
                        .ToList();

                    return IsGroupConversation(conversation, participants.Count) is false
                           && participants.Count == 2
                           && participants.Contains(currentUserId);
                })
                .Select(conversation =>
                {
                    var participants = data.ConversationParticipants
                        .Where(x => x.ConversationId == conversation.Id)
                        .Select(x => x.UserId)
                        .Distinct()
                        .ToList();
                    var otherUserId = participants.First(x => x != currentUserId);
                    return new { otherUserId, conversation.Id };
                })
                .GroupBy(x => x.otherUserId)
                .ToDictionary(x => x.Key, x => x.First().Id);

            var friendIds = data.Friendships
                .Where(x => x.UserAId == currentUserId || x.UserBId == currentUserId)
                .Select(x => x.UserAId == currentUserId ? x.UserBId : x.UserAId)
                .Distinct()
                .ToHashSet();

            var items = data.Users
                .Where(x => friendIds.Contains(x.Id))
                .Select(user =>
                {
                    var hasConversation = existingDirectMap.TryGetValue(user.Id, out var existingConversationId);
                    return new MessageableUserItem(
                        user.Id,
                        user.FullName,
                        user.AvatarUrl,
                        user.Bio,
                        hasConversation,
                        hasConversation ? existingConversationId : null);
                })
                .OrderBy(x => x.FullName)
                .ToList();

            return items.AsReadOnly();
        }, ct);
    }

    public async Task<ConversationOpenResult> GetOrCreateDirectConversationAsync(Guid otherUserId, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        if (otherUserId == currentUserId)
            throw new InvalidOperationException("Không thể tự nhắn tin cho chính mình.");

        return await _mockDataStore.UseAsync(data =>
        {
            var otherUser = data.Users.FirstOrDefault(x => x.Id == otherUserId)
                            ?? throw new InvalidOperationException("Người dùng không tồn tại.");

            var isFriend = data.Friendships.Any(x =>
                (x.UserAId == currentUserId && x.UserBId == otherUserId) ||
                (x.UserAId == otherUserId && x.UserBId == currentUserId));
            if (!isFriend)
                throw new InvalidOperationException("Chỉ có thể nhắn tin với bạn bè hiện có.");

            var existing = data.Conversations.FirstOrDefault(conversation =>
            {
                var participants = data.ConversationParticipants
                    .Where(x => x.ConversationId == conversation.Id)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToList();

                return IsGroupConversation(conversation, participants.Count) is false
                       && participants.Count == 2
                       && participants.Contains(currentUserId)
                       && participants.Contains(otherUserId);
            });

            if (existing is not null)
                return new ConversationOpenResult(existing.Id, false);

            var now = DateTime.UtcNow;
            var conversationId = Guid.NewGuid();
            var conversation = new MockConversation
            {
                Id = conversationId,
                Title = $"Chat với {otherUser.FullName}",
                IsGroup = false,
                Type = "DIRECT",
                LastMessageId = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            data.Conversations.Add(conversation);
            data.ConversationParticipants.Add(new MockConversationParticipant
            {
                ConversationId = conversationId,
                UserId = currentUserId,
                JoinedAt = now
            });
            data.ConversationParticipants.Add(new MockConversationParticipant
            {
                ConversationId = conversationId,
                UserId = otherUserId,
                JoinedAt = now
            });

            return new ConversationOpenResult(conversationId, true);
        }, ct);
    }

    public async Task<IReadOnlyList<MessageItem>> GetMessagesAsync(Guid conversationId, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureParticipant(data, conversationId, currentUserId);

            return data.Messages
                .Where(x => x.ConversationId == conversationId)
                .OrderBy(x => x.SentAt)
                .Select(x => MapToMessageItem(x, data))
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task<MessageItem> SendMessageAsync(Guid conversationId, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Nội dung tin nhắn không được để trống.");

        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureParticipant(data, conversationId, currentUserId);

            var now = DateTime.UtcNow;
            var message = new MockMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = content.Trim(),
                MessageType = "TEXT",
                Status = "SENT",
                CreatedAt = now,
                SentAt = now,
                ReadByUserIds = [currentUserId]
            };

            data.Messages.Add(message);

            var conversation = data.Conversations.First(x => x.Id == conversationId);
            conversation.LastMessageId = message.Id;
            conversation.UpdatedAt = now;

            return MapToMessageItem(message, data);
        }, ct);
    }

    private async Task<Guid> GetCurrentUserIdAsync(CancellationToken ct)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var value = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(value, out var userId))
            throw new InvalidOperationException("User not authenticated.");
        return userId;
    }

    private static void EnsureParticipant(MockDataSet data, Guid conversationId, Guid currentUserId)
    {
        var isParticipant = data.ConversationParticipants.Any(x =>
            x.ConversationId == conversationId && x.UserId == currentUserId);

        if (!isParticipant)
            throw new InvalidOperationException("Bạn không thuộc cuộc trò chuyện này.");
    }

    private static MessageItem MapToMessageItem(MockMessage message, MockDataSet data)
    {
        var sender = data.Users.FirstOrDefault(x => x.Id == message.SenderId);
        return new MessageItem(
            message.Id,
            message.ConversationId,
            message.SenderId,
            sender?.FullName ?? "Unknown User",
            sender?.AvatarUrl,
            message.Content,
            message.SentAt,
            message.MessageType,
            message.Status,
            message.CreatedAt,
            message.ReadByUserIds.AsReadOnly());
    }

    private static bool IsGroupConversation(MockConversation conversation, int participantCount)
        => conversation.IsGroup
           || string.Equals(conversation.Type, "GROUP", StringComparison.OrdinalIgnoreCase)
           || participantCount > 2;

    private static string BuildConversationTitle(MockConversation conversation, MockUser? otherUser)
    {
        if (otherUser is not null)
            return otherUser.FullName;

        if (!string.IsNullOrWhiteSpace(conversation.Title))
            return conversation.Title;

        return "Cuộc trò chuyện";
    }

    private static string? BuildConversationSubtitle(MockConversation conversation, bool isGroup, MockUser? otherUser)
    {
        if (isGroup)
            return "Nhóm";

        if (!string.IsNullOrWhiteSpace(otherUser?.Bio))
            return otherUser.Bio;

        return "Bạn bè";
    }
}
