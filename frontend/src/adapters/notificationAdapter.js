/**
 * notificationAdapter.js - Notification DTO to ViewModel adapters
 * 
 * Phase 3C-4B: Adapter skeleton only
 * 
 * IMPORTANT RULES:
 * - Do not invent fake values
 * - Do not use mock field names
 * - Return null/empty safe shape when input is missing
 * - Optional fields should render as "Chưa cập nhật" or be hidden in UI later
 */

const NOTIFICATION_TITLE_TRANSLATIONS = {
  "Welcome!": "Chào mừng!",
  "New Event Created": "Sự kiện mới đã được tạo",
  "Task Assigned": "Bạn được giao nhiệm vụ",
  "New join request": "Yêu cầu tham gia mới",
  "Department created": "Đã tạo phòng ban",
  "Request approved": "Yêu cầu đã được duyệt",
  "Request rejected": "Yêu cầu đã bị từ chối",
  "New friend request": "Lời mời kết bạn mới",
  "Friend request accepted": "Lời mời kết bạn đã được chấp nhận",
  "Friend request rejected": "Lời mời kết bạn đã bị từ chối",
  "New member recommendation": "Đề xuất thành viên mới",
  "Organization invitation": "Lời mời tham gia tổ chức",
  "Invitation accepted": "Lời mời đã được chấp nhận",
  "Invitation rejected": "Lời mời đã bị từ chối",
  ORG_INVITE: "Lời mời tham gia tổ chức",
  ORG_INVITE_RECOMMEND: "Đề xuất mời thành viên",
  "New task assigned": "Bạn được giao nhiệm vụ mới",
  "Task assignment updated": "Phân công nhiệm vụ đã được cập nhật",
  "Task completed": "Nhiệm vụ đã hoàn thành",
  "Department assignment updated": "Phân công phòng ban đã được cập nhật",
  "Department assignment removed": "Đã gỡ khỏi phòng ban",
  "Member added to your department": "Thành viên mới trong phòng ban",
  "Member removed from your department": "Đã xóa thành viên khỏi phòng ban",
  "Left organization": "Đã rời tổ chức",
  "Removed from organization": "Đã bị xóa khỏi tổ chức",
  "Member left organization": "Thành viên đã rời tổ chức",
};

function translateNotificationTitle(title) {
  if (!title) return "";
  const direct = NOTIFICATION_TITLE_TRANSLATIONS[title];
  if (direct) return direct;

  const normalized = String(title).toLowerCase();
  if (normalized === "request approved") return "Yêu cầu đã được duyệt";
  if (normalized === "request rejected") return "Yêu cầu đã bị từ chối";
  if (normalized === "invitation accepted") return "Lời mời đã được chấp nhận";
  if (normalized === "invitation rejected") return "Lời mời đã bị từ chối";

  return title;
}

