import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import { useAuth } from "../../hooks/useAuth.js";
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
import {
  getDepartmentTasks,
  createDepartmentTask,
  updateTaskStatus,
  assignTask,
} from "../../services/taskService.js";
import DepartmentCard from "../../components/org/DepartmentCard.jsx";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgDepartmentsPage.css";

function OrgDepartmentsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get("orgId");
  const { permissions, isMember } = useOrgContext();
  const { user } = useAuth();

  const [departments, setDepartments] = useState([]);
  const [members, setMembers] = useState([]);
  const [tasksByDepartment, setTasksByDepartment] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingDept, setEditingDept] = useState(null);

  const canManage = permissions.includes("org.departments.manage");
  const canManageMembers = permissions.includes("org.members.manage");
  const canManageOrgTasks = permissions.includes("org.events.manage");
  const currentUserId = user?.id || user?.userId || null;
  const myMember = members.find((m) => m.userId === currentUserId) || null;
  const myRoleName = (myMember?.role?.roleName || myMember?.roleName || "")
    .trim()
    .toLowerCase();
  const isLeader =
    myRoleName === "president" ||
    myRoleName === "vice president" ||
    myRoleName === "vicepresident";

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

        const taskEntries = await Promise.all(
          deptData.map(async (dept) => {
            try {
              const tasks = await getDepartmentTasks(orgId, dept.id);
              return [dept.id, tasks];
            } catch {
              return [dept.id, []];
            }
          }),
        );
        setTasksByDepartment(Object.fromEntries(taskEntries));
      } catch (err) {
        setError(err.message || "Không thể tải danh sách phòng ban");
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

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!canManage) return;

    const form = e.target;
    const departmentName = form.departmentName.value;
    const description = form.description.value;
    const managerId = form.managerId.value || undefined;

    setIsSubmitting(true);
    try {
      const newDept = await createDepartment(orgId, {
        departmentName,
        description: description || undefined,
        managerId,
      });
      setDepartments((prev) => [...prev, newDept]);
      if (managerId) {
        const updatedManager = await updateMemberDepartment(managerId, {
          departmentId: newDept.id,
        });
        setMembers((prev) =>
          prev.map((m) => (m.id === updatedManager.id ? updatedManager : m)),
        );
      }
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || "Không thể tạo phòng ban");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canManage || !editingDept) return;

    const form = e.target;
    const departmentName = form.departmentName.value;
    const description = form.description.value;
    const managerId = form.managerId.value || undefined;

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
      if (managerId) {
        const updatedManager = await updateMemberDepartment(managerId, {
          departmentId: editingDept.id,
        });
        setMembers((prev) =>
          prev.map((m) => (m.id === updatedManager.id ? updatedManager : m)),
        );
      }
      setEditingDept(null);
    } catch (err) {
      alert(err.message || "Không thể cập nhật phòng ban");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (deptId) => {
    if (!canManage) return;
    if (!window.confirm("Are you sure you want to delete this department?"))
      return;
    setIsSubmitting(true);
    try {
      await deleteDepartment(deptId);
      setDepartments((prev) => prev.filter((d) => d.id !== deptId));
    } catch (err) {
      alert(err.message || "Không thể xóa phòng ban");
    } finally {
      setIsSubmitting(false);
    }
  };

  const getManagerName = (dept) => {
    if (dept?.managerName) return dept.managerName;
    if (dept?.managerId) {
      const managerMember = members.find((m) => m.id === dept.managerId);
      return managerMember?.fullName || managerMember?.email || "-";
    }
    return "-";
  };

  const getDepartmentMembers = (dept) =>
    members.filter((m) => m.departmentId === dept.id);

  const isManagerOfOtherDepartment = (memberId, currentDeptId = null) =>
    departments.some((d) => d.managerId === memberId && d.id !== currentDeptId);

  const getEligibleManagersForCreate = () =>
    members.filter((m) => !m.departmentId && !isManagerOfOtherDepartment(m.id));

  const getEligibleManagersForEdit = (dept) =>
    members.filter((m) => {
      const isCurrentManager = m.id === dept.managerId;
      if (isCurrentManager) return true;

      const belongsToOtherDepartment =
        !!m.departmentId && m.departmentId !== dept.id;
      if (belongsToOtherDepartment) return false;

      if (isManagerOfOtherDepartment(m.id, dept.id)) return false;
      return !m.departmentId || m.departmentId === dept.id;
    });

  const getAssignableMembers = (dept) => {
    const managerId = dept.managerId || null;
    return members.filter((m) => {
      const isUnassigned = !m.departmentId;
      if (!isUnassigned) return false;
      if (m.id === managerId) return false;
      return true;
    });
  };

  const handleAddMembersToDepartment = async (deptId, memberIds) => {
    const isDeptManager =
      !!myMember &&
      departments.some((d) => d.id === deptId && d.managerId === myMember.id);
    if (!canManageMembers && !isDeptManager) return;
    setIsSubmitting(true);
    try {
      const selectedMembers = members.filter((m) => memberIds.includes(m.id));
      const invalid = selectedMembers.find(
        (m) => !!m.departmentId && m.departmentId !== deptId,
      );
      if (invalid) {
        throw new Error(
          `${invalid.fullName || invalid.email || "Thành viên"} đã thuộc phòng ban khác`,
        );
      }

      const selfMember = members.find((m) => m.userId === currentUserId);
      if (
        selfMember &&
        selfMember.departmentId &&
        selfMember.departmentId !== deptId &&
        memberIds.includes(selfMember.id)
      ) {
        throw new Error("Bạn đã thuộc phòng ban khác");
      }

      const updatedMembers = await Promise.all(
        memberIds.map((memberId) =>
          updateMemberDepartment(memberId, { departmentId: deptId }),
        ),
      );
      const byId = new Map(updatedMembers.map((m) => [m.id, m]));
      setMembers((prev) => prev.map((m) => byId.get(m.id) || m));
    } catch (err) {
      alert(err.message || "Không thể thêm thành viên vào phòng ban");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemoveMemberFromDepartment = async (deptId, memberId) => {
    const isDeptManager =
      !!myMember &&
      departments.some((d) => d.id === deptId && d.managerId === myMember.id);
    if (!canManageMembers && !isDeptManager) return;
    setIsSubmitting(true);
    try {
      const member = members.find((m) => m.id === memberId);
      if (!member) throw new Error("Không tìm thấy thành viên");
      if (!member.departmentId || member.departmentId !== deptId) {
        throw new Error("Thành viên không thuộc phòng ban này");
      }

      const updatedMember = await updateMemberDepartment(memberId, {
        departmentId: null,
      });
      setMembers((prev) =>
        prev.map((m) => (m.id === updatedMember.id ? updatedMember : m)),
      );
    } catch (err) {
      alert(err.message || "Không thể xóa thành viên khỏi phòng ban");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCreateTask = async (deptId, taskForm) => {
    setIsSubmitting(true);
    try {
      const payload = {
        task: {
          taskName: taskForm.taskName,
          description: taskForm.description || undefined,
          assigneeId: taskForm.assigneeId || undefined,
          deptId: deptId,
          deadline: taskForm.deadline
            ? new Date(taskForm.deadline).toISOString()
            : undefined,
          status: taskForm.status || "Todo",
        },
      };
      const created = await createDepartmentTask(orgId, deptId, payload);
      setTasksByDepartment((prev) => ({
        ...prev,
        [deptId]: [created, ...(prev[deptId] || [])],
      }));
    } catch (err) {
      alert(err.message || "Không thể tạo công việc phòng ban");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateTaskStatus = async (taskId, status) => {
    setIsSubmitting(true);
    try {
      const updated = await updateTaskStatus(taskId, { status });
      setTasksByDepartment((prev) => {
        const next = { ...prev };
        Object.keys(next).forEach((deptId) => {
          next[deptId] = (next[deptId] || []).map((t) =>
            t.id === taskId ? updated : t,
          );
        });
        return next;
      });
    } catch (err) {
      alert(err.message || "Không thể cập nhật trạng thái công việc");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAssignTask = async (taskId, assigneeId) => {
    setIsSubmitting(true);
    try {
      const updated = await assignTask(taskId, {
        assigneeId: assigneeId || null,
      });
      setTasksByDepartment((prev) => {
        const next = { ...prev };
        Object.keys(next).forEach((deptId) => {
          next[deptId] = (next[deptId] || []).map((t) =>
            t.id === taskId ? updated : t,
          );
        });
        return next;
      });
    } catch (err) {
      alert(err.message || "Không thể phân công công việc");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <LoadingSpinner message="Đang tải phòng ban..." />;
  if (error) return <ErrorState message={error} />;

  return (
    <div className="dept-page-container">
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
            Tạo phòng ban
          </button>
        )}
      </div>

      {showCreateForm && canManage && (
        <div
          className="dept-modal-overlay"
          onClick={() => setShowCreateForm(false)}
        >
          <div className="dept-modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="dept-modal-header">
              <h3>Tạo phòng ban mới</h3>
            </div>
            <form onSubmit={handleCreate}>
              <div className="dept-form-group">
                <label className="dept-form-label">Tên phòng ban *</label>
                <input name="departmentName" required className="dept-input" />
              </div>
              <div className="dept-form-group">
                <label className="dept-form-label">Mô tả</label>
                <input name="description" className="dept-input" />
              </div>
              <div className="dept-form-group">
                <label className="dept-form-label">Trưởng ban</label>
                <select name="managerId" className="dept-select">
                  <option value="">-- Trống --</option>
                  {getEligibleManagersForCreate().map((member) => (
                    <option key={member.id} value={member.id}>
                      {member.fullName || member.email}
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
                  Hủy
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

      {editingDept && canManage && (
        <div
          className="dept-modal-overlay"
          onClick={() => setEditingDept(null)}
        >
          <div className="dept-modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="dept-modal-header">
              <h3>Chỉnh sửa phòng ban</h3>
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
                  defaultValue={
                    editingDept.description || editingDept.function || ""
                  }
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
                  {getEligibleManagersForEdit(editingDept).map((member) => (
                    <option key={member.id} value={member.id}>
                      {member.fullName || member.email}
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
                  Hủy
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

      {departments.length === 0 ? (
        <EmptyState message="Chưa có phòng ban nào được tạo." />
      ) : (
        <div className="dept-grid">
          {departments.map((dept) =>
            (() => {
              const isDeptManager =
                !!myMember && dept.managerId === myMember.id;
              const deptCanManageMembers = canManageMembers || isDeptManager;
              const deptCanManage = canManage || isDeptManager;
              const deptCanManageTasks =
                canManageOrgTasks || isLeader || isDeptManager;
              return (
                <DepartmentCard
                  key={dept.id}
                  department={dept}
                  memberCount={getDepartmentMembers(dept).length}
                  departmentMembers={getDepartmentMembers(dept)}
                  assignableMembers={getAssignableMembers(dept)}
                  taskCount={(tasksByDepartment[dept.id] || []).length}
                  managerName={getManagerName(dept)}
                  canManage={deptCanManage}
                  canManageMembers={deptCanManageMembers}
                  canManageTasks={deptCanManageTasks}
                  isSubmitting={isSubmitting}
                  onEdit={setEditingDept}
                  onDelete={handleDelete}
                  onAddMembers={handleAddMembersToDepartment}
                  onRemoveMember={handleRemoveMemberFromDepartment}
                  tasks={tasksByDepartment[dept.id] || []}
                  onCreateTask={handleCreateTask}
                  onUpdateTaskStatus={handleUpdateTaskStatus}
                  onAssignTask={handleAssignTask}
                />
              );
            })(),
          )}
        </div>
      )}
    </div>
  );
}

export default OrgDepartmentsPage;
