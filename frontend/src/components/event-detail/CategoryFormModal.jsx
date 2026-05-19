/**
 * CategoryFormModal.jsx - Category form modal component (EventDetail tree)
 */

function CategoryFormModal({ isOpen = false, onClose, category = null }) {
  if (!isOpen) return null;

  return (
    <div className="category-form-modal">
      <div className="modal-content">
        <h3>{category ? "Sửa hạng mục" : "Tạo hạng mục"}</h3>
        <form>
          <div className="form-field">
            <label>Tên hạng mục</label>
            <input type="text" disabled placeholder="Tên hạng mục" />
          </div>
          <div className="form-field">
            <label>Mô tả</label>
            <textarea disabled placeholder="Mô tả hạng mục" />
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

export default CategoryFormModal;
