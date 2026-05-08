/**
 * UserSettingsPage.jsx - User settings page
 * 
 * Phase 4B-2: Write UI integration
 */

import { useState, useEffect } from 'react';
import { getMe, updateMe, changePassword } from '../../services/userService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ErrorState from '../../components/shared/ErrorState';

function UserSettingsPage() {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

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

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    const form = e.target;
    const fullName = form.fullName.value;
    const phoneNumber = form.phoneNumber.value;
    const address = form.address.value;
    const bio = form.bio.value;
    
    setIsSubmitting(true);
    try {
      const updated = await updateMe({
        fullName: fullName || undefined,
        phoneNumber: phoneNumber || undefined,
        address: address || undefined,
        bio: bio || undefined
      });
      setUser(updated);
      alert('Profile updated successfully');
    } catch (err) {
      alert(err.message || 'Failed to update profile');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handlePasswordChange = async (e) => {
    e.preventDefault();
    const form = e.target;
    const currentPassword = form.currentPassword.value;
    const newPassword = form.newPassword.value;
    const confirmPassword = form.confirmPassword.value;
    
    if (!currentPassword || !newPassword) {
      alert('Current password and new password are required');
      return;
    }
    
    if (newPassword !== confirmPassword) {
      alert('New password and confirm password do not match');
      return;
    }

    setIsSubmitting(true);
    try {
      await changePassword({
        currentPassword,
        newPassword
      });
      alert('Password changed successfully');
      form.reset();
    } catch (err) {
      alert(err.message || 'Failed to change password');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Settings"
          description="Manage your account settings"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Settings"
          description="Manage your account settings"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="app-page">
      <PageHeader
        title="Settings"
        description="Manage your account settings"
      />

      <div className="app-section">
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Profile Information</h3>
          </div>
          <form onSubmit={handleProfileUpdate} className="auth-form">
            <div className="form-group">
              <label className="form-label">Full Name</label>
              <input
                type="text"
                id="fullName"
                name="fullName"
                defaultValue={user?.fullName || ''}
                placeholder="Full name"
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Phone Number</label>
              <input
                type="text"
                id="phoneNumber"
                name="phoneNumber"
                defaultValue={user?.phoneNumber || ''}
                placeholder="Phone number"
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Address</label>
              <input
                type="text"
                id="address"
                name="address"
                defaultValue={user?.address || ''}
                placeholder="Address"
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Bio</label>
              <textarea
                id="bio"
                name="bio"
                defaultValue={user?.bio || ''}
                placeholder="Bio"
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

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Change Password</h3>
          </div>
          <form onSubmit={handlePasswordChange} className="auth-form">
            <div className="form-group">
              <label className="form-label">Current Password</label>
              <input
                type="password"
                id="currentPassword"
                name="currentPassword"
                placeholder="Current password"
                required
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">New Password</label>
              <input
                type="password"
                id="newPassword"
                name="newPassword"
                placeholder="New password"
                required
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label className="form-label">Confirm New Password</label>
              <input
                type="password"
                id="confirmPassword"
                name="confirmPassword"
                placeholder="Confirm new password"
                required
                className="form-input"
              />
            </div>

            <div className="app-action-row">
              <button type="submit" disabled={isSubmitting} className="app-button app-button--primary">
                {isSubmitting ? 'Changing...' : 'Change Password'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default UserSettingsPage;
