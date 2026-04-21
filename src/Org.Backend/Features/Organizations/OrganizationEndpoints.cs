// ---- Các endpoint CRUD cho module tổ chức (Organization) ----
// Bao gồm lấy danh sách, chi tiết, tạo, cập nhật, xóa mềm và khôi phục
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.Organizations;
using System.Security.Claims;

namespace Org.Backend.Features.Organizations;

// ---- GET /api/organizations — danh sách tổ chức có filter tìm kiếm và phân trang ----
public sealed class GetOrganizationsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetOrganizationsResponse>
{
    public override void Configure()
    {
        Get("/api/organizations");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var searchRaw = HttpContext.Request.Query.TryGetValue("search", out var searchValue)
            ? searchValue.ToString()
            : HttpContext.Request.Query.TryGetValue("q", out var qValue)
                ? qValue.ToString()
                : null;

        var search = OrganizationValidation.NormalizeOptional(searchRaw);

        var isActiveRaw = HttpContext.Request.Query["isActive"].ToString();
        var isActive = OrganizationValidation.ParseNullableBool(isActiveRaw);
        if (!string.IsNullOrWhiteSpace(isActiveRaw) && isActive is null)
            ThrowError("isActive must be true/false or 1/0.", StatusCodes.Status400BadRequest);

        var pageRaw = HttpContext.Request.Query["page"].ToString();
        var parsedPage = OrganizationValidation.ParsePositiveInt(pageRaw);
        if (!string.IsNullOrWhiteSpace(pageRaw) && parsedPage is null)
            ThrowError("page must be a positive integer.", StatusCodes.Status400BadRequest);
        var page = parsedPage ?? 1;

        var pageSizeRaw = HttpContext.Request.Query["pageSize"].ToString();
        var parsedPageSize = OrganizationValidation.ParsePositiveInt(pageSizeRaw);
        if (!string.IsNullOrWhiteSpace(pageSizeRaw) && parsedPageSize is null)
            ThrowError("pageSize must be a positive integer.", StatusCodes.Status400BadRequest);
        var pageSize = parsedPageSize ?? 20;

        if (pageSize > 100)
            ThrowError("pageSize must be between 1 and 100.", StatusCodes.Status400BadRequest);

        var organizationsQuery = db.Organizations
            .AsNoTracking();

        if (isActive is true)
            organizationsQuery = organizationsQuery.Where(x => x.Status == OrgStatus.Active);
        else if (isActive is false)
            organizationsQuery = organizationsQuery.Where(x => x.Status == OrgStatus.Inactive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.ToLower();
            organizationsQuery = organizationsQuery.Where(x =>
                x.OrgName.ToLower().Contains(keyword)
                || (x.Description != null && x.Description.ToLower().Contains(keyword))
                || (x.Location != null && x.Location.ToLower().Contains(keyword)));
        }

        var totalCount = await organizationsQuery.CountAsync(ct);
        var skip = (page - 1) * pageSize;

        var items = await organizationsQuery
            .OrderBy(x => x.OrgName)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => OrganizationContractMapping.ToSummaryDto(x))
            .ToListAsync(ct);

        await Send.OkAsync(new GetOrganizationsResponse(items, totalCount, page, pageSize, search, isActive), ct);
    }
}

// ---- GET /api/organizations/default — tổ chức đầu tiên user tham gia (dùng khởi tạo FE context) ----
public sealed class GetDefaultOrganizationEndpoint(AppDbContext db) : EndpointWithoutRequest<GetDefaultOrganizationResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/default");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        OrganizationSummaryDto? organization = null;

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdText, out var userId))
        {
            organization = await db.Members
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.JoinDate)
                .Select(x => new OrganizationSummaryDto(
                    x.Organization.Id,
                    x.Organization.OrgName,
                    x.Organization.Description))
                .FirstOrDefaultAsync(ct);
        }

        organization ??= await db.Organizations
            .AsNoTracking()
            .OrderBy(x => x.OrgName)
            .Select(x => new OrganizationSummaryDto(
                x.Id,
                x.OrgName,
                x.Description))
            .FirstOrDefaultAsync(ct);

        if (organization is null)
            ThrowError("No organization available.", StatusCodes.Status404NotFound);

        await Send.OkAsync(new GetDefaultOrganizationResponse(organization!), ct);
    }
}

// ---- GET /api/organizations/{id} — chi tiết một tổ chức ----
public sealed class GetOrganizationByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetOrganizationByIdResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var organization = await db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (organization is null)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, organization!.Id, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await Send.OkAsync(new GetOrganizationByIdResponse(OrganizationContractMapping.ToOrganizationDto(organization!)), ct);
    }
}

