// ---- Các endpoint CRUD cho module bài viết tổ chức ----
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Posts;
using System.Security.Claims;

namespace Org.Backend.Features.Posts;

// ---- POST /api/posts — tạo bài viết mới ----
public sealed class CreatePostEndpoint(AppDbContext db) : Endpoint<CreatePostRequest, OrganizationPostDto>
{
    public override void Configure()
    {
        Post("/api/posts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreatePostRequest req, CancellationToken ct)
    {
        var userId = ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        // Kiểm tra user có phải member của org không
        var member = await db.Members
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.UserId == userId.Value && m.OrgId == req.OrganizationId, ct);

        if (member is null)
            ThrowError("You must be a member of the organization to create posts.", StatusCodes.Status403Forbidden);

        // Kiểm tra quyền: chỉ Manager+ mới được tạo bài viết
        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, req.OrganizationId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Only Manager and above can create posts.", StatusCodes.Status403Forbidden);

        if (!Enum.TryParse<PostType>(req.PostType, ignoreCase: true, out var postType))
            ThrowError("Invalid PostType.", StatusCodes.Status400BadRequest);

        if (!Enum.TryParse<PostVisibility>(req.Visibility, ignoreCase: true, out var visibility))
            ThrowError("Invalid Visibility.", StatusCodes.Status400BadRequest);

        var post = new OrganizationPost
        {
            OrgId = req.OrganizationId,
            Title = req.Title.Trim(),
            Content = req.Content.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim(),
            PostType = postType,
            Visibility = visibility,
            TargetDepartmentId = req.TargetDepartmentId,
            CreatedBy = member.Id,
            RelatedEventId = req.RelatedEventId
        };

        db.OrganizationPosts.Add(post);
        await db.SaveChangesAsync(ct);

        var dto = await MapToDto(db, post, ct);
        await HttpContext.Response.SendAsync(dto, StatusCodes.Status201Created, cancellation: ct);
    }

    private static Guid? ParseUserId(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }

    private static async Task<OrganizationPostDto> MapToDto(AppDbContext db, OrganizationPost post, CancellationToken ct)
    {
        var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.Id == post.OrgId, ct);
        var creator = await db.Members.AsNoTracking().Include(m => m.User).FirstAsync(m => m.Id == post.CreatedBy, ct);
        
        Department? dept = null;
        if (post.TargetDepartmentId.HasValue)
            dept = await db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == post.TargetDepartmentId.Value, ct);

        Event? evt = null;
        if (post.RelatedEventId.HasValue)
            evt = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == post.RelatedEventId.Value, ct);

        return new OrganizationPostDto(
            post.Id,
            post.OrgId,
            org.OrgName,
            org.AvatarUrl,
            post.Title,
            post.Content,
            post.ImageUrl,
            post.PostType.ToString(),
            post.Visibility.ToString(),
            post.TargetDepartmentId,
            dept?.DeptName,
            post.CreatedBy,
            creator.User.FullName,
            creator.User.AvatarUrl,
            post.RelatedEventId,
            evt?.EventName,
            post.ViewCount,
            post.LikeCount,
            new DateTimeOffset(DateTime.SpecifyKind(post.CreatedAt, DateTimeKind.Utc)),
            post.UpdatedAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(post.UpdatedAt.Value, DateTimeKind.Utc)));
    }
}

// ---- GET /api/organizations/{orgId}/posts — danh sách bài viết của tổ chức ----
public sealed class GetOrganizationPostsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetPostsResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{orgId:guid}/posts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");

        var pageRaw = HttpContext.Request.Query["page"].ToString();
        var page = int.TryParse(pageRaw, out var p) && p > 0 ? p : 1;

        var pageSizeRaw = HttpContext.Request.Query["pageSize"].ToString();
        var pageSize = int.TryParse(pageSizeRaw, out var ps) && ps > 0 && ps <= 100 ? ps : 20;

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var query = db.OrganizationPosts
            .AsNoTracking()
            .Include(p => p.Organization)
            .Include(p => p.Creator).ThenInclude(m => m.User)
            .Include(p => p.TargetDepartment)
            .Include(p => p.RelatedEvent)
            .Where(p => p.OrgId == orgId);

        var totalCount = await query.CountAsync(ct);
        var skip = (page - 1) * pageSize;

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = posts.Select(p => new OrganizationPostDto(
            p.Id,
            p.OrgId,
            p.Organization.OrgName,
            p.Organization.AvatarUrl,
            p.Title,
            p.Content,
            p.ImageUrl,
            p.PostType.ToString(),
            p.Visibility.ToString(),
            p.TargetDepartmentId,
            p.TargetDepartment?.DeptName,
            p.CreatedBy,
            p.Creator.User.FullName,
            p.Creator.User.AvatarUrl,
            p.RelatedEventId,
            p.RelatedEvent?.EventName,
            p.ViewCount,
            p.LikeCount,
            new DateTimeOffset(DateTime.SpecifyKind(p.CreatedAt, DateTimeKind.Utc)),
            p.UpdatedAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(p.UpdatedAt.Value, DateTimeKind.Utc))))
            .ToList();

        await Send.OkAsync(new GetPostsResponse(items, totalCount, page, pageSize), ct);
    }
}

