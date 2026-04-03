// ---- Gom đăng ký service startup để Program.cs chỉ còn orchestration ----
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.Backend.Infrastructure.Auth;
using Org.Backend.Infrastructure.Database;
using System.Text;

namespace Org.Backend.Infrastructure.Startup;

public static class ServiceRegistrationExtensions
{
    // ---- Đăng ký service lõi: DbContext, CORS, JWT và Authorization ----
    public static IServiceCollection AddAppCoreServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // ---- DbContext dùng PostgreSQL theo connection string hiện hành ----
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // ---- CORS policy cho frontend; fallback AllowAny* khi chưa cấu hình origin ----
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                    return;
                }

                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // ---- Bind JWT options + service tạo access token ----
        services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var hasWeakSigningKey = string.IsNullOrWhiteSpace(jwtOptions.SigningKey)
            || jwtOptions.SigningKey.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
            || jwtOptions.SigningKey.Length < 32;

        // ---- Chặn startup production nếu signing key yếu để tránh lộ bảo mật ----
        if (hasWeakSigningKey && !builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured with a strong secret (at least 32 characters).");
        }

        // ---- Cảnh báo dev khi còn dùng key placeholder ----
        if (hasWeakSigningKey && builder.Environment.IsDevelopment())
        {
            Console.WriteLine("[WARN] Jwt:SigningKey is using a weak placeholder. Override it using user-secrets before sharing this environment.");
        }

        var jwtSigningKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

        // ---- Đăng ký JWT bearer authentication + policy-based authorization ----
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtSigningKey),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }

    // ---- Chỉ bật runtime API khi chạy web mode (không phải seed mode) ----
    public static IServiceCollection AddAppApiRuntime(this IServiceCollection services, bool isSeedMode)
    {
        if (isSeedMode)
        {
            // Seed mode chỉ cần DI lõi để migrate + seed, không cần endpoint runtime.
            return services;
        }

        services.AddFastEndpoints();
        services.AddOpenApi();

        return services;
    }
}
