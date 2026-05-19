/**
 * OrgReportsPlaceholderPage.jsx - Reports placeholder
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import PrototypePlaceholder from '../../components/shared/PrototypePlaceholder';
import ErrorState from '../../components/shared/ErrorState';

function OrgReportsPlaceholderPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  return (
    <div className="org-reports-placeholder-page">
      <PageHeader
        title="Báo cáo"
        description="Xem báo cáo của tổ chức và sự kiện"
      />

      <PrototypePlaceholder
        title="Báo cáo & Phân tích"
        description="Tính năng này cung cấp báo cáo và phân tích cho các sự kiện của tổ chức"
        status="PROTOTYPE_ONLY"
        notes="Nền tảng dữ liệu báo cáo đã có, nhưng giao diện/API báo cáo chưa được triển khai trong bản cơ sở."
      />
    </div>
  );
}

export default OrgReportsPlaceholderPage;
