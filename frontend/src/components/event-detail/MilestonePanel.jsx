/**
 * MilestonePanel.jsx - Milestone panel component (EventDetail tree)
 */

function MilestonePanel({ milestone }) {
  return (
    <div className="milestone-panel">
      <div className="milestone-header">
        <h3>{milestone?.name || "Mốc"}</h3>
        <div className="milestone-actions">
          <button disabled>Sửa</button>
          <button disabled>Xóa</button>
        </div>
      </div>

      <div className="milestone-categories">
        <button disabled>Tạo hạng mục</button>
      </div>
    </div>
  );
}

export default MilestonePanel;
