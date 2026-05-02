// ---- Các endpoint cho module đánh giá sự kiện ----
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Events;
using System.Security.Claims;

namespace Org.Backend.Features.Events;

// ---- POST /api/events/{eventId}/ratings — tạo đánh giá mới ----
public sealed class CreateEventRatingEndpoint(AppDbContext db) : Endpoint<CreateEventRatingRequest, EventRatingDto>
{
    public override void Configure()
    {
        Post("/api/events/{eventId:guid}/ratings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateEventRatingRequest req, CancellationToken ct)
    {
        var eventId = Route<Guid>("eventId");
        var userId = ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        if (req.Rating < 1 || req.Rating > 5)
            ThrowError("Rating must be between 1 and 5.", StatusCodes.Status400BadRequest);

        if (!Enum.TryParse<Domain.Enums.RatingAspect>(req.Aspect, ignoreCase: true, out var aspect))
            ThrowError("Invalid Aspect.", StatusCodes.Status400BadRequest);

        // Kiểm tra event tồn tại
        var eventExists = await db.Events.AnyAsync(e => e.Id == eventId, ct);
        if (!eventExists)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        // Kiểm tra user đã tham dự event chưa
        var attended = await db.Attendees
            .AnyAsync(a => a.EventId == eventId && a.UserId == userId.Value && a.Status == Domain.Enums.AttendeeStatus.Attended, ct);

        if (!attended)
            ThrowError("You must attend the event to rate it.", StatusCodes.Status403Forbidden);

        // Kiểm tra đã đánh giá aspect này chưa
        var existingRating = await db.EventRatings
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId.Value && r.Aspect == aspect, ct);

        if (existingRating is not null)
            ThrowError($"You have already rated this aspect ({aspect}).", StatusCodes.Status409Conflict);

        var rating = new EventRating
        {
            EventId = eventId,
            UserId = userId.Value,
            Rating = req.Rating,
            Aspect = aspect,
            Comment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim()
        };

        db.EventRatings.Add(rating);
        await db.SaveChangesAsync(ct);

        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId.Value, ct);

        var dto = new EventRatingDto(
            rating.Id,
            rating.EventId,
            rating.UserId,
            user.FullName,
            user.AvatarUrl,
            rating.Rating,
            rating.Aspect.ToString(),
            rating.Comment,
            new DateTimeOffset(DateTime.SpecifyKind(rating.CreatedAt, DateTimeKind.Utc)));

        await HttpContext.Response.SendAsync(dto, StatusCodes.Status201Created, cancellation: ct);
    }

    private static Guid? ParseUserId(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }
}

// ---- GET /api/events/{eventId}/ratings — danh sách đánh giá của sự kiện ----
public sealed class GetEventRatingsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetEventRatingsResponse>
{
    public override void Configure()
    {
        Get("/api/events/{eventId:guid}/ratings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var eventId = Route<Guid>("eventId");

        var ratings = await db.EventRatings
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.EventId == eventId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var items = ratings.Select(r => new EventRatingDto(
            r.Id,
            r.EventId,
            r.UserId,
            r.User.FullName,
            r.User.AvatarUrl,
            r.Rating,
            r.Aspect.ToString(),
            r.Comment,
            new DateTimeOffset(DateTime.SpecifyKind(r.CreatedAt, DateTimeKind.Utc))))
            .ToList();

        await Send.OkAsync(new GetEventRatingsResponse(items), ct);
    }
}

// ---- GET /api/events/{eventId}/ratings/stats — thống kê đánh giá ----
public sealed class GetEventRatingStatsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetEventRatingStatsResponse>
{
    public override void Configure()
    {
        Get("/api/events/{eventId:guid}/ratings/stats");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var eventId = Route<Guid>("eventId");

        var ratings = await db.EventRatings
            .AsNoTracking()
            .Where(r => r.EventId == eventId)
            .ToListAsync(ct);

        if (ratings.Count == 0)
        {
            var emptyStats = new EventRatingStatsDto(
                eventId, 0, 0, 0, 0, 0, 0,
                new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } });
            await Send.OkAsync(new GetEventRatingStatsResponse(emptyStats), ct);
            return;
        }

        var overallRatings = ratings.Where(r => r.Aspect == Domain.Enums.RatingAspect.Overall).ToList();
        var orgRatings = ratings.Where(r => r.Aspect == Domain.Enums.RatingAspect.Organization).ToList();
        var contentRatings = ratings.Where(r => r.Aspect == Domain.Enums.RatingAspect.Content).ToList();
        var venueRatings = ratings.Where(r => r.Aspect == Domain.Enums.RatingAspect.Venue).ToList();
        var foodRatings = ratings.Where(r => r.Aspect == Domain.Enums.RatingAspect.Food).ToList();

        var distribution = ratings
            .GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        for (int i = 1; i <= 5; i++)
        {
            if (!distribution.ContainsKey(i))
                distribution[i] = 0;
        }

        var stats = new EventRatingStatsDto(
            eventId,
            overallRatings.Count > 0 ? (float)overallRatings.Average(r => r.Rating) : 0,
            orgRatings.Count > 0 ? (float)orgRatings.Average(r => r.Rating) : 0,
            contentRatings.Count > 0 ? (float)contentRatings.Average(r => r.Rating) : 0,
            venueRatings.Count > 0 ? (float)venueRatings.Average(r => r.Rating) : 0,
            foodRatings.Count > 0 ? (float)foodRatings.Average(r => r.Rating) : 0,
            ratings.Count,
            distribution);

        await Send.OkAsync(new GetEventRatingStatsResponse(stats), ct);
    }
}

// ---- DELETE /api/ratings/{id} — xóa đánh giá ----
public sealed class DeleteEventRatingEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/ratings/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var userId = ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var rating = await db.EventRatings.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rating is null)
            ThrowError("Rating not found.", StatusCodes.Status404NotFound);

        if (rating.UserId != userId.Value)
            ThrowError("You can only delete your own ratings.", StatusCodes.Status403Forbidden);

        rating.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }

    private static Guid? ParseUserId(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }
}
