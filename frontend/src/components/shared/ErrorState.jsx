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

function ErrorState({ message = 'An error occurred', onRetry = null }) {
  return (
    <div className="app-error">
      <h3>Error</h3>
      <p>{message}</p>
      {onRetry && (
        <button onClick={onRetry} className="app-button app-button--primary">
          Retry
        </button>
      )}
    </div>
  );
}

export default ErrorState;
