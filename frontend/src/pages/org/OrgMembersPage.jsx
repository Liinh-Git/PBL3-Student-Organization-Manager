/**
 * OrgMembersPage.jsx - Organization members page
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  getOrganizationMembers,
  updateMemberDepartment,
  removeMember,
} from "../../services/memberService.js";
import { getOrganizationRoles } from "../../services/roleService.js";
import { getOrganizationDepartments } from "../../services/departmentService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgMembersPage.css";

// THÊM HÀM NÀY ĐỂ FIX LỖI ĐƯỜNG DẪN ẢNH
function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  if (url.startsWith("/")) return `${origin}${url}`;
  return `${origin}/${url}`;
}

function OrgMembersPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get("orgId");
  const { permissions, isMember } = useOrgContext();

  const [members, setMembers] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [roles, setRoles] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [memberToDelete, setMemberToDelete] = useState(null);

  const normalizeDepartments = (deptData) => {
    const list = Array.isArray(deptData) ? deptData : deptData?.items || [];
    return list
      .map((dept) => {
        if (!dept) return null;
        const id = dept.id || dept.departmentId || dept.deptId || null;
        const departmentName =
          dept.departmentName || dept.deptName || dept.name || "";
        return {
          ...dept,
          id,
          departmentName,
          deptName: departmentName,
        };
      })
      .filter((dept) => dept && dept.id);
  };

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadData() {
      setIsLoading(true);
      try {
        const [memberData, deptData, roleData] = await Promise.all([
          getOrganizationMembers(orgId),
          getOrganizationDepartments(orgId),
          getOrganizationRoles(orgId),
        ]);
        setMembers(memberData);
        setDepartments(normalizeDepartments(deptData));
        setRoles(roleData);
      } catch (err) {
        setError(err.message || "Không thể tải danh sách thành viên");
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [orgId, isMember]);

  if (!orgId) return <ErrorState message="Thiếu mã tổ chức" />;
  if (!isMember)
    return (
      <ForbiddenState message="You are not a member of this organization" />
    );

  const canManage = permissions.includes("org.members.manage");

  const handleUpdateDepartment = async (memberId, newDeptId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }
    setIsSubmitting(true);
    try {
      const updated = await updateMemberDepartment(memberId, {
        departmentId: newDeptId || null,
      });
      setMembers((prev) => prev.map((m) => (m.id === memberId ? updated : m)));
    } catch (err) {
      alert(err.message || "Không thể cập nhật phòng ban");
    } finally {
      setIsSubmitting(false);
    }
  };

  const confirmRemoveMember = async () => {
    if (!canManage || !memberToDelete) return;

    setIsSubmitting(true);
    try {
      await removeMember(memberToDelete.id);
      setMembers((prev) => prev.filter((m) => m.id !== memberToDelete.id));
      setMemberToDelete(null);
    } catch (err) {
      alert(err.message || "Không thể xóa thành viên");
    } finally {
      setIsSubmitting(false);
    }
  };

  const getRoleStyle = (roleName) => {
    if (!roleName) return "member";
    const name = roleName.toLowerCase();
    if (name.includes("chủ nhiệm") || name.includes("president"))
      return "president";
    if (name.includes("admin")) return "admin";
    if (
      name.includes("kiểm duyệt") ||
      name.includes("manager") ||
      name.includes("phó")
    )
      return "moderator";
    return "member";
  };

  if (isLoading)
    return <LoadingSpinner message="Đang tải danh sách thành viên..." />;
  if (error) return <ErrorState message={error} />;

  return (
    <div className="members-container">
      <div className="members-header-section">
        <div>
          <h1 className="members-page-title">Quản lý thành viên</h1>
          <p className="members-page-desc">
            Xem danh sách, phân quyền và điều phối nhân sự tổ chức.
          </p>
        </div>
      </div>

      <div className="members-stats-grid">
        <div className="m-stat-card primary">
          <div className="m-stat-icon dark">
            <svg
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
              <circle cx="9" cy="7" r="4" />
              <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
              <path d="M16 3.13a4 4 0 0 1 0 7.75" />
            </svg>
          </div>
          <div className="m-stat-info">
            <span>Tổng số thành viên</span>
            <h3>{members.length}</h3>
          </div>
        </div>

        <div className="m-stat-card accent">
          <div className="m-stat-icon orange">
            <svg
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <polygon points="12 2 2 7 12 12 22 7 12 2" />
              <polyline points="2 17 12 22 22 17" />
              <polyline points="2 12 12 17 22 12" />
            </svg>
          </div>
          <div className="m-stat-info">
            <span>Số lượng phòng ban</span>
            <h3>{departments.length}</h3>
          </div>
        </div>
      </div>

      <div className="members-table-wrapper">
        <div className="members-table-toolbar">
          <h3>
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <line x1="8" y1="6" x2="21" y2="6" />
              <line x1="8" y1="12" x2="21" y2="12" />
              <line x1="8" y1="18" x2="21" y2="18" />
              <line x1="3" y1="6" x2="3.01" y2="6" />
              <line x1="3" y1="12" x2="3.01" y2="12" />
              <line x1="3" y1="18" x2="3.01" y2="18" />
            </svg>
            Danh sách nhân sự
          </h3>
        </div>

        {members.length === 0 ? (
          <div className="app-empty-state">
            Chưa có thành viên nào trong danh sách.
          </div>
        ) : (
          <table className="modern-table">
            <thead>
              <tr>
                <th>Họ và Tên</th>
                <th>Phòng ban chuyên môn</th>
                <th>Vai trò</th>
                <th>Trạng thái</th>
                {canManage && <th style={{ textAlign: "center" }}>Thao tác</th>}
              </tr>
            </thead>
            <tbody>
              {members.map((member) => {
                const roleStyle = getRoleStyle(member.role?.roleName);
                const isPresident = roleStyle === "president";
                const rawAvatar = member.user?.avatarUrl || member.avatarUrl;
                const initialLetter =
                  member.user?.fullName?.charAt(0)?.toUpperCase() || "U";

                return (
                  <tr key={member.id}>
                    <td>
                      <div className="user-profile-cell">
                        <div className="user-avatar">
                          {rawAvatar ? (
                            <>
                              <img
                                src={toAbsoluteMediaUrl(rawAvatar)}
                                alt={member.user?.fullName}
                                onError={(e) => {
                                  // NẾU ẢNH BỊ LỖI -> ẨN ẢNH VÀ HIỆN CHỮ CÁI ĐẦU
                                  e.target.style.display = "none";
                                  if (e.target.nextSibling) {
                                    e.target.nextSibling.style.display = "flex";
                                  }
                                }}
                              />
                              <span style={{ display: "none" }}>
                                {initialLetter}
                              </span>
                            </>
                          ) : (
                            initialLetter
                          )}
                        </div>
                        <div className="user-meta">
                          <h4>
                            {member.user?.fullName || "Người dùng ẩn danh"}
                          </h4>
                          <p>{member.user?.email || "Không có email"}</p>
                        </div>
                      </div>
                    </td>

                    <td>
                      {canManage ? (
                        <select
                          value={member.departmentId || ""}
                          onChange={(e) =>
                            handleUpdateDepartment(member.id, e.target.value)
                          }
                          disabled={isSubmitting}
                          className="table-select"
                        >
                          <option value="">-- Trống --</option>
                          {departments.map((dept) => (
                            <option key={dept.id} value={dept.id}>
                              {dept.departmentName}
                            </option>
                          ))}
                        </select>
                      ) : (
                        member.department?.deptName || "-"
                      )}
                    </td>

                    <td>
                      <span className={`role-badge ${roleStyle}`}>
                        {member.role?.roleName || "THÀNH VIÊN"}
                      </span>
                    </td>

                    <td>
                      <span className="status-badge">
                        {member.status || "Hoạt động"}
                      </span>
                    </td>

                    {canManage && (
                      <td style={{ textAlign: "center" }}>
                        {!isPresident && (
                          <button
                            onClick={() => setMemberToDelete(member)}
                            disabled={isSubmitting}
                            className="btn-remove-icon"
                            title="Xóa thành viên"
                          >
                            <svg
                              width="18"
                              height="18"
                              viewBox="0 0 24 24"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="2"
                            >
                              <polyline points="3 6 5 6 21 6" />
                              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                              <line x1="10" y1="11" x2="10" y2="17" />
                              <line x1="14" y1="11" x2="14" y2="17" />
                            </svg>
                          </button>
                        )}
                      </td>
                    )}
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {memberToDelete && canManage && (
        <div
          className="org-modal-overlay"
          onClick={() => setMemberToDelete(null)}
        >
          <div
            className="org-modal"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: "450px" }}
          >
            <div className="org-modal-header" style={{ paddingBottom: "1rem" }}>
              <h3 style={{ color: "#ef4444" }}>Xác nhận xóa</h3>
            </div>
            <div className="org-modal-body">
              <p style={{ margin: 0, color: "#475569", lineHeight: "1.5" }}>
                Bạn có chắc chắn muốn xóa thành viên{" "}
                <strong>
                  {memberToDelete.user?.fullName || memberToDelete.user?.email}
                </strong>{" "}
                khỏi tổ chức không? Hành động này không thể hoàn tác.
              </p>
            </div>
            <div className="org-modal-footer">
              <button
                type="button"
                onClick={() => setMemberToDelete(null)}
                className="org-btn org-btn-secondary"
                disabled={isSubmitting}
              >
                Hủy bỏ
              </button>
              <button
                type="button"
                onClick={confirmRemoveMember}
                className="org-btn org-btn-danger"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Đang xóa..." : "Xóa thành viên"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrgMembersPage;
