/**
 * StatusBadge.jsx - Status badge component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * Display status badges for various entities (events, tasks, members, etc.)
 * 
 * Usage:
 *   <StatusBadge status="Active" variant="success" />
 *   <StatusBadge status="Pending" variant="warning" />
 *   <StatusBadge status="Cancelled" variant="danger" />
 */

function StatusBadge({ 
  status = 'Unknown',
  variant = 'default' // default, success, warning, danger, info
}) {
  const variantClass = variant === 'success' ? 'app-badge--success' :
                      variant === 'warning' ? 'app-badge--warning' :
                      variant === 'info' ? 'app-badge--info' :
                      '';
  
  return (
    <span className={`app-badge ${variantClass}`}>
      {status}
    </span>
  );
}

export default StatusBadge;
