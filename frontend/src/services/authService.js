/**
 * authService.js - Authentication service
 * 
 * Phase 4B-1: Real backend API integration
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
 */

import httpClient from '../api/httpClient.js';

/**
 * Login user
 * 
 * Backend route: POST /api/auth/login
 * Frontend path: /auth/login
 * Input:
 * - credentials: { email: string, password: string }
 * Response:
 * - ApiResponse<AuthTokenResponse>
 * - AuthTokenResponse: { accessToken: string, tokenType: string, expiresAtUtc: string, user: AuthUserDto }
 * Permission:
 * - Public
 */
export async function login(credentials) {
  const response = await httpClient.post('/auth/login', credentials);
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Login failed');
  }
  
  const { accessToken, expiresAtUtc, user } = response.data.data;
  
  // Store token in localStorage
  localStorage.setItem('org.auth.accessToken', accessToken);
  localStorage.setItem('org.auth.accessTokenExpiryUtc', expiresAtUtc);
  
  return { token: accessToken, expiryUtc: expiresAtUtc, user };
}

/**
 * Register new user
 * 
 * Backend route: POST /api/auth/register
 * Frontend path: /auth/register
 * Input:
 * - payload: { fullName: string, email: string, password: string, confirmPassword?: string }
 * Response:
 * - ApiResponse<AuthTokenResponse>
 * - AuthTokenResponse: { accessToken: string, tokenType: string, expiresAtUtc: string, user: AuthUserDto }
 * Permission:
 * - Public
 */
export async function register(payload) {
  const response = await httpClient.post('/auth/register', payload);
  
  if (!response.data.success) {
    const errorMessage = response.data.errors?.[0] || response.data.message || 'Registration failed';
    throw new Error(errorMessage);
  }
  
  const { accessToken, expiresAtUtc, user } = response.data.data;
  
  // Store token in localStorage
  localStorage.setItem('org.auth.accessToken', accessToken);
  localStorage.setItem('org.auth.accessTokenExpiryUtc', expiresAtUtc);
  
  return { token: accessToken, expiryUtc: expiresAtUtc, user };
}

/**
 * Get current authenticated user
 * 
 * Backend route: GET /api/auth/me
 * Frontend path: /auth/me
 * Input:
 * - None (uses Bearer token from httpClient interceptor)
 * Response:
 * - ApiResponse<CurrentUserResponse>
 * - CurrentUserResponse: { user: AuthUserDto }
 * Permission:
 * - JWT token required
 */
export async function getCurrentUser() {
  const response = await httpClient.get('/auth/me');
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to get current user');
  }
  
  return response.data.data.user;
}

/**
 * Logout (client-side only)
 * 
 * TODO Phase implementation:
 * Backend route: None (client-side only unless backend endpoint exists)
 * Frontend path later: N/A
 * Input:
 * - None
 * Response:
 * - None
 * Rules:
 * - Clear localStorage (org.auth.accessToken, org.auth.accessTokenExpiryUtc)
 * - Clear AuthContext state
 * - Redirect to /login
 * - If backend logout endpoint exists later, call it before clearing state
 */
export function logoutLocalOnly() {
  // This can be implemented safely as it's client-side only
  localStorage.removeItem('org.auth.accessToken');
  localStorage.removeItem('org.auth.accessTokenExpiryUtc');
  // Caller should handle state clearing and redirect
}
