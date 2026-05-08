/**
 * OrgResourcesPlaceholderPage.jsx - Resources placeholder
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * PROTOTYPE_ONLY: Resources entity exists in DB foundation but Resources page is not working.
 * 
 * Route: /org/resources?orgId=
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import PrototypePlaceholder from '../../components/shared/PrototypePlaceholder';
import ErrorState from '../../components/shared/ErrorState';

function OrgResourcesPlaceholderPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  return (
    <div className="org-resources-placeholder-page">
      <PageHeader
        title="Resources"
        description="Manage organization resources"
      />

      <PrototypePlaceholder
        title="Resources Management"
        description="This feature manages organization resources (equipment, facilities, etc.)"
        status="PROTOTYPE_ONLY"
        notes="Resources entity exists in database foundation but no working UI/API in base prototype."
      />
    </div>
  );
}

export default OrgResourcesPlaceholderPage;