// ---- POST /api/organizations — tạo tổ chức mới, tự gán user hiện tại làm President ----
public sealed class CreateOrganizationEndpoint(AppDbContext db) : Endpoint<CreateOrganizationRequest, OrganizationDto>
{
    public override void Configure()
    {
        Post("/api/organizations");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateOrganizationRequest req, CancellationToken ct)
    {
        var normalizedName = OrganizationValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Organization name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var nameExists = await db.Organizations
            .IgnoreQueryFilters()
            .AnyAsync(x => x.OrgName.ToLower() == normalizedName.ToLower(), ct);

        if (nameExists)
            ThrowError("Organization name already exists.", StatusCodes.Status409Conflict);

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Unauthorized.", StatusCodes.Status401Unauthorized);

        var userExists = await db.Users.AnyAsync(x => x.Id == userId, ct);
        if (!userExists)
            ThrowError("User not found.", StatusCodes.Status404NotFound);

        var organization = new Organization
        {
            OrgName = normalizedName,
            Description = OrganizationValidation.NormalizeOptional(req.Description),
            AvatarUrl = OrganizationValidation.NormalizeOptional(req.AvatarUrl),
            CoverUrl = OrganizationValidation.NormalizeOptional(req.CoverUrl),
            FoundingDate = req.FoundingDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Location = OrganizationValidation.NormalizeOptional(req.Location),
            TotalMembers = 1,
            Status = OrgStatus.Active
        };

        var presidentRole = new Role
        {
            OrgId = organization.Id,
            RoleName = MemberRole.President.ToString(),
            Description = "Organization owner",
            IsDefault = false
        };

        var ownerMember = new Member
        {
            UserId = userId,
            OrgId = organization.Id,
            RoleId = presidentRole.Id,
            JoinDate = DateTime.UtcNow
        };

        db.Organizations.Add(organization);
        db.Roles.Add(presidentRole);
        db.Members.Add(ownerMember);

        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(OrganizationContractMapping.ToOrganizationDto(organization), StatusCodes.Status201Created, cancellation: ct);
    }
}

// ---- PUT /api/organizations/{id} — cập nhật thông tin tổ chức (yêu cầu Manager+) ----
public sealed class UpdateOrganizationEndpoint(AppDbContext db) : Endpoint<UpdateOrganizationRequest, OrganizationDto>
{
    public override void Configure()
    {
        Put("/api/organizations/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateOrganizationRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var normalizedName = OrganizationValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Organization name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var organization = await db.Organizations
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (organization is null)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, organization!.Id, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var nameExists = await db.Organizations
            .AnyAsync(x => x.Id != organization.Id && x.OrgName.ToLower() == normalizedName.ToLower(), ct);

        if (nameExists)
            ThrowError("Organization name already exists.", StatusCodes.Status409Conflict);

        organization.OrgName = normalizedName;
        organization.Description = OrganizationValidation.NormalizeOptional(req.Description);
        organization.AvatarUrl = OrganizationValidation.NormalizeOptional(req.AvatarUrl);
        organization.CoverUrl = OrganizationValidation.NormalizeOptional(req.CoverUrl);
        organization.FoundingDate = req.FoundingDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        organization.Location = OrganizationValidation.NormalizeOptional(req.Location);
        organization.Status = req.IsActive ? OrgStatus.Active : OrgStatus.Inactive;

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(OrganizationContractMapping.ToOrganizationDto(organization), ct);
    }
}

// ---- DELETE /api/organizations/{id} — xóa mềm toàn bộ dữ liệu (yêu cầu VicePresident+) ----
public sealed class DeleteOrganizationEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/organizations/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var organization = await db.Organizations
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (organization is null)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, organization!.Id, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await OrganizationCascade.ToggleSoftDeleteAsync(db, organization.Id, isDeleted: true, ct);

        organization.IsDeleted = true;
        organization.Status = OrgStatus.Inactive;

        await db.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}

// ---- POST /api/organizations/{id}/restore — khôi phục tổ chức và dữ liệu con ----
public sealed class RestoreOrganizationEndpoint(AppDbContext db) : EndpointWithoutRequest<OrganizationDto>
{
    public override void Configure()
    {
        Post("/api/organizations/{id:guid}/restore");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var organization = await db.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (organization is null)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerRole = await OrganizationRestoreAuthorization.ResolveCallerRoleIncludingDeletedAsync(db, User, id, ct);
        if (callerRole is null || !OrganizationAuthorization.CanDelete(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await OrganizationCascade.ToggleSoftDeleteAsync(db, organization.Id, isDeleted: false, ct);

        organization.IsDeleted = false;
        organization.Status = OrgStatus.Active;

        await db.SaveChangesAsync(ct);
        await Send.OkAsync(OrganizationContractMapping.ToOrganizationDto(organization), ct);
    }
}

// ---- Helper validate các tham số đầu vào cho module tổ chức ----
internal static class OrganizationValidation
{
    public static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        return normalized.Length >= 2 ? normalized : null;
    }

    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static int? ParsePositiveInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value, out var parsed) && parsed > 0
            ? parsed
            : null;
    }

    public static bool? ParseNullableBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "true" or "1" => true,
            "false" or "0" => false,
            _ => null
        };
    }
}

