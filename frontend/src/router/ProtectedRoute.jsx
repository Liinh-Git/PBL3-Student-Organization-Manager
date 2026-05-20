/**
 * ProtectedRoute.jsx - Route guard for authenticated users
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Check AuthContext for isAuthenticated
 * - If not authenticated, redirect to /login
 * - If authenticated, render <Outlet /> for nested routes
 * - Show loading spinner while checking auth
 * 
 * Usage:
 *   <Route element={<ProtectedRoute />}>
 *     <Route path="/user/profile" element={<UserProfilePage />} />
 *   </Route>
 */

import { Navigate, Outlet } from 'react-router-dom';
import { useAuthContext } from '../contexts/AuthContext';
import LoadingSpinner from '../components/shared/LoadingSpinner';

function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuthContext();

  if (isLoading) {
    return <LoadingSpinner />;
  }
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}

export default ProtectedRoute;

