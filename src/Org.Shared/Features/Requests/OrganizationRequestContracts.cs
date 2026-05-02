namespace Org.Shared.Features.Requests;

public sealed record OrganizationRequestDto(
    Guid Id,
    Guid OrganizationId,
    Guid RequesterUserId,
    string RequesterName,
    string RequesterEmail,
    string? RequesterAvatarUrl,
    string RequestType,
    string Status,
    string? Title,
    string? Message,
    string? DesiredDepartment,
    string? DesiredPosition,
    string? Experience,
    string? Strengths,
    string? Reason,
    string? ReviewResponse,
    Guid? ReviewedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReviewedAtUtc);

public sealed record GetOrganizationRequestsResponse(IReadOnlyList<OrganizationRequestDto> Items);

public sealed record GetOrganizationRequestByIdResponse(OrganizationRequestDto Data);

public sealed record CreateOrganizationRequestSubmissionRequest(
    string RequestType,
    string? Title,
    string? Message,
    string? DesiredDepartment,
    string? DesiredPosition,
    string? Experience,
    string? Strengths,
    string? Reason);

public sealed record ReviewOrganizationRequestSubmissionRequest(
    string Decision,
    string? ResponseMessage);
