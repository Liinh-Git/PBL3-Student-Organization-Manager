/**
 * UserOrganizationsPage.jsx - User's organizations page
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyOrganizations } from '../../services/userService.js';
import { createOrganization } from '../../services/organizationService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import OrgCard from '../../components/org/OrgCard';

function UserOrganizationsPage() {
  const navigate = useNavigate();

  const [organizations, setOrganizations] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [createError, setCreateError] = useState(null);
  const [formData, setFormData] = useState({
    orgName: '',
    description: '',
    avatarUrl: '',
    coverUrl: '',
    foundingDate: '',
    location: '',
    contactEmail: '',
    contactPhone: ''
  });

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

  const handleCreateOrganization = async (e) => {
    e.preventDefault();
    setIsCreating(true);
    setCreateError(null);

    try {
      // Build payload with only non-empty fields
      const payload = {};
      if (formData.orgName) payload.orgName = formData.orgName;
      if (formData.description) payload.description = formData.description;
      if (formData.avatarUrl) payload.avatarUrl = formData.avatarUrl;
      if (formData.coverUrl) payload.coverUrl = formData.coverUrl;
      if (formData.foundingDate) payload.foundingDate = formData.foundingDate;
      if (formData.location) payload.location = formData.location;
      if (formData.contactEmail) payload.contactEmail = formData.contactEmail;
      if (formData.contactPhone) payload.contactPhone = formData.contactPhone;

      const newOrg = await createOrganization(payload);
      
      // Refresh organizations list
      const updatedOrgs = await getMyOrganizations();
      setOrganizations(updatedOrgs);
      
      // Close modal and reset form
      setShowCreateModal(false);
      setFormData({
        orgName: '',
        description: '',
        avatarUrl: '',
        coverUrl: '',
        foundingDate: '',
        location: '',
        contactEmail: '',
        contactPhone: ''
      });
    } catch (err) {
      setCreateError(err.message || 'Failed to create organization');
    } finally {
      setIsCreating(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
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
        actions={
          <button
            onClick={() => setShowCreateModal(true)}
            className="app-button app-button--primary"
          >
            Create Organization
          </button>
        }
      />

      <div className="app-section">
        {organizations.length === 0 ? (
          <EmptyState message="You are not a member of any organizations" />
        ) : (
          <div className="org-card-grid">
            {organizations.map((org) => (
              <OrgCard
                key={org.id}
                organization={org}
                onClick={handleOrgClick}
              />
            ))}
          </div>
        )}
      </div>

      {/* Create Organization Modal */}
      {showCreateModal && (
        <div className="app-modal-overlay" onClick={() => setShowCreateModal(false)}>
          <div className="app-modal" onClick={(e) => e.stopPropagation()}>
            <div className="app-modal-header">
              <h3>Create New Organization</h3>
              <button
                onClick={() => setShowCreateModal(false)}
                className="app-modal-close"
              >
                ×
              </button>
            </div>
            <div className="app-modal-body">
              <form onSubmit={handleCreateOrganization}>
                {createError && (
                  <div className="app-alert app-alert--error">
                    {createError}
                  </div>
                )}

                <div className="app-form-group">
                  <label htmlFor="orgName" className="app-form-label">
                    Organization Name *
                  </label>
                  <input
                    type="text"
                    id="orgName"
                    name="orgName"
                    value={formData.orgName}
                    onChange={handleInputChange}
                    className="app-form-input"
                    required
                    minLength={2}
                    maxLength={200}
                    placeholder="Enter organization name"
                  />
                </div>

                <div className="app-form-group">
                  <label htmlFor="description" className="app-form-label">
                    Description
                  </label>
                  <textarea
                    id="description"
                    name="description"
                    value={formData.description}
                    onChange={handleInputChange}
                    className="app-form-input"
                    rows={3}
                    placeholder="Enter organization description"
                  />
                </div>

                <div className="app-form-group">
                  <label htmlFor="avatarUrl" className="app-form-label">
                    Avatar URL
                  </label>
                  <input
                    type="url"
                    id="avatarUrl"
                    name="avatarUrl"
                    value={formData.avatarUrl}
                    onChange={handleInputChange}
                    className="app-form-input"
                    placeholder="https://example.com/avatar.png"
                  />
                </div>

                <div className="app-form-group">
                  <label htmlFor="coverUrl" className="app-form-label">
                    Cover URL
                  </label>
                  <input
                    type="url"
                    id="coverUrl"
                    name="coverUrl"
                    value={formData.coverUrl}
                    onChange={handleInputChange}
                    className="app-form-input"
                    placeholder="https://example.com/cover.png"
                  />
                </div>

                <div className="app-form-group">
                  <label htmlFor="location" className="app-form-label">
                    Location
                  </label>
                  <input
                    type="text"
                    id="location"
                    name="location"
                    value={formData.location}
                    onChange={handleInputChange}
                    className="app-form-input"
                    placeholder="Enter organization location"
                  />
                </div>

                <div className="app-form-group">
                  <label htmlFor="contactEmail" className="app-form-label">
                    Contact Email
                  </label>
                  <input
                    type="email"
                    id="contactEmail"
                    name="contactEmail"
                    value={formData.contactEmail}
                    onChange={handleInputChange}
                    className="app-form-input"
                    placeholder="contact@example.com"
                  />
                </div>

                <div className="app-form-group">
                  <label htmlFor="contactPhone" className="app-form-label">
                    Contact Phone
                  </label>
                  <input
                    type="tel"
                    id="contactPhone"
                    name="contactPhone"
                    value={formData.contactPhone}
                    onChange={handleInputChange}
                    className="app-form-input"
                    placeholder="+1234567890"
                  />
                </div>

                <div className="app-modal-actions">
                  <button
                    type="button"
                    onClick={() => setShowCreateModal(false)}
                    className="app-button app-button--secondary"
                    disabled={isCreating}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="app-button app-button--primary"
                    disabled={isCreating}
                  >
                    {isCreating ? 'Creating...' : 'Create Organization'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default UserOrganizationsPage;
