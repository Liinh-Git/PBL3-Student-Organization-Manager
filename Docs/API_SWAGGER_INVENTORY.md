# API Swagger Inventory (Live Runtime)

- Source: `Docs/swagger-live.json` (exported from `http://localhost:5058/swagger/v1/swagger.json`)
- Generated: 2026-05-06
- Total operations: 100

## Auth

### [POST] /api/auth/login

- Tags: Api
- OperationId: OrgBackendFeaturesAuthLoginEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesAuthLoginRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesAuthLoginResponse

### [GET] /api/auth/me

- Tags: Api
- OperationId: OrgBackendFeaturesAuthMeEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesAuthMeResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/auth/register

- Tags: Api
- OperationId: OrgBackendFeaturesAuthRegisterEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesAuthRegisterRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesAuthRegisterResponse

## Users

### [GET] /api/users/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesUsersGetUserByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetUserByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/users/{id}/friend-request

- Tags: Api
- OperationId: OrgBackendFeaturesUsersSendFriendRequestEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersFriendRequestDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/users/batch

- Tags: Api
- OperationId: OrgBackendFeaturesUsersGetUserProfilesBatchEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: array<string(guid)>
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetUserProfilesBatchResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/users/me

- Tags: Api
- OperationId: OrgBackendFeaturesUsersGetCurrentUserProfileEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetCurrentUserProfileResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/users/me

- Tags: Api
- OperationId: OrgBackendFeaturesUsersUpdateCurrentUserProfileEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesUsersUpdateCurrentUserProfileRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersUserProfileDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/users/me/change-password

- Tags: Api
- OperationId: OrgBackendFeaturesUsersChangePasswordEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesUsersChangePasswordRequest
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/users/me/discover/events

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetSuggestedEventsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetSuggestedEventsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/users/me/discover/organizations

- Tags: Api
- OperationId: OrgBackendFeaturesUsersGetSuggestedOrganizationsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetSuggestedOrganizationsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/users/me/events

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetMyRegisteredEventsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetMyRegisteredEventsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/users/me/friend-requests

- Tags: Api
- OperationId: OrgBackendFeaturesUsersGetFriendRequestsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetFriendRequestsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/users/me/friend-requests/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesUsersRejectFriendRequestEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/users/me/friend-requests/{id}/accept

- Tags: Api
- OperationId: OrgBackendFeaturesUsersAcceptFriendRequestEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersFriendRequestDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/users/me/friends

- Tags: Api
- OperationId: OrgBackendFeaturesUsersGetFriendsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetFriendsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/users/me/friends/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesUsersUnfriendEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/users/me/organizations

- Tags: Api
- OperationId: OrgBackendFeaturesUsersGetMyOrganizationsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesUsersGetMyOrganizationsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

## Organizations

### [GET] /api/organizations

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsGetOrganizationsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsGetOrganizationsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/organizations

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsCreateOrganizationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsCreateOrganizationRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsOrganizationDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/organizations/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsDeleteOrganizationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsGetOrganizationByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsGetOrganizationByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/organizations/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsUpdateOrganizationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsUpdateOrganizationRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsOrganizationDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/{id}/public-overview

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsGetPublicOrganizationOverviewEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsGetPublicOrganizationOverviewResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/organizations/{id}/restore

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsRestoreOrganizationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsOrganizationDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/default

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsGetDefaultOrganizationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsGetDefaultOrganizationResponse
  - 401 - Unauthorized
  - 403 - Forbidden

## Organization Roles/Permissions

### [POST] /api/organizations/{id}/members/{memberId}/role

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsAssignRoleToOrganizationMemberEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
  - memberId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsAssignOrganizationRoleRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMembersMemberDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/{id}/permissions

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsGetOrganizationPermissionsCatalogEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsGetOrganizationPermissionsCatalogResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/{id}/permissions/me

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsGetOrganizationPermissionsMeEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsGetOrganizationPermissionsMeResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/{id}/roles

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsGetOrganizationRolesEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsGetOrganizationRolesResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/organizations/{id}/roles

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsCreateOrganizationRoleEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsUpsertOrganizationRoleRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsOrganizationRoleDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/organizations/roles/{roleId}

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsDeleteOrganizationRoleEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - roleId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/organizations/roles/{roleId}

