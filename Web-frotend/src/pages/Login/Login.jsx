import { Link, useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import api from '../../api/axios';
import { setTokens } from '../../auth/tokenManager';
import { getFriendlyErrorMessage } from '../../utils/apiErrors';
import './Login.scss';
import { useState, useEffect } from "react";

const ROLE_CLAIM_URI = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

function Login() {
  const navigate = useNavigate();
  const [mode, setMode] = useState('employee'); // 'employee' | 'customer' — UI only, same endpoint either way
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const message = sessionStorage.getItem("authMessage");
    if (message) {
      setError(message);
      sessionStorage.removeItem("authMessage");
    }
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      // Single endpoint for both — backend checks Employee then Customer internally
      const response = await api.post('/auth/login', { email, password });
      const { accessToken, refreshToken } = response.data;
      setTokens(accessToken, refreshToken);

      const decoded = jwtDecode(accessToken);
      const userId = decoded.sub;
      const role = decoded.role || decoded[ROLE_CLAIM_URI];

      let user;
      if (role === 'Customer') {
        const { data } = await api.get(`/customers/${userId}`);
        const [firstName, ...rest] = (data.name || '').split(' ');
        user = {
          id: data.id,
          firstName: firstName || data.name || '',
          lastName: rest.join(' '),
          email: data.email,
          role: 'Customer',
          profilePicture: data.profilePicture,
        };
      } else {
        const { data } = await api.get(`/employees/${userId}`);
        user = {
          id: data.id,
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          role: data.role,
          departmentId: data.departmentId,
          departmentName: data.departmentName,
          profilePicture: data.profilePicture,
          isActive: data.isActive,
        };
      }

      // Gentle nudge if they picked the wrong tab — doesn't block login, just informs
      if (mode === 'customer' && role !== 'Customer') {
        setError("This account is an employee/admin account — you've been signed in and redirected accordingly.");
      } else if (mode === 'employee' && role === 'Customer') {
        setError("This account is a customer account — you've been signed in and redirected accordingly.");
      }

      localStorage.setItem('user', JSON.stringify(user));
      navigate(user.role === 'Customer' ? '/my-tickets' : '/dashboard');
    } catch (err) {
      setError(getFriendlyErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-brand">
          <div className="brand-icon">▥</div>
          <div>
            <h2>EMS</h2>
            <span>Management</span>
          </div>
        </div>

        <div className="login-toggle">
          <button type="button" className={mode === 'employee' ? 'active' : ''} onClick={() => setMode('employee')}>
            Employee / Admin
          </button>
          <button type="button" className={mode === 'customer' ? 'active' : ''} onClick={() => setMode('customer')}>
            Customer
          </button>
        </div>

        <div className="login-header">
          <h1>Welcome back</h1>
          <p>
            {mode === 'customer'
              ? 'Sign in to raise or track your support tickets.'
              : 'Sign in to manage your organization.'}
          </p>
        </div>

        {error && <div className="login-error">{error}</div>}

        <form className="login-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">Email Address</label>
            <input
              id="email"
              type="email"
              placeholder="Enter your email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              placeholder="Enter your password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" disabled={loading}>
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        {mode === 'customer' && (
          <p className="register-link">
            New here? <Link to="/register">Create an account</Link>
          </p>
        )}
      </div>
    </div>
  );
}

export default Login;