/**
 * TaskFormModal.jsx - Task form modal component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component provides a modal form to create or edit a task.
 * 
 * Props:
 * - isOpen: Boolean indicating if modal is open
 * - onClose: Callback to close modal
 * - onSubmit: Callback to submit form
 * - task: Task data object (for edit mode, null for create mode)
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render modal with form fields (title, description, status, priority, dueDate)
 * - Handle form submission
 * - Call onSubmit callback with form data
 * - Validate form fields
 * 
 * IMPORTANT:
 * - No real API calls in Phase 3C
 * - No fake data
 */

function TaskFormModal({
  isOpen = false,
  onClose,
  onSubmit,
  task = null
}) {
  // TODO Phase 3C-5+: Add form state
  // const [formData, setFormData] = useState({
  //   title: task?.title || '',
  //   description: task?.description || '',
  //   status: task?.status || 'NotStarted',
  //   priority: task?.priority || 'Medium',
  //   dueDate: task?.dueDate || ''
  // });

  // TODO Phase 3C-5+: Handle form submission
  // const handleSubmit = (e) => {
  //   e.preventDefault();
  //   onSubmit(formData);
  //   onClose();
  // };

  if (!isOpen) return null;

  return (
    <div className="task-form-modal">
      <div className="modal-content">
        <h3>{task ? 'Edit Task' : 'Create Task'}</h3>
        <form>
          {/* TODO Phase 3C-5+: Form fields */}
          <div className="form-field">
            <label>Title</label>
            <input type="text" disabled placeholder="Task title" />
          </div>

          <div className="form-field">
            <label>Description</label>
            <textarea disabled placeholder="Task description" />
          </div>

          <div className="form-field">
            <label>Status</label>
            <select disabled>
              <option value="NotStarted">Not Started</option>
              <option value="InProgress">In Progress</option>
              <option value="Completed">Completed</option>
            </select>
          </div>

          <div className="form-field">
            <label>Priority</label>
            <select disabled>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
          </div>

          <div className="form-field">
            <label>Due Date</label>
            <input type="date" disabled />
          </div>

          <div className="modal-actions">
            <button type="button" onClick={onClose}>Cancel</button>
            <button type="submit" disabled>Save (TODO Phase 3C-5+)</button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default TaskFormModal;
