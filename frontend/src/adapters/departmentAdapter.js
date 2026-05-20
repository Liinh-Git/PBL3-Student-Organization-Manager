/**
 * departmentAdapter.js - Department DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - simple pass-through adapters
 */

export function toDepartmentViewModel(dto) {
  if (!dto) return null;
  return dto;
}

export function toDepartmentListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toDepartmentViewModel).filter(Boolean);
}
