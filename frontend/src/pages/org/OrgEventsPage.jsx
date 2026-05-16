/**
 * OrgEventsPage.jsx - Organization events page
 *
 * UI refactor: card-grid workspace launcher, giữ nguyên API/permission/handler flow.
 */

import { useRef, useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getOrganizationEvents, createEvent, updateEvent, deleteEvent, uploadEventBanner } from '../../services/eventService.js';
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

  const toAbsoluteMediaUrl = (url) => {
    if (!url) return '';
    if (/^https?:\/\//i.test(url)) return url;
    const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
    const origin = apiBase.replace(/\/api\/?$/, '');
    return url.startsWith('/') ? `${origin}${url}` : `${origin}/${url}`;
  };

  const formatDate = (value) => {
    if (!value) return '-';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '-';
    return date.toLocaleDateString('vi-VN');
  };

  const formatTime = (value) => {
    if (!value || !String(value).includes('T')) return '-';
    return String(value).split('T')[1].substring(0, 5);
  };

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
    const startTime = form.startTime.value;
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
        startDate: `${startDate}T${startTime || '00:00'}:00Z`,
        location: location || undefined,
        targetParticipants: targetParticipants ? parseInt(targetParticipants, 10) : undefined,
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
    const startTime = form.startTime.value;
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
      const editingEventId = getEventId(editingEvent);
      if (!editingEventId) {
        alert('Event ID is missing');
        return;
      }
      const updated = await updateEvent(editingEventId, {
        eventName,
        description: description || undefined,
        startDate: `${startDate}T${startTime || '00:00'}:00Z`,
        location: location || undefined,
        targetParticipants: targetParticipants ? parseInt(targetParticipants, 10) : undefined,
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

  const closeForms = () => {
    setShowCreateForm(false);
    setEditingEvent(null);
  };

  const openWorkspace = (event) => {
    const selectedEventId = getEventId(event);
    if (!selectedEventId) {
      alert('Event ID is missing');
      return;
    }
    navigate(`/org/events/${selectedEventId}?orgId=${orgId}`);
  };

  const EventForm = ({ mode, event }) => {
    const isEdit = mode === 'edit';
    const fileInputRef = useRef(null);
    const [bannerValue, setBannerValue] = useState(isEdit ? event?.bannerUrl || '' : '');
    const [pendingBannerUrl, setPendingBannerUrl] = useState('');
    const [isUploadingBanner, setIsUploadingBanner] = useState(false);

    const handleBannerUpload = async (uploadEvent) => {
      const file = uploadEvent.target.files?.[0];
      if (!file) return;

      setIsUploadingBanner(true);
      try {
        const uploadedUrl = await uploadEventBanner(file);
        setPendingBannerUrl(uploadedUrl || '');
      } catch (err) {
        alert(err.message || 'Failed to upload event banner');
      } finally {
        setIsUploadingBanner(false);
        uploadEvent.target.value = '';
      }
    };

    const acceptPendingBanner = () => {
      setBannerValue(pendingBannerUrl);
      setPendingBannerUrl('');
    };

    const rejectPendingBanner = () => {
      setPendingBannerUrl('');
    };

    return (
      <div className="org-event-form-panel">
        <div className="org-event-form-header">
          <div>
            <p className="org-eyebrow">{isEdit ? 'Cập nhật dự án' : 'Tạo dự án'}</p>
            <h2>{isEdit ? 'Sửa sự kiện' : 'Tạo sự kiện mới'}</h2>
          </div>
          <button type="button" onClick={closeForms} className="org-icon-button" aria-label="Đóng form">
            ×
          </button>
        </div>

        <form onSubmit={isEdit ? handleUpdate : handleCreate} className="org-event-form-grid">
          <div className="form-group">
            <label htmlFor={isEdit ? 'editEventName' : 'eventName'} className="form-label">Event Name *</label>
            <input
              id={isEdit ? 'editEventName' : 'eventName'}
              name="eventName"
              className="form-input"
              defaultValue={isEdit ? getEventName(event) : ''}
              placeholder="Event name"
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor={isEdit ? 'editDescription' : 'description'} className="form-label">Description</label>
            <input
              id={isEdit ? 'editDescription' : 'description'}
              name="description"
              className="form-input"
              defaultValue={isEdit ? event?.description || '' : ''}
              placeholder="Description"
            />
          </div>
          <div className="form-group">
            <label htmlFor={isEdit ? 'editStartDate' : 'startDate'} className="form-label">Ngày tổ chức *</label>
            <input
              id={isEdit ? 'editStartDate' : 'startDate'}
              name="startDate"
              type="date"
              className="form-input"
              defaultValue={isEdit && event?.startDate ? String(event.startDate).split('T')[0] : ''}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor={isEdit ? 'editStartTime' : 'startTime'} className="form-label">Giờ bắt đầu</label>
            <input
              id={isEdit ? 'editStartTime' : 'startTime'}
              name="startTime"
              type="time"
              className="form-input"
              defaultValue={isEdit && event?.startDate && String(event.startDate).includes('T') ? String(event.startDate).split('T')[1].substring(0, 5) : '00:00'}
            />
          </div>
          <div className="form-group">
            <label htmlFor={isEdit ? 'editTargetParticipants' : 'targetParticipants'} className="form-label">Số lượng tham gia</label>
            <input
              id={isEdit ? 'editTargetParticipants' : 'targetParticipants'}
              name="targetParticipants"
              type="number"
              className="form-input"
              defaultValue={isEdit ? event?.targetParticipants || '' : ''}
              placeholder="Ví dụ: 100"
            />
          </div>
          <div className="form-group">
            <label htmlFor={isEdit ? 'editLocation' : 'location'} className="form-label">Location</label>
            <input
              id={isEdit ? 'editLocation' : 'location'}
              name="location"
              className="form-input"
              defaultValue={isEdit ? event?.location || '' : ''}
              placeholder="Location"
            />
          </div>
          <div className="form-group">
            <label htmlFor={isEdit ? 'editBannerUrl' : 'bannerUrl'} className="form-label">Banner URL</label>
            <input
              id={isEdit ? 'editBannerUrl' : 'bannerUrl'}
              name="bannerUrl"
              className="form-input"
              value={bannerValue}
              onChange={(e) => setBannerValue(e.target.value)}
              placeholder="Banner URL"
            />
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={handleBannerUpload}
              hidden
            />
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={isUploadingBanner}
              className="org-button org-button-ghost"
            >
              {isUploadingBanner ? 'Đang upload...' : 'Upload banner'}
            </button>
          </div>
          <div className="form-group">
            <label htmlFor={isEdit ? 'editVisibility' : 'visibility'} className="form-label">Visibility</label>
            <select
              id={isEdit ? 'editVisibility' : 'visibility'}
              name="visibility"
              defaultValue={isEdit ? event?.visibility || 'Private' : 'Private'}
              className="form-select"
            >
              <option value="Public">Public</option>
              <option value="OrganizationOnly">Organization Only</option>
              <option value="Private">Private</option>
            </select>
          </div>
          <div className="org-form-actions">
            <button type="submit" disabled={isSubmitting} className="org-button org-button-primary">
              {isSubmitting ? (isEdit ? 'Đang cập nhật...' : 'Đang tạo...') : (isEdit ? 'Lưu thay đổi' : 'Tạo sự kiện')}
            </button>
            <button type="button" onClick={closeForms} className="org-button org-button-ghost">
              Hủy
            </button>
          </div>
        </form>

        {pendingBannerUrl ? (
          <div className="org-upload-modal-backdrop" role="dialog" aria-modal="true" aria-label="Confirm event banner">
            <div className="org-upload-modal">
              <h3>Xác nhận chọn ảnh này?</h3>
              <img src={toAbsoluteMediaUrl(pendingBannerUrl)} alt="Ảnh banner vừa upload" />
              <p>{pendingBannerUrl}</p>
              <div className="org-upload-modal-actions">
                <button type="button" onClick={rejectPendingBanner} className="org-button org-button-ghost">
                  Hủy
                </button>
                <button type="button" onClick={acceptPendingBanner} className="org-button org-button-primary">
                  OK
                </button>
              </div>
            </div>
          </div>
        ) : null}
      </div>
    );
  };

  if (isLoading) {
    return (
      <div className="app-page org-events-page">
        <PageHeader
          title="Danh sách Dự án"
          description="Quản lý tổng quan các sự kiện và dự án của tổ chức."
          actions={canCreate && <button disabled className="app-button app-button--primary">Tạo sự kiện mới</button>}
        />
        <LoadingSpinner message="Loading events..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page org-events-page">
        <PageHeader
          title="Danh sách Dự án"
          description="Quản lý tổng quan các sự kiện và dự án của tổ chức."
          actions={canCreate && <button disabled className="app-button app-button--primary">Tạo sự kiện mới</button>}
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="org-events-page">
      <style>{`
        .org-events-page {
          min-height: 100vh;
          padding: 48px 34px;
          background: #F8FAFC;
          color: #0F172A;
        }

        .org-events-shell {
          width: min(1120px, 100%);
        }

        .org-events-header {
          display: flex;
          align-items: flex-start;
          justify-content: space-between;
          gap: 24px;
          margin-bottom: 38px;
        }

        .org-events-header h1 {
          margin: 0;
          color: #0F172A;
          font-size: clamp(28px, 3vw, 34px);
          line-height: 1.15;
          font-weight: 800;
          letter-spacing: -0.035em;
        }

        .org-events-header p {
          margin: 8px 0 0;
          color: #64748B;
          font-size: 14px;
        }

        .org-events-grid {
          display: grid;
          grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
          gap: 24px;
        }

        .org-event-card {
          display: flex;
          min-height: 304px;
          flex-direction: column;
          padding: 24px;
          background: #FFFFFF;
          border: 1px solid #DDE7F2;
          border-radius: 12px;
          box-shadow: 0 2px 4px rgba(15, 23, 42, 0.04), 0 10px 20px rgba(15, 23, 42, 0.03);
          transition: transform 160ms ease, box-shadow 160ms ease, border-color 160ms ease;
        }

        .org-event-card:hover {
          transform: translateY(-2px);
          border-color: #CBD5E1;
          box-shadow: 0 12px 28px rgba(15, 23, 42, 0.08);
        }

        .org-event-banner {
          width: 100%;
          max-height: 180px;
          object-fit: cover;
          margin-bottom: 16px;
          border-radius: 10px;
        }

        .org-event-card-top {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 12px;
          margin-bottom: 18px;
        }

        .org-status-badge {
          display: inline-flex;
          align-items: center;
          min-height: 24px;
          padding: 5px 9px;
          border-radius: 4px;
          background: #F1F5F9;
          color: #475569;
          font-size: 10px;
          line-height: 1;
          font-weight: 800;
          letter-spacing: 0.06em;
          text-transform: uppercase;
        }

        .org-event-card-actions {
          display: flex;
          align-items: center;
          gap: 6px;
        }

        .org-icon-button {
          display: inline-flex;
          width: 30px;
          height: 30px;
          align-items: center;
          justify-content: center;
          border: 0;
          border-radius: 8px;
          background: transparent;
          color: #94A3B8;
          font-size: 18px;
          font-weight: 800;
          cursor: pointer;
          transition: background 150ms ease, color 150ms ease;
        }

        .org-icon-button:hover {
          background: #F1F5F9;
          color: #0F172A;
        }

        .org-icon-button-danger:hover {
          background: #FEF2F2;
          color: #DC2626;
        }

        .org-event-card h2 {
          margin: 0 0 12px;
          color: #0F172A;
          font-size: 18px;
          line-height: 1.35;
          font-weight: 800;
          letter-spacing: -0.02em;
        }

        .org-event-description {
          flex: 1;
          margin: 0 0 24px;
          color: #475569;
          font-size: 14px;
          line-height: 1.55;
          display: -webkit-box;
          -webkit-line-clamp: 2;
          -webkit-box-orient: vertical;
          overflow: hidden;
        }

        .org-event-meta {
          display: grid;
          gap: 11px;
          margin-bottom: 24px;
          color: #334155;
          font-size: 14px;
        }

        .org-event-meta-row {
          display: flex;
          align-items: center;
          gap: 10px;
          min-width: 0;
        }

        .org-event-meta-row span:last-child {
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }

        .org-meta-icon {
          width: 16px;
          color: #94A3B8;
          flex: 0 0 16px;
          text-align: center;
        }

        .org-button {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          gap: 8px;
          min-height: 42px;
          padding: 0 18px;
          border-radius: 8px;
          border: 1px solid transparent;
          font-size: 14px;
          font-weight: 750;
          cursor: pointer;
          transition: background 150ms ease, border-color 150ms ease, color 150ms ease, box-shadow 150ms ease;
        }

        .org-button:disabled {
          cursor: not-allowed;
          opacity: 0.65;
        }

        .org-button-primary {
          background: #F97316;
          color: #FFFFFF;
          box-shadow: 0 4px 12px rgba(249, 115, 22, 0.2);
        }

        .org-button-primary:hover:not(:disabled) {
          background: #EA580C;
        }

        .org-button-ghost {
          background: #FFFFFF;
          border-color: #E2E8F0;
          color: #334155;
        }

        .org-button-ghost:hover:not(:disabled) {
          background: #F8FAFC;
          border-color: #CBD5E1;
        }

        .org-button-card {
          width: 100%;
          background: #F8FAFC;
          border-color: #DDE7F2;
          color: #0F172A;
          box-shadow: none;
        }

        .org-button-card:hover:not(:disabled) {
          background: #F1F5F9;
          border-color: #CBD5E1;
        }

        .org-event-form-panel {
          margin-bottom: 28px;
          padding: 24px;
          background: #FFFFFF;
          border: 1px solid #DDE7F2;
          border-radius: 14px;
          box-shadow: 0 12px 30px rgba(15, 23, 42, 0.06);
        }

        .org-event-form-header {
          display: flex;
          align-items: flex-start;
          justify-content: space-between;
          gap: 16px;
          margin-bottom: 20px;
        }

        .org-event-form-header h2 {
          margin: 3px 0 0;
          color: #0F172A;
          font-size: 20px;
          font-weight: 800;
          letter-spacing: -0.02em;
        }

        .org-eyebrow {
          margin: 0;
          color: #F97316;
          font-size: 11px;
          font-weight: 800;
          letter-spacing: 0.08em;
          text-transform: uppercase;
        }

        .org-event-form-grid {
          display: grid;
          grid-template-columns: repeat(2, minmax(0, 1fr));
          gap: 16px;
        }

        .org-form-actions {
          grid-column: 1 / -1;
          display: flex;
          justify-content: flex-end;
          gap: 10px;
          margin-top: 4px;
        }

        .org-empty-card {
          padding: 48px;
          background: #FFFFFF;
          border: 1px dashed #CBD5E1;
          border-radius: 14px;
        }

        .org-upload-modal-backdrop {
          position: fixed;
          inset: 0;
          z-index: 1000;
          display: flex;
          align-items: center;
          justify-content: center;
          padding: 24px;
          background: rgba(15, 23, 42, 0.48);
        }

        .org-upload-modal {
          width: min(560px, 100%);
          padding: 20px;
          background: #FFFFFF;
          border-radius: 12px;
          box-shadow: 0 22px 56px rgba(15, 23, 42, 0.24);
        }

        .org-upload-modal h3 {
          margin: 0 0 14px;
          color: #0F172A;
          font-size: 18px;
          font-weight: 800;
        }

        .org-upload-modal img {
          display: block;
          width: 100%;
          max-height: 320px;
          object-fit: contain;
          border-radius: 8px;
          background: #F8FAFC;
        }

        .org-upload-modal p {
          margin: 12px 0 0;
          color: #475569;
          font-size: 13px;
          overflow-wrap: anywhere;
        }

        .org-upload-modal-actions {
          display: flex;
          justify-content: flex-end;
          gap: 10px;
          margin-top: 18px;
        }

        @media (max-width: 720px) {
          .org-events-page {
            padding: 28px 16px;
          }

          .org-events-header {
            flex-direction: column;
            align-items: stretch;
          }

          .org-event-form-grid {
            grid-template-columns: 1fr;
          }

          .org-form-actions {
            flex-direction: column;
          }

          .org-form-actions .org-button,
          .org-events-header .org-button {
            width: 100%;
          }
        }
      `}</style>

      <main className="org-events-shell">
        <header className="org-events-header">
          <div>
            <h1>Danh sách Dự án</h1>
            <p>Quản lý tổng quan các sự kiện và dự án của tổ chức.</p>
          </div>
          {canCreate && (
            <button
              type="button"
              onClick={() => {
                setEditingEvent(null);
                setShowCreateForm(true);
              }}
              className="org-button org-button-primary"
            >
              + Tạo sự kiện mới
            </button>
          )}
        </header>

        {showCreateForm && canCreate && <EventForm mode="create" />}
        {editingEvent && canManage && <EventForm mode="edit" event={editingEvent} />}

        {events.length === 0 ? (
          <div className="org-empty-card">
            <EmptyState message="No events found" />
          </div>
        ) : (
          <section className="org-events-grid" aria-label="Danh sách sự kiện">
            {events.map((event) => {
              const eventId = getEventId(event);
              const bannerSrc = toAbsoluteMediaUrl(event?.bannerUrl || event?.coverUrl || event?.avatarUrl);
              return (
                <article key={eventId} className="org-event-card">
                  {bannerSrc ? (
                    <img
                      src={bannerSrc}
                      alt={`${getEventName(event) || 'Event'} banner`}
                      className="org-event-banner"
                      onError={(e) => {
                        e.currentTarget.style.display = 'none';
                      }}
                    />
                  ) : null}

                  <div className="org-event-card-top">
                    <span className="org-status-badge">{event.status || 'Chưa xác định'}</span>
                    {canManage && (
                      <div className="org-event-card-actions">
                        <button
                          type="button"
                          onClick={() => {
                            setShowCreateForm(false);
                            setEditingEvent(event);
                          }}
                          disabled={isSubmitting}
                          className="org-icon-button"
                          title="Sửa sự kiện"
                          aria-label="Sửa sự kiện"
                        >
                          ⚙
                        </button>
                        <button
                          type="button"
                          onClick={() => handleDelete(eventId)}
                          disabled={isSubmitting}
                          className="org-icon-button org-icon-button-danger"
                          title="Xóa sự kiện"
                          aria-label="Xóa sự kiện"
                        >
                          ×
                        </button>
                      </div>
                    )}
                  </div>

                  <h2>{getEventName(event) || '-'}</h2>
                  <p className="org-event-description">{event.description || 'Không có mô tả.'}</p>

                  <div className="org-event-meta">
                    <div className="org-event-meta-row">
                      <span className="org-meta-icon">□</span>
                      <span>{formatDate(event.startDate)}</span>
                      {formatTime(event.startDate) !== '-' && <span>· {formatTime(event.startDate)}</span>}
                    </div>
                    <div className="org-event-meta-row">
                      <span className="org-meta-icon">⌖</span>
                      <span>{event.location || 'Chưa xác định'}</span>
                    </div>
                  </div>

                  <button type="button" onClick={() => openWorkspace(event)} className="org-button org-button-card">
                    Mở không gian làm việc <span aria-hidden="true">→</span>
                  </button>
                </article>
              );
            })}
          </section>
        )}
      </main>
    </div>
  );
}

export default OrgEventsPage;
