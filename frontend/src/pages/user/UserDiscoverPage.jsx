/**
 * UserDiscoverPage.jsx - Discover organizations and events page
 * 
 * Phase 4B-1B: Safe read-only page completion
 * 
 * Connected to:
 * - userService.discoverMyOrganizations()
 * 
 * Note: discoverService.discoverEvents is not implemented yet (backend Phase 4A-5 pending)
 * 
 * Permissions:
 * - JWT (authenticated user)
 * 
 * Route:
 * - /user/discover
 */

import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { discoverMyOrganizations, getMyOrganizations } from '../../services/userService.js';
import { getOrganizationEvents, getPublicEvents } from '../../services/eventService.js';
import { createOrganizationRequest } from '../../services/requestService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import EventCard from '../../components/event/EventCard.jsx';

function UserDiscoverPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [organizations, setOrganizations] = useState([]);
  const [events, setEvents] = useState([]);
  const [myOrgIds, setMyOrgIds] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [requestingOrgId, setRequestingOrgId] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);

  useEffect(() => {
    async function loadData() {
      setIsLoading(true);
      setError(null);
      try {
        const [discoverableOrgs, myOrgs, publicEvents] = await Promise.all([
          discoverMyOrganizations(),
          getMyOrganizations(),
          getPublicEvents()
        ]);

        const orgIds = (myOrgs || []).map((org) => org.id).filter(Boolean);
        setMyOrgIds(orgIds);
        setOrganizations(discoverableOrgs || []);

        const orgEventsResults = await Promise.all(
          orgIds.map(async (orgId) => {
            try {
              const orgEvents = await getOrganizationEvents(orgId);
              return (orgEvents || []).map((event) => ({
                ...event,
                organizationId: event.organizationId || orgId
              }));
            } catch {
              return [];
            }
          })
        );

        const mergedEvents = [...(publicEvents || []), ...orgEventsResults.flat()];
        const uniqueEventMap = new Map();
        for (const event of mergedEvents) {
          const eventId = event?.id || event?.eventId;
          if (!eventId) continue;
          if (!uniqueEventMap.has(eventId)) uniqueEventMap.set(eventId, event);
        }
        setEvents(Array.from(uniqueEventMap.values()));
      } catch (err) {
        setError(err.message || 'Failed to load discoverable organizations');
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, []);

  const handleRequestToJoin = async (orgId, orgName) => {
    const safeOrgName = orgName || 'this organization';

    setRequestingOrgId(orgId);
    setSuccessMessage(null);

    try {
      await createOrganizationRequest(orgId, {
        requestType: 'JoinOrganization',
        content: `I would like to join ${safeOrgName}`,
      });

      setSuccessMessage(`Request sent to ${safeOrgName}`);
    } catch (err) {
      setError(err.message || 'Failed to send join request');
    } finally {
      setRequestingOrgId(null);
    }
  };

  const handleViewEvent = (event) => {
    const eventId = event?.id || event?.eventId;
    const orgId = event?.organizationId || event?.orgId || event?.organization?.id;

    if (!eventId) {
      alert('Event ID is missing');
      return;
    }

    if (orgId && myOrgIds.includes(orgId)) {
      navigate(`/org/events/${eventId}?orgId=${orgId}`);
      return;
    }

    navigate(`/events/${eventId}`);
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Discover"
          description="Find organizations and events to join"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Discover"
          description="Find organizations and events to join"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Discover"
        description="Find organizations and events to join"
      />

      <div className="app-section">
        {successMessage && (
          <div className="app-card" style={{ marginBottom: '16px', backgroundColor: '#d4edda', borderColor: '#c3e6cb' }}>
            <p style={{ color: '#155724', margin: 0 }}>{successMessage}</p>
          </div>
        )}
        {/* Organizations section */}
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Organizations</h3>
          </div>
          {organizations.length === 0 ? (
            <EmptyState message="No discoverable organizations found" />
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Organization Name</th>
                  <th>Description</th>
                  <th>Location</th>
                  <th>Members</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {organizations.map((org) => {
                const orgName = org.name || org.orgName || org.organizationName || 'Unknown organization';

                return (
                  <tr key={org.id}>
                    <td>{orgName}</td>
                    <td>{org.description || '-'}</td>
                    <td>{org.location || '-'}</td>
                    <td>{org.totalMembers ?? '-'}</td>
                    <td>{org.status || (org.isActive ? 'Active' : 'Inactive')}</td>
                    <td>
                      <button
                        onClick={() => handleRequestToJoin(org.id, orgName)}
                        disabled={requestingOrgId === org.id}
                        className="app-button app-button--primary"
                      >
                        {requestingOrgId === org.id ? 'Sending...' : 'Request to Join'}
                      </button>
                    </td>
                  </tr>
                );
              })}
              </tbody>
            </table>
          )}
        </div>

        {/* Events section */}
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Events</h3>
          </div>
          {events.length === 0 ? (
            <EmptyState message="No events available to view" />
          ) : (
            <div style={{ display: 'grid', gap: '1rem' }}>
              {events.map((event) => (
                <EventCard
                  key={event?.id || event?.eventId}
                  event={event}
                  onView={() => handleViewEvent(event)}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default UserDiscoverPage;
