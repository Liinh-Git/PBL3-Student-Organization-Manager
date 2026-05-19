/**
 * OrgFinancePlaceholderPage.jsx - Finance placeholder
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * PROTOTYPE_ONLY: Finance-specific module is excluded from base prototype.
 * 
 * Route: /org/finance?orgId=
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import PrototypePlaceholder from '../../components/shared/PrototypePlaceholder';
import ErrorState from '../../components/shared/ErrorState';

function OrgFinancePlaceholderPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  return (
    <div className="org-finance-placeholder-page">
      <PageHeader
        title="Finance"
        description="Quản lý tài chính của tổ chức"
      />

      <PrototypePlaceholder
        title="Finance Management"
        description="This feature manages organization finances, budgets, and transactions"
        status="PROTOTYPE_ONLY"
        notes="Finance-specific module is excluded from base prototype. Event.Budget field exists but no working finance module."
      />
    </div>
  );
}

export default OrgFinancePlaceholderPage;
