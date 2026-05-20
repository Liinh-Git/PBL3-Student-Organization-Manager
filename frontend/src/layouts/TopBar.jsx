/**
 * TopBar.jsx - Top navigation bar for authenticated pages
 */

import { useEffect, useMemo, useRef, useState } from "react";
import { useAuthContext } from "../contexts/AuthContext";
import { useNotifications } from "../hooks/useNotifications";
import { useSearchParams } from "react-router-dom";
import { getMyOrganizations } from "../services/userService.js";

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  if (url.startsWith("/")) return `${origin}${url}`;
  return `${origin}/${url}`;
}

function TopBar() {
  const { user } = useAuthContext();
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get("orgId");

  const [orgTitle, setOrgTitle] = useState("Student Organization Manager");

  const {
    unreadCount,
    notifications,
    isLoading,
    markAsRead,
    markAllAsRead,
    fetchNotifications,
    fetchUnreadCount,
  } = useNotifications();
  const [isPanelOpen, setIsPanelOpen] = useState(false);
  const panelRef = useRef(null);

  const latestNotifications = useMemo(
    () => notifications.slice(0, 8),
    [notifications],
  );
  const userAvatarSrc = toAbsoluteMediaUrl(user?.avatarUrl);

  useEffect(() => {
    if (orgId) {
      getMyOrganizations()
        .then((orgs) => {
          const current = orgs.find((o) => o.id === orgId);
          if (current)
            setOrgTitle(
              current.name || current.orgName || "Organization Workspace",
            );
        })
        .catch(() => setOrgTitle("Organization Workspace"));
    } else {
      setOrgTitle("Student Organization Manager");
    }
  }, [orgId]);

  useEffect(() => {
    function onClickOutside(event) {
      if (panelRef.current && !panelRef.current.contains(event.target)) {
        setIsPanelOpen(false);
      }
    }
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, []);

  const togglePanel = async () => {
    const next = !isPanelOpen;
    setIsPanelOpen(next);
    if (next) {
      await Promise.allSettled([fetchNotifications(), fetchUnreadCount()]);
    }
  };

  const formatTime = (iso) => {
    if (!iso) return "";
    const date = new Date(iso);
    if (Number.isNaN(date.getTime())) return "";
    return new Intl.DateTimeFormat("vi-VN", {
      hour: "2-digit",
      minute: "2-digit",
      day: "2-digit",
      month: "2-digit",
    }).format(date);
  };

  return (
    <header className="topbar">
      <div className="topbar-left">
        <h1
          style={{
            fontSize: "1.2rem",
            fontWeight: "800",
            color: "#0f172a",
            margin: 0,
            letterSpacing: "-0.02em",
          }}
        >
          {orgTitle}
        </h1>
      </div>

      <div className="topbar-right">
        {/* Nút Chuông Thông Báo */}
        <div
          ref={panelRef}
          style={{
            position: "relative",
            display: "flex",
            alignItems: "center",
          }}
        >
          <button
            type="button"
            onClick={togglePanel}
            className="btn-bell-clean"
            aria-label="Notifications"
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
              <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
            </svg>
            {unreadCount > 0 && (
              <span className="topbar-badge">
                {unreadCount > 99 ? "99+" : unreadCount}
              </span>
            )}
          </button>

          {/* Panel Thông Báo */}
          {isPanelOpen && (
            <div className="notifications-panel">
              <div className="notifications-header">
                <strong style={{ fontSize: "0.9rem", color: "#0f172a" }}>
                  Thông báo
                </strong>
                <button
                  type="button"
                  onClick={markAllAsRead}
                  disabled={unreadCount === 0}
                  style={{
                    border: "none",
                    background: "transparent",
                    color: unreadCount === 0 ? "#94a3b8" : "#fb923c",
                    cursor: unreadCount === 0 ? "default" : "pointer",
                    fontSize: "0.75rem",
                    fontWeight: "700",
                  }}
                >
                  ✓ Đã xem tất cả
                </button>
              </div>
              {isLoading && <p className="notifications-empty">Đang tải...</p>}
              {!isLoading && latestNotifications.length === 0 && (
                <p className="notifications-empty">Chưa có thông báo mới.</p>
              )}
              {!isLoading &&
                latestNotifications.map((item) => (
                  <div
                    key={item.id}
                    className={`notification-item${!item.isRead ? " is-unread" : ""}`}
                  >
                    <div
                      style={{
                        display: "flex",
                        justifyContent: "space-between",
                        gap: "0.75rem",
                      }}
                    >
                      <div style={{ minWidth: 0 }}>
                        <p className="notification-title">
                          {item.title || "Thông báo"}
                        </p>
                        <p className="notification-message">{item.message}</p>
                        <p className="notification-time">
                          {formatTime(item.createdAtUtc)}
                        </p>
                      </div>
                      {!item.isRead && (
                        <button
                          type="button"
                          onClick={() => markAsRead(item.id)}
                          style={{
                            alignSelf: "flex-start",
                            border: "none",
                            background: "transparent",
                            color: "#fb923c",
                            cursor: "pointer",
                            fontSize: "0.75rem",
                            fontWeight: "700",
                            whiteSpace: "nowrap",
                          }}
                        >
                          ✓
                        </button>
                      )}
                    </div>
                  </div>
                ))}
            </div>
          )}
        </div>

        {/* Nút Envelope / Tin nhắn */}

        {/* User Profile Box */}
        <div className="user-profile-badge">
          <div className="user-meta-info">
            <strong>{user?.fullName || "User 1"}</strong>
            <span>{user?.email || "example1@gmail.com"}</span>
          </div>
          <div className="user-avatar-sm">
            {userAvatarSrc ? (
              <img
                src={userAvatarSrc}
                alt={user?.fullName || "User avatar"}
                style={{
                  width: "100%",
                  height: "100%",
                  borderRadius: "inherit",
                  objectFit: "cover",
                }}
              />
            ) : (
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" />
              </svg>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}

export default TopBar;
