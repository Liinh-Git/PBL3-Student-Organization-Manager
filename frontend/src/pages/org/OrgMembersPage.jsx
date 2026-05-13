import { useState, useEffect, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import { useAuth } from "../../hooks/useAuth.js";
import {
  getOrganizationMembers,
  updateMemberDepartment,
  removeMember,
} from "../../services/memberService.js";
import { getFriends } from "../../services/friendService.js";
import {
  createOrganizationInvitation,
  createOrganizationInvitationRecommendation,
} from "../../services/invitationService.js";
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
  const [friends, setFriends] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showInviteForm, setShowInviteForm] = useState(false);
  const [friendSearch, setFriendSearch] = useState("");
  const [invitingFriendId, setInvitingFriendId] = useState(null);
  const [sentInviteUserIds, setSentInviteUserIds] = useState(new Set());
  const [pendingRemoveMember, setPendingRemoveMember] = useState(null);
  const [removeReason, setRemoveReason] = useState("");

  const canManage = permissions.includes("org.members.manage");
  const currentUserId = user?.id || user?.userId || null;

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadData() {
      setIsLoading(true);
      try {
        const [memberData, deptData, roleData, friendData] = await Promise.all([
          getOrganizationMembers(orgId),
          getOrganizationDepartments(orgId),
          getOrganizationRoles(orgId),
          getFriends(),
        ]);
        setMembers(memberData);
        setDepartments(deptData);
        setRoles(roleData);
        setFriends(friendData || []);
      } catch (err) {
        setError(err.message || "Failed to load members");
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [orgId, isMember, canManage]);

  const isSelfMember = (member) => !!currentUserId && member?.userId === currentUserId;

  const isLeadershipRole = (member) => {
    const roleName = (member?.roleName || member?.role?.roleName || "").trim().toLowerCase();
    return roleName === "president" || roleName === "vice president" || roleName === "vicepresident";
  };

  const canLeaveOrganization = (member) => isSelfMember(member) && !isLeadershipRole(member);
  const myMemberRecord = members.find((member) => isSelfMember(member)) || null;

  const filteredFriends = useMemo(() => {
    const keyword = friendSearch.trim().toLowerCase();
    return friends.filter((friend) => {
      const text = `${friend.fullName || ""} ${friend.email || ""}`.toLowerCase();
      return !keyword || text.includes(keyword);
    });
  }, [friends, friendSearch]);

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

  const handleInviteFriend = async (friendUserId) => {
    if (!isMember) return;
    setInvitingFriendId(friendUserId);
    try {
      try {
        if (canManage) {
          await createOrganizationInvitation(orgId, { receiverUserId: friendUserId });
        } else {
          await createOrganizationInvitationRecommendation(orgId, { receiverUserId: friendUserId });
        }
      } catch (inviteErr) {
        const msg = (inviteErr?.message || "").toLowerCase();
        if (msg.includes("permission to invite members")) {
          await createOrganizationInvitationRecommendation(orgId, { receiverUserId: friendUserId });
        } else {
          throw inviteErr;
        }
      }
      setSentInviteUserIds((prev) => {
        const next = new Set(prev);
        next.add(friendUserId);
        return next;
      });
    } catch (err) {
      alert(err.message || "Failed to invite friend");
    } finally {
      setInvitingFriendId(null);
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
          {isMember && (
            <button onClick={() => setShowInviteForm(true)} className="org-btn org-btn-primary" style={{ padding: "8px 16px", fontSize: "0.85rem" }}>
              {canManage ? "+ Mời bạn bè" : "+ Đề cử bạn bè"}
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

      {showInviteForm && isMember && (
        <div className="org-modal-overlay" onClick={() => setShowInviteForm(false)}>
          <div className="org-modal" onClick={(e) => e.stopPropagation()}>
            <div className="org-modal-header"><h3>{canManage ? "Mời bạn bè vào tổ chức hiện tại" : "Đề cử bạn bè (chờ leader duyệt)"}</h3></div>
            <div className="org-modal-body">
              <div className="org-form-group">
                <label className="org-form-label">Tìm bạn bè</label>
                <input
                  className="org-input"
                  value={friendSearch}
                  onChange={(e) => setFriendSearch(e.target.value)}
                  placeholder="Nhập tên hoặc email"
                />
              </div>
              <div style={{ display: "grid", gap: "0.6rem" }}>
                {filteredFriends.length === 0 ? (
                  <p style={{ color: "#64748b", margin: 0 }}>Không có bạn bè để mời.</p>
                ) : filteredFriends.map((friend) => {
                  const isMemberAlready = members.some((m) => m.userId === friend.userId);
                  const isSending = invitingFriendId === friend.userId;
                  const isSent = sentInviteUserIds.has(friend.userId);
                  return (
                    <div
                      key={friend.userId}
                      style={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        gap: "0.5rem",
                        border: "1px solid #e2e8f0",
                        borderRadius: "8px",
                        padding: "0.65rem 0.8rem",
                      }}
                    >
                      <div>
                        <div style={{ fontWeight: 700 }}>{friend.fullName}</div>
                        <div style={{ color: "#64748b", fontSize: "0.85rem" }}>{friend.email || "-"}</div>
                      </div>
                      <button
                        type="button"
                        className={`org-btn ${isMemberAlready || isSent ? "org-btn-secondary" : "org-btn-primary"}`}
                        disabled={isMemberAlready || isSent || isSending}
                        onClick={() => handleInviteFriend(friend.userId)}
                      >
                        {isMemberAlready ? "Đã là thành viên" : isSending ? "Đang gửi..." : isSent ? (canManage ? "Đã gửi lời mời" : "Đã gửi đề cử") : (canManage ? "Mời" : "Đề cử")}
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>
            <div className="org-modal-footer">
              <button type="button" onClick={() => setShowInviteForm(false)} className="org-btn org-btn-secondary" disabled={isSubmitting}>Đóng</button>
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
