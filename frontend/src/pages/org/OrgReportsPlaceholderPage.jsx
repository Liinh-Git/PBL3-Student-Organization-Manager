/**
 * OrgReportsPlaceholderPage.jsx - Reports placeholder
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * PROTOTYPE_ONLY: EventReports entity exists in DB foundation but Reports page is not working.
 * 
 * Route: /org/reports?orgId=
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import PrototypePlaceholder from '../../components/shared/PrototypePlaceholder';
import ErrorState from '../../components/shared/ErrorState';

function OrgReportsPlaceholderPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  return (
    <div className="org-reports-placeholder-page">
      <PageHeader
        title="Reports"
        description="View organization and event reports"
      />

      <PrototypePlaceholder
        title="Reports & Analytics"
        description="This feature provides reports and analytics for organization events"
        status="PROTOTYPE_ONLY"
        notes="EventReports entity exists in database foundation but no working UI/API in base prototype."
      />
    </div>
  );
}

export default OrgReportsPlaceholderPage;
