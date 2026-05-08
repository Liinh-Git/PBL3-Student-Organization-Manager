/**
 * OrgContext.jsx - Organization workspace state management
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - orgId comes from query string (?orgId=), NOT path params
 * - Workspace context is different from public overview
 * - Load public overview first in OrgOverviewPage
 * - permissions/me can return 403 for non-member
 * - 403 on permissions/me must NOT break public overview
 * - Workspace routes require membership/permissions
 * - Permission fallback must NEVER grant org.workspace.access
 * - If permission parse fails, return [] (no permissions)
 */

import { createContext, useContext, useState, useCallback, useMemo } from 'react';
import { getOrganizationById } from '../services/organizationService.js';
import { getMyPermissions } from '../services/roleService.js';

const OrgContext = createContext(null);

export function OrgProvider({ children }) {
  const [orgId, setOrgId] = useState(null);
  const [organization, setOrganization] = useState(null);
  const [permissions, setPermissions] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isMember, setIsMember] = useState(false);
  const [error, setError] = useState(null);

  const loadWorkspaceOrg = useCallback(async (id) => {
    // Guard: Don't reload if already loading the same org
    if (isLoading && orgId === id) {
      return;
    }

    // Guard: Don't reload if org is already loaded with the same id
    if (orgId === id && organization && !error) {
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const org = await getOrganizationById(id);
      setOrganization(org);
      setOrgId(id);
      
      // Try to load permissions (may 403 if not member)
      try {
        const perms = await getMyPermissions(id);
        setPermissions(perms.permissionKeys || []);
        setIsMember(true);
      } catch (permError) {
        if (permError.response?.status === 403) {
          setPermissions([]);
          setIsMember(false);
        } else {
          throw permError;
        }
      }
    } catch (err) {
      setError(err);
    } finally {
      setIsLoading(false);
    }
  }, [orgId, organization, isLoading, error]);

  const loadPermissions = useCallback(async (id) => {
    try {
      const perms = await getMyPermissions(id);
      setPermissions(perms.permissionKeys || []);
      setIsMember(true);
    } catch (error) {
      if (error.response?.status === 403) {
        setPermissions([]);
        setIsMember(false);
      } else {
        throw error;
      }
    }
  }, []);

  const clearOrg = useCallback(() => {
    setOrgId(null);
    setOrganization(null);
    setPermissions([]);
    setIsMember(false);
    setError(null);
  }, []);

  const value = useMemo(() => ({
    orgId,
    organization,
    permissions,
    isLoading,
    isMember,
    error,
    loadWorkspaceOrg,
    loadPermissions,
    clearOrg,
  }), [orgId, organization, permissions, isLoading, isMember, error, loadWorkspaceOrg, loadPermissions, clearOrg]);

  return <OrgContext.Provider value={value}>{children}</OrgContext.Provider>;
}

export const useOrgContext = () => {
  const context = useContext(OrgContext);
  if (!context) {
    throw new Error('useOrgContext must be used within OrgProvider');
  }
  return context;
};
