/**
 * EmptyState.jsx - Empty state component
 *
 * Phase 3C-4A: Foundation skeleton only
 *
 * Display when a list or collection is empty.
 *
 * Usage:
 *   <EmptyState message="No events found" />
 *   <EmptyState message="No members yet" action={<button>Add Member</button>} />
 */

function EmptyState({ message = "Chưa có dữ liệu", action = null }) {
  return (
    <div className="app-state app-empty" role="status" aria-live="polite">
      <div className="app-state-icon" aria-hidden="true">◻</div>
      <h3 className="app-state-title">Không có dữ liệu</h3>
      <p className="app-state-message">{message}</p>
      {action && <div className="app-action-row app-state-actions">{action}</div>}
    </div>
  );
}

export default EmptyState;
