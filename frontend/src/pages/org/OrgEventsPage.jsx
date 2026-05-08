/**
 * OrgEventsPage.jsx - Organization events page
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getOrganizationEvents, createEvent, updateEvent, deleteEvent } from '../../services/eventService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import ForbiddenState from '../../components/shared/ForbiddenState';

function OrgEventsPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const orgId = searchParams.get('orgId');
  const { permissions, isMember } = useOrgContext();

  const [events, setEvents] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingEvent, setEditingEvent] = useState(null);

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadEvents() {
      setIsLoading(true);
      try {
        const data = await getOrganizationEvents(orgId);
        setEvents(data);
      } catch (err) {
        setError(err.message || 'Failed to load events');
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, [orgId, isMember]);

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader
          title="Events"
          description="Manage organization events"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  const canCreate = permissions.includes('org.events.create');
  const canManage = permissions.includes('org.events.manage');

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!canCreate) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const eventName = form.eventName.value;
    const description = form.description.value;
    const startDate = form.startDate.value;
    const endDate = form.endDate.value;
    const location = form.location.value;
    const bannerUrl = form.bannerUrl.value;
    const visibility = form.visibility.value;
    
    if (!eventName || !startDate) {
      alert('Event name and start date are required');
      return;
    }

    setIsSubmitting(true);
    try {
      const newEvent = await createEvent(orgId, {
        eventName,
        description: description || undefined,
        startDate,
        endDate: endDate || undefined,
        location: location || undefined,
        bannerUrl: bannerUrl || undefined,
        visibility
      });
      setEvents(prev => [...prev, newEvent]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || 'Failed to create event');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canManage || !editingEvent) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const eventName = form.eventName.value;
    const description = form.description.value;
    const startDate = form.startDate.value;
    const endDate = form.endDate.value;
    const location = form.location.value;
    const bannerUrl = form.bannerUrl.value;
    const visibility = form.visibility.value;
    
    if (!eventName || !startDate) {
      alert('Event name and start date are required');
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await updateEvent(editingEvent.id, {
        eventName,
        description: description || undefined,
        startDate,
        endDate: endDate || undefined,
        location: location || undefined,
        bannerUrl: bannerUrl || undefined,
        visibility
      });
      setEvents(prev => prev.map(ev => ev.id === editingEvent.id ? updated : ev));
      setEditingEvent(null);
    } catch (err) {
      alert(err.message || 'Failed to update event');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (eventId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    if (!window.confirm('Are you sure you want to delete this event? This will also delete all milestones, categories, and tasks within it.')) {
      return;
    }

    setIsSubmitting(true);
    try {
      await deleteEvent(eventId);
      setEvents(prev => prev.filter(ev => ev.id !== eventId));
    } catch (err) {
      alert(err.message || 'Failed to delete event');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Events"
          description="Manage organization events"
          actions={canCreate && <button disabled className="app-button app-button--primary">Create Event</button>}
        />
        <LoadingSpinner message="Loading events..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Events"
          description="Manage organization events"
          actions={canCreate && <button disabled className="app-button app-button--primary">Create Event</button>}
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Events"
        description="Manage organization events"
        actions={
          canCreate && (
            <button 
              onClick={() => setShowCreateForm(true)}
              className="app-button app-button--primary"
            >
              Create Event
            </button>
          )
        }
      />

      {showCreateForm && canCreate && (
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Create Event</h3>
          </div>
          <form onSubmit={handleCreate} className="auth-form">
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: '0.9rem' }}>
              <div className="form-group">
                <label htmlFor="eventName" className="form-label">Event Name *</label>
                <input
                  id="eventName"
                  name="eventName"
                  className="form-input"
                  placeholder="Event name"
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="description" className="form-label">Description</label>
                <input
                  id="description"
                  name="description"
                  className="form-input"
                  placeholder="Description"
                />
              </div>
              <div className="form-group">
                <label htmlFor="startDate" className="form-label">Start Date *</label>
                <input
                  id="startDate"
                  name="startDate"
                  type="date"
                  className="form-input"
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="endDate" className="form-label">End Date</label>
                <input
                  id="endDate"
                  name="endDate"
                  type="date"
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label htmlFor="location" className="form-label">Location</label>
                <input
                  id="location"
                  name="location"
                  className="form-input"
                  placeholder="Location"
                />
              </div>
              <div className="form-group">
                <label htmlFor="bannerUrl" className="form-label">Banner URL</label>
                <input
                  id="bannerUrl"
                  name="bannerUrl"
                  className="form-input"
                  placeholder="Banner URL"
                />
              </div>
              <div className="form-group">
                <label htmlFor="visibility" className="form-label">Visibility</label>
                <select id="visibility" name="visibility" defaultValue="Private" className="form-select">
                  <option value="Public">Public</option>
                  <option value="OrganizationOnly">Organization Only</option>
                  <option value="Private">Private</option>
                </select>
              </div>
            </div>
            <div className="app-action-row">
              <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                {isSubmitting ? 'Creating...' : 'Create'}
              </button>
              <button type="button" onClick={() => setShowCreateForm(false)} className="app-button app-button--ghost">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {editingEvent && canManage && (
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Edit Event</h3>
          </div>
          <form onSubmit={handleUpdate} className="auth-form">
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: '0.9rem' }}>
              <div className="form-group">
                <label htmlFor="editEventName" className="form-label">Event Name *</label>
                <input
                  id="editEventName"
                  name="eventName"
                  className="form-input"
                  defaultValue={editingEvent.name}
                  placeholder="Event name"
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="editDescription" className="form-label">Description</label>
                <input
                  id="editDescription"
                  name="description"
                  className="form-input"
                  defaultValue={editingEvent.description || ''}
                  placeholder="Description"
                />
              </div>
              <div className="form-group">
                <label htmlFor="editStartDate" className="form-label">Start Date *</label>
                <input
                  id="editStartDate"
                  name="startDate"
                  type="date"
                  className="form-input"
                  defaultValue={editingEvent.startDate ? editingEvent.startDate.split('T')[0] : ''}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="editEndDate" className="form-label">End Date</label>
                <input
                  id="editEndDate"
                  name="endDate"
                  type="date"
                  className="form-input"
                  defaultValue={editingEvent.endDate ? editingEvent.endDate.split('T')[0] : ''}
                />
              </div>
              <div className="form-group">
                <label htmlFor="editLocation" className="form-label">Location</label>
                <input
                  id="editLocation"
                  name="location"
                  className="form-input"
                  defaultValue={editingEvent.location || ''}
                  placeholder="Location"
                />
              </div>
              <div className="form-group">
                <label htmlFor="editBannerUrl" className="form-label">Banner URL</label>
                <input
                  id="editBannerUrl"
                  name="bannerUrl"
                  className="form-input"
                  defaultValue={editingEvent.bannerUrl || ''}
                  placeholder="Banner URL"
                />
              </div>
              <div className="form-group">
                <label htmlFor="editVisibility" className="form-label">Visibility</label>
                <select id="editVisibility" name="visibility" defaultValue={editingEvent.visibility || 'Private'} className="form-select">
                  <option value="Public">Public</option>
                  <option value="OrganizationOnly">Organization Only</option>
                  <option value="Private">Private</option>
                </select>
              </div>
            </div>
            <div className="app-action-row">
              <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                {isSubmitting ? 'Updating...' : 'Update'}
              </button>
              <button type="button" onClick={() => setEditingEvent(null)} className="app-button app-button--ghost">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="app-section">
        {events.length === 0 ? (
          <EmptyState message="No events found" />
        ) : (
          <div className="app-card">
            <table>
              <thead>
                <tr>
                  <th>Event Name</th>
                  <th>Description</th>
                  <th>Start Date</th>
                  <th>End Date</th>
                  <th>Status</th>
                  <th>Visibility</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {events.map((event) => (
                  <tr key={event.id}>
                    <td>{event.name || '-'}</td>
                    <td>{event.description || '-'}</td>
                    <td>{event.startDate ? new Date(event.startDate).toLocaleDateString() : '-'}</td>
                    <td>{event.endDate ? new Date(event.endDate).toLocaleDateString() : '-'}</td>
                    <td><span className="app-badge app-badge--success">{event.status || '-'}</span></td>
                    <td>{event.visibility || '-'}</td>
                    <td>
                      <div className="app-action-row">
                        <button 
                          onClick={() => navigate(`/org/events/${event.id}?orgId=${orgId}`)}
                          className="app-button app-button--primary"
                        >
                          View
                        </button>
                        {canManage && (
                          <>
                            <button
                              onClick={() => setEditingEvent(event)}
                              disabled={isSubmitting}
                              className="app-button app-button--secondary"
                            >
                              Edit
                            </button>
                            <button
                              onClick={() => handleDelete(event.id)}
                              disabled={isSubmitting}
                              className="app-button app-button--danger"
                            >
                              Delete
                            </button>
                          </>
                        )}
                      </div>
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

export default OrgEventsPage;
