import { useState, useEffect, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { discoverMyOrganizations, getMyOrganizations } from '../../services/userService.js';
import { getOrganizationEvents, getPublicEvents } from '../../services/eventService.js';
import { getOrganizationMembers } from '../../services/memberService.js';
import { createOrganizationRequest, getMyPendingJoinRequests, withdrawOrganizationJoinRequest } from '../../services/requestService.js';
import {
  acceptFriendRequest,
  getFriendRequests,
  getMyOutgoingFriendRequests,
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
import OrgCard from '../../components/org/OrgCard.jsx';
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
  const [activeTab, setActiveTab] = useState('organizations');

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
      const mergedAvatarUrl =
        existing.avatarUrl ??
        existing.AvatarUrl ??
        org.avatarUrl ??
        org.AvatarUrl ??
        null;
      const mergedCoverUrl =
        existing.coverUrl ??
        existing.CoverUrl ??
        org.coverUrl ??
        org.CoverUrl ??
        null;
      mergedById.set(org.id, {
        ...existing,
        id: org.id,
        name: existing.name || org.name,
        orgName: existing.orgName || org.name,
        description: existing.description || org.description,
        avatarUrl: mergedAvatarUrl,
        coverUrl: mergedCoverUrl,
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
      const [orgIds, publicEvents, friendRequestsResult, outgoingRequestsResult, myFriendsResult, invitationsResult, suggestionsResult] = await Promise.all([
        syncMembershipAndPendingState(),
        getPublicEvents(),
        getFriendRequests().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err })),
        getMyOutgoingFriendRequests().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err })),
        getFriends().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err })),
        getMyInvitations().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err })),
        getFriendSuggestions().then((data) => ({ ok: true, data })).catch((err) => ({ ok: false, err }))
      ]);

      const friendRequests = friendRequestsResult.ok ? friendRequestsResult.data : [];
      const outgoingRequests = outgoingRequestsResult.ok ? outgoingRequestsResult.data : [];
      const myFriends = myFriendsResult.ok ? myFriendsResult.data : [];
      const invitations = invitationsResult.ok ? invitationsResult.data : [];
      const suggestions = suggestionsResult.ok ? suggestionsResult.data : [];

      const warnings = [];
      if (!friendRequestsResult.ok) warnings.push('friend requests');
      if (!outgoingRequestsResult.ok) warnings.push('outgoing friend requests');
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
      setSentFriendRequestUserIds(new Set((outgoingRequests || []).map((r) => r.receiverId).filter(Boolean)));

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

    if (!eventId) {
      setError('Event ID is missing');
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
        actions={<button className="app-button app-button--secondary" onClick={loadData}>Refresh</button>}
      />

      <div className="app-section discover-shell">
        {successMessage ? <div className="discover-success"><p>{successMessage}</p></div> : null}
        {partialWarning ? <div className="discover-success discover-success--warning"><p>{partialWarning}</p></div> : null}

        <div className="discover-layout">
          <aside className="discover-col discover-col--left">
            <div className="app-card discover-panel">
              <div className="app-section-header">
                <h3 className="app-section-title">Friend Requests</h3>
                <span className="app-badge app-badge--info">{incomingFriendRequests.length}</span>
              </div>
              {incomingFriendRequests.length === 0 ? <EmptyState message="No friend requests" /> : (
                <div className="discover-list">
                  {incomingFriendRequests.map((item) => {
                    const isBusy = processingFriendRequestId === item.id;
                    return (
                      <div key={item.id} className="discover-list-item discover-list-item--stacked">
                        <div>
                          <div className="discover-item-title">{item.senderName}</div>
                          <div className="discover-item-meta">Requested at: {item.createdAtUtc ? new Date(item.createdAtUtc).toLocaleString() : '-'}</div>
                        </div>
                        <div className="discover-actions discover-actions--full">
                          <button className="app-button app-button--primary discover-btn-half" disabled={isBusy} onClick={() => handleFriendReview(item.id, 'accept')}>{isBusy ? 'Processing...' : 'Accept'}</button>
                          <button className="app-button app-button--secondary discover-btn-half" disabled={isBusy} onClick={() => handleFriendReview(item.id, 'reject')}>Remove</button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            <div className="app-card discover-panel">
              <div className="app-section-header"><h3 className="app-section-title">Group Invitations</h3></div>
              {myInvitations.length === 0 ? <EmptyState message="No invitations" /> : (
                <div className="discover-list">
                  {myInvitations.map((item) => {
                    const isPending = item.status === 'Pending';
                    const isBusy = processingInvitationId === item.invitationId;
                    return (
                      <div key={item.invitationId} className="discover-list-item discover-list-item--stacked">
                        <div>
                          <div className="discover-item-title">{item.organizationName}</div>
                          <div className="discover-item-meta">Inviter: {item.inviterName || '-'}</div>
                          <div className="discover-item-meta">{item.message || '-'}</div>
                        </div>
                        <div className="discover-actions discover-actions--full">
                          {isPending ? (
                            <>
                              <button className="app-button app-button--primary discover-btn-half" disabled={isBusy} onClick={() => handleMyInvitationAction(item.invitationId, 'accept')}>{isBusy ? 'Processing...' : 'Accept'}</button>
                              <button className="app-button app-button--secondary discover-btn-half" disabled={isBusy} onClick={() => handleMyInvitationAction(item.invitationId, 'reject')}>Ignore</button>
                            </>
                          ) : (
                            <button className="app-button app-button--secondary" onClick={() => navigate(`/org/overview?orgId=${item.organizationId}`)}>View organization</button>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          </aside>

          <main className="discover-col discover-col--main">
            <div className="app-card discover-panel">
              <div className="discover-tabbar">
                <button className={`discover-tab ${activeTab === 'organizations' ? 'active' : ''}`} onClick={() => setActiveTab('organizations')}>Organizations</button>
                <button className={`discover-tab ${activeTab === 'people' ? 'active' : ''}`} onClick={() => setActiveTab('people')}>Community</button>
                <button className={`discover-tab ${activeTab === 'events' ? 'active' : ''}`} onClick={() => setActiveTab('events')}>Events</button>
              </div>

              {activeTab === 'organizations' ? (
                <>
                  <div className="discover-org-controls discover-org-controls--top">
                    <input type="text" className="form-input discover-search" value={orgSearch} onChange={(e) => setOrgSearch(e.target.value)} placeholder="Search organizations by name or description..." />
                    <select className="form-select discover-filter" value={orgFilter} onChange={(e) => setOrgFilter(e.target.value)}>
                      <option value="all">All</option>
                      <option value="joined">Joined</option>
                      <option value="pending">Pending</option>
                      <option value="available">Available</option>
                    </select>
                  </div>
                  {filteredOrganizations.length === 0 ? <EmptyState message="No organizations match your filter" /> : (
                    <div className="discover-org-grid">
                      {filteredOrganizations.map((org) => {
                        const orgName = org.name || org.orgName || org.organizationName || 'Unknown organization';
                        const isJoined = !!org.isJoined || myOrgIds.includes(org.id);
                        const isPending = pendingJoinOrgIds.has(org.id);
                        const isWorking = requestingOrgId === org.id;
                        const orgModel = {
                          ...org,
                          name: orgName,
                          orgName
                        };
                        return (
                          <div key={org.id} className={`discover-org-item ${isJoined ? 'discover-org-item--joined' : ''}`}>
                            <OrgCard organization={orgModel} />
                            <div className="discover-org-actions">
                              <button
                                onClick={() => {
                                  if (isJoined) {
                                    navigate(`/org/overview?orgId=${org.id}`);
                                    return;
                                  }
                                  handleRequestToJoin(org.id, orgName);
                                }}
                                disabled={isWorking}
                                className={`app-button ${isJoined || isPending ? 'app-button--secondary' : 'app-button--primary'} discover-btn-full`}
                                title={isJoined ? 'Open organization' : (isPending ? 'Click again to withdraw request' : undefined)}
                              >
                                {isWorking ? 'Processing...' : (isJoined ? 'Joined' : (isPending ? 'Request sent (click to withdraw)' : 'Join organization'))}
                              </button>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </>
              ) : null}

              {activeTab === 'people' ? (
                <div className="discover-community-grid">
                  <div className="app-card discover-subpanel">
                    <div className="app-section-header"><h3 className="app-section-title">Suggestions for you</h3></div>
                    {friendSuggestions.length === 0 ? <EmptyState message="No friend suggestions right now" /> : (
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
                                <button className={`app-button ${isSent ? 'app-button--secondary' : 'app-button--primary'}`} disabled={isSending || isSent} onClick={() => handleSendFriendRequest(user.userId)}>{isSending ? 'Sending...' : (isSent ? 'Request sent' : 'Add friend')}</button>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>

                  <div className="app-card discover-subpanel">
                    <div className="app-section-header"><h3 className="app-section-title">Invite Friends to Organizations</h3></div>
                    {friends.length === 0 ? <EmptyState message="No friends available to invite" /> : myOrganizations.length === 0 ? (
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
                              <div className="discover-actions discover-actions--stack">
                                <select className="form-select discover-filter discover-filter--full" value={selectedOrgId} onChange={(e) => setInviteOrgIdByFriendId((prev) => ({ ...prev, [friend.userId]: e.target.value }))}>
                                  <option value="">{availableOrgs.length > 0 ? 'Select organization' : 'No eligible organization'}</option>
                                  {availableOrgs.map((org) => (<option key={org.id} value={org.id}>{org.name || org.orgName || 'Unnamed org'}</option>))}
                                </select>
                                <button className="app-button app-button--primary discover-invite-button" disabled={!selectedOrgId || isBusy || availableOrgs.length === 0} onClick={() => handleInviteFriend(friend.userId)}>{isBusy ? 'Inviting...' : 'Invite'}</button>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                </div>
              ) : null}

              {activeTab === 'events' ? (
                events.length === 0 ? <EmptyState message="No events available" /> : (
                  <div className="discover-events-grid">
                    {events.map((event) => (<EventCard key={event?.id || event?.eventId} event={event} onView={() => handleViewEvent(event)} />))}
                  </div>
                )
              ) : null}
            </div>
          </main>

          <aside className="discover-col discover-col--right">
            <div className="app-card discover-panel">
              <div className="app-section-header"><h3 className="app-section-title">Featured Events</h3></div>
              {events.length === 0 ? <EmptyState message="No events available" /> : (
                <div className="discover-highlight-list">
                  {events.slice(0, 3).map((event) => (
                    <div key={event?.id || event?.eventId} className="discover-highlight-item" onClick={() => handleViewEvent(event)}>
                      <div className="discover-highlight-tag">Coming soon</div>
                      <div className="discover-item-title">{event?.title || event?.name || 'Untitled event'}</div>
                      <div className="discover-item-meta">{event?.location || '-'}</div>
                    </div>
                  ))}
                  <button className="app-button app-button--secondary discover-btn-full" onClick={() => setActiveTab('events')}>Explore all events</button>
                </div>
              )}
            </div>

            <div className="app-card discover-panel">
              <div className="app-section-header"><h3 className="app-section-title">Your Organizations</h3></div>
              {myOrganizations.length === 0 ? <EmptyState message="You have not joined any organization yet" /> : (
                <div className="discover-list">
                  {myOrganizations.slice(0, 4).map((org) => (
                    <div key={org.id} className="discover-list-item discover-list-item--compact">
                      <div className="discover-item-title">{org.name || org.orgName || 'Unnamed org'}</div>
                      <button className="app-button app-button--secondary" onClick={() => navigate(`/org/overview?orgId=${org.id}`)}>Open</button>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div className="discover-legal">
              <span>Privacy</span>
              <span>Terms</span>
              <span>Ads</span>
              <span>Cookies</span>
              <span>© 2026 SocialHub</span>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
}

export default UserDiscoverPage;
