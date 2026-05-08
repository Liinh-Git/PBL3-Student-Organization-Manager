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

function EventCard({ event, onClick }) {
  return (
    <div className="event-card" onClick={onClick}>
      <div className="event-card-header">
        <h4>{event?.name || 'Event Name'}</h4>
        <EventStatusBadge status={event?.status} />
      </div>

      <div className="event-card-body">
        <p>{event?.description || ''}</p>
        {/* TODO Phase 3C-5+: Display event.startDate, event.endDate */}
        {/* TODO Phase 3C-5+: Display event.location */}
      </div>
    </div>
  );
}

export default EventCard;
