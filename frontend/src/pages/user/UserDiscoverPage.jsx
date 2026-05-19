import { useState, useEffect, useMemo, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import {
  discoverMyOrganizations,
  getMyOrganizations,
  getMyEvents,
} from "../../services/userService.js";
import {
  cancelEventRegistration,
  getOrganizationEvents,
  getPublicEvents,
  registerForEvent,
} from "../../services/eventService.js";
import { getOrganizationMembers } from "../../services/memberService.js";
import {
  createOrganizationRequest,
  getMyPendingJoinRequests,
  withdrawOrganizationJoinRequest,
} from "../../services/requestService.js";
import {
  acceptFriendRequest,
  getFriendRequests,
  getMyOutgoingFriendRequests,
  getFriends,
  getFriendSuggestions,
  rejectFriendRequest,
  sendFriendRequest,
} from "../../services/friendService.js";
import {
  acceptMyInvitation,
  createOrganizationInvitation,
  createOrganizationInvitationRecommendation,
  getMyInvitations,
  rejectMyInvitation,
} from "../../services/invitationService.js";
import PageHeader from "../../components/shared/PageHeader";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import EventCard from "../../components/event/EventCard.jsx";
import OrgCard from "../../components/org/OrgCard.jsx";
import "./UserDiscoverPage.css";

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  if (url.startsWith("/")) return `${origin}${url}`;
  return `${origin}/${url}`;
}

// ── Icons ──────────────────────────────────────────────────────────────────
const IconUsers = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
    <circle cx="9" cy="7" r="4" />
    <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
    <path d="M16 3.13a4 4 0 0 1 0 7.75" />
  </svg>
);

const IconBuilding = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <rect x="3" y="3" width="18" height="18" rx="2" />
    <path d="M9 9h1" />
    <path d="M14 9h1" />
    <path d="M9 14h1" />
    <path d="M14 14h1" />
  </svg>
);

const IconCalendar = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <rect x="3" y="4" width="18" height="18" rx="2" />
    <line x1="16" y1="2" x2="16" y2="6" />
    <line x1="8" y1="2" x2="8" y2="6" />
    <line x1="3" y1="10" x2="21" y2="10" />
  </svg>
);

const IconStar = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="currentColor"
    stroke="none"
  >
    <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
  </svg>
);

const IconPin = () => (
  <svg
    width="12"
    height="12"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
    <circle cx="12" cy="10" r="3" />
  </svg>
);

const IconClock = () => (
  <svg
    width="12"
    height="12"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <circle cx="12" cy="12" r="10" />
    <polyline points="12 6 12 12 16 14" />
  </svg>
);

const IconCheck = () => (
  <svg
    width="13"
    height="13"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="3"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <polyline points="20 6 9 17 4 12" />
  </svg>
);

const IconX = () => (
  <svg
    width="12"
    height="12"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="3"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <line x1="18" y1="6" x2="6" y2="18" />
    <line x1="6" y1="6" x2="18" y2="18" />
  </svg>
);

const IconArrow = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <line x1="5" y1="12" x2="19" y2="12" />
    <polyline points="12 5 19 12 12 19" />
  </svg>
);

const IconMail = () => (
  <svg
    width="12"
    height="12"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
    <polyline points="22,6 12,13 2,6" />
  </svg>
);

// ── Toast ──────────────────────────────────────────────────────────────────
function Toast({ message, type = "success", onClose }) {
  useEffect(() => {
    const t = setTimeout(onClose, 1400);
    return () => clearTimeout(t);
  }, [message, onClose]);

  const icons = { success: "✓", warning: "⚠", error: "✕" };
  return (
    <div className={`discover-toast discover-toast--${type}`} onClick={onClose}>
      <span>{icons[type]}</span>
      <span>{message}</span>
      <button className="discover-toast-close" aria-label="Đóng">
        <IconX />
      </button>
    </div>
  );
}

