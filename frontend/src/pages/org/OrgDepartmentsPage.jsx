/**
 * OrgDepartmentsPage.jsx - Organization departments page
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getOrganizationDepartments, createDepartment, updateDepartment, deleteDepartment } from '../../services/departmentService.js';
import { getOrganizationMembers } from '../../services/memberService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import ForbiddenState from '../../components/shared/ForbiddenState';

function OrgDepartmentsPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');
  const { permissions, isMember } = useOrgContext();

  const [departments, setDepartments] = useState([]);
  const [members, setMembers] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingDept, setEditingDept] = useState(null);

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadData() {
      setIsLoading(true);
      try {
        const [deptData, memberData] = await Promise.all([
          getOrganizationDepartments(orgId),
          getOrganizationMembers(orgId)
        ]);
        setDepartments(deptData);
        setMembers(memberData);
      } catch (err) {
        setError(err.message || 'Failed to load departments');
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
      <div className="org-departments-page">
        <PageHeader
          title="Departments"
          description="Manage organization departments"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  const canManage = permissions.includes('org.departments.manage');

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const departmentName = form.departmentName.value;
    const description = form.description.value;
    const managerId = form.managerId.value || undefined;
    
    if (!departmentName) {
      alert('Department name is required');
      return;
    }

    setIsSubmitting(true);
    try {
      const newDept = await createDepartment(orgId, {
        departmentName,
        description: description || undefined,
        managerId
      });
      setDepartments(prev => [...prev, newDept]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || 'Failed to create department');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canManage || !editingDept) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const departmentName = form.departmentName.value;
    const description = form.description.value;
    const managerId = form.managerId.value || undefined;
    
    if (!departmentName) {
      alert('Department name is required');
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await updateDepartment(editingDept.id, {
        departmentName,
        description: description || undefined,
        managerId
      });
      setDepartments(prev => prev.map(d => d.id === editingDept.id ? updated : d));
      setEditingDept(null);
    } catch (err) {
      alert(err.message || 'Failed to update department');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (deptId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    if (!window.confirm('Are you sure you want to delete this department?')) {
      return;
    }

    setIsSubmitting(true);
    try {
      await deleteDepartment(deptId);
      setDepartments(prev => prev.filter(d => d.id !== deptId));
    } catch (err) {
      alert(err.message || 'Failed to delete department');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Departments"
          description="Manage organization departments"
          actions={canManage && <button className="app-button app-button--primary" disabled>Create Department</button>}
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Departments"
          description="Manage organization departments"
          actions={canManage && <button className="app-button app-button--primary" disabled>Create Department</button>}
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Departments"
        description="Manage organization departments"
        actions={
          canManage && (
            <button 
              onClick={() => setShowCreateForm(true)}
              className="app-button app-button--primary"
            >
              Create Department
            </button>
          )
        }
      />

      <div className="app-section">
        {showCreateForm && canManage && (
          <div className="app-card">
            <div className="app-section-header">
              <h3 className="app-section-title">Create Department</h3>
            </div>
            <form onSubmit={handleCreate} className="auth-form">
              <div className="form-group">
                <label className="form-label">Department Name *</label>
                <input
                  name="departmentName"
                  placeholder="Department name"
                  required
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Description</label>
                <input
                  name="description"
                  placeholder="Description"
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Manager</label>
                <select name="managerId" className="form-select">
                  <option value="">No Manager</option>
                  {members.map(member => (
                    <option key={member.id} value={member.id}>
                      {member.fullName || member.email}
                    </option>
                  ))}
                </select>
              </div>
              <div className="app-action-row">
                <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                  {isSubmitting ? 'Creating...' : 'Create'}
                </button>
                <button type="button" onClick={() => setShowCreateForm(false)} className="app-button app-button--ghost">
                  Cancel
                </button>
              </div>
            </form>
          </div>
        )}

        {editingDept && canManage && (
          <div className="app-card">
            <div className="app-section-header">
              <h3 className="app-section-title">Edit Department</h3>
            </div>
            <form onSubmit={handleUpdate} className="auth-form">
              <div className="form-group">
                <label className="form-label">Department Name *</label>
                <input
                  name="departmentName"
                  defaultValue={editingDept.departmentName}
                  placeholder="Department name"
                  required
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Description</label>
                <input
                  name="description"
                  defaultValue={editingDept.description || ''}
                  placeholder="Description"
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Manager</label>
                <select name="managerId" defaultValue={editingDept.managerId || ''} className="form-select">
                  <option value="">No Manager</option>
                  {members.map(member => (
                    <option key={member.id} value={member.id}>
                      {member.fullName || member.email}
                    </option>
                  ))}
                </select>
              </div>
              <div className="app-action-row">
                <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                  {isSubmitting ? 'Updating...' : 'Update'}
                </button>
                <button type="button" onClick={() => setEditingDept(null)} className="app-button app-button--ghost">
                  Cancel
                </button>
              </div>
            </form>
          </div>
        )}

        <div className="app-card">
          {departments.length === 0 ? (
            <EmptyState message="No departments found" />
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Manager</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {departments.map((dept) => (
                  <tr key={dept.id}>
                    <td>{dept.departmentName || '-'}</td>
                    <td>{dept.description || '-'}</td>
                    <td>{dept.manager?.user?.fullName || '-'}</td>
                    <td>{dept.status || '-'}</td>
                    <td>
                      <div className="app-action-row">
                        {canManage && (
                          <>
                            <button
                              onClick={() => setEditingDept(dept)}
                              disabled={isSubmitting}
                              className="app-button app-button--secondary"
                            >
                              Edit
                            </button>
                            <button
                              onClick={() => handleDelete(dept.id)}
                              disabled={isSubmitting}
                              className="app-button app-button--danger"
                            >
                              Delete
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
}

export default OrgDepartmentsPage;
