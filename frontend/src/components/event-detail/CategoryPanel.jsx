/**
 * CategoryPanel.jsx - Category panel component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component displays a single category and its tasks.
 * 
 * Props:
 * - category: Category data object
 * - tasks: Array of tasks for this category
 * - onCreateTask: Callback to create task
 * - onUpdateTask: Callback to update task
 * - onDeleteTask: Callback to delete task
 * - onUpdateTaskStatus: Callback to update task status
 * - onAssignTask: Callback to assign task
 * - canManage: Boolean indicating if user has org.events.manage permission
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render category header (name, description)
 * - Render TaskCard components for each task
 * - Add "Create Task" button
 * - Add category edit/delete actions
 * 
 * IMPORTANT:
 * - This component does NOT own source-of-truth state
 * - All state lives in OrgEventDetailPage or useEventDetailTree hook
 * - This component receives data and callbacks via props
 * - No real API calls in Phase 3C
 * - No fake data
 */

function CategoryPanel({
  category,
  tasks = [],
  onCreateTask,
  onUpdateTask,
  onDeleteTask,
  onUpdateTaskStatus,
  onAssignTask,
  canManage = false
}) {
  return (
    <div className="category-panel">
      {/* TODO Phase 3C-5+: Category header */}
      <div className="category-header">
        <h4>{category?.name || 'Category'}</h4>
        {/* TODO: Display category.description */}
        {canManage && (
          <div className="category-actions">
            <button disabled>Edit (TODO Phase 3C-5+)</button>
            <button disabled>Delete (TODO Phase 3C-5+)</button>
          </div>
        )}
      </div>

      {/* TODO Phase 3C-5+: Tasks section */}
      <div className="category-tasks">
        {canManage && (
          <button disabled>
            Create Task (TODO Phase 3C-5+)
          </button>
        )}
        {/* TODO: Render TaskCard components */}
        {/* TODO: Pass tasks and callbacks */}
      </div>
    </div>
  );
}

export default CategoryPanel;
