/**
 * organizationService.js - Organization CRUD and public overview service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - getMyOrganizations belongs to userService, NOT here
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * List organizations (admin/system-level list)
 * 
 * Backend route: GET /api/organizations
 * Frontend path: /organizations
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<List<OrganizationSummaryDto>>
 * Permission:
 * - JWT token required
 * Rules:
 * - This is NOT the same as getMyOrganizations
 * - May be admin-only or system-level list
 */
export async function listOrganizations() {
  const response = await httpClient.get('/organizations');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to list organizations');
  }
  
  return response.data.data; // Direct array, not data.items
}

/**
 * Create organization
 * 
 * Backend route: POST /api/organizations
 * Frontend path: /organizations
 * Input:
 * - payload: { orgName: string, description?: string, avatarUrl?: string, coverUrl?: string, foundingDate?: string, location?: string, contactEmail?: string, contactPhone?: string }
 * Response:
 * - ApiResponse<OrganizationDto>
 * Permission:
 * - JWT token required
 * Rules:
 * - OrgName uniqueness is enforced at service level
 * - Creator becomes first member with President role
 */
export async function createOrganization(payload) {
  const response = await httpClient.post('/organizations', payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create organization');
  }
  
  return response.data.data;
}

/**
 * Get default organization (if user has one)
 * 
 * Backend route: GET /api/organizations/default
 * Frontend path: /organizations/default
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<OrganizationDto>
 * Permission:
 * - JWT token required
 * Rules:
 * - Returns user's default/primary organization if set
 * - May return 404 if no default organization
 */
export async function getDefaultOrganization() {
  const response = await httpClient.get('/organizations/default');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get default organization');
  }
  
  return response.data.data;
}

/**
 * Get organization by ID (workspace context)
 * 
 * Backend route: GET /api/organizations/{id}
 * Frontend path: /organizations/{id}
 * Input:
 * - id: string (organization ID)
 * Response:
 * - ApiResponse<OrganizationDto>
 * Permission:
 * - org.workspace.access (member-only)
 * Rules:
 * - This is workspace context, requires membership
 * - For public overview, use getPublicOverview instead
 */
export async function getOrganizationById(id) {
  const response = await httpClient.get(`/organizations/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get organization');
  }
  
  return response.data.data;
}

/**
 * Update organization
 * 
 * Backend route: PUT /api/organizations/{id}
 * Frontend path: /organizations/{id}
 * Input:
 * - id: string (organization ID)
 * - payload: { orgName?: string, description?: string, avatarUrl?: string, coverUrl?: string, foundingDate?: string, location?: string, contactEmail?: string, contactPhone?: string }
 * Response:
 * - ApiResponse<OrganizationDto>
 * Permission:
 * - org.overview.write
 * Rules:
 * - OrgName uniqueness is enforced at service level
 */
export async function updateOrganization(id, payload) {
  const response = await httpClient.put(`/organizations/${id}`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update organization');
  }
  
  return response.data.data;
}

/**
 * Upload organization image and update avatar/cover URL in backend
 *
 * Backend route: POST /api/organizations/{id}/upload-image
 * Frontend path: /organizations/{id}/upload-image
 * Input:
 * - id: string (organization ID)
 * - file: File
 * - type: "avatar" | "cover"
 * Response:
 * - ApiResponse<OrganizationDto>
 */
export async function uploadOrganizationImage(id, file, type) {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('type', type);

  let response;
  try {
    response = await httpClient.post(`/organizations/${id}/upload-image`, formData);
  } catch (error) {
    const message =
      error.response?.data?.message ||
      error.response?.data?.errors?.[0] ||
      error.message ||
      'Failed to upload organization image';
    throw new Error(message);
  }

  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to upload organization image');
  }

  return response.data.data;
}

/**
 * Get organization public overview
 * 
 * Backend route: GET /api/organizations/{id}/public-overview
 * Frontend path: /organizations/{id}/public-overview
 * Input:
 * - id: string (organization ID)
 * Response:
 * - ApiResponse<OrganizationPublicOverviewDto>
 * Permission:
 * - Public or authenticated-public (no membership required)
 * Rules:
 * - This is public overview, does NOT require membership
 * - Returns limited public information
 * - Used by OrgOverviewPage before loading workspace context
 */
export async function getPublicOverview(id) {
  const response = await httpClient.get(`/organizations/${id}/public-overview`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get public overview');
  }
  
  return response.data.data;
}

/**
 * Delete organization
 * 
 * Backend route: DELETE /api/organizations/{id}
 * Frontend path: /organizations/{id}
 * Input:
 * - id: string (organization ID)
 * Response:
 * - ApiResponse<bool>
 * Permission:
 * - President only (role level 1)
 * Rules:
 * - Only President can delete organization
 * - Cascade deletes all related data (members, roles, events, departments, etc.)
 */
export async function deleteOrganization(id) {
  const response = await httpClient.delete(`/organizations/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete organization');
  }
  
  return response.data.data;
}
