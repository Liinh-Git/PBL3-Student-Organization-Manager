using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.Departments;
using Org.Shared.Features.Members;
using System.Security.Claims;

namespace Org.Backend.Features.Departments;

public sealed class GetDepartmentsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetDepartmentsResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{orgId:guid}/departments");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");
        var searchRaw = HttpContext.Request.Query.TryGetValue("search", out var searchValue)
            ? searchValue.ToString()
            : HttpContext.Request.Query.TryGetValue("q", out var qValue)
                ? qValue.ToString()
                : null;

        var search = DepartmentValidation.NormalizeOptional(searchRaw);

        var isActiveRaw = HttpContext.Request.Query["isActive"].ToString();
        var isActive = DepartmentValidation.ParseNullableBool(isActiveRaw);
        if (!string.IsNullOrWhiteSpace(isActiveRaw) && isActive is null)
            ThrowError("isActive must be true/false or 1/0.", StatusCodes.Status400BadRequest);

        var pageRaw = HttpContext.Request.Query["page"].ToString();
        var parsedPage = DepartmentValidation.ParsePositiveInt(pageRaw);
        if (!string.IsNullOrWhiteSpace(pageRaw) && parsedPage is null)
            ThrowError("page must be a positive integer.", StatusCodes.Status400BadRequest);
        var page = parsedPage ?? 1;

        var pageSizeRaw = HttpContext.Request.Query["pageSize"].ToString();
        var parsedPageSize = DepartmentValidation.ParsePositiveInt(pageSizeRaw);
        if (!string.IsNullOrWhiteSpace(pageSizeRaw) && parsedPageSize is null)
            ThrowError("pageSize must be a positive integer.", StatusCodes.Status400BadRequest);
        var pageSize = parsedPageSize ?? 20;

        if (pageSize > 100)
            ThrowError("pageSize must be between 1 and 100.", StatusCodes.Status400BadRequest);

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, orgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanRead(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var departmentsQuery = db.Departments
            .AsNoTracking()
            .Where(x => x.OrgId == orgId);

        if (isActive is false)
        {
            departmentsQuery = db.Departments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.OrgId == orgId && x.IsDeleted);
        }
        else if (isActive is true)
        {
            departmentsQuery = departmentsQuery.Where(x => !x.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.ToLower();
            departmentsQuery = departmentsQuery.Where(x =>
                x.DeptName.ToLower().Contains(keyword)
                || (x.Code != null && x.Code.ToLower().Contains(keyword))
                || (x.Function != null && x.Function.ToLower().Contains(keyword)));
        }

        var totalCount = await departmentsQuery.CountAsync(ct);
        var skip = (page - 1) * pageSize;

        var departments = await departmentsQuery
            .OrderBy(x => x.DeptName)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new
            {
                Entity = x,
                MemberCount = db.Members.Count(m => m.DepartmentId == x.Id)
            })
            .ToListAsync(ct);

        var items = departments
            .Select(x => ContractMapping.ToDepartmentDto(x.Entity, x.MemberCount))
            .ToList();

        await Send.OkAsync(new GetDepartmentsResponse(items, totalCount, page, pageSize, search, isActive), ct);
    }
}

