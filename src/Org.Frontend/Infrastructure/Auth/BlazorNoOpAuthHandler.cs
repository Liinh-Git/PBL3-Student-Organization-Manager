// ---- Handler giả để thỏa mãn yêu cầu của Authorization Middleware ----
// Blazor Server dùng [Authorize] trên Razor component tự kích hoạt AuthorizationMiddleware.
// Middleware đó cần IAuthenticationService + DefaultChallengeScheme hợp lệ.
// Handler này chỉ trả về "không auth" mà không redirect -- Blazor component tự xử lý redirect.
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Org.Frontend.Infrastructure.Auth;

public sealed class BlazorNoOpAuthOptions : AuthenticationSchemeOptions { }

public sealed class BlazorNoOpAuthHandler(
    IOptionsMonitor<BlazorNoOpAuthOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<BlazorNoOpAuthOptions>(options, logger, encoder)
{
    // ---- Luôn trả về NoResult: để Blazor-level AuthorizeRouteView quyết định redirect ----
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());

    // ---- Challenge: không làm gì cả, Blazor RedirectToLogin component sẽ điều hướng ----
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        => Task.CompletedTask;
}
