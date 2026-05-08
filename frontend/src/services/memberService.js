/**
 * memberService.js - Organization member management service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Role assignment belongs to roleService, NOT here
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';
import { toMemberListViewModel, toMemberViewModel } from '../adapters/memberAdapter.js';

/**
 * Get organization members
 * 
 * Backend route: GET /api/organizations/{orgId}/members
 * Frontend path: /organizations/{orgId}/members
 * Input:
 * - orgId: string (from query string ?orgId=)
 * Response:
 * - ApiResponse<List<MemberDto>>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - orgId comes from useSearchParams(), NOT useParams()
 * - Returns members with user, department, and role data
 */
export async function getOrganizationMembers(orgId) {
  const response = await httpClient.get(`/organizations/${orgId}/members`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get organization members');
  }
  
  return toMemberListViewModel(response.data.data); // Direct array, not data.items
}

/**
 * Add member to organization
 * 
 * Backend route: POST /api/organizations/{orgId}/members
 * Frontend path: /organizations/{orgId}/members
 * Input:
 * - orgId: string (from query string ?orgId=)
 * - payload: { userId: string, roleId?: string, departmentId?: string, studentCode?: string }
 * Response:
 * - ApiResponse<MemberDto>
 * Permission:
 * - org.members.manage
 * Rules:
 * - User must not already be a member
 * - Default role may be assigned if roleId not provided
 */
export async function addMember(orgId, payload) {
  const response = await httpClient.post(`/organizations/${orgId}/members`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to add member');
  }
  
  return toMemberViewModel(response.data.data);
}

/**
 * Update member department
 * 
 * Backend route: PUT /api/members/{id}/department
 * Frontend path: /members/{id}/department
 * Input:
 * - memberId: string
 * - payload: { departmentId: string }
 * Response:
 * - ApiResponse<MemberDto>
 * Permission:
 * - org.members.manage
 * Rules:
 * - departmentId can be null to remove department assignment
 */
export async function updateMemberDepartment(memberId, payload) {
  const response = await httpClient.put(`/members/${memberId}/department`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update member department');
  }
  
  return toMemberViewModel(response.data.data);
}

/**
 * Remove member from organization
 * 
 * Backend route: DELETE /api/members/{id}
 * Frontend path: /members/{id}
 * Input:
 * - memberId: string
 * Response:
 * - ApiResponse<bool>
 * Permission:
 * - org.members.manage
 * Rules:
 * - Soft-delete member record
 * - May prevent removing last President/admin
 */
export async function removeMember(memberId) {
  const response = await httpClient.delete(`/members/${memberId}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to remove member');
  }
  
  return response.data.data;
}
