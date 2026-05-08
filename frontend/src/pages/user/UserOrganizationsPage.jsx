/**
 * UserOrganizationsPage.jsx - User's organizations page
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyOrganizations } from '../../services/userService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';

function UserOrganizationsPage() {
  const navigate = useNavigate();

  const [organizations, setOrganizations] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function loadOrganizations() {
      setIsLoading(true);
      try {
        const data = await getMyOrganizations();
        setOrganizations(data);
      } catch (err) {
        setError(err.message || 'Failed to load organizations');
      } finally {
        setIsLoading(false);
      }
    }
    loadOrganizations();
  }, []);

  const handleOrgClick = (orgId) => {
    navigate(`/org/overview?orgId=${orgId}`);
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="My Organizations"
          description="Organizations you are a member of"
        />
        <LoadingSpinner message="Loading organizations..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="My Organizations"
          description="Organizations you are a member of"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="My Organizations"
        description="Organizations you are a member of"
      />

      <div className="app-section">
        {organizations.length === 0 ? (
          <EmptyState message="You are not a member of any organizations" />
        ) : (
          <div className="app-card">
            <table>
              <thead>
                <tr>
                  <th>Organization Name</th>
                  <th>Description</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {organizations.map((org) => (
                  <tr key={org.id}>
                    <td>{org.name || '-'}</td>
                    <td>{org.description || '-'}</td>
                    <td>{org.roleName || '-'}</td>
                    <td><span className="app-badge app-badge--success">Active</span></td>
                    <td>
                      <button 
                        onClick={() => handleOrgClick(org.id)}
                        className="app-button app-button--primary"
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default UserOrganizationsPage;
