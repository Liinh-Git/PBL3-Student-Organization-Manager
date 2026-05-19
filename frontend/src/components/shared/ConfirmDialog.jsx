/**
 * ConfirmDialog.jsx - Confirmation dialog component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Add modal/dialog styling
 * - Add backdrop/overlay
 * - Handle keyboard events (ESC to cancel)
 * - Add loading state for async confirmations
 * 
 * Usage:
 *   <ConfirmDialog
 *     isOpen={isOpen}
 *     title="Delete Event"
 *     message="Are you sure you want to delete this event?"
 *     onConfirm={handleDelete}
 *     onCancel={handleCancel}
 *   />
 */

function ConfirmDialog({ 
  isOpen = false,
  title = 'Xác nhận',
  message = 'Bạn có chắc chắn không?',
  confirmText = 'Xác nhận',
  cancelText = 'Hủy',
  onConfirm,
  onCancel
}) {
  if (!isOpen) return null;

  return (
    <div className="confirm-dialog-overlay">
      <div className="confirm-dialog">
        <h3>{title}</h3>
        <p>{message}</p>
        <div className="confirm-dialog-actions">
          <button onClick={onCancel} className="btn-cancel">
            {cancelText}
          </button>
          <button onClick={onConfirm} className="btn-confirm">
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  );
}

export default ConfirmDialog;
