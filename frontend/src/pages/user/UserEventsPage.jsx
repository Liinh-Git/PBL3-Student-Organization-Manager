import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyEvents } from '../../services/userService.js';
import { cancelEventRegistration, registerForEvent } from '../../services/eventService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import EventCard from '../../components/event/EventCard.jsx';
import './UserEventsPage.css';

function UserEventsPage() {
  const navigate = useNavigate();
  const [myEvents, setMyEvents] = useState([]);
  const [registrationMap, setRegistrationMap] = useState({});
  const [processingEventId, setProcessingEventId] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function loadEvents() {
      setIsLoading(true);
      setError(null);
      try {
        const myEventData = await getMyEvents();
        const normalizedMyEvents = Array.isArray(myEventData) ? myEventData : [];
        setMyEvents(normalizedMyEvents);

        const attendeeRegistration = {};
        normalizedMyEvents.forEach((evt) => {
          if (evt?.participationRole === 'Attendee') {
            attendeeRegistration[evt.id] = evt?.attendanceStatus !== 'Cancelled';
          }
        });
        setRegistrationMap(attendeeRegistration);
      } catch (err) {
        setError(err.message || 'Không thể tải danh sách sự kiện');
      } finally {
        setIsLoading(false);
      }
    }

    loadEvents();
  }, []);

  const getEventId = (evt) => evt?.id || evt?.eventId;
  const getEventStatus = (evt) => String(evt?.status || '').toLowerCase();
  const isEventJoinable = (evt) => !['cancelled', 'archived', 'completed'].includes(getEventStatus(evt));

  const memberEvents = useMemo(
    () => myEvents.filter((evt) => evt?.participationRole === 'OrganizationMember'),
    [myEvents],
  );

  const attendeeEvents = useMemo(
    () => myEvents.filter((evt) => evt?.participationRole === 'Attendee'),
    [myEvents],
  );

  const totalJoinedAsAttendee = useMemo(
    () => Object.values(registrationMap).filter(Boolean).length,
    [registrationMap],
  );

  const handleOpenWorkspace = (evt) => {
    const eventId = getEventId(evt);
    if (eventId && evt?.organizationId) {
      navigate(`/org/events/${eventId}?orgId=${evt.organizationId}`);
    }
  };

  const handleViewDetail = (evt) => {
    const eventId = getEventId(evt);
    if (eventId) {
      navigate(`/events/${eventId}`);
    }
  };

  const handleToggleRegistration = async (evt) => {
    const eventId = getEventId(evt);
    if (!eventId) return;

    setProcessingEventId(eventId);
    setError(null);
    try {
      const isRegistered = !!registrationMap[eventId];
      if (isRegistered) {
        await cancelEventRegistration(eventId, {});
        setRegistrationMap((prev) => ({ ...prev, [eventId]: false }));
      } else {
        await registerForEvent(eventId, {});
        setRegistrationMap((prev) => ({ ...prev, [eventId]: true }));
      }
    } catch (err) {
      setError(err.message || 'Không thể cập nhật đăng ký');
    } finally {
      setProcessingEventId(null);
    }
  };

  return (
    <div className="app-page user-events-page">
      <PageHeader
        title="Sự kiện của tôi"
        description="Hiển thị các sự kiện bạn tham gia với vai trò thành viên tổ chức hoặc attendee."
      />

      {isLoading && <LoadingSpinner message="Đang tải danh sách sự kiện..." />}
      {error && <ErrorState message={error} />}

      {!isLoading && !error && memberEvents.length === 0 && attendeeEvents.length === 0 && (
        <EmptyState message="Bạn chưa tham gia sự kiện nào." />
      )}

      {!isLoading && !error && (
        <>
          <section className="user-events-overview">
            <article className="user-events-metric">
              <p>Sự kiện tổ chức</p>
              <strong>{memberEvents.length}</strong>
            </article>
            <article className="user-events-metric">
              <p>Đang ghi danh</p>
              <strong>{totalJoinedAsAttendee}</strong>
            </article>
          </section>

          <div className="user-events-grid">
            <div className="app-card user-events-section">
              <div className="user-events-section-head">
                <h2 className="app-section-title">Sự kiện bạn tham gia tổ chức</h2>
                <span className="app-badge">{memberEvents.length}</span>
              </div>
              {memberEvents.length === 0 ? (
                <EmptyState message="Bạn chưa là event member ở sự kiện nào." />
              ) : (
                <div className="user-events-card-grid">
                  {memberEvents.map((evt) => (
                    <EventCard
                      key={getEventId(evt)}
                      event={evt}
                      showDetailButton={false}
                      footerActions={
                        <>
                          <button
                            type="button"
                            className="app-button app-button--ghost"
                            onClick={() => handleViewDetail(evt)}
                          >
                            Xem chi tiết
                          </button>
                          <button
                            type="button"
                            className="app-button app-button--primary"
                            onClick={() => handleOpenWorkspace(evt)}
                          >
                            Vào không gian làm việc
                          </button>
                        </>
                      }
                    />
                  ))}
                </div>
              )}
            </div>

            <div className="app-card user-events-section">
              <div className="user-events-section-head">
                <h2 className="app-section-title">Sự kiện bạn ghi danh attendee</h2>
                <span className="app-badge">{attendeeEvents.length}</span>
              </div>
              {attendeeEvents.length === 0 ? (
                <EmptyState message="Bạn chưa ghi danh attendee ở sự kiện nào." />
              ) : (
                <div className="user-events-card-grid">
                  {attendeeEvents.map((evt) => {
                    const eventId = getEventId(evt);
                    const isRegistered = !!registrationMap[eventId];
                    const isBusy = processingEventId === eventId;

                    return (
                      <EventCard
                        key={eventId}
                        event={evt}
                        showDetailButton={false}
                        footerActions={
                          <>
                            <button
                              type="button"
                              className="app-button app-button--ghost"
                              onClick={() => handleViewDetail(evt)}
                            >
                              Xem chi tiết
                            </button>
                            <button
                              type="button"
                              className={`app-button ${isRegistered ? 'app-button--secondary' : 'app-button--primary'}`}
                              onClick={() => handleToggleRegistration(evt)}
                              disabled={isBusy || !isEventJoinable(evt)}
                            >
                              {isBusy ? 'Đang xử lý...' : isRegistered ? 'Hủy tham gia' : 'Đăng ký tham gia'}
                            </button>
                          </>
                        }
                      />
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

export default UserEventsPage;
