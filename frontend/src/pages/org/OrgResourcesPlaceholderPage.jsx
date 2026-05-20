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
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  return (
    <div className="app-page org-resources-placeholder-page">
      <PageHeader
        title="Tài nguyên"
        description="Quản lý tài nguyên của tổ chức"
      />

      <div className="app-section">
        <PrototypePlaceholder
          title="Quản lý tài nguyên"
          description="Tính năng này quản lý tài nguyên tổ chức như thiết bị, cơ sở vật chất..."
          status="PROTOTYPE_ONLY"
          notes="Đã có cấu trúc dữ liệu tài nguyên trong nền tảng, nhưng UI/API đầy đủ chưa được triển khai ở bản cơ sở."
        />
      </div>
    </div>
  );
}

export default OrgResourcesPlaceholderPage;
