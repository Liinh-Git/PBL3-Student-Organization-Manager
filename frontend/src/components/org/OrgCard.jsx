/**
 * OrgCard.jsx - Organization card component
 * * This component displays an organization summary card using the new clean UI.
 * * Props:
 * - organization: Organization data object (OrganizationDto or OrganizationSummaryDto)
 * - onClick: Callback when card is clicked
 * * Data fields used:
 * - name: Organization name
 * - description: Organization description
 * - avatarUrl: Organization avatar/image
 * - coverUrl: Organization cover image
 * - totalMembers: Number of members
 * - location: Organization location
 * - status: Organization status
 * - foundingDate: Organization founding date (optional)
 */

import "./OrgCard.css";

const DEFAULT_ORG_COVER =
  "data:image/svg+xml;utf8," +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='300' viewBox='0 0 1200 300'>" +
      "<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>" +
      "<stop offset='0%' stop-color='#ecfeff'/><stop offset='100%' stop-color='#cffafe'/>" +
      "</linearGradient></defs>" +
      "<rect width='1200' height='300' fill='url(#g)'/>" +
      "<text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#155e75' font-family='Segoe UI' font-size='34'>Organization Cover</text>" +
      "</svg>",
  );

const DEFAULT_ORG_AVATAR =
  "data:image/svg+xml;utf8," +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='120' height='120' viewBox='0 0 120 120'>" +
      "<circle cx='60' cy='60' r='60' fill='#e2e8f0'/>" +
      "<circle cx='60' cy='45' r='18' fill='#64748b'/>" +
      "<path d='M24 98c6-20 22-30 36-30s30 10 36 30' fill='#64748b'/>" +
      "</svg>",
  );

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  let safeUrl = String(url).trim();
  if (!safeUrl) return "";
  safeUrl = safeUrl.replace(/\\/g, "/");
  safeUrl = safeUrl.replace(/^['"]|['"]$/g, "");

  if (/^https?:\/\//i.test(safeUrl)) return safeUrl;
  if (/^www\./i.test(safeUrl)) return `https://${safeUrl}`;

  const uploadsIndex = safeUrl.toLowerCase().indexOf("/uploads/");
  if (uploadsIndex >= 0) {
    safeUrl = safeUrl.slice(uploadsIndex);
  } else {
    const plainUploadsIndex = safeUrl.toLowerCase().indexOf("uploads/");
    if (plainUploadsIndex >= 0)
      safeUrl = `/${safeUrl.slice(plainUploadsIndex)}`;
  }

  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return safeUrl.startsWith("/")
    ? `${origin}${safeUrl}`
    : `${origin}/${safeUrl}`;
}

function OrgCard({ organization, onClick, onDelete, isDeleting = false }) {
  if (!organization) {
    return null;
  }

  const {
    id,
    name,
    orgName,
    organizationName,
    description,
    avatarUrl,
    AvatarUrl,
    coverUrl,
    CoverUrl,
    totalMembers,
    location,
    status,
    foundingDate,
  } = organization;

  const formatDate = (dateString) => {
    if (!dateString) return null;
    const date = new Date(dateString);
    return date.toLocaleDateString("vi-VN", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  const displayName = name || orgName || organizationName || "Tên tổ chức";
  const displayDate = formatDate(foundingDate);
  const avatarValue =
    avatarUrl ??
    AvatarUrl ??
    organization?.avatarURL ??
    organization?.AvatarURL;
  const coverValue =
    coverUrl ?? CoverUrl ?? organization?.coverURL ?? organization?.CoverURL;
  const avatarSrc = toAbsoluteMediaUrl(avatarValue) || DEFAULT_ORG_AVATAR;
  const coverSrc = toAbsoluteMediaUrl(coverValue) || DEFAULT_ORG_COVER;
  const memberCount = totalMembers ?? 0;

  return (
    <div className="org-card" onClick={() => onClick?.(id)}>
      <div className="org-card-media">
        <img
          className="org-card-cover"
          src={coverSrc}
          alt={`${displayName} cover`}
          onError={(e) => {
            e.currentTarget.onerror = null;
            e.currentTarget.src = DEFAULT_ORG_COVER;
          }}
        />
        <div className="org-card-logo" aria-label={`${displayName} avatar`}>
          <img
            src={avatarSrc}
            alt={`${displayName} avatar`}
            onError={(e) => {
              e.currentTarget.onerror = null;
              e.currentTarget.src = DEFAULT_ORG_AVATAR;
            }}
          />
        </div>
      </div>

      <h3 className="org-card-title">{displayName}</h3>
      <p className="org-card-desc">{description || "Chưa có mô tả"}</p>

      <div className="org-card-stats">
        <div className="org-stat-item">
          <svg
            width="11"
            height="11"
            viewBox="0 0 24 24"
            fill="none"
            stroke="#94a3b8"
            strokeWidth="2.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
          </svg>
          <span className="org-stat-label">Thành viên</span>
          <span className="org-stat-val">{memberCount}</span>
        </div>

        {status && (
          <div className="org-stat-item">
            <svg
              width="11"
              height="11"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#94a3b8"
              strokeWidth="2.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="12" cy="12" r="10" />
              <polyline points="12 6 12 12 16 14" />
            </svg>
            <span className="org-stat-label">Trạng thái</span>
            <span className="org-stat-val">{status}</span>
          </div>
        )}

        {location && (
          <div className="org-stat-item org-stat-item--wide">
            <svg
              width="11"
              height="11"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#94a3b8"
              strokeWidth="2.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
              <circle cx="12" cy="10" r="3" />
            </svg>
            <span className="org-stat-label">Địa điểm</span>
            <span className="org-stat-val small-text">{location}</span>
          </div>
        )}

        <div className="org-stat-item org-stat-item--wide">
          <svg
            width="11"
            height="11"
            viewBox="0 0 24 24"
            fill="none"
            stroke="#94a3b8"
            strokeWidth="2.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <rect x="3" y="4" width="18" height="18" rx="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
          </svg>
          <span className="org-stat-label">Ngày thành lập</span>
          <span className="org-stat-val small-text">{displayDate || "—"}</span>
        </div>
      </div>

      <div className="org-card-action-row">
        <div className="org-card-action">
          Xem chi tiết
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            style={{ marginLeft: "4px" }}
          >
            <line x1="5" y1="12" x2="19" y2="12"></line>
            <polyline points="12 5 19 12 12 19"></polyline>
          </svg>
        </div>

        {onDelete && (
          <button
            type="button"
            className="org-delete-btn"
            onClick={(e) => {
              e.stopPropagation();
              onDelete(id, name);
            }}
            disabled={isDeleting}
          >
            {isDeleting ? "Đang xóa..." : "Xóa"}
          </button>
        )}
      </div>
    </div>
  );
}

export default OrgCard;
