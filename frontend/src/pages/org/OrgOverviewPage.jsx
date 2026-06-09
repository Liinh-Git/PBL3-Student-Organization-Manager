import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  getPublicOverview,
  updateOrganization,
  uploadOrganizationImage,
} from "../../services/organizationService.js";
import {
  createOrganizationRequest,
  getMyPendingJoinRequests,
  withdrawOrganizationJoinRequest,
} from "../../services/requestService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
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
    orgId: loadedOrgId,
  } = useOrgContext();

  const [isEditMode, setIsEditMode] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isUploadingAvatar, setIsUploadingAvatar] = useState(false);
  const [isUploadingCover, setIsUploadingCover] = useState(false);
  const [publicOrg, setPublicOrg] = useState(null);
  const [publicLoading, setPublicLoading] = useState(false);
  const [publicError, setPublicError] = useState(null);
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

  // Join/pending/leave state — used for non-members and regular members
  const [joinStatus, setJoinStatus] = useState("none"); // "none" | "pending" | "member"
  const [isJoinWorking, setIsJoinWorking] = useState(false);
  const [joinFeedback, setJoinFeedback] = useState(null); // {type:"success"|"error", text}
  const [pendingChecked, setPendingChecked] = useState(false);

  useEffect(() => {
    if (
      orgId &&
      String(loadedOrgId || "") !== String(orgId) &&
      (!contextOrg || String(contextOrg.id) !== String(orgId))
    ) {
      loadWorkspaceOrg(orgId);
    }
  }, [orgId, loadedOrgId, contextOrg, loadWorkspaceOrg]);

  useEffect(() => {
    setPendingChecked(false);
    setJoinStatus("none");
    setJoinFeedback(null);
  }, [orgId]);

  useEffect(() => {
    if (!orgId) return;

    let cancelled = false;
    setPublicLoading(true);
    setPublicError(null);
    setPublicOrg(null);

    getPublicOverview(orgId)
      .then((data) => {
        if (!cancelled) setPublicOrg(data);
      })
      .catch((err) => {
        if (!cancelled) setPublicError(err);
      })
      .finally(() => {
        if (!cancelled) setPublicLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [orgId]);

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

  // Sync joinStatus when isMember is known
  useEffect(() => {
    if (isMember) setJoinStatus("member");
  }, [isMember]);

  // Check pending status for non-members
  useEffect(() => {
    if (!orgId || isMember || pendingChecked || contextLoading) return;
    let cancelled = false;
    getMyPendingJoinRequests()
      .then((list) => {
        if (cancelled) return;
        const has = (list || []).some(
          (r) => String(r.organizationId) === String(orgId),
        );
        setJoinStatus(has ? "pending" : "none");
        setPendingChecked(true);
      })
      .catch(() => {
        if (!cancelled) setPendingChecked(true);
      });
    return () => {
      cancelled = true;
    };
  }, [orgId, isMember, pendingChecked, contextLoading]);

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  if (contextLoading || (publicLoading && !publicOrg)) {
    return <LoadingSpinner message="Đang tải dữ liệu tổ chức..." />;
  }

  if (publicError && !publicOrg && !contextOrg) {
    return (
      <ErrorState
        message={publicError.message || "Không tải được thông tin tổ chức"}
      />
    );
  }

  if (!isMember) {
    // ── PUBLIC VIEW for non-members ──────────────────────────────────────────
    const pub = publicOrg || contextOrg || {};
    const pubName = pub.name || "Tổ chức";
    const pubInitial = pubName.charAt(0).toUpperCase();
    const pubAvatar = toAbsoluteMediaUrl(pub.avatarUrl || "");
    const pubCover = toAbsoluteMediaUrl(pub.coverUrl || "");
    const pubDate = pub.foundingDate
      ? new Date(pub.foundingDate).toLocaleDateString("vi-VN")
      : null;

    const handleJoin = async () => {
      setIsJoinWorking(true);
      setJoinFeedback(null);
      try {
        await createOrganizationRequest(orgId, {
          requestType: "JoinOrganization",
          content: "I would like to join this organization",
        });
        setJoinStatus("pending");
        setJoinFeedback({
          type: "success",
          text: "Đã gửi yêu cầu. Vui lòng chờ ban quản trị duyệt.",
        });
      } catch (err) {
        const msg = (err?.message || "").toLowerCase();
        if (
          msg.includes("already") ||
          msg.includes("pending") ||
          msg.includes("existed")
        ) {
          setJoinStatus("pending");
          setJoinFeedback({
            type: "info",
            text: "Yêu cầu của bạn đang chờ duyệt.",
          });
        } else {
          setJoinFeedback({
            type: "error",
            text: err.message || "Không gửi được yêu cầu",
          });
        }
      } finally {
        setIsJoinWorking(false);
      }
    };

    const handleWithdraw = async () => {
      setIsJoinWorking(true);
      setJoinFeedback(null);
      try {
        await withdrawOrganizationJoinRequest(orgId);
        setJoinStatus("none");
        setJoinFeedback({ type: "success", text: "Đã rút đơn tham gia." });
      } catch (err) {
        setJoinFeedback({
          type: "error",
          text: err.message || "Không rút được đơn",
        });
      } finally {
        setIsJoinWorking(false);
      }
    };

    return (
      <div className="org-overview-container">
        {/* Banner */}
        <div className="org-banner-section">
          {pubCover ? (
            <img className="org-banner-image" src={pubCover} alt="Ảnh bìa" />
          ) : null}
        </div>

        {/* Profile row */}
        <div className="org-profile-nav">
          <div className="org-avatar-wrapper">
            <div className="org-avatar-frame">
              {pubAvatar ? (
                <img
                  className="org-avatar-image"
                  src={pubAvatar}
                  alt={pubName}
                />
              ) : (
                <span className="org-avatar-fallback">{pubInitial}</span>
              )}
            </div>
          </div>

          <div className="org-title-block">
            <h1>{pubName}</h1>
            <p>
              {pub.description
                ? pub.description.slice(0, 80) +
                  (pub.description.length > 80 ? "…" : "")
                : "Tổ chức sinh viên"}
            </p>
          </div>

          {/* Join / Withdraw button in header */}
          <div className="org-header-actions">
            {joinStatus === "pending" ? (
              <button
                className="org-membership-btn org-membership-btn--pending"
                disabled={isJoinWorking}
                onClick={handleWithdraw}
              >
                <svg
                  width="15"
                  height="15"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <circle cx="12" cy="12" r="10" />
                  <line x1="15" y1="9" x2="9" y2="15" />
                  <line x1="9" y1="9" x2="15" y2="15" />
                </svg>
                {isJoinWorking ? "Đang xử lý…" : "Rút đơn tham gia"}
              </button>
            ) : (
              <button
                className="org-membership-btn org-membership-btn--join"
                disabled={isJoinWorking}
                onClick={handleJoin}
              >
                <svg
                  width="15"
                  height="15"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
                  <circle cx="9" cy="7" r="4" />
                  <line x1="19" y1="8" x2="19" y2="14" />
                  <line x1="22" y1="11" x2="16" y2="11" />
                </svg>
                {isJoinWorking ? "Đang xử lý…" : "Tham gia CLB"}
              </button>
            )}
          </div>
        </div>

        {/* Feedback toast */}
        {joinFeedback && (
          <div
            className={`org-join-feedback org-join-feedback--${joinFeedback.type}`}
          >
            {joinFeedback.type === "success" && (
              <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <polyline points="20 6 9 17 4 12" />
              </svg>
            )}
            {joinFeedback.type === "pending" || joinFeedback.type === "info" ? (
              <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <circle cx="12" cy="12" r="10" />
                <polyline points="12 6 12 12 16 14" />
              </svg>
            ) : null}
            {joinFeedback.type === "error" && (
              <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <circle cx="12" cy="12" r="10" />
                <line x1="15" y1="9" x2="9" y2="15" />
                <line x1="9" y1="9" x2="15" y2="15" />
              </svg>
            )}
            <span>{joinFeedback.text}</span>
            <button
              className="org-join-feedback__close"
              onClick={() => setJoinFeedback(null)}
            >
              ×
            </button>
          </div>
        )}

        {/* Stats */}
        <div className="org-stats-dashboard">
          {pub.location && (
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
                <span className="stat-label-text">Địa điểm</span>
                <p className="stat-value-text">{pub.location}</p>
              </div>
            </div>
          )}
          {pub.totalMembers != null && (
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
                <span className="stat-label-text">Tổng thành viên</span>
                <p className="stat-value-text">{pub.totalMembers} người</p>
              </div>
            </div>
          )}
          {pubDate && (
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
                <span className="stat-label-text">Ngày thành lập</span>
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
                  {pubDate}
                </div>
              </div>
            </div>
          )}
          {joinStatus === "pending" && (
            <div className="stat-item-card stat-item-card--pending-status">
              <div className="stat-icon-circle stat-icon-circle--amber">
                <svg
                  width="20"
                  height="20"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                >
                  <circle cx="12" cy="12" r="10" />
                  <polyline points="12 6 12 12 16 14" />
                </svg>
              </div>
              <div>
                <span className="stat-label-text">Trạng thái</span>
                <p className="stat-value-text">Đang chờ duyệt</p>
              </div>
            </div>
          )}
        </div>

        {/* Main layout */}
        <div className="org-main-layout">
          <div className="layout-left">
            <h2 className="content-header">Giới thiệu</h2>
            <div className="info-text-card">
              {pub.description || "Tổ chức chưa có mô tả."}
            </div>
          </div>
          <div className="layout-right">
            {/* CTA card */}
            <div className="org-join-cta-card">
              <div className="org-join-cta-card__icon">
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
                  <path d="M17 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
                  <circle cx="9" cy="7" r="4" />
                  <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
                  <path d="M16 3.13a4 4 0 0 1 0 7.75" />
                </svg>
              </div>
              <strong>Tham gia cùng chúng tôi</strong>
              <p>
                Đăng ký thành viên để nhận thông báo về những hoạt động mới nhất
                của tổ chức.
              </p>
              {joinStatus === "pending" ? (
                <button
                  className="org-membership-btn org-membership-btn--pending"
                  disabled={isJoinWorking}
                  onClick={handleWithdraw}
                >
                  <svg
                    width="15"
                    height="15"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  >
                    <circle cx="12" cy="12" r="10" />
                    <line x1="15" y1="9" x2="9" y2="15" />
                    <line x1="9" y1="9" x2="15" y2="15" />
                  </svg>
                  {isJoinWorking ? "Đang xử lý…" : "Rút đơn tham gia"}
                </button>
              ) : (
                <button
                  className="org-membership-btn org-membership-btn--join"
                  disabled={isJoinWorking}
                  onClick={handleJoin}
                >
                  <svg
                    width="15"
                    height="15"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  >
                    <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
                    <circle cx="9" cy="7" r="4" />
                    <line x1="19" y1="8" x2="19" y2="14" />
                    <line x1="22" y1="11" x2="16" y2="11" />
                  </svg>
                  {isJoinWorking ? "Đang xử lý…" : "Tham gia CLB"}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
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
      alert(err.message || "Không thể cập nhật tổ chức");
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
      alert(err.message || "Không thể tải ảnh lên");
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
    : contextOrg?.name || "Tổ chức";
  const descriptionValue = isEditMode
    ? formState.description
    : contextOrg?.description || "Tổ chức chưa có mô tả.";
  const locationValue = isEditMode
    ? formState.location
    : contextOrg?.location || "Chưa cập nhật";
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
            alt="Ảnh bìa tổ chức"
          />
        ) : null}
        {isEditMode && canEdit ? (
          <label className="org-image-edit org-image-edit--cover">
            {isUploadingCover ? "Đang tải..." : "Sửa ảnh bìa/banner"}
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
                alt="Ảnh đại diện tổ chức"
              />
            ) : (
              <span className="org-avatar-fallback">{initial}</span>
            )}
          </div>
          {isEditMode && canEdit ? (
            <label className="org-image-edit org-image-edit--avatar">
              {isUploadingAvatar ? "Đang tải..." : "Sửa ảnh đại diện"}
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
          <p>Tổng quan tổ chức và thông tin liên hệ chính thức.</p>
        </div>

        {canEdit ? (
          <div className="org-header-actions">
            {!isEditMode ? (
              <button
                onClick={() => setIsEditMode(true)}
                className="org-btn-header"
              >
                Sửa
              </button>
            ) : (
              <>
                <button
                  onClick={handleCancel}
                  className="org-btn org-btn-secondary"
                  disabled={isSubmitting}
                >
                  Hủy
                </button>
                <button
                  onClick={handleSave}
                  className="org-btn org-btn-primary"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? "Đang lưu..." : "Lưu"}
                </button>
              </>
            )}
          </div>
        ) : (
          <div className="org-header-actions">
            <button
              className="org-membership-btn org-membership-btn--leave"
              disabled={isJoinWorking}
              onClick={async () => {
                if (
                  !window.confirm(
                    "Bạn có chắc muốn rời khỏi tổ chức này không?",
                  )
                )
                  return;
                setIsJoinWorking(true);
                try {
                  const svc =
                    await import("../../services/memberService.js").catch(
                      () => null,
                    );
                  if (svc?.leaveOrganization)
                    await svc.leaveOrganization(orgId);
                  else if (svc?.removeMember)
                    await svc.removeMember(orgId, "me");
                  await loadWorkspaceOrg(orgId);
                } catch (err) {
                  alert(err.message || "Không rời được tổ chức");
                } finally {
                  setIsJoinWorking(false);
                }
              }}
            >
              <svg
                width="15"
                height="15"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                <polyline points="16 17 21 12 16 7" />
                <line x1="21" y1="12" x2="9" y2="12" />
              </svg>
              {isJoinWorking ? "Đang xử lý…" : "Rời CLB"}
            </button>
          </div>
        )}
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
            <span className="stat-label-text">Địa điểm</span>
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
            <span className="stat-label-text">Thành viên</span>
            <p className="stat-value-text">
              {contextOrg?.totalMembers || 0} người
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
              <span className="org-form-label-small">Ngày tạo</span>
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
