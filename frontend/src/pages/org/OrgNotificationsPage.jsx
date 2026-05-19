/**
 * OrgNotificationsPage.jsx - Organization notifications page
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * Future Service Usage:
 * - notificationService.getNotifications(params)
 * - notificationService.markNotificationRead(id)
 * - notificationService.markAllNotificationsRead()
 * 
 * Future Adapter Usage:
 * - notificationAdapter.toNotificationViewModel()
 * 
 * Permissions:
 * - JWT (authenticated user)
 * 
 * Route: /org/notifications?orgId=
 * 
 * IMPORTANT: No real API calls in Phase 3C, No fake data
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import ErrorState from '../../components/shared/ErrorState';
import EmptyState from '../../components/shared/EmptyState';

function OrgNotificationsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Thông báo"
        description="Xem thông báo của tổ chức"
      />
      <div className="app-section">
        <div className="app-card">
          <EmptyState message="Tính năng thông báo chưa được triển khai" />
        </div>
      </div>
    </div>
  );
}

export default OrgNotificationsPage;
