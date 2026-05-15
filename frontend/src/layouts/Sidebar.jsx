/**
 * Sidebar.jsx - Navigation sidebar with rail for authenticated pages
 */

import { useEffect, useMemo, useState } from "react";
import {
  Link,
  useLocation,
  useNavigate,
  useSearchParams,
} from "react-router-dom";
import { useAuthContext } from "../contexts/AuthContext";
import { getMyOrganizations } from "../services/userService.js";

function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const routeOrgId = searchParams.get("orgId");

  const { user, logout } = useAuthContext();

  const [organizations, setOrganizations] = useState([]);
  const [selectedWorkspace, setSelectedWorkspace] = useState("user");
  const [selectedOrgId, setSelectedOrgId] = useState(null);

  const isActive = (path) =>
    location.pathname === path || location.pathname.startsWith(path + "/");

  const getInitials = (name) => {
    if (!name || typeof name !== "string") return "U";
    const parts = name.trim().split(/\s+/);
    if (parts.length === 0 || !parts[0]) return "U";
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  };

  const getOrgInitials = (org) => {
    const orgName = org?.name || org?.orgName || org?.organizationName || "";
    if (!orgName) return "O";
    return getInitials(orgName);
  };

  useEffect(() => {
    async function loadOrganizations() {
      try {
        const data = await getMyOrganizations();
        setOrganizations(Array.isArray(data) ? data : []);
      } catch {
        setOrganizations([]);
      }
    }
    loadOrganizations();
  }, []);

  useEffect(() => {
    if (location.pathname.startsWith("/org") && routeOrgId) {
      setSelectedWorkspace("org");
      setSelectedOrgId(routeOrgId);
      return;
    }

    if (location.pathname.startsWith("/user")) {
      setSelectedWorkspace("user");
    }
  }, [location.pathname, routeOrgId]);

  const activeOrgId = useMemo(() => {
    if (selectedWorkspace !== "org") return null;
    return selectedOrgId || routeOrgId || organizations[0]?.id || null;
  }, [selectedWorkspace, selectedOrgId, routeOrgId, organizations]);

  const handleSelectUserWorkspace = () => {
    setSelectedWorkspace("user");
    navigate("/user/discover");
  };

  const handleSelectOrgWorkspace = (orgId) => {
    setSelectedWorkspace("org");
    setSelectedOrgId(orgId);
    navigate(`/org/overview?orgId=${orgId}`);
  };

  return (
    <>
      <aside className="app-rail">
        <div className="app-rail-divider"></div>

        <button
          type="button"
          onClick={handleSelectUserWorkspace}
          className={`app-rail-icon ${selectedWorkspace === "user" ? "app-rail-icon--active" : ""}`}
          title="User Workspace"
        >
          {getInitials(user?.fullName)}
        </button>

        <div className="app-rail-divider"></div>

        <div
          style={{
            display: "grid",
            gap: "8px",
            width: "100%",
            justifyItems: "center",
            overflowY: "auto",
          }}
        >
          {organizations.map((org) => (
            <button
              key={org.id}
              type="button"
              onClick={() => handleSelectOrgWorkspace(org.id)}
              className={`app-rail-icon ${selectedWorkspace === "org" && activeOrgId === org.id ? "app-rail-icon--active" : ""}`}
              title={org.orgName || org.name || "Organization"}
            >
              {getOrgInitials(org)}
            </button>
          ))}
        </div>

        {/* Nút Đăng xuất ở cuối Rail - Đổi SVG đẹp hơn */}
        <div style={{ marginTop: "auto", marginBottom: "1.5rem" }}>
          <button
            onClick={logout}
            className="app-rail-logout"
            title="Đăng xuất"
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#ff9b51"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <rect x="3" y="3" width="11" height="18" rx="2" />
              <polyline points="15 17 20 12 15 7" />
              <line x1="20" y1="12" x2="9" y2="12" />
            </svg>
          </button>
        </div>
      </aside>

      <aside className="app-sidebar">
        <div style={{ height: "16px" }}></div>

        <nav className="sidebar-nav">
          {selectedWorkspace === "user" && (
            <div className="nav-section">
              <ul>
                <li>
                  <Link
                    to="/user/discover"
                    className={isActive("/user/discover") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <circle cx="12" cy="12" r="10" />
                      <polygon points="16.24 7.76 14.12 14.12 7.76 16.24 9.88 9.88 16.24 7.76" />
                    </svg>
                    Khám phá
                  </Link>
                </li>
                <li>
                  <Link
                    to="/user/organizations"
                    className={isActive("/user/organizations") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <rect x="4" y="4" width="16" height="16" rx="2" ry="2" />
                      <rect x="9" y="9" width="6" height="6" />
                    </svg>
                    Tổ chức của tôi
                  </Link>
                </li>
                <li>
                  <Link
                    to="/user/events"
                    className={isActive("/user/events") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                      <line x1="16" y1="2" x2="16" y2="6" />
                      <line x1="8" y1="2" x2="8" y2="6" />
                      <line x1="3" y1="10" x2="21" y2="10" />
                    </svg>
                    Sự kiện của tôi
                  </Link>
                </li>
                <li>
                  <Link
                    to="/user/settings"
                    className={isActive("/user/settings") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <circle cx="12" cy="12" r="3" />
                      <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z" />
                    </svg>
                    Cài đặt
                  </Link>
                </li>
              </ul>
            </div>
          )}

          {selectedWorkspace === "org" && activeOrgId && (
            <div className="nav-section">
              <ul>
                <li>
                  <Link
                    to={`/org/overview?orgId=${activeOrgId}`}
                    className={isActive("/org/overview") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <rect x="3" y="3" width="7" height="7" />
                      <rect x="14" y="3" width="7" height="7" />
                      <rect x="14" y="14" width="7" height="7" />
                      <rect x="3" y="14" width="7" height="7" />
                    </svg>
                    Tổng quan
                  </Link>
                </li>
                <li>
                  <Link
                    to={`/org/members?orgId=${activeOrgId}`}
                    className={isActive("/org/members") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                      <circle cx="9" cy="7" r="4" />
                      <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
                      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
                    </svg>
                    Thành viên
                  </Link>
                </li>
                <li>
                  <Link
                    to={`/org/departments?orgId=${activeOrgId}`}
                    className={isActive("/org/departments") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <rect x="4" y="4" width="16" height="16" rx="2" ry="2" />
                      <rect x="9" y="9" width="6" height="6" />
                    </svg>
                    Phòng ban
                  </Link>
                </li>
                <li>
                  <Link
                    to={`/org/events?orgId=${activeOrgId}`}
                    className={isActive("/org/events") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                      <line x1="16" y1="2" x2="16" y2="6" />
                      <line x1="8" y1="2" x2="8" y2="6" />
                      <line x1="3" y1="10" x2="21" y2="10" />
                    </svg>
                    Sự kiện
                  </Link>
                </li>
                <li>
                  <Link
                    to={`/org/requests?orgId=${activeOrgId}`}
                    className={isActive("/org/requests") ? "active" : ""}
                  >
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z" />
                    </svg>
                    Yêu cầu
                  </Link>
                </li>
              </ul>
            </div>
          )}
        </nav>
      </aside>
    </>
  );
}

export default Sidebar;
