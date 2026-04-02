using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);
var isSeedMode = args.Contains("--seed", StringComparer.OrdinalIgnoreCase);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

if (!isSeedMode)
{
    builder.Services.AddFastEndpoints();
    builder.Services.AddOpenApi();
}

var app = builder.Build();

if (isSeedMode)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
    Console.WriteLine("Seeded data successfully.");
    Console.WriteLine($"Users: {await db.Users.CountAsync()}");
    Console.WriteLine($"Organizations: {await db.Organizations.CountAsync()}");
    Console.WriteLine($"Departments: {await db.Departments.CountAsync()}");
    Console.WriteLine($"Roles: {await db.Roles.CountAsync()}");
    Console.WriteLine($"Permissions: {await db.Permissions.CountAsync()}");
    Console.WriteLine($"RolePermissions: {await db.RolePermissions.CountAsync()}");
    Console.WriteLine($"Members: {await db.Members.CountAsync()}");
    Console.WriteLine($"Roles: {await db.Roles.CountAsync()}");
    Console.WriteLine($"Events: {await db.Events.CountAsync()}");
    Console.WriteLine($"EventMembers: {await db.EventMembers.CountAsync()}");
    Console.WriteLine($"EventReports: {await db.EventReports.CountAsync()}");
    Console.WriteLine($"Milestones: {await db.Milestones.CountAsync()}");
    Console.WriteLine($"Tasks: {await db.Tasks.CountAsync()}");
    Console.WriteLine($"Attendees: {await db.Attendees.CountAsync()}");
    Console.WriteLine($"DigitalAssets: {await db.DigitalAssets.CountAsync()}");
    Console.WriteLine($"Requests: {await db.Requests.CountAsync()}");
    Console.WriteLine($"Resources: {await db.Resources.CountAsync()}");
    Console.WriteLine($"ActivityHistories: {await db.ActivityHistories.CountAsync()}");
    return;
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

if (!isSeedMode)
{
    app.UseFastEndpoints();
}

app.Run();
