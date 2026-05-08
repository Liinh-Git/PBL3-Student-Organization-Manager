/**
 * userService.js - User profile and user-scoped data service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - getMyOrganizations belongs HERE, not in organizationService
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Get current user profile
 * 
 * Backend route: GET /api/users/me
 * Frontend path: /users/me
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<UserProfileDto>
 * Permission:
 * - JWT token required
 */
export async function getMe() {
  const response = await httpClient.get('/users/me');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get user profile');
  }
  
  return response.data.data;
}

/**
 * Update current user profile
 * 
 * Backend route: PUT /api/users/me
 * Frontend path: /users/me
 * Input:
 * - payload: { fullName?: string, phoneNumber?: string, dob?: string, gender?: string, address?: string, avatarUrl?: string, bio?: string, socialLinks?: string, profileVisibility?: string }
 * Response:
 * - ApiResponse<UserProfileDto>
 * Permission:
 * - JWT token required
 */
export async function updateMe(payload) {
  const response = await httpClient.put('/users/me', payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to update profile');
  }
  
  return response.data.data;
}

/**
 * Change password
 * 
 * Backend route: PUT /api/users/me/change-password
 * Frontend path: /users/me/change-password
 * Input:
 * - payload: { currentPassword: string, newPassword: string, confirmPassword?: string }
 * Response:
 * - ApiResponse<bool>
 * Permission:
 * - JWT token required
 * Rules:
 * - Validate currentPassword on backend
 * - Hash newPassword on backend
 */
export async function changePassword(payload) {
  const response = await httpClient.put('/users/me/change-password', payload);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to change password');
  }
  
  return response.data.data;
}

/**
 * Get my organizations (CANONICAL LOCATION)
 * 
 * Backend route: GET /api/users/me/organizations
 * Frontend path: /users/me/organizations
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<List<MyOrganizationDto>>
 * Permission:
 * - JWT token required
 * Rules:
 * - This is the CANONICAL location for getMyOrganizations
 * - Do NOT create getMyOrganizations in organizationService
 * - Returns organizations where user is a member
 */
export async function getMyOrganizations() {
  const response = await httpClient.get('/users/me/organizations');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get my organizations');
  }
  
  return response.data.data; // Direct array, not data.items
}

/**
 * Get my events
 * 
 * Backend route: GET /api/users/me/events
 * Frontend path: /users/me/events
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<List<MyEventDto>>
 * Permission:
 * - JWT token required
 * Rules:
 * - Returns events where user is creator, event member, or attendee
 */
export async function getMyEvents() {
  const response = await httpClient.get('/users/me/events');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get my events');
  }
  
  return response.data.data; // Direct array, not data.items
}

/**
 * Discover organizations (user-scoped discover)
 * 
 * Backend route: GET /api/users/me/discover/organizations
 * Frontend path: /users/me/discover/organizations
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<List<DiscoverOrganizationDto>>
 * Permission:
 * - JWT token required
 * Rules:
 * - Returns organizations user can discover/join
 * - Excludes organizations user is already a member of
 */
export async function discoverMyOrganizations() {
  const response = await httpClient.get('/users/me/discover/organizations');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to discover organizations');
  }
  
  return response.data.data; // Direct array, not data.items
}
