/**
 * httpClient.js - Centralized HTTP client for API calls
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Add request interceptor to attach Bearer token from localStorage
 * - Add response interceptor to handle 401 (clear auth, redirect to login)
 * - Add response interceptor to handle 403 (do NOT globally redirect, handle at page level)
 * - Parse ApiResponse<T> wrapper from backend
 * - Extract data from response.data.data if using ApiResponse wrapper
 * 
 * IMPORTANT RULES:
 * - VITE_API_BASE_URL already includes /api
 * - Service paths must NOT include /api prefix
 * - 401 should clear auth and redirect to /login
 * - 403 should NOT globally redirect, render <ForbiddenState /> at page level
 * - No mock fallback, no fake data
 */

import axios from 'axios';

// API_BASE_URL already includes /api suffix
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const httpClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - attach Bearer token
httpClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('org.auth.accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Let browser set multipart boundary for FormData requests.
    if (typeof FormData !== 'undefined' && config.data instanceof FormData) {
      if (config.headers) {
        delete config.headers['Content-Type'];
        delete config.headers['content-type'];
      }
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle 401, extract data from ApiResponse<T>
httpClient.interceptors.response.use(
  (response) => {
    // Backend uses ApiResponse<T> wrapper: { success, data, message, errors }
    // Return the full response so services can check success and handle errors
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      // Clear auth and redirect to login
      localStorage.removeItem('org.auth.accessToken');
      localStorage.removeItem('org.auth.accessTokenExpiryUtc');
      window.location.href = '/login';
    }
    // Do NOT handle 403 globally - let pages handle it
    return Promise.reject(error);
  }
);

export default httpClient;
