/**
 * milestoneAdapter.js - Milestone DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 */

export function toMilestoneViewModel(dto) {
  if (!dto) return null;
  return dto;
}

export function toMilestoneListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toMilestoneViewModel).filter(Boolean);
}
