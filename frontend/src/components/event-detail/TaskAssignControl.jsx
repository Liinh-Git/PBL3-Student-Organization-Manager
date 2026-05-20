/**
 * TaskAssignControl.jsx - Task assignment control (EventDetail tree)
 */

function TaskAssignControl({ task, members = [], departments = [], canManage = false }) {
  return (
    <div className="task-assign-control">
      <div className="form-field">
        <label>Thành viên được giao:</label>
        <select disabled={!canManage} value={task?.assignedMemberId || ""}>
          <option value="">Chưa gán</option>
          {members.map((member) => (
            <option key={member.id} value={member.id}>{member.fullName || member.email || member.id}</option>
          ))}
        </select>
      </div>

      <div className="form-field">
        <label>Phòng ban phụ trách:</label>
        <select disabled={!canManage} value={task?.assignedDepartmentId || ""}>
          <option value="">Chưa gán</option>
          {departments.map((dept) => (
            <option key={dept.id} value={dept.id}>{dept.departmentName || dept.deptName || dept.id}</option>
          ))}
        </select>
      </div>
    </div>
  );
}

export default TaskAssignControl;
