/**
 * requestService.js - Request management service (join organization workflow)
 * 
 * Phase 3C-4B: Service skeleton only
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - No real API calls yet
 * - No mock data, no fake success
 */

// import httpClient from '../api/httpClient.js';

/**
 * Get organization requests
 * 
 * TODO Phase implementation:
 * Backend route: GET /api/organizations/{orgId}/requests
 * Frontend path later: /organizations/{orgId}/requests
 * Input:
 * - orgId: string (from query string ?orgId=)
 * - params: { page?: number, pageSize?: number, requestType?: string, status?: string }
 * Response:
 * - ApiResponse<ListResponse<RequestDto>>
 * Adapter:
 * - requestAdapter.toRequestListViewModel
 * Permission:
 * - org.requests.view
 * Rules:
 * - Returns requests for the organization
 * - Used by OrgRequestsPage to list pending/reviewed requests
 */
export async function getOrganizationRequests(orgId, params = {}) {
  throw new Error('TODO: implement getOrganizationRequests after API contract verification');
}

/**
 * Create organization request (join request)
 * 
 * TODO Phase implementation:
 * Backend route: POST /api/organizations/{orgId}/requests
 * Frontend path later: /organizations/{orgId}/requests
 * Input:
 * - orgId: string
 * - payload: { requestType: string, title?: string, content: string, desiredDepartmentId?: string, desiredPosition?: string }
 * Response:
 * - ApiResponse<RequestDto>
 * Adapter:
 * - requestAdapter.toRequestViewModel
 * Permission:
 * - JWT token required (any authenticated user can submit)
 * Rules:
 * - requestType values: JoinOrganization, DepartmentChange, RoleChange, EventParticipation, Other
 * - content is required
 */
export async function createOrganizationRequest(orgId, payload) {
  throw new Error('TODO: implement createOrganizationRequest after API contract verification');
}

/**
 * Get request by ID
 * 
 * TODO Phase implementation:
 * Backend route: GET /api/requests/{requestId}
 * Frontend path later: /requests/{requestId}
 * Input:
 * - requestId: string
 * Response:
 * - ApiResponse<RequestDto>
 * Adapter:
 * - requestAdapter.toRequestViewModel
 * Permission:
 * - org.requests.view or request sender
 * Rules:
 * - Returns request with sender and reviewer data
 */
export async function getRequestById(requestId) {
  throw new Error('TODO: implement getRequestById after API contract verification');
}

/**
 * Review request (approve/reject)
 * 
 * TODO Phase implementation:
 * Backend route: POST /api/organizations/requests/{requestId}/review
 * Frontend path later: /organizations/requests/{requestId}/review
 * Input:
 * - requestId: string
 * - payload: { status: string, reviewNote?: string }
 * Response:
 * - ApiResponse<RequestDto>
 * Adapter:
 * - requestAdapter.toRequestViewModel
 * Permission:
 * - org.requests.review or org.requests.approve
 * Rules:
 * - status values: Approved, Rejected
 * - reviewNote is optional
 * - On Approved for JoinOrganization, create Member record
 */
export async function reviewRequest(requestId, payload) {
  throw new Error('TODO: implement reviewRequest after API contract verification');
}
