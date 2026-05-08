/**
 * MilestoneFormModal.jsx - Milestone form modal component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component provides a modal form to create or edit a milestone.
 * 
 * Props:
 * - isOpen: Boolean indicating if modal is open
 * - onClose: Callback to close modal
 * - onSubmit: Callback to submit form
 * - milestone: Milestone data object (for edit mode, null for create mode)
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render modal with form fields (name, description, dueDate, orderIndex)
 * - Handle form submission
 * - Call onSubmit callback with form data
 * - Validate form fields
 * 
 * IMPORTANT:
 * - No real API calls in Phase 3C
 * - No fake data
 */

function MilestoneFormModal({
  isOpen = false,
  onClose,
  onSubmit,
  milestone = null
}) {
  // TODO Phase 3C-5+: Add form state
  // const [formData, setFormData] = useState({
  //   name: milestone?.name || '',
  //   description: milestone?.description || '',
  //   dueDate: milestone?.dueDate || '',
  //   orderIndex: milestone?.orderIndex || 0
  // });

  // TODO Phase 3C-5+: Handle form submission
  // const handleSubmit = (e) => {
  //   e.preventDefault();
  //   onSubmit(formData);
  //   onClose();
  // };

  if (!isOpen) return null;

  return (
    <div className="milestone-form-modal">
      <div className="modal-content">
        <h3>{milestone ? 'Edit Milestone' : 'Create Milestone'}</h3>
        <form>
          {/* TODO Phase 3C-5+: Form fields */}
          <div className="form-field">
            <label>Name</label>
            <input type="text" disabled placeholder="Milestone name" />
          </div>

          <div className="form-field">
            <label>Description</label>
            <textarea disabled placeholder="Milestone description" />
          </div>

          <div className="form-field">
            <label>Due Date</label>
            <input type="date" disabled />
          </div>

          <div className="form-field">
            <label>Order Index</label>
            <input type="number" disabled placeholder="0" />
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

export default MilestoneFormModal;
