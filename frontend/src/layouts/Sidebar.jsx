/**
 * Sidebar.jsx - Navigation sidebar with rail for authenticated pages
 * 
 * Old repo layout: left icon rail (64px) + menu pane (220px)
 * 
 * IMPORTANT RULES:
 * - No Posts/Comments links (hard-excluded from rescue v1)
 * - Prototype-only nav items are hidden from demo navigation
 * - No fake org data, no fake counts
 * - Rail uses already loaded user organizations if available
 * - Do not call loadWorkspaceOrg repeatedly (org load loop fix)
 * 
 * Prototype-only pages are hidden from demo navigation. Task CRUD is available in EventDetail tree.
 */

import { Link, useLocation, useSearchParams } from 'react-router-dom';
import { useAuthContext } from '../contexts/AuthContext';

function Sidebar() {
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');
  const { user } = useAuthContext();

  const isActive = (path) => {
    return location.pathname === path || location.pathname.startsWith(path + '/');
  };

  // Get user initials for avatar
  const getInitials = (name) => {
    if (!name) return 'U';
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  };

  return (
    <>
      {/* Left Icon Rail (64px) */}
      <aside className="app-rail">
        <div className="app-rail-logo">SOM</div>
        
        <div className="app-rail-divider"></div>
        
        {/* Organization icons - placeholder for org avatars */}
        {/* In a full implementation, this would show user's organizations */}
        
        <div className="app-rail-divider"></div>
        
        {/* My Organizations shortcut */}
        <Link 
          to="/user/organizations" 
          className="app-rail-shortcut"
          title="My Organizations"
        >
          🏢
        </Link>
        
        {/* Profile shortcut */}
        <Link 
          to="/user/profile" 
          className="app-rail-shortcut"
          title="Profile"
        >
          👤
        </Link>
        
        {/* Settings shortcut */}
        <Link 
          to="/user/settings" 
          className="app-rail-shortcut"
          title="Settings"
        >
          ⚙️
        </Link>
      </aside>

      {/* Menu Pane (220px) */}
      <aside className="app-sidebar">
        <div className="sidebar-header">
          <h2>Workspace</h2>
        </div>

        <nav className="sidebar-nav">
          {/* User workspace links */}
          <div className="nav-section">
            <h3>User</h3>
            <ul>
              <li><Link to="/user/organizations" className={isActive('/user/organizations') ? 'active' : ''}>My Organizations</Link></li>
              <li><Link to="/user/events" className={isActive('/user/events') ? 'active' : ''}>My Events</Link></li>
              <li><Link to="/user/friends" className={isActive('/user/friends') ? 'active' : ''}>Friends</Link></li>
              <li><Link to="/user/discover" className={isActive('/user/discover') ? 'active' : ''}>Discover</Link></li>
              <li><Link to="/user/profile" className={isActive('/user/profile') ? 'active' : ''}>Profile</Link></li>
              <li><Link to="/user/settings" className={isActive('/user/settings') ? 'active' : ''}>Settings</Link></li>
            </ul>
          </div>

          {/* Org workspace links (conditional on orgId) */}
          {orgId && (
            <div className="nav-section">
              <h3>Organization</h3>
              <ul>
                <li><Link to={`/org/overview?orgId=${orgId}`} className={isActive('/org/overview') ? 'active' : ''}>Overview</Link></li>
                <li><Link to={`/org/members?orgId=${orgId}`} className={isActive('/org/members') ? 'active' : ''}>Members</Link></li>
                <li><Link to={`/org/departments?orgId=${orgId}`} className={isActive('/org/departments') ? 'active' : ''}>Departments</Link></li>
                <li><Link to={`/org/events?orgId=${orgId}`} className={isActive('/org/events') ? 'active' : ''}>Events</Link></li>
                <li><Link to={`/org/roles?orgId=${orgId}`} className={isActive('/org/roles') ? 'active' : ''}>Roles</Link></li>
                <li><Link to={`/org/requests?orgId=${orgId}`} className={isActive('/org/requests') ? 'active' : ''}>Requests</Link></li>
                <li><Link to={`/org/notifications?orgId=${orgId}`} className={isActive('/org/notifications') ? 'active' : ''}>Notifications</Link></li>
              </ul>
            </div>
          )}
        </nav>
      </aside>
    </>
  );
}

export default Sidebar;
