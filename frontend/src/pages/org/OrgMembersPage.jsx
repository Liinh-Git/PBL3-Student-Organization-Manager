/**
 * OrgMembersPage.jsx - Organization members page
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  getOrganizationMembers,
  addMember,
  updateMemberDepartment,
  removeMember,
} from "../../services/memberService.js";
import { getOrganizationRoles } from "../../services/roleService.js";
import { getOrganizationDepartments } from "../../services/departmentService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgMembersPage.css"; // Import file CSS giao diện mới

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
  const [showAddForm, setShowAddForm] = useState(false);

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

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  if (!isMember) {
    return (
      <ForbiddenState message="You are not a member of this organization" />
    );
  }

  const canManage = permissions.includes("org.members.manage");

  const handleAddMember = async (e) => {
    e.preventDefault();
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const form = e.target;
    const userId = form.userId.value;
    const roleId = form.roleId.value;
    const departmentId = form.departmentId.value;
    const studentCode = form.studentCode.value;

    if (!userId) {
      alert("User ID is required");
      return;
    }

    setIsSubmitting(true);
    try {
      const newMember = await addMember(orgId, {
        userId,
        roleId: roleId || undefined,
        departmentId: departmentId || undefined,
        studentCode: studentCode || undefined,
      });
      setMembers((prev) => [...prev, newMember]);
      form.reset();
      setShowAddForm(false);
    } catch (err) {
      alert(err.message || "Không thể thêm thành viên");
    } finally {
      setIsSubmitting(false);
    }
  };

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

  const handleRemoveMember = async (memberId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    if (!window.confirm("Are you sure you want to remove this member?")) {
      return;
    }

    setIsSubmitting(true);
    try {
      await removeMember(memberId);
      setMembers((prev) => prev.filter((m) => m.id !== memberId));
    } catch (err) {
      alert(err.message || "Không thể xóa thành viên");
    } finally {
      setIsSubmitting(false);
    }
  };

  // Hàm phụ trợ lấy style cho Role Badge cho đẹp mắt
  const getRoleStyle = (roleName) => {
    if (!roleName) return "member";
    const name = roleName.toLowerCase();
    if (name.includes("admin") || name.includes("chủ nhiệm")) return "admin";
    if (name.includes("kiểm duyệt") || name.includes("manager"))
      return "moderator";
    return "member";
  };

  if (isLoading)
    return <LoadingSpinner message="Đang tải danh sách thành viên..." />;
  if (error) return <ErrorState message={error} />;

  return (
    <div className="members-container">
      {/* Header */}
      <div className="members-header-section">
        <div>
          <h1 className="members-page-title">Quản lý thành viên</h1>
          <p className="members-page-desc">
            Xem danh sách, phân quyền và điều phối nhân sự tổ chức.
          </p>
        </div>
      </div>

      {/* Thẻ Thống Kê (Lấy từ dữ liệu có sẵn) */}
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

      {/* Bảng Danh Sách */}
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
          {canManage && (
            <button
              onClick={() => setShowAddForm(true)}
              className="org-btn org-btn-primary"
              style={{ padding: "8px 16px", fontSize: "0.85rem" }}
            >
              + Thêm thành viên
            </button>
          )}
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
              {members.map((member) => (
                <tr key={member.id}>
                  {/* Cột 1: Gộp Avatar, Name và Email */}
                  <td>
                    <div className="user-profile-cell">
                      <div className="user-avatar">
                        {member.user?.fullName?.charAt(0)?.toUpperCase() || "U"}
                      </div>
                      <div className="user-meta">
                        <h4>{member.user?.fullName || "Người dùng ẩn danh"}</h4>
                        <p>{member.user?.email || "Không có email"}</p>
                      </div>
                    </div>
                  </td>

                  {/* Cột 2: Select đổi phòng ban */}
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

                  {/* Cột 3: Role Badge */}
                  <td>
                    <span
                      className={`role-badge ${getRoleStyle(member.role?.roleName)}`}
                    >
                      {member.role?.roleName || "THÀNH VIÊN"}
                    </span>
                  </td>

                  {/* Cột 4: Status */}
                  <td>
                    <span className="status-badge">
                      {member.status || "Hoạt động"}
                    </span>
                  </td>

                  {/* Cột 5: Xóa */}
                  {canManage && (
                    <td style={{ textAlign: "center" }}>
                      <button
                        onClick={() => handleRemoveMember(member.id)}
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
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Modal Thêm Thành Viên - Đồng bộ thiết kế với Modal Tổ chức */}
      {showAddForm && canManage && (
        <div
          className="org-modal-overlay"
          onClick={() => setShowAddForm(false)}
        >
          <div className="org-modal" onClick={(e) => e.stopPropagation()}>
            <div className="org-modal-header">
              <h3>Thêm thành viên mới</h3>
            </div>

            <div className="org-modal-body">
              <form id="addMemberForm" onSubmit={handleAddMember}>
                <div className="org-form-grid">
                  <div className="org-form-group">
                    <label htmlFor="userId" className="org-form-label">
                      Mã User ID *
                    </label>
                    <input
                      id="userId"
                      name="userId"
                      className="org-input"
                      placeholder="Nhập ID người dùng"
                      required
                    />
                  </div>

                  <div className="org-form-group">
                    <label htmlFor="studentCode" className="org-form-label">
                      Mã Sinh Viên
                    </label>
                    <input
                      id="studentCode"
                      name="studentCode"
                      className="org-input"
                      placeholder="VD: 102210..."
                    />
                  </div>

                  <div className="org-form-group">
                    <label htmlFor="roleId" className="org-form-label">
                      Vai trò
                    </label>
                    <select id="roleId" name="roleId" className="org-select">
                      <option value="">-- Chọn vai trò --</option>
                      {roles.map((role) => (
                        <option key={role.id} value={role.id}>
                          {role.roleName}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="org-form-group">
                    <label htmlFor="departmentId" className="org-form-label">
                      Phòng ban
                    </label>
                    <select
                      id="departmentId"
                      name="departmentId"
                      className="org-select"
                    >
                      <option value="">-- Chọn phòng ban --</option>
                      {departments.map((dept) => (
                        <option key={dept.id} value={dept.id}>
                          {dept.departmentName}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              </form>
            </div>

            <div className="org-modal-footer">
              <button
                type="button"
                onClick={() => setShowAddForm(false)}
                className="org-btn org-btn-secondary"
                disabled={isSubmitting}
              >
                Hủy bỏ
              </button>
              <button
                type="submit"
                form="addMemberForm"
                className="org-btn org-btn-primary"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Đang xử lý..." : "Thêm thành viên"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrgMembersPage;
