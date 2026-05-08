using System.Text;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Auth.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Backend.Infrastructure.Persistence;
using Org.Backend.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add password hasher for seeding and authentication
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// Add JWT authentication
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] 
    ?? throw new InvalidOperationException("JWT SigningKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add application services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Phase 4A-2A services
builder.Services.AddScoped<Org.Backend.Features.Users.Services.IUserService, Org.Backend.Features.Users.Services.UserService>();
builder.Services.AddScoped<Org.Backend.Features.Organizations.Services.IOrganizationService, Org.Backend.Features.Organizations.Services.OrganizationService>();
builder.Services.AddScoped<Org.Backend.Features.RolesPermissions.Services.IRoleService, Org.Backend.Features.RolesPermissions.Services.RoleService>();

// Phase 4A-3 services
builder.Services.AddScoped<Org.Backend.Features.Members.Services.IMemberService, Org.Backend.Features.Members.Services.MemberService>();
builder.Services.AddScoped<Org.Backend.Features.Departments.Services.IDepartmentService, Org.Backend.Features.Departments.Services.DepartmentService>();
builder.Services.AddScoped<Org.Backend.Features.Events.Services.IEventService, Org.Backend.Features.Events.Services.EventService>();

// Phase 4A-4 services
builder.Services.AddScoped<Org.Backend.Features.Milestones.Services.IMilestoneService, Org.Backend.Features.Milestones.Services.MilestoneService>();
builder.Services.AddScoped<Org.Backend.Features.EventCategories.Services.IEventCategoryService, Org.Backend.Features.EventCategories.Services.EventCategoryService>();
builder.Services.AddScoped<Org.Backend.Features.Tasks.Services.ITaskService, Org.Backend.Features.Tasks.Services.TaskService>();

// BE-FINAL-2 supporting services
builder.Services.AddScoped<Org.Backend.Features.Requests.Services.IRequestService, Org.Backend.Features.Requests.Services.RequestService>();
builder.Services.AddScoped<Org.Backend.Features.Notifications.Services.INotificationService, Org.Backend.Features.Notifications.Services.NotificationService>();
builder.Services.AddScoped<Org.Backend.Features.Friends.Services.IFriendService, Org.Backend.Features.Friends.Services.FriendService>();
builder.Services.AddScoped<Org.Backend.Features.Discover.Services.IDiscoverService, Org.Backend.Features.Discover.Services.DiscoverService>();

// Add FastEndpoints
builder.Services.AddFastEndpoints();

// Add dev data seeder (development only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<DevDataSeeder>();
}

var app = builder.Build();

// Development-only database migration and seeding
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Apply migrations (development only)
        // Note: For production, migrations should be applied via CLI/CI pipeline
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("[Migration] Database migrated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Migration] Warning: Could not apply migrations: {ex.Message}");
            Console.WriteLine("[Migration] Attempting to ensure database exists...");
            dbContext.Database.EnsureCreated();
        }
        
        // Seed development data
        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
            await seeder.SeedAsync();
            Console.WriteLine("[Seeder] Development data seeded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seeder] Warning: Could not seed development data: {ex.Message}");
        }
    }
}

// Configure the HTTP request pipeline
app.UseCors();

// CRITICAL: Middleware order matters
app.UseAuthentication();
app.UseAuthorization();

// Add FastEndpoints
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Endpoints.ShortNames = true;
});

app.MapGet("/", () => "PBL3 Rescue - Phase 4A-1 Auth Backend Implementation");

app.Run();
