/**
 * OrgRequestsPage.jsx - Organization requests page
 */

import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getOrganizationRequests, reviewRequest } from '../../services/requestService.js';
import PageHeader from '../../components/shared/PageHeader';
import ErrorState from '../../components/shared/ErrorState';
import EmptyState from '../../components/shared/EmptyState';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ForbiddenState from '../../components/shared/ForbiddenState';

function formatDateTime(value) {
  if (!value) return '-';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';
  return date.toLocaleString();
}

function OrgRequestsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');
  const { permissions, isMember } = useOrgContext();

  const [requests, setRequests] = useState([]);
  const [statusFilter, setStatusFilter] = useState('All');
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const canView = permissions.includes('org.requests.view');
  const canReview = permissions.includes('org.requests.review') || permissions.includes('org.requests.approve');

  useEffect(() => {
    if (!orgId || !isMember || !canView) return;

    async function loadRequests() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getOrganizationRequests(orgId);
        setRequests(data);
      } catch (err) {
        setError(err.message || 'Failed to load requests');
      } finally {
        setIsLoading(false);
      }
    }

    loadRequests();
  }, [orgId, isMember, canView]);

  const filteredRequests = useMemo(() => {
    if (statusFilter === 'All') return requests;
    return requests.filter((item) => item.status === statusFilter);
  }, [requests, statusFilter]);

  const summary = useMemo(() => {
    const pending = requests.filter((r) => r.status === 'Pending').length;
    const approved = requests.filter((r) => r.status === 'Approved').length;
    const rejected = requests.filter((r) => r.status === 'Rejected').length;
    return { pending, approved, rejected, total: requests.length };
  }, [requests]);

  const handleReview = async (requestId, decision) => {
    if (!canReview) return;

    const reviewNote = window.prompt(
      `${decision} request${decision === 'Rejected' ? ' - nhập lý do (optional)' : ' - ghi chú (optional)'}`,
      ''
    );

    if (reviewNote === null) {
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await reviewRequest(requestId, { decision, reviewNote });
      setRequests((prev) => prev.map((item) => (item.id === requestId ? updated : item)));
    } catch (err) {
      alert(err.message || 'Failed to review request');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader
          title="Requests"
          description="Manage organization join requests"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  if (!canView) {
    return (
      <div className="app-page">
        <PageHeader
          title="Requests"
          description="Manage organization join requests"
        />
        <ForbiddenState message="You do not have permission to view requests" />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Requests"
          description="Manage organization join requests"
        />
        <LoadingSpinner message="Loading requests..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Requests"
          description="Manage organization join requests"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Requests"
        description="Manage organization join requests"
      />
      <div className="app-section">
        <div className="app-card">
          <div className="app-section-header" style={{ marginBottom: '1rem' }}>
            <h3 className="app-section-title">Request Summary</h3>
          </div>
          <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
            <span className="app-badge app-badge--info">Total: {summary.total}</span>
            <span className="app-badge app-badge--warning">Pending: {summary.pending}</span>
            <span className="app-badge app-badge--success">Approved: {summary.approved}</span>
            <span className="app-badge">Rejected: {summary.rejected}</span>
          </div>
        </div>

        <div className="app-card">
          <div className="app-section-header" style={{ marginBottom: '1rem' }}>
            <h3 className="app-section-title">Filters</h3>
          </div>
          <div className="form-group" style={{ maxWidth: '220px' }}>
            <label className="form-label" htmlFor="statusFilter">Status</label>
            <select
              id="statusFilter"
              className="form-select"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <option value="All">All</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
              <option value="Cancelled">Cancelled</option>
              <option value="Closed">Closed</option>
            </select>
          </div>
        </div>

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Organization Requests</h3>
          </div>
          {filteredRequests.length === 0 ? (
            <EmptyState message="No requests found" />
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Sender</th>
                  <th>Type</th>
                  <th>Content</th>
                  <th>Desired</th>
                  <th>Status</th>
                  <th>Created</th>
                  <th>Reviewed</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredRequests.map((item) => (
                  <tr key={item.id}>
                    <td>
                      <div>{item.senderName}</div>
                      <div style={{ color: '#6b7280', fontSize: '0.875rem' }}>{item.senderEmail || '-'}</div>
                    </td>
                    <td>{item.requestType}</td>
                    <td style={{ maxWidth: '320px' }}>
                      <div style={{ fontWeight: 600 }}>{item.title || '-'}</div>
                      <div>{item.content}</div>
                    </td>
                    <td>
                      <div>{item.desiredDepartmentName || '-'}</div>
                      <div style={{ color: '#6b7280', fontSize: '0.875rem' }}>{item.desiredPosition || '-'}</div>
                    </td>
                    <td>
                      <span className={`app-badge ${item.status === 'Approved' ? 'app-badge--success' : item.status === 'Pending' ? 'app-badge--warning' : ''}`}>
                        {item.status}
                      </span>
                    </td>
                    <td>{formatDateTime(item.createdAtUtc)}</td>
                    <td>
                      <div>{formatDateTime(item.reviewedAt)}</div>
                      <div style={{ color: '#6b7280', fontSize: '0.875rem' }}>{item.reviewedByMemberName || '-'}</div>
                      {item.reviewNote ? <div style={{ fontSize: '0.875rem' }}>Note: {item.reviewNote}</div> : null}
                    </td>
                    <td>
                      {canReview && item.status === 'Pending' ? (
                        <div className="app-action-row">
                          <button
                            className="app-button app-button--primary"
                            disabled={isSubmitting}
                            onClick={() => handleReview(item.id, 'Approved')}
                          >
                            Approve
                          </button>
                          <button
                            className="app-button app-button--danger"
                            disabled={isSubmitting}
                            onClick={() => handleReview(item.id, 'Rejected')}
                          >
                            Reject
                          </button>
                        </div>
                      ) : (
                        '-'
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
}

export default OrgRequestsPage;
