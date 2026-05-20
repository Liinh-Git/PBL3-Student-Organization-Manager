/**
 * categoryAdapter.js - EventCategory DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 */

export function toCategoryViewModel(dto) {
  if (!dto) return null;
  // Ensure tasks array exists (initialize to empty array if absent)
  return {
    ...dto,
    tasks: dto.tasks || []
  };
}

export function toCategoryListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toCategoryViewModel).filter(Boolean);
}
