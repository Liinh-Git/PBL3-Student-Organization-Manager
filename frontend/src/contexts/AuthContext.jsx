/**
 * AuthContext.jsx - Authentication state management
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - Token stored in localStorage with key: org.auth.accessToken
 * - Token expiry stored with key: org.auth.accessTokenExpiryUtc
 * - 401 should clear auth state
 * - 403 should NOT clear auth state (user is authenticated but not authorized)
 * - No mock data, no fake success
 */

import { createContext, useContext, useState, useEffect } from 'react';
import { login as authServiceLogin, getCurrentUser, logoutLocalOnly } from '../services/authService.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  // Initialize auth on mount
  useEffect(() => {
    initAuth();
  }, []);

  const initAuth = async () => {
    const storedToken = localStorage.getItem('org.auth.accessToken');
    if (!storedToken) {
      setIsLoading(false);
      return;
    }
    try {
      const currentUser = await getCurrentUser();
      setUser(currentUser);
      setToken(storedToken);
      setIsAuthenticated(true);
    } catch (error) {
      if (error.response?.status === 401) {
        localStorage.removeItem('org.auth.accessToken');
        localStorage.removeItem('org.auth.accessTokenExpiryUtc');
      }
      setIsAuthenticated(false);
    } finally {
      setIsLoading(false);
    }
  };

  const login = async (credentials) => {
    const response = await authServiceLogin(credentials);
    const { token, user } = response;
    setToken(token);
    setUser(user);
    setIsAuthenticated(true);
  };

  const logout = () => {
    logoutLocalOnly();
    setUser(null);
    setToken(null);
    setIsAuthenticated(false);
    window.location.href = '/login';
  };

  const value = {
    user,
    token,
    isAuthenticated,
    isLoading,
    initAuth,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuthContext = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuthContext must be used within AuthProvider');
  }
  return context;
};
