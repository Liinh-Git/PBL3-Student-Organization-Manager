/**
 * OrgMemberRoute.jsx - Route guard for organization workspace routes
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Get orgId from useSearchParams() (NOT useParams())
 * - Check OrgContext for isMember
 * - If not member, render <ForbiddenState />
 * - If member, render <Outlet /> for nested routes
 * - Show loading spinner while checking membership
 * 
 * IMPORTANT RULES:
 * - orgId comes from query string (?orgId=), NOT path params
 * - NEVER use useParams() for orgId
 * - Permission fallback must NEVER grant org.workspace.access
 * - If permission check fails, render <ForbiddenState />
 * 
 * Usage:
 *   <Route element={<OrgMemberRoute />}>
 *     <Route path="/org/members" element={<OrgMembersPage />} />
 *   </Route>
 */

import { Outlet, useSearchParams, useNavigate } from 'react-router-dom';
import { useOrgContext } from '../contexts/OrgContext';
import { useEffect } from 'react';
import LoadingSpinner from '../components/shared/LoadingSpinner';
import ErrorState from '../components/shared/ErrorState';
import ForbiddenState from '../components/shared/ForbiddenState';

function OrgMemberRoute() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const orgId = searchParams.get('orgId');
  const { isMember, isLoading, loadWorkspaceOrg, orgId: currentOrgId } = useOrgContext();

  useEffect(() => {
    if (orgId) {
      // Guard: Only load if orgId has changed or no org is currently loaded
      if (orgId !== currentOrgId) {
        loadWorkspaceOrg(orgId);
      }
    } else {
      // Redirect to My Organizations if orgId is missing
      navigate('/user/organizations');
    }
  }, [orgId, currentOrgId, loadWorkspaceOrg, navigate]);

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (!isMember) {
    return <ForbiddenState message="You are not a member of this organization" />;
  }

  return <Outlet />;
}

export default OrgMemberRoute;

