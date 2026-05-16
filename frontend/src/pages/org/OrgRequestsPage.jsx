/**
 * OrgRequestsPage.jsx - Organization requests page
 */

import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { useAuth } from '../../hooks/useAuth.js';
import { getOrganizationMembers } from '../../services/memberService.js';
import { createOrganizationRequest, getOrganizationRequests, reviewRequest } from '../../services/requestService.js';
import PageHeader from '../../components/shared/PageHeader';
import ErrorState from '../../components/shared/ErrorState';
import EmptyState from '../../components/shared/EmptyState';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ForbiddenState from '../../components/shared/ForbiddenState';
import './OrgRequestsPage.css';

function formatDateTime(value) {
  if (!value) return '-';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';
  return date.toLocaleString();
}

const iconBaseProps = {
  width: 18,
  height: 18,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 2,
  strokeLinecap: 'round',
  strokeLinejoin: 'round'
};

const IconClock = () => (
  <svg {...iconBaseProps} aria-hidden="true">
    <path d="M12 8v4l3 3" />
    <path d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
);

const IconCheck = () => (
  <svg {...iconBaseProps} aria-hidden="true">
    <path d="M5 13l4 4L19 7" />
  </svg>
);

const IconX = () => (
  <svg {...iconBaseProps} aria-hidden="true">
    <path d="M6 18L18 6M6 6l12 12" />
  </svg>
);

const IconUser = () => (
  <svg {...iconBaseProps} aria-hidden="true">
    <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0z" />
    <path d="M12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
  </svg>
);

const IconPlus = () => (
  <svg {...iconBaseProps} aria-hidden="true">
    <path d="M12 4v16m8-8H4" />
  </svg>
);

const IconCalendar = () => (
  <svg {...iconBaseProps} aria-hidden="true">
    <path d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
  </svg>
);

const IconReply = () => (
  <svg {...iconBaseProps} aria-hidden="true">
    <path d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
  </svg>
);

const STATUS_OPTIONS = [
  { value: 'All', label: 'Tất cả yêu cầu' },
  { value: 'Pending', label: 'Đang chờ xử lý' },
  { value: 'Approved', label: 'Đã chấp thuận' },
  { value: 'Rejected', label: 'Đã từ chối' },
  { value: 'Cancelled', label: 'Đã hủy' },
  { value: 'Closed', label: 'Đã đóng' }
];

function getStatusBadgeClass(status) {
  if (status === 'Approved') {
    return 'app-badge app-badge--success org-requests-status';
  }
  if (status === 'Pending') {
    return 'app-badge app-badge--warning org-requests-status';
  }
  if (status === 'Rejected') {
    return 'app-badge org-requests-status org-requests-status--rejected';
  }
  return 'app-badge org-requests-status';
}

function OrgRequestsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');
  const { permissions, isMember } = useOrgContext();
  const { user } = useAuth();

  const [requests, setRequests] = useState([]);
  const [members, setMembers] = useState([]);
  const [statusFilter, setStatusFilter] = useState('All');
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [showCreateForm, setShowCreateForm] = useState(false);

  const canView = isMember && (
    permissions.includes('org.requests.view') ||
    permissions.includes('org.requests.review') ||
    permissions.includes('org.requests.approve')
  );
  const canReview = permissions.includes('org.requests.review') || permissions.includes('org.requests.approve');
  const canCreateRequest = isMember && !canReview;

  useEffect(() => {
    if (!orgId || !isMember || !canView) return;

    async function loadRequests() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getOrganizationRequests(orgId);
        setRequests(data);
        const memberData = await getOrganizationMembers(orgId);
        setMembers(memberData);
      } catch (err) {
        setError(err.message || 'Failed to load requests');
      } finally {
        setIsLoading(false);
      }
    }

    loadRequests();
  }, [orgId, isMember, canView]);

  const handleCreateRequest = async (e) => {
    e.preventDefault();
    if (!canCreateRequest) return;

    const form = e.target;
    const requestType = form.requestType.value;
    const content = form.content.value;
    const title = form.title.value;

    if (!content) {
      alert('Content is required');
      return;
    }

    setIsSubmitting(true);
    try {
      const created = await createOrganizationRequest(orgId, {
        requestType: requestType || 'Other',
        title: title || undefined,
        content
      });
      setRequests((prev) => [created, ...prev]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || 'Failed to create request');
    } finally {
      setIsSubmitting(false);
    }
  };

  const filteredRequests = useMemo(() => {
    if (statusFilter === 'All') return requests;
    return requests.filter((item) => item.status === statusFilter);
  }, [requests, statusFilter]);

  const summary = useMemo(() => {
    const pending = requests.filter((r) => r.status === 'Pending').length;
    const approved = requests.filter((r) => r.status === 'Approved').length;
    const rejected = requests.filter((r) => r.status === 'Rejected').length;
    return { pending, approved, rejected, total: requests.length };
  }, [requests]);

  const handleReview = async (requestId, decision) => {
    if (!canReview) return;

    const reviewNote = window.prompt(
      `${decision} request${decision === 'Rejected' ? ' - nhập lý do (optional)' : ' - ghi chú (optional)'}`,
      ''
    );

    if (reviewNote === null) {
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await reviewRequest(requestId, { decision, reviewNote });
      setRequests((prev) => prev.map((item) => (item.id === requestId ? updated : item)));
    } catch (err) {
      alert(err.message || 'Failed to review request');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader
          title="Quản lý Yêu cầu"
          description="Kiểm duyệt và xử lý các kiến nghị từ thành viên"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  if (!canView) {
    return (
      <div className="app-page">
        <PageHeader
          title="Quản lý Yêu cầu"
          description="Kiểm duyệt và xử lý các kiến nghị từ thành viên"
        />
        <ForbiddenState message="You do not have permission to view requests" />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Quản lý Yêu cầu"
          description="Kiểm duyệt và xử lý các kiến nghị từ thành viên"
        />
        <LoadingSpinner message="Loading requests..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Quản lý Yêu cầu"
          description="Kiểm duyệt và xử lý các kiến nghị từ thành viên"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  const summaryCards = [
    { key: 'total', label: 'Tất cả đơn', value: summary.total, tone: 'primary', icon: <IconUser /> },
    { key: 'pending', label: 'Cần xử lý', value: summary.pending, tone: 'warning', icon: <IconClock /> },
    { key: 'approved', label: 'Đã chấp thuận', value: summary.approved, tone: 'success', icon: <IconCheck /> },
    { key: 'rejected', label: 'Đã từ chối', value: summary.rejected, tone: 'danger', icon: <IconX /> }
  ];

  return (
    <div className="app-page org-requests-page">
      <PageHeader
        title="Quản lý Yêu cầu"
        description="Kiểm duyệt và xử lý các kiến nghị từ thành viên"
        actions={
          canCreateRequest ? (
            <button
              onClick={() => setShowCreateForm((v) => !v)}
              className={`app-button ${showCreateForm ? 'app-button--ghost' : 'app-button--primary'}`}
            >
              {showCreateForm ? <IconX /> : <IconPlus />}
              {showCreateForm ? 'Đóng form' : 'Tạo yêu cầu'}
            </button>
          ) : null
        }
      />
      <div className="app-section org-requests-section">
        {showCreateForm && canCreateRequest && (
          <div className="app-card org-requests-create">
            <div className="app-section-header">
              <div>
                <h3 className="app-section-title">Tạo yêu cầu mới</h3>
                <p className="app-section-subtitle">Nêu rõ nhu cầu để tổ chức phản hồi nhanh hơn.</p>
              </div>
            </div>
            <form onSubmit={handleCreateRequest} className="auth-form org-requests-form">
              <div className="org-requests-form-grid">
                <div className="form-group">
                  <label className="form-label">Loại yêu cầu</label>
                  <select name="requestType" className="form-select" defaultValue="Other">
                    <option value="DepartmentChange">Chuyển phòng ban</option>
                    <option value="RoleChange">Thay đổi vai trò</option>
                    <option value="EventParticipation">Tham gia sự kiện</option>
                    <option value="Other">Khác</option>
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Tiêu đề</label>
                  <input name="title" className="form-input" placeholder="Tiêu đề (tùy chọn)" />
                </div>
              </div>
              <div className="form-group">
                <label className="form-label">Nội dung *</label>
                <textarea
                  name="content"
                  className="form-input"
                  placeholder="Nội dung yêu cầu"
                  rows={3}
                  required
                />
              </div>
              <div className="app-action-row org-requests-form-actions">
                <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                  {isSubmitting ? 'Đang gửi...' : 'Gửi yêu cầu'}
                </button>
              </div>
            </form>
          </div>
        )}

        <div className="org-requests-summary-grid">
          {summaryCards.map((card) => (
            <div key={card.key} className={`app-card org-requests-summary-card org-requests-summary-card--${card.tone}`}>
              <div>
                <div className="org-requests-summary-label">{card.label}</div>
                <div className="org-requests-summary-value">{card.value}</div>
              </div>
              <div className="org-requests-summary-icon">
                {card.icon}
              </div>
            </div>
          ))}
        </div>

        <div className="app-card org-requests-filter-card">
          <div className="app-section-header">
            <div>
              <h3 className="app-section-title">Bộ lọc trạng thái</h3>
              <p className="app-section-subtitle">Chọn trạng thái để lọc nhanh danh sách yêu cầu.</p>
            </div>
          </div>
          <div className="org-requests-filters">
            {STATUS_OPTIONS.map((option) => (
              <button
                key={option.value}
                type="button"
                className={`app-button org-requests-filter-button ${statusFilter === option.value ? 'org-requests-filter-button--active' : ''}`}
                onClick={() => setStatusFilter(option.value)}
              >
                {option.label}
              </button>
            ))}
          </div>
        </div>

        <div className="app-section-header org-requests-list-header">
          <div>
            <h3 className="app-section-title">Danh sách yêu cầu</h3>
            <p className="app-section-subtitle">Theo dõi chi tiết từng yêu cầu từ thành viên.</p>
          </div>
          <span className="app-badge app-badge--info org-requests-count">{filteredRequests.length}</span>
        </div>

        {filteredRequests.length === 0 ? (
          <EmptyState message="Không có yêu cầu nào" />
        ) : (
          <div className="org-requests-list">
            {filteredRequests.map((item) => {
              const statusClass = getStatusBadgeClass(item.status);
              const senderInitial = (item.senderName || '?').trim().charAt(0).toUpperCase();

              return (
                <div key={item.id} className="app-card org-requests-card">
                  <div className="org-requests-card-header">
                    <div className="org-requests-card-sender">
                      <div className="org-requests-avatar">{senderInitial}</div>
                      <div>
                        <div className="org-requests-sender-name">{item.senderName}</div>
                        <div className="org-requests-sender-email">{item.senderEmail || '-'}</div>
                      </div>
                    </div>
                    <span className={statusClass}>{item.status}</span>
                  </div>

                  <div className="org-requests-chips">
                    <span className="app-chip org-requests-chip">{item.requestType}</span>
                    <span className="app-chip org-requests-chip org-requests-chip--accent">Phòng ban: {item.desiredDepartmentName || '-'}</span>
                    <span className="app-chip org-requests-chip org-requests-chip--accent">Vị trí: {item.desiredPosition || '-'}</span>
                  </div>

                  <div>
                    <div className="org-requests-title">{item.title || '-'}</div>
                    <div className="org-requests-content">{item.content}</div>
                  </div>

                  <div className="org-requests-meta">
                    <div className="org-requests-timeline">
                      <IconCalendar />
                      <span>Đã tạo: {formatDateTime(item.createdAtUtc)}</span>
                    </div>
                    <div className="org-requests-review-meta">
                      <span>Đã duyệt: {formatDateTime(item.reviewedAt)}</span>
                      <span>Người duyệt: {item.reviewedByMemberName || '-'}</span>
                    </div>
                    {item.reviewNote ? (
                      <div className="org-requests-review-note">
                        <div className="org-requests-review-title">
                          <IconReply />
                          <span>Ghi chú duyệt</span>
                        </div>
                        <div className="org-requests-review-text">{item.reviewNote}</div>
                      </div>
                    ) : null}
                  </div>

                  <div className="org-requests-actions">
                    {canReview && item.status === 'Pending' ? (
                      <div className="app-action-row">
                        <button
                          className="app-button app-button--danger"
                          disabled={isSubmitting}
                          onClick={() => handleReview(item.id, 'Rejected')}
                        >
                          <IconX />
                          Từ chối
                        </button>
                        <button
                          className="app-button app-button--primary"
                          disabled={isSubmitting}
                          onClick={() => handleReview(item.id, 'Approved')}
                        >
                          <IconCheck />
                          Chấp thuận
                        </button>
                      </div>
                    ) : (
                      <span className="org-requests-actions-muted">-</span>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}

export default OrgRequestsPage;
