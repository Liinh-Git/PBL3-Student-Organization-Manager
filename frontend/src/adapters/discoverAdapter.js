/**
 * discoverAdapter.js - Discover DTO to ViewModel adapters
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
 * Convert DiscoverOrganizationDto to DiscoverOrganizationViewModel
 * 
 * TODO Phase implementation:
 * Input: DiscoverOrganizationDto from DiscoverContracts.cs.TODO
 * Expected fields:
 * - id, orgName, description?, avatarUrl?, totalMembers?, foundingDate?, location?
 * Output ViewModel:
 * - Used by UserDiscoverPage
 * Rules:
 * - Do not fake totalMembers/foundingDate/location if missing
 * - Optional fields should render as "Chưa cập nhật" or be hidden in UI later
 */
export function toDiscoverOrganizationViewModel(dto) {
  if (!dto) return null;
  throw new Error('TODO: implement toDiscoverOrganizationViewModel after DiscoverOrganizationDto is verified');
}

/**
 * Convert DiscoverEventDto to DiscoverEventViewModel
 * 
 * TODO Phase implementation:
 * Input: DiscoverEventDto from DiscoverContracts.cs.TODO
 * Expected fields:
 * - id, organizationId, organizationName?, eventName, description?, startDate, endDate, status, location?, averageRating?
 * Output ViewModel:
 * - Used by UserDiscoverPage
 * Rules:
 * - Do not fake organizationName/averageRating if missing
 * - Only public events are returned
 */
export function toDiscoverEventViewModel(dto) {
  if (!dto) return null;
  throw new Error('TODO: implement toDiscoverEventViewModel after DiscoverEventDto is verified');
}
