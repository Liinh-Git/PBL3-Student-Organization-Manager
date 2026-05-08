/**
 * OrgCard.jsx - Organization card component
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component displays an organization summary card.
 * 
 * Props:
 * - organization: Organization data object
 * - onClick: Callback when card is clicked
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render organization information (name, description, createdAt)
 * - Add click handler
 * - Display member count (if available)
 * 
 * IMPORTANT:
 * - No fake organization data
 * - Props-driven only
 */

function OrgCard({ organization, onClick }) {
  return (
    <div className="org-card" onClick={onClick}>
      <div className="org-card-header">
        <h4>{organization?.name || 'Organization Name'}</h4>
      </div>

      <div className="org-card-body">
        <p>{organization?.description || ''}</p>
        {/* TODO Phase 3C-5+: Display organization.createdAt */}
        {/* TODO Phase 3C-5+: Display member count if available */}
      </div>
    </div>
  );
}

export default OrgCard;
