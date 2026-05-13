/**
 * OrgCategoryPage.jsx - Category / Task management page
 */

import { useState, useEffect } from "react";
import { useParams, useSearchParams, useNavigate } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import { getMilestoneCategories } from "../../services/categoryService.js";
import {
  createTask,
  updateTask,
  updateTaskStatus,
  assignTask,
  deleteTask,
} from "../../services/taskService.js";
import { getOrganizationMembers } from "../../services/memberService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgCategoryPage.css";

function OrgCategoryPage() {
  const { eventId, categoryId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const orgId = searchParams.get("orgId");
  const milestoneId = searchParams.get("milestoneId");

  const { permissions, isMember } = useOrgContext();
  const canManage = permissions.includes("org.events.manage");

  const [category, setCategory] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [members, setMembers] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Modals
  const [showTaskForm, setShowTaskForm] = useState(false);
  const [editingTask, setEditingTask] = useState(null);

  useEffect(() => {
    if (!categoryId || !milestoneId || !orgId) return;

    async function loadData() {
      setIsLoading(true);
      try {
        // Fetch categories to find the current one
        const cats = await getMilestoneCategories(milestoneId);
        const currentCat = cats.find((c) => c.id === categoryId);
        if (currentCat) {
          setCategory(currentCat);
          setTasks(currentCat.tasks || []);
        }

        // Fetch members for assignee dropdown
        const mems = await getOrganizationMembers(orgId);
        setMembers(mems);
      } catch (err) {
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [categoryId, milestoneId, orgId]);

  if (!isMember)
    return <ForbiddenState message="Bạn không có quyền truy cập" />;
  if (isLoading) return <LoadingSpinner />;
  if (!category)
    return <ErrorState message="Không tìm thấy dữ liệu hạng mục" />;

  const getTaskAssigneeId = (task) =>
    task?.assigneeId || task?.assignedMemberId || task?.assignee?.id || "";
  const getTaskAssigneeName = (task) => {
    const directName =
      task?.assignee?.user?.fullName ||
      task?.assignee?.fullName ||
      task?.assigneeName;
    if (directName) return directName;
    const assigneeId = getTaskAssigneeId(task);
    if (!assigneeId) return "-";
    const matched = members.find((m) => m.id === assigneeId);
    return matched?.fullName || matched?.email || "-";
  };

  const handleCreateTask = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    setIsSubmitting(true);
    const form = e.target;
    try {
      const newTask = await createTask(categoryId, {
        taskName: form.taskName.value,
        description: form.description.value || undefined,
        priority: form.priority.value,
        deadline: form.deadline.value || undefined,
      });
      setTasks((prev) => [...prev, newTask]);
      setShowTaskForm(false);
    } catch (err) {
      alert(err.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateTask = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    setIsSubmitting(true);
    const form = e.target;
    try {
      const updatedTask = await updateTask(editingTask.id, {
        taskName: form.taskName.value,
        description: form.description.value || undefined,
        priority: form.priority.value,
        deadline: form.deadline.value || undefined,
      });
      setTasks((prev) =>
        prev.map((t) =>
          t.id === editingTask.id
            ? {
                ...t,
                taskName: updatedTask.taskName,
                description: updatedTask.description,
                priority: updatedTask.priority,
                deadline: updatedTask.deadline,
              }
            : t,
        ),
      );
      setEditingTask(null);
    } catch (err) {
      alert(err.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateStatus = async (taskId, newStatus) => {
    if (!canManage) return;
    try {
      const updated = await updateTaskStatus(taskId, { status: newStatus });
      setTasks((prev) => prev.map((t) => (t.id === taskId ? updated : t)));
    } catch (err) {
      alert(err.message);
    }
  };

  const handleAssignTask = async (taskId, assigneeId) => {
    if (!canManage) return;
    try {
      const updated = await assignTask(taskId, {
        assigneeId: assigneeId || null,
        deptId: null,
      });
      setTasks((prev) => prev.map((t) => (t.id === taskId ? updated : t)));
    } catch (err) {
      alert(err.message);
    }
  };

  const handleDeleteTask = async (taskId) => {
    if (!canManage || !window.confirm("Delete this task?")) return;
    try {
      await deleteTask(taskId);
      setTasks((prev) => prev.filter((t) => t.id !== taskId));
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="cat-page-container">
      <div className="cat-header">
        <div className="cat-header-left">
          <p onClick={() => navigate(`/org/events/${eventId}?orgId=${orgId}`)}>
            ← Quay lại Sự kiện
          </p>
          <h1>Chi tiết mục: {category.categoryName}</h1>
        </div>
      </div>

      <div className="cat-info-card">
        <h4>THÔNG TIN MỤC</h4>
        <p>
          {category.description || "Chưa có mô tả chi tiết cho hạng mục này."}
        </p>
      </div>

      <div className="task-board-header">
        <h2>Phân chia nhiệm vụ</h2>
        {canManage && (
          <button
            onClick={() => setShowTaskForm(true)}
            className="cat-btn cat-btn-primary"
          >
            + Thêm Task
          </button>
        )}
      </div>

      <div className="task-list-modern">
        <table>
          <thead>
            <tr>
              <th>Tên công việc</th>
              <th>Độ ưu tiên</th>
              <th>Hạn chót</th>
              <th>Trạng thái</th>
              <th>Người phụ trách</th>
              {canManage && <th>Thao tác</th>}
            </tr>
          </thead>
          <tbody>
            {tasks.length === 0 ? (
              <tr>
                <td
                  colSpan="6"
                  style={{ textAlign: "center", padding: "2rem" }}
                >
                  Chưa có công việc nào.
                </td>
              </tr>
            ) : (
              tasks.map((task) => (
                <tr key={task.id}>
                  <td className="task-name-cell">
                    <h4>{task.taskName}</h4>
                    <p>{task.description}</p>
                  </td>
                  <td>
                    <span
                      className="task-status-badge"
                      style={{ background: "var(--ink-500)" }}
                    >
                      {task.priority || "Medium"}
                    </span>
                  </td>
                  <td style={{ fontSize: "0.85rem" }}>
                    {task.deadline ? task.deadline.split("T")[0] : "-"}
                  </td>
                  <td>
                    {canManage ? (
                      <select
                        className="task-select-clean"
                        value={task.status}
                        onChange={(e) =>
                          handleUpdateStatus(task.id, e.target.value)
                        }
                      >
                        <option value="Todo">Cần làm (Todo)</option>
                        <option value="InProgress">Đang làm</option>
                        <option value="Done">Đã xong</option>
                      </select>
                    ) : (
                      <span className="task-status-badge">{task.status}</span>
                    )}
                  </td>
                  <td>
                    {canManage ? (
                      <select
                        className="task-select-clean"
                        value={getTaskAssigneeId(task)}
                        onChange={(e) =>
                          handleAssignTask(task.id, e.target.value)
                        }
                      >
                        <option value="">-- Chưa giao --</option>
                        {members.map((m) => (
                          <option key={m.id} value={m.id}>
                            {m.fullName || m.email}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <span>{getTaskAssigneeName(task)}</span>
                    )}
                  </td>
                  {canManage && (
                    <td>
                      <button
                        onClick={() => setEditingTask(task)}
                        style={{
                          border: "none",
                          background: "transparent",
                          cursor: "pointer",
                          color: "var(--ink-600)",
                        }}
                      >
                        ✎
                      </button>
                      <button
                        onClick={() => handleDeleteTask(task.id)}
                        style={{
                          border: "none",
                          background: "transparent",
                          cursor: "pointer",
                          color: "red",
                          marginLeft: 8,
                        }}
                      >
                        ✕
                      </button>
                    </td>
                  )}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* MODAL THÊM / SỬA TASK */}
      {(showTaskForm || editingTask) && canManage && (
        <div
          className="cat-modal-overlay"
          onClick={() => {
            setShowTaskForm(false);
            setEditingTask(null);
          }}
        >
          <div className="cat-modal" onClick={(e) => e.stopPropagation()}>
            <div className="cat-modal-header">
              <h3>{editingTask ? "Sửa công việc" : "Thêm công việc mới"}</h3>
            </div>
            <div className="cat-modal-body">
              <form
                id="taskForm"
                onSubmit={editingTask ? handleUpdateTask : handleCreateTask}
              >
                <div className="form-group" style={{ marginBottom: "1rem" }}>
                  <label className="form-label">Tên công việc *</label>
                  <input
                    name="taskName"
                    className="form-input"
                    defaultValue={editingTask?.taskName || ""}
                    required
                    style={{
                      width: "100%",
                      padding: "10px",
                      borderRadius: "8px",
                      border: "1px solid #e2e8f0",
                    }}
                  />
                </div>
                <div className="form-group" style={{ marginBottom: "1rem" }}>
                  <label className="form-label">Mô tả chi tiết</label>
                  <textarea
                    name="description"
                    className="form-input"
                    defaultValue={editingTask?.description || ""}
                    style={{
                      width: "100%",
                      padding: "10px",
                      borderRadius: "8px",
                      border: "1px solid #e2e8f0",
                      minHeight: "80px",
                    }}
                  />
                </div>
                <div style={{ display: "flex", gap: "1rem" }}>
                  <div className="form-group" style={{ flex: 1 }}>
                    <label className="form-label">Độ ưu tiên</label>
                    <select
                      name="priority"
                      className="form-input"
                      defaultValue={editingTask?.priority || "Medium"}
                      style={{
                        width: "100%",
                        padding: "10px",
                        borderRadius: "8px",
                        border: "1px solid #e2e8f0",
                      }}
                    >
                      <option value="Low">Thấp</option>
                      <option value="Medium">Trung bình</option>
                      <option value="High">Cao</option>
                      <option value="Urgent">Khẩn cấp</option>
                    </select>
                  </div>
                  <div className="form-group" style={{ flex: 1 }}>
                    <label className="form-label">Hạn chót</label>
                    <input
                      name="deadline"
                      type="date"
                      className="form-input"
                      defaultValue={
                        editingTask?.deadline
                          ? editingTask.deadline.split("T")[0]
                          : ""
                      }
                      style={{
                        width: "100%",
                        padding: "10px",
                        borderRadius: "8px",
                        border: "1px solid #e2e8f0",
                      }}
                    />
                  </div>
                </div>
              </form>
            </div>
            <div className="cat-modal-footer">
              <button
                onClick={() => {
                  setShowTaskForm(false);
                  setEditingTask(null);
                }}
                className="cat-btn cat-btn-secondary"
              >
                Hủy
              </button>
              <button
                form="taskForm"
                type="submit"
                disabled={isSubmitting}
                className="cat-btn cat-btn-primary"
              >
                {editingTask ? "Cập nhật" : "Thêm Task"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrgCategoryPage;
