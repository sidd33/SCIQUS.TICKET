import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import api from '../../api/axios';
import { setTokens } from '../../auth/tokenManager';
import { getFriendlyErrorMessage } from '../../utils/apiErrors';
import './Register.scss';

const EMPTY_FORM = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
};

export default function Register() {
  const navigate = useNavigate();

  const [form, setForm] = useState(EMPTY_FORM);
  const [fieldErrors, setFieldErrors] = useState({});
  const [formError, setFormError] = useState('');
  const [loading, setLoading] = useState(false);

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (fieldErrors[name]) {
      setFieldErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return next;
      });
    }
  }

  function validate() {
    const errors = {};
    if (!form.firstName.trim()) errors.firstName = 'First name is required.';
    if (!form.lastName.trim()) errors.lastName = 'Last name is required.';
    if (!form.email.trim()) {
      errors.email = 'Email is required.';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      errors.email = 'Enter a valid email address.';
    }
    if (!form.password) {
      errors.password = 'Password is required.';
    } else if (form.password.length < 6) {
      errors.password = 'Password must be at least 6 characters.';
    }
    if (form.confirmPassword !== form.password) {
      errors.confirmPassword = 'Passwords do not match.';
    }
    return errors;
  }

  function mapBackendErrors(errors) {
    const mapped = {};
    Object.entries(errors).forEach(([field, messages]) => {
      const key = field.charAt(0).toLowerCase() + field.slice(1);
      mapped[key] = Array.isArray(messages) ? messages[0] : messages;
    });
    return mapped;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setFormError('');
    setFieldErrors({});

    const clientErrors = validate();
    if (Object.keys(clientErrors).length > 0) {
      setFieldErrors(clientErrors);
      return;
    }

    try {
      setLoading(true);

      // RegisterRequestDto: Name (single field), Email, Password
      const { data } = await api.post('/auth/register', {
        name: `${form.firstName} ${form.lastName}`.trim(),
        email: form.email,
        password: form.password,
      });

      const { accessToken, refreshToken } = data;
      setTokens(accessToken, refreshToken);

      // Registration always creates a Customer
      const decoded = jwtDecode(accessToken);
      const user = {
        id: decoded.sub,
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        role: 'Customer',
      };
      localStorage.setItem('user', JSON.stringify(user));

      navigate('/my-tickets');
    } catch (err) {
      const backendErrors = err.response?.data?.errors;
      if (backendErrors) {
        setFieldErrors(mapBackendErrors(backendErrors));
      } else {
        setFormError(getFriendlyErrorMessage(err));
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="register-page">
      <div className="register-card">
        <div className="register-brand">
          <div className="brand-icon">▥</div>
          <div>
            <h2>EMS</h2>
            <span>Management</span>
          </div>
        </div>

        <div className="register-header">
          <h1>Create your account</h1>
          <p>Sign up to raise and track support tickets.</p>
        </div>

        {formError && <div className="register-error">{formError}</div>}

        <form className="register-form" onSubmit={handleSubmit} noValidate>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName">First Name</label>
              <input
                id="firstName"
                name="firstName"
                value={form.firstName}
                onChange={handleChange}
                placeholder="Enter your first name"
                className={fieldErrors.firstName ? 'input-error' : ''}
              />
              {fieldErrors.firstName && <span className="field-error-text">{fieldErrors.firstName}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="lastName">Last Name</label>
              <input
                id="lastName"
                name="lastName"
                value={form.lastName}
                onChange={handleChange}
                placeholder="Enter your last name"
                className={fieldErrors.lastName ? 'input-error' : ''}
              />
              {fieldErrors.lastName && <span className="field-error-text">{fieldErrors.lastName}</span>}
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="email">Email Address</label>
            <input
              id="email"
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              placeholder="Enter your email"
              className={fieldErrors.email ? 'input-error' : ''}
            />
            {fieldErrors.email && <span className="field-error-text">{fieldErrors.email}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              placeholder="Create a password"
              className={fieldErrors.password ? 'input-error' : ''}
            />
            {fieldErrors.password && <span className="field-error-text">{fieldErrors.password}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              id="confirmPassword"
              type="password"
              name="confirmPassword"
              value={form.confirmPassword}
              onChange={handleChange}
              placeholder="Re-enter your password"
              className={fieldErrors.confirmPassword ? 'input-error' : ''}
            />
            {fieldErrors.confirmPassword && <span className="field-error-text">{fieldErrors.confirmPassword}</span>}
          </div>

          <button type="submit" disabled={loading}>
            {loading ? 'Creating account...' : 'Create Account'}
          </button>
        </form>

        <p className="register-footer">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </div>
  );
}