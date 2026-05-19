/**
 * FormModal.jsx - Generic form modal component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Add modal/dialog styling
 * - Add backdrop/overlay
 * - Handle keyboard events (ESC to close)
 * - Add loading state for form submission
 * - Add form validation display
 * 
 * Usage:
 *   <FormModal
 *     isOpen={isOpen}
 *     title="Create Event"
 *     onClose={handleClose}
 *   >
 *     <form onSubmit={handleSubmit}>
 *       ...form fields...
 *     </form>
 *   </FormModal>
 */

function FormModal({ 
  isOpen = false,
  title = 'Biểu mẫu',
  onClose,
  children
}) {
  if (!isOpen) return null;

  return (
    <div className="form-modal-overlay">
      <div className="form-modal">
        <div className="form-modal-header">
          <h3>{title}</h3>
          <button onClick={onClose} className="close-button">
            &times;
          </button>
        </div>
        <div className="form-modal-body">
          {children}
        </div>
      </div>
    </div>
  );
}

export default FormModal;
