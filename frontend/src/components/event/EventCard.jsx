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

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;

  const apiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return url.startsWith("/") ? `${origin}${url}` : `${origin}/${url}`;
}

function EventCard({ event, onView }) {
  const eventName = event?.name || event?.eventName || 'Untitled Event';
  const organizationName = event?.organizationName || event?.orgName || event?.organization?.orgName || '-';
  const participationLabel = event?.participationRole || event?.participantRole || event?.relationType || null;
  const bannerSrc = toAbsoluteMediaUrl(event?.bannerUrl || event?.coverUrl || event?.avatarUrl);

  return (
    <div className="app-card app-card--interactive">
      {bannerSrc ? (
        <img
          src={bannerSrc}
          alt={`${eventName} banner`}
          style={{ width: "100%", maxHeight: 180, objectFit: "cover" }}
          onError={(e) => {
            e.currentTarget.style.display = "none";
          }}
        />
      ) : null}

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
