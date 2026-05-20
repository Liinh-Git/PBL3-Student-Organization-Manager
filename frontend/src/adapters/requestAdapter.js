/**
 * requestAdapter.js - Request DTO to ViewModel adapters
 * 
 * Phase 3C-4B: Adapter skeleton only
 * 
 * IMPORTANT RULES:
 * - Do not invent fake values
 * - Do not use mock field names
 * - Return null/empty safe shape when input is missing
 * - Optional fields should render as "Chưa cập nhật" or be hidden in UI later
 */

/**
 * Convert RequestDto to RequestViewModel
 * 
 * TODO Phase implementation:
 * Input: RequestDto from RequestContracts.cs.TODO
 * Expected fields:
 * - id, senderId, orgId, requestType, title?, content, desiredDepartmentId?, desiredPosition?, status, reviewNote?, reviewedByMemberId?, reviewedAt?, createdAt
 * Output ViewModel:
 * - Used by OrgRequestsPage
 * Rules:
 * - requestType values: JoinOrganization, DepartmentChange, RoleChange, EventParticipation, Other
 * - status values: Pending, Approved, Rejected, Cancelled, Closed
 * - Do not fake desiredDepartment/desiredPosition if missing
 */
export function toRequestViewModel(dto) {
  if (!dto) return null;
  return {
    id: dto.id,
    senderId: dto.senderId,
    senderName: dto.senderName || 'Unknown',
    senderEmail: dto.senderEmail || null,
    organizationId: dto.organizationId,
    organizationName: dto.organizationName || '',
    requestType: dto.requestType || 'Other',
    title: dto.title || '',
    content: dto.content || '',
    desiredDepartmentId: dto.desiredDepartmentId || null,
    desiredDepartmentName: dto.desiredDepartmentName || null,
    desiredPosition: dto.desiredPosition || null,
    status: dto.status || 'Pending',
    reviewNote: dto.reviewNote || '',
    reviewedByMemberId: dto.reviewedByMemberId || null,
    reviewedByMemberName: dto.reviewedByMemberName || null,
    reviewedAt: dto.reviewedAt || null,
    createdAtUtc: dto.createdAtUtc || null,
    updatedAtUtc: dto.updatedAtUtc || null
  };
}

/**
 * Convert RequestDto[] to RequestListViewModel
 * 
 * TODO Phase implementation:
 * Input: RequestDto[] from RequestContracts.cs.TODO
 * Output ViewModel:
 * - Array of RequestViewModel
 * Rules:
 * - Map each item using toRequestViewModel
 * - Filter out null items
 */
export function toRequestListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toRequestViewModel).filter(Boolean);
}
