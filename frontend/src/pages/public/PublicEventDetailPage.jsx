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
      setError('Event ID is required');
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
        setError(err.message || 'Failed to load public event');
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
      alert(err.message || 'Failed to register for event');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelRegistration = async () => {
    if (!window.confirm('Cancel your registration for this event?')) {
      return;
    }

    setIsSubmitting(true);
    try {
      const result = await cancelEventRegistration(eventId);
      setRegistration(result);
    } catch (err) {
      alert(err.message || 'Failed to cancel registration');
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
      <style>{`
        .public-event-detail-page {
          width: 100%;
          min-height: calc(100vh - 62px);
          align-content: start;
          background: linear-gradient(135deg, #F8FBFF 0%, #EAF3FF 52%, #F7FBFF 100%);
        }

        .public-event-detail-shell {
          display: grid;
          grid-template-columns: minmax(0, 1fr) minmax(320px, 380px);
          gap: 32px;
          align-items: start;
          width: min(1280px, 100%);
        }

        .public-event-hero,
        .public-registration-panel {
          border: 1px solid #DDE7F2;
          border-radius: 8px;
          background: #FFFFFF;
          overflow: hidden;
        }

        .public-event-banner {
          width: 100%;
          aspect-ratio: 16 / 7;
          object-fit: cover;
          background: #F8FAFC;
        }

        .public-event-body,
        .public-registration-panel {
          padding: 32px;
        }

        .public-event-body h2,
        .public-registration-panel h3 {
          margin: 0 0 18px;
          color: #0F1F33;
        }

        .public-event-body > p,
        .public-registration-panel p {
          margin: 0 0 22px;
          color: #102A43;
          font-size: 15px;
          line-height: 1.6;
        }

        .public-event-meta {
          display: grid;
          grid-template-columns: repeat(2, minmax(0, 1fr));
          gap: 16px;
          margin-top: 28px;
        }

        .public-info-card {
          display: flex;
          align-items: center;
          gap: 16px;
          min-height: 92px;
          padding: 18px;
          border: 1px solid #DDE7F2;
          border-radius: 8px;
          background: #FFFFFF;
          box-shadow: 0 10px 24px rgba(15, 31, 51, 0.05);
        }

        .public-info-icon {
          display: inline-flex;
          width: 54px;
          height: 54px;
          flex: 0 0 54px;
          align-items: center;
          justify-content: center;
          border-radius: 8px;
          background: #EFF4F7;
          color: #F97316;
          font-size: 22px;
          font-weight: 900;
        }

        .public-info-card div span {
          display: block;
          color: #7C8CA3;
          font-size: 12px;
          font-weight: 850;
          text-transform: uppercase;
        }

        .public-info-card strong {
          display: block;
          margin-top: 3px;
          color: #12263A;
          font-size: 20px;
          line-height: 1.2;
        }

        .public-registration-panel {
          display: grid;
          gap: 10px;
        }

        .registration-status {
          display: inline-flex;
          width: fit-content;
          margin-bottom: 14px;
          padding: 6px 10px;
          border-radius: 4px;
          background: #ECFDF5;
          color: #047857;
          font-size: 12px;
          font-weight: 800;
        }

        @media (max-width: 800px) {
          .public-event-detail-shell {
            grid-template-columns: 1fr;
            gap: 20px;
          }

          .public-event-meta {
            grid-template-columns: 1fr;
          }

          .public-event-body,
          .public-registration-panel {
            padding: 22px;
          }
        }
      `}</style>

      {isLoading && <LoadingSpinner />}
      {error && <ErrorState message={error} />}

      <PageHeader
        title={event?.eventName || event?.name || 'Event Details'}
        description={event?.organizationName || 'Public event information'}
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
              <p>{event.description || 'No description yet.'}</p>
              <div className="public-event-meta">
                <div className="public-info-card">
                  <span className="public-info-icon">⌖</span>
                  <div>
                    <span>Location</span>
                    <strong>{event.location || '-'}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">◷</span>
                  <div>
                    <span>Start time</span>
                    <strong>{formatTime(event.startDate)}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">#</span>
                  <div>
                    <span>Participants</span>
                    <strong>{participantText}</strong>
                  </div>
                </div>
                <div className="public-info-card">
                  <span className="public-info-icon">✓</span>
                  <div>
                    <span>Status</span>
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
            <h3>Registration</h3>
            {isEventMember ? (
              <>
                <span className="registration-status">EventMember</span>
                <p>You are participating as an event organizer (BTC).</p>
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
                  {isSubmitting ? 'Cancelling...' : 'Cancel registration'}
                </button>
              </>
            ) : (
              <>
                <p>Register to join this public event as an attendee.</p>
                <button
                  type="button"
                  className="app-button app-button--primary"
                  onClick={handleRegister}
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Registering...' : isAuthenticated ? 'Register' : 'Log in to register'}
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
