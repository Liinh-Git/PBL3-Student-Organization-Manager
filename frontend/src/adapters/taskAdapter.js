/**
 * taskAdapter.js - Task DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 */

export function toTaskViewModel(dto) {
  if (!dto) return null;
  return dto;
}

export function toTaskListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toTaskViewModel).filter(Boolean);
}
