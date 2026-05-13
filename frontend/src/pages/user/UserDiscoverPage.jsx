import { useState, useEffect, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { discoverMyOrganizations, getMyOrganizations } from '../../services/userService.js';
import { getOrganizationEvents, getPublicEvents } from '../../services/eventService.js';
import { getOrganizationMembers } from '../../services/memberService.js';
import { createOrganizationRequest, getMyPendingJoinRequests, withdrawOrganizationJoinRequest } from '../../services/requestService.js';
import {
  acceptFriendRequest,
  getFriendRequests,
  getFriends,
  getFriendSuggestions,
  rejectFriendRequest,
  sendFriendRequest
} from '../../services/friendService.js';
import {
  acceptMyInvitation,
  createOrganizationInvitation,
  createOrganizationInvitationRecommendation,
  getMyInvitations,
  rejectMyInvitation
} from '../../services/invitationService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import EventCard from '../../components/event/EventCard.jsx';
import './UserDiscoverPage.css';

function UserDiscoverPage() {
  const navigate = useNavigate();

  const [organizations, setOrganizations] = useState([]);
  const [events, setEvents] = useState([]);
  const [myOrgIds, setMyOrgIds] = useState([]);
  const [myOrganizations, setMyOrganizations] = useState([]);
  const [orgMemberUserIdsMap, setOrgMemberUserIdsMap] = useState({});
  const [pendingJoinOrgIds, setPendingJoinOrgIds] = useState(new Set());
  const [incomingFriendRequests, setIncomingFriendRequests] = useState([]);
  const [friends, setFriends] = useState([]);
  const [friendSuggestions, setFriendSuggestions] = useState([]);
  const [sentFriendRequestUserIds, setSentFriendRequestUserIds] = useState(new Set());
  const [myInvitations, setMyInvitations] = useState([]);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  const [partialWarning, setPartialWarning] = useState(null);

  const [requestingOrgId, setRequestingOrgId] = useState(null);
  const [processingFriendRequestId, setProcessingFriendRequestId] = useState(null);
  const [sendingFriendRequestToUserId, setSendingFriendRequestToUserId] = useState(null);
  const [invitingKey, setInvitingKey] = useState(null);
  const [inviteOrgIdByFriendId, setInviteOrgIdByFriendId] = useState({});
  const [processingInvitationId, setProcessingInvitationId] = useState(null);

  const [orgSearch, setOrgSearch] = useState('');
  const [orgFilter, setOrgFilter] = useState('all'); // all|joined|pending|available

  const syncMembershipAndPendingState = useCallback(async () => {
    const [discoverableOrgs, myOrgs, pendingJoinRequests] = await Promise.all([
      discoverMyOrganizations(),
      getMyOrganizations(),
      getMyPendingJoinRequests()
    ]);

    const myOrgList = myOrgs || [];
    setMyOrganizations(myOrgList);
    const joinedOrgIds = myOrgList.map((org) => org.id).filter(Boolean);
    setMyOrgIds(joinedOrgIds);

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
        status: existing.status || 'Active',
        isJoined: true
      });
    });

    const merged = Array.from(mergedById.values());
    setOrganizations(merged);

    const pendingIds = new Set((pendingJoinRequests || []).map((r) => r.organizationId).filter(Boolean));
    joinedOrgIds.forEach((id) => pendingIds.delete(id));
    setPendingJoinOrgIds(pendingIds);

    return joinedOrgIds;
  }, []);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    setPartialWarning(null);

    try {
      const [orgIds, publicEvents, friendRequestsResult, myFriendsResult, invitationsResult, suggestionsResult] = await Promise.all([
        syncMembershipAndPendingState(),
        getPublicEvents(),
        getFriendRequests().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err })),
        getFriends().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err })),
        getMyInvitations().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err })),
        getFriendSuggestions().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err }))
      ]);

      const friendRequests = friendRequestsResult.ok ? friendRequestsResult.data : [];
      const myFriends = myFriendsResult.ok ? myFriendsResult.data : [];
      const invitations = invitationsResult.ok ? invitationsResult.data : [];
      const suggestions = suggestionsResult.ok ? suggestionsResult.data : [];

      const warnings = [];
      if (!friendRequestsResult.ok) warnings.push('friend requests');
      if (!myFriendsResult.ok) warnings.push('friends');
      if (!invitationsResult.ok) warnings.push('invitations');
      if (!suggestionsResult.ok) warnings.push('friend suggestions');
      if (warnings.length > 0) {
        setPartialWarning(`Some sections could not be loaded: ${warnings.join(', ')}.`);
      }

      const pendingIncoming = (friendRequests || []).filter((r) => r.status === 'Pending');
      setIncomingFriendRequests(pendingIncoming);
      setFriends(myFriends || []);
      setMyInvitations(invitations || []);
      setFriendSuggestions(suggestions || []);

      const memberMap = {};
      await Promise.all(
        (orgIds || []).map(async (id) => {
          try {
            const orgMembers = await getOrganizationMembers(id);
            memberMap[id] = new Set((orgMembers || []).map((m) => m.userId).filter(Boolean));
          } catch {
            memberMap[id] = new Set();
          }
        }),
      );
      setOrgMemberUserIdsMap(memberMap);

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
      setError(err.message || 'Failed to load discover page');
    } finally {
      setIsLoading(false);
    }
  }, [syncMembershipAndPendingState]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  useEffect(() => {
    const onFocus = () => {
      syncMembershipAndPendingState().catch(() => {});
    };
    window.addEventListener('focus', onFocus);
    return () => window.removeEventListener('focus', onFocus);
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
          setSuccessMessage(`Withdrawn join request for ${safeOrgName}`);
        } else {
          setSuccessMessage(`No pending join request found for ${safeOrgName}`);
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
        setSuccessMessage(`Join request sent to ${safeOrgName}`);
      }
    } catch (err) {
      setError(err.message || 'Failed to process join request');
    } finally {
      setRequestingOrgId(null);
    }
  };

  const handleFriendReview = async (requestId, decision) => {
    setProcessingFriendRequestId(requestId);
    setError(null);
    setSuccessMessage(null);

    try {
      if (decision === 'accept') {
        await acceptFriendRequest(requestId);
        setSuccessMessage('Friend request accepted');
      } else {
        await rejectFriendRequest(requestId);
        setSuccessMessage('Friend request rejected');
      }
      setIncomingFriendRequests((prev) => prev.filter((r) => r.id !== requestId));
    } catch (err) {
      setError(err.message || 'Failed to process friend request');
    } finally {
      setProcessingFriendRequestId(null);
    }
  };

  const handleViewEvent = (event) => {
    const eventId = event?.id || event?.eventId;
    const orgId = event?.organizationId || event?.orgId || event?.organization?.id;

    if (!eventId) {
      setError('Event ID is missing');
      return;
    }

    if (orgId && myOrgIds.includes(orgId)) {
      navigate(`/org/events/${eventId}?orgId=${orgId}`);
      return;
    }

    navigate(`/events/${eventId}`);
  };

  const handleInviteFriend = async (friendUserId) => {
    const targetOrgId = inviteOrgIdByFriendId[friendUserId];
    if (!targetOrgId) {
      setError('Please choose an organization before inviting');
      return;
    }

    const busyKey = `${friendUserId}:${targetOrgId}`;
    setInvitingKey(busyKey);
    setError(null);
    setSuccessMessage(null);

    try {
      try {
        await createOrganizationInvitation(targetOrgId, { receiverUserId: friendUserId });
        setSuccessMessage('Invitation sent. Waiting for confirmation.');
      } catch (inviteErr) {
        const msg = (inviteErr?.message || '').toLowerCase();
        if (msg.includes('permission to invite members')) {
          await createOrganizationInvitationRecommendation(targetOrgId, { receiverUserId: friendUserId });
          setSuccessMessage('Recommendation sent to leaders for review.');
        } else {
          throw inviteErr;
        }
      }
    } catch (err) {
      setError(err.message || 'Failed to invite friend');
    } finally {
      setInvitingKey(null);
    }
  };

  const handleMyInvitationAction = async (invitationId, action) => {
    setProcessingInvitationId(invitationId);
    setError(null);
    setSuccessMessage(null);
    try {
      if (action === 'accept') {
        await acceptMyInvitation(invitationId);
        setSuccessMessage('Invitation accepted');
      } else {
        await rejectMyInvitation(invitationId);
        setSuccessMessage('Invitation rejected');
      }
      await loadData();
    } catch (err) {
      setError(err.message || 'Failed to process invitation');
    } finally {
      setProcessingInvitationId(null);
    }
  };

  const handleSendFriendRequest = async (receiverId) => {
    setSendingFriendRequestToUserId(receiverId);
    setError(null);
    setSuccessMessage(null);
    try {
      await sendFriendRequest({ receiverId });
      setSuccessMessage('Friend request sent');
      setSentFriendRequestUserIds((prev) => {
        const next = new Set(prev);
        next.add(receiverId);
        return next;
      });
    } catch (err) {
      setError(err.message || 'Failed to send friend request');
    } finally {
      setSendingFriendRequestToUserId(null);
    }
  };

  const filteredOrganizations = useMemo(() => {
    const keyword = orgSearch.trim().toLowerCase();

    return organizations.filter((org) => {
      const orgName = (org.name || org.orgName || org.organizationName || '').toLowerCase();
      const orgDesc = (org.description || '').toLowerCase();
      const isJoined = !!org.isJoined || myOrgIds.includes(org.id);
      const isPending = pendingJoinOrgIds.has(org.id);

      const matchesKeyword = !keyword || orgName.includes(keyword) || orgDesc.includes(keyword);
      if (!matchesKeyword) return false;

      if (orgFilter === 'joined') return isJoined;
      if (orgFilter === 'pending') return !isJoined && isPending;
      if (orgFilter === 'available') return !isJoined && !isPending;
      return true;
    });
  }, [organizations, orgSearch, orgFilter, myOrgIds, pendingJoinOrgIds]);

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader title="Discover" description="Find organizations, events, and requests" />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader title="Discover" description="Find organizations, events, and requests" />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page discover-page">
      <PageHeader
        title="Discover"
        description="Find organizations, events, and pending friend requests"
        actions={
          <button className="app-button app-button--secondary" onClick={loadData}>
            Refresh
          </button>
        }
      />

      <div className="app-section">
        {successMessage ? (
          <div className="discover-success">
            <p>{successMessage}</p>
          </div>
        ) : null}
        {partialWarning ? (
          <div className="discover-success" style={{ background: '#fffbeb', borderColor: '#fde68a' }}>
            <p style={{ color: '#92400e' }}>{partialWarning}</p>
          </div>
        ) : null}

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">My Organization Invitations</h3>
          </div>

          {myInvitations.length === 0 ? (
            <EmptyState message="No invitations yet" />
          ) : (
            <div className="discover-list">
              {myInvitations.map((item) => {
                const isPending = item.status === 'Pending';
                const isBusy = processingInvitationId === item.invitationId;
                return (
                  <div key={item.invitationId} className="discover-list-item">
                    <div>
                      <div className="discover-item-title">{item.organizationName}</div>
                      <div className="discover-item-meta">Inviter: {item.inviterName || '-'}</div>
                      <div className="discover-item-meta">Message: {item.message || '-'}</div>
                      <div className="discover-item-meta">Status: {item.status}</div>
                    </div>
                    <div className="discover-actions">
                      {isPending ? (
                        <>
                          <button
                            className="app-button app-button--primary"
                            disabled={isBusy}
                            onClick={() => handleMyInvitationAction(item.invitationId, 'accept')}
                          >
                            {isBusy ? 'Processing...' : 'Accept'}
                          </button>
                          <button
                            className="app-button app-button--danger"
                            disabled={isBusy}
                            onClick={() => handleMyInvitationAction(item.invitationId, 'reject')}
                          >
                            Reject
                          </button>
                        </>
                      ) : (
                        <button
                          className="app-button app-button--secondary"
                          onClick={() => navigate(`/org/overview?orgId=${item.organizationId}`)}
                        >
                          View organization
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Friend Suggestions</h3>
          </div>

          {friendSuggestions.length === 0 ? (
            <EmptyState message="No friend suggestions right now" />
          ) : (
            <div className="discover-list">
              {friendSuggestions.map((user) => {
                const isSending = sendingFriendRequestToUserId === user.userId;
                const isSent = sentFriendRequestUserIds.has(user.userId);
                return (
                  <div key={user.userId} className="discover-list-item">
                    <div>
                      <div className="discover-item-title">{user.fullName}</div>
                      <div className="discover-item-meta">{user.email || '-'}</div>
                    </div>
                    <div className="discover-actions">
                      <button
                        className={`app-button ${isSent ? 'app-button--secondary' : 'app-button--primary'}`}
                        disabled={isSending || isSent}
                        onClick={() => handleSendFriendRequest(user.userId)}
                      >
                        {isSending ? 'Sending...' : (isSent ? 'Request sent' : 'Add friend')}
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Incoming Friend Requests</h3>
          </div>

          {incomingFriendRequests.length === 0 ? (
            <EmptyState message="No pending friend requests" />
          ) : (
            <div className="discover-list">
              {incomingFriendRequests.map((item) => {
                const isBusy = processingFriendRequestId === item.id;
                return (
                  <div key={item.id} className="discover-list-item">
                    <div>
                      <div className="discover-item-title">{item.senderName}</div>
                      <div className="discover-item-meta">Requested at: {item.createdAtUtc ? new Date(item.createdAtUtc).toLocaleString() : '-'}</div>
                    </div>
                    <div className="discover-actions">
                      <button
                        className="app-button app-button--primary"
                        disabled={isBusy}
                        onClick={() => handleFriendReview(item.id, 'accept')}
                      >
                        {isBusy ? 'Processing...' : 'Accept'}
                      </button>
                      <button
                        className="app-button app-button--danger"
                        disabled={isBusy}
                        onClick={() => handleFriendReview(item.id, 'reject')}
                      >
                        Reject
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Invite Friends to Your Organizations</h3>
          </div>

          {friends.length === 0 ? (
            <EmptyState message="No friends available to invite" />
          ) : myOrganizations.length === 0 ? (
            <EmptyState message="You need to join at least one organization to invite friends" />
          ) : (
            <div className="discover-list">
              {friends.map((friend) => {
                const selectedOrgId = inviteOrgIdByFriendId[friend.userId] || '';
                const busyKey = `${friend.userId}:${selectedOrgId}`;
                const isBusy = invitingKey === busyKey;
                const availableOrgs = myOrganizations.filter((org) => {
                  const memberSet = orgMemberUserIdsMap[org.id];
                  return !(memberSet && memberSet.has(friend.userId));
                });

                return (
                  <div key={friend.userId} className="discover-list-item">
                    <div>
                      <div className="discover-item-title">{friend.fullName}</div>
                      <div className="discover-item-meta">{friend.email || '-'}</div>
                    </div>
                    <div className="discover-actions">
                      <select
                        className="form-select discover-filter"
                        value={selectedOrgId}
                        onChange={(e) =>
                          setInviteOrgIdByFriendId((prev) => ({
                            ...prev,
                            [friend.userId]: e.target.value
                          }))
                        }
                      >
                        <option value="">{availableOrgs.length > 0 ? 'Select organization' : 'No eligible organization'}</option>
                        {availableOrgs.map((org) => (
                          <option key={org.id} value={org.id}>
                            {org.name || org.orgName || 'Unnamed org'}
                          </option>
                        ))}
                      </select>
                      <button
                        className="app-button app-button--primary"
                        disabled={!selectedOrgId || isBusy || availableOrgs.length === 0}
                        onClick={() => handleInviteFriend(friend.userId)}
                      >
                        {isBusy ? 'Inviting...' : 'Invite'}
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="app-card">
          <div className="app-section-header discover-org-header">
            <h3 className="app-section-title">Organizations</h3>
            <div className="discover-org-controls">
              <input
                type="text"
                className="form-input discover-search"
                value={orgSearch}
                onChange={(e) => setOrgSearch(e.target.value)}
                placeholder="Search name or description"
              />
              <select className="form-select discover-filter" value={orgFilter} onChange={(e) => setOrgFilter(e.target.value)}>
                <option value="all">All</option>
                <option value="joined">Joined</option>
                <option value="pending">Pending</option>
                <option value="available">Available</option>
              </select>
            </div>
          </div>

          {filteredOrganizations.length === 0 ? (
            <EmptyState message="No organizations match your filter" />
          ) : (
            <div className="discover-org-grid">
              {filteredOrganizations.map((org) => {
                const orgName = org.name || org.orgName || org.organizationName || 'Unknown organization';
                const isJoined = !!org.isJoined || myOrgIds.includes(org.id);
                const isPending = pendingJoinOrgIds.has(org.id);
                const isWorking = requestingOrgId === org.id;

                return (
                  <div key={org.id} className="discover-org-card">
                    <div className="discover-item-title">{orgName}</div>
                    <div className="discover-item-meta">{org.description || '-'}</div>
                    <div className="discover-item-meta">Location: {org.location || '-'}</div>
                    <div className="discover-item-meta">Members: {org.totalMembers ?? '-'}</div>
                    <div className="discover-item-meta">Status: {org.status || (org.isActive ? 'Active' : 'Inactive')}</div>

                    <div className="discover-actions">
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
                        title={isJoined ? 'Open organization' : (isPending ? 'Click again to withdraw request' : undefined)}
                      >
                        {isWorking ? 'Processing...' : (isJoined ? 'View details' : (isPending ? 'Request sent (click to withdraw)' : 'Request to join'))}
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Events</h3>
          </div>
          {events.length === 0 ? (
            <EmptyState message="No events available" />
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
