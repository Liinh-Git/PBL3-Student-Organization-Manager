/**
 * memberAdapter.js - Member DTO to ViewModel adapters
 * 
 * Phase 4B-1: Real backend integration - normalize shape for UI pages
 */

function firstNonEmpty(...values) {
  for (const value of values) {
    if (value !== undefined && value !== null && value !== '') return value;
  }
  return null;
}

export function toMemberViewModel(dto) {
  if (!dto) return null;

  const fullName = firstNonEmpty(dto.fullName, dto.user?.fullName, dto.userName, dto.name);
  const email = firstNonEmpty(dto.email, dto.user?.email);
  const roleId = firstNonEmpty(dto.roleId, dto.role?.id);
  const roleName = firstNonEmpty(dto.roleName, dto.role?.roleName, dto.role?.name);
  const departmentId = firstNonEmpty(dto.departmentId, dto.department?.id);
  const departmentName = firstNonEmpty(dto.departmentName, dto.department?.departmentName, dto.department?.deptName);

  return {
    ...dto,
    fullName: fullName || '',
    email: email || '',
    roleId: roleId || null,
    departmentId: departmentId || null,
    user: {
      ...(dto.user || {}),
      fullName: fullName || '',
      email: email || ''
    },
    role: {
      ...(dto.role || {}),
      id: roleId || null,
      roleName: roleName || ''
    },
    department: {
      ...(dto.department || {}),
      id: departmentId || null,
      departmentName: departmentName || '',
      deptName: departmentName || ''
    }
  };
}

export function toMemberListViewModel(items) {
  if (!Array.isArray(items)) return [];
  return items.map(toMemberViewModel).filter(Boolean);
}
