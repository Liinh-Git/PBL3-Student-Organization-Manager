/**
 * TaskStatusControl.jsx - Task status control component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component provides a control to update task status.
 * 
 * Props:
 * - task: Task data object
 * - onUpdateStatus: Callback to update task status
 * - canManage: Boolean indicating if user has org.events.manage permission
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render status dropdown/select
 * - Handle status change
 * - Call onUpdateStatus callback
 * 
 * Task Status Values (from TaskStatus enum):
 * - NotStarted
 * - InProgress
 * - Completed
 * - Blocked
 * 
 * IMPORTANT:
 * - This component does NOT own source-of-truth state
 * - All state lives in OrgEventDetailPage or useEventDetailTree hook
 * - This component receives task data and callback via props
 * - No real API calls in Phase 3C
 */

function TaskStatusControl({
  task,
  onUpdateStatus,
  canManage = false
}) {
  // TODO Phase 3C-5+: Handle status change
  // const handleStatusChange = (newStatus) => {
  //   onUpdateStatus(task.id, { status: newStatus });
  // };

  return (
    <div className="task-status-control">
      <label>Status:</label>
      <select disabled={!canManage} value={task?.status || 'NotStarted'}>
        <option value="NotStarted">Not Started</option>
        <option value="InProgress">In Progress</option>
        <option value="Completed">Completed</option>
      </select>
    </div>
  );
}

export default TaskStatusControl;
