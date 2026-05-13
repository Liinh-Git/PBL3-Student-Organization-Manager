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

import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { discoverMyOrganizations, getMyOrganizations } from '../../services/userService.js';
import { getOrganizationEvents, getPublicEvents } from '../../services/eventService.js';
import { createOrganizationRequest, getMyPendingJoinRequests, withdrawOrganizationJoinRequest } from '../../services/requestService.js';
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
  const [pendingJoinOrgIds, setPendingJoinOrgIds] = useState(new Set());

  const syncMembershipAndPendingState = useCallback(async () => {
    const [discoverableOrgs, myOrgs, pendingJoinRequests] = await Promise.all([
      discoverMyOrganizations(),
      getMyOrganizations(),
      getMyPendingJoinRequests()
    ]);

    const myOrgList = myOrgs || [];
    const orgIds = myOrgList.map((org) => org.id).filter(Boolean);
    setMyOrgIds(orgIds);

    // Merge discoverable orgs + my orgs so joined state always wins in UI.
    const mergedById = new Map();
    (discoverableOrgs || []).forEach((org) => {
      if (org?.id) mergedById.set(org.id, org);
    });
    myOrgList.forEach((org) => {
      if (!org?.id) return;
      const existing = mergedById.get(org.id) || {};
      mergedById.set(org.id, {
        ...existing,
        id: org.id,
        name: existing.name || org.name,
        orgName: existing.orgName || org.name,
        description: existing.description || org.description,
        status: existing.status || "Active",
        isJoined: true
      });
    });
    setOrganizations(Array.from(mergedById.values()));

    const pendingIds = new Set((pendingJoinRequests || []).map((r) => r.organizationId).filter(Boolean));
    orgIds.forEach((id) => pendingIds.delete(id));
    setPendingJoinOrgIds(pendingIds);

    return orgIds;
  }, []);

  useEffect(() => {
    async function loadData() {
      setIsLoading(true);
      setError(null);
      try {
        const [orgIds, publicEvents] = await Promise.all([
          syncMembershipAndPendingState(),
          getPublicEvents()
        ]);

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
  }, [syncMembershipAndPendingState]);

  useEffect(() => {
    const syncQuietly = async () => {
      try {
        await syncMembershipAndPendingState();
      } catch {
        // keep current UI state if background sync fails
      }
    };

    const onFocus = () => { syncQuietly(); };
    window.addEventListener('focus', onFocus);
    const intervalId = window.setInterval(syncQuietly, 5000);

    return () => {
      window.removeEventListener('focus', onFocus);
      window.clearInterval(intervalId);
    };
  }, [syncMembershipAndPendingState]);

  const handleRequestToJoin = async (orgId, orgName) => {
    const safeOrgName = orgName || 'this organization';
    if (myOrgIds.includes(orgId)) return;

    setRequestingOrgId(orgId);
    setSuccessMessage(null);
    setError(null);

    try {
      if (pendingJoinOrgIds.has(orgId)) {
        const withdrew = await withdrawOrganizationJoinRequest(orgId);
        if (withdrew) {
          setPendingJoinOrgIds((prev) => {
            const next = new Set(prev);
            next.delete(orgId);
            return next;
          });
          setSuccessMessage(`Đã thu hồi yêu cầu tham gia ${safeOrgName}`);
        } else {
          setSuccessMessage(`Không có yêu cầu đang chờ để thu hồi cho ${safeOrgName}`);
        }
      } else {
        await createOrganizationRequest(orgId, {
          requestType: 'JoinOrganization',
          content: `I would like to join ${safeOrgName}`,
        });

        setPendingJoinOrgIds((prev) => {
          const next = new Set(prev);
          next.add(orgId);
          return next;
        });
        setSuccessMessage(`Đã gửi yêu cầu tham gia ${safeOrgName}`);
      }
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
                const isJoined = !!org.isJoined || myOrgIds.includes(org.id);
                const isPending = pendingJoinOrgIds.has(org.id);
                const isWorking = requestingOrgId === org.id;

                return (
                  <tr key={org.id}>
                    <td>{orgName}</td>
                    <td>{org.description || '-'}</td>
                    <td>{org.location || '-'}</td>
                    <td>{org.totalMembers ?? '-'}</td>
                    <td>{org.status || (org.isActive ? 'Active' : 'Inactive')}</td>
                    <td>
                      <button
                        onClick={() => {
                          if (isJoined) {
                            navigate(`/org/overview?orgId=${org.id}`);
                            return;
                          }
                          handleRequestToJoin(org.id, orgName);
                        }}
                        disabled={isWorking}
                        className={`app-button ${isJoined || isPending ? 'app-button--secondary' : 'app-button--primary'}`}
                        title={isJoined ? 'Xem chi tiết tổ chức' : (isPending ? 'Nhấn lại để thu hồi yêu cầu' : undefined)}
                      >
                        {isWorking ? 'Đang xử lý...' : (isJoined ? 'Xem chi tiết' : (isPending ? 'Đã gửi yêu cầu tham gia' : 'Request to Join'))}
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
