/**
 * requestService.js - Request management service (join organization workflow)
 * 
 * Phase 4B-1B: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';
import { toRequestListViewModel, toRequestViewModel } from '../adapters/requestAdapter.js';

function getApiErrorMessage(responseData, fallbackMessage) {
  if (!responseData) return fallbackMessage;
  if (responseData.message) return responseData.message;
  if (Array.isArray(responseData.errors) && responseData.errors.length > 0) {
    return responseData.errors.join(', ');
  }
  return fallbackMessage;
}

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
  const response = await httpClient.get(`/organizations/${orgId}/requests`, { params });

  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to get organization requests'));
  }

  return toRequestListViewModel(response.data.data);
}

/**
 * Create organization request (join request)
 * 
 * Backend route: POST /api/organizations/{orgId}/requests
 * Frontend path: /organizations/{orgId}/requests
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
 * Backend Contract:
 * - Backend expects: { orgId: guid, request: { requestType, content, ... } }
 */
export async function createOrganizationRequest(orgId, payload) {
  try {
    // Wrap payload to match backend contract: CreateRequestEndpointRequest
    const requestBody = {
      orgId: orgId,
      request: payload
    };

    const response = await httpClient.post(`/organizations/${orgId}/requests`, requestBody);

    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to create request'));
    }

    return response.data.data;
  } catch (error) {
    const responseData = error?.response?.data;
    throw new Error(getApiErrorMessage(responseData, error?.message || 'Failed to create request'));
  }
}

export async function getMyPendingJoinRequests() {
  const response = await httpClient.get('/users/me/requests/pending-join');

  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to get pending join requests'));
  }

  return Array.isArray(response.data.data) ? response.data.data : [];
}

export async function withdrawOrganizationJoinRequest(orgId) {
  const response = await httpClient.post(`/organizations/${orgId}/requests/withdraw`, { orgId });

  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to withdraw join request'));
  }

  return !!response.data.data;
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
  const response = await httpClient.get(`/requests/${requestId}`);

  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to get request'));
  }

  return toRequestViewModel(response.data.data);
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
  const requestBody = {
    requestId,
    review: {
      decision: payload?.decision,
      reviewNote: payload?.reviewNote || undefined
    }
  };

  const response = await httpClient.post(`/organizations/requests/${requestId}/review`, requestBody);

  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to review request'));
  }

  return toRequestViewModel(response.data.data);
}
