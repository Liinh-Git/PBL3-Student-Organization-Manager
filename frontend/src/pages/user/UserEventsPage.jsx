import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getMyEvents } from "../../services/userService.js";
import {
  cancelEventRegistration,
  registerForEvent,
} from "../../services/eventService.js";
import PageHeader from "../../components/shared/PageHeader";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import "./UserEventsPage.css";

// ── Icons ──
const IconBuilding = () => (
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
    <rect x="3" y="3" width="18" height="18" rx="2" />
    <path d="M9 9h1" />
    <path d="M14 9h1" />
    <path d="M9 14h1" />
    <path d="M14 14h1" />
  </svg>
);

const IconTicket = () => (
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
    <rect x="2" y="7" width="20" height="10" rx="2" ry="2" />
    <path d="M12 11v2" />
    <path d="M17 7v10" />
    <path d="M7 7v10" />
  </svg>
);

const IconSearch = () => (
  <svg
    width="16"
    height="16"
    viewBox="0 0 24 24"
    fill="none"
    stroke="#94a3b8"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <circle cx="11" cy="11" r="8" />
    <line x1="21" y1="21" x2="16.65" y2="16.65" />
  </svg>
);

const IconGrid = () => (
  <svg
    width="16"
    height="16"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <rect x="3" y="3" width="7" height="7" rx="1" />
    <rect x="14" y="3" width="7" height="7" rx="1" />
    <rect x="14" y="14" width="7" height="7" rx="1" />
    <rect x="3" y="14" width="7" height="7" rx="1" />
  </svg>
);

const IconList = () => (
  <svg
    width="16"
    height="16"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <line x1="8" y1="6" x2="21" y2="6" />
    <line x1="8" y1="12" x2="21" y2="12" />
    <line x1="8" y1="18" x2="21" y2="18" />
    <line x1="3" y1="6" x2="3.01" y2="6" />
    <line x1="3" y1="12" x2="3.01" y2="12" />
    <line x1="3" y1="18" x2="3.01" y2="18" />
  </svg>
);

const IconClock = () => (
  <svg
    width="13"
    height="13"
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

const IconPin = () => (
  <svg
    width="13"
    height="13"
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

const IconBriefcase = () => (
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
    <rect x="2" y="7" width="20" height="14" rx="2" ry="2" />
    <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16" />
  </svg>
);

// ── Helpers ──
const DEFAULT_EVENT_BANNER =
  "data:image/svg+xml;utf8," +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='360' viewBox='0 0 1200 360'>" +
      "<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>" +
      "<stop offset='0%' stop-color='#fed7aa'/><stop offset='100%' stop-color='#ffedd5'/>" +
      "</linearGradient></defs>" +
      "<rect width='1200' height='360' fill='url(#g)'/>" +
      "<text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#ea580c' font-family='Segoe UI' font-size='42'>Banner sự kiện</text>" +
      "</svg>",
  );

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  let safeUrl = String(url).trim();
  if (!safeUrl) return "";
  safeUrl = safeUrl.replace(/\\/g, "/").replace(/^['\"]|['\"]$/g, "");
  if (/^https?:\/\//i.test(safeUrl)) return safeUrl;
  if (/^www\./i.test(safeUrl)) return `https://${safeUrl}`;

  const uploadsIndex = safeUrl.toLowerCase().indexOf("/uploads/");
  if (uploadsIndex >= 0) {
    safeUrl = safeUrl.slice(uploadsIndex);
  } else {
    const plainUploadsIndex = safeUrl.toLowerCase().indexOf("uploads/");
    if (plainUploadsIndex >= 0)
      safeUrl = `/${safeUrl.slice(plainUploadsIndex)}`;
  }

  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return safeUrl.startsWith("/")
    ? `${origin}${safeUrl}`
    : `${origin}/${safeUrl}`;
}

function parseDateBadge(dateStr) {
  if (!dateStr) return { month: "-", day: "-" };
  const d = new Date(dateStr);
  return {
    month: d.getMonth() + 1,
    day: d.getDate(),
  };
}

function formatTimeLine(startStr, endStr) {
  if (!startStr) return "-";
  const s = new Date(startStr);
  const sTime = s.toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
  });
  const sDate = s.toLocaleDateString("sv-SE"); // YYYY-MM-DD

  if (!endStr) return `${sTime} - ${sDate}`;
  const e = new Date(endStr);
  const eTime = e.toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
  });
  return `${sTime}   ${sDate}`;
}

