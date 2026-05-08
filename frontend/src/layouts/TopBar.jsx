/**
 * TopBar.jsx - Top navigation bar for authenticated pages
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Add user menu dropdown
 * - Add notification badge with unread count
 * - Add org switcher if in org workspace
 * - Add logout button
 * - No fake user data, no fake notification counts
 * 
 * IMPORTANT RULES:
 * - Static skeleton only in Phase 3C-4A
 * - No fake notification counts
 * - No fake user data
 * - Notification badge will use useNotifications hook later
 */

import { Link } from 'react-router-dom';
import { useAuthContext } from '../contexts/AuthContext';
// import { useNotifications } from '../hooks/useNotifications';

function TopBar() {
  const { user, logout } = useAuthContext();
  // const { unreadCount } = useNotifications();

  const handleLogout = () => {
    logout();
  };

  return (
    <header className="topbar">
      <div className="topbar-left">
        <h1>Student Organization Manager</h1>
      </div>

      <div className="topbar-right">
        {/* TODO Phase 3C-4B/4C: Add notification badge */}
        {/* <NotificationBadge count={unreadCount} /> */}

        <div className="user-menu">
          <span>{user?.fullName || 'User'}</span>
          <button 
            onClick={handleLogout}
            className="app-button app-button--ghost"
            style={{ padding: '0.42rem 0.7rem', fontSize: '0.8rem' }}
          >
            Logout
          </button>
        </div>
      </div>
    </header>
  );
}

export default TopBar;
