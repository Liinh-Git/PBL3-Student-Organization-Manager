import { useMemo, useState } from "react";
import { getTaskById } from "../../services/taskService.js";
function IconShell({ className, children }) {
  return (
    <span className={className} aria-hidden="true">
      {children}
    </span>
  );
}

function UsersIcon({ className }) { return <IconShell className={className}>👥</IconShell>; }
function ClipboardListIcon({ className }) { return <IconShell className={className}>▤</IconShell>; }
function SettingsIcon({ className }) { return <IconShell className={className}>⚙</IconShell>; }
function TrashIcon({ className }) { return <IconShell className={className}>🗑</IconShell>; }
function UserPlusIcon({ className }) { return <IconShell className={className}>＋</IconShell>; }
function PlusIcon({ className }) { return <IconShell className={className}>＋</IconShell>; }
function SearchIcon({ className }) { return <IconShell className={className}>⌕</IconShell>; }
function CalendarIcon({ className }) { return <IconShell className={className}>📅</IconShell>; }
function CheckCircleIcon({ className }) { return <IconShell className={className}>✓</IconShell>; }
function ClockIcon({ className }) { return <IconShell className={className}>○</IconShell>; }
function AlertCircleIcon({ className }) { return <IconShell className={className}>!</IconShell>; }
function XCircleIcon({ className }) { return <IconShell className={className}>×</IconShell>; }
function MoreVerticalIcon({ className }) { return <IconShell className={className}>⋮</IconShell>; }
function EyeIcon({ className }) { return <IconShell className={className}>◔</IconShell>; }

const TASK_STATUS_ORDER = [
  { key: "Todo", label: "Todo" },
  { key: "InProgress", label: "InProgress" },
  { key: "Blocked", label: "Blocked" },
  { key: "Done", label: "Done" },
  { key: "Cancelled", label: "Cancelled" },
];

