namespace Org.Frontend.Services.Posts;

public interface IPostService
{
    Task<IReadOnlyList<PostFeedItem>> GetFeedAsync(int take = 20, CancellationToken ct = default);
    Task<PostFeedItem> CreatePostAsync(CreatePostInput input, CancellationToken ct = default);
    Task AddCommentAsync(Guid postId, string content, CancellationToken ct = default);
}

public sealed record CreatePostInput(
    Guid OrgId,
    string Title,
    string Content,
    string PostType = "General",
    string Visibility = "Public",
    Guid? RelatedEventId = null,
    IReadOnlyList<string>? MediaUrls = null);

public sealed record PostFeedItem(
    Guid Id,
    Guid OrgId,
    string OrgName,
    Guid AuthorUserId,
    string AuthorName,
    string? AuthorAvatarUrl,
    string Title,
    string Content,
    string PostType,
    string Visibility,
    IReadOnlyList<string> MediaUrls,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    DateTime CreatedAt);
