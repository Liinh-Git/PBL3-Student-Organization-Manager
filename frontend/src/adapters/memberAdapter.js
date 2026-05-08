/**
 * memberAdapter.js - Member DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 */

export function toMemberViewModel(dto) {
  if (!dto) return null;
  return dto;
}

export function toMemberListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toMemberViewModel).filter(Boolean);
}
