/**
 * Sidebar.jsx - Navigation sidebar with rail for authenticated pages
 */

import { useEffect, useMemo, useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthContext } from '../contexts/AuthContext';
import { getMyOrganizations } from '../services/userService.js';

function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const routeOrgId = searchParams.get('orgId');
  const { user } = useAuthContext();

  const [organizations, setOrganizations] = useState([]);
  const [selectedWorkspace, setSelectedWorkspace] = useState('user');
  const [selectedOrgId, setSelectedOrgId] = useState(null);

  const isActive = (path) => location.pathname === path || location.pathname.startsWith(path + '/');

  const getInitials = (name) => {
    if (!name) return 'U';
    return name.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2);
  };

  const getOrgInitials = (org) => {
    const orgName = org?.name || org?.orgName || org?.organizationName || '';
    if (!orgName) return 'O';
    return orgName.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2);
  };

  useEffect(() => {
    async function loadOrganizations() {
      try {
        const data = await getMyOrganizations();
        setOrganizations(Array.isArray(data) ? data : []);
      } catch {
        setOrganizations([]);
      }
    }
    loadOrganizations();
  }, []);

  useEffect(() => {
    if (location.pathname.startsWith('/org') && routeOrgId) {
      setSelectedWorkspace('org');
      setSelectedOrgId(routeOrgId);
      return;
    }

    if (location.pathname.startsWith('/user')) {
      setSelectedWorkspace('user');
    }
  }, [location.pathname, routeOrgId]);

  const activeOrgId = useMemo(() => {
    if (selectedWorkspace !== 'org') return null;
    return selectedOrgId || routeOrgId || organizations[0]?.id || null;
  }, [selectedWorkspace, selectedOrgId, routeOrgId, organizations]);

  const handleSelectUserWorkspace = () => {
    setSelectedWorkspace('user');
    navigate('/user/organizations');
  };

  const handleSelectOrgWorkspace = (orgId) => {
    setSelectedWorkspace('org');
    setSelectedOrgId(orgId);
    navigate(`/org/overview?orgId=${orgId}`);
  };

  return (
    <>
      <aside className="app-rail">

        <div className="app-rail-divider"></div>

        <button
          type="button"
          onClick={handleSelectUserWorkspace}
          className={`app-rail-icon ${selectedWorkspace === 'user' ? 'app-rail-icon--active' : ''}`}
          title="User Workspace"
        >
          {getInitials(user?.fullName)}
        </button>

        <div className="app-rail-divider"></div>

        <div style={{ display: 'grid', gap: '8px', width: '100%', justifyItems: 'center', overflowY: 'auto' }}>
          {organizations.map((org) => (
            <button
              key={org.id}
              type="button"
              onClick={() => handleSelectOrgWorkspace(org.id)}
              className={`app-rail-icon ${selectedWorkspace === 'org' && activeOrgId === org.id ? 'app-rail-icon--active' : ''}`}
              title={org.orgName || org.name || 'Organization'}
            >
              {getOrgInitials(org)}
            </button>
          ))}
        </div>
      </aside>

      <aside className="app-sidebar">
        <div className="sidebar-header">
          <h2>{selectedWorkspace === 'org' ? 'Organization' : 'User'}</h2>
        </div>

        <nav className="sidebar-nav">
          {selectedWorkspace === 'user' && (
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
          )}

          {selectedWorkspace === 'org' && activeOrgId && (
            <div className="nav-section">
              <h3>Organization</h3>
              <ul>
                <li><Link to={`/org/overview?orgId=${activeOrgId}`} className={isActive('/org/overview') ? 'active' : ''}>Overview</Link></li>
                <li><Link to={`/org/members?orgId=${activeOrgId}`} className={isActive('/org/members') ? 'active' : ''}>Members</Link></li>
                <li><Link to={`/org/departments?orgId=${activeOrgId}`} className={isActive('/org/departments') ? 'active' : ''}>Departments</Link></li>
                <li><Link to={`/org/events?orgId=${activeOrgId}`} className={isActive('/org/events') ? 'active' : ''}>Events</Link></li>
                <li><Link to={`/org/roles?orgId=${activeOrgId}`} className={isActive('/org/roles') ? 'active' : ''}>Roles</Link></li>
                <li><Link to={`/org/requests?orgId=${activeOrgId}`} className={isActive('/org/requests') ? 'active' : ''}>Requests</Link></li>
                <li><Link to={`/org/notifications?orgId=${activeOrgId}`} className={isActive('/org/notifications') ? 'active' : ''}>Notifications</Link></li>
              </ul>
            </div>
          )}
        </nav>
      </aside>
    </>
  );
}

export default Sidebar;
