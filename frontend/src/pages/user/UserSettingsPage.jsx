/**
 * UserSettingsPage.jsx - User settings page
 * 
 * Phase 4B-2: Write UI integration
 */

import { useState } from 'react';
import { changePassword } from '../../services/userService.js';
import PageHeader from '../../components/shared/PageHeader';

function UserSettingsPage() {
  const [isSubmitting, setIsSubmitting] = useState(false);

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

  return (
    <div className="app-page">
      <PageHeader
        title="Settings"
        description="Change your password"
      />

      <div className="app-section">
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
