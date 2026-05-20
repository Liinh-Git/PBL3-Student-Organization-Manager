/**
 * TaskStatusControl.jsx - Task status control component (EventDetail tree)
 */

function TaskStatusControl({
  task,
  onUpdateStatus,
  canManage = false,
}) {
  // Placeholder component for tree prototype
  void onUpdateStatus;

  return (
    <div className="task-status-control">
      <label>Trạng thái:</label>
      <select disabled={!canManage} value={task?.status || "NotStarted"}>
        <option value="NotStarted">Chưa bắt đầu</option>
        <option value="InProgress">Đang làm</option>
        <option value="Completed">Hoàn thành</option>
      </select>
    </div>
  );
}

export default TaskStatusControl;
