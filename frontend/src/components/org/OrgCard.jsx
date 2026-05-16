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

import './OrgCard.css';

const DEFAULT_ORG_COVER =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='300' viewBox='0 0 1200 300'>" +
      "<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>" +
      "<stop offset='0%' stop-color='#ecfeff'/><stop offset='100%' stop-color='#cffafe'/>" +
      "</linearGradient></defs>" +
      "<rect width='1200' height='300' fill='url(#g)'/>" +
      "<text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#155e75' font-family='Arial' font-size='34'>Organization Cover</text>" +
    "</svg>"
  );

const DEFAULT_ORG_AVATAR =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='120' height='120' viewBox='0 0 120 120'>" +
      "<circle cx='60' cy='60' r='60' fill='#e2e8f0'/>" +
      "<circle cx='60' cy='45' r='18' fill='#64748b'/>" +
      "<path d='M24 98c6-20 22-30 36-30s30 10 36 30' fill='#64748b'/>" +
    "</svg>"
  );

function toAbsoluteMediaUrl(url) {
  if (!url) return '';
  let safeUrl = String(url).trim();
  if (!safeUrl) return '';
  safeUrl = safeUrl.replace(/\\/g, '/');
  safeUrl = safeUrl.replace(/^['"]|['"]$/g, '');

  if (/^https?:\/\//i.test(safeUrl)) return safeUrl;
  if (/^www\./i.test(safeUrl)) return `https://${safeUrl}`;

  const uploadsIndex = safeUrl.toLowerCase().indexOf('/uploads/');
  if (uploadsIndex >= 0) {
    safeUrl = safeUrl.slice(uploadsIndex);
  } else {
    const plainUploadsIndex = safeUrl.toLowerCase().indexOf('uploads/');
    if (plainUploadsIndex >= 0) safeUrl = `/${safeUrl.slice(plainUploadsIndex)}`;
  }

  const apiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return safeUrl.startsWith("/") ? `${origin}${safeUrl}` : `${origin}/${safeUrl}`;
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
  const avatarValue = avatarUrl ?? AvatarUrl ?? organization?.avatarURL ?? organization?.AvatarURL;
  const coverValue = coverUrl ?? CoverUrl ?? organization?.coverURL ?? organization?.CoverURL;
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
          <span className="org-stat-label">Thành viên</span>
          <span className="org-stat-val">{memberCount}</span>
        </div>

        {location && (
          <div className="org-stat-item org-stat-item--wide">
            <span className="org-stat-label">Địa điểm</span>
            <span className="org-stat-val small-text">{location}</span>
          </div>
        )}

        {status && (
          <div className="org-stat-item">
            <span className="org-stat-label">Trạng thái</span>
            <span className="org-stat-val">{status}</span>
          </div>
        )}

        {displayDate && (
          <div className="org-stat-item">
            <span className="org-stat-label">
              Ngày thành lập
            </span>
            <span className="org-stat-val small-text">{displayDate}</span>
          </div>
        )}
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
