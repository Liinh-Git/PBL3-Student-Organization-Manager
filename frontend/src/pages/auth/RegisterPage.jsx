/**
 * RegisterPage.jsx - User registration page
 * 
 * Phase 3C-5+: Full implementation with backend API integration
 * 
 * Implementation:
 * - Registration form (email, password, confirmPassword, firstName, lastName)
 * - Handle form submission with validation
 * - Call authService.register()
 * - Redirect to /login on success
 * - Display error message on failure
 * 
 * Service Usage:
 * - authService.register(payload)
 * 
 * Permissions:
 * - Public (no authentication required)
 * 
 * Route:
 * - /register
 * 
 * Form Fields:
 * - email (required, email format)
 * - password (required, min 8 chars)
 * - confirmPassword (required, must match password)
 * - firstName (required)
 * - lastName (required)
 * 
 * State Management:
 * - formData: useState for form data
 * - errors: useState for validation errors
 * - isSubmitting: useState for submission loading
 * - apiError: useState for API error
 * - useNavigate for redirect
 */

import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import ErrorState from '../../components/shared/ErrorState';
import { register } from '../../services/authService';

function RegisterPage() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: ''
  });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [apiError, setApiError] = useState(null);

  const validateForm = () => {
    const newErrors = {};

    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format';
    }

    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 8) {
      newErrors.password = 'Password must be at least 8 characters';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password';
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    if (!formData.firstName.trim()) {
      newErrors.firstName = 'First name is required';
    }

    if (!formData.lastName.trim()) {
      newErrors.lastName = 'Last name is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    // Clear error for this field when user starts typing
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setApiError(null);

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    try {
      // Backend expects fullName, so combine firstName + lastName
      const payload = {
        fullName: `${formData.firstName.trim()} ${formData.lastName.trim()}`,
        email: formData.email.trim(),
        password: formData.password,
        confirmPassword: formData.confirmPassword
      };

      await register(payload);
      // Redirect to /login on success
      navigate('/login');
    } catch (err) {
      // Extract error message from response if available
      const errorMessage = err.response?.data?.errors?.[0] || 
                          err.response?.data?.message || 
                          err.message || 
                          'Registration failed';
      setApiError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  const isFormValid = () => {
    return (
      formData.email.trim() &&
      formData.password &&
      formData.confirmPassword &&
      formData.firstName.trim() &&
      formData.lastName.trim() &&
      Object.values(errors).every(error => !error)
    );
  };

  return (
    <div className="register-page">
      <PageHeader
        title="Register"
        description="Create a new account"
      />

      <div className="register-form-container">
        {apiError && <ErrorState message={apiError} />}

        <form className="register-form" onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              placeholder="Enter your email"
              value={formData.email}
              onChange={handleChange}
            />
            {errors.email && <span className="error-message">{errors.email}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              placeholder="Enter your password"
              value={formData.password}
              onChange={handleChange}
            />
            {errors.password && <span className="error-message">{errors.password}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              placeholder="Confirm your password"
              value={formData.confirmPassword}
              onChange={handleChange}
            />
            {errors.confirmPassword && <span className="error-message">{errors.confirmPassword}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="firstName">First Name</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              placeholder="Enter your first name"
              value={formData.firstName}
              onChange={handleChange}
            />
            {errors.firstName && <span className="error-message">{errors.firstName}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="lastName">Last Name</label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              placeholder="Enter your last name"
              value={formData.lastName}
              onChange={handleChange}
            />
            {errors.lastName && <span className="error-message">{errors.lastName}</span>}
          </div>

          <button
            type="submit"
            disabled={isSubmitting || !isFormValid()}
            className="register-button"
          >
            {isSubmitting ? 'Registering...' : 'Register'}
          </button>
        </form>

        <div className="register-footer">
          <p>
            Already have an account? <Link to="/login">Login</Link>
          </p>
        </div>
      </div>
    </div>
  );
}

export default RegisterPage;