public sealed class GetDepartmentByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetDepartmentByIdResponse>
{
    public override void Configure()
    {
        Get("/api/departments/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var department = await db.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanRead(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var memberCount = await db.Members.CountAsync(x => x.DepartmentId == department.Id, ct);
        await Send.OkAsync(new GetDepartmentByIdResponse(ContractMapping.ToDepartmentDto(department, memberCount)), ct);
    }
}

public sealed class CreateDepartmentEndpoint(AppDbContext db) : Endpoint<CreateDepartmentRequest, DepartmentDto>
{
    public override void Configure()
    {
        Post("/api/departments");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateDepartmentRequest req, CancellationToken ct)
    {
        var normalizedName = DepartmentValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Department name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var normalizedCode = DepartmentValidation.NormalizeCode(req.Code, normalizedName);

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == req.OrganizationId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, req.OrganizationId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanWrite(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var codeExists = await db.Departments.AnyAsync(
            x => x.OrgId == req.OrganizationId && x.Code != null && x.Code.ToLower() == normalizedCode.ToLower(),
            ct);

        if (codeExists)
            ThrowError("Department code already exists in organization.", StatusCodes.Status409Conflict);

        if (req.ManagerMemberId is not null)
        {
            var managerExists = await db.Members
                .AnyAsync(x => x.Id == req.ManagerMemberId.Value && x.OrgId == req.OrganizationId, ct);

            if (!managerExists)
                ThrowError("Manager member not found in organization.", StatusCodes.Status404NotFound);
        }

        var entity = new Domain.Entities.Department
        {
            OrgId = req.OrganizationId,
            DeptName = normalizedName,
            Code = normalizedCode,
            Function = DepartmentValidation.NormalizeOptional(req.Description),
            ManagerId = req.ManagerMemberId
        };

        db.Departments.Add(entity);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(ContractMapping.ToDepartmentDto(entity, 0), StatusCodes.Status201Created, cancellation: ct);
    }
}

public sealed class UpdateDepartmentEndpoint(AppDbContext db) : Endpoint<UpdateDepartmentRequest, DepartmentDto>
{
    public override void Configure()
    {
        Put("/api/departments/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateDepartmentRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var department = await db.Departments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanWrite(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var normalizedName = DepartmentValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Department name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var normalizedCode = DepartmentValidation.NormalizeCode(req.Code, normalizedName);

        var codeExists = await db.Departments.AnyAsync(
            x => x.OrgId == department.OrgId && x.Id != department.Id && x.Code != null && x.Code.ToLower() == normalizedCode.ToLower(),
            ct);

        if (codeExists)
            ThrowError("Department code already exists in organization.", StatusCodes.Status409Conflict);

        if (req.ManagerMemberId is not null)
        {
            var managerExists = await db.Members
                .AnyAsync(x => x.Id == req.ManagerMemberId.Value && x.OrgId == department!.OrgId, ct);

            if (!managerExists)
                ThrowError("Manager member not found in organization.", StatusCodes.Status404NotFound);
        }

        department!.Code = normalizedCode;
        department.DeptName = normalizedName;
        department.Function = DepartmentValidation.NormalizeOptional(req.Description);
        department.ManagerId = req.ManagerMemberId;
        department.IsDeleted = !req.IsActive;

        await db.SaveChangesAsync(ct);

        var memberCount = await db.Members.CountAsync(x => x.DepartmentId == department.Id, ct);
        await Send.OkAsync(ContractMapping.ToDepartmentDto(department, memberCount), ct);
    }
}

public sealed class DeleteDepartmentEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/departments/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var department = await db.Departments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanDelete(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var members = await db.Members.Where(x => x.DepartmentId == id).ToListAsync(ct);
        foreach (var member in members)
        {
            member.DepartmentId = null;
        }

        department!.ManagerId = null;
        department.IsDeleted = true;

        await db.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}

public sealed class RestoreDepartmentEndpoint(AppDbContext db) : EndpointWithoutRequest<DepartmentDto>
{
    public override void Configure()
    {
        Post("/api/departments/{id:guid}/restore");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var department = await db.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanDelete(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        department!.IsDeleted = false;
        await db.SaveChangesAsync(ct);

        var memberCount = await db.Members.CountAsync(x => x.DepartmentId == department.Id, ct);
        await Send.OkAsync(ContractMapping.ToDepartmentDto(department, memberCount), ct);
    }
}

public sealed class UpdateDepartmentManagerEndpoint(AppDbContext db) : Endpoint<UpdateDepartmentManagerRequest, DepartmentDto>
{
    public override void Configure()
    {
        Put("/api/departments/{id:guid}/manager");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateDepartmentManagerRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var department = await db.Departments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanAssign(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (req.ManagerMemberId is not null)
        {
            var managerExists = await db.Members
                .AnyAsync(x => x.Id == req.ManagerMemberId.Value && x.OrgId == department.OrgId, ct);

            if (!managerExists)
                ThrowError("Manager member not found in organization.", StatusCodes.Status404NotFound);
        }

        department.ManagerId = req.ManagerMemberId;
        await db.SaveChangesAsync(ct);

        var memberCount = await db.Members.CountAsync(x => x.DepartmentId == department.Id, ct);
        await Send.OkAsync(ContractMapping.ToDepartmentDto(department, memberCount), ct);
    }
}

public sealed class GetDepartmentMembersEndpoint(AppDbContext db) : EndpointWithoutRequest<GetDepartmentMembersResponse>
{
    public override void Configure()
    {
        Get("/api/departments/{id:guid}/members");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var department = await db.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanRead(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var members = await db.Members
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.DepartmentId == id)
            .OrderBy(x => x.User.FullName)
            .ToListAsync(ct);

        var items = members.Select(ContractMapping.ToMemberDto).ToList();
        await Send.OkAsync(new GetDepartmentMembersResponse(items), ct);
    }
}

public sealed class AssignDepartmentMemberEndpoint(AppDbContext db) : EndpointWithoutRequest<MemberDto>
{
    public override void Configure()
    {
        Post("/api/departments/{id:guid}/members/{memberId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var departmentId = Route<Guid>("id");
        var memberId = Route<Guid>("memberId");

        var department = await db.Departments.FirstOrDefaultAsync(x => x.Id == departmentId, ct);
        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanAssign(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var member = await db.Members
            .Include(x => x.User)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == memberId && x.OrgId == department.OrgId, ct);

        if (member is null)
            ThrowError("Member not found in organization.", StatusCodes.Status404NotFound);

        member.DepartmentId = departmentId;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToMemberDto(member), ct);
    }
}

public sealed class RemoveDepartmentMemberEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/departments/{id:guid}/members/{memberId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var departmentId = Route<Guid>("id");
        var memberId = Route<Guid>("memberId");

