/**
 * UserOrganizationsPage.jsx - User's organizations page
 * * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getMyOrganizations } from "../../services/userService.js";
import { createOrganization } from "../../services/organizationService.js";
import OrgCard from "../../components/org/OrgCard";
import "./UserOrganizationsPage.css";

function UserOrganizationsPage() {
  const navigate = useNavigate();

  const [organizations, setOrganizations] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [createError, setCreateError] = useState(null);

  const [formData, setFormData] = useState({
    orgName: "",
    description: "",
    avatarUrl: "",
    coverUrl: "",
    foundingDate: "",
    location: "",
    contactEmail: "",
    contactPhone: "",
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

  const handleCreateOrganization = async (e) => {
    e.preventDefault();
    setIsCreating(true);
    setCreateError(null);

    try {
      // Build payload with only non-empty fields
      const payload = {};
      if (formData.orgName) payload.orgName = formData.orgName;
      if (formData.description) payload.description = formData.description;
      if (formData.avatarUrl) payload.avatarUrl = formData.avatarUrl;
      if (formData.coverUrl) payload.coverUrl = formData.coverUrl;
      if (formData.foundingDate) payload.foundingDate = formData.foundingDate;
      if (formData.location) payload.location = formData.location;
      if (formData.contactEmail) payload.contactEmail = formData.contactEmail;
      if (formData.contactPhone) payload.contactPhone = formData.contactPhone;

      await createOrganization(payload);

      // Refresh organizations list
      const updatedOrgs = await getMyOrganizations();
      setOrganizations(updatedOrgs);

      // Close modal and reset form
      setShowCreateModal(false);
      setFormData({
        orgName: "",
        description: "",
        avatarUrl: "",
        coverUrl: "",
        foundingDate: "",
        location: "",
        contactEmail: "",
        contactPhone: "",
      });
    } catch (err) {
      setCreateError(err.message || "Tạo tổ chức thất bại");
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
          <p>Quản lý và tham gia kiến tạo cộng đồng</p>
        </div>
      </div>

      {isLoading ? (
        <div className="org-loading">Đang tải danh sách tổ chức...</div>
      ) : error ? (
        <div className="org-alert-error">{error}</div>
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
              onClick={() => setShowCreateModal(true)}
            >
              <div className="org-create-icon">+</div>
              <h3>Tạo tổ chức mới</h3>
              <p>
                Bắt đầu hành trình kiến tạo một cộng đồng sinh viên mới của
                riêng bạn.
              </p>
            </button>

            {/* Organizations List */}
            {organizations.map((org) => (
              <OrgCard
                key={org.id}
                organization={org}
                onClick={handleOrgClick}
              />
            ))}
          </div>
        </div>
      )}

      {/* Create Organization Modal */}
      {showCreateModal && (
        <div
          className="org-modal-overlay"
          onClick={() => setShowCreateModal(false)}
        >
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
                    <label htmlFor="avatarUrl" className="org-form-label">
                      Link Ảnh đại diện
                    </label>
                    <input
                      type="url"
                      id="avatarUrl"
                      name="avatarUrl"
                      value={formData.avatarUrl}
                      onChange={handleInputChange}
                      className="org-input"
                      placeholder="https://example.com/avatar.png"
                    />
                  </div>

                  <div className="org-form-group">
                    <label htmlFor="coverUrl" className="org-form-label">
                      Link Ảnh bìa
                    </label>
                    <input
                      type="url"
                      id="coverUrl"
                      name="coverUrl"
                      value={formData.coverUrl}
                      onChange={handleInputChange}
                      className="org-input"
                      placeholder="https://example.com/cover.png"
                    />
                  </div>

                  <div className="org-form-group">
                    <label htmlFor="foundingDate" className="org-form-label">
                      Ngày thành lập
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
                        SĐT liên hệ
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
                onClick={() => setShowCreateModal(false)}
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
    </div>
  );
}

export default UserOrganizationsPage;
