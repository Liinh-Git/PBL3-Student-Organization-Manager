/**
 * OrgMembersPage.jsx - Organization members page
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getOrganizationMembers, addMember, updateMemberDepartment, removeMember } from '../../services/memberService.js';
import { getOrganizationRoles } from '../../services/roleService.js';
import { getOrganizationDepartments } from '../../services/departmentService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import ForbiddenState from '../../components/shared/ForbiddenState';

function OrgMembersPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');
  const { permissions, isMember } = useOrgContext();

  const [members, setMembers] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [roles, setRoles] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showAddForm, setShowAddForm] = useState(false);

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadData() {
      setIsLoading(true);
      try {
        const [memberData, deptData, roleData] = await Promise.all([
          getOrganizationMembers(orgId),
          getOrganizationDepartments(orgId),
          getOrganizationRoles(orgId)
        ]);
        setMembers(memberData);
        setDepartments(deptData);
        setRoles(roleData);
      } catch (err) {
        setError(err.message || 'Failed to load members');
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [orgId, isMember]);

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader
          title="Members"
          description="Manage organization members"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  const canManage = permissions.includes('org.members.manage');

  const handleAddMember = async (e) => {
    e.preventDefault();
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const userId = form.userId.value;
    const roleId = form.roleId.value;
    const departmentId = form.departmentId.value;
    const studentCode = form.studentCode.value;
    
    if (!userId) {
      alert('User ID is required');
      return;
    }

    setIsSubmitting(true);
    try {
      const newMember = await addMember(orgId, {
        userId,
        roleId: roleId || undefined,
        departmentId: departmentId || undefined,
        studentCode: studentCode || undefined
      });
      setMembers(prev => [...prev, newMember]);
      form.reset();
      setShowAddForm(false);
    } catch (err) {
      alert(err.message || 'Failed to add member');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateDepartment = async (memberId, newDeptId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await updateMemberDepartment(memberId, { departmentId: newDeptId || null });
      setMembers(prev => prev.map(m => m.id === memberId ? updated : m));
    } catch (err) {
      alert(err.message || 'Failed to update department');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemoveMember = async (memberId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    if (!window.confirm('Are you sure you want to remove this member?')) {
      return;
    }

    setIsSubmitting(true);
    try {
      await removeMember(memberId);
      setMembers(prev => prev.filter(m => m.id !== memberId));
    } catch (err) {
      alert(err.message || 'Failed to remove member');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Members"
          description="Manage organization members"
          actions={canManage && <button disabled className="app-button app-button--primary">Add Member</button>}
        />
        <LoadingSpinner message="Loading members..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Members"
          description="Manage organization members"
          actions={canManage && <button disabled className="app-button app-button--primary">Add Member</button>}
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Members"
        description="Manage organization members"
        actions={
          canManage && (
            <button 
              onClick={() => setShowAddForm(true)}
              className="app-button app-button--primary"
            >
              Add Member
            </button>
          )
        }
      />

      {showAddForm && canManage && (
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Add Member</h3>
          </div>
          <form onSubmit={handleAddMember} className="auth-form">
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: '0.9rem' }}>
              <div className="form-group">
                <label htmlFor="userId" className="form-label">User ID *</label>
                <input
                  id="userId"
                  name="userId"
                  className="form-input"
                  placeholder="User ID"
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="studentCode" className="form-label">Student Code</label>
                <input
                  id="studentCode"
                  name="studentCode"
                  className="form-input"
                  placeholder="Student Code"
                />
              </div>
              <div className="form-group">
                <label htmlFor="roleId" className="form-label">Role</label>
                <select id="roleId" name="roleId" className="form-select">
                  <option value="">No Role</option>
                  {roles.map(role => (
                    <option key={role.id} value={role.id}>
                      {role.roleName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label htmlFor="departmentId" className="form-label">Department</label>
                <select id="departmentId" name="departmentId" className="form-select">
                  <option value="">No Department</option>
                  {departments.map(dept => (
                    <option key={dept.id} value={dept.id}>
                      {dept.deptName || dept.departmentName}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="app-action-row">
              <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                {isSubmitting ? 'Adding...' : 'Add'}
              </button>
              <button type="button" onClick={() => setShowAddForm(false)} className="app-button app-button--ghost">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="app-section">
        {members.length === 0 ? (
          <EmptyState message="No members found" />
        ) : (
          <div className="app-card">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {members.map((member) => (
                  <tr key={member.id}>
                    <td>{member.user?.fullName || '-'}</td>
                    <td>{member.user?.email || '-'}</td>
                    <td>
                      {canManage ? (
                        <select
                          value={member.departmentId || ''}
                          onChange={(e) => handleUpdateDepartment(member.id, e.target.value)}
                          disabled={isSubmitting}
                          className="form-select"
                          style={{ minWidth: '150px' }}
                        >
                          <option value="">No Department</option>
                          {departments.map(dept => (
                            <option key={dept.id} value={dept.id}>
                              {dept.deptName || dept.departmentName}
                            </option>
                          ))}
                        </select>
                      ) : (
                        member.department?.deptName || '-'
                      )}
                    </td>
                    <td>{member.role?.roleName || '-'}</td>
                    <td><span className="app-badge app-badge--success">{member.status || '-'}</span></td>
                    <td>
                      {canManage && (
                        <button
                          onClick={() => handleRemoveMember(member.id)}
                          disabled={isSubmitting}
                          className="app-button app-button--danger"
                        >
                          {isSubmitting ? 'Removing...' : 'Remove'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default OrgMembersPage;
