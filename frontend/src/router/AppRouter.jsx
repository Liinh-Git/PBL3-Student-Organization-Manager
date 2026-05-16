/**
 * AppRouter.jsx - Main application router
 * 
 * Phase 3C-4C: Page skeleton routes added
 * 
 * IMPORTANT RULES:
 * - orgId comes from useSearchParams(), NOT useParams()
 * - useParams() is ONLY for resource IDs in path (e.g., /events/:eventId)
 * - All pages are skeletons with TODO comments
 * - No real API calls in Phase 3C
 * - No fake data
 * 
 * EXCLUDED routes (no route, no page):
 * - /org/posts (hard-excluded from rescue v1)
 * - /org/comments (hard-excluded from rescue v1)
 * - Working messages/chat routes (placeholder only if needed)
 */

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import AppLayout from '../layouts/AppLayout';
import ProtectedRoute from './ProtectedRoute';
import OrgMemberRoute from './OrgMemberRoute';

// Public pages
import HomePage from '../pages/public/HomePage';
import PublicEventsPage from '../pages/public/PublicEventsPage';
import PublicEventDetailPage from '../pages/public/PublicEventDetailPage';

// Auth pages
import LoginPage from '../pages/auth/LoginPage';
import RegisterPage from '../pages/auth/RegisterPage';

// User workspace pages
import UserOrganizationsPage from '../pages/user/UserOrganizationsPage';
import UserEventsPage from '../pages/user/UserEventsPage';
import UserSettingsPage from '../pages/user/UserSettingsPage';
import UserFriendsPage from '../pages/user/UserFriendsPage';
import UserDiscoverPage from '../pages/user/UserDiscoverPage';

// Org workspace pages
import OrgOverviewPage from '../pages/org/OrgOverviewPage';
import OrgMembersPage from '../pages/org/OrgMembersPage';
import OrgDepartmentsPage from '../pages/org/OrgDepartmentsPage';
import OrgEventsPage from '../pages/org/OrgEventsPage';
import OrgEventDetailPage from '../pages/org/OrgEventDetailPage';
import OrgRequestsPage from '../pages/org/OrgRequestsPage';
import OrgRolesPage from '../pages/org/OrgRolesPage';

// Prototype-only placeholder pages
import OrgTasksPlaceholderPage from '../pages/org/OrgTasksPlaceholderPage';
import OrgResourcesPlaceholderPage from '../pages/org/OrgResourcesPlaceholderPage';
import OrgReportsPlaceholderPage from '../pages/org/OrgReportsPlaceholderPage';
import OrgFinancePlaceholderPage from '../pages/org/OrgFinancePlaceholderPage';

function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/" element={<HomePage />} />
        <Route path="/events" element={<PublicEventsPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        
        {/* Protected routes */}
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            {/* User workspace */}
            <Route path="/user/organizations" element={<UserOrganizationsPage />} />
            <Route path="/user/events" element={<UserEventsPage />} />
            <Route path="/user/settings" element={<UserSettingsPage />} />
            <Route path="/user/friends" element={<UserFriendsPage />} />
            <Route path="/user/discover" element={<UserDiscoverPage />} />
            <Route path="/events/:eventId" element={<PublicEventDetailPage />} />

            {/* Org workspace routes (requires membership) */}
            <Route element={<OrgMemberRoute />}>
              <Route path="/org/overview" element={<OrgOverviewPage />} />
              <Route path="/org/members" element={<OrgMembersPage />} />
              <Route path="/org/departments" element={<OrgDepartmentsPage />} />
              <Route path="/org/events" element={<OrgEventsPage />} />
              <Route path="/org/events/:eventId" element={<OrgEventDetailPage />} />
              <Route path="/org/requests" element={<OrgRequestsPage />} />
              <Route path="/org/roles" element={<OrgRolesPage />} />
              {/* Prototype-only placeholder routes */}
              <Route path="/org/tasks" element={<OrgTasksPlaceholderPage />} />
              <Route path="/org/resources" element={<OrgResourcesPlaceholderPage />} />
              <Route path="/org/reports" element={<OrgReportsPlaceholderPage />} />
              <Route path="/org/finance" element={<OrgFinancePlaceholderPage />} />
            </Route>
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default AppRouter;
