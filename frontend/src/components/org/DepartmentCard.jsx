/**
 * DepartmentCard.jsx - Component hiển thị thẻ phòng ban
 */

import { useState } from "react";

function DepartmentCard({
  department,
  memberCount,
  departmentMembers,
  assignableMembers,
  taskCount,
  managerName,
  canManage,
  canManageMembers,
  isSubmitting,
  onEdit,
  onDelete,
  onAddMember,
}) {
  const [selectedMember, setSelectedMember] = useState("");

  const handleAddSubmit = (e) => {
    e.preventDefault();
    if (selectedMember) {
      onAddMember(department.id, selectedMember);
      setSelectedMember("");
    }
  };

  // Lấy tối đa 3 thành viên để render Avatar xếp chồng
  const displayMembers = departmentMembers.slice(0, 3);
  const remainingMembers = memberCount > 3 ? memberCount - 3 : 0;

  return (
    <div className="dept-card">
      <h3 className="dept-card-title">
        {department.departmentName || department.deptName}
      </h3>
      <p className="dept-card-desc">
        {department.description || "Chưa có mô tả phòng ban."}
      </p>

      {/* Box Thành viên */}
      <div className="dept-members-box">
        <div className="dept-avatars-stack">
          {displayMembers.length > 0 ? (
            displayMembers.map((m, idx) => (
              <div
                key={idx}
                className="dept-avatar-circle"
                style={{ zIndex: 10 - idx }}
              >
                {m.user?.fullName?.charAt(0)?.toUpperCase() ||
                  m.fullName?.charAt(0)?.toUpperCase() ||
                  "U"}
              </div>
            ))
          ) : (
            <div
              className="dept-avatar-circle"
              style={{ background: "#cbd5e1" }}
            >
              -
            </div>
          )}
          {remainingMembers > 0 && (
            <div className="dept-avatar-circle more" style={{ zIndex: 0 }}>
              +{remainingMembers}
            </div>
          )}
        </div>
        <div className="dept-members-info">
          <h4>{memberCount} Thành viên</h4>
          <p>Quản lý: {managerName}</p>
        </div>
      </div>

      {/* Box Nhiệm vụ (Priority Tasks) */}
      <div className="dept-tasks-header">Nhiệm vụ phòng ban</div>
      <div className="dept-tasks-box">
        <h4>Tổng số công việc</h4>
        <p>{taskCount || 0} Nhiệm vụ được giao</p>
      </div>

      {/* Thêm thành viên mới (Nếu có quyền) */}
      {canManageMembers && (
        <form onSubmit={handleAddSubmit} className="dept-add-member-row">
          <select
            value={selectedMember}
            onChange={(e) => setSelectedMember(e.target.value)}
            className="dept-select-sm"
            disabled={isSubmitting}
          >
            <option value="">Thêm thành viên...</option>
            {assignableMembers.map((m) => (
              <option key={m.id} value={m.id}>
                {m.fullName || m.email || m.user?.fullName}
              </option>
            ))}
          </select>
          <button
            type="submit"
            disabled={isSubmitting || !selectedMember}
            className="btn-add-sm"
          >
            Thêm
          </button>
        </form>
      )}

      {/* Các nút Hành động */}
      {canManage && (
        <div className="dept-actions">
          <button
            className="btn-dark-full"
            onClick={() => onEdit(department)}
            disabled={isSubmitting}
          >
            Chỉnh sửa chi tiết
          </button>
          <button
            className="btn-danger-icon"
            onClick={() => onDelete(department.id)}
            disabled={isSubmitting}
            title="Xóa phòng ban"
          >
            <svg
              width="20"
              height="20"
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
        </div>
      )}
    </div>
  );
}

export default DepartmentCard;
