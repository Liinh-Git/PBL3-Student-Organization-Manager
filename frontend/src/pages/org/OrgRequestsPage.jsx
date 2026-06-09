/**
 * OrgRequestsPage.jsx - Organization requests page
 */

import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import { useAuth } from "../../hooks/useAuth.js";
import { getOrganizationMembers } from "../../services/memberService.js";
import {
  createOrganizationRequest,
  getOrganizationRequests,
  reviewRequest,
} from "../../services/requestService.js";
import PageHeader from "../../components/shared/PageHeader";
import ErrorState from "../../components/shared/ErrorState";
import EmptyState from "../../components/shared/EmptyState";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgRequestsPage.css";

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleString();
}

const iconBaseProps = {
  width: 18,
  height: 18,
  viewBox: "0 0 24 24",
  fill: "none",
  stroke: "currentColor",
  strokeWidth: 2,
  strokeLinecap: "round",
  strokeLinejoin: "round",
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

// Lọc bỏ trạng thái Cancelled và Closed
const STATUS_OPTIONS = [
  { value: "All", label: "Tất cả yêu cầu" },
  { value: "Pending", label: "Đang chờ xử lý" },
  { value: "Approved", label: "Đã chấp thuận" },
  { value: "Rejected", label: "Đã từ chối" },
];

function getStatusBadgeClass(status) {
  if (status === "Approved") {
    return "kora-status-badge kora-status-success";
  }
  if (status === "Pending") {
    return "kora-status-badge kora-status-warning";
  }
  if (status === "Rejected") {
    return "kora-status-badge kora-status-rejected";
  }
  return "kora-status-badge";
}

function OrgRequestsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get("orgId");
  const { permissions, isMember } = useOrgContext();
  const { user } = useAuth();

  const [requests, setRequests] = useState([]);
  const [members, setMembers] = useState([]);
  const [statusFilter, setStatusFilter] = useState("All");
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [showCreateForm, setShowCreateForm] = useState(false);

  // State quản lý popup
  const [reviewModal, setReviewModal] = useState({
    isOpen: false,
    requestId: null,
    decision: null,
  });
  const [reviewNote, setReviewNote] = useState("");

  const canView =
    isMember &&
    (permissions.includes("org.requests.view") ||
      permissions.includes("org.requests.review") ||
      permissions.includes("org.requests.approve"));
  const canReview =
    permissions.includes("org.requests.review") ||
    permissions.includes("org.requests.approve");
  const canCreateRequest = isMember && !canReview;

  useEffect(() => {
    if (!orgId || !isMember || !canView) return;

    async function loadRequests() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getOrganizationRequests(orgId);
        // Chỉ lưu các yêu cầu Pending, Approved, Rejected
        const activeRequests = data.filter((r) =>
          ["Pending", "Approved", "Rejected"].includes(r.status),
        );
        setRequests(activeRequests);
        const memberData = await getOrganizationMembers(orgId);
        setMembers(memberData);
      } catch (err) {
        setError(err.message || "Không thể tải danh sách yêu cầu");
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
      alert("Content is required");
      return;
    }

    setIsSubmitting(true);
    try {
      const created = await createOrganizationRequest(orgId, {
        requestType: requestType || "Other",
        title: title || undefined,
        content,
      });
      setRequests((prev) => [created, ...prev]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || "Không thể tạo yêu cầu");
    } finally {
      setIsSubmitting(false);
    }
  };

  const filteredRequests = useMemo(() => {
    if (statusFilter === "All") return requests;
    return requests.filter((item) => item.status === statusFilter);
  }, [requests, statusFilter]);

  const summary = useMemo(() => {
    const pending = requests.filter((r) => r.status === "Pending").length;
    const approved = requests.filter((r) => r.status === "Approved").length;
    const rejected = requests.filter((r) => r.status === "Rejected").length;
    return { pending, approved, rejected, total: requests.length };
  }, [requests]);

  // Mở modal duyệt
  const handleReviewClick = (requestId, decision) => {
    if (!canReview) return;
    setReviewModal({ isOpen: true, requestId, decision });
    setReviewNote(""); // Reset lại ghi chú
  };

  // Xác nhận gọi API
  const confirmReview = async () => {
    const { requestId, decision } = reviewModal;
    if (!requestId || !decision) return;

    setIsSubmitting(true);
    try {
      const updated = await reviewRequest(requestId, { decision, reviewNote });
      setRequests((prev) =>
        prev.map((item) => (item.id === requestId ? updated : item)),
      );
      // Đóng modal sau khi thành công
      setReviewModal({ isOpen: false, requestId: null, decision: null });
    } catch (err) {
      alert(err.message || "Không thể duyệt yêu cầu");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  if (!isMember) {
    return (
      <div className="kora-page-wrapper">
        <PageHeader
          title="Yêu cầu đang chờ duyệt"
          description="Quản lý và xem xét các yêu cầu tham gia tổ chức cũng như các đề xuất từ thành viên."
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  if (!canView) {
    return (
      <div className="kora-page-wrapper">
        <PageHeader
          title="Yêu cầu đang chờ duyệt"
          description="Quản lý và xem xét các yêu cầu tham gia tổ chức cũng như các đề xuất từ thành viên."
        />
        <ForbiddenState message="You do not have permission to view requests" />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="kora-page-wrapper">
        <PageHeader
          title="Yêu cầu đang chờ duyệt"
          description="Quản lý và xem xét các yêu cầu tham gia tổ chức cũng như các đề xuất từ thành viên."
        />
        <LoadingSpinner message="Đang tải danh sách yêu cầu..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="kora-page-wrapper">
        <PageHeader
          title="Yêu cầu đang chờ duyệt"
          description="Quản lý và xem xét các yêu cầu tham gia tổ chức cũng như các đề xuất từ thành viên."
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="kora-page-wrapper">
      <div className="kora-header-section">
        <div className="kora-header-text">
          <h1 className="kora-page-title">Yêu cầu đang chờ duyệt</h1>
          <p className="kora-page-subtitle">
            Quản lý và xem xét các yêu cầu tham gia tổ chức cũng như các đề xuất
            từ thành viên.
          </p>
        </div>
        {canCreateRequest && (
          <button
            onClick={() => setShowCreateForm((v) => !v)}
            className="kora-btn-create"
          >
            {showCreateForm ? <IconX /> : <IconPlus />}
            {showCreateForm ? " Đóng form" : " Tạo yêu cầu mới"}
          </button>
        )}
      </div>

      <div className="kora-content-section">
        {showCreateForm && canCreateRequest && (
          <div className="kora-create-form-box">
            <h3 className="kora-box-title">Tạo yêu cầu mới</h3>
            <p className="kora-box-subtitle">
              Nêu rõ nhu cầu để tổ chức phản hồi nhanh hơn.
            </p>

            <form onSubmit={handleCreateRequest} className="kora-form">
              <div className="kora-form-grid">
                <div className="kora-form-group">
                  <label className="kora-form-label">Loại yêu cầu</label>
                  <select
                    name="requestType"
                    className="kora-form-input"
                    defaultValue="Other"
                  >
                    <option value="DepartmentChange">Chuyển phòng ban</option>
                    <option value="RoleChange">Thay đổi vai trò</option>
                    <option value="EventParticipation">Tham gia sự kiện</option>
                    <option value="Other">Khác</option>
                  </select>
                </div>
                <div className="kora-form-group">
                  <label className="kora-form-label">Tiêu đề</label>
                  <input
                    name="title"
                    className="kora-form-input"
                    placeholder="Tiêu đề (tùy chọn)"
                  />
                </div>
              </div>
              <div className="kora-form-group">
                <label className="kora-form-label">Nội dung *</label>
                <textarea
                  name="content"
                  className="kora-form-input"
                  placeholder="Nội dung yêu cầu"
                  rows={3}
                  required
                />
              </div>
              <div className="kora-form-actions">
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="kora-btn-submit"
                >
                  {isSubmitting ? "Đang gửi..." : "Gửi yêu cầu"}
                </button>
              </div>
            </form>
          </div>
        )}

        {/* Filters */}
        <div className="kora-filter-container">
          <div className="kora-filter-buttons">
            {STATUS_OPTIONS.map((option) => (
              <button
                key={option.value}
                type="button"
                className={`kora-filter-btn filter-${option.value} ${statusFilter === option.value ? "active" : ""}`}
                onClick={() => setStatusFilter(option.value)}
              >
                {option.label}
              </button>
            ))}
          </div>
        </div>

        {/* Main List Section */}
        <div className="kora-list-header">
          <h2 className="kora-section-title">Danh sách yêu cầu</h2>
          <span className="kora-count-badge">
            {filteredRequests.length} YÊU CẦU
          </span>
        </div>

        {filteredRequests.length === 0 ? (
          <EmptyState message="Không có yêu cầu nào phù hợp." />
        ) : (
          <div className="kora-request-grid">
            {filteredRequests.map((item) => {
              const statusClass = getStatusBadgeClass(item.status);
              const senderInitial = (item.senderName || "?")
                .trim()
                .charAt(0)
                .toUpperCase();

              // Xác định trạng thái tiếng Việt
              let statusLabel = item.status;
              if (item.status === "Pending") statusLabel = "ĐANG CHỜ";
              if (item.status === "Approved") statusLabel = "ĐÃ CHẤP THUẬN";
              if (item.status === "Rejected") statusLabel = "ĐÃ TỪ CHỐI";

              return (
                <div key={item.id} className="kora-req-card">
                  {/* Top: Avatar, Info, Time */}
                  <div className="kora-req-top">
                    <div className="kora-req-avatar">
                      {item.senderAvatarUrl ||
                      item.avatarUrl ||
                      item.userAvatarUrl ? (
                        <img
                          src={toAbsoluteMediaUrl(
                            item.senderAvatarUrl ||
                              item.avatarUrl ||
                              item.userAvatarUrl,
                          )}
                          alt={item.senderName}
                        />
                      ) : (
                        senderInitial
                      )}
                    </div>
                    <div className="kora-req-info">
                      <h4>{item.senderName}</h4>
                      <p>{item.senderEmail || "Không có email"}</p>
                      <div className="kora-req-tags">
                        <span className="kora-tag kora-tag-blue">
                          {item.requestType}
                        </span>
                      </div>
                    </div>
                    <div className="kora-req-time">
                      {formatDateTime(item.createdAtUtc)}
                    </div>
                  </div>

                  {/* Body: Title, Content, Status */}
                  <div className="kora-req-body">
                    {item.title && (
                      <h5 className="kora-req-title">{item.title}</h5>
                    )}

                    <div className="kora-req-meta">
                      <span className={statusClass}>{statusLabel}</span>
                      {item.reviewedByMemberName && (
                        <span className="kora-req-reviewer">
                          | Duyệt bởi: {item.reviewedByMemberName} (
                          {formatDateTime(item.reviewedAt)})
                        </span>
                      )}
                    </div>

                    {item.reviewNote && (
                      <div className="kora-req-note">
                        <strong>Ghi chú:</strong> {item.reviewNote}
                      </div>
                    )}
                  </div>

                  {/* Bottom: Actions (Accept/Reject) */}
                  <div className="kora-req-actions">
                    {canReview && item.status === "Pending" ? (
                      <>
                        <button
                          className="kora-btn-accept"
                          disabled={isSubmitting}
                          onClick={() => handleReviewClick(item.id, "Approved")}
                        >
                          CHẤP NHẬN
                        </button>
                        <button
                          className="kora-btn-reject"
                          disabled={isSubmitting}
                          onClick={() => handleReviewClick(item.id, "Rejected")}
                        >
                          TỪ CHỐI
                        </button>
                      </>
                    ) : (
                      <span className="kora-text-muted">
                        Không có thao tác khả dụng
                      </span>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* POPUP MODAL DUYỆT */}
      {reviewModal.isOpen && (
        <div className="kora-modal-overlay">
          <div className="kora-modal-content">
            <h3 className="kora-modal-title">
              {reviewModal.decision === "Approved"
                ? "Chấp thuận yêu cầu"
                : "Từ chối yêu cầu"}
            </h3>

            <div className="kora-modal-form-group">
              <label className="kora-modal-label">
                GHI CHÚ{" "}
                {reviewModal.decision === "Rejected" ? "(LÝ DO)" : "(TÙY CHỌN)"}
              </label>
              <textarea
                className="kora-modal-input"
                value={reviewNote}
                onChange={(e) => setReviewNote(e.target.value)}
                rows={3}
                placeholder="Nhập ghi chú tại đây..."
              />
            </div>

            <div className="kora-modal-actions">
              <button
                className="kora-modal-btn-cancel"
                disabled={isSubmitting}
                onClick={() =>
                  setReviewModal({
                    isOpen: false,
                    requestId: null,
                    decision: null,
                  })
                }
              >
                Hủy
              </button>
              <button
                className="kora-modal-btn-confirm"
                disabled={isSubmitting}
                onClick={confirmReview}
              >
                {isSubmitting ? "Đang xử lý..." : "Xác nhận"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrgRequestsPage;
