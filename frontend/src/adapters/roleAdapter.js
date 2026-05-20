/**
 * roleAdapter.js - Role and Permission DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 * 
 * IMPORTANT RULES:
 * - Backend DTOs are already well-structured for frontend use
 * - Simple pass-through for HTML-first UI
 */

/**
 * Convert PermissionDto to PermissionViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toPermissionViewModel(dto) {
  if (!dto) return null;
  return dto;
}

/**
 * Convert RoleDto to RoleViewModel
 * Pass-through since backend DTO matches frontend needs
 */
export function toRoleViewModel(dto) {
  if (!dto) return null;
  return dto;
}

/**
 * Convert RoleDto[] to RoleListViewModel
 * Map each item using toRoleViewModel
 */
export function toRoleListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toRoleViewModel).filter(Boolean);
}
