/**
 * ErrorState.jsx - Error state component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * Display when an error occurs during data loading or operations.
 * 
 * Usage:
 *   <ErrorState message="Failed to load data" />
 *   <ErrorState message="Network error" onRetry={() => refetch()} />
 */

function ErrorState({ message = 'Đã xảy ra lỗi', onRetry = null }) {
  return (
    <div className="app-state app-error" role="alert">
      <div className="app-state-icon" aria-hidden="true">!</div>
      <h3 className="app-state-title">Lỗi</h3>
      <p className="app-state-message">{message}</p>
      {onRetry && (
        <button onClick={onRetry} className="app-button app-button--primary">
          Thử lại
        </button>
      )}
    </div>
  );
}

export default ErrorState;
