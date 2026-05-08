/**
 * OrgSwitcher.jsx - Organization switcher component
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component provides a dropdown to switch between user's organizations.
 * 
 * Props:
 * - currentOrgId: Current organization ID
 * - organizations: Array of user's organizations
 * - onSwitch: Callback when organization is switched
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render dropdown with organization list
 * - Handle organization switch
 * - Update URL query string with new orgId
 * 
 * IMPORTANT:
 * - No fake organization data
 * - Props-driven only
 * - orgId is query string parameter, not path parameter
 */

function OrgSwitcher({ currentOrgId, organizations = [], onSwitch }) {
  // TODO Phase 3C-5+: Handle organization switch
  // const handleSwitch = (orgId) => {
  //   onSwitch(orgId);
  //   // Update URL query string: ?orgId=newOrgId
  // };

  return (
    <div className="org-switcher">
      <label>Organization:</label>
      <select value={currentOrgId || ''} disabled={organizations.length === 0}>
        <option value="">Select Organization</option>
        {organizations.map(org => (
          <option key={org.id} value={org.id}>
            {org.name}
          </option>
        ))}
      </select>
    </div>
  );
}

export default OrgSwitcher;
