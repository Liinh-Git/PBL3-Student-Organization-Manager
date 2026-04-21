// ---- Client gọi API auth từ frontend (register, login, me) ----
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Org.Shared.Features.Auth;

namespace Org.Frontend.Services.Auth;

public sealed class AuthApiClient : IAuthService
{
    private readonly HttpClient _httpClient;

    // ---- Inject HttpClient đã được cấu hình BaseUrl ----
    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ---- Gọi API đăng ký ----
    public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        return SendAsync<RegisterResponse>(HttpMethod.Post, "/api/auth/register", request, ct);
    }

    // ---- Gọi API đăng nhập ----
    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        return SendAsync<LoginResponse>(HttpMethod.Post, "/api/auth/login", request, ct);
    }

    // ---- Gọi API me với access token ----
    public Task<MeResponse> GetMeAsync(string accessToken, CancellationToken ct = default)
    {
        return SendAsync<MeResponse>(HttpMethod.Get, "/api/auth/me", null, ct, accessToken);
    }

    // ---- Hàm dùng chung gửi request và parse response ----
    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string url,
        object? payload,
        CancellationToken ct,
        string? accessToken = null)
    {
        // Bước 1: tạo HttpRequestMessage theo method + endpoint
        using var request = new HttpRequestMessage(method, url);

        // Bước 2: gắn Bearer token nếu endpoint yêu cầu xác thực
        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Bước 3: gắn payload JSON cho request có body
        if (payload is not null)
            request.Content = JsonContent.Create(payload);

        // Bước 4: gửi request sang backend
        using var response = await _httpClient.SendAsync(request, ct);

        // Bước 5: nếu backend trả lỗi thì đọc message và ném exception
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ReadErrorMessageAsync(response, ct);
            throw new AuthApiException(errorMessage, (int)response.StatusCode);
        }

        // Bước 6: parse JSON thành DTO, lỗi nếu response rỗng
        var content = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        if (content is null)
            throw new AuthApiException("Empty response from authentication API.", (int)response.StatusCode);

        return content;
    }

    // ---- Thử đọc message lỗi từ backend, fallback nếu không có ----
    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var fallback = $"Authentication API failed with status code {(int)response.StatusCode}.";
        // Thử đọc raw body để lấy message lỗi do backend trả về
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(raw))
            return fallback;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("message", out var messageElement)
                    && messageElement.ValueKind == JsonValueKind.String)
                {
                    return messageElement.GetString() ?? fallback;
                }

                if (doc.RootElement.TryGetProperty("reason", out var reasonElement)
                    && reasonElement.ValueKind == JsonValueKind.String)
                {
                    return reasonElement.GetString() ?? fallback;
                }
            }
        }
        catch (JsonException)
        {
            // Nếu backend trả về plain text hoặc format không đúng, dùng raw text
        }

        return raw;
    }
}
