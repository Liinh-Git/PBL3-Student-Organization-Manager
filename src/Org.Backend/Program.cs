// ---- Backend startup: kết nối DB, cấu hình JWT, CORS, và FastEndpoints ----
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.Backend.Domain.Entities;
using Org.Backend.Infrastructure.Auth;
using Org.Backend.Infrastructure.Database;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---- Đăng ký AppDbContext sử dụng PostgreSQL ----
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

// ---- Cấu hình CORS cho frontend ----
builder.Services.AddCors(options =>
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

// ---- Bind JWT options và đăng ký service tạo token ----
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var hasWeakSigningKey = string.IsNullOrWhiteSpace(jwtOptions.SigningKey)
    || jwtOptions.SigningKey.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
    || jwtOptions.SigningKey.Length < 32;

// ---- Bắt lỗi nếu khóa ký quá yếu trong production ----
if (hasWeakSigningKey && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Jwt:SigningKey must be configured with a strong secret (at least 32 characters).");

// ---- Cảnh báo trong dev để tránh lộ mật khẩu ----
if (hasWeakSigningKey && builder.Environment.IsDevelopment())
    Console.WriteLine("[WARN] Jwt:SigningKey is using a weak placeholder. Override it using user-secrets before sharing this environment.");

var jwtSigningKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

// ---- Cấu hình JWT bearer authentication ----
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

// ---- Bật authorization (policy-based) ----
builder.Services.AddAuthorization();

// ---- FastEndpoints + OpenAPI ----
builder.Services.AddFastEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();

// ---- OpenAPI chỉ trong dev ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ---- Middleware pipeline ----
app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();
