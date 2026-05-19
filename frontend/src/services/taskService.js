/**
 * taskService.js - Task management service (inside EventDetail)
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Task is CORE inside EventDetail tree
 * - Only /org/tasks aggregate board is PROTOTYPE_ONLY
 * - Do NOT create getOrgTasks or aggregate board service
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Create task
 * 
 * Backend route: POST /api/categories/{categoryId}/tasks
 * Frontend path: /categories/{categoryId}/tasks
 * Input:
 * - categoryId: string
 * - payload: { taskName: string, description?: string, assigneeId?: string, deptId?: string, priority: string, deadline?: string }
 * Response:
 * - ApiResponse<TaskDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - On success, append TaskDto to local category.tasks[] in EventDetail tree state
 * - Single assignee only (assigneeId is Member ID)
 * - priority is required (Low, Medium, High, Urgent)
 */
export async function createTask(categoryId, payload) {
  const response = await httpClient.post(`/categories/${categoryId}/tasks`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create task');
  }
  
  return response.data.data;
}

/**
 * Get task by ID
 * 
 * Backend route: GET /api/tasks/{taskId}
 * Frontend path: /tasks/{taskId}
 * Input:
 * - taskId: string
 * Response:
 * - ApiResponse<TaskDto>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - Returns task with assignee and department data
 */
export async function getTaskById(taskId) {
  const response = await httpClient.get(`/tasks/${taskId}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get task');
  }
  
  return response.data.data;
}

/**
 * Update task
 * 
 * Backend route: PUT /api/tasks/{taskId}
 * Frontend path: /tasks/{taskId}
 * Input:
 * - taskId: string
 * - payload: { taskName?: string, description?: string, priority?: string, deadline?: string, note?: string }
 * Response:
 * - ApiResponse<TaskDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - On success, mutate task in local category.tasks[] in EventDetail tree state
 */
export async function updateTask(taskId, payload) {
  const response = await httpClient.put(`/tasks/${taskId}`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update task');
  }
  
  return response.data.data;
}

/**
 * Delete task
 * 
 * Backend route: DELETE /api/tasks/{taskId}
 * Frontend path: /tasks/{taskId}
 * Input:
 * - taskId: string
 * Response:
 * - ApiResponse<void>
 * Permission:
 * - org.events.manage
 * Rules:
 * - Soft-delete task record
 * - On success, remove task from local category.tasks[] in EventDetail tree state
 */
export async function deleteTask(taskId) {
  const response = await httpClient.delete(`/tasks/${taskId}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete task');
  }
  
  return response.data.data;
}

/**
 * Update task status
 * 
 * Backend route: PUT /api/tasks/{taskId}/status
 * Frontend path: /tasks/{taskId}/status
 * Input:
 * - taskId: string
 * - payload: { status: string }
 * Response:
 * - ApiResponse<TaskDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - status values: Todo, InProgress, Done
 * - On success, mutate task in local category.tasks[] in EventDetail tree state
 */
export async function updateTaskStatus(taskId, payload) {
  const response = await httpClient.put(`/tasks/${taskId}/status`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update task status');
  }
  
  return response.data.data;
}

/**
 * Assign task
 * 
 * Backend route: PUT /api/tasks/{taskId}/assign
 * Frontend path: /tasks/{taskId}/assign
 * Input:
 * - taskId: string
 * - payload: { assigneeId: string | null, deptId?: string | null }
 * Response:
 * - ApiResponse<TaskDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - assigneeId is Member ID (single assignee only)
 * - assigneeId can be null to unassign
 * - On success, mutate task in local category.tasks[] in EventDetail tree state
 */
export async function assignTask(taskId, payload) {
  const response = await httpClient.put(`/tasks/${taskId}/assign`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to assign task');
  }
  
  return response.data.data;
}

export async function getDepartmentTasks(orgId, departmentId) {
  const response = await httpClient.get(`/organizations/${orgId}/departments/${departmentId}/tasks`);
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get department tasks');
  }
  return response.data.data;
}

export async function createDepartmentTask(orgId, departmentId, payload) {
  const response = await httpClient.post(`/organizations/${orgId}/departments/${departmentId}/tasks`, payload);
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create department task');
  }
  return response.data.data;
}

/**
 * IMPORTANT NOTE:
 * 
 * Do NOT create getOrgTasks() or any aggregate board service.
 * /org/tasks aggregate board is PROTOTYPE_ONLY and has no API endpoint.
 * Task is CORE inside EventDetail tree only.
 * 
 * Task list is obtained from:
 * 1. GET /milestones/{milestoneId}/categories returns CategoryDto[]
 * 2. CategoryDto may include tasks[] array
 * 3. If tasks[] is absent, frontend page/hook initializes tasks: []
 * 4. Task CRUD operations mutate the local tree state
 */