        var department = await db.Departments.FirstOrDefaultAsync(x => x.Id == departmentId, ct);
        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanAssign(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var member = await db.Members
            .FirstOrDefaultAsync(x => x.Id == memberId && x.OrgId == department.OrgId, ct);

        if (member is null)
            ThrowError("Member not found in organization.", StatusCodes.Status404NotFound);

        if (department.ManagerId == memberId)
            department.ManagerId = null;

        if (member.DepartmentId == departmentId)
            member.DepartmentId = null;

        await db.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}

public sealed class GetDepartmentTasksOverviewEndpoint(AppDbContext db) : EndpointWithoutRequest<GetDepartmentTasksOverviewResponse>
{
    public override void Configure()
    {
        Get("/api/departments/{id:guid}/tasks/overview");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var departmentId = Route<Guid>("id");

        var department = await db.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == departmentId, ct);

        if (department is null)
            ThrowError("Department not found.", StatusCodes.Status404NotFound);

        var callerRole = await DepartmentAuthorization.ResolveCallerRoleAsync(db, User, department!.OrgId, ct);
        if (callerRole is null || !DepartmentAuthorization.CanRead(callerRole.Value))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var taskItems = await db.Tasks
            .AsNoTracking()
            .Include(x => x.Assignee)
            .ThenInclude(x => x!.User)
            .Where(x => x.DeptId == departmentId || (x.Assignee != null && x.Assignee.DepartmentId == departmentId))
            .Select(x => new DepartmentTaskOverviewItemDto(
                x.Id,
                x.TaskName,
                x.Status,
                x.Priority,
                x.Deadline == null ? null : DateOnly.FromDateTime(x.Deadline.Value),
                x.AssigneeId,
                x.Assignee == null ? null : x.Assignee.User.FullName))
            .ToListAsync(ct);

        var distinctItems = taskItems
            .GroupBy(x => x.TaskId)
            .Select(x => x.First())
            .ToList();

        var totalTasks = distinctItems.Count;
        var completedTaskCount = distinctItems.Count(x => x.Status == Org.Shared.TaskStatus.Done);
        var openTaskCount = Math.Max(0, totalTasks - completedTaskCount);

        var orderedPreview = distinctItems
            .OrderBy(x => DepartmentAuthorization.TaskOrder(x.Status))
            .ThenBy(x => x.DueDate ?? DateOnly.MaxValue)
            .ThenBy(x => x.Title)
            .Take(8)
            .ToList();

        await Send.OkAsync(new GetDepartmentTasksOverviewResponse(
            departmentId,
            totalTasks,
            openTaskCount,
            completedTaskCount,
            orderedPreview), ct);
    }
}


