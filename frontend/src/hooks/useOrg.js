/**
 * useOrg.js - Convenience hook for accessing OrgContext
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * This hook provides access to organization workspace state and methods.
 * It's a convenience wrapper around OrgContext.
 * 
 * Usage:
 *   const { orgId, organization, permissions, isMember, loadWorkspaceOrg } = useOrg();
 */

import { useOrgContext } from '../contexts/OrgContext';

export function useOrg() {
  return useOrgContext();
}
