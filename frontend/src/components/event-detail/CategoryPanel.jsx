/**
 * CategoryPanel.jsx - Category panel component (EventDetail tree)
 */

function CategoryPanel({ category }) {
  return (
    <div className="category-panel">
      <div className="category-header">
        <h4>{category?.name || "Hạng mục"}</h4>
        <div className="category-actions">
          <button disabled>Sửa</button>
          <button disabled>Xóa</button>
        </div>
      </div>

      <div className="category-tasks">
        <button disabled>Tạo nhiệm vụ</button>
      </div>
    </div>
  );
}

export default CategoryPanel;
