/**
 * EventStatusBadge.jsx - Event status badge component
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component displays an event status badge.
 * 
 * Props:
 * - status: Event status value (from EventStatus enum)
 * 
 * Event Status Values:
 * - Draft
 * - Published
 * - InProgress
 * - Completed
 * - Cancelled
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render status badge with appropriate styling
 * - Map status to display text and color
 * 
 * IMPORTANT:
 * - Props-driven only
 * - No fake data
 */

function EventStatusBadge({ status = 'Draft' }) {
  // TODO Phase 3C-5+: Map status to display text and color
  const statusMap = {
    Draft: { text: 'Draft', className: 'status-draft' },
    Published: { text: 'Published', className: 'status-published' },
    InProgress: { text: 'In Progress', className: 'status-in-progress' },
    Completed: { text: 'Completed', className: 'status-completed' },
    Cancelled: { text: 'Cancelled', className: 'status-cancelled' }
  };

  const statusInfo = statusMap[status] || statusMap.Draft;

  return (
    <span className={`event-status-badge ${statusInfo.className}`}>
      {statusInfo.text}
    </span>
  );
}

export default EventStatusBadge;
