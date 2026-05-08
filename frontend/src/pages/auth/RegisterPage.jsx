/**
 * RegisterPage.jsx - User registration page
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Create registration form (email, password, confirmPassword, firstName, lastName)
 * - Handle form submission
 * - Call authService.register()
 * - Redirect to /login on success
 * - Display error message on failure
 * 
 * Future Service Usage:
 * - authService.register(payload)
 * 
 * Permissions:
 * - Public (no authentication required)
 * 
 * Route:
 * - /register
 * 
 * Form Fields:
 * - email (required)
 * - password (required, min 8 chars)
 * - confirmPassword (required, must match password)
 * - firstName (required)
 * - lastName (required)
 * 
 * State Management:
 * - TODO: useState for form data
 * - TODO: useState for validation errors
 * - TODO: useState for submission loading
 * - TODO: useState for API error
 * - TODO: useNavigate for redirect
 * 
 * IMPORTANT:
 * - No real API calls in Phase 3C
 * - No fake registration success
 * - No hardcoded user data
 */

import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import ErrorState from '../../components/shared/ErrorState';

function RegisterPage() {
  const navigate = useNavigate();

  // TODO Phase 3C-5+: Add state management
  // const [formData, setFormData] = useState({
  //   email: '',
  //   password: '',
  //   confirmPassword: '',
  //   firstName: '',
  //   lastName: ''
  // });
  // const [errors, setErrors] = useState({});
  // const [isSubmitting, setIsSubmitting] = useState(false);
  // const [apiError, setApiError] = useState(null);

  // TODO Phase 3C-5+: Handle form submission
  // const handleSubmit = async (e) => {
  //   e.preventDefault();
  //   setIsSubmitting(true);
  //   setApiError(null);
  //   try {
  //     await authService.register(formData);
  //     // Redirect to /login on success
  //     navigate('/login');
  //   } catch (err) {
  //     setApiError(err.message);
  //   } finally {
  //     setIsSubmitting(false);
  //   }
  // };

  return (
    <div className="register-page">
      <PageHeader
        title="Register"
        description="Create a new account"
      />

      <div className="register-form-container">
        {/* TODO Phase 3C-5+: Show ErrorState when apiError */}

        <form className="register-form">
          {/* TODO Phase 3C-5+: Email input */}
          <div className="form-field">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              placeholder="Enter your email"
              disabled
            />
          </div>

          {/* TODO Phase 3C-5+: Password input */}
          <div className="form-field">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              placeholder="Enter your password"
              disabled
            />
          </div>

          {/* TODO Phase 3C-5+: Confirm password input */}
          <div className="form-field">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              placeholder="Confirm your password"
              disabled
            />
          </div>

          {/* TODO Phase 3C-5+: First name input */}
          <div className="form-field">
            <label htmlFor="firstName">First Name</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              placeholder="Enter your first name"
              disabled
            />
          </div>

          {/* TODO Phase 3C-5+: Last name input */}
          <div className="form-field">
            <label htmlFor="lastName">Last Name</label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              placeholder="Enter your last name"
              disabled
            />
          </div>

          {/* TODO Phase 3C-5+: Submit button */}
          <button type="submit" disabled className="register-button">
            Register (TODO Phase 3C-5+)
          </button>
        </form>

        {/* TODO Phase 3C-5+: Link to login */}
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
