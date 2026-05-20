/**
 * usePermission.js - Permission checking hook
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Implement hasPermission(permissionKey) to check single permission
 * - Implement hasAnyPermission(keys) to check if user has any of the keys
 * - Implement hasAllPermissions(keys) to check if user has all keys
 * - Use permissions from OrgContext
 * - Safe behavior if permissions missing (return false)
 * - Fallback must NEVER grant org.workspace.access
 * 
 * IMPORTANT RULES:
 * - Permission fallback must return false for all checks
 * - Never grant org.workspace.access by default
 * - Permission keys must match canonical keys from SHARED_CONTRACT_CONSISTENCY_MATRIX.md
 * - roleService.getMyPermissions must normalize response to string[]
 */

import { useOrg } from './useOrg';

export function usePermission() {
  const { permissions } = useOrg();

  const hasPermission = (permissionKey) => {
    // TODO Phase 3C-4B/4C: Implement permission check
    // if (!permissions || !Array.isArray(permissions)) return false;
    // return permissions.includes(permissionKey);
    return false; // Safe fallback: deny by default
  };

  const hasAnyPermission = (permissionKeys) => {
    // TODO Phase 3C-4B/4C: Implement any permission check
    // if (!permissions || !Array.isArray(permissions)) return false;
    // return permissionKeys.some(key => permissions.includes(key));
    return false; // Safe fallback: deny by default
  };

  const hasAllPermissions = (permissionKeys) => {
    // TODO Phase 3C-4B/4C: Implement all permissions check
    // if (!permissions || !Array.isArray(permissions)) return false;
    // return permissionKeys.every(key => permissions.includes(key));
    return false; // Safe fallback: deny by default
  };

  return {
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
  };
}

