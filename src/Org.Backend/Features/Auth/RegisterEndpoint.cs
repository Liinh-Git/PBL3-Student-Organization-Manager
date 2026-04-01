// ---- Endpoint đăng ký: tạo tài khoản mới trong hệ thống ----
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth;

public sealed class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
{
    private readonly AppDbContext _db;

    // ---- Inject DbContext để truy cập dữ liệu người dùng ----
    public RegisterEndpoint(AppDbContext db)
    {
        _db = db;
    }

    // ---- Cấu hình route đăng ký và cho phép gọi không cần đăng nhập ----
    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
    }

    // ---- Xử lý đăng ký: kiểm tra dữ liệu -> trùng email -> băm mật khẩu -> lưu DB -> trả kết quả ----
    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        // Bước 1: chuẩn hóa email để tránh trùng do khác hoa/thường
        var email = req.Email.Trim().ToLowerInvariant();

        // Bước 2: validate dữ liệu đầu vào cơ bản họ tên tối thiểu 2 ký tự
        if (string.IsNullOrWhiteSpace(req.FullName) || req.FullName.Trim().Length < 2)
            ThrowError("FullName must be at least 2 characters.", StatusCodes.Status400BadRequest);

        // Validate định dạng email cơ bản
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            ThrowError("Email is invalid.", StatusCodes.Status400BadRequest);

        // Validate mật khẩu tối thiểu 8 ký tự
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            ThrowError("Password must be at least 8 characters.", StatusCodes.Status400BadRequest);

        // Bước 3: kiểm tra email đã tồn tại hay chưa
        var exists = await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct);
        if (exists)
            ThrowError("Email is already registered.", StatusCodes.Status409Conflict);

        // Bước 4: tạo user mới và băm mật khẩu trước khi lưu
        var user = new User
        {
            FullName = req.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Status = UserStatus.Active
        };

        // Bước 5: lưu user mới vào database
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        // Bước 6: trả dữ liệu cơ bản sau khi đăng ký thành công
        await Send.OkAsync(new RegisterResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email
        }, ct);
    }
}