// ---- GET /api/posts/discover — bài viết public cho trang khám phá ----
public sealed class GetDiscoverPostsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetDiscoverPostsResponse>
{
    public override void Configure()
    {
        Get("/api/posts/discover");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var posts = await db.OrganizationPosts
            .AsNoTracking()
            .Include(p => p.Organization)
            .Include(p => p.Creator).ThenInclude(m => m.User)
            .Include(p => p.RelatedEvent)
            .Where(p => p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        var items = posts.Select(p => new DiscoverPostDto(
            p.Id,
            p.OrgId,
            p.Organization.OrgName,
            p.Organization.AvatarUrl,
            p.Title,
            p.Content,
            p.ImageUrl,
            p.PostType.ToString(),
            p.CreatedBy,
            p.Creator.User.FullName,
            p.Creator.User.AvatarUrl,
            p.RelatedEventId,
            p.RelatedEvent?.EventName,
            p.ViewCount,
            p.LikeCount,
            new DateTimeOffset(DateTime.SpecifyKind(p.CreatedAt, DateTimeKind.Utc))))
            .ToList();

        await Send.OkAsync(new GetDiscoverPostsResponse(items), ct);
    }
}

// ---- GET /api/posts/{id} — chi tiết bài viết ----
public sealed class GetPostByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetPostByIdResponse>
{
    public override void Configure()
    {
        Get("/api/posts/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var post = await db.OrganizationPosts
            .AsNoTracking()
            .Include(p => p.Organization)
            .Include(p => p.Creator).ThenInclude(m => m.User)
            .Include(p => p.TargetDepartment)
            .Include(p => p.RelatedEvent)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (post is null)
            ThrowError("Post not found.", StatusCodes.Status404NotFound);

        // Kiểm tra quyền xem
        if (post.Visibility == PostVisibility.Private)
        {
            var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, post.OrgId, ct);
            if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
                ThrowError("Forbidden.", StatusCodes.Status403Forbidden);
        }
        else if (post.Visibility == PostVisibility.MembersOnly)
        {
            var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, post.OrgId, ct);
            if (callerContext is null)
                ThrowError("Forbidden.", StatusCodes.Status403Forbidden);
        }

        // Tăng view count
        var postToUpdate = await db.OrganizationPosts.FirstAsync(p => p.Id == id, ct);
        postToUpdate.ViewCount++;
        await db.SaveChangesAsync(ct);

        var dto = new OrganizationPostDto(
            post.Id,
            post.OrgId,
            post.Organization.OrgName,
            post.Organization.AvatarUrl,
            post.Title,
            post.Content,
            post.ImageUrl,
            post.PostType.ToString(),
            post.Visibility.ToString(),
            post.TargetDepartmentId,
            post.TargetDepartment?.DeptName,
            post.CreatedBy,
            post.Creator.User.FullName,
            post.Creator.User.AvatarUrl,
            post.RelatedEventId,
            post.RelatedEvent?.EventName,
            postToUpdate.ViewCount,
            post.LikeCount,
            new DateTimeOffset(DateTime.SpecifyKind(post.CreatedAt, DateTimeKind.Utc)),
            post.UpdatedAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(post.UpdatedAt.Value, DateTimeKind.Utc)));

        await Send.OkAsync(new GetPostByIdResponse(dto), ct);
    }
}

// ---- DELETE /api/posts/{id} — xóa bài viết ----
public sealed class DeletePostEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/posts/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var post = await db.OrganizationPosts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (post is null)
            ThrowError("Post not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, post.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        post.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
