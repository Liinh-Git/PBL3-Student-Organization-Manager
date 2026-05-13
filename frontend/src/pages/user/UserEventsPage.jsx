/**
 * UserEventsPage.jsx - User's events page
 *
 * Phase 4B-1: Real backend API integration
 */

import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { getMyEvents } from "../../services/userService.js";
import PageHeader from "../../components/shared/PageHeader";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import "./UserEventsPage.css";

function UserEventsPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [events, setEvents] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function loadEvents() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getMyEvents();
        setEvents(Array.isArray(data) ? data : []);
      } catch (err) {
        setError(err.message || "Failed to load events");
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, []);

  const getEventName = (evt) => evt?.name || evt?.eventName || "Untitled Event";

  const handleViewEvent = (evt) => {
    const orgId = evt?.organizationId;
    const eventId = evt?.id;
    if (orgId && eventId) {
      navigate(`/org/events/${eventId}?orgId=${orgId}`);
    }
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleString("vi-VN", {
      dateStyle: "short",
      timeStyle: "short",
    });
  };

  return (
    <div className="app-page">
      <div className="ue-dashboard">
        <PageHeader
          title="Tất cả sự kiện"
          description="Danh sách các sự kiện bạn đang tham gia"
        />

        {isLoading && <LoadingSpinner message="Đang tải sự kiện..." />}

        {error && <ErrorState message={error} />}

        {!isLoading && !error && events.length === 0 && (
          <EmptyState message="Bạn chưa tham gia sự kiện nào." />
        )}

        {!isLoading && !error && events.length > 0 && (
          <div className="ue-list">
            {events.map((evt) => (
              <div className="ue-card" key={evt.id}>
                {/* Cột 1: Ảnh sự kiện */}
                <div className="ue-image-wrap">
                  {evt.imageUrl ? (
                    <img src={evt.imageUrl} alt="Event" className="ue-image" />
                  ) : (
                    <div className="ue-placeholder">
                      <svg
                        width="32"
                        height="32"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="#94a3b8"
                        strokeWidth="1.5"
                      >
                        <rect
                          x="3"
                          y="3"
                          width="18"
                          height="18"
                          rx="2"
                          ry="2"
                        ></rect>
                        <circle cx="8.5" cy="8.5" r="1.5"></circle>
                        <polyline points="21 15 16 10 5 21"></polyline>
                      </svg>
                    </div>
                  )}
                </div>

                {/* Cột 2: Nội dung */}
                <div className="ue-details">
                  <div className="ue-org">
                    {evt.organizationName || "Chưa rõ tổ chức"}
                  </div>

                  <h4 className="ue-title">{getEventName(evt)}</h4>

                  <div className="ue-meta">
                    <div className="ue-meta-item">
                      <svg
                        width="16"
                        height="16"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                      >
                        <rect
                          x="3"
                          y="4"
                          width="18"
                          height="18"
                          rx="2"
                          ry="2"
                        ></rect>
                        <line x1="16" y1="2" x2="16" y2="6"></line>
                        <line x1="8" y1="2" x2="8" y2="6"></line>
                        <line x1="3" y1="10" x2="21" y2="10"></line>
                      </svg>
                      <span>{formatDateTime(evt.startDate)}</span>
                    </div>
                    <div className="ue-meta-item">
                      <svg
                        width="16"
                        height="16"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                      >
                        <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                        <circle cx="12" cy="10" r="3"></circle>
                      </svg>
                      <span>{evt.location || "Chưa cập nhật"}</span>
                    </div>
                  </div>
                </div>

                {/* Cột 3: Nút bấm */}
                <div className="ue-actions">
                  {evt.organizationId && (
                    <button
                      className="ue-btn"
                      onClick={() => handleViewEvent(evt)}
                    >
                      Xem
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default UserEventsPage;
