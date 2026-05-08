/**
 * OrgRequestsPage.jsx - Organization requests page
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * Future Service Usage:
 * - requestService.getOrganizationRequests(orgId, params)
 * - requestService.reviewRequest(requestId, payload)
 * 
 * Future Adapter Usage:
 * - requestAdapter.toRequestViewModel()
 * 
 * Permissions:
 * - org.requests.view (read)
 * - org.requests.review/approve (review)
 * 
 * Route: /org/requests?orgId=
 * 
 * IMPORTANT: No real API calls in Phase 3C, No fake data
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import ErrorState from '../../components/shared/ErrorState';

function OrgRequestsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Requests"
        description="Manage organization join requests"
      />
      <div className="app-section">
        <div className="app-card">
          <EmptyState message="Requests feature not implemented yet" />
        </div>
      </div>
    </div>
  );
}

export default OrgRequestsPage;
