import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner.jsx";
import ErrorState from "../../components/shared/ErrorState.jsx";
import {
  getEventById,
  getPublicEventById,
  updateEvent,
} from "../../services/eventService.js";
import { getMyPermissions } from "../../services/roleService.js";
import {
  getMyEventRegistration,
  joinEvent,
} from "../../services/attendeeService.js";
import "./EventDetailPage.css";

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return url.startsWith("/") ? `${origin}${url}` : `${origin}/${url}`;
}

function toDateTimeLocalInput(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

function toIsoUtcFromLocalInput(value) {
  if (!value) return null;
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return null;
  return date.toISOString();
}

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleString("vi-VN");
}

function formatDateOnly(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleDateString("vi-VN", {
    weekday: "long",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

function formatTimeOnly(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" });
}

function formatMaybe(value) {
  if (value === null || value === undefined || value === "") return "-";
  return String(value);
}

function buildDraft(eventData) {
  return {
    eventName: eventData?.name || eventData?.eventName || "",
    description: eventData?.description || "",
    startDate: toDateTimeLocalInput(eventData?.startDate),
    endDate: toDateTimeLocalInput(eventData?.endDate),
    location: eventData?.location || "",
    bannerUrl: eventData?.bannerUrl || "",
    visibility: eventData?.visibility || "Private",
    targetParticipants: eventData?.targetParticipants ?? "",
  };
}

function getStatusLabel(status) {
  const normalized = String(status || "").toLowerCase();
  if (["published", "active", "ongoing"].includes(normalized)) return "Đang diễn ra";
  if (["draft", "planned"].includes(normalized)) return "Bản nháp";
  if (["completed", "archived"].includes(normalized)) return "Đã kết thúc";
  if (["cancelled"].includes(normalized)) return "Đã hủy";
  return status || "Không rõ";
}

function getStatusTone(status) {
  const normalized = String(status || "").toLowerCase();
  if (["published", "active", "ongoing"].includes(normalized)) return "live";
  if (["draft", "planned"].includes(normalized)) return "draft";
  if (["cancelled"].includes(normalized)) return "danger";
  return "neutral";
}

function EventDetailPage() {
  const { eventId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { isAuthenticated, isLoading: authLoading } = useAuth();

  const [eventData, setEventData] = useState(null);
  const [sourceMode, setSourceMode] = useState("public");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [permissions, setPermissions] = useState([]);
  const [viewMode, setViewMode] = useState("preview");
  const [draft, setDraft] = useState(null);
  const [editingField, setEditingField] = useState(null);
  const [isSaving, setIsSaving] = useState(false);
  const [joinState, setJoinState] = useState({ isRegistered: false, status: null, isEventMember: false });
  const [isJoining, setIsJoining] = useState(false);
  const orgId = searchParams.get("orgId");
  const from = searchParams.get("from");
  const startMode = searchParams.get("mode");
  const handleBack = () => {
    if (from === "org-workspace" && orgId) {
      navigate(`/org/events/${eventId}?orgId=${orgId}`);
      return;
    }
    navigate(-1);
  };

  useEffect(() => {
    if (authLoading) return;
    if (!eventId) {
      setError("Event ID is missing");
      setIsLoading(false);
      return;
    }

    let isMounted = true;
    async function loadEventDetail() {
      setIsLoading(true);
      setError(null);
      setEditingField(null);

      try {
        let loadedEvent = null;
        let mode = "public";

        if (isAuthenticated) {
          try {
            loadedEvent = await getEventById(eventId);
            mode = "workspace";
          } catch {
            // fallback to public read route
          }
        }

        if (!loadedEvent) {
          loadedEvent = await getPublicEventById(eventId);
          mode = "public";
        }

        const nextPermissions = [];
        if (isAuthenticated && loadedEvent?.organizationId) {
          try {
            const permissionData = await getMyPermissions(loadedEvent.organizationId);
            if (Array.isArray(permissionData?.permissionKeys)) {
              nextPermissions.push(...permissionData.permissionKeys);
            }
          } catch {
            // keep as empty for non-member
          }
        }

        let registration = { isRegistered: false, status: null, isEventMember: false };
        if (isAuthenticated) {
          try {
            const response = await getMyEventRegistration(eventId);
            registration = {
              isRegistered: !!response?.isRegistered && response?.status !== "Cancelled",
              status: response?.status || null,
              isEventMember: !!response?.isEventMember,
            };
          } catch {
            registration = { isRegistered: false, status: null, isEventMember: false };
          }
        }

        if (!isMounted) return;
        setEventData(loadedEvent);
        setDraft(buildDraft(loadedEvent));
        setPermissions(nextPermissions);
        setJoinState(registration);
        setSourceMode(mode);
      } catch (err) {
        if (!isMounted) return;
        setError(err.message || "Failed to load event detail");
      } finally {
        if (isMounted) setIsLoading(false);
      }
    }

    loadEventDetail();

    return () => {
      isMounted = false;
    };
  }, [authLoading, eventId, isAuthenticated]);

  const canEditEvent = useMemo(
    () => permissions.includes("org.events.create") && permissions.includes("org.events.manage"),
    [permissions],
  );

  const canJoin = useMemo(() => {
    if (!eventData) return false;
    const status = String(eventData.status || "");
    return !["Cancelled", "Archived", "Completed"].includes(status);
  }, [eventData]);

  const statusTone = useMemo(() => getStatusTone(eventData?.status), [eventData?.status]);
  const participantSummary = useMemo(() => {
    const registered = Number(eventData?.registeredParticipants ?? 0);
    const safeRegistered = Number.isFinite(registered) ? Math.max(0, registered) : 0;
    if (
      eventData?.targetParticipants === null ||
      eventData?.targetParticipants === undefined ||
      eventData?.targetParticipants === null
    ) {
      return `${safeRegistered}/-`;
    }
    return `${safeRegistered}/${eventData.targetParticipants}`;
  }, [eventData?.registeredParticipants, eventData?.targetParticipants]);

  useEffect(() => {
    if (!canEditEvent && viewMode === "edit") {
      setViewMode("preview");
      setEditingField(null);
    }
  }, [canEditEvent, viewMode]);

  useEffect(() => {
    if (startMode === "edit" && canEditEvent) {
      setViewMode("edit");
    }
  }, [startMode, canEditEvent]);

  const handleSave = async () => {
    if (!canEditEvent || !draft) return;
    if (!draft.eventName?.trim()) {
      alert("Event name is required.");
      return;
    }

    const startDateIso = toIsoUtcFromLocalInput(draft.startDate);
    const endDateIso = toIsoUtcFromLocalInput(draft.endDate) || startDateIso;
    if (!startDateIso || !endDateIso) {
      alert("Start date and end date are required.");
      return;
    }

    setIsSaving(true);
    try {
      const updated = await updateEvent(eventId, {
        eventName: draft.eventName.trim(),
        description: draft.description || null,
        startDate: startDateIso,
        endDate: endDateIso,
        location: draft.location || null,
        bannerUrl: draft.bannerUrl || null,
        visibility: draft.visibility || "Private",
        targetParticipants:
          draft.targetParticipants === "" ||
          draft.targetParticipants === null ||
          draft.targetParticipants === undefined
            ? null
            : Number(draft.targetParticipants),
      });

      setEventData(updated);
      setDraft(buildDraft(updated));
      setViewMode("preview");
      setEditingField(null);
      setSourceMode("workspace");
    } catch (err) {
      alert(err.message || "Failed to update event");
    } finally {
      setIsSaving(false);
    }
  };

  const handleJoin = async () => {
    if (!isAuthenticated || !canJoin) return;
    setIsJoining(true);
    try {
      const response = await joinEvent(eventId);
      setJoinState({
        isRegistered: !!response?.isRegistered,
        status: response?.status || "Registered",
        isEventMember: !!response?.isEventMember,
      });
    } catch (err) {
      alert(err.message || "Failed to join event");
    } finally {
      setIsJoining(false);
    }
  };

  const bannerSrc = toAbsoluteMediaUrl(eventData?.bannerUrl);

  if (isLoading) {
    return (
      <div className="event-remix-page">
        <LoadingSpinner message="Loading event detail..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="event-remix-page app-page">
        <div className="event-remix-top-nav">
          <button
            type="button"
            className="event-remix-back-btn"
            onClick={handleBack}
          >
            <span>‹</span> Trở về
          </button>
        </div>
        <ErrorState message={error} />
      </div>
    );
  }

  if (!eventData || !draft) {
    return (
      <div className="event-remix-page app-page">
        <ErrorState message="Event not found" />
      </div>
    );
  }

  const renderEditableField = ({ keyName, label, type = "text", options = [] }) => {
    const isEditing = viewMode === "edit" && canEditEvent && editingField === keyName;
    const value = draft[keyName] ?? "";
    const isClickable = viewMode === "edit" && canEditEvent;

    return (
      <div className="event-remix-field" key={keyName}>
        <div className="event-remix-label">{label}</div>
        {isEditing ? (
          type === "textarea" ? (
            <textarea
              className="form-input event-remix-input"
              value={value}
              autoFocus
              rows={4}
              onChange={(e) => setDraft((prev) => ({ ...prev, [keyName]: e.target.value }))}
              onBlur={() => setEditingField(null)}
            />
          ) : type === "select" ? (
            <select
              className="form-select event-remix-input"
              value={value}
              autoFocus
              onChange={(e) => setDraft((prev) => ({ ...prev, [keyName]: e.target.value }))}
              onBlur={() => setEditingField(null)}
            >
              {options.map((opt) => (
                <option key={opt} value={opt}>
                  {opt}
                </option>
              ))}
            </select>
          ) : (
            <input
              className="form-input event-remix-input"
              type={type}
              value={value}
              autoFocus
              onChange={(e) => setDraft((prev) => ({ ...prev, [keyName]: e.target.value }))}
              onBlur={() => setEditingField(null)}
            />
          )
        ) : (
          <button
            type="button"
            className={`event-remix-value ${isClickable ? "event-remix-value--editable" : ""}`}
            onClick={() => {
              if (isClickable) setEditingField(keyName);
            }}
          >
            {type === "datetime-local" ? formatDateTime(toIsoUtcFromLocalInput(value)) : formatMaybe(value)}
          </button>
        )}
      </div>
    );
  };

  return (
    <div className="event-remix-page">
      <div className="event-remix-top-nav">
        <button
          type="button"
          className="event-remix-back-btn"
          onClick={handleBack}
        >
          <span>‹</span> Trở về
        </button>
        {canEditEvent && (
          <div style={{ display: "inline-flex", gap: "8px", marginLeft: "8px" }}>
            {viewMode === "edit" ? (
              <>
                <button type="button" className="event-remix-back-btn" onClick={handleSave} disabled={isSaving}>
                  {isSaving ? "Đang lưu..." : "Lưu"}
                </button>
                <button
                  type="button"
                  className="event-remix-back-btn"
                  onClick={() => {
                    setDraft(buildDraft(eventData));
                    setEditingField(null);
                    setViewMode("preview");
                  }}
                >
                  Hủy sửa
                </button>
              </>
            ) : (
              <button type="button" className="event-remix-back-btn" onClick={() => setViewMode("edit")}>
                Chỉnh sửa
              </button>
            )}
          </div>
        )}
      </div>

      <section className="event-remix-hero">
        <div className="event-remix-glow" />
        <div className="event-remix-hero-container">
          <div className="event-remix-hero-content">
            <div className="event-remix-tag-group">
              <span className={`event-remix-chip event-remix-chip--accent event-remix-chip--${statusTone}`}>
                {getStatusLabel(eventData.status)}
              </span>
              <span className="event-remix-chip event-remix-chip--outline">
                {sourceMode === "workspace" ? "Workspace Event" : "Public Event"}
              </span>
            </div>

            <h1 className="event-remix-hero-title">
              {eventData.name || "Event Detail"}
            </h1>
          </div>

          <div className="event-remix-hero-visual">
            {bannerSrc ? (
              <img
                src={bannerSrc}
                alt={`${eventData.name || "Event"} banner`}
                onError={(e) => {
                  e.currentTarget.style.display = "none";
                }}
              />
            ) : (
              <div className="event-remix-hero-placeholder">Event Banner</div>
            )}
          </div>
        </div>
      </section>

      <div className="event-remix-stats-wrapper">
        <div className="event-remix-stats">
          <div className="event-remix-stat-item">
            <span className="event-remix-stat-label">Người tham gia</span>
            <span className="event-remix-stat-val">{participantSummary}</span>
          </div>
          <div className="event-remix-stat-item">
            <span className="event-remix-stat-label">Đánh giá trung bình</span>
            <span className="event-remix-stat-val">{formatMaybe(eventData.averageRating)}</span>
          </div>
          <div className="event-remix-stat-item">
            <span className="event-remix-stat-label">Đơn vị tổ chức</span>
            <span className="event-remix-stat-val">{eventData.organizationName || "-"}</span>
          </div>
        </div>
      </div>

      <main className="event-remix-main-layout">
        <div className="event-remix-details-col">
          <section className="event-remix-section">
            <h2 className="event-remix-section-title">Về sự kiện này</h2>
            <p className="event-remix-text-content">
              {eventData.description || "Sự kiện chưa có mô tả chi tiết."}
            </p>
          </section>
        </div>

        <div className="event-remix-sidebar-col">
          <div className="event-remix-action-widget">
            <h3>Đăng ký tham gia</h3>
            <p>Giữ chỗ ngay hôm nay để không bỏ lỡ sự kiện hấp dẫn nhất.</p>
            {isAuthenticated ? (
              joinState.isEventMember ? (
                <button type="button" className="event-remix-btn-join" disabled>
                  Tham gia với tư cách BTC
                </button>
              ) : joinState.isRegistered ? (
                <button type="button" className="event-remix-btn-join" disabled>
                  Bạn đã đăng ký ({joinState.status || "Registered"})
                </button>
              ) : (
                <button
                  type="button"
                  className="event-remix-btn-join"
                  onClick={handleJoin}
                  disabled={isJoining || !canJoin}
                >
                  {isJoining ? "Đang xử lý..." : "Xác nhận tham gia"}
                </button>
              )
            ) : (
              <button
                type="button"
                className="event-remix-btn-join"
                onClick={() => navigate("/login")}
              >
                Đăng nhập để tham gia
              </button>
            )}
          </div>

          <div className="event-remix-logistics-card">
            <div className="event-remix-logistic-row">
              <div className="event-remix-log-icon">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <rect width="18" height="18" x="3" y="4" rx="2" />
                  <line x1="16" x2="16" y1="2" y2="6" />
                  <line x1="8" x2="8" y1="2" y2="6" />
                  <line x1="3" x2="21" y1="10" y2="10" />
                </svg>
              </div>
              <div className="event-remix-log-info">
                <h4>Thời gian tổ chức</h4>
                <p>
                  {formatTimeOnly(eventData.startDate)} - {formatTimeOnly(eventData.endDate)}
                  <br />
                  {formatDateOnly(eventData.startDate)}
                </p>
              </div>
            </div>
            <div className="event-remix-logistic-row">
              <div className="event-remix-log-icon">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z" />
                  <circle cx="12" cy="10" r="3" />
                </svg>
              </div>
              <div className="event-remix-log-info">
                <h4>Địa điểm</h4>
                <p>{eventData.location || "-"}</p>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}

export default EventDetailPage;
