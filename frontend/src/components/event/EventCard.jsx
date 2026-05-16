/**
 * EventCard.jsx - Event card component
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component displays an event summary card.
 * 
 * Props:
 * - event: Event data object
 * - onClick: Callback when card is clicked
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render event information (name, description, startDate, endDate, location, status)
 * - Add click handler
 * - Display EventStatusBadge
 * 
 * IMPORTANT:
 * - No fake event data
 * - Props-driven only
 */

import EventStatusBadge from './EventStatusBadge';

const DEFAULT_EVENT_BANNER =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='360' viewBox='0 0 1200 360'>" +
      "<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>" +
      "<stop offset='0%' stop-color='#dbeafe'/><stop offset='100%' stop-color='#bfdbfe'/>" +
      "</linearGradient></defs>" +
      "<rect width='1200' height='360' fill='url(#g)'/>" +
      "<text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#1e3a8a' font-family='Arial' font-size='42'>Event Banner</text>" +
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

function EventCard({ event, onView }) {
  const eventName = event?.name || event?.eventName || 'Untitled Event';
  const organizationName = event?.organizationName || event?.OrganizationName || event?.orgName || event?.OrgName || event?.organization?.orgName || '-';
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
    <div className="app-card app-card--interactive">
      <img
        src={bannerSrc}
        alt={`${eventName} banner`}
        style={{ width: "100%", maxHeight: 180, objectFit: "cover" }}
        onError={(e) => {
          e.currentTarget.onerror = null;
          e.currentTarget.src = DEFAULT_EVENT_BANNER;
        }}
      />

      <div className="app-section-header">
        <h4 className="app-section-title">{eventName}</h4>
        <EventStatusBadge status={event?.status} />
      </div>

      <div className="app-muted">
        <p>{event?.description || ''}</p>
        <p><strong>Organization:</strong> {organizationName}</p>
        <p><strong>Start:</strong> {event?.startDate ? new Date(event.startDate).toLocaleDateString() : '-'}</p>
        <p><strong>End:</strong> {event?.endDate ? new Date(event.endDate).toLocaleDateString() : '-'}</p>
        <p><strong>Location:</strong> {event?.location || '-'}</p>
        {participationLabel && <p><strong>Your Role:</strong> {participationLabel}</p>}
      </div>

      <div className="app-action-row">
        <button type="button" onClick={onView} className="app-button app-button--primary">
          View
        </button>
      </div>
    </div>
  );
}

export default EventCard;
