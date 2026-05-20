/**
 * organizationAdapter.js - Organization DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 * 
 * IMPORTANT RULES:
 * - Backend DTOs are already well-structured for frontend use
 * - Simple pass-through for HTML-first UI
 * - Optional fields should render as "Chưa cập nhật" or be hidden in UI
 */

/**
 * Convert OrganizationDto to OrganizationViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toOrganizationViewModel(dto) {
  if (!dto) return null;
  return dto;
}

/**
 * Convert OrganizationSummaryDto to OrganizationSummaryViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toOrganizationSummaryViewModel(dto) {
  if (!dto) return null;
  return dto;
}

/**
 * Convert OrganizationPublicOverviewDto to OrganizationPublicOverviewViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toOrganizationPublicOverviewViewModel(dto) {
  if (!dto) return null;
  return dto;
}
