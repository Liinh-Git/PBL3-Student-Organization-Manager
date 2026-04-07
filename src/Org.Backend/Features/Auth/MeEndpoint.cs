// ---- Endpoint me: lấy thông tin user hiện tại từ token ----
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Auth;
using System.Security.Claims;

namespace Org.Backend.Features.Auth;

public sealed class MeEndpoint : EndpointWithoutRequest<MeResponse>
{
    private readonly AppDbContext _db;

    // ---- Inject DbContext để lấy thông tin user hiện tại ----
    public MeEndpoint(AppDbContext db)
    {
        _db = db;
    }

    // ---- Cấu hình route và bật JWT bearer auth ----
    public override void Configure()
    {
        Get("/api/auth/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    // ---- Xử lý: đọc userId từ claim -> lấy user -> trả thông tin ----
    public override async Task HandleAsync(CancellationToken ct)
    {
        // Bước 1: lấy userId từ claim NameIdentifier trong access token
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        // Bước 2: truy vấn user theo userId lấy từ token
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            ThrowError("User not found.", StatusCodes.Status404NotFound);

        // Bước 3: trả thông tin hồ sơ để frontend hiển thị trạng thái đăng nhập
        await Send.OkAsync(new MeResponse
        {
            UserId = user!.Id,
            FullName = user.FullName,
            Email = user.Email,
            Status = user.Status.ToString()
        }, ct);
    }
}
