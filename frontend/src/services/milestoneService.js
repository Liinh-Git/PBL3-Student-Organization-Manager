/**
 * milestoneService.js - Milestone management service (inside EventDetail)
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Milestones are part of EventDetail tree
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Get event milestones
 * 
 * Backend route: GET /api/events/{eventId}/milestones
 * Frontend path: /events/{eventId}/milestones
 * Input:
 * - eventId: string (from useParams())
 * Response:
 * - ApiResponse<MilestoneDto[]>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - Returns milestones ordered by OrderIndex
 * - Used by EventDetail page to load milestone tree
 */
export async function getEventMilestones(eventId) {
  const response = await httpClient.get(`/events/${eventId}/milestones`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get event milestones');
  }
  
  return response.data.data; // Direct array
}

/**
 * Create milestone
 * 
 * Backend route: POST /api/events/{eventId}/milestones
 * Frontend path: /events/{eventId}/milestones
 * Input:
 * - eventId: string (from useParams())
 * - payload: { title: string, description?: string, orderIndex: number, startDate?: string, endDate?: string }
 * Response:
 * - ApiResponse<MilestoneDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - OrderIndex should be maintained for timeline rendering
 * - startDate/endDate are optional
 */
export async function createMilestone(eventId, payload) {
  const response = await httpClient.post(`/events/${eventId}/milestones`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create milestone');
  }
  
  return response.data.data;
}

/**
 * Get milestone by ID
 * 
 * Backend route: GET /api/milestones/{id}
 * Frontend path: /milestones/{id}
 * Input:
 * - id: string (milestone ID)
 * Response:
 * - ApiResponse<MilestoneDto>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - Returns milestone with categories if included
 */
export async function getMilestoneById(id) {
  const response = await httpClient.get(`/milestones/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get milestone');
  }
  
  return response.data.data;
}

/**
 * Update milestone
 * 
 * Backend route: PUT /api/milestones/{id}
 * Frontend path: /milestones/{id}
 * Input:
 * - id: string (milestone ID)
 * - payload: { title?: string, description?: string, orderIndex?: number, startDate?: string, endDate?: string, status?: string }
 * Response:
 * - ApiResponse<MilestoneDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - OrderIndex changes may affect other milestones
 */
export async function updateMilestone(id, payload) {
  const response = await httpClient.put(`/milestones/${id}`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update milestone');
  }
  
  return response.data.data;
}

/**
 * Delete milestone
 * 
 * Backend route: DELETE /api/milestones/{id}
 * Frontend path: /milestones/{id}
 * Input:
 * - id: string (milestone ID)
 * Response:
 * - ApiResponse<void>
 * Permission:
 * - org.events.manage
 * Rules:
 * - Soft-delete milestone record
 * - Cascade soft-delete to categories and tasks
 */
export async function deleteMilestone(id) {
  const response = await httpClient.delete(`/milestones/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete milestone');
  }
  
  return response.data.data;
}
