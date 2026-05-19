/**
 * PublicEventsPage.jsx - Public events listing page
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Load public events list
 * - Add search/filter controls
 * - Add pagination
 * - Link to PublicEventDetailPage
 * 
 * Future Service Usage:
 * - eventService.getPublicEvents(params)
 * 
 * Future Adapter Usage:
 * - eventAdapter.toEventPublicViewModel()
 * 
 * Permissions:
 * - Public (no authentication required)
 * 
 * Route:
 * - /events
 * 
 * Query Params:
 * - ?search= (optional)
 * - ?page= (optional)
 * - ?pageSize= (optional)
 * 
 * State Management:
 * - TODO: useState for events list
 * - TODO: useState for search/filter params
 * - TODO: useState for pagination
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

import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { getPublicEvents } from '../../services/eventService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';

function PublicEventsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  
  const [events, setEvents] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const searchTerm = searchParams.get('search') || '';

  useEffect(() => {
    async function loadEvents() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getPublicEvents();
        setEvents(Array.isArray(data) ? data : data?.items || []);
      } catch (err) {
        setError(err.message || 'Không thể tải danh sách sự kiện công khai');
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, []);

  const filteredEvents = useMemo(() => {
    const keyword = searchTerm.trim().toLowerCase();
    if (!keyword) return events;
    return events.filter((event) => {
      const name = event.eventName || event.name || '';
      const org = event.organizationName || '';
      const location = event.location || '';
      return `${name} ${org} ${location}`.toLowerCase().includes(keyword);
    });
  }, [events, searchTerm]);

  const handleSearchChange = (e) => {
    const value = e.target.value;
    const nextParams = new URLSearchParams(searchParams);
    if (value) {
      nextParams.set('search', value);
    } else {
      nextParams.delete('search');
    }
    setSearchParams(nextParams);
  };

  const handleReloadEvents = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await getPublicEvents();
      setEvents(Array.isArray(data) ? data : data?.items || []);
    } catch (err) {
      setError(err.message || 'Không thể tải danh sách sự kiện công khai');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="public-events-page">
      <style>{`
        .public-events-page {
          padding: 34px;
        }

        .public-events-toolbar {
          display: flex;
          gap: 12px;
          margin-bottom: 20px;
        }

        .public-events-toolbar input {
          width: min(420px, 100%);
          min-height: 42px;
          border: 1px solid #DDE7F2;
          border-radius: 8px;
          padding: 0 12px;
        }

        .public-events-grid {
          display: grid;
          grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
          gap: 18px;
        }

        .public-event-card {
          display: grid;
          gap: 12px;
          padding: 18px;
          border: 1px solid #DDE7F2;
          border-radius: 8px;
          background: #FFFFFF;
        }

        .public-event-card h3 {
          margin: 0;
          font-size: 18px;
        }

        .public-event-card p {
          margin: 0;
          color: #64748B;
        }
      `}</style>

      <PageHeader
        title="Sự kiện công khai"
        description="Khám phá các sự kiện sắp diễn ra"
        actions={
          <button onClick={handleReloadEvents} disabled={isLoading} className="app-button app-button--primary">
            {isLoading ? 'Đang tải...' : 'Làm mới'}
          </button>
        }
      />

      <div className="public-events-toolbar">
        <input
          value={searchTerm}
          onChange={handleSearchChange}
          placeholder="Tìm theo sự kiện, tổ chức hoặc địa điểm"
        />
      </div>

      <div className="app-section">
        {isLoading && <LoadingSpinner />}
        {error && <ErrorState message={error} />}
        {!isLoading && !error && filteredEvents.length === 0 && <EmptyState message="Không có sự kiện công khai nào" />}
        {!isLoading && !error && filteredEvents.length > 0 && (
          <div className="public-events-grid">
            {filteredEvents.map((event) => (
              <article key={event.id} className="public-event-card">
                <h3>{event.eventName || event.name || '-'}</h3>
                <p>{event.organizationName || '-'}</p>
                <p>{event.startDate ? new Date(event.startDate).toLocaleDateString('vi-VN') : '-'}</p>
                <p>{event.location || '-'}</p>
                <button
                  type="button"
                  className="app-button app-button--primary"
                  onClick={() => navigate(`/events/${event.id}`, { state: { returnTo: '/user/discover' } })}
                >
                  Xem chi tiết
                </button>
              </article>
            ))}
          </div>
        )}
      </div>

    </div>
  );
}

export default PublicEventsPage;
