/**
 * notificationAdapter.js - Notification DTO to ViewModel adapters
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
 * Convert NotificationDto to NotificationViewModel
 * 
 * TODO Phase implementation:
 * Input: NotificationDto from NotificationContracts.cs.TODO
 * Expected fields:
 * - id, receiverId, actorId?, title, message, type, relatedEntityType?, relatedEntityId?, actionUrl?, isRead, readAt?, createdAt
 * Output ViewModel:
 * - Used by notification list and badge components
 * Rules:
 * - type values: System, RequestSubmitted, RequestReviewed, FriendRequest, EventCreated, EventUpdated, EventReminder, TaskAssigned, TaskDue, ResourceChanged
 * - actorId is optional (system notifications may not have actor)
 * - Do not fake actor if missing
 */
export function toNotificationViewModel(dto) {
  if (!dto) return null;
  throw new Error('TODO: implement toNotificationViewModel after NotificationDto is verified');
}

/**
 * Convert NotificationDto[] to NotificationListViewModel
 * 
 * TODO Phase implementation:
 * Input: NotificationDto[] from NotificationContracts.cs.TODO
 * Output ViewModel:
 * - Array of NotificationViewModel
 * Rules:
 * - Map each item using toNotificationViewModel
 * - Filter out null items
 */
export function toNotificationListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toNotificationViewModel).filter(Boolean);
}
