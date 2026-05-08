/**
 * MilestonePanel.jsx - Milestone panel component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component displays a single milestone and its categories.
 * 
 * Props:
 * - milestone: Milestone data object
 * - categories: Array of categories for this milestone
 * - tasks: Map of tasks by categoryId
 * - onCreateCategory: Callback to create category
 * - onUpdateCategory: Callback to update category
 * - onDeleteCategory: Callback to delete category
 * - onCreateTask: Callback to create task
 * - onUpdateTask: Callback to update task
 * - onDeleteTask: Callback to delete task
 * - onUpdateTaskStatus: Callback to update task status
 * - onAssignTask: Callback to assign task
 * - canManage: Boolean indicating if user has org.events.manage permission
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render milestone header (name, description, dueDate, orderIndex)
 * - Render CategoryPanel components for each category
 * - Add "Create Category" button
 * - Add milestone edit/delete actions
 * 
 * IMPORTANT:
 * - This component does NOT own source-of-truth state
 * - All state lives in OrgEventDetailPage or useEventDetailTree hook
 * - This component receives data and callbacks via props
 * - No real API calls in Phase 3C
 * - No fake data
 */

function MilestonePanel({
  milestone,
  categories = [],
  tasks = {},
  onCreateCategory,
  onUpdateCategory,
  onDeleteCategory,
  onCreateTask,
  onUpdateTask,
  onDeleteTask,
  onUpdateTaskStatus,
  onAssignTask,
  canManage = false
}) {
  return (
    <div className="milestone-panel">
      {/* TODO Phase 3C-5+: Milestone header */}
      <div className="milestone-header">
        <h3>{milestone?.name || 'Milestone'}</h3>
        {/* TODO: Display milestone.description */}
        {/* TODO: Display milestone.dueDate */}
        {/* TODO: Display milestone.orderIndex */}
        {canManage && (
          <div className="milestone-actions">
            <button disabled>Edit (TODO Phase 3C-5+)</button>
            <button disabled>Delete (TODO Phase 3C-5+)</button>
          </div>
        )}
      </div>

      {/* TODO Phase 3C-5+: Categories section */}
      <div className="milestone-categories">
        {canManage && (
          <button disabled>
            Create Category (TODO Phase 3C-5+)
          </button>
        )}
        {/* TODO: Render CategoryPanel components */}
        {/* TODO: Pass categories, tasks, and callbacks */}
      </div>
    </div>
  );
}

export default MilestonePanel;
