/**
 * LoginPage.jsx - User login page
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuthContext } from '../../contexts/AuthContext.jsx';
import PageHeader from '../../components/shared/PageHeader';
import ErrorState from '../../components/shared/ErrorState';

function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuthContext();

  const [formData, setFormData] = useState({ email: '', password: '' });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [apiError, setApiError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    setApiError(null);
    try {
      await login(formData);
      navigate('/user/organizations');
    } catch (err) {
      setApiError(err.message || 'Login failed');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  return (
    <div className="auth-form-shell">
      <div className="auth-form-header">
        <h2>Welcome Back</h2>
        <p>Sign in to your account to continue</p>
      </div>

      {apiError && <div className="auth-alert auth-alert-error">{apiError}</div>}

      <form className="auth-form" onSubmit={handleSubmit}>
        <div className="auth-field-group">
          <label htmlFor="email" className="auth-label">Email</label>
          <input
            type="email"
            id="email"
            name="email"
            className="auth-input"
            placeholder="Enter your email"
            value={formData.email}
            onChange={handleChange}
            disabled={isSubmitting}
            required
          />
        </div>

        <div className="auth-field-group">
          <label htmlFor="password" className="auth-label">Password</label>
          <input
            type="password"
            id="password"
            name="password"
            className="auth-input"
            placeholder="Enter your password"
            value={formData.password}
            onChange={handleChange}
            disabled={isSubmitting}
            required
          />
        </div>

        <button type="submit" disabled={isSubmitting} className="auth-primary-btn">
          {isSubmitting ? 'Logging in...' : 'Sign In'}
        </button>
      </form>

      <div className="auth-footnote">
        Don't have an account? <Link to="/register">Register</Link>
      </div>
    </div>
  );
}

export default LoginPage;
