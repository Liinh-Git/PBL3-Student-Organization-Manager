/**
 * roleService.js - Role and permission management service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Role assignment belongs HERE, not in memberService
 * - Permission fallback must NEVER grant org.workspace.access
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Get my permissions in organization
 * 
 * Backend route: GET /api/organizations/{orgId}/permissions/me
 * Frontend path: /organizations/{orgId}/permissions/me
 * Input:
 * - orgId: string (from query string ?orgId=)
 * Response:
 * - ApiResponse<MyPermissionsResponse>
 * - MyPermissionsResponse: { permissionKeys: string[], roleId: string, roleName: string, memberId: string, organizationId: string }
 * Permission:
 * - JWT token required
 * Rules:
 * - May return 403 if user is not a member
 * - Response shape is confirmed from backend docs
 * - Use normalizePermissionKeys helper
 */
export async function getMyPermissions(orgId) {
  const response = await httpClient.get(`/organizations/${orgId}/permissions/me`);
  
  if (!response.data.success) {
    // If 403, user is not a member - return empty permissions
    if (response.status === 403) {
      return { permissionKeys: [], roleId: null, roleName: null, memberId: null, organizationId: orgId };
    }
    throw new Error(response.data.message || 'Failed to get permissions');
  }
  
  const data = response.data.data;
  // Normalize permission keys to array
  const permissionKeys = normalizePermissionKeys(data);
  
  return {
    permissionKeys,
    roleId: data.roleId || null,
    roleName: data.roleName || null,
    memberId: data.memberId || null,
    organizationId: data.organizationId || orgId
  };
}

/**
 * Normalize permission keys from various response shapes
 * 
 * This helper can be implemented safely as it does not call API.
 * It normalizes various possible response shapes to string[].
 * 
 * Accepted shapes:
 * - string[]
 * - { permissionKeys: string[] }
 * - { permissions: string[] }
 * - { data: string[] }
 * - { data: { permissionKeys: string[] } }
 * - { data: { permissions: string[] } }
 * 
 * Safe fallback:
 * - return [] (no permissions)
 * 
 * CRITICAL: Fallback must NEVER grant org.workspace.access or any write/manage permissions.
 */
export function normalizePermissionKeys(response) {
  if (Array.isArray(response)) return response;
  if (Array.isArray(response?.permissionKeys)) return response.permissionKeys;
  if (Array.isArray(response?.permissions)) return response.permissions;
  if (Array.isArray(response?.data)) return response.data;
  if (Array.isArray(response?.data?.permissionKeys)) return response.data.permissionKeys;
  if (Array.isArray(response?.data?.permissions)) return response.data.permissions;

  console.warn('[roleService] Cannot parse permissions, using safe fallback');
  return [];
}

/**
 * Get organization permissions (all available permissions)
 * 
 * TODO Phase implementation:
 * Backend route: GET /api/organizations/{orgId}/permissions
 * Frontend path later: /organizations/{orgId}/permissions
 * Input:
 * - orgId: string (from query string ?orgId=)
 * Response:
 * - ApiResponse<PermissionDto[]>
 * Adapter:
 * - roleAdapter.toPermissionViewModel (if needed)
 * Permission:
 * - org.roles.view
 * Rules:
 * - Returns all available permissions in the system
 * - Used for role creation/editing UI
 */
export async function getOrganizationPermissions(orgId) {
  throw new Error('TODO: implement getOrganizationPermissions after API contract verification');
}

/**
 * Get organization roles
 * 
 * Backend route: GET /api/organizations/{orgId}/roles
 * Frontend path: /organizations/{orgId}/roles
 * Input:
 * - orgId: string (from query string ?orgId=)
 * Response:
 * - ApiResponse<List<RoleDto>>
 * Permission:
 * - org.roles.view
 * Rules:
 * - Returns custom roles for the organization
 */
export async function getOrganizationRoles(orgId) {
  const response = await httpClient.get(`/organizations/${orgId}/roles`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get organization roles');
  }
  
  return response.data.data; // Direct array, not data.items
}

/**
 * Create role
 * 
 * Backend route: POST /api/organizations/{orgId}/roles
 * Frontend path: /organizations/{orgId}/roles
 * Input:
 * - orgId: string (from query string ?orgId=)
 * - payload: { roleName: string, description?: string, permissionKeys: string[] }
 * Response:
 * - ApiResponse<RoleDto>
 * Permission:
 * - org.roles.create
 * Rules:
 * - RoleName must be unique within organization
 * - permissionKeys must be valid permission keys (e.g., "org.overview.write")
 */
export async function createRole(orgId, payload) {
  const response = await httpClient.post(`/organizations/${orgId}/roles`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create role');
  }
  
  return response.data.data;
}

/**
 * Update role
 * 
 * Backend route: PUT /api/organizations/roles/{roleId}
 * Frontend path: /organizations/roles/{roleId}
 * Input:
 * - roleId: string
 * - payload: { roleName?: string, description?: string, permissionKeys?: string[] }
 * Response:
 * - ApiResponse<RoleDto>
 * Permission:
 * - org.roles.update
 * Rules:
 * - Cannot update default/system roles
 * - RoleName must be unique within organization
 */
export async function updateRole(roleId, payload) {
  const response = await httpClient.put(`/organizations/roles/${roleId}`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update role');
  }
  
  return response.data.data;
}

/**
 * Delete role
 * 
 * Backend route: DELETE /api/organizations/roles/{roleId}
 * Frontend path: /organizations/roles/{roleId}
 * Input:
 * - roleId: string
 * Response:
 * - ApiResponse<bool>
 * Permission:
 * - org.roles.delete
 * Rules:
 * - Cannot delete default/system roles
 * - May prevent deleting role if members are assigned
 */
export async function deleteRole(roleId) {
  const response = await httpClient.delete(`/organizations/roles/${roleId}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete role');
  }
  
  return response.data.data;
}

/**
 * Assign role to member (CANONICAL LOCATION)
 * 
 * Backend route: POST /api/organizations/{orgId}/members/{memberId}/role
 * Frontend path: /organizations/{orgId}/members/{memberId}/role
 * Input:
 * - orgId: string (from query string ?orgId=)
 * - memberId: string
 * - payload: { roleId: string }
 * Response:
 * - ApiResponse<MemberDto>
 * Permission:
 * - org.roles.assign
 * Rules:
 * - This is the CANONICAL location for role assignment
 * - Do NOT create assignRole in memberService
 * - RoleId is canonical, not fake frontend role GUID
 * - Member.RoleId is the source of truth
 */
export async function assignRoleToMember(orgId, memberId, payload) {
  const response = await httpClient.post(`/organizations/${orgId}/members/${memberId}/role`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to assign role to member');
  }
  
  return response.data.data;
}
