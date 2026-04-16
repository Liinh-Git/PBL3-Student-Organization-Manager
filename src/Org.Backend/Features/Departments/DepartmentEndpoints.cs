using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Departments;

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

        var departments = await db.Departments
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.DeptName)
            .Select(x => new
            {
                Entity = x,
                MemberCount = db.Members.Count(m => m.DepartmentId == x.Id)
            })
            .ToListAsync(ct);

        var items = departments
            .Select(x => ContractMapping.ToDepartmentDto(x.Entity, x.MemberCount))
            .ToList();

        await Send.OkAsync(new GetDepartmentsResponse(items), ct);
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
        var orgExists = await db.Organizations.AnyAsync(x => x.Id == req.OrganizationId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

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
            DeptName = req.Name.Trim(),
            Code = req.Code?.Trim(),
            Function = req.Description?.Trim(),
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

        if (req.ManagerMemberId is not null)
        {
            var managerExists = await db.Members
                .AnyAsync(x => x.Id == req.ManagerMemberId.Value && x.OrgId == department!.OrgId, ct);

            if (!managerExists)
                ThrowError("Manager member not found in organization.", StatusCodes.Status404NotFound);
        }

        department!.Code = req.Code?.Trim();
        department.DeptName = req.Name.Trim();
        department.Function = req.Description?.Trim();
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
