/**
 * OrgOverviewPage.jsx - Organization overview page
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getOrganizationById, updateOrganization } from '../../services/organizationService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ErrorState from '../../components/shared/ErrorState';
import ForbiddenState from '../../components/shared/ForbiddenState';

function OrgOverviewPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');
  const { organization: contextOrg, loadWorkspaceOrg, permissions, isMember, isLoading: contextLoading } = useOrgContext();

  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);

  useEffect(() => {
    if (orgId && !contextOrg) {
      loadWorkspaceOrg(orgId);
    }
  }, [orgId, contextOrg, loadWorkspaceOrg]);

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (contextLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Organization Overview"
          description="View organization details and statistics"
        />
        <LoadingSpinner message="Loading organization..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Organization Overview"
          description="View organization details and statistics"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader
          title="Organization Overview"
          description="View organization details and statistics"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  const canEdit = permissions.includes('org.overview.write');

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canEdit || !orgId) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const orgName = form.orgName.value;
    const description = form.description.value;
    const location = form.location.value;
    const contactEmail = form.contactEmail.value;
    const contactPhone = form.contactPhone.value;
    
    if (!orgName) {
      alert('Organization name is required');
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await updateOrganization(orgId, {
        name: orgName,
        description: description || undefined,
        location: location || undefined,
        contactEmail: contactEmail || undefined,
        contactPhone: contactPhone || undefined
      });
      loadWorkspaceOrg(orgId);
      setShowEditForm(false);
    } catch (err) {
      alert(err.message || 'Failed to update organization');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="app-page">
      <PageHeader
        title="Organization Overview"
        description="View organization details and statistics"
        actions={
          canEdit && (
            <button 
              onClick={() => setShowEditForm(true)}
              className="app-button app-button--primary"
            >
              Edit Organization
            </button>
          )
        }
      />

      {showEditForm && canEdit && (
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Edit Organization</h3>
          </div>
          <form onSubmit={handleUpdate} className="auth-form">
            <div className="form-group">
              <label htmlFor="orgName" className="form-label">Organization Name *</label>
              <input
                id="orgName"
                name="orgName"
                className="form-input"
                defaultValue={contextOrg?.name || ''}
                placeholder="Organization name"
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="description" className="form-label">Description</label>
              <input
                id="description"
                name="description"
                className="form-input"
                defaultValue={contextOrg?.description || ''}
                placeholder="Description"
              />
            </div>
            <div className="form-group">
              <label htmlFor="location" className="form-label">Location</label>
              <input
                id="location"
                name="location"
                className="form-input"
                defaultValue={contextOrg?.location || ''}
                placeholder="Location"
              />
            </div>
            <div className="form-group">
              <label htmlFor="contactEmail" className="form-label">Contact Email</label>
              <input
                id="contactEmail"
                name="contactEmail"
                className="form-input"
                defaultValue={contextOrg?.contactEmail || ''}
                placeholder="Contact Email"
              />
            </div>
            <div className="form-group">
              <label htmlFor="contactPhone" className="form-label">Contact Phone</label>
              <input
                id="contactPhone"
                name="contactPhone"
                className="form-input"
                defaultValue={contextOrg?.contactPhone || ''}
                placeholder="Contact Phone"
              />
            </div>
            <div className="app-action-row">
              <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                {isSubmitting ? 'Updating...' : 'Update'}
              </button>
              <button type="button" onClick={() => setShowEditForm(false)} className="app-button app-button--ghost">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="app-section">
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Organization Details</h3>
          </div>
          <div style={{ display: 'grid', gap: '1rem' }}>
            <div>
              <label className="form-label">Name</label>
              <p style={{ margin: '0.25rem 0 0', color: 'var(--ink-700)' }}>{contextOrg?.name || '-'}</p>
            </div>
            <div>
              <label className="form-label">Description</label>
              <p style={{ margin: '0.25rem 0 0', color: 'var(--ink-700)' }}>{contextOrg?.description || '-'}</p>
            </div>
            <div>
              <label className="form-label">Created</label>
              <p style={{ margin: '0.25rem 0 0', color: 'var(--ink-700)' }}>{contextOrg?.createdAt ? new Date(contextOrg.createdAt).toLocaleDateString() : '-'}</p>
            </div>
          </div>
        </div>
      </div>

      <div className="app-section">
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Statistics</h3>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, minmax(0, 1fr))', gap: '1rem' }}>
            <div style={{ background: '#f4f8fc', border: '1px solid #dde7f1', borderRadius: '0.85rem', padding: '0.85rem' }}>
              <label className="form-label">Members</label>
              <p style={{ margin: '0.2rem 0 0', fontSize: '1.6rem', fontWeight: '700', color: 'var(--ink-900)' }}>{contextOrg?.totalMembers || '-'}</p>
            </div>
            <div style={{ background: '#f4f8fc', border: '1px solid #dde7f1', borderRadius: '0.85rem', padding: '0.85rem' }}>
              <label className="form-label">Status</label>
              <p style={{ margin: '0.2rem 0 0', fontSize: '1.6rem', fontWeight: '700', color: 'var(--ink-900)' }}>{contextOrg?.status || '-'}</p>
            </div>
            <div style={{ background: '#f4f8fc', border: '1px solid #dde7f1', borderRadius: '0.85rem', padding: '0.85rem' }}>
              <label className="form-label">Location</label>
              <p style={{ margin: '0.2rem 0 0', fontSize: '1.6rem', fontWeight: '700', color: 'var(--ink-900)' }}>{contextOrg?.location || '-'}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default OrgOverviewPage;
