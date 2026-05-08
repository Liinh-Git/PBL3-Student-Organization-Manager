/**
 * TaskCard.jsx - Task card component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component displays a single task.
 * 
 * Props:
 * - task: Task data object
 * - onUpdate: Callback to update task
 * - onDelete: Callback to delete task
 * - onUpdateStatus: Callback to update task status
 * - onAssign: Callback to assign task
 * - canManage: Boolean indicating if user has org.events.manage permission
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render task information (title, description, status, priority, dueDate)
 * - Render TaskStatusControl
 * - Render TaskAssignControl
 * - Add task edit/delete actions
 * 
 * IMPORTANT:
 * - This component does NOT own source-of-truth state
 * - All state lives in OrgEventDetailPage or useEventDetailTree hook
 * - This component receives task data and callbacks via props
 * - No real API calls in Phase 3C
 * - No fake data
 */

function TaskCard({
  task,
  onUpdate,
  onDelete,
  onUpdateStatus,
  onAssign,
  canManage = false
}) {
  return (
    <div className="task-card">
      {/* TODO Phase 3C-5+: Task header */}
      <div className="task-header">
        <h5>{task?.title || 'Task'}</h5>
        {canManage && (
          <div className="task-actions">
            <button disabled>Edit (TODO Phase 3C-5+)</button>
            <button disabled>Delete (TODO Phase 3C-5+)</button>
          </div>
        )}
      </div>

      {/* TODO Phase 3C-5+: Task details */}
      <div className="task-details">
        <p>{task?.description || ''}</p>
        {/* TODO: Display task.status */}
        {/* TODO: Display task.priority */}
        {/* TODO: Display task.dueDate */}
        {/* TODO: Display task.assignedMemberId */}
        {/* TODO: Display task.assignedDepartmentId */}
      </div>

      {/* TODO Phase 3C-5+: Task controls */}
      <div className="task-controls">
        {/* TODO: Render TaskStatusControl */}
        {/* TODO: Render TaskAssignControl */}
      </div>
    </div>
  );
}

export default TaskCard;
