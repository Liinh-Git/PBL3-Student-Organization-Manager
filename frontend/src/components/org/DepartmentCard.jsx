import { useState } from 'react';

function DepartmentCard({
  department,
  memberCount = 0,
  departmentMembers = [],
  assignableMembers = [],
  taskCount = null,
  managerName = '-',
  canManage = false,
  canManageMembers = false,
  isSubmitting = false,
  onEdit,
  onDelete,
  onAddMember
}) {
  const normalizedTaskCount = taskCount ?? '-';
  const [selectedMemberId, setSelectedMemberId] = useState('');

  return (
    <div className="app-card">
      <div className="app-section-header">
        <h3 className="app-section-title">{department.departmentName || 'Unnamed Department'}</h3>
      </div>

      <div className="app-muted">
        <p><strong>Manager:</strong> {managerName}</p>
        <p><strong>Description:</strong> {department.description || '-'}</p>
        <p><strong>Members:</strong> {memberCount}</p>
        <p><strong>Tasks:</strong> {normalizedTaskCount}</p>
        <p><strong>Status:</strong> {department.status || '-'}</p>
        <p>
          <strong>Department Members:</strong>{' '}
          {departmentMembers.length > 0
            ? departmentMembers.map((m) => m.fullName || m.email || m.id).join(', ')
            : '-'}
        </p>
      </div>

      <div className="app-action-row">
        {canManage && (
          <>
            <button
              type="button"
              onClick={() => onEdit(department)}
              disabled={isSubmitting}
              className="app-button app-button--secondary"
            >
              Edit
            </button>
            <button
              type="button"
              onClick={() => onDelete(department.id)}
              disabled={isSubmitting}
              className="app-button app-button--danger"
            >
              Delete
            </button>
          </>
        )}
      </div>

      {canManageMembers && (
        <div className="app-action-row" style={{ marginTop: '0.75rem' }}>
          <select
            value={selectedMemberId}
            onChange={(e) => setSelectedMemberId(e.target.value)}
            disabled={isSubmitting || assignableMembers.length === 0}
            className="form-select"
          >
            <option value="">Select member to add</option>
            {assignableMembers.map((member) => (
              <option key={member.id} value={member.id}>
                {member.fullName || member.email}
              </option>
            ))}
          </select>
          <button
            type="button"
            onClick={() => {
              onAddMember(department.id, selectedMemberId);
              setSelectedMemberId('');
            }}
            disabled={isSubmitting || !selectedMemberId}
            className="app-button app-button--ghost"
          >
            Add Member
          </button>
        </div>
      )}
    </div>
  );
}

export default DepartmentCard;
