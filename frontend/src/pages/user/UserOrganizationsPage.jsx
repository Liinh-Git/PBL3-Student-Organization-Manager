/**
 * UserOrganizationsPage.jsx - User's organizations page
 * * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getMyOrganizations } from "../../services/userService.js";
import {
  createOrganization,
  deleteOrganization,
  uploadOrganizationImage,
} from "../../services/organizationService.js";
import OrgCard from "../../components/org/OrgCard";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import "./UserOrganizationsPage.css";

function UserOrganizationsPage() {
  const navigate = useNavigate();

  const [organizations, setOrganizations] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  // States cho Tạo Tổ chức
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [createError, setCreateError] = useState(null);

  // States cho Xóa / Giải tán Tổ chức
  const [deletingOrgId, setDeletingOrgId] = useState(null);
  const [orgToDelete, setOrgToDelete] = useState(null); // Quản lý Popup Xóa

  const [formData, setFormData] = useState({
    orgName: "",
    description: "",
    foundingDate: "",
    location: "",
    contactEmail: "",
    contactPhone: "",
  });
  const [imageFiles, setImageFiles] = useState({
    avatar: null,
    cover: null,
  });

  useEffect(() => {
    async function loadOrganizations() {
      setIsLoading(true);
      try {
        const data = await getMyOrganizations();
        setOrganizations(data);
      } catch (err) {
        setError(err.message || "Tải danh sách tổ chức thất bại");
      } finally {
        setIsLoading(false);
      }
    }
    loadOrganizations();
  }, []);

  const handleOrgClick = (orgId) => {
    navigate(`/org/overview?orgId=${orgId}`);
  };

  const resetCreateForm = () => {
    setFormData({
      orgName: "",
      description: "",
      foundingDate: "",
      location: "",
      contactEmail: "",
      contactPhone: "",
    });
    setImageFiles({
      avatar: null,
      cover: null,
    });
    setCreateError(null);
  };

  const handleCloseCreateModal = () => {
    if (isCreating) return;
    setShowCreateModal(false);
    resetCreateForm();
  };

  const handleCreateOrganization = async (e) => {
    e.preventDefault();
    setIsCreating(true);
    setCreateError(null);

    try {
      // Build payload with only non-empty fields
      const payload = {};
      if (formData.orgName) payload.orgName = formData.orgName;
      if (formData.description) payload.description = formData.description;
      if (formData.foundingDate) payload.foundingDate = formData.foundingDate;
      if (formData.location) payload.location = formData.location;
      if (formData.contactEmail) payload.contactEmail = formData.contactEmail;
      if (formData.contactPhone) payload.contactPhone = formData.contactPhone;

      const createdOrg = await createOrganization(payload);

      const uploadTasks = [];
      if (createdOrg?.id && imageFiles.avatar) {
        uploadTasks.push(
          uploadOrganizationImage(createdOrg.id, imageFiles.avatar, "avatar"),
        );
      }
      if (createdOrg?.id && imageFiles.cover) {
        uploadTasks.push(
          uploadOrganizationImage(createdOrg.id, imageFiles.cover, "cover"),
        );
      }

      if (uploadTasks.length > 0) {
        const uploadResults = await Promise.allSettled(uploadTasks);
        const failedUpload = uploadResults.find(
          (result) => result.status === "rejected",
        );
        if (failedUpload) {
          const message =
            failedUpload.reason?.response?.data?.message ||
            failedUpload.reason?.message ||
            "Upload ảnh thất bại";
          setError(
            `Tạo tổ chức thành công nhưng upload ảnh thất bại: ${message}`,
          );
        }
      }

      // Refresh organizations list
      const updatedOrgs = await getMyOrganizations();
      setOrganizations(updatedOrgs);

      // Close modal and reset form
      setShowCreateModal(false);
      resetCreateForm();
    } catch (err) {
      setCreateError(
        err.response?.data?.message || err.message || "Tạo tổ chức thất bại",
      );
    } finally {
      setIsCreating(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleImageFileChange = (e) => {
    const { name, files } = e.target;
    setImageFiles((prev) => ({
      ...prev,
      [name]: files?.[0] || null,
    }));
  };

  // Mở Popup Xóa
  const handleDeleteClick = (orgId, orgName) => {
    const org = organizations.find((o) => o.id === orgId);
    setOrgToDelete(org || { id: orgId, name: orgName });
  };

  // Xác nhận Xóa thực tế gọi API
  const confirmDeleteOrganization = async () => {
    if (!orgToDelete) return;
    setDeletingOrgId(orgToDelete.id);
    setError(null);

    try {
      await deleteOrganization(orgToDelete.id);
      setOrganizations((prev) =>
        prev.filter((org) => org.id !== orgToDelete.id),
      );
      setOrgToDelete(null); // Đóng popup sau khi xóa xong
    } catch (err) {
      setError(err.message || "Rời tổ chức thất bại");
    } finally {
      setDeletingOrgId(null);
    }
  };

  // Kiểm tra quyền Chủ nhiệm dựa trên Role name trả về
  const checkIsPresident = (org) => {
    if (!org) return false;
    const role = (org.roleName || org.role?.roleName || "").toLowerCase();
    return (
      role.includes("chủ nhiệm") || role.includes("president") || org.isOwner
    );
  };

  return (
    <div className="org-layout">
      {/* Header */}
      <div className="org-header">
        <div className="org-header-icon">
          <svg
            width="28"
            height="28"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="M3 21h18"></path>
            <path d="M3 10h18"></path>
            <path d="M5 6l7-3 7 3"></path>
            <path d="M4 10v11"></path>
            <path d="M20 10v11"></path>
            <path d="M8 14v3"></path>
            <path d="M12 14v3"></path>
            <path d="M16 14v3"></path>
          </svg>
        </div>
        <div className="org-header-text">
          <h1>Tổ chức của tôi</h1>
          <p>Quản lý và tham gia xây dựng cộng đồng</p>
        </div>
      </div>

      {isLoading ? (
        <LoadingSpinner message="Đang tải danh sách tổ chức..." />
      ) : error ? (
        <ErrorState message={error} />
      ) : (
        <div className="org-section">
          <div className="org-section-title">
            <h2>Danh sách tổ chức</h2>
            <span className="org-badge">Tổng cộng: {organizations.length}</span>
          </div>

          <div className="org-grid">
            {/* Create New Card */}
            <button
              className="org-create-card"
              onClick={() => {
                resetCreateForm();
                setShowCreateModal(true);
              }}
            >
              <div className="org-create-icon">+</div>
              <h3>Tạo tổ chức mới</h3>
              <p>
                Bắt đầu hành trình xây dựng một cộng đồng sinh viên mới của
                riêng bạn.
              </p>
            </button>

            {/* Organizations List */}
            {organizations.map((org) => (
              <OrgCard
                key={org.id}
                organization={org}
                onClick={handleOrgClick}
                onDelete={handleDeleteClick} /* Đổi hàm gọi sang mở Popup */
                isDeleting={deletingOrgId === org.id}
              />
            ))}
          </div>
        </div>
      )}

      {/* ─── POPUP TẠO TỔ CHỨC ─── */}
      {showCreateModal && (
        <div className="org-modal-overlay" onClick={handleCloseCreateModal}>
          <div className="org-modal" onClick={(e) => e.stopPropagation()}>
            <div className="org-modal-header">
              <h3>Tạo tổ chức mới</h3>
              <p>
                Khai báo thông tin cơ bản để thành lập một câu lạc bộ hoặc tổ
                chức mới.
              </p>
            </div>

            <div className="org-modal-body">
              <form id="createOrgForm" onSubmit={handleCreateOrganization}>
                {createError && (
                  <div className="org-alert-error">{createError}</div>
                )}

                <div className="org-form-group">
                  <label htmlFor="orgName" className="org-form-label">
                    Tên tổ chức *
                  </label>
                  <input
                    type="text"
                    id="orgName"
                    name="orgName"
                    value={formData.orgName}
                    onChange={handleInputChange}
                    className="org-input"
                    required
                    minLength={2}
                    maxLength={200}
                    placeholder="Ví dụ: CLB Âm nhạc Kora, Ban Truyền thông..."
                  />
                </div>

                <div className="org-form-group">
                  <label htmlFor="description" className="org-form-label">
                    Mô tả hoạt động
                  </label>
                  <textarea
                    id="description"
                    name="description"
                    value={formData.description}
                    onChange={handleInputChange}
                    className="org-input"
                    placeholder="Giới thiệu ngắn gọn về mục đích, sứ mệnh và hoạt động của tổ chức..."
                  />
                </div>

                <div className="org-form-group">
                  <label htmlFor="location" className="org-form-label">
                    Địa điểm (Không bắt buộc)
                  </label>
                  <input
                    type="text"
                    id="location"
                    name="location"
                    value={formData.location}
                    onChange={handleInputChange}
                    className="org-input"
                    placeholder="Ví dụ: Tòa nhà A1, Đại học Bách Khoa..."
                  />
                </div>

                {/* Phần thông tin thêm */}
                <div
                  style={{
                    marginTop: "2rem",
                    paddingTop: "1rem",
                    borderTop: "1px solid #e2e8f0",
                  }}
                >
                  <p
                    style={{
                      fontSize: "0.8rem",
                      color: "#94a3b8",
                      marginBottom: "1rem",
                    }}
                  >
                    THÔNG TIN BỔ SUNG
                  </p>

                  <div className="org-form-group">
                    <label htmlFor="avatar" className="org-form-label">
                      Ảnh đại diện
                    </label>
                    <input
                      type="file"
                      id="avatar"
                      name="avatar"
                      accept="image/jpeg,image/png,image/webp"
                      onChange={handleImageFileChange}
                      className="org-input"
                    />
                  </div>

                  <div className="org-form-group">
                    <label htmlFor="cover" className="org-form-label">
                      Ảnh bìa
                    </label>
                    <input
                      type="file"
                      id="cover"
                      name="cover"
                      accept="image/jpeg,image/png,image/webp"
                      onChange={handleImageFileChange}
                      className="org-input"
                    />
                  </div>

                  <div className="org-form-group">
                    <label htmlFor="foundingDate" className="org-form-label">
                      Ngày thành lập (Không bắt buộc)
                    </label>
                    <input
                      type="date"
                      id="foundingDate"
                      name="foundingDate"
                      value={formData.foundingDate}
                      onChange={handleInputChange}
                      className="org-input"
                    />
                  </div>

                  <div style={{ display: "flex", gap: "1rem" }}>
                    <div className="org-form-group" style={{ flex: 1 }}>
                      <label htmlFor="contactEmail" className="org-form-label">
                        Email liên hệ
                      </label>
                      <input
                        type="email"
                        id="contactEmail"
                        name="contactEmail"
                        value={formData.contactEmail}
                        onChange={handleInputChange}
                        className="org-input"
                        placeholder="contact@example.com"
                      />
                    </div>
                    <div className="org-form-group" style={{ flex: 1 }}>
                      <label htmlFor="contactPhone" className="org-form-label">
                        Số điện thoại liên hệ
                      </label>
                      <input
                        type="tel"
                        id="contactPhone"
                        name="contactPhone"
                        value={formData.contactPhone}
                        onChange={handleInputChange}
                        className="org-input"
                        placeholder="0123456789"
                      />
                    </div>
                  </div>
                </div>
              </form>
            </div>

            <div className="org-modal-footer">
              <button
                type="button"
                onClick={handleCloseCreateModal}
                className="org-btn org-btn-secondary"
                disabled={isCreating}
              >
                Hủy bỏ
              </button>
              <button
                type="submit"
                form="createOrgForm"
                className="org-btn org-btn-primary"
                disabled={isCreating}
              >
                {isCreating ? "Đang tạo..." : "Tạo ngay"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── POPUP XÓA / GIẢI TÁN TỔ CHỨC ─── */}
      {orgToDelete &&
        (() => {
          const isPresident = checkIsPresident(orgToDelete);
          const orgNameDisplay =
            orgToDelete.name || orgToDelete.orgName || "tổ chức này";

          return (
            <div
              className="org-modal-overlay"
              onClick={() => setOrgToDelete(null)}
            >
              <div
                className="org-modal"
                onClick={(e) => e.stopPropagation()}
                style={{ maxWidth: "450px" }}
              >
                <div
                  className="org-modal-header"
                  style={{ paddingBottom: "1rem" }}
                >
                  <h3 style={{ color: "#ef4444" }}>
                    {isPresident ? "Giải tán tổ chức" : "Rời tổ chức"}
                  </h3>
                </div>
                <div className="org-modal-body">
                  <p style={{ margin: 0, color: "#475569", lineHeight: "1.5" }}>
                    {isPresident
                      ? `Bạn có chắc chắn muốn giải tán tổ chức "${orgNameDisplay}"? Toàn bộ dữ liệu, thành viên, và các sự kiện liên quan sẽ bị xóa vĩnh viễn. Hành động này không thể hoàn tác.`
                      : `Bạn có chắc chắn muốn rời tổ chức "${orgNameDisplay}"? Hành động này không thể hoàn tác.`}
                  </p>
                </div>
                <div
                  className="org-modal-footer"
                  style={{ borderTop: "none", paddingTop: 0 }}
                >
                  <button
                    type="button"
                    onClick={() => setOrgToDelete(null)}
                    className="org-btn org-btn-secondary"
                    disabled={deletingOrgId === orgToDelete.id}
                  >
                    Hủy bỏ
                  </button>
                  <button
                    type="button"
                    onClick={confirmDeleteOrganization}
                    className="org-btn org-btn-danger"
                    disabled={deletingOrgId === orgToDelete.id}
                  >
                    {deletingOrgId === orgToDelete.id
                      ? "Đang xử lý..."
                      : isPresident
                        ? "Giải tán"
                        : "Rời tổ chức"}
                  </button>
                </div>
              </div>
            </div>
          );
        })()}
    </div>
  );
}

export default UserOrganizationsPage;
