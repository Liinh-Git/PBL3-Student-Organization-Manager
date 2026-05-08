/**
 * TaskAssignControl.jsx - Task assign control component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component provides a control to assign task to member or department.
 * 
 * Props:
 * - task: Task data object
 * - onAssign: Callback to assign task
 * - canManage: Boolean indicating if user has org.events.manage permission
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render member/department select dropdowns
 * - Load members and departments lists
 * - Handle assignment change
 * - Call onAssign callback
 * 
 * IMPORTANT:
 * - This component does NOT own source-of-truth state
 * - All state lives in OrgEventDetailPage or useEventDetailTree hook
 * - This component receives task data and callback via props
 * - Task has single assignee only (assignedMemberId OR assignedDepartmentId)
 * - No real API calls in Phase 3C
 */

function TaskAssignControl({
  task,
  onAssign,
  canManage = false
}) {
  // TODO Phase 3C-5+: Load members and departments
  // const [members, setMembers] = useState([]);
  // const [departments, setDepartments] = useState([]);

  // TODO Phase 3C-5+: Handle assignment change
  // const handleAssignMember = (memberId) => {
  //   onAssign(task.id, { memberId, departmentId: null });
  // };

  // const handleAssignDepartment = (departmentId) => {
  //   onAssign(task.id, { memberId: null, departmentId });
  // };

  return (
    <div className="task-assign-control">
      <div className="assign-field">
        <label>Assigned Member:</label>
        <select disabled={!canManage} value={task?.assignedMemberId || ''}>
          <option value="">Unassigned</option>
          {/* TODO: Render member options */}
        </select>
      </div>

      <div className="assign-field">
        <label>Assigned Department:</label>
        <select disabled={!canManage} value={task?.assignedDepartmentId || ''}>
          <option value="">Unassigned</option>
          {/* TODO: Render department options */}
        </select>
      </div>
    </div>
  );
}

export default TaskAssignControl;
