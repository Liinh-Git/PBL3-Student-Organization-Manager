/**
 * friendService.js - Friend request management service
 * 
 * Phase 3C-4B: Service skeleton only
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - No real API calls yet
 * - No mock data, no fake success
 */

// import httpClient from '../api/httpClient.js';

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
  throw new Error('TODO: implement getFriends after API contract verification');
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
  throw new Error('TODO: implement getFriendRequests after API contract verification');
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
  throw new Error('TODO: implement sendFriendRequest after API contract verification');
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
  throw new Error('TODO: implement acceptFriendRequest after API contract verification');
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
  throw new Error('TODO: implement rejectFriendRequest after API contract verification');
}
