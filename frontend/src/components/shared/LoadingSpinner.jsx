/**
 * LoadingSpinner.jsx - Loading spinner component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * Simple reusable loading spinner for async operations.
 * 
 * Usage:
 *   <LoadingSpinner />
 *   <LoadingSpinner message="Loading data..." />
 */

function LoadingSpinner({ message = 'Đang tải...' }) {
  return (
    <div className="app-state app-loading" role="status" aria-live="polite">
      <div className="app-state-icon app-state-icon--spin" aria-hidden="true">⟳</div>
      <h3 className="app-state-title">Đang tải...</h3>
      {message && <p className="app-state-message">{message}</p>}
    </div>
  );
}

export default LoadingSpinner;
