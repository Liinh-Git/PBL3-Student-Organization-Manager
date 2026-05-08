/**
 * AppLayout.jsx - Main layout for authenticated pages
 * 
 * Old repo layout: app-shell with app-rail (64px) + app-sidebar (220px) + app-main
 * 
 * Usage:
 *   <Route element={<AppLayout />}>
 *     <Route path="/user/profile" element={<UserProfilePage />} />
 *     <Route path="/org/members" element={<OrgMembersPage />} />
 *   </Route>
 */

import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import TopBar from './TopBar';

function AppLayout() {
  return (
    <div className="app-shell">
      <Sidebar />

      <div className="app-layout-main">
        <TopBar />

        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default AppLayout;

