/**
 * OrgRolesPage.jsx - Organization roles page
 * 
 * Phase 4B-2: Write UI integration
 */

import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getOrganizationRoles, createRole, updateRole, deleteRole, assignRoleToMember } from '../../services/roleService.js';
import { getOrganizationMembers } from '../../services/memberService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ErrorState from '../../components/shared/ErrorState';
import ForbiddenState from '../../components/shared/ForbiddenState';

function OrgRolesPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');
  const { permissions, isMember } = useOrgContext();

  const [roles, setRoles] = useState([]);
  const [members, setMembers] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingRole, setEditingRole] = useState(null);

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadData() {
      setIsLoading(true);
      try {
        const [roleData, memberData] = await Promise.all([
          getOrganizationRoles(orgId),
          getOrganizationMembers(orgId)
        ]);
        setRoles(roleData);
        setMembers(memberData);
      } catch (err) {
        setError(err.message || 'Không thể tải danh sách vai trò');
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [orgId, isMember]);

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader
          title="Vai trò & Quyền hạn"
          description="Quản lý vai trò và quyền hạn trong tổ chức"
        />
        <ForbiddenState message="Bạn không phải thành viên của tổ chức này" />
      </div>
    );
  }

  const canCreate = permissions.includes('org.roles.create');
  const canUpdate = permissions.includes('org.roles.update');
  const canDelete = permissions.includes('org.roles.delete');
  const canAssign = permissions.includes('org.roles.assign');

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!canCreate) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const roleName = form.roleName.value;
    const description = form.description.value;
    const permissionKeys = form.permissionKeys.value.split(',').map(p => p.trim()).filter(p => p);
    
    if (!roleName) {
      alert('Tên vai trò là bắt buộc');
      return;
    }

    setIsSubmitting(true);
    try {
      const newRole = await createRole(orgId, {
        roleName,
        description: description || undefined,
        permissionKeys
      });
      setRoles(prev => [...prev, newRole]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || 'Không thể tạo vai trò');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canUpdate || !editingRole) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const roleName = form.roleName.value;
    const description = form.description.value;
    const permissionKeys = form.permissionKeys.value.split(',').map(p => p.trim()).filter(p => p);
    
    if (!roleName) {
      alert('Tên vai trò là bắt buộc');
      return;
    }

    setIsSubmitting(true);
    try {
      const updated = await updateRole(editingRole.id, {
        roleName,
        description: description || undefined,
        permissionKeys
      });
      setRoles(prev => prev.map(r => r.id === editingRole.id ? updated : r));
      setEditingRole(null);
    } catch (err) {
      alert(err.message || 'Không thể cập nhật vai trò');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (roleId) => {
    if (!canDelete) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    if (!window.confirm('Bạn có chắc muốn xóa vai trò này?')) {
      return;
    }

    setIsSubmitting(true);
    try {
      await deleteRole(roleId);
      setRoles(prev => prev.filter(r => r.id !== roleId));
    } catch (err) {
      alert(err.message || 'Không thể xóa vai trò');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAssignRole = async (memberId, roleId) => {
    if (!canAssign) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    setIsSubmitting(true);
    try {
      const updatedMember = await assignRoleToMember(orgId, memberId, { roleId });
      setMembers(prev => prev.map(m => m.id === memberId ? updatedMember : m));
    } catch (err) {
      alert(err.message || 'Không thể gán vai trò');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Vai trò & Quyền hạn"
          description="Quản lý vai trò và quyền hạn trong tổ chức"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Vai trò & Quyền hạn"
          description="Quản lý vai trò và quyền hạn trong tổ chức"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Vai trò & Quyền hạn"
        description="Quản lý vai trò và quyền hạn trong tổ chức"
        actions={
          canCreate && (
            <button onClick={() => setShowCreateForm(true)} className="app-button app-button--primary">
              Tạo vai trò
            </button>
          )
        }
      />

      <div className="app-section">
        {showCreateForm && canCreate && (
          <div className="app-card">
            <div className="app-section-header">
              <h3 className="app-section-title">Tạo vai trò</h3>
            </div>
            <form onSubmit={handleCreate} className="auth-form">
              <div className="form-group">
                <label className="form-label">Tên vai trò *</label>
                <input
                  name="roleName"
                  placeholder="Tên vai trò"
                  required
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Mô tả</label>
                <input
                  name="description"
                  placeholder="Mô tả"
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Danh sách quyền (phân tách bằng dấu phẩy)</label>
                <input
                  name="permissionKeys"
                  placeholder="Ví dụ: org.overview.write, org.events.manage"
                  className="form-input"
                />
              </div>
              <div className="app-action-row">
                <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                  {isSubmitting ? 'Đang tạo...' : 'Tạo'}
                </button>
                <button type="button" onClick={() => setShowCreateForm(false)} className="app-button app-button--ghost">
                  Hủy
                </button>
              </div>
            </form>
          </div>
        )}

        {editingRole && canUpdate && (
          <div className="app-card">
            <div className="app-section-header">
              <h3 className="app-section-title">Sửa vai trò</h3>
            </div>
            <form onSubmit={handleUpdate} className="auth-form">
              <div className="form-group">
                <label className="form-label">Tên vai trò *</label>
                <input
                  name="roleName"
                  defaultValue={editingRole.roleName}
                  placeholder="Tên vai trò"
                  required
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Mô tả</label>
                <input
                  name="description"
                  defaultValue={editingRole.description || ''}
                  placeholder="Mô tả"
                  className="form-input"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Danh sách quyền (phân tách bằng dấu phẩy)</label>
                <input
                  name="permissionKeys"
                  defaultValue={(editingRole.permissionKeys || []).join(', ')}
                  placeholder="Ví dụ: org.overview.write, org.events.manage"
                  className="form-input"
                />
              </div>
              <div className="app-action-row">
                <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                  {isSubmitting ? 'Đang cập nhật...' : 'Cập nhật'}
                </button>
                <button type="button" onClick={() => setEditingRole(null)} className="app-button app-button--ghost">
                  Hủy
                </button>
              </div>
            </form>
          </div>
        )}

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Vai trò</h3>
          </div>
          {roles.length === 0 ? (
            <EmptyState message="Chưa có vai trò nào" />
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Tên vai trò</th>
                  <th>Mô tả</th>
                  <th>Quyền</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {roles.map((role) => (
                  <tr key={role.id}>
                    <td>{role.roleName || '-'}</td>
                    <td>{role.description || '-'}</td>
                    <td style={{ maxWidth: '300px', wordWrap: 'break-word' }}>
                      {(role.permissionKeys || []).map((perm, idx) => (
                        <span key={idx} className="app-badge app-badge--info" style={{ marginRight: '4px', marginBottom: '4px', display: 'inline-block' }}>
                          {perm}
                        </span>
                      )) || '-'}
                    </td>
                    <td>
                      <div className="app-action-row">
                        {canUpdate && (
                          <button
                            onClick={() => setEditingRole(role)}
                            disabled={isSubmitting}
                            className="app-button app-button--secondary"
                          >
                            Sửa
                          </button>
                        )}
                        {canDelete && (
                          <button
                            onClick={() => handleDelete(role.id)}
                            disabled={isSubmitting}
                            className="app-button app-button--danger"
                          >
                            Xóa
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {canAssign && (
          <div className="app-card">
            <div className="app-section-header">
              <h3 className="app-section-title">Gán vai trò cho thành viên</h3>
            </div>
            {members.length === 0 ? (
              <EmptyState message="Chưa có thành viên nào" />
            ) : (
              <table>
                <thead>
                  <tr>
                    <th>Thành viên</th>
                    <th>Email</th>
                    <th>Vai trò hiện tại</th>
                    <th>Gán vai trò</th>
                  </tr>
                </thead>
                <tbody>
                  {members.map((member) => (
                    <tr key={member.id}>
                      <td>{member.user?.fullName || '-'}</td>
                      <td>{member.user?.email || '-'}</td>
                      <td>{member.role?.roleName || '-'}</td>
                      <td>
                        <select
                          value={member.roleId || ''}
                          onChange={(e) => handleAssignRole(member.id, e.target.value)}
                          disabled={isSubmitting}
                          className="form-select"
                          style={{ minWidth: '150px' }}
                        >
                          <option value="">Không có vai trò</option>
                          {roles.map(role => (
                            <option key={role.id} value={role.id}>
                              {role.roleName}
                            </option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default OrgRolesPage;
