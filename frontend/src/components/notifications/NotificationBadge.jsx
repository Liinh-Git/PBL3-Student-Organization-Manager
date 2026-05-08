/**
 * NotificationBadge.jsx - Notification badge component
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component displays a notification badge with unread count.
 * 
 * Props:
 * - unreadCount: Number of unread notifications
 * - onClick: Callback when badge is clicked
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render notification icon with badge
 * - Display unread count
 * - Add click handler to navigate to notifications page
 * 
 * IMPORTANT:
 * - No fake unread count
 * - Props-driven only
 */

function NotificationBadge({ unreadCount = 0, onClick }) {
  return (
    <div className="notification-badge" onClick={onClick}>
      <span className="notification-icon">🔔</span>
      {unreadCount > 0 && (
        <span className="notification-count">{unreadCount}</span>
      )}
    </div>
  );
}

export default NotificationBadge;
