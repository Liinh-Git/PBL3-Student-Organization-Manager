/**
 * UserDiscoverPage.jsx - Discover organizations and events page
 * 
 * Phase 4B-1B: Safe read-only page completion
 * 
 * Connected to:
 * - userService.discoverMyOrganizations()
 * 
 * Note: discoverService.discoverEvents is not implemented yet (backend Phase 4A-5 pending)
 * 
 * Permissions:
 * - JWT (authenticated user)
 * 
 * Route:
 * - /user/discover
 */

import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { discoverMyOrganizations } from '../../services/userService.js';
import { createOrganizationRequest } from '../../services/requestService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';

function UserDiscoverPage() {
  const [searchParams] = useSearchParams();

  const [organizations, setOrganizations] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [requestingOrgId, setRequestingOrgId] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);

  useEffect(() => {
    async function loadData() {
      setIsLoading(true);
      try {
        const data = await discoverMyOrganizations();
        setOrganizations(data);
      } catch (err) {
        setError(err.message || 'Failed to load discoverable organizations');
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, []);

  const handleRequestToJoin = async (orgId, orgName) => {
    const safeOrgName = orgName || 'this organization';

    setRequestingOrgId(orgId);
    setSuccessMessage(null);

    try {
      await createOrganizationRequest(orgId, {
        requestType: 'JoinOrganization',
        content: `I would like to join ${safeOrgName}`,
      });

      setSuccessMessage(`Request sent to ${safeOrgName}`);
    } catch (err) {
      setError(err.message || 'Failed to send join request');
    } finally {
      setRequestingOrgId(null);
    }
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Discover"
          description="Find organizations and events to join"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Discover"
          description="Find organizations and events to join"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Discover"
        description="Find organizations and events to join"
      />

      <div className="app-section">
        {successMessage && (
          <div className="app-card" style={{ marginBottom: '16px', backgroundColor: '#d4edda', borderColor: '#c3e6cb' }}>
            <p style={{ color: '#155724', margin: 0 }}>{successMessage}</p>
          </div>
        )}
        {/* Organizations section */}
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Organizations</h3>
          </div>
          {organizations.length === 0 ? (
            <EmptyState message="No discoverable organizations found" />
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Organization Name</th>
                  <th>Description</th>
                  <th>Location</th>
                  <th>Members</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {organizations.map((org) => {
                const orgName = org.name || org.orgName || org.organizationName || 'Unknown organization';

                return (
                  <tr key={org.id}>
                    <td>{orgName}</td>
                    <td>{org.description || '-'}</td>
                    <td>{org.location || '-'}</td>
                    <td>{org.totalMembers ?? '-'}</td>
                    <td>{org.status || (org.isActive ? 'Active' : 'Inactive')}</td>
                    <td>
                      <button
                        onClick={() => handleRequestToJoin(org.id, orgName)}
                        disabled={requestingOrgId === org.id}
                        className="app-button app-button--primary"
                      >
                        {requestingOrgId === org.id ? 'Sending...' : 'Request to Join'}
                      </button>
                    </td>
                  </tr>
                );
              })}
              </tbody>
            </table>
          )}
        </div>

        {/* Events section - Not implemented yet (backend Phase 4A-5 pending) */}
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Events</h3>
          </div>
          <EmptyState message="Event discovery not implemented yet (backend Phase 4A-5 pending)" />
        </div>
      </div>
    </div>
  );
}

export default UserDiscoverPage;
