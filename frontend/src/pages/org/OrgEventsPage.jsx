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
  const getEventId = (event) => event?.id || event?.eventId;
  const getEventName = (event) => event?.name || event?.eventName;

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
    const startTime = form.startTime.value; // repurposed endDate field label but actually we need to handle this
    const location = form.location.value;
    const targetParticipants = form.targetParticipants.value;
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
        startDate: `${startDate}T${startTime || '00:00'}:00Z`, // Combine date and time
        location: location || undefined,
        targetParticipants: targetParticipants ? parseInt(targetParticipants) : undefined,
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
      const editingEventId = getEventId(editingEvent);
      if (!editingEventId) {
        alert('Event ID is missing');
        return;
      }
      const updated = await updateEvent(editingEventId, {
        eventName,
        description: description || undefined,
        startDate: `${form.startDate.value}T${form.startTime.value || '00:00'}:00Z`,
        location: location || undefined,
        targetParticipants: form.targetParticipants.value ? parseInt(form.targetParticipants.value) : undefined,
        bannerUrl: bannerUrl || undefined,
        visibility
      });
      setEvents(prev => prev.map(ev => (getEventId(ev) === editingEventId ? updated : ev)));
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
      setEvents(prev => prev.filter(ev => getEventId(ev) !== eventId));
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
                <label htmlFor="startDate" className="form-label">Ngày tổ chức *</label>
                <input
                  id="startDate"
                  name="startDate"
                  type="date"
                  className="form-input"
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="startTime" className="form-label">Giờ bắt đầu</label>
                <input
                  id="startTime"
                  name="startTime"
                  type="time"
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label htmlFor="targetParticipants" className="form-label">Số lượng tham gia</label>
                <input
                  id="targetParticipants"
                  name="targetParticipants"
                  type="number"
                  className="form-input"
                  placeholder="Ví dụ: 100"
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
                  defaultValue={getEventName(editingEvent)}
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
                <label htmlFor="editStartDate" className="form-label">Ngày tổ chức *</label>
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
                <label htmlFor="editStartTime" className="form-label">Giờ bắt đầu</label>
                <input
                  id="editStartTime"
                  name="startTime"
                  type="time"
                  className="form-input"
                  defaultValue={editingEvent.startDate && editingEvent.startDate.includes('T') ? editingEvent.startDate.split('T')[1].substring(0, 5) : '00:00'}
                />
              </div>
              <div className="form-group">
                <label htmlFor="editTargetParticipants" className="form-label">Số lượng tham gia</label>
                <input
                  id="editTargetParticipants"
                  name="targetParticipants"
                  type="number"
                  className="form-input"
                  defaultValue={editingEvent.targetParticipants || ''}
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
                  <th>Ngày tổ chức</th>
                  <th>Giờ bắt đầu</th>
                  <th>Số lượng</th>
                  <th>Status</th>
                  <th>Visibility</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {events.map((event) => (
                  <tr key={getEventId(event)}>
                    <td>{getEventName(event) || '-'}</td>
                    <td>{event.description || '-'}</td>
                    <td>{event.startDate ? new Date(event.startDate).toLocaleDateString() : '-'}</td>
                    <td>{event.startDate && event.startDate.includes('T') ? event.startDate.split('T')[1].substring(0, 5) : '-'}</td>
                    <td>{event.targetParticipants || '-'}</td>
                    <td><span className="app-badge app-badge--success">{event.status || '-'}</span></td>
                    <td>{event.visibility || '-'}</td>
                    <td>
                      <div className="app-action-row">
                        <button 
                          onClick={() => {
                            const selectedEventId = getEventId(event);
                            if (!selectedEventId) {
                              alert('Event ID is missing');
                              return;
                            }
                            navigate(`/org/events/${selectedEventId}?orgId=${orgId}`);
                          }}
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
                              onClick={() => handleDelete(getEventId(event))}
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