- Tags: Api
- OperationId: OrgBackendFeaturesOrganizationsUpdateOrganizationRoleEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - roleId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsUpsertOrganizationRoleRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesOrganizationsOrganizationRoleDto
  - 401 - Unauthorized
  - 403 - Forbidden

## Members

### [DELETE] /api/members/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesMembersDeleteMemberEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/members/{id}/department

- Tags: Api
- OperationId: OrgBackendFeaturesMembersUpdateMemberDepartmentEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesMembersUpdateMemberDepartmentRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMembersMemberDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/members/{id}/role

- Tags: Api
- OperationId: OrgBackendFeaturesMembersUpdateMemberRoleEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesMembersUpdateMemberRoleRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMembersMemberDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/organizations/{orgId}/leave

- Tags: Api
- OperationId: OrgBackendFeaturesMembersLeaveOrganizationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - orgId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/{orgId}/members

- Tags: Api
- OperationId: OrgBackendFeaturesMembersGetMembersEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - orgId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMembersGetMembersResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/organizations/{orgId}/members

- Tags: Api
- OperationId: OrgBackendFeaturesMembersCreateMemberEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - orgId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesMembersCreateMemberRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMembersMemberDto
  - 401 - Unauthorized
  - 403 - Forbidden

## Departments

### [POST] /api/departments

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsCreateDepartmentEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsCreateDepartmentRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsDepartmentDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/departments/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsDeleteDepartmentEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/departments/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsGetDepartmentByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsGetDepartmentByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/departments/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsUpdateDepartmentEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsUpdateDepartmentRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsDepartmentDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/departments/{id}/manager

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsUpdateDepartmentManagerEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsUpdateDepartmentManagerRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsDepartmentDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/departments/{id}/members

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsGetDepartmentMembersEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsGetDepartmentMembersResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/departments/{id}/members/{memberId}

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsRemoveDepartmentMemberEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
  - memberId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/departments/{id}/members/{memberId}

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsAssignDepartmentMemberEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
  - memberId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMembersMemberDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/departments/{id}/restore

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsRestoreDepartmentEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsDepartmentDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/departments/{id}/tasks/overview

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsGetDepartmentTasksOverviewEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsGetDepartmentTasksOverviewResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/{orgId}/departments

- Tags: Api
- OperationId: OrgBackendFeaturesDepartmentsGetDepartmentsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - orgId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesDepartmentsGetDepartmentsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

## Events

### [POST] /api/events

- Tags: Api
- OperationId: OrgBackendFeaturesEventsCreateEventEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesEventsCreateEventRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsEventDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/events/{eventId}/ratings

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetEventRatingsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - eventId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsGetEventRatingsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/events/{eventId}/ratings

- Tags: Api
- OperationId: OrgBackendFeaturesEventsCreateEventRatingEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - eventId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesEventsCreateEventRatingRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsEventRatingDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/events/{eventId}/ratings/stats

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetEventRatingStatsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - eventId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsGetEventRatingStatsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/events/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesEventsDeleteEventEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/events/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetEventByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsGetEventByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/events/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesEventsUpdateEventEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesEventsUpdateEventRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsEventDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/events/{id}/public

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetPublicEventByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsGetEventByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/events/{id}/restore

- Tags: Api
- OperationId: OrgBackendFeaturesEventsRestoreEventEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsEventDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/events/{id}/visibility

- Tags: Api
- OperationId: OrgBackendFeaturesEventsUpdateEventVisibilityEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesEventsUpdateEventVisibilityRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsEventDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/events/public

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetPublicEventsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsGetPublicEventsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/ratings/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesEventsDeleteEventRatingEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