// ---- Mapping entity Organization sang DTO (dùng nội bộ trong feature này) ----
// Lưu ý: IsActive = true khi KHÔNG xóa VÀ Status == Active
internal static class OrganizationContractMapping
{
    public static OrganizationSummaryDto ToSummaryDto(Organization organization)
        => new(
            organization.Id,
            organization.OrgName,
            organization.Description);

    public static OrganizationDto ToOrganizationDto(Organization organization)
        => new(
            organization.Id,
            organization.OrgName,
            organization.Description,
            organization.AvatarUrl,
            organization.CoverUrl,
            organization.FoundingDate is null ? null : DateOnly.FromDateTime(organization.FoundingDate.Value),
            organization.Location,
            organization.TotalMembers,
            !organization.IsDeleted && organization.Status == OrgStatus.Active,
            ToUtcOffset(organization.CreatedAt),
            organization.UpdatedAt is null ? null : ToUtcOffset(organization.UpdatedAt.Value));

    private static DateTimeOffset ToUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}

// ---- Authorization khi khôi phục: cho phép truy cập cả entity đã xóa mềm ----
internal static class OrganizationRestoreAuthorization
{
    public static async Task<MemberRole?> ResolveCallerRoleIncludingDeletedAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        Guid orgId,
        CancellationToken ct)
    {
        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            return null;

        var membership = await db.Members
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.OrgId == orgId, ct);

        if (membership is null)
            return null;

        if (string.IsNullOrWhiteSpace(membership.Role?.RoleName))
            return MemberRole.Member;

        return Enum.TryParse<MemberRole>(membership.Role.RoleName, ignoreCase: true, out var parsed)
            ? parsed
            : MemberRole.Member;
    }
}

// ---- Xóa mềm/khôi phục toàn bộ dữ liệu liên quan đến tổ chức (cascade) ----
internal static class OrganizationCascade
{
    public static async Task ToggleSoftDeleteAsync(AppDbContext db, Guid orgId, bool isDeleted, CancellationToken ct)
    {
        var roles = await db.Roles
            .IgnoreQueryFilters()
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var members = await db.Members
            .IgnoreQueryFilters()
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var departments = await db.Departments
            .IgnoreQueryFilters()
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var events = await db.Events
            .IgnoreQueryFilters()
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var resources = await db.Resources
            .IgnoreQueryFilters()
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var requests = await db.Requests
            .IgnoreQueryFilters()
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var activities = await db.ActivityHistories
            .IgnoreQueryFilters()
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var eventIds = events.Select(x => x.Id).ToList();

        var milestones = eventIds.Count == 0
            ? []
            : await db.Milestones
                .IgnoreQueryFilters()
                .Where(x => eventIds.Contains(x.EventId))
                .ToListAsync(ct);

        var milestoneIds = milestones.Select(x => x.Id).ToList();

        var categories = milestoneIds.Count == 0
            ? []
            : await db.EventCategories
                .IgnoreQueryFilters()
                .Where(x => milestoneIds.Contains(x.MilestoneId))
                .ToListAsync(ct);

        var categoryIds = categories.Select(x => x.Id).ToList();

        var tasks = categoryIds.Count == 0
            ? []
            : await db.Tasks
                .IgnoreQueryFilters()
                .Where(x => categoryIds.Contains(x.EventCategoryId))
                .ToListAsync(ct);

        foreach (var role in roles)
            role.IsDeleted = isDeleted;

        foreach (var member in members)
            member.IsDeleted = isDeleted;

        foreach (var department in departments)
            department.IsDeleted = isDeleted;

        foreach (var resource in resources)
            resource.IsDeleted = isDeleted;

        foreach (var request in requests)
            request.IsDeleted = isDeleted;

        foreach (var activity in activities)
            activity.IsDeleted = isDeleted;

        foreach (var task in tasks)
            task.IsDeleted = isDeleted;

        foreach (var category in categories)
            category.IsDeleted = isDeleted;

        foreach (var milestone in milestones)
            milestone.IsDeleted = isDeleted;

        foreach (var evt in events)
            evt.IsDeleted = isDeleted;
    }
}
