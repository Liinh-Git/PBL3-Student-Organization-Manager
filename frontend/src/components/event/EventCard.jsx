import EventStatusBadge from "./EventStatusBadge";
import "./EventCard.css";

const DEFAULT_EVENT_BANNER =
  "data:image/svg+xml;utf8," +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='360' viewBox='0 0 1200 360'>" +
      "<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>" +
      "<stop offset='0%' stop-color='#dbeafe'/><stop offset='100%' stop-color='#bfdbfe'/>" +
      "</linearGradient></defs>" +
      "<rect width='1200' height='360' fill='url(#g)'/>" +
      "<text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#1e3a8a' font-family='Arial' font-size='42'>Banner sự kiện</text>" +
      "</svg>",
  );

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  let safeUrl = String(url).trim();
  if (!safeUrl) return "";

  safeUrl = safeUrl.replace(/\\/g, "/");
  safeUrl = safeUrl.replace(/^['\"]|['\"]$/g, "");

  if (/^https?:\/\//i.test(safeUrl)) return safeUrl;
  if (/^www\./i.test(safeUrl)) return `https://${safeUrl}`;

  const uploadsIndex = safeUrl.toLowerCase().indexOf("/uploads/");
  if (uploadsIndex >= 0) {
    safeUrl = safeUrl.slice(uploadsIndex);
  } else {
    const plainUploadsIndex = safeUrl.toLowerCase().indexOf("uploads/");
    if (plainUploadsIndex >= 0) {
      safeUrl = `/${safeUrl.slice(plainUploadsIndex)}`;
    }
  }

  const apiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return safeUrl.startsWith("/") ? `${origin}${safeUrl}` : `${origin}/${safeUrl}`;
}

function formatDay(dateString) {
  if (!dateString) return "-";
  const d = new Date(dateString);
  return d.toLocaleDateString("vi-VN", {
    weekday: "short",
    day: "2-digit",
    month: "short",
  });
}

function formatTime(dateString) {
  if (!dateString) return "-";
  const d = new Date(dateString);
  return d.toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function EventCard({ event, onView, showDetailButton = true, footerActions = null }) {
  const eventName = event?.name || event?.eventName || "Untitled Event";
  const organizationName =
    event?.organizationName ||
    event?.OrganizationName ||
    event?.orgName ||
    event?.OrgName ||
    event?.organization?.orgName ||
    "-";

  const participationLabel = event?.participationRole || event?.participantRole || event?.relationType || null;

  const bannerValue =
    event?.bannerUrl ??
    event?.BannerUrl ??
    event?.coverUrl ??
    event?.CoverUrl ??
    event?.avatarUrl ??
    event?.AvatarUrl;

  const bannerSrc = toAbsoluteMediaUrl(bannerValue) || DEFAULT_EVENT_BANNER;

  return (
    <div className="event-card">
      <div className="event-card-cover-wrap">
        <img
          src={bannerSrc}
          alt={`${eventName} banner`}
          className="event-card-cover"
          onError={(e) => {
            e.currentTarget.onerror = null;
            e.currentTarget.src = DEFAULT_EVENT_BANNER;
          }}
        />
        <div className="event-card-status-badge">
          <EventStatusBadge status={event?.status} />
        </div>
      </div>

      <div className="event-card-body">
        <h4 className="event-card-title">{eventName}</h4>

        {event?.description ? <p className="event-card-desc">{event.description}</p> : null}

        <div className="event-card-meta">
          <div className="event-card-meta-row">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#94a3b8" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <rect x="3" y="3" width="18" height="18" rx="2" />
              <path d="M9 9h1" />
              <path d="M14 9h1" />
              <path d="M9 14h1" />
              <path d="M14 14h1" />
            </svg>
            <span className="event-card-meta-label">Tổ chức</span>
            <span className="event-card-meta-val">{organizationName}</span>
          </div>

          <div className="event-card-meta-row">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#94a3b8" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <rect x="3" y="4" width="18" height="18" rx="2" />
              <line x1="16" y1="2" x2="16" y2="6" />
              <line x1="8" y1="2" x2="8" y2="6" />
              <line x1="3" y1="10" x2="21" y2="10" />
            </svg>
            <span className="event-card-meta-label">Ngày</span>
            <span className="event-card-meta-val">{event?.startDate ? formatDay(event.startDate) : "-"}</span>
          </div>

          <div className="event-card-meta-row">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#94a3b8" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <circle cx="12" cy="12" r="10" />
              <polyline points="12 6 12 12 16 14" />
            </svg>
            <span className="event-card-meta-label">Giờ</span>
            <span className="event-card-meta-val">{event?.startDate ? formatTime(event.startDate) : "-"}</span>
          </div>

          {event?.location ? (
            <div className="event-card-meta-row">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#94a3b8" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                <circle cx="12" cy="10" r="3" />
              </svg>
              <span className="event-card-meta-label">Địa điểm</span>
              <span className="event-card-meta-val event-card-meta-val--truncate">{event.location}</span>
            </div>
          ) : null}

          {participationLabel ? (
            <div className="event-card-meta-row">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#94a3b8" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                <circle cx="12" cy="7" r="4" />
              </svg>
              <span className="event-card-meta-label">Vai trò</span>
              <span className="event-card-meta-val">{participationLabel}</span>
            </div>
          ) : null}
        </div>
      </div>

      {showDetailButton || footerActions ? (
        <div className="event-card-footer">
          {showDetailButton ? (
            <button type="button" onClick={onView} className="event-card-btn">
              Xem chi tiết
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <line x1="5" y1="12" x2="19" y2="12" />
                <polyline points="12 5 19 12 12 19" />
              </svg>
            </button>
          ) : null}
          {footerActions ? <div className="event-card-footer-actions">{footerActions}</div> : null}
        </div>
      ) : null}
    </div>
  );
}

export default EventCard;
