/**
 * eventAdapter.js - Event DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 */

export function toEventViewModel(dto) {
  if (!dto) return null;
  return dto;
}

export function toEventSummaryViewModel(dto) {
  if (!dto) return null;
  return dto;
}

export function toEventPublicViewModel(dto) {
  if (!dto) return null;
  return dto;
}

export function toEventListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toEventSummaryViewModel).filter(Boolean);
}
