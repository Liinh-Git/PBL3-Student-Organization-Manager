/**
 * useAuth.js - Convenience hook for accessing AuthContext
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * This hook provides access to authentication state and methods.
 * It's a convenience wrapper around AuthContext.
 * 
 * Usage:
 *   const { user, isAuthenticated, login, logout } = useAuth();
 */

import { useAuthContext } from '../contexts/AuthContext';

export function useAuth() {
  return useAuthContext();
}
