/**
 * TopBar.jsx - Top navigation bar for authenticated pages
 * * Phase 3C-4A: Foundation skeleton only
 */

import { Link } from "react-router-dom";
import { useAuthContext } from "../contexts/AuthContext";
// import { useNotifications } from '../hooks/useNotifications';

function TopBar() {
  const { user, logout } = useAuthContext();
  // const { unreadCount } = useNotifications();

  const handleLogout = () => {
    logout();
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
        {/* TODO Phase 3C-4B/4C: Add notification badge */}
        {/* <NotificationBadge count={unreadCount} /> */}

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
