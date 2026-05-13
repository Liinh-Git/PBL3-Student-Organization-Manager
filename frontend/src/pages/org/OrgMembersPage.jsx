import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import { useAuth } from "../../hooks/useAuth.js";
import {
  getOrganizationMembers,
  addMember,
  updateMemberDepartment,
  removeMember,
} from "../../services/memberService.js";
import { getOrganizationRoles } from "../../services/roleService.js";
import { getOrganizationDepartments } from "../../services/departmentService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgMembersPage.css";

function OrgMembersPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get("orgId");
  const { permissions, isMember } = useOrgContext();
  const { user } = useAuth();

  const [members, setMembers] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [roles, setRoles] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showAddForm, setShowAddForm] = useState(false);
  const [pendingRemoveMember, setPendingRemoveMember] = useState(null);
  const [removeReason, setRemoveReason] = useState("");

  const canManage = permissions.includes("org.members.manage");
  const currentUserId = user?.id || user?.userId || null;

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadData() {
      setIsLoading(true);
      try {
        const [memberData, deptData, roleData] = await Promise.all([
          getOrganizationMembers(orgId),
          getOrganizationDepartments(orgId),
          getOrganizationRoles(orgId),
        ]);
        setMembers(memberData);
        setDepartments(deptData);
        setRoles(roleData);
      } catch (err) {
        setError(err.message || "Failed to load members");
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [orgId, isMember]);

  const isSelfMember = (member) => !!currentUserId && member?.userId === currentUserId;

  const isLeadershipRole = (member) => {
    const roleName = (member?.roleName || member?.role?.roleName || "").trim().toLowerCase();
    return roleName === "president" || roleName === "vice president" || roleName === "vicepresident";
  };

  const canLeaveOrganization = (member) => isSelfMember(member) && !isLeadershipRole(member);
  const myMemberRecord = members.find((member) => isSelfMember(member)) || null;

  const getDisplayRole = (member) => {
    const roleName = (member?.roleName || member?.role?.roleName || "MEMBER").trim();
    const normalized = roleName.toLowerCase();
    if (normalized === "manager") {
      const deptName = member?.departmentName || member?.department?.departmentName || member?.department?.deptName || "";
      if (deptName) return `Trưởng ban ${deptName}`;
      return "Trưởng ban";
    }
    return roleName;
  };

  const handleAddMember = async (e) => {
    e.preventDefault();
    if (!canManage) return;

    const form = e.target;
    const userId = form.userId.value;
    if (!userId) return;

    setIsSubmitting(true);
    try {
      const newMember = await addMember(orgId, {
        userId,
        roleId: form.roleId.value || undefined,
        departmentId: form.departmentId.value || undefined,
        studentCode: form.studentCode.value || undefined,
      });
      setMembers((prev) => [...prev, newMember]);
      form.reset();
      setShowAddForm(false);
    } catch (err) {
      alert(err.message || "Failed to add member");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateDepartment = async (memberId, newDeptId) => {
    if (!canManage) return;
    setIsSubmitting(true);
    try {
      const updated = await updateMemberDepartment(memberId, { departmentId: newDeptId || null });
      setMembers((prev) => prev.map((m) => (m.id === memberId ? updated : m)));
    } catch (err) {
      alert(err.message || "Failed to update department");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemoveMember = (memberId) => {
    if (!canManage) return;
    const targetMember = members.find((m) => m.id === memberId);
    if (targetMember && isSelfMember(targetMember)) return;
    setPendingRemoveMember(targetMember);
    setRemoveReason("");
  };

  const handleLeaveOrganization = (member) => {
    if (!canLeaveOrganization(member)) {
      alert("You cannot leave organization with current role");
      return;
    }
    setPendingRemoveMember(member);
    setRemoveReason("");
  };

  const handleConfirmRemoveMember = async () => {
    if (!pendingRemoveMember?.id) return;
    setIsSubmitting(true);
    try {
      await removeMember(pendingRemoveMember.id, { reason: removeReason || undefined });
      setMembers((prev) => prev.filter((m) => m.id !== pendingRemoveMember.id));
      const isSelf = isSelfMember(pendingRemoveMember);
      setPendingRemoveMember(null);
      if (isSelf) {
        window.location.href = "/user/organizations";
      }
    } catch (err) {
      alert(err.message || "Failed to remove member");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!orgId) return <ErrorState message="Organization ID is required" />;
  if (!isMember) return <ForbiddenState message="You are not a member of this organization" />;
  if (isLoading) return <LoadingSpinner message="Đang tải danh sách thành viên..." />;
  if (error) return <ErrorState message={error} />;

  return (
    <div className="members-container">
      <div className="members-table-wrapper">
        <div className="members-table-toolbar">
          <h3>Danh sách nhân sự</h3>
          {canManage && (
            <button onClick={() => setShowAddForm(true)} className="org-btn org-btn-primary" style={{ padding: "8px 16px", fontSize: "0.85rem" }}>
              + Thêm thành viên
            </button>
          )}
        </div>

        <table className="modern-table">
          <thead>
            <tr>
              <th>Họ và tên</th>
              <th>Phòng ban</th>
              <th>Vai trò</th>
              <th>Trạng thái</th>
              {canManage && <th style={{ textAlign: "center" }}>Thao tác</th>}
            </tr>
          </thead>
          <tbody>
            {members.map((member) => (
              <tr key={member.id}>
                <td>{member.fullName || member.user?.fullName || "Ẩn danh"}</td>
                <td>
                  {canManage ? (
                    <select
                      value={member.departmentId || ""}
                      onChange={(e) => handleUpdateDepartment(member.id, e.target.value)}
                      disabled={isSubmitting}
                      className="table-select"
                    >
                      <option value="">-- Trống --</option>
                      {departments.map((dept) => (
                        <option key={dept.id} value={dept.id}>
                          {dept.departmentName || dept.deptName}
                        </option>
                      ))}
                    </select>
                  ) : (
                    member.departmentName || member.department?.departmentName || member.department?.deptName || "-"
                  )}
                </td>
                <td>{getDisplayRole(member)}</td>
                <td>{member.status || "Active"}</td>
                {canManage && (
                  <td style={{ textAlign: "center" }}>
                    <div style={{ display: "inline-flex", gap: "0.5rem" }}>
                      {!isSelfMember(member) && (
                        <button onClick={() => handleRemoveMember(member.id)} disabled={isSubmitting} className="btn-remove-icon" title="Xóa thành viên">
                          X
                        </button>
                      )}
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>

        {canLeaveOrganization(myMemberRecord) && (
          <div style={{ marginTop: "1rem" }}>
            <button onClick={() => handleLeaveOrganization(myMemberRecord)} disabled={isSubmitting} className="app-button app-button--danger">
              {isSubmitting ? "Leaving..." : "Leave Organization"}
            </button>
          </div>
        )}
      </div>

      {showAddForm && canManage && (
        <div className="org-modal-overlay" onClick={() => setShowAddForm(false)}>
          <div className="org-modal" onClick={(e) => e.stopPropagation()}>
            <div className="org-modal-header"><h3>Thêm thành viên mới</h3></div>
            <div className="org-modal-body">
              <form id="addMemberForm" onSubmit={handleAddMember}>
                <div className="org-form-group"><label className="org-form-label">User ID *</label><input name="userId" className="org-input" required /></div>
                <div className="org-form-group"><label className="org-form-label">Mã sinh viên</label><input name="studentCode" className="org-input" /></div>
                <div className="org-form-group"><label className="org-form-label">Vai trò</label><select name="roleId" className="org-select"><option value="">-- Chọn vai trò --</option>{roles.map((role) => <option key={role.id} value={role.id}>{role.roleName}</option>)}</select></div>
                <div className="org-form-group"><label className="org-form-label">Phòng ban</label><select name="departmentId" className="org-select"><option value="">-- Chọn phòng ban --</option>{departments.map((dept) => <option key={dept.id} value={dept.id}>{dept.departmentName || dept.deptName}</option>)}</select></div>
              </form>
            </div>
            <div className="org-modal-footer">
              <button type="button" onClick={() => setShowAddForm(false)} className="org-btn org-btn-secondary" disabled={isSubmitting}>Hủy</button>
              <button type="submit" form="addMemberForm" className="org-btn org-btn-primary" disabled={isSubmitting}>{isSubmitting ? "Đang xử lý..." : "Thêm thành viên"}</button>
            </div>
          </div>
        </div>
      )}

      {pendingRemoveMember && (
        <div className="org-modal-overlay" onClick={() => setPendingRemoveMember(null)}>
          <div className="org-modal" onClick={(e) => e.stopPropagation()}>
            <div className="org-modal-header"><h3>{isSelfMember(pendingRemoveMember) ? "Rời nhóm" : "Xóa thành viên"}</h3></div>
            <div className="org-modal-body">
              <div className="org-form-group">
                <label className="org-form-label">Lý do</label>
                <textarea className="org-input" value={removeReason} onChange={(e) => setRemoveReason(e.target.value)} />
              </div>
              <p style={{ color: "#b91c1c", fontWeight: 700 }}>
                {isSelfMember(pendingRemoveMember) ? "Bạn sắp rời khỏi nhóm." : "Bạn sắp xóa thành viên khỏi nhóm."}
              </p>
            </div>
            <div className="org-modal-footer">
              <button type="button" className="org-btn org-btn-secondary" onClick={() => setPendingRemoveMember(null)} disabled={isSubmitting}>Hủy</button>
              <button type="button" className="org-btn org-btn-primary" onClick={handleConfirmRemoveMember} disabled={isSubmitting}>{isSubmitting ? "Đang xử lý..." : "Xác nhận"}</button>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}

export default OrgMembersPage;