function DepartmentCard({
  department,
  memberCount,
  departmentMembers = [],
  assignableMembers = [],
  taskCount,
  managerName,
  canManage,
  canManageMembers,
  canManageTasks,
  isSubmitting,
  onEdit,
  onDelete,
  onAddMembers,
  onRemoveMember,
  tasks = [],
  onCreateTask,
  onUpdateTaskStatus,
  onAssignTask,
}) {
  const [activeTab, setActiveTab] = useState("overview");
  const [query, setQuery] = useState("");
  const [selectedMemberIds, setSelectedMemberIds] = useState([]);
  const [showTaskForm, setShowTaskForm] = useState(false);
  const [openTaskStatuses, setOpenTaskStatuses] = useState(() =>
    TASK_STATUS_ORDER.reduce((acc, status) => {
      acc[status.key] = true;
      return acc;
    }, {}),
  );
  const [taskForm, setTaskForm] = useState({
    taskName: "",
    description: "",
    assigneeId: "",
    deadline: "",
    status: "Todo",
  });
  const [selectedTaskDetail, setSelectedTaskDetail] = useState(null);
  const [taskDetailLoading, setTaskDetailLoading] = useState(false);
  const [taskDetailError, setTaskDetailError] = useState("");

  const availableMembers = useMemo(() => {
    const q = query.trim().toLowerCase();
    return assignableMembers.filter((m) => {
      const fullName = (m.fullName || m.user?.fullName || "").toLowerCase();
      const email = (m.email || m.user?.email || "").toLowerCase();
      if (!q) return true;
      return fullName.includes(q) || email.includes(q);
    });
  }, [assignableMembers, query]);

  const groupedTasks = useMemo(() => {
    const groups = { Todo: [], InProgress: [], Blocked: [], Done: [], Cancelled: [] };
    tasks.forEach((t) => {
      const key = t.status || "Todo";
      if (!groups[key]) groups[key] = [];
      groups[key].push(t);
    });
    return groups;
  }, [tasks]);

  const totalTasks = taskCount || tasks.length;

  const getMemberLabel = (member) => member.fullName || member.user?.fullName || member.email || member.user?.email || "Không rõ";

  const getStatusIcon = (status) => {
    switch (status) {
      case "Todo":
        return <ClockIcon className="dept-status-icon dept-status-icon--muted" />;
      case "InProgress":
        return <ClockIcon className="dept-status-icon dept-status-icon--progress" />;
      case "Blocked":
        return <AlertCircleIcon className="dept-status-icon dept-status-icon--blocked" />;
      case "Done":
        return <CheckCircleIcon className="dept-status-icon dept-status-icon--done" />;
      case "Cancelled":
        return <XCircleIcon className="dept-status-icon dept-status-icon--cancelled" />;
      default:
        return null;
    }
  };

  const getTaskStatusClass = (status) => {
    switch (status) {
      case "InProgress":
        return "dept-task-pill--progress";
      case "Blocked":
        return "dept-task-pill--blocked";
      case "Done":
        return "dept-task-pill--done";
      case "Cancelled":
        return "dept-task-pill--cancelled";
      default:
        return "dept-task-pill--todo";
    }
  };

  const getTaskMeta = (status) => TASK_STATUS_ORDER.find((item) => item.key === status) || TASK_STATUS_ORDER[0];

  const toggleTaskGroup = (status) => {
    setOpenTaskStatuses((prev) => ({ ...prev, [status]: !prev[status] }));
  };

  const openTaskDetail = async (task) => {
    setSelectedTaskDetail(task);
    setTaskDetailLoading(true);
    setTaskDetailError("");
    try {
      const detail = await getTaskById(task.id);
      setSelectedTaskDetail(detail);
    } catch (error) {
      setTaskDetailError(error?.message || "Failed to load task details");
      setSelectedTaskDetail(task);
    } finally {
      setTaskDetailLoading(false);
    }
  };

  const toggleMember = (id) => {
    setSelectedMemberIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
  };

  const submitAddMembers = (e) => {
    e.preventDefault();
    if (!selectedMemberIds.length) return;
    onAddMembers(department.id, selectedMemberIds);
    setSelectedMemberIds([]);
  };

  const submitTask = (e) => {
    e.preventDefault();
    if (!canManageTasks) return;
    onCreateTask(department.id, taskForm);
    setTaskForm({ taskName: "", description: "", assigneeId: "", deadline: "", status: "Todo" });
    setShowTaskForm(false);
  };

  return (
    <div className="dept-card">
      <div className="dept-card-header">
        <div className="dept-card-heading">
          <div className="dept-card-title-row">
            <h3 className="dept-card-title">{department.departmentName || department.deptName}</h3>
            <span className="dept-card-chip">{managerName ? "Có quản lý" : "Chưa có quản lý"}</span>
          </div>
          <p className="dept-card-desc">{department.description || department.function || "Chưa có mô tả phòng ban."}</p>
        </div>

        {canManage && (
          <div className="dept-card-actions-top">
            <button onClick={() => onEdit(department)} className="dept-icon-button" title="Sửa" type="button">
              <SettingsIcon className="dept-icon-button__icon" />
            </button>
            <button onClick={() => onDelete(department.id)} className="dept-icon-button dept-icon-button--danger" title="Xóa" type="button">
              <TrashIcon className="dept-icon-button__icon" />
            </button>
          </div>
        )}
      </div>

      <div className="dept-card-tabs">
        {[
          { id: "overview", label: "Tổng quan" },
          { id: "members", label: "Thành viên" },
          { id: "tasks", label: "Nhiệm vụ" },
        ].map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`dept-card-tab ${activeTab === tab.id ? "is-active" : ""}`}
            type="button"
          >
            <span>{tab.label}</span>
          </button>
        ))}
      </div>

      <div className="dept-card-panel">
        {activeTab === "overview" && (
          <div className="dept-panel-stack">
            <div className="dept-manager-card">
              <div className="dept-manager-row">
                <div className="dept-manager-avatar">{managerName?.charAt(0) || "M"}</div>
                <div>
                  <p className="dept-manager-name">{managerName || "Chưa phân công"}</p>
                  <p className="dept-manager-sub">Người phụ trách chính của phòng ban</p>
                </div>
              </div>
            </div>

            <div className="dept-overview-card">
              <p className="dept-section-label">Trạng thái nhanh</p>
              <div className="dept-overview-list">
                <div className="dept-overview-item">
                  <span className="dept-overview-key">Thành viên</span>
                  <strong className="dept-overview-value">{memberCount}</strong>
                </div>
                <div className="dept-overview-item">
                  <span className="dept-overview-key">Nhiệm vụ</span>
                  <strong className="dept-overview-value">{totalTasks}</strong>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === "members" && (
          <div className="dept-panel-stack">
            {canManageMembers ? (
              <>
                <form onSubmit={submitAddMembers} className="dept-member-picker">
                  <div className="dept-panel-head">
                    <div>
                      <p className="dept-section-label">Thêm thành viên</p>
                      <h4 className="dept-panel-title">Chọn người có thể thêm vào phòng ban</h4>
                    </div>
                    <span className="dept-panel-meta">{selectedMemberIds.length} đã chọn</span>
                  </div>

                  <div className="dept-search-wrap">
                    <SearchIcon className="dept-search-icon" />
                    <input
                      className="dept-member-search"
                      placeholder="Tìm thành viên mới..."
                      value={query}
                      onChange={(e) => setQuery(e.target.value)}
                    />
                  </div>

                  <div className="dept-member-list custom-scrollbar">
                    {availableMembers.length > 0 ? (
                      availableMembers.map((m) => (
                        <label key={m.id} className={`dept-member-item ${selectedMemberIds.includes(m.id) ? "is-selected" : ""}`}>
                          <input
                            type="checkbox"
                            className="dept-member-checkbox"
                            checked={selectedMemberIds.includes(m.id)}
                            onChange={() => toggleMember(m.id)}
                          />
                          <div className="dept-member-text">
                            <span className="dept-member-name">{getMemberLabel(m)}</span>
                            <span className="dept-member-email">{m.email || m.user?.email || ""}</span>
                          </div>
                        </label>
                      ))
                    ) : (
                      <p className="dept-empty-state">Không tìm thấy ai phù hợp</p>
                    )}
                  </div>

                  <button disabled={isSubmitting || !selectedMemberIds.length} className="dept-primary-action" type="submit">
                    <UserPlusIcon className="dept-primary-action__icon" />
                    <span>Thêm ({selectedMemberIds.length}) thành viên</span>
                  </button>
                </form>

                <div className="dept-member-roster">
                  <div className="dept-panel-head">
                    <div>
                      <p className="dept-section-label">Thành viên hiện tại</p>
                      <h4 className="dept-panel-title">Danh sách đang thuộc phòng ban</h4>
                    </div>
                    <span className="dept-panel-meta">{departmentMembers.length} người</span>
                  </div>

                  <div className="dept-current-members custom-scrollbar">
                    {departmentMembers.length > 0 ? (
                      departmentMembers.map((m) => (
                        <div key={m.id} className="dept-current-member">
                          <div className="dept-current-avatar">{getMemberLabel(m).charAt(0) || "U"}</div>
                          <div className="dept-member-text">
                            <span className="dept-member-name">{getMemberLabel(m)}</span>
                            <span className="dept-member-email">{m.email || m.user?.email || ""}</span>
                          </div>
                          <button
                            type="button"
                            className="dept-icon-button dept-icon-button--danger"
                            onClick={() => onRemoveMember?.(department.id, m.id)}
                            disabled={isSubmitting}
                            title="Xóa khỏi ban"
                          >
                            <TrashIcon className="dept-icon-button__icon" />
                          </button>
                        </div>
                      ))
                    ) : (
                      <p className="dept-empty-state">Phòng ban chưa có thành viên</p>
                    )}
                  </div>
                </div>
              </>
            ) : (
              <div className="dept-member-roster">
                <div className="dept-panel-head">
                  <div>
                    <p className="dept-section-label">Thành viên hiện tại</p>
                    <h4 className="dept-panel-title">Danh sách đang thuộc phòng ban</h4>
                  </div>
                  <span className="dept-panel-meta">{departmentMembers.length} người</span>
                </div>

                <div className="dept-current-members custom-scrollbar">
                  {departmentMembers.length > 0 ? (
                    departmentMembers.map((m) => (
                      <div key={m.id} className="dept-current-member">
                        <div className="dept-current-avatar">{getMemberLabel(m).charAt(0) || "U"}</div>
                        <div className="dept-member-text">
                          <span className="dept-member-name">{getMemberLabel(m)}</span>
                          <span className="dept-member-email">{m.email || m.user?.email || ""}</span>
                        </div>
                      </div>
                    ))
                  ) : (
                    <p className="dept-empty-state">Phòng ban chưa có thành viên</p>
                  )}
                </div>
              </div>
            )}
          </div>
        )}

        {activeTab === "tasks" && (
          <div className="dept-panel-stack">
            <section className="dept-task-accordion">
              <div className="dept-task-accordion-header">
                <div>
                  <p className="dept-section-label">Danh sách công việc</p>
                  <h4 className="dept-panel-title">Quản lý nhiệm vụ theo trạng thái</h4>
                </div>
                {canManageTasks && (
                  <button
                    onClick={() => setShowTaskForm((prev) => !prev)}
                    className={`dept-task-toggle dept-task-toggle--wide ${showTaskForm ? "is-active" : ""}`}
                    type="button"
                  >
                    {showTaskForm ? <MoreVerticalIcon className="dept-task-toggle__icon" /> : <PlusIcon className="dept-task-toggle__icon" />}
                    <span>{showTaskForm ? "Ẩn form tạo task" : "+ Tạo nhiệm vụ mới"}</span>
                  </button>
                )}
              </div>

              <div className="dept-task-accordion-body">
                {showTaskForm && canManageTasks && (
                    <form onSubmit={submitTask} className="dept-task-form">
                      <div className="dept-task-form-row">
                        <input
                          className="dept-input"
                          type="date"
                          value={taskForm.deadline}
                          onChange={(e) => setTaskForm((prev) => ({ ...prev, deadline: e.target.value }))}
                        />
                      </div>
                      <div className="dept-task-status-row">
                        {TASK_STATUS_ORDER.map((statusInfo) => (
                          <button
                            key={statusInfo.key}
                            type="button"
                            className={`dept-task-status-chip ${taskForm.status === statusInfo.key ? "is-active" : ""}`}
                            onClick={() => setTaskForm((prev) => ({ ...prev, status: statusInfo.key }))}
                          >
                            {statusInfo.label}
                          </button>
                        ))}
                      </div>
                      <input
                        className="dept-input"
                        placeholder="Tên nhiệm vụ..."
                        value={taskForm.taskName}
                        onChange={(e) => setTaskForm((prev) => ({ ...prev, taskName: e.target.value }))}
                        required
                      />
                      <textarea
                        className="dept-input dept-input--textarea"
                        placeholder="Mô tả chi tiết..."
                        value={taskForm.description}
                        onChange={(e) => setTaskForm((prev) => ({ ...prev, description: e.target.value }))}
                      />
                      <select
                        className="dept-select"
                        value={taskForm.assigneeId}
                        onChange={(e) => setTaskForm((prev) => ({ ...prev, assigneeId: e.target.value }))}
                      >
                        <option value="">-- Chọn người thực hiện --</option>
                        {departmentMembers.map((m) => (
                          <option key={m.id} value={m.id}>
                            {getMemberLabel(m)}
                          </option>
                        ))}
                      </select>
                      <button className="dept-primary-action dept-primary-action--full" type="submit" disabled={isSubmitting}>
                        Tạo nhiệm vụ mới
                      </button>
                    </form>
                  )}
                <div className="dept-task-groups custom-scrollbar">
                  {TASK_STATUS_ORDER.map((statusInfo) => {
                    const list = groupedTasks[statusInfo.key] || [];
                    if (!list.length) return null;
                    const isOpen = openTaskStatuses[statusInfo.key] !== false;

                    return (
                      <section key={statusInfo.key} className="dept-task-group">
                        <button
                          type="button"
                          className={`dept-task-group-header ${isOpen ? "is-open" : "is-closed"}`}
                          onClick={() => toggleTaskGroup(statusInfo.key)}
                        >
                          <div className="dept-task-group-title-wrap">
                            {getStatusIcon(statusInfo.key)}
                            <span className="dept-task-group-title">{statusInfo.label}</span>
                          </div>
                          <div className="dept-task-group-meta">
                            <span className="dept-task-group-count">{list.length}</span>
                            <span className="dept-task-group-toggle">{isOpen ? "^" : "v"}</span>
                          </div>
                        </button>

                        {isOpen && (
                          <div className="dept-task-group-list">
                            {list.map((task) => {
                              const assignedMember = departmentMembers.find((member) => member.id === task.assigneeId);
                              const taskMeta = getTaskMeta(task.status || "Todo");

                              return (
                                <article key={task.id} className="dept-task-item">
                                  <div className="dept-task-item-body">
                                    <div className="dept-task-item-head">
                                      <h5 className="dept-task-title">{task.taskName}</h5>
                                      <div className="dept-task-item-actions">
                                        <button
                                          type="button"
                                          className="dept-task-detail-button"
                                          onClick={() => openTaskDetail(task)}
                                          title="Xem chi tiết task"
                                        >
                                          <EyeIcon className="dept-task-detail-button__icon" />
                                        </button>
                                        <span className={`dept-task-pill ${getTaskStatusClass(task.status)}`}>{taskMeta.label}</span>
                                      </div>
                                    </div>
                                    <p className="dept-task-desc">{task.description || "Không có mô tả"}</p>
                                    <div className="dept-task-meta">
                                      {task.deadline && (
                                        <span className="dept-task-meta-chip">
                                          <CalendarIcon className="dept-task-meta-icon" />
                                          <span>{task.deadline}</span>
                                        </span>
                                      )}
                                      {assignedMember && (
                                        <span className="dept-task-meta-chip">
                                          <UsersIcon className="dept-task-meta-icon" />
                                          <span>{getMemberLabel(assignedMember)}</span>
                                        </span>
                                      )}
                                    </div>
                                  </div>

                                  <div className="dept-task-controls">
                                    <div className="dept-task-inline-field">
                                      <span className="dept-task-inline-label">Status</span>
                                      <select
                                        value={task.status}
                                        onChange={(e) => onUpdateTaskStatus(task.id, e.target.value)}
                                        className="dept-select dept-select--compact dept-select--compact-chip"
                                      >
                                        <option value="Todo">Todo</option>
                                        <option value="InProgress">InProgress</option>
                                        <option value="Blocked">Blocked</option>
                                        <option value="Done">Done</option>
                                        <option value="Cancelled">Cancelled</option>
                                      </select>
                                    </div>

                                    <div className="dept-task-inline-field">
                                      <span className="dept-task-inline-label">Assignee</span>
                                      <select
                                        value={task.assigneeId || ""}
                                        onChange={(e) => onAssignTask(task.id, e.target.value)}
                                        className="dept-select dept-select--compact dept-select--compact-chip dept-select--compact-accent"
                                      >
                                        <option value="">Gán người</option>
                                        {departmentMembers.map((m) => (
                                          <option key={m.id} value={m.id}>
                                            {getMemberLabel(m)}
                                          </option>
                                        ))}
                                      </select>
                                    </div>
                                  </div>
                                </article>
                              );
                            })}
                          </div>
                        )}
                      </section>
                    );
                  })}

                  {tasks.length === 0 && !showTaskForm && (
                    <div className="dept-empty-card">
                      <ClipboardListIcon className="dept-empty-icon" />
                      <p className="dept-empty-state">Chưa có nhiệm vụ nào</p>
                    </div>
                  )}
                </div>
              </div>
            </section>

            {selectedTaskDetail && (
              <div className="dept-task-detail-modal" onClick={() => setSelectedTaskDetail(null)}>
                <div className="dept-task-detail-card" onClick={(e) => e.stopPropagation()}>
                  <div className="dept-task-detail-header">
                    <div>
                      <p className="dept-section-label">Chi tiết task</p>
                      <h4 className="dept-panel-title">{selectedTaskDetail.taskName}</h4>
                    </div>
                    <button type="button" className="dept-task-detail-close" onClick={() => setSelectedTaskDetail(null)}>
                      ×
                    </button>
                  </div>

                  {taskDetailLoading ? (
                    <p className="dept-empty-state">Đang tải chi tiết...</p>
                  ) : (
                    <div className="dept-task-detail-body">
                      {taskDetailError && <p className="dept-task-detail-error">{taskDetailError}</p>}
                      <div className="dept-task-detail-grid">
                        <div>
                          <span className="dept-task-detail-label">Status</span>
                          <strong>{selectedTaskDetail.status || "Todo"}</strong>
                        </div>
                        <div>
                          <span className="dept-task-detail-label">Priority</span>
                          <strong>{selectedTaskDetail.priority || "-"}</strong>
                        </div>
                        <div>
                          <span className="dept-task-detail-label">Assignee</span>
                          <strong>{selectedTaskDetail.assigneeName || "Chưa gán"}</strong>
                        </div>
                        <div>
                          <span className="dept-task-detail-label">Phòng ban</span>
                          <strong>{selectedTaskDetail.deptName || "-"}</strong>
                        </div>
                        <div>
                          <span className="dept-task-detail-label">Deadline</span>
                          <strong>{selectedTaskDetail.deadline ? new Date(selectedTaskDetail.deadline).toLocaleString() : "-"}</strong>
                        </div>
                        <div>
                          <span className="dept-task-detail-label">Completed</span>
                          <strong>{selectedTaskDetail.completedAt ? new Date(selectedTaskDetail.completedAt).toLocaleString() : "-"}</strong>
                        </div>
                      </div>

                      <div className="dept-task-detail-block">
                        <span className="dept-task-detail-label">Mô tả</span>
                        <p>{selectedTaskDetail.description || "Không có mô tả"}</p>
                      </div>

                      <div className="dept-task-detail-block">
                        <span className="dept-task-detail-label">Ghi chú</span>
                        <p>{selectedTaskDetail.note || "-"}</p>
                      </div>

                      <div className="dept-task-detail-grid dept-task-detail-grid--subtle">
                        <div>
                          <span className="dept-task-detail-label">Created by</span>
                          <strong>{selectedTaskDetail.createdByMemberName || "-"}</strong>
                        </div>
                        <div>
                          <span className="dept-task-detail-label">Created at</span>
                          <strong>{selectedTaskDetail.createdAtUtc ? new Date(selectedTaskDetail.createdAtUtc).toLocaleString() : "-"}</strong>
                        </div>
                        <div>
                          <span className="dept-task-detail-label">Updated at</span>
                          <strong>{selectedTaskDetail.updatedAtUtc ? new Date(selectedTaskDetail.updatedAtUtc).toLocaleString() : "-"}</strong>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default DepartmentCard;
