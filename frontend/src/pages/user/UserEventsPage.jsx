/**
 * UserEventsPage.jsx - User's events page
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Load user's events (events user is involved in)
 * - Display event cards
 * - Add filter/search controls
 * - Link to event detail pages
 * 
 * Future Service Usage:
 * - userService.getMyEvents(params)
 * 
 * Future Adapter Usage:
 * - userAdapter.toMyEventViewModel()
 * 
 * Permissions:
 * - JWT (authenticated user)
 * 
 * Route:
 * - /user/events
 * 
 * Query Params:
 * - ?search= (optional)
 * - ?status= (optional)
 * 
 * State Management:
 * - TODO: useState for events list
 * - TODO: useState for filter params
 * - TODO: useEffect to load events
 * - TODO: Loading state
 * - TODO: Error state
 * - TODO: Empty state
 * 
 * IMPORTANT:
 * - No real API calls in Phase 3C
 * - No fake event data
 * - No mock event cards
 */

import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { getMyEvents } from '../../services/userService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import EventCard from '../../components/event/EventCard.jsx';

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
        setError(err.message || 'Failed to load my events');
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, []);

  const getEventId = (event) => event?.id || event?.eventId;
  const getOrgId = (event) => event?.organizationId || event?.orgId || event?.organization?.id;

  const handleViewEvent = (event) => {
    const eventId = getEventId(event);
    const orgId = getOrgId(event);

    if (!eventId || !orgId) {
      alert('Missing event or organization ID');
      return;
    }

    navigate(`/org/events/${eventId}?orgId=${orgId}`);
  };

  const searchKeyword = (searchParams.get('search') || '').trim().toLowerCase();
  const statusFilter = (searchParams.get('status') || '').trim().toLowerCase();

  const filteredEvents = events.filter((event) => {
    const eventName = String(event?.name || event?.eventName || '').toLowerCase();
    const description = String(event?.description || '').toLowerCase();
    const status = String(event?.status || '').toLowerCase();
    const matchesSearch = !searchKeyword || eventName.includes(searchKeyword) || description.includes(searchKeyword);
    const matchesStatus = !statusFilter || status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="app-page">
      <PageHeader
        title="My Events"
        description="Events you are involved in"
      />

      <div className="app-section">
        {isLoading && <LoadingSpinner message="Loading your events..." />}
        {!isLoading && error && <ErrorState message={error} />}
        {!isLoading && !error && filteredEvents.length === 0 && (
          <EmptyState message="No events found for your account" />
        )}
        {!isLoading && !error && filteredEvents.length > 0 && (
          <div style={{ display: 'grid', gap: '1rem' }}>
            {filteredEvents.map((event) => (
              <EventCard
                key={getEventId(event)}
                event={event}
                onView={() => handleViewEvent(event)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default UserEventsPage;
