/**
 * PageHeader.jsx - Page header component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * Reusable page header with title, description, and optional actions.
 * 
 * Usage:
 *   <PageHeader
 *     title="Events"
 *     description="Manage organization events"
 *     actions={<button>Create Event</button>}
 *   />
 */

function PageHeader({ 
  title = 'Page Title',
  description = null,
  actions = null,
  kicker = null
}) {
  return (
    <div className="app-page-header">
      <div>
        {kicker && <div className="app-kicker">{kicker}</div>}
        <h1 className="app-page-title">{title}</h1>
        {description && <p className="app-page-subtitle">{description}</p>}
      </div>
      {actions && (
        <div className="app-action-row">
          {actions}
        </div>
      )}
    </div>
  );
}

export default PageHeader;
