/**
 * OrgDepartmentsPage.jsx - Organization departments page
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  getOrganizationDepartments,
  createDepartment,
  updateDepartment,
  deleteDepartment,
} from "../../services/departmentService.js";
import {
  getOrganizationMembers,
  updateMemberDepartment,
} from "../../services/memberService.js";
import DepartmentCard from "../../components/org/DepartmentCard.jsx";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgDepartmentsPage.css"; // Import CSS mới

function OrgDepartmentsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get("orgId");
  const { permissions, isMember } = useOrgContext();

  const [departments, setDepartments] = useState([]);
  const [members, setMembers] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingDept, setEditingDept] = useState(null);

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadData() {
      setIsLoading(true);
      try {
        const [deptData, memberData] = await Promise.all([
          getOrganizationDepartments(orgId),
          getOrganizationMembers(orgId),
        ]);
        setDepartments(deptData);
        setMembers(memberData);
      } catch (err) {
        setError(err.message || "Failed to load departments");
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [orgId, isMember]);

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (!isMember) {
    return (
      <ForbiddenState message="You are not a member of this organization" />
    );
  }

  const canManage = permissions.includes("org.departments.manage");
  const canManageMembers = permissions.includes("org.members.manage");

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!canManage) {
      alert("You do not have permission to perform this action");
      return;
    }

    const form = e.target;
    const departmentName = form.departmentName.value;
    const description = form.description.value;
    const managerId = form.managerId.value || undefined;

    if (!departmentName) {
      alert("Department name is required");
      return;
    }

    setIsSubmitting(true);
    try {
      const newDept = await createDepartment(orgId, {
        departmentName,
        description: description || undefined,
        managerId,
      });
      setDepartments((prev) => [...prev, newDept]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || "Failed to create department");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canManage || !editingDept) {
      alert("You do not have permission to perform this action");
      return;
    }

    const form = e.target;
    const departmentName = form.departmentName.value;
    const description = form.description.value;
    const managerId = form.managerId.value || undefined;

    if (!departmentName) {
      alert("Department name is required");
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await updateDepartment(editingDept.id, {
        departmentName,
        description: description || undefined,
        managerId,
      });
      setDepartments((prev) =>
        prev.map((d) => (d.id === editingDept.id ? updated : d)),
      );
      setEditingDept(null);
    } catch (err) {
      alert(err.message || "Failed to update department");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (deptId) => {
    if (!canManage) {
      alert("You do not have permission to perform this action");
      return;
    }

    if (!window.confirm("Are you sure you want to delete this department?")) {
      return;
    }

    setIsSubmitting(true);
    try {
      await deleteDepartment(deptId);
      setDepartments((prev) => prev.filter((d) => d.id !== deptId));
    } catch (err) {
      alert(err.message || "Failed to delete department");
    } finally {
      setIsSubmitting(false);
    }
  };

  // Logic trợ giúp dữ liệu (Giữ nguyên)
  const getManagerName = (dept) => {
    if (dept?.manager?.user?.fullName) return dept.manager.user.fullName;
    if (dept?.manager?.fullName) return dept.manager.fullName;
    if (dept?.managerName) return dept.managerName;

    if (dept?.managerId) {
      const managerMember = members.find((m) => m.id === dept.managerId);
      return managerMember?.fullName || managerMember?.email || "-";
    }
    return "-";
  };

  const getMemberCount = (dept) =>
    members.filter((m) => m.departmentId === dept.id).length;
  const getDepartmentMembers = (dept) =>
    members.filter((m) => m.departmentId === dept.id);
  const getAssignableMembers = (dept) =>
    members.filter((m) => m.departmentId !== dept.id);

  const getTaskCount = (dept) => {
    if (typeof dept?.taskCount === "number") return dept.taskCount;
    if (typeof dept?.tasksCount === "number") return dept.tasksCount;
    if (Array.isArray(dept?.tasks)) return dept.tasks.length;
    return null;
  };

  const handleAddMemberToDepartment = async (deptId, memberId) => {
    if (!canManageMembers) {
      alert("You do not have permission to perform this action");
      return;
    }
    if (!memberId) {
      alert("Please select a member");
      return;
    }

    setIsSubmitting(true);
    try {
      const updatedMember = await updateMemberDepartment(memberId, {
        departmentId: deptId,
      });
      setMembers((prev) =>
        prev.map((m) => (m.id === updatedMember.id ? updatedMember : m)),
      );
    } catch (err) {
      alert(err.message || "Failed to add member to department");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <LoadingSpinner message="Đang tải phòng ban..." />;
  if (error) return <ErrorState message={error} />;

  return (
    <div className="dept-page-container">
      {/* Header */}
      <div className="dept-header-section">
        <div>
          <h1 className="dept-page-title">Cơ cấu phòng ban</h1>
          <p className="dept-page-desc">
            Quản lý và điều phối các ban chuyên môn trong tổ chức.
          </p>
        </div>
        {canManage && (
          <button
            onClick={() => setShowCreateForm(true)}
            className="btn-orange-header"
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
            >
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Tạo phòng ban
          </button>
        )}
      </div>

      {/* Modal Tạo Mới */}
      {showCreateForm && canManage && (
        <div
          className="dept-modal-overlay"
          onClick={() => setShowCreateForm(false)}
        >
          <div className="dept-modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="dept-modal-header">
              <h3>Tạo phòng ban mới</h3>
              <p>Khởi tạo ban chuyên môn mới cho tổ chức của bạn.</p>
            </div>
            <form onSubmit={handleCreate}>
              <div className="dept-form-group">
                <label className="dept-form-label">Tên phòng ban *</label>
                <input
                  name="departmentName"
                  placeholder="Ví dụ: Ban Truyền thông"
                  required
                  className="dept-input"
                />
              </div>
              <div className="dept-form-group">
                <label className="dept-form-label">Mô tả</label>
                <input
                  name="description"
                  placeholder="Nhiệm vụ chính của ban..."
                  className="dept-input"
                />
              </div>
              <div className="dept-form-group">
                <label className="dept-form-label">Trưởng ban</label>
                <select name="managerId" className="dept-select">
                  <option value="">-- Trống --</option>
                  {members.map((member) => (
                    <option key={member.id} value={member.id}>
                      {member.fullName || member.email || member.user?.fullName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="dept-modal-footer">
                <button
                  type="button"
                  onClick={() => setShowCreateForm(false)}
                  className="dept-btn dept-btn-secondary"
                  disabled={isSubmitting}
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="dept-btn dept-btn-primary"
                >
                  {isSubmitting ? "Đang tạo..." : "Xác nhận tạo"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Chỉnh Sửa */}
      {editingDept && canManage && (
        <div
          className="dept-modal-overlay"
          onClick={() => setEditingDept(null)}
        >
          <div className="dept-modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="dept-modal-header">
              <h3>Chỉnh sửa phòng ban</h3>
              <p>Cập nhật thông tin chi tiết cho ban chuyên môn.</p>
            </div>
            <form onSubmit={handleUpdate}>
              <div className="dept-form-group">
                <label className="dept-form-label">Tên phòng ban *</label>
                <input
                  name="departmentName"
                  defaultValue={
                    editingDept.deptName || editingDept.departmentName || ""
                  }
                  required
                  className="dept-input"
                />
              </div>
              <div className="dept-form-group">
                <label className="dept-form-label">Mô tả</label>
                <input
                  name="description"
                  defaultValue={editingDept.description || ""}
                  className="dept-input"
                />
              </div>
              <div className="dept-form-group">
                <label className="dept-form-label">Trưởng ban</label>
                <select
                  name="managerId"
                  defaultValue={editingDept.managerId || ""}
                  className="dept-select"
                >
                  <option value="">-- Trống --</option>
                  {members.map((member) => (
                    <option key={member.id} value={member.id}>
                      {member.fullName || member.email || member.user?.fullName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="dept-modal-footer">
                <button
                  type="button"
                  onClick={() => setEditingDept(null)}
                  className="dept-btn dept-btn-secondary"
                  disabled={isSubmitting}
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="dept-btn dept-btn-primary"
                >
                  {isSubmitting ? "Đang cập nhật..." : "Lưu thay đổi"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Danh sách phòng ban */}
      {departments.length === 0 ? (
        <EmptyState message="Chưa có phòng ban nào được tạo." />
      ) : (
        <div className="dept-grid">
          {departments.map((dept) => (
            <DepartmentCard
              key={dept.id}
              department={dept}
              memberCount={getMemberCount(dept)}
              departmentMembers={getDepartmentMembers(dept)}
              assignableMembers={getAssignableMembers(dept)}
              taskCount={getTaskCount(dept)}
              managerName={getManagerName(dept)}
              canManage={canManage}
              canManageMembers={canManageMembers}
              isSubmitting={isSubmitting}
              onEdit={setEditingDept}
              onDelete={handleDelete}
              onAddMember={handleAddMemberToDepartment}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default OrgDepartmentsPage;
