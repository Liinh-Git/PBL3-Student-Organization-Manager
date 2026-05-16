/**
 * UserEventsPage.jsx - User's events page
 *
 * Phase 4B-1: Real backend API integration
 */

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyEvents } from '../../services/userService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import EventCard from '../../components/event/EventCard.jsx';

function UserEventsPage() {
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
        setError(err.message || 'Failed to load events');
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, []);

  const getEventName = (evt) => evt?.name || evt?.eventName || 'Untitled Event';

  const handleViewEvent = (evt) => {
    const eventId = evt?.id;
    if (eventId) {
      navigate(`/events/${eventId}`);
    }
  };

  return (
    <div className="app-page">
      <PageHeader
        title="My Events"
        description="Events you are involved in"
      />

      {isLoading && <LoadingSpinner message="Loading events..." />}

      {error && <ErrorState message={error} />}

      {!isLoading && !error && events.length === 0 && (
        <EmptyState message="You are not involved in any events yet." />
      )}

      {!isLoading && !error && events.length > 0 && (
        <div className="app-section">
          <div className="app-card">
            <table>
              <thead>
                <tr>
                  <th>Event Name</th>
                  <th>Organization</th>
                  <th>Start Date</th>
                  <th>End Date</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {events.map((evt) => (
                  <tr key={evt.id}>
                    <td>{getEventName(evt)}</td>
                    <td>{evt.organizationName || '-'}</td>
                    <td>{evt.startDate ? new Date(evt.startDate).toLocaleDateString() : '-'}</td>
                    <td>{evt.endDate ? new Date(evt.endDate).toLocaleDateString() : '-'}</td>
                    <td>
                      <span className={`app-badge ${evt.status === 'Active' ? 'app-badge--success' : evt.status === 'Draft' ? 'app-badge--warning' : ''}`}>
                        {evt.status || '-'}
                      </span>
                    </td>
                    <td>
                      {evt.organizationId && (
                        <button
                          className="app-button app-button--secondary"
                          onClick={() => handleViewEvent(evt)}
                        >
                          View
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

export default UserEventsPage;
