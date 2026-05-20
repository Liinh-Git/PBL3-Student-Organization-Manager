/**
 * ForbiddenState.jsx - 403 Forbidden state component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * Display when user is authenticated but not authorized to access a resource.
 * 
 * IMPORTANT RULES:
 * - 403 should NOT globally redirect to /forbidden
 * - Render this component at page/route level
 * - User is authenticated but lacks permission
 * 
 * Usage:
 *   <ForbiddenState />
 *   <ForbiddenState message="You are not a member of this organization" />
 */

function ForbiddenState({ message = 'Bạn không có quyền truy cập tài nguyên này' }) {
  return (
    <div className="app-state app-forbidden" role="alert">
      <div className="app-state-icon" aria-hidden="true">⛔</div>
      <h3 className="app-state-title">Không có quyền truy cập</h3>
      <p className="app-state-message">{message}</p>
    </div>
  );
}

export default ForbiddenState;
