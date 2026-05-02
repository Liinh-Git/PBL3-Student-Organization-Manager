using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;

namespace Org.Frontend.Services.Posts;

public sealed class PostMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IPostService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<IReadOnlyList<PostFeedItem>> GetFeedAsync(int take = 20, CancellationToken ct = default)
    {
        return await _mockDataStore.UseAsync(data =>
        {
            return data.Posts
                .OrderByDescending(x => x.CreatedAt)
                .Take(Math.Max(1, take))
                .Select(x => MapToFeedItem(x, data))
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task<PostFeedItem> CreatePostAsync(CreatePostInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new InvalidOperationException("Tiêu đề bài viết là bắt buộc.");
        if (string.IsNullOrWhiteSpace(input.Content))
            throw new InvalidOperationException("Nội dung bài viết là bắt buộc.");

        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            if (!data.Organizations.Any(x => x.Id == input.OrgId))
                throw new InvalidOperationException("Tổ chức không tồn tại.");

            var post = new MockPost
            {
                Id = Guid.NewGuid(),
                OrgId = input.OrgId,
                AuthorUserId = currentUserId,
                RelatedEventId = input.RelatedEventId,
                Title = input.Title.Trim(),
                Content = input.Content.Trim(),
                PostType = input.PostType,
                Visibility = input.Visibility,
                MediaUrls = input.MediaUrls?.ToList() ?? [],
                LikeCount = 0,
                CommentCount = 0,
                ShareCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            data.Posts.Add(post);
            return MapToFeedItem(post, data);
        }, ct);
    }

    public async Task AddCommentAsync(Guid postId, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Nội dung bình luận không được để trống.");

        var currentUserId = await GetCurrentUserIdAsync(ct);
        await _mockDataStore.UseAsync(data =>
        {
            var post = data.Posts.FirstOrDefault(x => x.Id == postId)
                       ?? throw new InvalidOperationException("Bài viết không tồn tại.");

            data.PostComments.Add(new MockPostComment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                AuthorUserId = currentUserId,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            post.CommentCount = data.PostComments.Count(x => x.PostId == postId);
            return 0;
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

    private static PostFeedItem MapToFeedItem(MockPost post, MockDataSet data)
    {
        var org = data.Organizations.FirstOrDefault(x => x.Id == post.OrgId);
        var author = data.Users.FirstOrDefault(x => x.Id == post.AuthorUserId);

        return new PostFeedItem(
            post.Id,
            post.OrgId,
            org?.OrgName ?? "Unknown Organization",
            post.AuthorUserId,
            author?.FullName ?? "Unknown User",
            author?.AvatarUrl,
            post.Title,
            post.Content,
            post.PostType,
            post.Visibility,
            post.MediaUrls.AsReadOnly(),
            post.LikeCount,
            post.CommentCount,
            post.ShareCount,
            post.CreatedAt);
    }
}
