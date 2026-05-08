/**
 * UserProfilePage.jsx - User profile page
 * 
 * Phase 4B-1B: Safe read-only page completion
 * 
 * Connected to:
 * - userService.getMe()
 * 
 * Permissions:
 * - JWT (authenticated user)
 * 
 * Route:
 * - /user/profile
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMe } from '../../services/userService.js';
import { useAuthContext } from '../../contexts/AuthContext.jsx';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ErrorState from '../../components/shared/ErrorState';

function UserProfilePage() {
  const navigate = useNavigate();
  const { user: authUser } = useAuthContext();

  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function loadProfile() {
      setIsLoading(true);
      try {
        const data = await getMe();
        setUser(data);
      } catch (err) {
        setError(err.message || 'Failed to load profile');
      } finally {
        setIsLoading(false);
      }
    }
    loadProfile();
  }, []);

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="My Profile"
          description="View your profile information"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="My Profile"
          description="View your profile information"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="My Profile"
        description="View your profile information"
        actions={
          <button disabled onClick={() => navigate('/user/settings')} className="app-button app-button--primary">
            Edit Profile (Write UI Pending)
          </button>
        }
      />

      <div className="app-section">
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Profile Information</h3>
          </div>
          <table>
            <tbody>
              <tr>
                <th>Email</th>
                <td>{user?.email || authUser?.email || '-'}</td>
              </tr>
              <tr>
                <th>Full Name</th>
                <td>{user?.fullName || authUser?.fullName || '-'}</td>
              </tr>
              <tr>
                <th>Phone Number</th>
                <td>{user?.phoneNumber || '-'}</td>
              </tr>
              <tr>
                <th>Status</th>
                <td>{user?.status || '-'}</td>
              </tr>
              <tr>
                <th>Member Since</th>
                <td>{user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : '-'}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default UserProfilePage;
