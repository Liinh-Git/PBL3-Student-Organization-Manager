/**
 * departmentService.js - Department management service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Get organization departments
 * 
 * Backend route: GET /api/organizations/{orgId}/departments
 * Frontend path: /organizations/{orgId}/departments
 * Input:
 * - orgId: string (from query string ?orgId=)
 * Response:
 * - ApiResponse<List<DepartmentDto>>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - orgId comes from useSearchParams(), NOT useParams()
 * - Returns departments with manager data
 */
export async function getOrganizationDepartments(orgId) {
  const response = await httpClient.get(`/organizations/${orgId}/departments`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get organization departments');
  }
  
  return response.data.data; // Direct array, not data.items
}

/**
 * Create department
 * 
 * Backend route: POST /api/organizations/{orgId}/departments
 * Frontend path: /organizations/{orgId}/departments
 * Input:
 * - orgId: string (from query string ?orgId=)
 * - payload: { departmentName: string, description?: string, managerId?: string }
 * Response:
 * - ApiResponse<DepartmentDto>
 * Permission:
 * - org.departments.manage
 * Rules:
 * - ManagerId must point to a valid Member if provided
 */
export async function createDepartment(orgId, payload) {
  const response = await httpClient.post(`/organizations/${orgId}/departments`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create department');
  }
  
  return response.data.data;
}

/**
 * Get department by ID
 * 
 * Backend route: GET /api/departments/{id}
 * Frontend path: /departments/{id}
 * Input:
 * - id: string (department ID)
 * Response:
 * - ApiResponse<DepartmentDto>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - Returns department with manager and member count
 */
export async function getDepartmentById(id) {
  const response = await httpClient.get(`/departments/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get department');
  }
  
  return response.data.data;
}

/**
 * Update department
 * 
 * Backend route: PUT /api/departments/{id}
 * Frontend path: /departments/{id}
 * Input:
 * - id: string (department ID)
 * - payload: { departmentName?: string, description?: string, managerId?: string }
 * Response:
 * - ApiResponse<DepartmentDto>
 * Permission:
 * - org.departments.manage
 * Rules:
 * - ManagerId must point to a valid Member if provided
 */
export async function updateDepartment(id, payload) {
  const response = await httpClient.put(`/departments/${id}`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update department');
  }
  
  return response.data.data;
}

/**
 * Delete department
 * 
 * Backend route: DELETE /api/departments/{id}
 * Frontend path: /departments/{id}
 * Input:
 * - id: string (department ID)
 * Response:
 * - ApiResponse<bool>
 * Permission:
 * - org.departments.manage
 * Rules:
 * - Soft-delete department record (sets status to Archived)
 * - May prevent deleting department if members are assigned
 */
export async function deleteDepartment(id) {
  const response = await httpClient.delete(`/departments/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete department');
  }
  
  return response.data.data;
}
