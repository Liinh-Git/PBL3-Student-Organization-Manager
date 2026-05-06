using System.Net.Http.Json;

namespace Org.Frontend.Services.Auth;

public interface IAuthenticatedBackendClient
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default);

    Task<T?> GetFromJsonAsync<T>(string uri, CancellationToken ct = default);

    Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(string uri, TRequest body, CancellationToken ct = default);

    Task<TResponse?> PutAsJsonAsync<TRequest, TResponse>(string uri, TRequest body, CancellationToken ct = default);

    Task DeleteAsync(string uri, CancellationToken ct = default);
}
