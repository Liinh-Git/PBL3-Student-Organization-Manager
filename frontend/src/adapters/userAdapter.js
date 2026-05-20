/**
 * userAdapter.js - User DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 * 
 * IMPORTANT RULES:
 * - Backend DTOs are already well-structured for frontend use
 * - Simple pass-through for HTML-first UI
 * - Optional fields should render as "Chưa cập nhật" or be hidden in UI
 */

/**
 * Convert UserProfileDto to UserProfileViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toUserProfileViewModel(dto) {
  if (!dto) return null;
  return dto;
}

/**
 * Convert MyOrganizationDto to MyOrganizationViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toMyOrganizationViewModel(dto) {
  if (!dto) return null;
  return dto;
}

/**
 * Convert MyEventDto to MyEventViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toMyEventViewModel(dto) {
  if (!dto) return null;
  return dto;
}

/**
 * Convert DiscoverOrganizationDto to DiscoverOrganizationViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toDiscoverOrganizationViewModel(dto) {
  if (!dto) return null;
  return dto;
}