function UserEventsPage() {
  const navigate = useNavigate();
  const [myEvents, setMyEvents] = useState([]);
  const [registrationMap, setRegistrationMap] = useState({});
  const [processingEventId, setProcessingEventId] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [activeTab, setActiveTab] = useState("member");
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    async function loadEvents() {
      setIsLoading(true);
      setError(null);
      try {
        const myEventData = await getMyEvents();
        const normalizedMyEvents = Array.isArray(myEventData)
          ? myEventData
          : [];
        setMyEvents(normalizedMyEvents);

        const attendeeRegistration = {};
        normalizedMyEvents.forEach((evt) => {
          if (evt?.participationRole === "Attendee") {
            attendeeRegistration[evt.id] =
              evt?.attendanceStatus !== "Cancelled";
          }
        });
        setRegistrationMap(attendeeRegistration);
      } catch (err) {
        setError(err.message || "Không thể tải danh sách sự kiện");
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, []);

  const getEventId = (evt) => evt?.id || evt?.eventId;
  const getEventStatus = (evt) => String(evt?.status || "").toLowerCase();
  const isEventJoinable = (evt) =>
    !["cancelled", "archived", "completed"].includes(getEventStatus(evt));

  const memberEvents = useMemo(
    () =>
      myEvents.filter((evt) => evt?.participationRole === "OrganizationMember"),
    [myEvents],
  );

  const attendeeEvents = useMemo(
    () => myEvents.filter((evt) => evt?.participationRole === "Attendee"),
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
    if (eventId) navigate(`/events/${eventId}`);
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
      setError(err.message || "Không thể cập nhật đăng ký");
    } finally {
      setProcessingEventId(null);
    }
  };

  let activeEventsList = activeTab === "member" ? memberEvents : attendeeEvents;

  if (searchTerm.trim() !== "") {
    const lowerTerm = searchTerm.toLowerCase();
    activeEventsList = activeEventsList.filter((evt) => {
      const title = (evt?.name || evt?.eventName || "").toLowerCase();
      return title.includes(lowerTerm);
    });
  }

  return (
    <div className="app-page user-events-page">
      {/* ── HEADER ── */}
      <div className="ue-header-layout">
        <div className="ue-header-text">
          <h1 className="ue-page-title">Sự kiện của tôi</h1>
          <p className="ue-page-desc">
            Quản lý các sự kiện bạn tham gia tổ chức hoặc đã ghi danh tham dự.
          </p>
        </div>

        <div className="ue-stats-box">
          <div className="ue-stat-item">
            <span className="ue-stat-label">TỔ CHỨC</span>
            <span className="ue-stat-value">{memberEvents.length}</span>
          </div>
          <div className="ue-stat-divider"></div>
          <div className="ue-stat-item">
            <span className="ue-stat-label">GHI DANH</span>
            <span className="ue-stat-value">{totalJoinedAsAttendee}</span>
          </div>
        </div>
      </div>

      {isLoading && <LoadingSpinner message="Đang tải danh sách sự kiện..." />}
      {error && <ErrorState message={error} />}

      {!isLoading && !error && (
        <div className="ue-main-content">
          {/* ── TOOLBAR (Tương tự thiết kế trong ảnh) ── */}
          <div className="ue-toolbar">
            <div className="ue-tabs-container">
              <button
                className={`ue-tab-btn ${activeTab === "member" ? "active" : ""}`}
                onClick={() => setActiveTab("member")}
              >
                <IconBuilding /> Ban tổ chức
              </button>
              <button
                className={`ue-tab-btn ${activeTab === "attendee" ? "active" : ""}`}
                onClick={() => setActiveTab("attendee")}
              >
                <IconTicket /> Khách tham dự
              </button>
            </div>

            <div className="ue-toolbar-right">
              <div className="ue-search-box">
                <IconSearch />
                <input
                  type="text"
                  placeholder="Tìm tên sự kiện..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
              <div className="ue-view-toggle">
                <button className="ue-view-btn active">
                  <IconGrid />
                </button>
                <button className="ue-view-btn">
                  <IconList />
                </button>
              </div>
            </div>
          </div>

          {/* ── CUSTOM EVENT CARDS GRID ── */}
          {activeEventsList.length === 0 ? (
            <EmptyState
              message={`Không tìm thấy sự kiện ${activeTab === "member" ? "tổ chức" : "ghi danh"} nào.`}
            />
          ) : (
            <div className="ue-cards-grid">
              {activeEventsList.map((evt) => {
                const eventId = getEventId(evt);
                const isRegistered = !!registrationMap[eventId];
                const isBusy = processingEventId === eventId;
                const isMemberTab = activeTab === "member";
                const title = evt?.name || evt?.eventName || "Untitled Event";
                const orgName =
                  evt?.organizationName ||
                  evt?.OrganizationName ||
                  evt?.orgName ||
                  evt?.OrgName ||
                  evt?.organization?.orgName ||
                  "Chưa rõ tổ chức";

                const bannerValue =
                  evt?.bannerUrl ??
                  evt?.BannerUrl ??
                  evt?.coverUrl ??
                  evt?.CoverUrl ??
                  evt?.avatarUrl;
                const bannerSrc =
                  toAbsoluteMediaUrl(bannerValue) || DEFAULT_EVENT_BANNER;

                const { month, day } = parseDateBadge(evt?.startDate);
                const timeLine = formatTimeLine(evt?.startDate, evt?.endDate);
                const location = evt?.location || "-";

                return (
                  <div key={eventId} className="ue-custom-card">
                    <div className="ue-card-cover-wrapper">
                      <img
                        src={bannerSrc}
                        alt={title}
                        className="ue-card-cover-img"
                      />

                      <div className="ue-badge-date">
                        <span className="ue-bd-month">THÁNG {month}</span>
                        <span className="ue-bd-day">{day}</span>
                      </div>

                      <div className="ue-badge-role">
                        {isMemberTab ? "Ban tổ chức" : "Khách tham dự"}
                      </div>
                    </div>

                    <div className="ue-card-body">
                      <div className="ue-card-org">
                        <IconBuilding />
                        <span className="truncate-text">{orgName}</span>
                      </div>
                      <h3 className="ue-card-title">{title}</h3>
                      <div className="ue-card-info-row">
                        <IconClock />
                        <span>{timeLine}</span>
                      </div>
                      <div className="ue-card-info-row">
                        <IconPin />
                        <span className="truncate-text">{location}</span>
                      </div>
                    </div>

                    <div className="ue-card-footer">
                      {isMemberTab ? (
                        <button
                          className="ue-btn ue-btn-primary"
                          onClick={() => handleOpenWorkspace(evt)}
                        >
                          <IconBriefcase /> Workspace
                        </button>
                      ) : (
                        <button
                          className={`ue-btn ${isRegistered ? "ue-btn-outline-primary" : "ue-btn-primary"}`}
                          onClick={() => handleToggleRegistration(evt)}
                          disabled={isBusy || !isEventJoinable(evt)}
                        >
                          {isBusy
                            ? "Đang xử lý..."
                            : isRegistered
                              ? "Hủy tham gia"
                              : "Đăng ký tham dự"}
                        </button>
                      )}

                      <button
                        className="ue-btn ue-btn-outline"
                        onClick={() => handleViewDetail(evt)}
                      >
                        Chi tiết
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default UserEventsPage;
