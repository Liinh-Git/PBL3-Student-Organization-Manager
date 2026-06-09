/**
 * RegisterPage.jsx - User registration page
 * Phase 3C-5+: Full implementation with backend API integration
 */

import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { register } from "../../services/authService";
import "./auth.css"; // Import file CSS thuần

function RegisterPage() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    email: "",
    password: "",
    confirmPassword: "",
    firstName: "",
    lastName: "",
  });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [apiError, setApiError] = useState(null);

  const validateForm = () => {
    const newErrors = {};

    if (!formData.email.trim()) {
      newErrors.email = "Email không được để trống";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = "Định dạng email không hợp lệ";
    }

    if (!formData.password) {
      newErrors.password = "Mật khẩu không được để trống";
    } else if (formData.password.length < 8) {
      newErrors.password = "Mật khẩu phải có ít nhất 8 ký tự";
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = "Vui lòng xác nhận mật khẩu";
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = "Mật khẩu không khớp";
    }

    if (!formData.firstName.trim()) {
      newErrors.firstName = "Tên không được để trống";
    }

    if (!formData.lastName.trim()) {
      newErrors.lastName = "Họ không được để trống";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: "" }));
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
      const payload = {
        fullName: `${formData.firstName.trim()} ${formData.lastName.trim()}`,
        email: formData.email.trim(),
        password: formData.password,
        confirmPassword: formData.confirmPassword,
      };

      await register(payload);
      navigate("/login");
    } catch (err) {
      setApiError(err.message || "Đăng ký thất bại");
    } finally {
      setIsSubmitting(false);
    }
  };
  // đã có hàm validateForm để kiểm tra tính hợp lệ của form trước khi submit, và hàm isFormValid để kiểm tra xem form có thể submit hay không (dựa trên lỗi và dữ liệu đã nhập).
  const isFormValid = () => {
    return (
      formData.email.trim() &&
      formData.password &&
      formData.confirmPassword &&
      formData.firstName.trim() &&
      formData.lastName.trim() &&
      Object.values(errors).every((error) => !error)
    );
  };

  return (
    <div className="auth-layout">
      {/* Top Navigation */}
      <div className="auth-nav">
        <Link to="/login" className="auth-nav-link">
          Đăng nhập
        </Link>
        <span
          className="auth-nav-btn"
          style={{ cursor: "default", opacity: 0.9 }}
        >
          Đăng ký
        </span>
      </div>

      {/* Main Content */}
      <div className="auth-main">
        <div className="auth-card">
          <div className="auth-header">
            <h2 className="auth-title">Đăng ký</h2>
            <p className="auth-subtitle">
              Vui lòng điền thông tin để tạo tài khoản mới
            </p>
          </div>

          {apiError && <div className="auth-alert-error">{apiError}</div>}

          <form onSubmit={handleSubmit} className="auth-form">
            <div className="auth-row">
              <div className="auth-field">
                <label htmlFor="lastName" className="auth-label">
                  Họ
                </label>
                <input
                  type="text"
                  id="lastName"
                  name="lastName"
                  className={`auth-input ${errors.lastName ? "error" : ""}`}
                  placeholder="Nguyễn Văn"
                  value={formData.lastName}
                  onChange={handleChange}
                  style={{ paddingLeft: "1rem" }}
                />
                {errors.lastName && (
                  <p className="auth-field-error">{errors.lastName}</p>
                )}
              </div>

              <div className="auth-field">
                <label htmlFor="firstName" className="auth-label">
                  Tên
                </label>
                <input
                  type="text"
                  id="firstName"
                  name="firstName"
                  className={`auth-input ${errors.firstName ? "error" : ""}`}
                  placeholder="A"
                  value={formData.firstName}
                  onChange={handleChange}
                  style={{ paddingLeft: "1rem" }}
                />
                {errors.firstName && (
                  <p className="auth-field-error">{errors.firstName}</p>
                )}
              </div>
            </div>

            <div className="auth-field">
              <label htmlFor="email" className="auth-label">
                Email
              </label>
              <div className="auth-input-wrapper">
                <svg
                  className="auth-icon"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"
                  />
                </svg>
                <input
                  type="email"
                  id="email"
                  name="email"
                  className={`auth-input ${errors.email ? "error" : ""}`}
                  placeholder="Email đăng nhập"
                  value={formData.email}
                  onChange={handleChange}
                />
              </div>
              {errors.email && (
                <p className="auth-field-error">{errors.email}</p>
              )}
            </div>

            <div className="auth-field">
              <label htmlFor="password" className="auth-label">
                Mật khẩu
              </label>
              <div className="auth-input-wrapper">
                <svg
                  className="auth-icon"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
                  />
                </svg>
                <input
                  type="password"
                  id="password"
                  name="password"
                  className={`auth-input ${errors.password ? "error" : ""}`}
                  placeholder="Ít nhất 8 ký tự"
                  value={formData.password}
                  onChange={handleChange}
                />
              </div>
              {errors.password && (
                <p className="auth-field-error">{errors.password}</p>
              )}
            </div>

            <div className="auth-field">
              <label htmlFor="confirmPassword" className="auth-label">
                Xác nhận mật khẩu
              </label>
              <div className="auth-input-wrapper">
                <svg
                  className="auth-icon"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
                  />
                </svg>
                <input
                  type="password"
                  id="confirmPassword"
                  name="confirmPassword"
                  className={`auth-input ${errors.confirmPassword ? "error" : ""}`}
                  placeholder="Nhập lại mật khẩu"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                />
              </div>
              {errors.confirmPassword && (
                <p className="auth-field-error">{errors.confirmPassword}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={isSubmitting || !isFormValid()}
              className="auth-btn"
            >
              {isSubmitting ? "Đang xử lý..." : "Đăng ký"}
            </button>
          </form>

          <div className="auth-footer">
            Đã có tài khoản?{" "}
            <Link to="/login" className="auth-link">
              Đăng nhập ngay
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}

export default RegisterPage;
