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
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';

function UserDiscoverPage() {
  const [searchParams] = useSearchParams();

  const [organizations, setOrganizations] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

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
                {organizations.map((org) => (
                  <tr key={org.id}>
                    <td>{org.orgName || '-'}</td>
                    <td>{org.description || '-'}</td>
                    <td>{org.location || '-'}</td>
                    <td>{org.totalMembers || '-'}</td>
                    <td>{org.status || '-'}</td>
                    <td>
                      <button disabled className="app-button app-button--primary">Request to Join (Write UI Pending)</button>
                    </td>
                  </tr>
                ))}
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
