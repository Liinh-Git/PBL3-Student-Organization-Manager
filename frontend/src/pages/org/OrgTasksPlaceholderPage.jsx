/**
 * OrgTasksPlaceholderPage.jsx - Aggregate task board placeholder
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import PrototypePlaceholder from '../../components/shared/PrototypePlaceholder';
import ErrorState from '../../components/shared/ErrorState';

function OrgTasksPlaceholderPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  return (
    <div className="app-page org-tasks-placeholder-page">
      <PageHeader
        title="Bảng công việc"
        description="Bảng công việc tổng hợp từ tất cả sự kiện"
      />

      <div className="app-section">
        <PrototypePlaceholder
          title="Bảng công việc tổng hợp"
          description="Tính năng này hiển thị toàn bộ nhiệm vụ của các sự kiện trong một bảng thống nhất"
          status="PROTOTYPE_ONLY"
          notes="CRUD nhiệm vụ đã có đầy đủ trong cây Event Detail (Sự kiện → Mốc → Hạng mục → Nhiệm vụ). Bảng tổng hợp chưa được triển khai trong bản cơ sở."
        />
      </div>
    </div>
  );
}

export default OrgTasksPlaceholderPage;
