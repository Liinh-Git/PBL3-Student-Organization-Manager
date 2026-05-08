/**
 * CategoryFormModal.jsx - Category form modal component (EventDetail tree)
 * 
 * Phase 3C-4C: Component skeleton only
 * 
 * This component provides a modal form to create or edit a category.
 * 
 * Props:
 * - isOpen: Boolean indicating if modal is open
 * - onClose: Callback to close modal
 * - onSubmit: Callback to submit form
 * - category: Category data object (for edit mode, null for create mode)
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Render modal with form fields (name, description)
 * - Handle form submission
 * - Call onSubmit callback with form data
 * - Validate form fields
 * 
 * IMPORTANT:
 * - No real API calls in Phase 3C
 * - No fake data
 */

function CategoryFormModal({
  isOpen = false,
  onClose,
  onSubmit,
  category = null
}) {
  // TODO Phase 3C-5+: Add form state
  // const [formData, setFormData] = useState({
  //   name: category?.name || '',
  //   description: category?.description || ''
  // });

  // TODO Phase 3C-5+: Handle form submission
  // const handleSubmit = (e) => {
  //   e.preventDefault();
  //   onSubmit(formData);
  //   onClose();
  // };

  if (!isOpen) return null;

  return (
    <div className="category-form-modal">
      <div className="modal-content">
        <h3>{category ? 'Edit Category' : 'Create Category'}</h3>
        <form>
          {/* TODO Phase 3C-5+: Form fields */}
          <div className="form-field">
            <label>Name</label>
            <input type="text" disabled placeholder="Category name" />
          </div>

          <div className="form-field">
            <label>Description</label>
            <textarea disabled placeholder="Category description" />
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

export default CategoryFormModal;
