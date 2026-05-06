using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Frontend.ViewModels;
using Org.Shared.Features.Organizations;
using Org.Shared.Features.Requests;

namespace Org.Frontend.Services.Requests;

public sealed class RequestApiClient(IAuthenticatedBackendClient backendClient) : IRequestService
{
    private readonly IAuthenticatedBackendClient _backendClient = backendClient;

    public async Task<bool> CanViewOrganizationRequestsAsync(Guid orgId, CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationPermissionsMeResponse>(
            $"api/organizations/{orgId:D}/permissions/me",
            ct);
        return payload?.Data.CanViewRequests ?? false;
    }

    public async Task<bool> CanReviewOrganizationRequestsAsync(Guid orgId, CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationPermissionsMeResponse>(
            $"api/organizations/{orgId:D}/permissions/me",
            ct);
        return payload?.Data.CanReviewRequests ?? false;
    }

    public async Task<List<RequestViewModel>> GetPendingRequestsAsync(Guid orgId, CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationRequestsResponse>(
            $"api/organizations/{orgId:D}/requests?status=PENDING",
            ct) ?? new GetOrganizationRequestsResponse([]);

        return payload.Items.Select(MapToRequestViewModel).ToList();
    }

    public async Task<RequestDetailViewModel?> GetRequestDetailAsync(Guid requestId, CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationRequestByIdResponse>(
            $"api/organizations/requests/{requestId:D}",
            ct);

        if (payload is null)
            return null;

        return MapToRequestDetailViewModel(payload.Data);
    }

    public Task ApproveRequestAsync(Guid requestId, CancellationToken ct = default)
        => ReviewRequestAsync(requestId, decision: "APPROVE", ct);

    public Task RejectRequestAsync(Guid requestId, CancellationToken ct = default)
        => ReviewRequestAsync(requestId, decision: "REJECT", ct);

    public async Task SubmitJoinRequestAsync(JoinRequestFormViewModel form, CancellationToken ct = default)
    {
        var payload = new CreateOrganizationRequestSubmissionRequest(
            RequestType: "JOIN",
            Title: "Join organization request",
            Message: form.Reason,
            DesiredDepartment: form.DesiredDepartmentName,
            DesiredPosition: form.DesiredPosition,
            Experience: form.Experience,
            Strengths: form.Strengths,
            Reason: form.Reason);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/organizations/{form.OrgId:D}/requests")
        {
            Content = JsonContent.Create(payload)
        };
        using var _ = await _backendClient.SendAsync(request, ct);
    }

    public async Task SubmitOrganizationRequestAsync(CreateOrganizationRequestViewModel form, CancellationToken ct = default)
    {
        var payload = new CreateOrganizationRequestSubmissionRequest(
            RequestType: string.IsNullOrWhiteSpace(form.RequestType) ? "GeneralOrgRequest" : form.RequestType.Trim(),
            Title: form.Title,
            Message: form.Message,
            DesiredDepartment: null,
            DesiredPosition: null,
            Experience: null,
            Strengths: null,
            Reason: null);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/organizations/{form.OrgId:D}/requests")
        {
            Content = JsonContent.Create(payload)
        };
        using var _ = await _backendClient.SendAsync(request, ct);
    }

    private async Task ReviewRequestAsync(Guid requestId, string decision, CancellationToken ct)
    {
        var payload = new ReviewOrganizationRequestSubmissionRequest(decision, null);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/organizations/requests/{requestId:D}/review")
        {
            Content = JsonContent.Create(payload)
        };
        using var _ = await _backendClient.SendAsync(request, ct);
    }

    private static RequestViewModel MapToRequestViewModel(OrganizationRequestDto item)
    {
        var tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.DesiredDepartment))
            tags.Add(item.DesiredDepartment.Trim());
        if (!string.IsNullOrWhiteSpace(item.DesiredPosition))
            tags.Add(item.DesiredPosition.Trim());

        return new RequestViewModel
        {
            Id = item.Id,
            OrgId = item.OrganizationId,
            UserId = item.RequesterUserId,
            UserName = item.RequesterName,
            Email = item.RequesterEmail,
            AvatarUrl = item.RequesterAvatarUrl,
            Tags = tags,
            RequestType = NormalizeRequestType(item.RequestType),
            Title = item.Title,
            Message = item.Message,
            CreatedAt = item.CreatedAtUtc.UtcDateTime,
            DesiredDepartment = item.DesiredDepartment,
            DesiredPosition = item.DesiredPosition,
            Experience = item.Experience
        };
    }

    private static RequestDetailViewModel MapToRequestDetailViewModel(OrganizationRequestDto item)
    {
        return new RequestDetailViewModel
        {
            Id = item.Id,
            OrgId = item.OrganizationId,
            UserId = item.RequesterUserId,
            UserName = item.RequesterName,
            Email = item.RequesterEmail,
            AvatarUrl = item.RequesterAvatarUrl,
            RequestType = NormalizeRequestType(item.RequestType),
            Title = item.Title,
            Message = item.Message,
            CreatedAt = item.CreatedAtUtc.UtcDateTime,
            DesiredDepartment = item.DesiredDepartment,
            DesiredPosition = item.DesiredPosition,
            Experience = item.Experience,
            Strengths = item.Strengths,
            Reason = item.Reason
        };
    }

    private static string NormalizeRequestType(string? requestType)
    {
        return requestType?.Trim().ToUpperInvariant() switch
        {
            "JOIN" => "JOIN",
            "JOINCLUB" => "JOIN",
            _ => requestType ?? "GENERAL_ORG_REQUEST"
        };
    }
}
