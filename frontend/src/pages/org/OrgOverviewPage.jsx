import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  updateOrganization,
  uploadOrganizationImage,
} from "../../services/organizationService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgOverviewPage.css";

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  if (url.startsWith("/")) return `${origin}${url}`;
  return `${origin}/${url}`;
}

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

  const [isEditMode, setIsEditMode] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isUploadingAvatar, setIsUploadingAvatar] = useState(false);
  const [isUploadingCover, setIsUploadingCover] = useState(false);
  const [formState, setFormState] = useState({
    orgName: "",
    description: "",
    location: "",
    contactEmail: "",
    contactPhone: "",
    foundingDate: "",
  });
  const [avatarPreviewUrl, setAvatarPreviewUrl] = useState(null);
  const [coverPreviewUrl, setCoverPreviewUrl] = useState(null);
  const [imageUrls, setImageUrls] = useState({ avatarUrl: "", coverUrl: "" });

  useEffect(() => {
    if (orgId && (!contextOrg || String(contextOrg.id) !== String(orgId))) {
      loadWorkspaceOrg(orgId);
    }
  }, [orgId, contextOrg, loadWorkspaceOrg]);

  useEffect(() => {
    if (!contextOrg) return;
    setFormState({
      orgName: contextOrg.name || "",
      description: contextOrg.description || "",
      location: contextOrg.location || "",
      contactEmail: contextOrg.contactEmail || "",
      contactPhone: contextOrg.contactPhone || "",
      foundingDate: contextOrg.foundingDate
        ? new Date(contextOrg.foundingDate).toISOString().slice(0, 10)
        : "",
    });
    setImageUrls({
      avatarUrl: contextOrg.avatarUrl || "",
      coverUrl: contextOrg.coverUrl || "",
    });
  }, [contextOrg]);

  useEffect(() => {
    return () => {
      if (avatarPreviewUrl) URL.revokeObjectURL(avatarPreviewUrl);
      if (coverPreviewUrl) URL.revokeObjectURL(coverPreviewUrl);
    };
  }, [avatarPreviewUrl, coverPreviewUrl]);

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (contextLoading) {
    return <LoadingSpinner message="Loading organization data..." />;
  }

  if (!isMember) {
    return (
      <ForbiddenState message="You are not a member of this organization" />
    );
  }

  const canEdit = permissions.includes("org.overview.write");

  const handleFormChange = (e) => {
    const { name, value } = e.target;
    setFormState((prev) => ({ ...prev, [name]: value }));
  };

  const resetForm = () => {
    if (!contextOrg) return;
    setFormState({
      orgName: contextOrg.name || "",
      description: contextOrg.description || "",
      location: contextOrg.location || "",
      contactEmail: contextOrg.contactEmail || "",
      contactPhone: contextOrg.contactPhone || "",
      foundingDate: contextOrg.foundingDate
        ? new Date(contextOrg.foundingDate).toISOString().slice(0, 10)
        : "",
    });
    if (avatarPreviewUrl) URL.revokeObjectURL(avatarPreviewUrl);
    if (coverPreviewUrl) URL.revokeObjectURL(coverPreviewUrl);
    setAvatarPreviewUrl(null);
    setCoverPreviewUrl(null);
    setImageUrls({
      avatarUrl: contextOrg.avatarUrl || "",
      coverUrl: contextOrg.coverUrl || "",
    });
  };

  const handleCancel = () => {
    resetForm();
    setIsEditMode(false);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    if (!canEdit || !orgId) return;

    const payload = {
      orgName: formState.orgName,
      description: formState.description || undefined,
      location: formState.location || undefined,
      contactEmail: formState.contactEmail || undefined,
      contactPhone: formState.contactPhone || undefined,
      foundingDate: formState.foundingDate
        ? new Date(formState.foundingDate).toISOString()
        : undefined,
      avatarUrl: imageUrls.avatarUrl || undefined,
      coverUrl: imageUrls.coverUrl || undefined,
    };

    setIsSubmitting(true);
    try {
      await updateOrganization(orgId, payload);
      await loadWorkspaceOrg(orgId);
      setIsEditMode(false);
    } catch (err) {
      alert(err.message || "Failed to update organization");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUploadImage = async (event, type) => {
    if (!canEdit || !orgId) return;
    const file = event.target.files?.[0];
    if (!file) return;

    if (type === "avatar") {
      if (avatarPreviewUrl) URL.revokeObjectURL(avatarPreviewUrl);
      setAvatarPreviewUrl(URL.createObjectURL(file));
      setIsUploadingAvatar(true);
    } else {
      if (coverPreviewUrl) URL.revokeObjectURL(coverPreviewUrl);
      setCoverPreviewUrl(URL.createObjectURL(file));
      setIsUploadingCover(true);
    }

    try {
      const updatedOrg = await uploadOrganizationImage(orgId, file, type);
      setImageUrls((prev) => ({
        avatarUrl: updatedOrg?.avatarUrl || prev.avatarUrl || "",
        coverUrl: updatedOrg?.coverUrl || prev.coverUrl || "",
      }));
      await loadWorkspaceOrg(orgId);
      if (type === "avatar") setAvatarPreviewUrl(null);
      if (type === "cover") setCoverPreviewUrl(null);
    } catch (err) {
      alert(err.message || "Failed to upload image");
      if (type === "avatar") {
        if (avatarPreviewUrl) URL.revokeObjectURL(avatarPreviewUrl);
        setAvatarPreviewUrl(null);
      } else {
        if (coverPreviewUrl) URL.revokeObjectURL(coverPreviewUrl);
        setCoverPreviewUrl(null);
      }
    } finally {
      if (type === "avatar") setIsUploadingAvatar(false);
      if (type === "cover") setIsUploadingCover(false);
      event.target.value = "";
    }
  };

  const displayFoundingDate = contextOrg?.foundingDate
    ? new Date(contextOrg.foundingDate).toLocaleDateString("vi-VN")
    : "Not set";

  const avatarSrc = avatarPreviewUrl || toAbsoluteMediaUrl(imageUrls.avatarUrl);
  const coverSrc = coverPreviewUrl || toAbsoluteMediaUrl(imageUrls.coverUrl);
  const initial = (contextOrg?.name || "O").charAt(0).toUpperCase();

  const titleValue = isEditMode
    ? formState.orgName
    : contextOrg?.name || "Organization";
  const descriptionValue = isEditMode
    ? formState.description
    : contextOrg?.description || "Organization has no description yet.";
  const locationValue = isEditMode
    ? formState.location
    : contextOrg?.location || "Not specified";
  const contactEmailValue = isEditMode
    ? formState.contactEmail
    : contextOrg?.contactEmail || "-";
  const contactPhoneValue = isEditMode
    ? formState.contactPhone
    : contextOrg?.contactPhone || "-";

  return (
    <div className="org-overview-container">
      <div className="org-banner-section">
        {coverSrc ? (
          <img
            className="org-banner-image"
            src={coverSrc}
            alt="Organization cover"
          />
        ) : null}
        {isEditMode && canEdit ? (
          <label className="org-image-edit org-image-edit--cover">
            {isUploadingCover ? "Uploading..." : "Edit cover/banner"}
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={(e) => handleUploadImage(e, "cover")}
            />
          </label>
        ) : null}
      </div>

      <div className="org-profile-nav">
        <div className="org-avatar-wrapper">
          <div className="org-avatar-frame">
            {avatarSrc ? (
              <img
                className="org-avatar-image"
                src={avatarSrc}
                alt="Organization avatar"
              />
            ) : (
              <span className="org-avatar-fallback">{initial}</span>
            )}
          </div>
          {isEditMode && canEdit ? (
            <label className="org-image-edit org-image-edit--avatar">
              {isUploadingAvatar ? "Uploading..." : "Edit avatar"}
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp"
                onChange={(e) => handleUploadImage(e, "avatar")}
              />
            </label>
          ) : null}
        </div>

        <div className="org-title-block">
          {isEditMode ? (
            <input
              type="text"
              name="orgName"
              className="org-inline-input org-inline-input--title"
              value={formState.orgName}
              onChange={handleFormChange}
              required
            />
          ) : (
            <h1>{titleValue}</h1>
          )}
          <p>Organization overview and official contact information.</p>
        </div>

        {canEdit ? (
          <div className="org-header-actions">
            {!isEditMode ? (
              <button
                onClick={() => setIsEditMode(true)}
                className="org-btn-header"
              >
                Edit
              </button>
            ) : (
              <>
                <button
                  onClick={handleCancel}
                  className="org-btn org-btn-secondary"
                  disabled={isSubmitting}
                >
                  Cancel
                </button>
                <button
                  onClick={handleSave}
                  className="org-btn org-btn-primary"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? "Saving..." : "Save"}
                </button>
              </>
            )}
          </div>
        ) : null}
      </div>

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
            <span className="stat-label-text">Location</span>
            {isEditMode ? (
              <input
                type="text"
                name="location"
                className="org-inline-input"
                value={formState.location}
                onChange={handleFormChange}
              />
            ) : (
              <p className="stat-value-text">{locationValue}</p>
            )}
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
            <span className="stat-label-text">Members</span>
            <p className="stat-value-text">
              {contextOrg?.totalMembers || 0} people
            </p>
          </div>
        </div>

        <div className="stat-item-card stat-item-card--founding">
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
            <span className="stat-label-text">Founding date</span>
            {isEditMode ? (
              <input
                type="date"
                name="foundingDate"
                className="org-inline-input"
                value={formState.foundingDate}
                onChange={handleFormChange}
              />
            ) : (
              <div className="stat-founding-badge">
                <svg
                  width="12"
                  height="12"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <rect x="3" y="4" width="18" height="18" rx="2" />
                  <line x1="16" y1="2" x2="16" y2="6" />
                  <line x1="8" y1="2" x2="8" y2="6" />
                  <line x1="3" y1="10" x2="21" y2="10" />
                </svg>
                {displayFoundingDate}
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="org-main-layout">
        <div className="layout-left">
          <h2 className="content-header">About</h2>
          <div className="info-text-card">
            {isEditMode ? (
              <textarea
                name="description"
                className="org-inline-textarea"
                value={formState.description}
                onChange={handleFormChange}
                placeholder="Describe organization goals and activities..."
              />
            ) : (
              descriptionValue
            )}
          </div>
        </div>

        <div className="layout-right">
          <h2 className="content-header">Contact</h2>
          <div className="contact-info-list">
            <div className="contact-row">
              <span className="org-form-label-small">Contact email</span>
              {isEditMode ? (
                <input
                  type="email"
                  name="contactEmail"
                  className="org-inline-input"
                  value={formState.contactEmail}
                  onChange={handleFormChange}
                />
              ) : (
                <p className="contact-val-text">{contactEmailValue}</p>
              )}
            </div>
            <div className="contact-row">
              <span className="org-form-label-small">Phone</span>
              {isEditMode ? (
                <input
                  type="tel"
                  name="contactPhone"
                  className="org-inline-input"
                  value={formState.contactPhone}
                  onChange={handleFormChange}
                />
              ) : (
                <p className="contact-val-text">{contactPhoneValue}</p>
              )}
            </div>
            <div className="contact-row">
              <span className="org-form-label-small">Created at</span>
              <p className="contact-val-text">
                {contextOrg?.createdAtUtc
                  ? new Date(contextOrg.createdAtUtc).toLocaleDateString(
                      "vi-VN",
                    )
                  : "-"}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default OrgOverviewPage;
