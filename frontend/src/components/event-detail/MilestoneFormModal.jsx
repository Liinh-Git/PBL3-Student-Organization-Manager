/**
 * MilestoneFormModal.jsx - Milestone form modal component (EventDetail tree)
 */

function MilestoneFormModal({ isOpen = false, onClose, milestone = null }) {
  if (!isOpen) return null;

  return (
    <div className="milestone-form-modal">
      <div className="modal-content">
        <h3>{milestone ? "Sửa mốc" : "Tạo mốc"}</h3>
        <form>
          <div className="form-field">
            <label>Tên mốc</label>
            <input type="text" disabled placeholder="Tên mốc" />
          </div>
          <div className="form-field">
            <label>Mô tả</label>
            <textarea disabled placeholder="Mô tả mốc" />
          </div>
          <div className="form-field">
            <label>Hạn chót</label>
            <input type="date" disabled />
          </div>
          <div className="modal-actions">
            <button type="button" onClick={onClose}>Hủy</button>
            <button type="submit" disabled>Lưu</button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default MilestoneFormModal;
