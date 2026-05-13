/**
 * friendService.js - Friend request management service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';
import {
  toFriendViewModel,
  toFriendRequestListViewModel,
  toFriendRequestViewModel
} from '../adapters/friendAdapter.js';

function getApiErrorMessage(responseData, fallbackMessage) {
  if (!responseData) return fallbackMessage;
  if (responseData.message) return responseData.message;
  if (Array.isArray(responseData.errors) && responseData.errors.length > 0) {
    return responseData.errors.join(', ');
  }
  return fallbackMessage;
}

/**
 * Get friends
 * 
 * TODO Phase implementation:
 * Backend route: GET /api/friends
 * Frontend path later: /friends
 * Input:
 * - params: { page?: number, pageSize?: number, search?: string }
 * Response:
 * - ApiResponse<ListResponse<FriendDto>>
 * Adapter:
 * - friendAdapter.toFriendViewModel
 * Permission:
 * - JWT token required
 * Rules:
 * - Returns accepted friend relationships for current user
 */
export async function getFriends(params = {}) {
  const response = await httpClient.get('/friends', { params });
  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to get friends'));
  }
  const items = Array.isArray(response.data.data) ? response.data.data : [];
  return items.map(toFriendViewModel).filter(Boolean);
}

/**
 * Get friend requests
 * 
 * TODO Phase implementation:
 * Backend route: GET /api/friends/requests
 * Frontend path later: /friends/requests
 * Input:
 * - params: { page?: number, pageSize?: number, status?: string }
 * Response:
 * - ApiResponse<ListResponse<FriendRequestDto>>
 * Adapter:
 * - friendAdapter.toFriendRequestListViewModel
 * Permission:
 * - JWT token required
 * Rules:
 * - Returns friend requests (sent and received) for current user
 * - Can filter by status (Pending, Accepted, Rejected, Cancelled, Blocked)
 */
export async function getFriendRequests(params = {}) {
  const response = await httpClient.get('/friends/requests', { params });
  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to get friend requests'));
  }
  return toFriendRequestListViewModel(response.data.data);
}

export async function getFriendSuggestions(params = {}) {
  const response = await httpClient.get('/friends/suggestions', { params });
  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to get friend suggestions'));
  }
  const items = Array.isArray(response.data.data) ? response.data.data : [];
  return items.map(toFriendViewModel).filter(Boolean);
}

/**
 * Send friend request
 * 
 * TODO Phase implementation:
 * Backend route: POST /api/friends/requests
 * Frontend path later: /friends/requests
 * Input:
 * - payload: { receiverId: string }
 * Response:
 * - ApiResponse<FriendRequestDto>
 * Adapter:
 * - friendAdapter.toFriendRequestViewModel
 * Permission:
 * - JWT token required
 * Rules:
 * - SenderId is current user (from JWT)
 * - SenderId != ReceiverId (enforced at service level)
 * - Cannot send duplicate request
 */
export async function sendFriendRequest(payload) {
  try {
    const response = await httpClient.post('/friends/requests', payload);
    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to send friend request'));
    }
    return toFriendRequestViewModel(response.data.data);
  } catch (error) {
    throw new Error(getApiErrorMessage(error?.response?.data, error?.message || 'Failed to send friend request'));
  }
}

/**
 * Accept friend request
 * 
 * TODO Phase implementation:
 * Backend route: POST /api/friends/requests/{id}/accept
 * Frontend path later: /friends/requests/{id}/accept
 * Input:
 * - id: string (friend request ID)
 * Response:
 * - ApiResponse<FriendRequestDto>
 * Adapter:
 * - friendAdapter.toFriendRequestViewModel
 * Permission:
 * - JWT token required (must be receiver)
 * Rules:
 * - Updates status to Accepted
 * - Sets respondedAt timestamp
 */
export async function acceptFriendRequest(id) {
  try {
    const response = await httpClient.post(`/friends/requests/${id}/accept`, {});
    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to accept friend request'));
    }
    return toFriendViewModel(response.data.data);
  } catch (error) {
    throw new Error(getApiErrorMessage(error?.response?.data, error?.message || 'Failed to accept friend request'));
  }
}

/**
 * Reject friend request
 * 
 * TODO Phase implementation:
 * Backend route: POST /api/friends/requests/{id}/reject
 * Frontend path later: /friends/requests/{id}/reject
 * Input:
 * - id: string (friend request ID)
 * Response:
 * - ApiResponse<FriendRequestDto>
 * Adapter:
 * - friendAdapter.toFriendRequestViewModel
 * Permission:
 * - JWT token required (must be receiver)
 * Rules:
 * - Updates status to Rejected
 * - Sets respondedAt timestamp
 */
export async function rejectFriendRequest(id) {
  try {
    const response = await httpClient.post(`/friends/requests/${id}/reject`, {});
    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to reject friend request'));
    }
    return !!response.data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error?.response?.data, error?.message || 'Failed to reject friend request'));
  }
}
