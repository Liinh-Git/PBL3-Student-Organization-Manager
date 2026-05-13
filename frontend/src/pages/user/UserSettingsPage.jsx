/**
 * UserSettingsPage.jsx
 * * Phase 4B: Merged User Profile and Settings Page with Modern Tab UI
 * Connected to: getMe, updateMe, changePassword
 * * NOTE: Cấu trúc logic, biến và API call được giữ nguyên gốc 100%.
 */

import { useState, useEffect } from "react";
import { getMe, updateMe, changePassword } from "../../services/userService.js";
import PageHeader from "../../components/shared/PageHeader";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import "./UserSettingsPage.css"; // File CSS đi kèm tạo ở dưới

function UserSettingsPage() {
  // --- TAB STATE ---
  const [activeTab, setActiveTab] = useState("profile");

  // --- PROFILE STATE ---
  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    phoneNumber: "",
    address: "",
    bio: "",
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmittingProfile, setIsSubmittingProfile] = useState(false);

  // --- PASSWORD STATE ---
  const [isSubmittingPassword, setIsSubmittingPassword] = useState(false);

  // --- PROFILE LOGIC ---
  useEffect(() => {
    async function loadProfile() {
      setIsLoading(true);
      try {
        const data = await getMe();
        setFormData({
          fullName: data?.fullName || "",
          email: data?.email || "",
          phoneNumber: data?.phoneNumber || "",
          address: data?.address || "",
          bio: data?.bio || "",
        });
      } catch (err) {
        setError(err.message || "Failed to load profile");
      } finally {
        setIsLoading(false);
      }
    }
    loadProfile();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    setIsSubmittingProfile(true);
    try {
      const updated = await updateMe({
        fullName: formData.fullName || undefined,
        phoneNumber: formData.phoneNumber || undefined,
        address: formData.address || undefined,
        bio: formData.bio || undefined,
      });
      setFormData((prev) => ({
        ...prev,
        fullName: updated?.fullName || prev.fullName,
        email: updated?.email || prev.email,
        phoneNumber: updated?.phoneNumber || prev.phoneNumber,
        address: updated?.address || prev.address,
        bio: updated?.bio || prev.bio,
      }));
      alert("Profile updated successfully");
    } catch (err) {
      alert(err.message || "Failed to update profile");
    } finally {
      setIsSubmittingProfile(false);
    }
  };

  // --- PASSWORD LOGIC ---
  const handlePasswordChange = async (e) => {
    e.preventDefault();
    const form = e.target;
    const currentPassword = form.currentPassword.value;
    const newPassword = form.newPassword.value;
    const confirmPassword = form.confirmPassword.value;

    if (!currentPassword || !newPassword) {
      alert("Current password and new password are required");
      return;
    }

    if (newPassword !== confirmPassword) {
      alert("New password and confirm password do not match");
      return;
    }

    setIsSubmittingPassword(true);
    try {
      await changePassword({
        currentPassword,
        newPassword,
      });
      alert("Password changed successfully");
      form.reset();
    } catch (err) {
      alert(err.message || "Failed to change password");
    } finally {
      setIsSubmittingPassword(false);
    }
  };

  // --- RENDER LOADERS ---
  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Cài đặt tài khoản"
          description="Quản lý hồ sơ và bảo mật của bạn"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Cài đặt tài khoản"
          description="Quản lý hồ sơ và bảo mật của bạn"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  // --- MAIN RENDER ---
  return (
    <div className="app-page settings-modern-page">
      <PageHeader
        title="Cài đặt tài khoản"
        description="Quản lý thông tin hồ sơ và bảo mật của bạn."
      />

      <div className="settings-wrapper">
        {/* TAB NAVIGATION */}
        <div className="pill-tabs">
          <button
            className={`pill-btn ${activeTab === "profile" ? "active" : ""}`}
            onClick={() => setActiveTab("profile")}
          >
            Hồ sơ cá nhân
          </button>
          <button
            className={`pill-btn ${activeTab === "security" ? "active" : ""}`}
            onClick={() => setActiveTab("security")}
          >
            Đổi mật khẩu
          </button>
        </div>

        {/* TAB CONTENT */}
        <div className="settings-content-box">
          {/* TAB 1: PROFILE */}
          {activeTab === "profile" && (
            <div className="tab-pane animate-fade">
              <div className="pane-header">
                <h3 className="app-section-title">Thông tin cá nhân</h3>
              </div>

              <form
                onSubmit={handleProfileUpdate}
                className="auth-form modern-grid-form"
              >
                <div className="form-group">
                  <label className="form-label">Name</label>
                  <input
                    type="text"
                    id="fullName"
                    name="fullName"
                    value={formData.fullName}
                    placeholder="Full name"
                    onChange={handleChange}
                    className="form-input"
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Email</label>
                  <input
                    type="email"
                    id="email"
                    name="email"
                    value={formData.email}
                    placeholder="Email"
                    disabled
                    className="form-input readonly"
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Phone</label>
                  <input
                    type="text"
                    id="phoneNumber"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    placeholder="Phone number"
                    onChange={handleChange}
                    className="form-input"
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Address</label>
                  <input
                    type="text"
                    id="address"
                    name="address"
                    value={formData.address}
                    placeholder="Address"
                    onChange={handleChange}
                    className="form-input"
                  />
                </div>

                <div className="form-group full-width">
                  <label className="form-label">Bio</label>
                  <textarea
                    id="bio"
                    name="bio"
                    value={formData.bio}
                    placeholder="Bio"
                    onChange={handleChange}
                    className="form-textarea"
                    rows="3"
                  />
                </div>

                <div className="app-action-row full-width flex-end">
                  <button
                    type="submit"
                    disabled={isSubmittingProfile}
                    className="app-button app-button--primary btn-orange"
                  >
                    {isSubmittingProfile ? "Updating..." : "Lưu thay đổi"}
                  </button>
                </div>
              </form>
            </div>
          )}

          {/* TAB 2: PASSWORD */}
          {activeTab === "security" && (
            <div className="tab-pane animate-fade">
              <div className="pane-header">
                <h3 className="app-section-title">Thay đổi mật khẩu</h3>
              </div>

              <form
                onSubmit={handlePasswordChange}
                className="auth-form modern-grid-form"
              >
                <div className="form-group full-width max-w-half">
                  <label className="form-label">Current Password</label>
                  <input
                    type="password"
                    id="currentPassword"
                    name="currentPassword"
                    placeholder="Current password"
                    required
                    className="form-input"
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">New Password</label>
                  <input
                    type="password"
                    id="newPassword"
                    name="newPassword"
                    placeholder="New password"
                    required
                    className="form-input"
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Confirm New Password</label>
                  <input
                    type="password"
                    id="confirmPassword"
                    name="confirmPassword"
                    placeholder="Confirm new password"
                    required
                    className="form-input"
                  />
                </div>

                <div className="app-action-row full-width flex-end">
                  <button
                    type="submit"
                    disabled={isSubmittingPassword}
                    className="app-button app-button--primary btn-orange"
                  >
                    {isSubmittingPassword ? "Changing..." : "Cập nhật mật khẩu"}
                  </button>
                </div>
              </form>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default UserSettingsPage;
