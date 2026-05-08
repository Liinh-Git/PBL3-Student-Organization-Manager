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

function EventCard({ event, onView }) {
  const eventName = event?.name || event?.eventName || 'Untitled Event';
  const organizationName = event?.organizationName || event?.orgName || event?.organization?.orgName || '-';
  const participationLabel = event?.participationRole || event?.participantRole || event?.relationType || null;

  return (
    <div className="app-card app-card--interactive">
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
