/**
 * TaskCard.jsx - Task card component (EventDetail tree)
 */

function TaskCard({ task }) {
  return (
    <div className="task-card">
      <div className="task-header">
        <h5>{task?.title || "Nhiệm vụ"}</h5>
        <div className="task-actions">
          <button disabled>Sửa</button>
          <button disabled>Xóa</button>
        </div>
      </div>
      <p>{task?.description || ""}</p>
    </div>
  );
}

export default TaskCard;