function translateNotificationMessage(message) {
  const text = message || "";

  if (text === "Welcome to the Student Organization Management System.") {
    return "Chào mừng bạn đến với hệ thống quản lý tổ chức sinh viên.";
  }

  if (text === "You received a new friend request.") {
    return "Bạn nhận được một lời mời kết bạn mới.";
  }

  if (text === "Your friend request was rejected.") {
    return "Lời mời kết bạn của bạn đã bị từ chối.";
  }

  const friendAcceptedMatch = text.match(
    /^(.+?) accepted your friend request\.$/,
  );
  if (friendAcceptedMatch) {
    return `${friendAcceptedMatch[1]} đã chấp nhận lời mời kết bạn của bạn.`;
  }

  const joinRequestMatch = text.match(
    /^(.+?) submitted a join request to (.+?)\.$/,
  );
  if (joinRequestMatch) {
    return `${joinRequestMatch[1]} đã gửi yêu cầu tham gia ${joinRequestMatch[2]}.`;
  }

  const recommendationMatch = text.match(
    /^(.+?) recommended a friend to join (.+?)\.$/,
  );
  if (recommendationMatch) {
    return `${recommendationMatch[1]} đã đề xuất một người bạn tham gia ${recommendationMatch[2]}.`;
  }

  const organizationInvitationMatch = text.match(
    /^(.+?) invited you to join (.+?)\.$/,
  );
  if (organizationInvitationMatch) {
    return `${organizationInvitationMatch[1]} đã mời bạn tham gia ${organizationInvitationMatch[2]}.`;
  }

  const invitationReviewedMatch = text.match(
    /^(.+?) (accepted|rejected) your invitation to (.+?)\.$/,
  );
  if (invitationReviewedMatch) {
    const decision =
      invitationReviewedMatch[2] === "accepted"
        ? "chấp nhận"
        : "từ chối";
    return `${invitationReviewedMatch[1]} đã ${decision} lời mời tham gia ${invitationReviewedMatch[3]} của bạn.`;
  }

  const departmentCreatedMatch = text.match(
    /^Department '?(.+?)'? has been created\.$/,
  );
  if (departmentCreatedMatch) {
    return `Phòng ban ${departmentCreatedMatch[1]} đã được tạo.`;
  }

  const requestReviewedMatch = text.match(
    /^Your request to (.+?) was (approved|rejected) by (.+?)(?:\.|\. (.+))$/,
  );
  if (requestReviewedMatch) {
    const decision =
      requestReviewedMatch[2] === "approved" ? "được duyệt" : "bị từ chối";
    const detail = requestReviewedMatch[4]
      ? ` ${requestReviewedMatch[4]}`
      : "";
    return `Yêu cầu tham gia ${requestReviewedMatch[1]} của bạn đã ${decision} bởi ${requestReviewedMatch[3]}.${detail}`;
  }

  const taskAssignedMatch = text.match(
    /^Task '(.+?)' has been assigned to you\.$/,
  );
  if (taskAssignedMatch) {
    return `Bạn đã được giao nhiệm vụ "${taskAssignedMatch[1]}".`;
  }

  const taskDoneMatch = text.match(/^Task '(.+?)' has been moved to Done\.$/);
  if (taskDoneMatch) {
    return `Nhiệm vụ "${taskDoneMatch[1]}" đã được chuyển sang hoàn thành.`;
  }

  const eventCreatedMatch = text.match(
    /^A new event '(.+?)' has been created\.$/,
  );
  if (eventCreatedMatch) {
    return `Sự kiện mới "${eventCreatedMatch[1]}" đã được tạo.`;
  }

  const assignedDeptMatch = text.match(
    /^You have been assigned to department '(.+?)'\.$/,
  );
  if (assignedDeptMatch) {
    return `Bạn đã được phân công vào phòng ban "${assignedDeptMatch[1]}".`;
  }

  if (text === "You have been removed from your department.") {
    return "Bạn đã được gỡ khỏi phòng ban.";
  }

  const memberAssignedDeptMatch = text.match(
    /^(.+?) was assigned to your department '(.+?)'\.$/,
  );
  if (memberAssignedDeptMatch) {
    return `${memberAssignedDeptMatch[1]} đã được phân công vào phòng ban "${memberAssignedDeptMatch[2]}".`;
  }

  const memberRemovedDeptMatch = text.match(
    /^(.+?) was removed from your department '(.+?)'\.$/,
  );
  if (memberRemovedDeptMatch) {
    return `${memberRemovedDeptMatch[1]} đã được xóa khỏi phòng ban "${memberRemovedDeptMatch[2]}".`;
  }

  const memberLeftOrgMatch = text.match(
    /^(.+?) has left (.+?)\. Reason: (.+)$/,
  );
  if (memberLeftOrgMatch) {
    return `${memberLeftOrgMatch[1]} đã rời ${memberLeftOrgMatch[2]}. Lý do: ${memberLeftOrgMatch[3]}`;
  }

  return text;
}

/**
 * Convert NotificationDto to NotificationViewModel
 * 
 * TODO Phase implementation:
 * Input: NotificationDto from NotificationContracts.cs.TODO
 * Expected fields:
 * - id, receiverId, actorId?, title, message, type, relatedEntityType?, relatedEntityId?, actionUrl?, isRead, readAt?, createdAt
 * Output ViewModel:
 * - Used by notification list and badge components
 * Rules:
 * - type values: System, RequestSubmitted, RequestReviewed, FriendRequest, EventCreated, EventUpdated, EventReminder, TaskAssigned, TaskDue, ResourceChanged
 * - actorId is optional (system notifications may not have actor)
 * - Do not fake actor if missing
 */
export function toNotificationViewModel(dto) {
  if (!dto) return null;
  return {
    id: dto.id ?? null,
    receiverId: dto.receiverId ?? null,
    actorId: dto.actorId ?? null,
    actorName: dto.actorName ?? null,
    title: translateNotificationTitle(dto.title),
    message: translateNotificationMessage(dto.message),
    type: dto.type ?? 'System',
    relatedEntityType: dto.relatedEntityType ?? null,
    relatedEntityId: dto.relatedEntityId ?? null,
    actionUrl: dto.actionUrl ?? null,
    isRead: Boolean(dto.isRead),
    readAt: dto.readAt ?? null,
    createdAtUtc: dto.createdAtUtc ?? null,
  };
}

/**
 * Convert NotificationDto[] to NotificationListViewModel
 * 
 * TODO Phase implementation:
 * Input: NotificationDto[] from NotificationContracts.cs.TODO
 * Output ViewModel:
 * - Array of NotificationViewModel
 * Rules:
 * - Map each item using toNotificationViewModel
 * - Filter out null items
 */
export function toNotificationListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toNotificationViewModel).filter(Boolean);
}
