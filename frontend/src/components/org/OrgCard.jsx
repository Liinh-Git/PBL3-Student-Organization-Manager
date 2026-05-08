/**
 * OrgCard.jsx - Organization card component
 * 
 * This component displays an organization summary card.
 * 
 * Props:
 * - organization: Organization data object (OrganizationDto or OrganizationSummaryDto)
 * - onClick: Callback when card is clicked
 * 
 * Data fields used:
 * - name: Organization name
 * - description: Organization description
 * - avatarUrl: Organization avatar/image
 * - totalMembers: Number of members
 * - status: Organization status
 * - foundingDate: Organization founding date (optional)
 * - createdAtUtc: Creation timestamp (optional)
 */

function OrgCard({ organization, onClick }) {
  if (!organization) {
    return null;
  }

  const {
    id,
    name,
    description,
    avatarUrl,
    totalMembers,
    status,
    foundingDate,
    createdAtUtc
  } = organization;

  // Format date for display
  const formatDate = (dateString) => {
    if (!dateString) return null;
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const displayDate = formatDate(foundingDate) || formatDate(createdAtUtc);

  return (
    <div className="org-card" onClick={() => onClick?.(id)}>
      <div className="org-card-header">
        {avatarUrl ? (
          <img 
            src={avatarUrl} 
            alt={`${name} avatar`} 
            className="org-card-avatar"
            onError={(e) => {
              e.target.style.display = 'none';
            }}
          />
        ) : (
          <div className="org-card-avatar-placeholder">
            {name?.charAt(0)?.toUpperCase() || 'O'}
          </div>
        )}
        <h4 className="org-card-name">{name || 'Organization Name'}</h4>
      </div>

      <div className="org-card-body">
        <p className="org-card-description">
          {description || 'No description available'}
        </p>
        
        <div className="org-card-meta">
          <div className="org-card-meta-item">
            <span className="org-card-meta-label">Members:</span>
            <span className="org-card-meta-value">{totalMembers || 0}</span>
          </div>
          
          {status && (
            <div className="org-card-meta-item">
              <span className="org-card-meta-label">Status:</span>
              <span className="org-card-meta-value">{status}</span>
            </div>
          )}
          
          {displayDate && (
            <div className="org-card-meta-item">
              <span className="org-card-meta-label">
                {foundingDate ? 'Founded:' : 'Created:'}
              </span>
              <span className="org-card-meta-value">{displayDate}</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default OrgCard;
