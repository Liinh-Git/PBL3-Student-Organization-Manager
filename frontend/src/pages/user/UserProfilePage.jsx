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
import { getMe, updateMe } from '../../services/userService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ErrorState from '../../components/shared/ErrorState';

function UserProfilePage() {
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phoneNumber: '',
    address: '',
    bio: ''
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    async function loadProfile() {
      setIsLoading(true);
      try {
        const data = await getMe();
        setFormData({
          fullName: data?.fullName || '',
          email: data?.email || '',
          phoneNumber: data?.phoneNumber || '',
          address: data?.address || '',
          bio: data?.bio || ''
        });
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

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      const updated = await updateMe({
        fullName: formData.fullName || undefined,
        phoneNumber: formData.phoneNumber || undefined,
        address: formData.address || undefined,
        bio: formData.bio || undefined
      });
      setFormData(prev => ({
        ...prev,
        fullName: updated?.fullName || prev.fullName,
        email: updated?.email || prev.email,
        phoneNumber: updated?.phoneNumber || prev.phoneNumber,
        address: updated?.address || prev.address,
        bio: updated?.bio || prev.bio
      }));
      alert('Profile updated successfully');
    } catch (err) {
      alert(err.message || 'Failed to update profile');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="app-page">
      <PageHeader
        title="My Profile"
        description="Manage your profile information"
      />

      <div className="app-section">
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Profile Information</h3>
          </div>
          <form onSubmit={handleProfileUpdate} className="auth-form">
            <div className="form-group">
              <label className="form-label">Name</label>
              <input
                type="text"
                id="fullName"
                name="fullName"
                value={formData.fullName}
                placeholder="Full name"
                onChange={handleChange}
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Email</label>
              <input
                type="email"
                id="email"
                name="email"
                value={formData.email}
                placeholder="Email"
                disabled
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Phone</label>
              <input
                type="text"
                id="phoneNumber"
                name="phoneNumber"
                value={formData.phoneNumber}
                placeholder="Phone number"
                onChange={handleChange}
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Address</label>
              <input
                type="text"
                id="address"
                name="address"
                value={formData.address}
                placeholder="Address"
                onChange={handleChange}
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Bio</label>
              <textarea
                id="bio"
                name="bio"
                value={formData.bio}
                placeholder="Bio"
                onChange={handleChange}
                className="form-textarea"
              />
            </div>

            <div className="app-action-row">
              <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                {isSubmitting ? 'Updating...' : 'Update Profile'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default UserProfilePage;
