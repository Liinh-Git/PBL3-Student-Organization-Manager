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
    <div className="app-loading">
      <h3>Đang tải...</h3>
      {message && <p>{message}</p>}
    </div>
  );
}

export default LoadingSpinner;
