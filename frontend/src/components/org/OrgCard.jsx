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

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;

  const apiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return url.startsWith("/") ? `${origin}${url}` : `${origin}/${url}`;
}

function OrgCard({ organization, onClick, onDelete, isDeleting = false }) {
  if (!organization) {
    return null;
  }

  const {
    id,
    name,
    description,
    avatarUrl,
    coverUrl,
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

  const displayDate = formatDate(foundingDate);
  const avatarSrc = toAbsoluteMediaUrl(avatarUrl);
  const coverSrc = toAbsoluteMediaUrl(coverUrl);
  const memberCount = totalMembers ?? 0;

  return (
    <div className="org-card" onClick={() => onClick?.(id)}>
      <div className="org-card-media">
        {coverSrc ? (
          <img
            className="org-card-cover"
            src={coverSrc}
            alt={`${name || "Organization"} cover`}
            onError={(e) => {
              e.currentTarget.style.display = "none";
            }}
          />
        ) : (
          <div className="org-card-cover-placeholder" />
        )}
        <div className="org-card-logo" aria-label={`${name || "Organization"} avatar`}>
          {avatarSrc ? (
            <img
              src={avatarSrc}
              alt={`${name || "Organization"} avatar`}
              onError={(e) => {
                e.currentTarget.style.display = "none";
              }}
            />
          ) : (
            <svg
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M12 2L2 7l10 5 10-5-10-5z"></path>
              <path d="M2 17l10 5 10-5"></path>
              <path d="M2 12l10 5 10-5"></path>
            </svg>
          )}
        </div>
      </div>

      <h3 className="org-card-title">{name || "Tên tổ chức"}</h3>
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
