/**
 * PublicEventDetailPage.jsx - Public event detail page
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Load public event details
 * - Display event information (name, description, dates, location)
 * - Show public event metadata (if available)
 * - Add registration/RSVP button (if applicable)
 * 
 * Future Service Usage:
 * - eventService.getPublicEventById(id)
 * 
 * Future Adapter Usage:
 * - eventAdapter.toEventPublicViewModel()
 * 
 * Permissions:
 * - Public (no authentication required)
 * 
 * Route:
 * - /events/:eventId
 * 
 * Route Params:
 * - eventId (from useParams())
 * 
 * State Management:
 * - TODO: useState for event data
 * - TODO: useEffect to load event
 * - TODO: Loading state
 * - TODO: Error state (404 if event not found or not public)
 * 
 * IMPORTANT:
 * - No real API calls in Phase 3C
 * - No fake event data
 * - This is PUBLIC view, not org workspace view
 * - Org workspace event detail is OrgEventDetailPage
 */

import { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import {
  cancelEventRegistration,
  getMyEventRegistration,
  getPublicEventById,
  registerForEvent
} from '../../services/eventService.js';
import { useAuth } from '../../hooks/useAuth.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ErrorState from '../../components/shared/ErrorState';
import './public.css';

function toAbsoluteMediaUrl(url) {
  if (!url) return '';
  if (/^https?:\/\//i.test(url)) return url;

  const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
  const origin = apiBase.replace(/\/api\/?$/, '');
  return url.startsWith('/') ? `${origin}${url}` : `${origin}/${url}`;
}

function formatDateTime(value) {
  if (!value) return '-';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';
  return date.toLocaleString('vi-VN');
}

function formatTime(value) {
  if (!value) return '-';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';
  return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

function PublicEventDetailPage() {
  const { eventId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated } = useAuth();

  const [event, setEvent] = useState(null);
  const [registration, setRegistration] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!eventId) {
      setError('Thiếu mã sự kiện');
      return;
    }

    async function loadEvent() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getPublicEventById(eventId);
        setEvent(data);

        if (isAuthenticated) {
          const myRegistration = await getMyEventRegistration(eventId);
          setRegistration(myRegistration);
        }
      } catch (err) {
        setError(err.message || 'Không thể tải chi tiết sự kiện công khai');
      } finally {
        setIsLoading(false);
      }
    }

    loadEvent();
  }, [eventId, isAuthenticated]);

  const activeRegistration = registration && registration.status !== 'Cancelled' ? registration : null;
  const isEventMember = !!registration?.isEventMember;

  const handleRegister = async () => {
    if (!isAuthenticated) {
      navigate(`/login?returnUrl=/events/${eventId}`);
      return;
    }

    setIsSubmitting(true);
    try {
      const result = await registerForEvent(eventId);
      setRegistration(result);
    } catch (err) {
      alert(err.message || 'Không thể đăng ký sự kiện');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelRegistration = async () => {
    if (!window.confirm('Bạn có chắc muốn hủy đăng ký sự kiện này?')) {
      return;
    }

    setIsSubmitting(true);
    try {
      const result = await cancelEventRegistration(eventId);
      setRegistration(result);
    } catch (err) {
      alert(err.message || 'Không thể hủy đăng ký');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleBack = () => {
    const returnTo = location.state?.returnTo || '/user/discover';
    navigate(returnTo);
  };

  const bannerSrc = toAbsoluteMediaUrl(event?.bannerUrl);
  const registeredParticipants = event?.registeredParticipants ?? 0;
  const participantText = event?.targetParticipants
    ? `${registeredParticipants}/${event.targetParticipants}`
    : `${registeredParticipants}`;

  return (
    <div className="app-page public-event-detail-page">

      {isLoading && <LoadingSpinner />}
      {error && <ErrorState message={error} />}

      <PageHeader
        title={event?.eventName || event?.name || 'Chi tiết sự kiện'}
        description={event?.organizationName || 'Thông tin sự kiện công khai'}
        actions={
          <button type="button" className="app-button app-button--secondary" onClick={handleBack}>
            Quay lại
          </button>
        }
      />

      {!isLoading && !error && event && (
        <div className="public-event-detail-shell">
          <section className="public-event-hero">
            {bannerSrc ? (
              <img
                src={bannerSrc}
                alt={`${event.name || 'Event'} banner`}
                className="public-event-banner"
                onError={(e) => {
                  e.currentTarget.style.display = 'none';
                }}
              />
            ) : null}
            <div className="public-event-body">
              <h2>{event.eventName || event.name || '-'}</h2>
              <p>{event.description || 'Chưa có mô tả.'}</p>
              <div className="public-event-meta">
                <div className="public-info-card">
                  <span className="public-info-icon">⌖</span>
                  <div>
                    <span>Địa điểm</span>
                    <strong>{event.location || '-'}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">◷</span>
                  <div>
                    <span>Giờ bắt đầu</span>
                    <strong>{formatTime(event.startDate)}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">#</span>
                  <div>
                    <span>Người tham gia</span>
                    <strong>{participantText}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">✓</span>
                  <div>
                    <span>Trạng thái</span>
                    <strong>{event.status || '-'}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">→</span>
                  <div>
                    <span>Start</span>
                    <strong>{formatDateTime(event.startDate)}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">←</span>
                  <div>
                    <span>End</span>
                    <strong>{formatDateTime(event.endDate)}</strong>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <aside className="public-registration-panel">
            <h3>Đăng ký tham gia</h3>
            {isEventMember ? (
              <>
                <span className="registration-status">Thành viên BTC</span>
                <p>Bạn đang tham gia với vai trò thành viên ban tổ chức.</p>
                <button
                  type="button"
                  className="app-button app-button--secondary"
                  disabled
                >
                  Tham gia với tư cách BTC
                </button>
              </>
            ) : activeRegistration ? (
              <>
                <span className="registration-status">{activeRegistration.status}</span>
                <p>You are registered for this event.</p>
                <button
                  type="button"
                  className="app-button app-button--secondary"
                  onClick={handleCancelRegistration}
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Đang hủy...' : 'Hủy đăng ký'}
                </button>
              </>
            ) : (
              <>
                <p>Đăng ký để tham gia sự kiện công khai này với vai trò attendee.</p>
                <button
                  type="button"
                  className="app-button app-button--primary"
                  onClick={handleRegister}
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Đang đăng ký...' : isAuthenticated ? 'Đăng ký tham gia' : 'Đăng nhập để đăng ký'}
                </button>
              </>
            )}
          </aside>
        </div>
      )}
    </div>
  );
}

export default PublicEventDetailPage;
