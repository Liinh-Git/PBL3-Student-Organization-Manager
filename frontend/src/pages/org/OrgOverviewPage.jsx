/**
 * OrgOverviewPage.jsx - Organization overview page
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import { updateOrganization } from "../../services/organizationService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgOverviewPage.css";

function OrgOverviewPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get("orgId");
  const {
    organization: contextOrg,
    loadWorkspaceOrg,
    permissions,
    isMember,
    isLoading: contextLoading,
  } = useOrgContext();

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    if (orgId && (!contextOrg || String(contextOrg.id) !== String(orgId))) {
      loadWorkspaceOrg(orgId);
    }
  }, [orgId, contextOrg, loadWorkspaceOrg]);

  if (!orgId) {
    return <ErrorState message="Cần có ID tổ chức để xem trang này" />;
  }

  if (contextLoading) {
    return <LoadingSpinner message="Đang tải dữ liệu tổ chức..." />;
  }

  if (!isMember) {
    return (
      <ForbiddenState message="Bạn không phải là thành viên của tổ chức này" />
    );
  }

  const canEdit = permissions.includes("org.overview.write");

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canEdit || !orgId) return;

    const form = e.target;
    const payload = {
      name: form.orgName.value,
      description: form.description.value || undefined,
      location: form.location.value || undefined,
      contactEmail: form.contactEmail.value || undefined,
      contactPhone: form.contactPhone.value || undefined,
    };

    setIsSubmitting(true);
    try {
      await updateOrganization(orgId, payload);
      loadWorkspaceOrg(orgId);
      setShowEditForm(false);
    } catch (err) {
      alert(err.message || "Cập nhật thất bại");
    } finally {
      setIsSubmitting(false);
    }
  };

  const displayFoundingDate = contextOrg?.foundingDate
    ? new Date(contextOrg.foundingDate).toLocaleDateString("vi-VN")
    : "Chưa cập nhật";

  return (
    <div className="org-overview-container">
      {/* Banner */}
      <div className="org-banner-section"></div>

      {/* Header Info */}
      <div className="org-profile-nav">
        <div className="org-avatar-frame">
          <svg
            width="45"
            height="45"
            viewBox="0 0 24 24"
            fill="none"
            stroke="white"
            strokeWidth="2.5"
          >
            <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" />
          </svg>
        </div>
        <div className="org-title-block">
          <h1>{contextOrg?.name || "Tổ chức"}</h1>
          <p>Trang thông tin tổng quan và liên hệ chính thức.</p>
        </div>
        {canEdit && (
          <button
            onClick={() => setShowEditForm(true)}
            className="org-btn-header"
          >
            Chỉnh sửa hồ sơ
          </button>
        )}
      </div>

      {/* Stats Grid */}
      <div className="org-stats-dashboard">
        <div className="stat-item-card">
          <div className="stat-icon-circle">
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
            >
              <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
              <circle cx="12" cy="10" r="3" />
            </svg>
          </div>
          <div>
            <span className="org-form-label-small">Địa điểm</span>
            <p className="stat-value-text">
              {contextOrg?.location || "Chưa xác định"}
            </p>
          </div>
        </div>

        <div className="stat-item-card">
          <div className="stat-icon-circle">
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
            >
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
              <circle cx="9" cy="7" r="4" />
              <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            </svg>
          </div>
          <div>
            <span className="org-form-label-small">Thành viên</span>
            <p className="stat-value-text">
              {contextOrg?.totalMembers || 0} Người
            </p>
          </div>
        </div>

        <div className="stat-item-card">
          <div className="stat-icon-circle">
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
            >
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
              <line x1="16" y1="2" x2="16" y2="6" />
              <line x1="8" y1="2" x2="8" y2="6" />
              <line x1="3" y1="10" x2="21" y2="10" />
            </svg>
          </div>
          <div>
            <span className="org-form-label-small">Ngày thành lập</span>
            <p className="stat-value-text">{displayFoundingDate}</p>
          </div>
        </div>
      </div>

      {/* Main Content Layout */}
      <div className="org-main-layout">
        <div className="layout-left">
          <h2 className="content-header">Giới thiệu chung</h2>
          <div className="info-text-card">
            {contextOrg?.description || "Tổ chức chưa có mô tả chi tiết."}
          </div>
        </div>

        <div className="layout-right">
          <h2 className="content-header">Thông tin liên hệ</h2>
          <div className="contact-info-list">
            <div className="contact-row">
              <span className="org-form-label-small">Email liên hệ</span>
              <p className="contact-val-text">
                {contextOrg?.contactEmail || "-"}
              </p>
            </div>
            <div className="contact-row">
              <span className="org-form-label-small">Số điện thoại</span>
              <p className="contact-val-text">
                {contextOrg?.contactPhone || "-"}
              </p>
            </div>
            <div className="contact-row">
              <span className="org-form-label-small">
                Ngày gia nhập hệ thống
              </span>
              <p className="contact-val-text">
                {contextOrg?.createdAt
                  ? new Date(contextOrg.createdAt).toLocaleDateString("vi-VN")
                  : "-"}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Edit Profile Modal - Cấu trúc đồng bộ với Tạo tổ chức */}
      {showEditForm && canEdit && (
        <div
          className="org-modal-overlay"
          onClick={() => setShowEditForm(false)}
        >
          <div className="org-modal" onClick={(e) => e.stopPropagation()}>
            <div className="org-modal-header">
              <h3>Chỉnh sửa hồ sơ</h3>
              <p>
                Cập nhật thông tin chi tiết để mọi người hiểu rõ hơn về tổ chức
                của bạn.
              </p>
            </div>

            <div className="org-modal-body">
              <form id="editOrgForm" onSubmit={handleUpdate}>
                <div className="org-form-group">
                  <label htmlFor="orgName" className="org-form-label">
                    Tên tổ chức *
                  </label>
                  <input
                    type="text"
                    id="orgName"
                    name="orgName"
                    className="org-input"
                    defaultValue={contextOrg?.name || ""}
                    required
                    placeholder="Nhập tên tổ chức..."
                  />
                </div>

                <div className="org-form-group">
                  <label htmlFor="description" className="org-form-label">
                    Mô tả hoạt động
                  </label>
                  <textarea
                    id="description"
                    name="description"
                    className="org-input"
                    defaultValue={contextOrg?.description || ""}
                    placeholder="Mô tả về mục đích và các hoạt động..."
                    style={{ minHeight: "120px", resize: "vertical" }}
                  />
                </div>

                <div className="org-form-group">
                  <label htmlFor="location" className="org-form-label">
                    Địa điểm
                  </label>
                  <input
                    type="text"
                    id="location"
                    name="location"
                    className="org-input"
                    defaultValue={contextOrg?.location || ""}
                    placeholder="Ví dụ: Tòa nhà A1, Đại học Bách Khoa..."
                  />
                </div>

                <div className="org-form-row">
                  <div className="org-form-group flex-1">
                    <label htmlFor="contactEmail" className="org-form-label">
                      Email liên hệ
                    </label>
                    <input
                      type="email"
                      id="contactEmail"
                      name="contactEmail"
                      className="org-input"
                      defaultValue={contextOrg?.contactEmail || ""}
                      placeholder="email@example.com"
                    />
                  </div>
                  <div className="org-form-group flex-1">
                    <label htmlFor="contactPhone" className="org-form-label">
                      Số điện thoại
                    </label>
                    <input
                      type="tel"
                      id="contactPhone"
                      name="contactPhone"
                      className="org-input"
                      defaultValue={contextOrg?.contactPhone || ""}
                      placeholder="0123 456 789"
                    />
                  </div>
                </div>
              </form>
            </div>

            <div className="org-modal-footer">
              <button
                type="button"
                onClick={() => setShowEditForm(false)}
                className="org-btn org-btn-secondary"
                disabled={isSubmitting}
              >
                Hủy bỏ
              </button>
              <button
                type="submit"
                form="editOrgForm"
                className="org-btn org-btn-primary"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Đang lưu..." : "Lưu thay đổi"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrgOverviewPage;
