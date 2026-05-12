/**
 * TopBar.jsx - Top navigation bar for authenticated pages
 * * Phase 3C-4A: Foundation skeleton only
 */

import { useEffect, useMemo, useRef, useState } from 'react';
import { useAuthContext } from "../contexts/AuthContext";
import { useNotifications } from '../hooks/useNotifications';

function TopBar() {
  const { user, logout } = useAuthContext();
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

  const handleLogout = () => {
    logout();
  };

  const latestNotifications = useMemo(() => notifications.slice(0, 8), [notifications]);

  useEffect(() => {
    function onClickOutside(event) {
      if (panelRef.current && !panelRef.current.contains(event.target)) {
        setIsPanelOpen(false);
      }
    }

    document.addEventListener('mousedown', onClickOutside);
    return () => document.removeEventListener('mousedown', onClickOutside);
  }, []);

  const togglePanel = async () => {
    const next = !isPanelOpen;
    setIsPanelOpen(next);

    if (next) {
      await Promise.allSettled([fetchNotifications(), fetchUnreadCount()]);
    }
  };

  const formatTime = (iso) => {
    if (!iso) return '';
    const date = new Date(iso);
    if (Number.isNaN(date.getTime())) return '';

    return new Intl.DateTimeFormat('vi-VN', {
      hour: '2-digit',
      minute: '2-digit',
      day: '2-digit',
      month: '2-digit',
    }).format(date);
  };

  return (
    <header
      style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        padding: "1rem 2.5rem",
        backgroundColor: "#ffffff",
        borderBottom: "1px solid #e2e8f0",
        width: "100%",
        boxSizing: "border-box",
      }}
    >
      <div style={{ display: "flex", alignItems: "center" }}>
        <h1
          style={{
            fontSize: "1.25rem",
            fontWeight: "800",
            color: "#0f172a",
            margin: 0,
          }}
        >
          Student Organization Manager
        </h1>
      </div>

      <div style={{ display: "flex", alignItems: "center", gap: "1.5rem" }}>
        <div ref={panelRef} style={{ position: 'relative' }}>
          <button
            type="button"
            onClick={togglePanel}
            style={{
              position: 'relative',
              width: '40px',
              height: '40px',
              borderRadius: '999px',
              border: '1px solid #cbd5e1',
              backgroundColor: '#ffffff',
              cursor: 'pointer',
              fontSize: '1.2rem',
            }}
            aria-label="Notifications"
          >
            🔔
            {unreadCount > 0 && (
              <span
                style={{
                  position: 'absolute',
                  top: '-6px',
                  right: '-6px',
                  minWidth: '20px',
                  height: '20px',
                  padding: '0 4px',
                  borderRadius: '999px',
                  backgroundColor: '#dc2626',
                  color: '#ffffff',
                  fontSize: '0.75rem',
                  fontWeight: '700',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  border: '2px solid #ffffff',
                  boxSizing: 'border-box',
                }}
              >
                {unreadCount > 99 ? '99+' : unreadCount}
              </span>
            )}
          </button>

          {isPanelOpen && (
            <div
              style={{
                position: 'absolute',
                top: '48px',
                right: 0,
                width: '360px',
                maxHeight: '420px',
                overflowY: 'auto',
                border: '1px solid #e2e8f0',
                borderRadius: '12px',
                backgroundColor: '#ffffff',
                boxShadow: '0 12px 36px rgba(15, 23, 42, 0.14)',
                zIndex: 50,
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '0.75rem 1rem', borderBottom: '1px solid #f1f5f9' }}>
                <strong style={{ fontSize: '0.9rem', color: '#0f172a' }}>Thông báo</strong>
                <button
                  type="button"
                  onClick={markAllAsRead}
                  disabled={unreadCount === 0}
                  style={{
                    border: 'none',
                    background: 'transparent',
                    color: unreadCount === 0 ? '#94a3b8' : '#0f766e',
                    cursor: unreadCount === 0 ? 'default' : 'pointer',
                    fontSize: '0.8rem',
                    fontWeight: '700',
                  }}
                >
                  ✓ Đã xem tất cả
                </button>
              </div>

              {isLoading && <p style={{ margin: 0, padding: '1rem', fontSize: '0.85rem', color: '#64748b' }}>Đang tải...</p>}
              {!isLoading && latestNotifications.length === 0 && (
                <p style={{ margin: 0, padding: '1rem', fontSize: '0.85rem', color: '#64748b' }}>
                  Chưa có thông báo.
                </p>
              )}

              {!isLoading && latestNotifications.map((item) => (
                <div
                  key={item.id}
                  style={{
                    padding: '0.75rem 1rem',
                    borderBottom: '1px solid #f8fafc',
                    backgroundColor: item.isRead ? '#ffffff' : '#f8fafc',
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', gap: '0.75rem' }}>
                    <div style={{ minWidth: 0 }}>
                      <p style={{ margin: 0, fontSize: '0.85rem', fontWeight: '700', color: '#0f172a' }}>{item.title || 'Thông báo'}</p>
                      <p style={{ margin: '0.25rem 0 0', fontSize: '0.8rem', color: '#475569' }}>{item.message}</p>
                      <p style={{ margin: '0.35rem 0 0', fontSize: '0.75rem', color: '#94a3b8' }}>{formatTime(item.createdAtUtc)}</p>
                    </div>
                    {!item.isRead && (
                      <button
                        type="button"
                        onClick={() => markAsRead(item.id)}
                        style={{
                          alignSelf: 'flex-start',
                          border: 'none',
                          background: 'transparent',
                          color: '#0f766e',
                          cursor: 'pointer',
                          fontSize: '0.8rem',
                          fontWeight: '700',
                          whiteSpace: 'nowrap',
                        }}
                      >
                        ✓ Đã xem
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "1rem" }}>
          <span
            style={{
              fontSize: "0.875rem",
              fontWeight: "600",
              color: "#334155",
            }}
          >
            {user?.fullName || "Dat Quy"}
          </span>
          <button
            onClick={handleLogout}
            style={{
              padding: "0.5rem 1rem",
              fontSize: "0.875rem",
              fontWeight: "600",
              backgroundColor: "#f1f5f9",
              color: "#475569",
              border: "none",
              borderRadius: "8px",
              cursor: "pointer",
              transition: "all 0.2s",
            }}
            onMouseOver={(e) => (e.target.style.backgroundColor = "#e2e8f0")}
            onMouseOut={(e) => (e.target.style.backgroundColor = "#f1f5f9")}
          >
            Logout
          </button>
        </div>
      </div>
    </header>
  );
}

export default TopBar;
