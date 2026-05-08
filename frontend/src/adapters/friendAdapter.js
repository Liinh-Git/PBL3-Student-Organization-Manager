/**
 * friendAdapter.js - Friend and FriendRequest DTO to ViewModel adapters
 * 
 * Phase 3C-4B: Adapter skeleton only
 * 
 * IMPORTANT RULES:
 * - Do not invent fake values
 * - Do not use mock field names
 * - Return null/empty safe shape when input is missing
 * - Optional fields should render as "Chưa cập nhật" or be hidden in UI later
 */

/**
 * Convert FriendDto to FriendViewModel
 * 
 * TODO Phase implementation:
 * Input: FriendDto from FriendContracts.cs.TODO
 * Expected fields:
 * - id, userId, friendId, friend (nested UserDto), status, createdAt
 * Output ViewModel:
 * - Used by UserFriendsPage
 * Rules:
 * - friend is nested UserDto with fullName, email, avatarUrl
 * - status values: Pending, Accepted, Rejected, Cancelled, Blocked
 * - Do not fake friend data if missing
 */
export function toFriendViewModel(dto) {
  if (!dto) return null;
  throw new Error('TODO: implement toFriendViewModel after FriendDto is verified');
}

/**
 * Convert FriendRequestDto to FriendRequestViewModel
 * 
 * TODO Phase implementation:
 * Input: FriendRequestDto from FriendContracts.cs.TODO
 * Expected fields:
 * - id, senderId, receiverId, sender (nested UserDto?), receiver (nested UserDto?), status, respondedAt?
 * Output ViewModel:
 * - Used by UserFriendsPage for friend requests
 * Rules:
 * - sender/receiver are nested UserDto with fullName, email, avatarUrl
 * - status values: Pending, Accepted, Rejected, Cancelled, Blocked
 * - respondedAt is set when status is Accepted/Rejected
 * - Do not fake sender/receiver if missing
 */
export function toFriendRequestViewModel(dto) {
  if (!dto) return null;
  throw new Error('TODO: implement toFriendRequestViewModel after FriendRequestDto is verified');
}

/**
 * Convert FriendRequestDto[] to FriendRequestListViewModel
 * 
 * TODO Phase implementation:
 * Input: FriendRequestDto[] from FriendContracts.cs.TODO
 * Output ViewModel:
 * - Array of FriendRequestViewModel
 * Rules:
 * - Map each item using toFriendRequestViewModel
 * - Filter out null items
 */
export function toFriendRequestListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toFriendRequestViewModel).filter(Boolean);
}
