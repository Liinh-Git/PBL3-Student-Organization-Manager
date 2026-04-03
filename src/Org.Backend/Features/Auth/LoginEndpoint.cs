// ---- Endpoint đăng nhập: xác thực thông tin và cấp JWT ----
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Auth;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth;

public sealed class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;

    // ---- Inject DbContext và service tạo JWT để dùng trong luồng đăng nhập ----
    public LoginEndpoint(AppDbContext db, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    // ---- Cấu hình route cho đăng nhập và cho phép anonymous ----
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    // ---- Xử lý đăng nhập: tìm user -> kiểm tra mật khẩu -> cấp JWT -> trả kết quả ----
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // Bước 1: chuẩn hóa email đầu vào
        var email = req.Email.Trim().ToLowerInvariant();

        // Bước 2: tìm user theo email trong database
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        // Nếu không có user thì trả về lỗi xác thực 401
        if (user is null)
            ThrowError("Invalid credentials.", StatusCodes.Status401Unauthorized);

        // Bước 3: so khớp mật khẩu người dùng nhập với mật khẩu đã băm
        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user!.PasswordHash);
        if (!ok)
            ThrowError("Invalid credentials.", StatusCodes.Status401Unauthorized);

        // Bước 4: cập nhật lần đăng nhập gần nhất
        user.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Bước 5: tạo access token mới cho user vừa xác thực thành công
        var (token, expiresAtUtc) = _jwtTokenService.CreateAccessToken(user);

        // Bước 6: trả về token và thông tin cơ bản của user cho frontend
        await Send.OkAsync(new LoginResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email
        }, ct);
    }
}
