/**
 * DepartmentCard.jsx
 * Collapsed  → overview (name, desc, manager, stats, member avatars)
 * Expanded   → overview + task list (sorted by status + deadline)
 * Member btn → popup with member list + add member
 */

import { useState, useRef, useEffect, useMemo } from "react";
import { getTaskById } from "../../services/taskService.js";
import "./DepartmentCard.css";

/* ─── helpers ─────────────────────────────────── */
const initials = (name = "") => {
  const p = name.trim().split(/\s+/);
  if (!p[0]) return "?";
  return p.length === 1
    ? p[0].slice(0, 2).toUpperCase()
    : (p[0][0] + p[p.length - 1][0]).toUpperCase();
};

const STATUS_META = {
  Todo: { label: "Todo", cls: "dc-pill--todo" },
  InProgress: { label: "Đang làm", cls: "dc-pill--progress" },
  Done: { label: "Done", cls: "dc-pill--done" },
};

const STATUS_ORDER = ["InProgress", "Todo", "Done"];

const fmtDate = (iso) => {
  if (!iso) return null;
  const d = new Date(iso);
  return Number.isNaN(d.getTime())
    ? null
    : d.toLocaleDateString("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
};

const getFirstSentence = (text) => {
  if (!text) return "Không có mô tả";
  const trimmed = text.trim();
  const parts = trimmed.split(/(?<=[.!?])\s+/);
  return parts[0] || trimmed;
};

const sortByDeadline = (a, b) => {
  const ad = a.deadline ? new Date(a.deadline).getTime() : Infinity;
  const bd = b.deadline ? new Date(b.deadline).getTime() : Infinity;
  return ad - bd;
};

/* ─── AvatarStack ─────────────────────────────── */
function AvatarStack({ members, max = 4, onClick }) {
  const shown = members.slice(0, max);
  const extra = members.length - max;
  return (
    <button
      type="button"
      className="dc-avatar-stack"
      onClick={onClick}
      title="Xem thành viên"
    >
      {shown.map((m) => (
        <span key={m.id} className="dc-avatar" title={m.fullName || m.email}>
          {initials(m.fullName || m.email)}
        </span>
      ))}
      {extra > 0 && <span className="dc-avatar dc-avatar--more">+{extra}</span>}
      {members.length === 0 && (
        <span className="dc-avatar dc-avatar--empty">—</span>
      )}
    </button>
  );
}

/* ─── MembersPopup ────────────────────────────── */
function MembersPopup({
  departmentMembers,
  assignableMembers,
  canManageMembers,
  isSubmitting,
  onAddMembers,
  onRemoveMember,
  deptId,
  onClose,
  anchorRef,
}) {
  const popupRef = useRef(null);
  const [query, setQuery] = useState("");
  const [selected, setSelected] = useState([]);
  const [showAdd, setShowAdd] = useState(false);

  /* close on outside click */
  useEffect(() => {
    const handler = (e) => {
      if (
        popupRef.current &&
        !popupRef.current.contains(e.target) &&
        anchorRef.current &&
        !anchorRef.current.contains(e.target)
      )
        onClose();
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, [onClose, anchorRef]);

  const filteredAdd = assignableMembers.filter((m) =>
    (m.fullName || m.email || "").toLowerCase().includes(query.toLowerCase()),
  );

  const toggleSelect = (id) =>
    setSelected((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );

  const handleAdd = () => {
    if (selected.length === 0) return;
    onAddMembers(deptId, selected);
    setSelected([]);
    setShowAdd(false);
    setQuery("");
  };

  return (
    <div className="dc-members-popup" ref={popupRef}>
      {/* header */}
      <div className="dc-popup-head">
        <span className="dc-popup-title">
          Thành viên{" "}
          <span className="dc-popup-count">{departmentMembers.length}</span>
        </span>
        <div style={{ display: "flex", gap: "0.4rem", alignItems: "center" }}>
          {canManageMembers && (
            <button
              type="button"
              className={`dc-popup-add-btn${showAdd ? " is-active" : ""}`}
              onClick={() => {
                setShowAdd((v) => !v);
                setQuery("");
                setSelected([]);
              }}
            >
              {showAdd ? "✕" : "+ Thêm"}
            </button>
          )}
          <button type="button" className="dc-popup-close" onClick={onClose}>
            ✕
          </button>
        </div>
      </div>

      {/* add member panel */}
      {showAdd && (
        <div className="dc-popup-add-panel">
          <input
            className="dc-popup-search"
            placeholder="Tìm thành viên..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            autoFocus
          />
          <div className="dc-popup-list dc-popup-list--scroll">
            {filteredAdd.length === 0 ? (
              <p className="dc-popup-empty">Không có thành viên phù hợp</p>
            ) : (
              filteredAdd.map((m) => (
                <label
                  key={m.id}
                  className={`dc-popup-row dc-popup-row--pick${selected.includes(m.id) ? " is-selected" : ""}`}
                >
                  <input
                    type="checkbox"
                    className="dc-popup-checkbox"
                    checked={selected.includes(m.id)}
                    onChange={() => toggleSelect(m.id)}
                  />
                  <span className="dc-avatar dc-avatar--sm">
                    {initials(m.fullName || m.email)}
                  </span>
                  <span className="dc-popup-row-text">
                    <strong>{m.fullName || "—"}</strong>
                    <small>{m.email}</small>
                  </span>
                </label>
              ))
            )}
          </div>
          <button
            type="button"
            className="dc-popup-confirm"
            disabled={selected.length === 0 || isSubmitting}
            onClick={handleAdd}
          >
            {isSubmitting
              ? "Đang thêm..."
              : `Thêm ${selected.length > 0 ? `(${selected.length})` : ""}`}
          </button>
        </div>
      )}

      {/* current members list */}
      {!showAdd && (
        <div className="dc-popup-list dc-popup-list--scroll">
          {departmentMembers.length === 0 ? (
            <p className="dc-popup-empty">Chưa có thành viên nào</p>
          ) : (
            departmentMembers.map((m) => (
              <div key={m.id} className="dc-popup-row">
                <span className="dc-avatar dc-avatar--sm">
                  {initials(m.fullName || m.email)}
                </span>
                <span className="dc-popup-row-text">
                  <strong>{m.fullName || "—"}</strong>
                  <small>{m.roleName || m.role?.roleName || m.email}</small>
                </span>
                {canManageMembers && (
                  <button
                    type="button"
                    className="dc-popup-remove"
                    disabled={isSubmitting}
                    onClick={() => onRemoveMember(deptId, m.id)}
                    title="Xóa khỏi phòng ban"
                  >
                    ✕
                  </button>
                )}
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}

/* ─── AddTaskForm ─────────────────────────────── */
function AddTaskForm({
  deptId,
  departmentMembers,
  isSubmitting,
  onCreateTask,
  onClose,
}) {
  const [form, setForm] = useState({
    taskName: "",
    description: "",
    deadline: "",
    status: "Todo",
    assigneeId: "",
  });
  const set = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.value }));

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.taskName.trim()) return;
    onCreateTask(deptId, { ...form });
    onClose();
  };

  return (
    <form className="dc-add-task-form" onSubmit={handleSubmit}>
      <input
        className="dc-add-input"
        placeholder="Tên nhiệm vụ *"
        value={form.taskName}
        onChange={set("taskName")}
        required
        autoFocus
      />
      <textarea
        className="dc-add-input dc-add-textarea"
        placeholder="Mô tả..."
        value={form.description}
        onChange={set("description")}
      />
      <div className="dc-add-row">
        <select
          className="dc-add-select"
          value={form.status}
          onChange={set("status")}
        >
          {Object.entries(STATUS_META).map(([k, v]) => (
            <option key={k} value={k}>
              {v.label}
            </option>
          ))}
        </select>
        <input
          type="date"
          className="dc-add-select"
          value={form.deadline}
          onChange={set("deadline")}
        />
        <select
          className="dc-add-select"
          value={form.assigneeId}
          onChange={set("assigneeId")}
        >
          <option value="">Giao cho...</option>
          {departmentMembers.map((m) => (
            <option key={m.id} value={m.id}>
              {m.fullName || m.email}
            </option>
          ))}
        </select>
      </div>
      <div className="dc-add-footer">
        <button type="button" className="dc-add-cancel" onClick={onClose}>
          Hủy
        </button>
        <button type="submit" className="dc-add-submit" disabled={isSubmitting}>
          {isSubmitting ? "Đang lưu..." : "Thêm nhiệm vụ"}
        </button>
      </div>
    </form>
  );
}

/* ─── DepartmentCard (main) ───────────────────── */
function DepartmentCard({
  department,
  memberCount,
  departmentMembers,
  assignableMembers,
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
  tasks,
  onCreateTask,
  onUpdateTaskStatus,
  onAssignTask,
  initiallyExpanded = false,
}) {
  const [expanded, setExpanded] = useState(initiallyExpanded);
  const [showMembers, setShowMembers] = useState(false);
  const [showAddTask, setShowAddTask] = useState(false);
  const [statusFilter, setStatusFilter] = useState(["All"]);
  const [selectedTaskDetail, setSelectedTaskDetail] = useState(null);
  const [taskDetailLoading, setTaskDetailLoading] = useState(false);
  const [taskDetailError, setTaskDetailError] = useState("");
  const avatarBtnRef = useRef(null);

  const deptId = department.id;
  const deptName =
    department.deptName || department.departmentName || "Phòng ban";
  const deptDesc = department.description || department.function || "";

  const todoTasks = tasks.filter((t) => t.status === "Todo" || !t.status);
  const inProgTasks = tasks.filter((t) => t.status === "InProgress");
  const doneTasks = tasks.filter((t) => t.status === "Done");

  const tasksByStatus = useMemo(
    () =>
      STATUS_ORDER.map((status) => {
        const list = tasks
          .filter((t) => (t.status || "Todo") === status)
          .sort(sortByDeadline);
        return {
          status,
          label: STATUS_META[status]?.label || status,
          cls: STATUS_META[status]?.cls || "dc-pill--todo",
          tasks: list,
        };
      }),
    [tasks],
  );

  const activeFilters = Array.isArray(statusFilter)
    ? statusFilter
    : [statusFilter];

  const visibleTasks = activeFilters.includes("All")
    ? tasksByStatus.flatMap((g) => g.tasks)
    : tasksByStatus
        .filter((g) => activeFilters.includes(g.status))
        .flatMap((g) => g.tasks);

  const openTaskDetail = async (task) => {
    setSelectedTaskDetail(task);
    setTaskDetailLoading(true);
    setTaskDetailError("");
    try {
      const detail = await getTaskById(task.id);
      setSelectedTaskDetail(detail);
    } catch (error) {
      setTaskDetailError(error?.message || "Không thể tải chi tiết công việc");
      setSelectedTaskDetail(task);
    } finally {
      setTaskDetailLoading(false);
    }
  };

  return (
    <div className={`dc-card${expanded ? " dc-card--expanded" : ""}`}>
      {/* ── Top: name + avatar stack + actions ── */}
      <div className="dc-card-top">
        <div className="dc-card-title-area">
          <h3 className="dc-card-name">{deptName}</h3>
          {deptDesc && <p className="dc-card-desc">{deptDesc}</p>}
        </div>

        {/* avatar stack → members popup anchor */}
        <div className="dc-card-top-right" ref={avatarBtnRef}>
          <AvatarStack
            members={departmentMembers}
            max={3}
            onClick={() => setShowMembers((v) => !v)}
          />
        </div>
      </div>

      {/* ── Members popup ── */}
      {showMembers && (
        <MembersPopup
          deptId={deptId}
          departmentMembers={departmentMembers}
          assignableMembers={assignableMembers}
          canManageMembers={canManageMembers}
          isSubmitting={isSubmitting}
          onAddMembers={onAddMembers}
          onRemoveMember={onRemoveMember}
          onClose={() => setShowMembers(false)}
          anchorRef={avatarBtnRef}
        />
      )}

      {/* ── Stats row ── */}
      <div className="dc-stats">
        <div className="dc-stat">
          <svg
            width="13"
            height="13"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
          </svg>
          <span>{memberCount} thành viên</span>
        </div>
        <div className="dc-stat">
          <svg
            width="13"
            height="13"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <path d="M9 11l3 3L22 4" />
            <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
          </svg>
          <span>{taskCount} nhiệm vụ</span>
        </div>
        {managerName && managerName !== "-" && (
          <div className="dc-stat dc-stat--manager">
            <svg
              width="13"
              height="13"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <circle cx="12" cy="8" r="4" />
              <path d="M6 20v-2a4 4 0 0 1 8 0v2" />
            </svg>
            <span>{managerName}</span>
          </div>
        )}
      </div>

      {/* ── Task progress bar (always visible) ── */}
      {taskCount > 0 && (
        <div className="dc-progress-wrap">
          <div className="dc-progress-bar">
            <div
              className="dc-progress-fill"
              style={{
                width: `${Math.round((doneTasks.length / taskCount) * 100)}%`,
              }}
            />
          </div>
          <span className="dc-progress-label">
            {doneTasks.length}/{taskCount} hoàn thành
          </span>
        </div>
      )}

      {/* ── Expand: task list ── */}
      {expanded && (
        <div className="dc-tasks-section">
          <div className="dc-tasks-header">
            <span className="dc-tasks-label">Nhiệm vụ</span>
            <div className="dc-tasks-header-pills">
              {inProgTasks.length > 0 && (
                <span className="dc-pill dc-pill--progress">
                  {inProgTasks.length} đang làm
                </span>
              )}
              {todoTasks.length > 0 && (
                <span className="dc-pill dc-pill--todo">
                  {todoTasks.length} todo
                </span>
              )}
            </div>
            {canManageTasks && !showAddTask && (
              <button
                type="button"
                className="dc-add-task-btn"
                onClick={() => setShowAddTask(true)}
              >
                + Thêm
              </button>
            )}
          </div>

          {/* Filters */}
          <div className="dc-task-filters">
            <button
              type="button"
              className={`dc-task-filter-btn${activeFilters.includes("All") ? " is-active" : ""}`}
              onClick={() => setStatusFilter(["All"])}
            >
              Tất cả ({tasks.length})
            </button>
            {tasksByStatus.map((g) => (
              <button
                key={g.status}
                type="button"
                className={`dc-task-filter-btn${activeFilters.includes(g.status) ? " is-active" : ""}`}
                onClick={() =>
                  setStatusFilter((prev) => {
                    const list = Array.isArray(prev) ? prev : [prev];
                    if (list.includes("All")) {
                      return [g.status];
                    }
                    if (list.includes(g.status)) {
                      const next = list.filter((x) => x !== g.status);
                      return next.length ? next : ["All"];
                    }
                    return [...list, g.status];
                  })
                }
              >
                {g.label} ({g.tasks.length})
              </button>
            ))}
          </div>

          {showAddTask && (
            <AddTaskForm
              deptId={deptId}
              departmentMembers={departmentMembers}
              isSubmitting={isSubmitting}
              onCreateTask={onCreateTask}
              onClose={() => setShowAddTask(false)}
            />
          )}

          <div className="dc-task-list">
            {visibleTasks.length === 0 ? (
              <p className="dc-task-empty">Chưa có nhiệm vụ nào.</p>
            ) : (
              visibleTasks.map((task) => {
                const meta = STATUS_META[task.status] || STATUS_META.Todo;
                const assignee = departmentMembers.find(
                  (m) => m.id === task.assigneeId,
                );
                return (
                  <button
                    key={task.id}
                    type="button"
                    className="dc-task-item"
                    onClick={() => openTaskDetail(task)}
                  >
                    <div className="dc-task-item-head">
                      <div>
                        <h4 className="dc-task-title">
                          {task.taskName || task.name}
                        </h4>
                        <p className="dc-task-desc">
                          {getFirstSentence(task.description)}
                        </p>
                      </div>
                      <span className={`dc-pill ${meta.cls}`}>
                        {meta.label}
                      </span>
                    </div>

                    <div className="dc-task-meta">
                      {fmtDate(task.deadline) && (
                        <span className="dc-task-meta-chip">
                          <svg
                            className="dc-task-meta-icon"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                          >
                            <rect x="3" y="4" width="18" height="18" rx="2" />
                            <line x1="16" y1="2" x2="16" y2="6" />
                            <line x1="8" y1="2" x2="8" y2="6" />
                            <line x1="3" y1="10" x2="21" y2="10" />
                          </svg>
                          <span>{fmtDate(task.deadline)}</span>
                        </span>
                      )}
                      {assignee && (
                        <span className="dc-task-meta-chip">
                          <svg
                            className="dc-task-meta-icon"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                          >
                            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                            <circle cx="9" cy="7" r="4" />
                          </svg>
                          <span>{assignee.fullName || assignee.email}</span>
                        </span>
                      )}
                    </div>
                  </button>
                );
              })
            )}
          </div>
        </div>
      )}

      {/* ── Footer: expand toggle + manage actions ── */}
      <div className="dc-card-footer">
        <button
          type="button"
          className="dc-expand-btn"
          onClick={() => setExpanded((v) => !v)}
        >
          {expanded ? (
            <>
              <svg
                width="13"
                height="13"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
              >
                <polyline points="18 15 12 9 6 15" />
              </svg>
              Thu gọn
            </>
          ) : (
            <>
              <svg
                width="13"
                height="13"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
              >
                <polyline points="6 9 12 15 18 9" />
              </svg>
              Xem nhiệm vụ
            </>
          )}
        </button>

        {canManage && (
          <div className="dc-manage-actions">
            <button
              type="button"
              className="dc-icon-btn"
              title="Chỉnh sửa"
              onClick={() => onEdit(department)}
            >
              <svg
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
              </svg>
            </button>
            <button
              type="button"
              className="dc-icon-btn dc-icon-btn--danger"
              title="Xóa"
              onClick={() => onDelete(deptId)}
              disabled={isSubmitting}
            >
              <svg
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <polyline points="3 6 5 6 21 6" />
                <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
                <path d="M10 11v6M14 11v6" />
                <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
              </svg>
            </button>
          </div>
        )}
      </div>

      {selectedTaskDetail && (
        <div
          className="dc-task-detail-modal"
          onClick={() => setSelectedTaskDetail(null)}
        >
          <div
            className="dc-task-detail-card"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="dc-task-detail-header">
              <div>
                <p className="dc-task-detail-label">Chi tiết task</p>
                <h4 className="dc-card-name">{selectedTaskDetail.taskName}</h4>
              </div>
              <button
                type="button"
                className="dc-task-detail-close"
                onClick={() => setSelectedTaskDetail(null)}
              >
                ×
              </button>
            </div>

            {taskDetailLoading ? (
              <p className="dc-task-empty">Đang tải chi tiết...</p>
            ) : (
              <div className="dc-task-detail-body">
                {taskDetailError && (
                  <p className="dc-task-detail-error">{taskDetailError}</p>
                )}
                <div className="dc-task-detail-grid">
                  <div>
                    <span className="dc-task-detail-label">Trạng thái</span>
                    {canManageTasks ? (
                      <select
                        className="dc-add-select"
                        value={selectedTaskDetail.status || "Todo"}
                        onChange={(e) => {
                          const status = e.target.value;
                          setSelectedTaskDetail((prev) => ({
                            ...prev,
                            status,
                          }));
                          onUpdateTaskStatus(selectedTaskDetail.id, status);
                        }}
                      >
                        {Object.entries(STATUS_META).map(([k, v]) => (
                          <option key={k} value={k}>
                            {v.label}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <strong>{selectedTaskDetail.status || "Todo"}</strong>
                    )}
                  </div>

                  <div>
                    <span className="dc-task-detail-label">Độ ưu tiên</span>
                    <strong>{selectedTaskDetail.priority || "-"}</strong>
                  </div>

                  <div>
                    <span className="dc-task-detail-label">Assignee</span>
                    {canManageTasks ? (
                      <select
                        className="dc-add-select"
                        value={selectedTaskDetail.assigneeId || ""}
                        onChange={(e) => {
                          const assigneeId = e.target.value || null;
                          setSelectedTaskDetail((prev) => ({
                            ...prev,
                            assigneeId,
                          }));
                          onAssignTask(selectedTaskDetail.id, assigneeId);
                        }}
                      >
                        <option value="">Chưa gán</option>
                        {departmentMembers.map((m) => (
                          <option key={m.id} value={m.id}>
                            {m.fullName || m.email}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <strong>
                        {selectedTaskDetail.assigneeName || "Chưa gán"}
                      </strong>
                    )}
                  </div>

                  <div>
                    <span className="dc-task-detail-label">Phòng ban</span>
                    <strong>{selectedTaskDetail.deptName || "-"}</strong>
                  </div>

                  <div>
                    <span className="dc-task-detail-label">Deadline</span>
                    <strong>
                      {selectedTaskDetail.deadline
                        ? new Date(selectedTaskDetail.deadline).toLocaleString()
                        : "-"}
                    </strong>
                  </div>

                  <div>
                    <span className="dc-task-detail-label">Hoàn thành</span>
                    <strong>
                      {selectedTaskDetail.completedAt
                        ? new Date(
                            selectedTaskDetail.completedAt,
                          ).toLocaleString()
                        : "-"}
                    </strong>
                  </div>
                </div>

                <div className="dc-task-detail-block">
                  <span className="dc-task-detail-label">Mô tả</span>
                  <p>{selectedTaskDetail.description || "Không có mô tả"}</p>
                </div>

                <div className="dc-task-detail-block">
                  <span className="dc-task-detail-label">Ghi chú</span>
                  <p>{selectedTaskDetail.note || "-"}</p>
                </div>

                <div className="dc-task-detail-grid dc-task-detail-grid--subtle">
                  <div>
                    <span className="dc-task-detail-label">Created by</span>
                    <strong>
                      {selectedTaskDetail.createdByMemberName || "-"}
                    </strong>
                  </div>
                  <div>
                    <span className="dc-task-detail-label">Ngày tạo</span>
                    <strong>
                      {selectedTaskDetail.createdAtUtc
                        ? new Date(
                            selectedTaskDetail.createdAtUtc,
                          ).toLocaleString()
                        : "-"}
                    </strong>
                  </div>
                  <div>
                    <span className="dc-task-detail-label">Updated at</span>
                    <strong>
                      {selectedTaskDetail.updatedAtUtc
                        ? new Date(
                            selectedTaskDetail.updatedAtUtc,
                          ).toLocaleString()
                        : "-"}
                    </strong>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default DepartmentCard;
