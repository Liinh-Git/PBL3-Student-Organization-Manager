/**
 * PrototypePlaceholder.jsx - Placeholder for prototype-only features
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * Used for PROTOTYPE_ONLY modules that are not implemented in base prototype:
 * - /org/tasks aggregate board
 * - Reports page
 * - Finance page
 * - Resources page
 * - Optional unavailable future features
 * 
 * IMPORTANT RULES:
 * - No fake data, no fake board, no fake counts
 * - Clear indication that feature is prototype-only
 * - Accept title, description, status, and optional notes
 * 
 * Usage:
 *   <PrototypePlaceholder
 *     title="Task Board"
 *     description="Aggregate task board across all events"
 *     status="PROTOTYPE_ONLY"
 *     notes="Task CRUD is available inside EventDetail tree"
 *   />
 */

function PrototypePlaceholder({ 
  title = 'Feature', 
  description = 'This feature is in prototype stage',
  status = 'PROTOTYPE_ONLY',
  notes = null
}) {
  return (
    <div className="prototype-placeholder">
      <div className="placeholder-content">
        <div className="app-state-icon" aria-hidden="true">⧗</div>
        <h2>{title}</h2>
        <p className="placeholder-description">{description}</p>
        <span className="placeholder-status">{status}</span>
        {notes && (
          <div className="placeholder-notes">
            <p><strong>Note:</strong> {notes}</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default PrototypePlaceholder;

