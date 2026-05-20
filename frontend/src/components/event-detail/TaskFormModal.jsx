/**
 * TaskFormModal.jsx - Task form modal component (EventDetail tree)
 */

function TaskFormModal({
  isOpen = false,
  onClose,
  onSubmit,
  task = null,
}) {
  void onSubmit;

  if (!isOpen) return null;

  return (
    <div className="task-form-modal">
      <div className="modal-content">
        <h3>{task ? "Sửa nhiệm vụ" : "Tạo nhiệm vụ"}</h3>
        <form>
          <div className="form-field">
            <label>Tiêu đề</label>
            <input type="text" disabled placeholder="Tên nhiệm vụ" />
          </div>

          <div className="form-field">
            <label>Mô tả</label>
            <textarea disabled placeholder="Mô tả nhiệm vụ" />
          </div>

          <div className="form-field">
            <label>Trạng thái</label>
            <select disabled>
              <option value="NotStarted">Chưa bắt đầu</option>
              <option value="InProgress">Đang làm</option>
              <option value="Completed">Hoàn thành</option>
            </select>
          </div>

          <div className="form-field">
            <label>Độ ưu tiên</label>
            <select disabled>
              <option value="Low">Thấp</option>
              <option value="Medium">Trung bình</option>
              <option value="High">Cao</option>
              <option value="Critical">Khẩn cấp</option>
            </select>
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

export default TaskFormModal;
