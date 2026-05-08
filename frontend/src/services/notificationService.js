/**
 * notificationService.js - Notification management service
 * 
 * Phase 3C-4B: Service skeleton only
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - REST API first, SignalR optional future
 * - No real API calls yet
 * - No mock data, no fake success
 */

// import httpClient from '../api/httpClient.js';

/**
 * Get notifications
 * 
 * TODO Phase implementation:
 * Backend route: GET /api/notifications
 * Frontend path later: /notifications
 * Input:
 * - params: { page?: number, pageSize?: number, isRead?: boolean, type?: string }
 * Response:
 * - ApiResponse<ListResponse<NotificationDto>>
 * Adapter:
 * - notificationAdapter.toNotificationListViewModel
 * Permission:
 * - JWT token required
 * Rules:
 * - Returns notifications for current user
 * - Can filter by isRead and type
 */
export async function getNotifications(params = {}) {
  throw new Error('TODO: implement getNotifications after API contract verification');
}

/**
 * Get unread notification count
 * 
 * TODO Phase implementation:
 * Backend route: GET /api/notifications/unread-count
 * Frontend path later: /notifications/unread-count
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<UnreadCountResponse>
 * - UnreadCountResponse: { count: number }
 * Permission:
 * - JWT token required
 * Rules:
 * - Returns count of unread notifications
 * - Used by NotificationBadge component
 */
export async function getUnreadCount() {
  throw new Error('TODO: implement getUnreadCount after API contract verification');
}

/**
 * Mark notification as read
 * 
 * TODO Phase implementation:
 * Backend route: POST /api/notifications/{id}/read
 * Frontend path later: /notifications/{id}/read
 * Input:
 * - id: string (notification ID)
 * Response:
 * - ApiResponse<void> or success message
 * Permission:
 * - JWT token required
 * Rules:
 * - Marks single notification as read
 * - Updates isRead = true, readAt = now
 */
export async function markNotificationRead(id) {
  throw new Error('TODO: implement markNotificationRead after API contract verification');
}

/**
 * Mark all notifications as read
 * 
 * TODO Phase implementation:
 * Backend route: POST /api/notifications/read-all
 * Frontend path later: /notifications/read-all
 * Input:
 * - None (uses Bearer token)
 * Response:
 * - ApiResponse<void> or success message
 * Permission:
 * - JWT token required
 * Rules:
 * - Marks all user's notifications as read
 * - Updates isRead = true, readAt = now for all unread notifications
 */
export async function markAllNotificationsRead() {
  throw new Error('TODO: implement markAllNotificationsRead after API contract verification');
}

/**
 * FUTURE: SignalR real-time notifications
 * 
 * TODO Phase implementation (optional future):
 * - SignalR hub connection for real-time notifications
 * - Subscribe to notification events
 * - Update notification badge in real-time
 * - Not required for base prototype
 */
