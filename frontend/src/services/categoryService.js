/**
 * categoryService.js - EventCategory management service (inside EventDetail)
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - EventCategories are part of EventDetail tree
 * - CategoryDto may include tasks[] array
 * - Do NOT invent list-by-category task endpoint
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Get milestone categories
 * 
 * Backend route: GET /api/milestones/{milestoneId}/categories
 * Frontend path: /milestones/{milestoneId}/categories
 * Input:
 * - milestoneId: string
 * Response:
 * - ApiResponse<EventCategoryDto[]>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - Returns categories ordered by OrderIndex
 * - CategoryDto may include tasks[] array
 * - If tasks[] is absent, frontend page/hook later initializes tasks: []
 * - Do NOT invent a separate list-by-category task endpoint
 */
export async function getMilestoneCategories(milestoneId) {
  const response = await httpClient.get(`/milestones/${milestoneId}/categories`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get milestone categories');
  }
  
  return response.data.data; // Direct array
}

/**
 * Create category
 * 
 * Backend route: POST /api/milestones/{milestoneId}/categories
 * Frontend path: /milestones/{milestoneId}/categories
 * Input:
 * - milestoneId: string
 * - payload: { categoryName: string, description?: string, orderIndex: number, ownerDepartmentId?: string }
 * Response:
 * - ApiResponse<EventCategoryDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - OrderIndex should be maintained for board rendering
 * - ownerDepartmentId is optional
 */
export async function createCategory(milestoneId, payload) {
  const response = await httpClient.post(`/milestones/${milestoneId}/categories`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create category');
  }
  
  return response.data.data;
}

/**
 * Get category by ID
 * 
 * Backend route: GET /api/categories/{id}
 * Frontend path: /categories/{id}
 * Input:
 * - id: string (category ID)
 * Response:
 * - ApiResponse<EventCategoryDto>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - CategoryDto may include tasks[] array
 * - If tasks[] is absent, frontend page/hook later initializes tasks: []
 */
export async function getCategoryById(id) {
  const response = await httpClient.get(`/categories/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get category');
  }
  
  return response.data.data;
}

/**
 * Update category
 * 
 * Backend route: PUT /api/categories/{id}
 * Frontend path: /categories/{id}
 * Input:
 * - id: string (category ID)
 * - payload: { categoryName?: string, description?: string, orderIndex?: number, ownerDepartmentId?: string }
 * Response:
 * - ApiResponse<EventCategoryDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - OrderIndex changes may affect other categories
 */
export async function updateCategory(id, payload) {
  const response = await httpClient.put(`/categories/${id}`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update category');
  }
  
  return response.data.data;
}

/**
 * Delete category
 * 
 * Backend route: DELETE /api/categories/{id}
 * Frontend path: /categories/{id}
 * Input:
 * - id: string (category ID)
 * Response:
 * - ApiResponse<void>
 * Permission:
 * - org.events.manage
 * Rules:
 * - Soft-delete category record
 * - Cascade soft-delete to tasks
 */
export async function deleteCategory(id) {
  const response = await httpClient.delete(`/categories/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete category');
  }
  
  return response.data.data;
}