## Milestones

### [GET] /api/events/{eventId}/milestones

- Tags: Api
- OperationId: OrgBackendFeaturesMilestonesGetMilestonesEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - eventId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMilestonesGetMilestonesResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/events/{eventId}/milestones

- Tags: Api
- OperationId: OrgBackendFeaturesMilestonesCreateMilestoneEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - eventId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesMilestonesCreateMilestoneRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMilestonesMilestoneDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/milestones/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesMilestonesDeleteMilestoneEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/milestones/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesMilestonesGetMilestoneByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMilestonesGetMilestoneByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/milestones/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesMilestonesUpdateMilestoneEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesMilestonesUpdateMilestoneRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMilestonesMilestoneDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/milestones/{id}/restore

- Tags: Api
- OperationId: OrgBackendFeaturesMilestonesRestoreMilestoneEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesMilestonesMilestoneDto
  - 401 - Unauthorized
  - 403 - Forbidden

## Event Categories

### [DELETE] /api/categories/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesEventCategoriesDeleteEventCategoryEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/categories/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesEventCategoriesGetEventCategoryByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventCategoriesGetEventCategoryByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/categories/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesEventCategoriesUpdateEventCategoryEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesEventCategoriesUpdateEventCategoryRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventCategoriesEventCategoryDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/categories/{id}/restore

- Tags: Api
- OperationId: OrgBackendFeaturesEventCategoriesRestoreEventCategoryEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventCategoriesEventCategoryDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/milestones/{milestoneId}/categories

- Tags: Api
- OperationId: OrgBackendFeaturesEventCategoriesGetEventCategoriesEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - milestoneId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventCategoriesGetEventCategoriesResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/milestones/{milestoneId}/categories

- Tags: Api
- OperationId: OrgBackendFeaturesEventCategoriesCreateEventCategoryEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - milestoneId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesEventCategoriesCreateEventCategoryRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventCategoriesEventCategoryDto
  - 401 - Unauthorized
  - 403 - Forbidden

## Tasks

### [GET] /api/categories/{categoryId}/tasks

- Tags: Api
- OperationId: OrgBackendFeaturesTasksGetTasksEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - categoryId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesTasksGetTasksResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/categories/{categoryId}/tasks

- Tags: Api
- OperationId: OrgBackendFeaturesTasksCreateTaskEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - categoryId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesTasksCreateTaskRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesTasksTaskDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/tasks/{taskId}

- Tags: Api
- OperationId: OrgBackendFeaturesTasksDeleteTaskEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - taskId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/tasks/{taskId}

- Tags: Api
- OperationId: OrgBackendFeaturesTasksGetTaskByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - taskId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesTasksGetTaskByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/tasks/{taskId}

- Tags: Api
- OperationId: OrgBackendFeaturesTasksUpdateTaskEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - taskId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesTasksUpdateTaskRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesTasksTaskDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/tasks/{taskId}/assign

- Tags: Api
- OperationId: OrgBackendFeaturesTasksUpdateTaskAssignEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - taskId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesTasksAssignTaskRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesTasksTaskDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/tasks/{taskId}/restore

- Tags: Api
- OperationId: OrgBackendFeaturesTasksRestoreTaskEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - taskId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesTasksTaskDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/tasks/{taskId}/status

- Tags: Api
- OperationId: OrgBackendFeaturesTasksUpdateTaskStatusEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - taskId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesTasksUpdateTaskStatusRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesTasksTaskDto
  - 401 - Unauthorized
  - 403 - Forbidden

## Requests

### [GET] /api/organizations/{id}/requests

- Tags: Api
- OperationId: OrgBackendFeaturesRequestsGetOrganizationRequestsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesRequestsGetOrganizationRequestsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/organizations/{id}/requests

