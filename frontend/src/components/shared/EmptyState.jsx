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

function EmptyState({ message = "No data available", action = null }) {
  return (
    <div className="app-empty">
      <h3>No Data</h3>
      <p>{message}</p>
      {action && <div className="app-action-row">{action}</div>}
    </div>
  );
}

export default EmptyState;
