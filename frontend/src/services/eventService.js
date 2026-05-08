/**
 * eventService.js - Event management service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Do not fake TargetParticipants/Budget/AverageRating if missing
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Get organization events
 * 
 * Backend route: GET /api/organizations/{orgId}/events
 * Frontend path: /organizations/{orgId}/events
 * Input:
 * - orgId: string (from query string ?orgId=)
 * Response:
 * - ApiResponse<List<EventSummaryDto>>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - VITE_API_BASE_URL already includes /api, so do NOT include /api in service path
 * - Do not use mock fallback
 */
export async function getOrganizationEvents(orgId) {
  const response = await httpClient.get(`/organizations/${orgId}/events`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get organization events');
  }
  
  return response.data.data; // Direct array, not data.items
}

/**
 * Create event
 * 
 * Backend route: POST /api/organizations/{orgId}/events
 * Frontend path: /organizations/{orgId}/events
 * Input:
 * - orgId: string (from query string ?orgId=)
 * - payload: { eventName: string, description?: string, startDate: string, endDate?: string, location?: string, bannerUrl?: string, visibility?: string }
 * Response:
 * - ApiResponse<EventDto>
 * Permission:
 * - org.events.create
 * Rules:
 * - startDate must be before endDate if both provided
 * - visibility defaults to "Private" if not provided (Public, OrganizationOnly, Private)
 */
export async function createEvent(orgId, payload) {
  const response = await httpClient.post(`/organizations/${orgId}/events`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to create event');
  }
  
  return response.data.data;
}

/**
 * Get event by ID (workspace context)
 * 
 * Backend route: GET /api/events/{id}
 * Frontend path: /events/{id}
 * Input:
 * - id: string (event ID from useParams())
 * Response:
 * - ApiResponse<EventDto>
 * Permission:
 * - org.workspace.access
 * Rules:
 * - This is workspace context, requires membership
 * - For public event view, use getPublicEventById instead
 * - EventDto includes: id, organizationId, name, description, startDate, endDate, status, visibility, location, targetParticipants, budget, averageRating, tags
 */
export async function getEventById(id) {
  const response = await httpClient.get(`/events/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get event');
  }
  
  return response.data.data;
}

/**
 * Update event
 * 
 * Backend route: PUT /api/events/{id}
 * Frontend path: /events/{id}
 * Input:
 * - id: string (event ID)
 * - payload: { eventName?: string, description?: string, startDate?: string, endDate?: string, location?: string, bannerUrl?: string, visibility?: string }
 * Response:
 * - ApiResponse<EventDto>
 * Permission:
 * - org.events.manage
 * Rules:
 * - startDate must be before endDate if both provided
 * - visibility must be Public, OrganizationOnly, or Private
 */
export async function updateEvent(id, payload) {
  const response = await httpClient.put(`/events/${id}`, payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update event');
  }
  
  return response.data.data;
}

/**
 * Delete event
 * 
 * Backend route: DELETE /api/events/{id}
 * Frontend path: /events/{id}
 * Input:
 * - id: string (event ID)
 * Response:
 * - ApiResponse<bool>
 * Permission:
 * - org.events.manage
 * Rules:
 * - Soft-delete event record (sets status to Cancelled)
 * - Cascade soft-delete to milestones/categories/tasks
 */
export async function deleteEvent(id) {
  const response = await httpClient.delete(`/events/${id}`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete event');
  }
  
  return response.data.data;
}

/**
 * Get public events
 * 
 * Backend route: GET /api/events/public
 * Frontend path: /events/public
 * Input:
 * - None
 * Response:
 * - ApiResponse<List<EventPublicDto>>
 * Permission:
 * - Public or authenticated-public
 * Rules:
 * - Returns only events with visibility = Public
 * - Limited information compared to workspace context
 */
export async function getPublicEvents() {
  const response = await httpClient.get('/events/public');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get public events');
  }
  
  return response.data.data; // Direct array, not data.items
}

/**
 * Get public event by ID
 * 
 * Backend route: GET /api/events/{id}/public
 * Frontend path: /events/{id}/public
 * Input:
 * - id: string (event ID)
 * Response:
 * - ApiResponse<EventPublicDto>
 * Permission:
 * - Public or authenticated-public
 * Rules:
 * - Returns only if event visibility = Public
 * - Limited information compared to workspace context
 */
export async function getPublicEventById(id) {
  const response = await httpClient.get(`/events/${id}/public`);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get public event');
  }
  
  return response.data.data;
}