- Tags: Api
- OperationId: OrgBackendFeaturesRequestsCreateOrganizationRequestEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesRequestsCreateOrganizationRequestSubmissionRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesRequestsOrganizationRequestDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/organizations/requests/{requestId}

- Tags: Api
- OperationId: OrgBackendFeaturesRequestsGetOrganizationRequestByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - requestId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesRequestsGetOrganizationRequestByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/organizations/requests/{requestId}/review

- Tags: Api
- OperationId: OrgBackendFeaturesRequestsReviewOrganizationRequestEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - requestId [path] required :: string(guid)
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesRequestsReviewOrganizationRequestSubmissionRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesRequestsOrganizationRequestDto
  - 401 - Unauthorized
  - 403 - Forbidden

## Notifications

### [GET] /api/notifications

- Tags: Api
- OperationId: OrgBackendFeaturesNotificationsGetNotificationsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - Page [query] optional :: integer(int32)
  - PageSize [query] optional :: integer(int32)
  - IsRead [query] optional :: boolean
  - Type [query] optional :: string
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesNotificationsGetNotificationsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/notifications/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesNotificationsDeleteNotificationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/notifications/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesNotificationsGetNotificationByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesNotificationsGetNotificationByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/notifications/{id}/read

- Tags: Api
- OperationId: OrgBackendFeaturesNotificationsMarkAsReadEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesNotificationsMarkAsReadResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/notifications/clear-all

- Tags: Api
- OperationId: OrgBackendFeaturesNotificationsClearAllNotificationsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - */*: #/components/schemas/OrgSharedFeaturesNotificationsClearNotificationsRequest
  - application/json: #/components/schemas/OrgSharedFeaturesNotificationsClearNotificationsRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesNotificationsClearNotificationsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [PUT] /api/notifications/read-all

- Tags: Api
- OperationId: OrgBackendFeaturesNotificationsMarkAllAsReadEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesNotificationsMarkAllAsReadResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/notifications/unread-count

- Tags: Api
- OperationId: OrgBackendFeaturesNotificationsGetUnreadCountEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesNotificationsGetUnreadCountResponse
  - 401 - Unauthorized
  - 403 - Forbidden

## Posts

### [GET] /api/organizations/{orgId}/posts

- Tags: Api
- OperationId: OrgBackendFeaturesPostsGetOrganizationPostsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - orgId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesPostsGetPostsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [POST] /api/posts

- Tags: Api
- OperationId: OrgBackendFeaturesPostsCreatePostEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - required: True
  - application/json: #/components/schemas/OrgSharedFeaturesPostsCreatePostRequest
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesPostsOrganizationPostDto
  - 401 - Unauthorized
  - 403 - Forbidden

### [DELETE] /api/posts/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesPostsDeletePostEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 204 - No Content
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/posts/{id}

- Tags: Api
- OperationId: OrgBackendFeaturesPostsGetPostByIdEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - id [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesPostsGetPostByIdResponse
  - 401 - Unauthorized
  - 403 - Forbidden

### [GET] /api/posts/discover

- Tags: Api
- OperationId: OrgBackendFeaturesPostsGetDiscoverPostsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesPostsGetDiscoverPostsResponse
  - 401 - Unauthorized
  - 403 - Forbidden

## Admin

### [POST] /api/admin/apply-migration

- Tags: Api
- OperationId: OrgBackendFeaturesAdminApplyMigrationEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - none
- Request body:
  - none
- Responses:
  - 204 - No Content

## Other/Unknown

### [GET] /api/organizations/{orgId}/events

- Tags: Api
- OperationId: OrgBackendFeaturesEventsGetOrganizationEventsEndpoint
- Summary: (none)
- Security (Swagger): none declared on operation
- Parameters:
  - orgId [path] required :: string(guid)
- Request body:
  - none
- Responses:
  - 200 - Success - application/json: #/components/schemas/OrgSharedFeaturesEventsGetOrganizationEventsResponse
  - 401 - Unauthorized
  - 403 - Forbidden


