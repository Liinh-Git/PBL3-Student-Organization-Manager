import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth.js';
import LoadingSpinner from '../../components/shared/LoadingSpinner.jsx';
import ErrorState from '../../components/shared/ErrorState.jsx';
import { getEventById, getPublicEventById, updateEvent } from '../../services/eventService.js';
import { getMyPermissions } from '../../services/roleService.js';
import { getMyEventRegistration, joinEvent } from '../../services/attendeeService.js';
import './EventDetailPage.css';

function toAbsoluteMediaUrl(url) {
  if (!url) return '';
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
  const origin = apiBase.replace(/\/api\/?$/, '');
  return url.startsWith('/') ? `${origin}${url}` : `${origin}/${url}`;
}

function toDateTimeLocalInput(value) {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

function toIsoUtcFromLocalInput(value) {
  if (!value) return null;
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return null;
  return date.toISOString();
}

function formatDateTime(value) {
  if (!value) return '-';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';
  return date.toLocaleString();
}

function formatMaybe(value) {
  if (value === null || value === undefined || value === '') return '-';
  return String(value);
}

function buildDraft(eventData) {
  return {
    eventName: eventData?.name || eventData?.eventName || '',
    description: eventData?.description || '',
    startDate: toDateTimeLocalInput(eventData?.startDate),
    endDate: toDateTimeLocalInput(eventData?.endDate),
    location: eventData?.location || '',
    bannerUrl: eventData?.bannerUrl || '',
    visibility: eventData?.visibility || 'Private',
    targetParticipants: eventData?.targetParticipants ?? ''
  };
}

function EventDetailPage() {
  const { eventId } = useParams();
  const navigate = useNavigate();
  const { isAuthenticated, isLoading: authLoading } = useAuth();

  const [eventData, setEventData] = useState(null);
  const [sourceMode, setSourceMode] = useState('public');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [permissions, setPermissions] = useState([]);
  const [viewMode, setViewMode] = useState('preview');
  const [draft, setDraft] = useState(null);
  const [editingField, setEditingField] = useState(null);
  const [isSaving, setIsSaving] = useState(false);
  const [joinState, setJoinState] = useState({ isRegistered: false, status: null });
  const [isJoining, setIsJoining] = useState(false);

  useEffect(() => {
    if (authLoading) return;
    if (!eventId) {
      setError('Event ID is missing');
      setIsLoading(false);
      return;
    }

    let isMounted = true;
    async function loadEventDetail() {
      setIsLoading(true);
      setError(null);
      setEditingField(null);

      try {
        let loadedEvent = null;
        let mode = 'public';

        if (isAuthenticated) {
          try {
            loadedEvent = await getEventById(eventId);
            mode = 'workspace';
          } catch (workspaceError) {
            const status = workspaceError?.response?.status;
            if (status !== 403 && status !== 404) {
              // ignore here and try public route next
            }
          }
        }

        if (!loadedEvent) {
          loadedEvent = await getPublicEventById(eventId);
          mode = 'public';
        }

        const nextPermissions = [];
        if (isAuthenticated && loadedEvent?.organizationId) {
          try {
            const permissionData = await getMyPermissions(loadedEvent.organizationId);
            if (Array.isArray(permissionData?.permissionKeys)) {
              nextPermissions.push(...permissionData.permissionKeys);
            }
          } catch {
            // Non-member or no permission endpoint access => keep empty list.
          }
        }

        let registration = { isRegistered: false, status: null };
        if (isAuthenticated) {
          try {
            const response = await getMyEventRegistration(eventId);
            registration = {
              isRegistered: !!response?.isRegistered && response?.status !== 'Cancelled',
              status: response?.status || null
            };
          } catch {
            registration = { isRegistered: false, status: null };
          }
        }

        if (!isMounted) return;
        setEventData(loadedEvent);
        setDraft(buildDraft(loadedEvent));
        setPermissions(nextPermissions);
        setJoinState(registration);
        setSourceMode(mode);
      } catch (err) {
        if (!isMounted) return;
        setError(err.message || 'Failed to load event detail');
      } finally {
        if (isMounted) setIsLoading(false);
      }
    }

    loadEventDetail();

    return () => {
      isMounted = false;
    };
  }, [authLoading, eventId, isAuthenticated]);

  const canEditEvent = useMemo(() => {
    return permissions.includes('org.events.create') && permissions.includes('org.events.manage');
  }, [permissions]);

  const canJoin = useMemo(() => {
    if (!eventData) return false;
    const status = String(eventData.status || '');
    return !['Cancelled', 'Archived', 'Completed'].includes(status);
  }, [eventData]);

  useEffect(() => {
    if (!canEditEvent && viewMode === 'edit') {
      setViewMode('preview');
      setEditingField(null);
    }
  }, [canEditEvent, viewMode]);

  const handleSave = async () => {
    if (!canEditEvent || !draft) return;
    if (!draft.eventName?.trim()) {
      alert('Event name is required.');
      return;
    }

    const startDateIso = toIsoUtcFromLocalInput(draft.startDate);
    const endDateIso = toIsoUtcFromLocalInput(draft.endDate) || startDateIso;
    if (!startDateIso || !endDateIso) {
      alert('Start date and end date are required.');
      return;
    }

    setIsSaving(true);
    try {
      const updated = await updateEvent(eventId, {
        eventName: draft.eventName.trim(),
        description: draft.description || null,
        startDate: startDateIso,
        endDate: endDateIso,
        location: draft.location || null,
        bannerUrl: draft.bannerUrl || null,
        visibility: draft.visibility || 'Private',
        targetParticipants:
          draft.targetParticipants === '' || draft.targetParticipants === null || draft.targetParticipants === undefined
            ? null
            : Number(draft.targetParticipants)
      });

      setEventData(updated);
      setDraft(buildDraft(updated));
      setViewMode('preview');
      setEditingField(null);
      setSourceMode('workspace');
    } catch (err) {
      alert(err.message || 'Failed to update event');
    } finally {
      setIsSaving(false);
    }
  };

  const handleJoin = async () => {
    if (!isAuthenticated || !canJoin) return;
    setIsJoining(true);
    try {
      const response = await joinEvent(eventId);
      setJoinState({
        isRegistered: !!response?.isRegistered,
        status: response?.status || 'Registered'
      });
    } catch (err) {
      alert(err.message || 'Failed to join event');
    } finally {
      setIsJoining(false);
    }
  };

  const bannerSrc = toAbsoluteMediaUrl(eventData?.bannerUrl);

  if (isLoading) {
    return (
      <div className="event-detail-page">
        <LoadingSpinner message="Loading event detail..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="event-detail-page">
        <button type="button" onClick={() => navigate(-1)} className="app-button app-button--ghost event-detail-back-btn">
          ← Back
        </button>
        <ErrorState message={error} />
      </div>
    );
  }

  if (!eventData || !draft) {
    return (
      <div className="event-detail-page">
        <ErrorState message="Event not found" />
      </div>
    );
  }

  const renderEditableField = ({ keyName, label, type = 'text', options = [] }) => {
    const isEditing = viewMode === 'edit' && canEditEvent && editingField === keyName;
    const value = draft[keyName] ?? '';
    const isClickable = viewMode === 'edit' && canEditEvent;

    return (
      <div className="event-detail-field" key={keyName}>
        <div className="event-detail-label">{label}</div>
        {isEditing ? (
          type === 'textarea' ? (
            <textarea
              className="form-input event-detail-input"
              value={value}
              autoFocus
              rows={4}
              onChange={(e) => setDraft((prev) => ({ ...prev, [keyName]: e.target.value }))}
              onBlur={() => setEditingField(null)}
            />
          ) : type === 'select' ? (
            <select
              className="form-select event-detail-input"
              value={value}
              autoFocus
              onChange={(e) => setDraft((prev) => ({ ...prev, [keyName]: e.target.value }))}
              onBlur={() => setEditingField(null)}
            >
              {options.map((opt) => (
                <option key={opt} value={opt}>
                  {opt}
                </option>
              ))}
            </select>
          ) : (
            <input
              className="form-input event-detail-input"
              type={type}
              value={value}
              autoFocus
              onChange={(e) => setDraft((prev) => ({ ...prev, [keyName]: e.target.value }))}
              onBlur={() => setEditingField(null)}
            />
          )
        ) : (
          <button
            type="button"
            className={`event-detail-value ${isClickable ? 'event-detail-value--editable' : ''}`}
            onClick={() => {
              if (isClickable) setEditingField(keyName);
            }}
          >
            {type === 'datetime-local' ? formatDateTime(toIsoUtcFromLocalInput(value)) : formatMaybe(value)}
          </button>
        )}
      </div>
    );
  };

  return (
    <div className="event-detail-page app-page">
      <button type="button" onClick={() => navigate(-1)} className="app-button app-button--ghost event-detail-back-btn">
        ← Back
      </button>

      <section className="app-card event-detail-hero">
        <div className="event-detail-hero-header">
          <div>
            <h1 className="app-page-title">{eventData.name || 'Event Detail'}</h1>
            <p className="app-page-subtitle">
              Mode source: {sourceMode} | Status: {eventData.status || '-'} | Visibility: {eventData.visibility || '-'}
            </p>
          </div>
          <div className="app-action-row">
            {canEditEvent ? (
              <>
                <button
                  type="button"
                  className={`app-button ${viewMode === 'preview' ? 'app-button--secondary' : 'app-button--ghost'}`}
                  onClick={() => {
                    setViewMode('preview');
                    setEditingField(null);
                    setDraft(buildDraft(eventData));
                  }}
                >
                  Preview
                </button>
                <button
                  type="button"
                  className={`app-button ${viewMode === 'edit' ? 'app-button--secondary' : 'app-button--ghost'}`}
                  onClick={() => setViewMode('edit')}
                >
                  Edit
                </button>
                {viewMode === 'edit' ? (
                  <button type="button" className="app-button app-button--primary" onClick={handleSave} disabled={isSaving}>
                    {isSaving ? 'Saving...' : 'Save changes'}
                  </button>
                ) : null}
              </>
            ) : null}

            {isAuthenticated ? (
              joinState.isRegistered ? (
                <span className="app-badge app-badge--success">Joined ({joinState.status || 'Registered'})</span>
              ) : (
                <button type="button" className="app-button app-button--primary" onClick={handleJoin} disabled={isJoining || !canJoin}>
                  {isJoining ? 'Joining...' : 'Join event'}
                </button>
              )
            ) : (
              <button type="button" className="app-button app-button--primary" onClick={() => navigate('/login')}>
                Login to join
              </button>
            )}
          </div>
        </div>

        {bannerSrc ? (
          <img
            src={bannerSrc}
            alt={`${eventData.name || 'Event'} banner`}
            className="event-detail-banner"
            onError={(e) => {
              e.currentTarget.style.display = 'none';
            }}
          />
        ) : null}
      </section>

      <section className="app-card app-section">
        <div className="app-section-header">
          <h2 className="app-section-title">Editable Fields</h2>
          <p className="app-section-subtitle">
            {canEditEvent
              ? 'In edit mode, click any field below to edit directly on this page.'
              : 'You are in preview mode only.'}
          </p>
        </div>
        <div className="event-detail-grid">
          {renderEditableField({ keyName: 'eventName', label: 'Event Name' })}
          {renderEditableField({ keyName: 'description', label: 'Description', type: 'textarea' })}
          {renderEditableField({ keyName: 'startDate', label: 'Start Date', type: 'datetime-local' })}
          {renderEditableField({ keyName: 'endDate', label: 'End Date', type: 'datetime-local' })}
          {renderEditableField({ keyName: 'location', label: 'Location' })}
          {renderEditableField({ keyName: 'bannerUrl', label: 'Banner URL' })}
          {renderEditableField({
            keyName: 'visibility',
            label: 'Visibility',
            type: 'select',
            options: ['Public', 'OrganizationOnly', 'Private']
          })}
          {renderEditableField({ keyName: 'targetParticipants', label: 'Target Participants', type: 'number' })}
        </div>
      </section>

      <section className="app-card app-section">
        <div className="app-section-header">
          <h2 className="app-section-title">Read-Only Metadata</h2>
        </div>
        <div className="event-detail-grid">
          <div className="event-detail-field">
            <div className="event-detail-label">Event ID</div>
            <div className="event-detail-value event-detail-value--readonly">{formatMaybe(eventData.id || eventId)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Organization ID</div>
            <div className="event-detail-value event-detail-value--readonly">{formatMaybe(eventData.organizationId)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Organization Name</div>
            <div className="event-detail-value event-detail-value--readonly">{formatMaybe(eventData.organizationName)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Status</div>
            <div className="event-detail-value event-detail-value--readonly">{formatMaybe(eventData.status)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Average Rating</div>
            <div className="event-detail-value event-detail-value--readonly">{formatMaybe(eventData.averageRating)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Budget</div>
            <div className="event-detail-value event-detail-value--readonly">{formatMaybe(eventData.budget)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Tags</div>
            <div className="event-detail-value event-detail-value--readonly">{formatMaybe(eventData.tags)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Created At (UTC)</div>
            <div className="event-detail-value event-detail-value--readonly">{formatDateTime(eventData.createdAtUtc)}</div>
          </div>
          <div className="event-detail-field">
            <div className="event-detail-label">Updated At (UTC)</div>
            <div className="event-detail-value event-detail-value--readonly">{formatDateTime(eventData.updatedAtUtc)}</div>
          </div>
        </div>
      </section>

      <section className="app-card app-section">
        <div className="app-section-header">
          <h2 className="app-section-title">Debug Snapshot</h2>
        </div>
        <pre className="event-detail-json">{JSON.stringify(eventData, null, 2)}</pre>
      </section>
    </div>
  );
}

export default EventDetailPage;