// ── Popup modal ────────────────────────────────────────────────────────────
function PopupModal({ title, onClose, children }) {
  useEffect(() => {
    const onKey = (e) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [onClose]);

  return (
    <div className="discover-popup-overlay" onClick={onClose}>
      <div className="discover-popup" onClick={(e) => e.stopPropagation()}>
        <div className="discover-popup-header">
          <h3 className="discover-popup-title">{title}</h3>
          <button className="discover-popup-close" onClick={onClose}>
            <IconX />
          </button>
        </div>
        <div className="discover-popup-body">{children}</div>
      </div>
    </div>
  );
}

// ── Org card ───────────────────────────────────────────────────────────────
function DiscoverOrgCard({
  org,
  isJoined,
  isPending,
  isWorking,
  onAction,
  onOpen,
}) {
  const orgName =
    org.name || org.orgName || org.organizationName || "Tổ chức chưa xác định";
  const avatarSrc = toAbsoluteMediaUrl(org.avatarUrl || org.logoUrl || "");
  const coverSrc = toAbsoluteMediaUrl(org.coverUrl || org.bannerUrl || "");
  const initial = orgName.charAt(0).toUpperCase();

  // Pick a gradient per org (deterministic)
  const gradients = [
    "linear-gradient(135deg,#60a5fa 0%,#2563eb 60%,#7c3aed 100%)",
    "linear-gradient(135deg,#34d399 0%,#059669 60%,#0891b2 100%)",
    "linear-gradient(135deg,#fb923c 0%,#ea580c 60%,#dc2626 100%)",
    "linear-gradient(135deg,#a78bfa 0%,#7c3aed 60%,#2563eb 100%)",
    "linear-gradient(135deg,#f472b6 0%,#db2777 60%,#7c3aed 100%)",
  ];
  const grad = gradients[(orgName.charCodeAt(0) || 0) % gradients.length];

  return (
    <div className="dsc-org-card">
      <div
        className="dsc-org-cover"
        style={
          coverSrc
            ? {
                backgroundImage: `url(${coverSrc})`,
                backgroundSize: "cover",
                backgroundPosition: "center",
              }
            : { background: grad }
        }
      />
      <div className="dsc-org-avatar">
        {avatarSrc ? (
          <img
            src={avatarSrc}
            alt={orgName}
            onError={(e) => {
              e.target.style.display = "none";
              e.target.nextSibling.style.display = "flex";
            }}
          />
        ) : null}
        <span style={{ display: avatarSrc ? "none" : "flex" }}>{initial}</span>
      </div>
      <div className="dsc-org-body">
        <div className="dsc-org-name">{orgName}</div>
        <div className="dsc-org-meta">
          {org.location ? (
            <>
              <IconPin /> {org.location}
              {org.totalMembers != null ? " · " : ""}
            </>
          ) : (
            ""
          )}
          {org.totalMembers != null ? `${org.totalMembers} thành viên` : ""}
        </div>
        <div className="dsc-org-desc">{org.description || ""}</div>
        <div className="dsc-org-actions">
          {/* View button always shown */}
          <button
            className="dsc-btn dsc-btn--outline"
            onClick={() => onOpen(org.id)}
          >
            {isJoined ? "Xem tổ chức" : "Xem"} <IconArrow />
          </button>
          {/* Join/status button for non-members */}
          {!isJoined && (
            <button
              className={`dsc-btn ${isPending ? "dsc-btn--ghost" : "dsc-btn--primary"}`}
              disabled={isWorking}
              onClick={() => onAction(org.id, orgName)}
            >
              {isWorking
                ? "Đang xử lý…"
                : isPending
                  ? "Đã gửi · Rút lại"
                  : "Tham gia"}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

// ── Main page ──────────────────────────────────────────────────────────────
function UserDiscoverPage() {
  const navigate = useNavigate();

  const [organizations, setOrganizations] = useState([]);
  const [events, setEvents] = useState([]);
  const [myEventRoleMap, setMyEventRoleMap] = useState({});
  const [eventRegistrationMap, setEventRegistrationMap] = useState({});
  const [processingEventId, setProcessingEventId] = useState(null);
  const [myOrgIds, setMyOrgIds] = useState([]);
  const [myOrganizations, setMyOrganizations] = useState([]);
  const [orgMemberUserIdsMap, setOrgMemberUserIdsMap] = useState({});
  const [pendingJoinOrgIds, setPendingJoinOrgIds] = useState(new Set());
  const [incomingFriendRequests, setIncomingFriendRequests] = useState([]);
  const [friends, setFriends] = useState([]);
  const [friendSuggestions, setFriendSuggestions] = useState([]);
  const [sentFriendRequestUserIds, setSentFriendRequestUserIds] = useState(
    new Set(),
  );
  const [myInvitations, setMyInvitations] = useState([]);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  const [partialWarning, setPartialWarning] = useState(null);

  const [requestingOrgId, setRequestingOrgId] = useState(null);
  const [processingFriendRequestId, setProcessingFriendRequestId] =
    useState(null);
  const [sendingFriendRequestToUserId, setSendingFriendRequestToUserId] =
    useState(null);
  const [invitingKey, setInvitingKey] = useState(null);
  const [inviteOrgIdByFriendId, setInviteOrgIdByFriendId] = useState({});
  const [processingInvitationId, setProcessingInvitationId] = useState(null);

  const [orgSearch, setOrgSearch] = useState("");
  const [orgSearchInput, setOrgSearchInput] = useState("");
  const [orgFilter, setOrgFilter] = useState("all");
  const [activeTab, setActiveTab] = useState("organizations");
  const [popup, setPopup] = useState(null);

  // ── sync membership ──────────────────────────────────────────────────────
  const syncMembershipAndPendingState = useCallback(async () => {
    const [discoverableOrgs, myOrgs, pendingJoinRequests] = await Promise.all([
      discoverMyOrganizations(),
      getMyOrganizations(),
      getMyPendingJoinRequests(),
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
        status: existing.status || "Active",
        isJoined: true,
      });
    });

    setOrganizations(Array.from(mergedById.values()));

    const pendingIds = new Set(
      (pendingJoinRequests || []).map((r) => r.organizationId).filter(Boolean),
    );
    joinedOrgIds.forEach((id) => pendingIds.delete(id));
    setPendingJoinOrgIds(pendingIds);
    return joinedOrgIds;
  }, []);

  // ── load data ────────────────────────────────────────────────────────────
  const loadData = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    setPartialWarning(null);
    try {
      const [
        orgIds,
        publicEvents,
        myEvents,
        friendRequestsResult,
        outgoingRequestsResult,
        myFriendsResult,
        invitationsResult,
        suggestionsResult,
      ] = await Promise.all([
        syncMembershipAndPendingState(),
        getPublicEvents(),
        getMyEvents(),
        getFriendRequests()
          .then((d) => ({ ok: true, d }))
          .catch((e) => ({ ok: false, e })),
        getMyOutgoingFriendRequests()
          .then((d) => ({ ok: true, d }))
          .catch((e) => ({ ok: false, e })),
        getFriends()
          .then((d) => ({ ok: true, d }))
          .catch((e) => ({ ok: false, e })),
        getMyInvitations()
          .then((d) => ({ ok: true, d }))
          .catch((e) => ({ ok: false, e })),
        getFriendSuggestions()
          .then((d) => ({ ok: true, d }))
          .catch((e) => ({ ok: false, e })),
      ]);

      const friendRequests = friendRequestsResult.ok
        ? friendRequestsResult.d
        : [];
      const outgoingRequests = outgoingRequestsResult.ok
        ? outgoingRequestsResult.d
        : [];
      const myFriends = myFriendsResult.ok ? myFriendsResult.d : [];
      const invitations = invitationsResult.ok ? invitationsResult.d : [];
      const suggestions = suggestionsResult.ok ? suggestionsResult.d : [];

      const warnings = [];
      if (!friendRequestsResult.ok) warnings.push("lời mời kết bạn");
      if (!outgoingRequestsResult.ok) warnings.push("yêu cầu đã gửi");
      if (!myFriendsResult.ok) warnings.push("danh sách bạn bè");
      if (!invitationsResult.ok) warnings.push("lời mời tổ chức");
      if (!suggestionsResult.ok) warnings.push("gợi ý kết bạn");
      if (warnings.length > 0)
        setPartialWarning(`Không tải được: ${warnings.join(", ")}.`);

      setIncomingFriendRequests(
        (friendRequests || []).filter((r) => r.status === "Pending"),
      );
      setFriends(myFriends || []);
      setMyInvitations(invitations || []);
      setFriendSuggestions(suggestions || []);
      setSentFriendRequestUserIds(
        new Set(
          (outgoingRequests || []).map((r) => r.receiverId).filter(Boolean),
        ),
      );

      const memberMap = {};
      await Promise.all(
        (orgIds || []).map(async (id) => {
          try {
            const orgMembers = await getOrganizationMembers(id);
            memberMap[id] = new Set(
              (orgMembers || []).map((m) => m.userId).filter(Boolean),
            );
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
              organizationId: event.organizationId || orgId,
            }));
          } catch {
            return [];
          }
        }),
      );

      const mergedEvents = [
        ...(publicEvents || []),
        ...orgEventsResults.flat(),
      ];
      const uniqueEventMap = new Map();
      for (const event of mergedEvents) {
        const eventId = event?.id || event?.eventId;
        if (eventId && !uniqueEventMap.has(eventId))
          uniqueEventMap.set(eventId, event);
      }
      setEvents(Array.from(uniqueEventMap.values()));

      const roleMap = {};
      const registrationMap = {};
      (myEvents || []).forEach((evt) => {
        if (!evt?.id) return;
        if (evt?.participationRole) {
          roleMap[evt.id] = evt.participationRole;
        }
        if (evt?.participationRole === "Attendee") {
          registrationMap[evt.id] = evt?.attendanceStatus !== "Cancelled";
        }
      });
      setMyEventRoleMap(roleMap);
      setEventRegistrationMap(registrationMap);
    } catch (err) {
      setError(err.message || "Không tải được trang Khám phá");
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
    window.addEventListener("focus", onFocus);
    return () => window.removeEventListener("focus", onFocus);
  }, [syncMembershipAndPendingState]);

  // ── handlers ─────────────────────────────────────────────────────────────
  const handleRequestToJoin = async (orgId, orgName) => {
    const safeOrgName = orgName || "tổ chức này";
    if (myOrgIds.includes(orgId)) return;
    setRequestingOrgId(orgId);
    setSuccessMessage(null);
    setError(null);
    try {
      if (pendingJoinOrgIds.has(orgId)) {
        const withdrew = await withdrawOrganizationJoinRequest(orgId);
        if (withdrew) {
          setPendingJoinOrgIds((prev) => {
            const n = new Set(prev);
            n.delete(orgId);
            return n;
          });
          setSuccessMessage(`Đã rút yêu cầu tham gia ${safeOrgName}`);
        } else {
          setSuccessMessage(`Không tìm thấy yêu cầu chờ cho ${safeOrgName}`);
        }
      } else {
        await createOrganizationRequest(orgId, {
          requestType: "JoinOrganization",
          content: `I would like to join ${safeOrgName}`,
        });
        setPendingJoinOrgIds((prev) => {
          const n = new Set(prev);
          n.add(orgId);
          return n;
        });
        setSuccessMessage(`Đã gửi yêu cầu tham gia ${safeOrgName}`);
      }
    } catch (err) {
      setError(err.message || "Không xử lý được yêu cầu");
    } finally {
      setRequestingOrgId(null);
    }
  };

  const getDiscoverEventRole = useCallback(
    (event) => {
      const eventId = event?.id || event?.eventId;
      const roleFromMyEvents = eventId ? myEventRoleMap[eventId] : null;
      if (roleFromMyEvents === "OrganizationMember") return "OrganizationMember";
      if (roleFromMyEvents === "Attendee") return "Attendee";
      if (event?.organizationId && myOrgIds.includes(event.organizationId)) {
        return "OrganizationMember";
      }
      return "Attendee";
    },
    [myEventRoleMap, myOrgIds],
  );

  const handleToggleDiscoverEventRegistration = async (event) => {
    const eventId = event?.id || event?.eventId;
    if (!eventId) return;

    const status = String(event?.status || "").toLowerCase();
    if (["cancelled", "archived", "completed"].includes(status)) return;

    setProcessingEventId(eventId);
    setError(null);
    try {
      const isRegistered = !!eventRegistrationMap[eventId];
      if (isRegistered) {
        await cancelEventRegistration(eventId, {});
        setEventRegistrationMap((prev) => ({ ...prev, [eventId]: false }));
      } else {
        await registerForEvent(eventId, {});
        setEventRegistrationMap((prev) => ({ ...prev, [eventId]: true }));
      }
    } catch (err) {
      setError(err.message || "Không thể cập nhật trạng thái ghi danh");
    } finally {
      setProcessingEventId(null);
    }
  };

  const handleFriendReview = async (requestId, decision) => {
    setProcessingFriendRequestId(requestId);
    setError(null);
    setSuccessMessage(null);
    try {
      if (decision === "accept") {
        await acceptFriendRequest(requestId);
        setSuccessMessage("Đã chấp nhận lời mời kết bạn");
      } else {
        await rejectFriendRequest(requestId);
        setSuccessMessage("Đã từ chối lời mời kết bạn");
      }
      setIncomingFriendRequests((prev) =>
        prev.filter((r) => r.id !== requestId),
      );
    } catch (err) {
      setError(err.message || "Không xử lý được lời mời");
    } finally {
      setProcessingFriendRequestId(null);
    }
  };

  const handleViewEvent = (event) => {
    const eventId = event?.id || event?.eventId;
    const orgId =
      event?.organizationId || event?.orgId || event?.organization?.id;
    if (!eventId) {
      setError("Thiếu ID sự kiện");
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
      setError("Vui lòng chọn tổ chức trước khi mời");
      return;
    }
    const busyKey = `${friendUserId}:${targetOrgId}`;
    setInvitingKey(busyKey);
    setError(null);
    setSuccessMessage(null);
    try {
      try {
        await createOrganizationInvitation(targetOrgId, {
          receiverUserId: friendUserId,
        });
        setSuccessMessage("Đã gửi lời mời. Đang chờ xác nhận.");
      } catch (inviteErr) {
        const msg = (inviteErr?.message || "").toLowerCase();
        if (msg.includes("permission to invite members")) {
          await createOrganizationInvitationRecommendation(targetOrgId, {
            receiverUserId: friendUserId,
          });
          setSuccessMessage("Đã gửi đề xuất cho người quản lý xét duyệt.");
        } else {
          throw inviteErr;
        }
      }
    } catch (err) {
      setError(err.message || "Không gửi được lời mời");
    } finally {
      setInvitingKey(null);
    }
  };

  const handleMyInvitationAction = async (invitationId, action) => {
    setProcessingInvitationId(invitationId);
    setError(null);
    setSuccessMessage(null);
    try {
      if (action === "accept") {
        await acceptMyInvitation(invitationId);
        setSuccessMessage("Đã chấp nhận lời mời tổ chức");
      } else {
        await rejectMyInvitation(invitationId);
        setSuccessMessage("Đã từ chối lời mời tổ chức");
      }
      // Remove the item immediately after action (accept or reject)
      setMyInvitations((prev) =>
        prev.filter((i) => i.invitationId !== invitationId),
      );
    } catch (err) {
      setError(err.message || "Không xử lý được lời mời");
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
      setSuccessMessage("Đã gửi lời mời kết bạn");
      setSentFriendRequestUserIds((prev) => {
        const n = new Set(prev);
        n.add(receiverId);
        return n;
      });
    } catch (err) {
      setError(err.message || "Không gửi được lời mời kết bạn");
    } finally {
      setSendingFriendRequestToUserId(null);
    }
  };

  const filteredOrganizations = useMemo(() => {
    const keyword = orgSearch.trim().toLowerCase();
    return organizations.filter((org) => {
      const orgName = (
        org.name ||
        org.orgName ||
        org.organizationName ||
        ""
      ).toLowerCase();
      const orgDesc = (org.description || "").toLowerCase();
      const isJoined = !!org.isJoined || myOrgIds.includes(org.id);
      const isPending = pendingJoinOrgIds.has(org.id);
      if (keyword && !orgName.includes(keyword) && !orgDesc.includes(keyword))
        return false;
      if (orgFilter === "joined") return isJoined;
      if (orgFilter === "pending") return !isJoined && isPending;
      if (orgFilter === "available") return !isJoined && !isPending;
      return true;
    });
  }, [organizations, orgSearch, orgFilter, myOrgIds, pendingJoinOrgIds]);

  // ── loading / error states ────────────────────────────────────────────────
  if (isLoading)
    return (
      <div className="app-page">
        <PageHeader
          title="Khám phá"
          description="Tìm tổ chức, sự kiện và kết nối"
        />
        <LoadingSpinner />
      </div>
    );

  if (error && !successMessage)
    return (
      <div className="app-page">
        <PageHeader
          title="Khám phá"
          description="Tìm tổ chức, sự kiện và kết nối"
        />
        <ErrorState message={error} />
      </div>
    );

  const friendRequestCount = incomingFriendRequests.length;
  const pendingInvitationCount = myInvitations.filter(
    (i) => i.status === "Pending",
  ).length;

  // ── helpers ───────────────────────────────────────────────────────────────
  const formatStartTime = (dateString) => {
    if (!dateString) return null;
    const d = new Date(dateString);
    return d.toLocaleTimeString("vi-VN", {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const formatMonthDay = (dateString) => {
    if (!dateString) return null;
    const d = new Date(dateString);
    return {
      month: d.toLocaleString("vi-VN", { month: "short" }).toUpperCase(),
      day: d.getDate(),
    };
  };

  // ── render ────────────────────────────────────────────────────────────────
  return (
    <div className="app-page discover-page">
      <PageHeader
        title="Khám phá"
        description="Tìm tổ chức, sự kiện và kết nối mới"
        actions={
          <button
            className="app-button app-button--secondary"
            onClick={loadData}
          >
            Làm mới
          </button>
        }
      />

      {/* Toasts */}
      {successMessage && (
        <Toast
          message={successMessage}
          type="success"
          onClose={() => setSuccessMessage(null)}
        />
      )}
      {partialWarning && (
        <Toast
          message={partialWarning}
          type="warning"
          onClose={() => setPartialWarning(null)}
        />
      )}
      {error && (
        <Toast message={error} type="error" onClose={() => setError(null)} />
      )}

      <div className="app-section discover-shell">
        <div className="discover-layout">
          {/* ── MAIN CONTENT (left column) ── */}
          <main className="discover-col discover-col--main">
            <div className="app-card discover-panel">
              {/* Tab bar */}
              <div className="discover-tabbar">
                <button
                  className={`discover-tab ${activeTab === "organizations" ? "active" : ""}`}
                  onClick={() => setActiveTab("organizations")}
                >
                  <IconBuilding /> Tổ chức
                </button>
                <button
                  className={`discover-tab ${activeTab === "people" ? "active" : ""}`}
                  onClick={() => setActiveTab("people")}
                >
                  <IconUsers /> Cộng đồng
                </button>
                <button
                  className={`discover-tab ${activeTab === "events" ? "active" : ""}`}
                  onClick={() => setActiveTab("events")}
                >
                  <IconCalendar /> Sự kiện
                </button>
              </div>

              {/* Organizations tab */}
              {activeTab === "organizations" && (
                <>
                  <div className="discover-org-controls discover-org-controls--top">
                    <div className="discover-search-wrap">
                      <input
                        type="text"
                        className="discover-search"
                        value={orgSearchInput}
                        onChange={(e) => setOrgSearchInput(e.target.value)}
                        onKeyDown={(e) => {
                          if (e.key === "Enter") setOrgSearch(orgSearchInput);
                        }}
                        placeholder="Tìm kiếm tổ chức, câu lạc bộ..."
                      />
                      <button
                        className="discover-search-btn"
                        onClick={() => setOrgSearch(orgSearchInput)}
                        title="Tìm kiếm"
                      >
                        <svg
                          width="15"
                          height="15"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2.5"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        >
                          <circle cx="11" cy="11" r="8" />
                          <line x1="21" y1="21" x2="16.65" y2="16.65" />
                        </svg>
                      </button>
                    </div>
                    <select
                      className="discover-filter"
                      value={orgFilter}
                      onChange={(e) => setOrgFilter(e.target.value)}
                    >
                      <option value="all">Tất cả</option>
                      <option value="joined">Đã tham gia</option>
                      <option value="pending">Đang chờ</option>
                      <option value="available">Chưa tham gia</option>
                    </select>
                  </div>
                  {filteredOrganizations.length === 0 ? (
                    <EmptyState message="Không có tổ chức nào phù hợp" />
                  ) : (
                    <div className="discover-org-grid">
                      {filteredOrganizations.map((org) => {
                        const isJoined =
                          !!org.isJoined || myOrgIds.includes(org.id);
                        const isPending = pendingJoinOrgIds.has(org.id);
                        const isWorking = requestingOrgId === org.id;
                        const orgName =
                          org.name ||
                          org.orgName ||
                          org.organizationName ||
                          "Tổ chức chưa xác định";
                        return (
                          <DiscoverOrgCard
                            key={org.id}
                            org={{ ...org, name: orgName, orgName }}
                            isJoined={isJoined}
                            isPending={isPending}
                            isWorking={isWorking}
                            onAction={handleRequestToJoin}
                            onOpen={(id) =>
                              navigate(`/org/overview?orgId=${id}`)
                            }
                          />
                        );
                      })}
                    </div>
                  )}
                </>
              )}

              {/* People / Community tab */}
              {activeTab === "people" && (
                <div className="discover-community-grid">
                  {/* Friend suggestions */}
                  <div className="app-card discover-subpanel">
                    <div className="dsc-panel-header">
                      <h3 className="dsc-panel-title">
                        <span className="dsc-panel-title-icon dsc-panel-title-icon--blue">
                          <IconUsers />
                        </span>
                        Gợi ý kết bạn
                      </h3>
                    </div>
                    {friendSuggestions.length === 0 ? (
                      <EmptyState message="Không có gợi ý kết bạn nào" />
                    ) : (
                      <div className="discover-list">
                        {friendSuggestions.map((user) => {
                          const isSending =
                            sendingFriendRequestToUserId === user.userId;
                          const isSent = sentFriendRequestUserIds.has(
                            user.userId,
                          );
                          const initial = (user.fullName || "?")
                            .charAt(0)
                            .toUpperCase();
                          return (
                            <div
                              key={user.userId}
                              className="discover-list-item"
                            >
                              <div
                                style={{
                                  display: "flex",
                                  alignItems: "center",
                                  gap: "0.65rem",
                                }}
                              >
                                <div className="dsc-avatar-circle">
                                  {initial}
                                </div>
                                <div>
                                  <div className="discover-item-title">
                                    {user.fullName}
                                  </div>
                                  <div
                                    className="discover-item-meta"
                                    style={{
                                      display: "flex",
                                      alignItems: "center",
                                      gap: "0.25rem",
                                    }}
                                  >
                                    <IconMail /> {user.email || "-"}
                                  </div>
                                </div>
                              </div>
                              <div className="discover-actions">
                                <button
                                  className={`dsc-btn ${isSent ? "dsc-btn--ghost" : "dsc-btn--primary"}`}
                                  disabled={isSending || isSent}
                                  onClick={() =>
                                    handleSendFriendRequest(user.userId)
                                  }
                                >
                                  {isSending
                                    ? "Đang gửi…"
                                    : isSent
                                      ? "Đã gửi"
                                      : "Kết bạn"}
                                </button>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>

                  {/* Invite friends to org */}
                  <div className="app-card discover-subpanel">
                    <div className="dsc-panel-header">
                      <h3 className="dsc-panel-title">
                        <span className="dsc-panel-title-icon dsc-panel-title-icon--green">
                          <IconBuilding />
                        </span>
                        Mời bạn bè vào tổ chức
                      </h3>
                    </div>
                    {friends.length === 0 ? (
                      <EmptyState message="Chưa có bạn bè để mời" />
                    ) : myOrganizations.length === 0 ? (
                      <EmptyState message="Bạn cần tham gia ít nhất một tổ chức để mời bạn bè" />
                    ) : (
                      <div className="discover-list">
                        {friends.map((friend) => {
                          const selectedOrgId =
                            inviteOrgIdByFriendId[friend.userId] || "";
                          const busyKey = `${friend.userId}:${selectedOrgId}`;
                          const isBusy = invitingKey === busyKey;
                          const availableOrgs = myOrganizations.filter(
                            (org) => {
                              const memberSet = orgMemberUserIdsMap[org.id];
                              return !(
                                memberSet && memberSet.has(friend.userId)
                              );
                            },
                          );
                          const initial = (friend.fullName || "?")
                            .charAt(0)
                            .toUpperCase();

                          return (
                            <div
                              key={friend.userId}
                              className="dsc-invite-card"
                            >
                              <div className="dsc-invite-header">
                                <div className="dsc-avatar-circle">
                                  {initial}
                                </div>
                                <div style={{ flex: 1, minWidth: 0 }}>
                                  <div className="discover-item-title">
                                    {friend.fullName}
                                  </div>
                                  <div
                                    className="discover-item-meta"
                                    style={{
                                      display: "flex",
                                      alignItems: "center",
                                      gap: "0.25rem",
                                    }}
                                  >
                                    <IconMail /> {friend.email || "-"}
                                  </div>
                                </div>
                              </div>
                              <div className="dsc-invite-controls">
                                <select
                                  className="dsc-invite-select"
                                  value={selectedOrgId}
                                  disabled={availableOrgs.length === 0}
                                  onChange={(e) =>
                                    setInviteOrgIdByFriendId((prev) => ({
                                      ...prev,
                                      [friend.userId]: e.target.value,
                                    }))
                                  }
                                >
                                  <option value="">
                                    {availableOrgs.length > 0
                                      ? "Chọn tổ chức…"
                                      : "Không có tổ chức"}
                                  </option>
                                  {availableOrgs.map((org) => (
                                    <option key={org.id} value={org.id}>
                                      {org.name || org.orgName || "Unnamed org"}
                                    </option>
                                  ))}
                                </select>
                                <button
                                  className="dsc-btn dsc-btn--primary"
                                  disabled={
                                    !selectedOrgId ||
                                    isBusy ||
                                    availableOrgs.length === 0
                                  }
                                  onClick={() =>
                                    handleInviteFriend(friend.userId)
                                  }
                                >
                                  {isBusy ? "Đang mời…" : "Mời"}
                                </button>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* Events tab */}
              {activeTab === "events" &&
                (events.length === 0 ? (
                  <EmptyState message="Không có sự kiện nào" />
                ) : (
                  <div className="discover-events-grid">
                    {events.map((event) => {
                      const eventId = event?.id || event?.eventId;
                      const role = getDiscoverEventRole(event);
                      const isMemberEvent = role === "OrganizationMember";
                      const isRegistered = !!eventRegistrationMap[eventId];
                      const isBusy = processingEventId === eventId;
                      const status = String(event?.status || "").toLowerCase();
                      const canToggle = !["cancelled", "archived", "completed"].includes(status);

                      return (
                        <EventCard
                          key={eventId}
                          event={event}
                          showDetailButton={false}
                          footerActions={
                            <>
                              <button
                                type="button"
                                className="app-button app-button--ghost"
                                onClick={() => handleViewEvent(event)}
                              >
                                Xem chi tiết
                              </button>
                              {isMemberEvent ? (
                                <button
                                  type="button"
                                  className="app-button app-button--primary"
                                  onClick={() =>
                                    navigate(
                                      `/org/events/${eventId}?orgId=${event.organizationId}`,
                                    )
                                  }
                                  disabled={!event?.organizationId}
                                >
                                  Vào không gian làm việc
                                </button>
                              ) : (
                                <button
                                  type="button"
                                  className={`app-button ${isRegistered ? "app-button--secondary" : "app-button--primary"}`}
                                  onClick={() => handleToggleDiscoverEventRegistration(event)}
                                  disabled={isBusy || !canToggle}
                                >
                                  {isBusy
                                    ? "Đang xử lý..."
                                    : isRegistered
                                      ? "Hủy tham gia"
                                      : "Tham gia"}
                                </button>
                              )}
                            </>
                          }
                        />
                      );
                    })}
                  </div>
                ))}
            </div>
          </main>

          {/* ── RIGHT SIDEBAR ── */}
          <aside className="discover-col discover-col--right">
            {/* Friend Requests */}
            <div className="app-card discover-panel">
              <div className="dsc-panel-header">
                <h3 className="dsc-panel-title">
                  <span className="dsc-panel-title-icon dsc-panel-title-icon--blue">
                    <IconUsers />
                  </span>
                  Lời mời kết bạn
                </h3>
                {friendRequestCount > 0 && (
                  <span className="dsc-badge">{friendRequestCount}</span>
                )}
              </div>

              {incomingFriendRequests.length === 0 ? (
                <EmptyState message="Không có lời mời kết bạn" />
              ) : (
                <>
                  {incomingFriendRequests.slice(0, 3).map((item) => {
                    const isBusy = processingFriendRequestId === item.id;
                    const initial = (item.senderName || "?")
                      .charAt(0)
                      .toUpperCase();
                    return (
                      <div key={item.id} className="dsc-friend-row">
                        <div className="dsc-avatar-circle">{initial}</div>
                        <div className="dsc-friend-info">
                          <div className="discover-item-title">
                            {item.senderName}
                          </div>
                          <div
                            className="discover-item-meta"
                            style={{
                              display: "flex",
                              alignItems: "center",
                              gap: "0.25rem",
                            }}
                          >
                            <IconMail />
                            {item.senderEmail ||
                              (item.createdAtUtc
                                ? new Date(
                                    item.createdAtUtc,
                                  ).toLocaleDateString("vi-VN")
                                : "")}
                          </div>
                        </div>
                        <div className="dsc-friend-btns">
                          <button
                            className="dsc-btn dsc-btn--confirm"
                            disabled={isBusy}
                            onClick={() =>
                              handleFriendReview(item.id, "accept")
                            }
                            title="Chấp nhận"
                          >
                            <IconCheck />
                          </button>
                          <button
                            className="dsc-btn dsc-btn--deny"
                            disabled={isBusy}
                            onClick={() =>
                              handleFriendReview(item.id, "reject")
                            }
                            title="Từ chối"
                          >
                            <IconX />
                          </button>
                        </div>
                      </div>
                    );
                  })}
                  {friendRequestCount > 3 && (
                    <button
                      className="dsc-see-all-btn"
                      onClick={() => setPopup("friendRequests")}
                    >
                      Xem tất cả ({friendRequestCount})
                    </button>
                  )}
                </>
              )}
            </div>

            {/* Org Invitations */}
            <div className="app-card discover-panel">
              <div className="dsc-panel-header">
                <h3 className="dsc-panel-title">
                  <span className="dsc-panel-title-icon dsc-panel-title-icon--green">
                    <IconBuilding />
                  </span>
                  Lời mời tổ chức
                </h3>
                {pendingInvitationCount > 0 && (
                  <span className="dsc-badge">{pendingInvitationCount}</span>
                )}
              </div>

              {myInvitations.length === 0 ? (
                <EmptyState message="Không có lời mời tổ chức" />
              ) : (
                <>
                  {myInvitations.slice(0, 3).map((item) => {
                    const isPending = item.status === "Pending";
                    const isBusy = processingInvitationId === item.invitationId;
                    const initial = (item.organizationName || "?")
                      .charAt(0)
                      .toUpperCase();
                    return (
                      <div key={item.invitationId} className="dsc-invite-row">
                        <div className="dsc-avatar-circle dsc-avatar-circle--org">
                          {initial}
                        </div>
                        <div className="dsc-friend-info">
                          <div className="discover-item-title">
                            {item.organizationName}
                          </div>
                          <div className="discover-item-meta">
                            Từ: {item.inviterName || "-"}
                          </div>
                        </div>
                        {isPending && (
                          <div className="dsc-friend-btns">
                            <button
                              className="dsc-btn dsc-btn--confirm"
                              disabled={isBusy}
                              onClick={() =>
                                handleMyInvitationAction(
                                  item.invitationId,
                                  "accept",
                                )
                              }
                              title="Chấp nhận"
                            >
                              {isBusy ? "…" : <IconCheck />}
                            </button>
                            <button
                              className="dsc-btn dsc-btn--deny"
                              disabled={isBusy}
                              onClick={() =>
                                handleMyInvitationAction(
                                  item.invitationId,
                                  "reject",
                                )
                              }
                              title="Từ chối"
                            >
                              <IconX />
                            </button>
                          </div>
                        )}
                      </div>
                    );
                  })}
                  {myInvitations.length > 3 && (
                    <button
                      className="dsc-see-all-btn"
                      onClick={() => setPopup("invitations")}
                    >
                      Xem tất cả ({myInvitations.length})
                    </button>
                  )}
                </>
              )}
            </div>

            {/* Featured events */}
            <div className="app-card discover-panel">
              <div className="dsc-panel-header">
                <h3 className="dsc-panel-title">
                  <span
                    className="dsc-panel-title-icon dsc-panel-title-icon--amber"
                    style={{ color: "#d97706" }}
                  >
                    <IconStar />
                  </span>
                  Sự kiện nổi bật
                </h3>
              </div>
              {events.length === 0 ? (
                <EmptyState message="Không có sự kiện nào" />
              ) : (
                <div className="discover-highlight-list">
                  {events.slice(0, 4).map((event) => {
                    const startDate = event?.startDate
                      ? new Date(event.startDate)
                      : null;
                    const md = startDate
                      ? formatMonthDay(event.startDate)
                      : null;
                    const time = startDate
                      ? formatStartTime(event.startDate)
                      : null;
                    return (
                      <div
                        key={event?.id || event?.eventId}
                        className="dsc-event-highlight"
                        onClick={() => handleViewEvent(event)}
                      >
                        {md && (
                          <div className="dsc-event-date">
                            <span className="dsc-event-month">{md.month}</span>
                            <span className="dsc-event-day">{md.day}</span>
                          </div>
                        )}
                        <div className="dsc-event-info">
                          <div
                            className="discover-item-title"
                            style={{ fontSize: "0.85rem" }}
                          >
                            {event?.title || event?.name || "Sự kiện"}
                          </div>
                          {time && (
                            <div className="dsc-event-time">
                              <IconClock /> {time}
                            </div>
                          )}
                          {event?.location && (
                            <div
                              className="discover-item-meta"
                              style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "0.2rem",
                              }}
                            >
                              <IconPin /> {event.location}
                            </div>
                          )}
                        </div>
                      </div>
                    );
                  })}
                  <button
                    className="dsc-btn dsc-btn--ghost dsc-btn--full"
                    style={{ marginTop: "0.25rem" }}
                    onClick={() => setPopup("events")}
                  >
                    Xem tất cả sự kiện →
                  </button>
                </div>
              )}
            </div>

            {/* Legal */}
            <div className="discover-legal">
              <span>Quyền riêng tư</span>
              <span>Điều khoản</span>
              <span>Quảng cáo</span>
              <span>Cookie</span>
              <span>© 2026 SocialHub</span>
            </div>
          </aside>
        </div>
      </div>

      {/* POPUP: All friend requests */}
      {popup === "friendRequests" && (
        <PopupModal
          title={`Lời mời kết bạn (${friendRequestCount})`}
          onClose={() => setPopup(null)}
        >
          <div className="discover-list">
            {incomingFriendRequests.map((item) => {
              const isBusy = processingFriendRequestId === item.id;
              const initial = (item.senderName || "?").charAt(0).toUpperCase();
              return (
                <div key={item.id} className="dsc-friend-row">
                  <div className="dsc-avatar-circle">{initial}</div>
                  <div className="dsc-friend-info">
                    <div className="discover-item-title">{item.senderName}</div>
                    <div
                      className="discover-item-meta"
                      style={{
                        display: "flex",
                        alignItems: "center",
                        gap: "0.25rem",
                      }}
                    >
                      <IconMail />
                      {item.senderEmail ||
                        (item.createdAtUtc
                          ? new Date(item.createdAtUtc).toLocaleDateString(
                              "vi-VN",
                            )
                          : "")}
                    </div>
                  </div>
                  <div className="dsc-friend-btns">
                    <button
                      className="dsc-btn dsc-btn--primary dsc-btn--sm"
                      disabled={isBusy}
                      onClick={() => handleFriendReview(item.id, "accept")}
                    >
                      {isBusy ? "…" : "Chấp nhận"}
                    </button>
                    <button
                      className="dsc-btn dsc-btn--ghost dsc-btn--sm"
                      disabled={isBusy}
                      onClick={() => handleFriendReview(item.id, "reject")}
                    >
                      Từ chối
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        </PopupModal>
      )}

      {/* POPUP: All invitations */}
      {popup === "invitations" && (
        <PopupModal
          title={`Lời mời tổ chức (${myInvitations.length})`}
          onClose={() => setPopup(null)}
        >
          <div className="discover-list">
            {myInvitations.map((item) => {
              const isPending = item.status === "Pending";
              const isBusy = processingInvitationId === item.invitationId;
              const initial = (item.organizationName || "?")
                .charAt(0)
                .toUpperCase();
              return (
                <div key={item.invitationId} className="dsc-invite-row">
                  <div className="dsc-avatar-circle dsc-avatar-circle--org">
                    {initial}
                  </div>
                  <div className="dsc-friend-info">
                    <div className="discover-item-title">
                      {item.organizationName}
                    </div>
                    <div className="discover-item-meta">
                      Từ: {item.inviterName || "-"}
                    </div>
                    {item.message && (
                      <div className="discover-item-meta">{item.message}</div>
                    )}
                  </div>
                  {isPending ? (
                    <div className="dsc-friend-btns">
                      <button
                        className="dsc-btn dsc-btn--primary dsc-btn--sm"
                        disabled={isBusy}
                        onClick={() =>
                          handleMyInvitationAction(item.invitationId, "accept")
                        }
                      >
                        {isBusy ? "…" : "Chấp nhận"}
                      </button>
                      <button
                        className="dsc-btn dsc-btn--ghost dsc-btn--sm"
                        disabled={isBusy}
                        onClick={() =>
                          handleMyInvitationAction(item.invitationId, "reject")
                        }
                      >
                        Từ chối
                      </button>
                    </div>
                  ) : (
                    <button
                      className="dsc-btn dsc-btn--outline dsc-btn--sm"
                      onClick={() =>
                        navigate(`/org/overview?orgId=${item.organizationId}`)
                      }
                    >
                      Xem
                    </button>
                  )}
                </div>
              );
            })}
          </div>
        </PopupModal>
      )}

      {/* POPUP: All events */}
      {popup === "events" && (
        <PopupModal
          title={`Tất cả sự kiện (${events.length})`}
          onClose={() => setPopup(null)}
        >
          <div className="discover-list">
            {events.map((event) => {
              const startDate = event?.startDate
                ? new Date(event.startDate)
                : null;
              const md = startDate ? formatMonthDay(event.startDate) : null;
              const time = startDate ? formatStartTime(event.startDate) : null;
              return (
                <div
                  key={event?.id || event?.eventId}
                  className="discover-list-item"
                  onClick={() => handleViewEvent(event)}
                  style={{ cursor: "pointer" }}
                >
                  <div style={{ display: "flex", gap: "0.65rem" }}>
                    {md && (
                      <div className="dsc-event-date">
                        <span className="dsc-event-month">{md.month}</span>
                        <span className="dsc-event-day">{md.day}</span>
                      </div>
                    )}
                    <div>
                      <div className="discover-item-title">
                        {event?.title || event?.name || "Sự kiện"}
                      </div>
                      {time && (
                        <div className="discover-item-meta">
                          <IconClock /> {time}
                        </div>
                      )}
                      {event?.location && (
                        <div className="discover-item-meta">
                          <IconPin /> {event.location}
                        </div>
                      )}
                    </div>
                  </div>
                  <button className="dsc-btn dsc-btn--outline dsc-btn--sm">
                    Xem
                  </button>
                </div>
              );
            })}
          </div>
        </PopupModal>
      )}
    </div>
  );
}

export default UserDiscoverPage;
