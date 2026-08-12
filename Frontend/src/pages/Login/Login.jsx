import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import { Building2, Lock, Mail, ArrowRight, ShieldCheck } from 'lucide-react';
import api from '../../api/axios';
import { setTokens } from '../../auth/tokenManager';
import './Login.scss';

const ROLE_CLAIM_URI = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

export default function Login() {
  const navigate = useNavigate();
  const [mode, setMode] = useState('employee');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const msg = sessionStorage.getItem('authMessage');
    if (msg) {
      setError(msg);
      sessionStorage.removeItem('authMessage');
    }
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await api.post('/auth/login', { email, password });
      const { accessToken, refreshToken } = response.data;
      setTokens(accessToken, refreshToken);

      const decoded = jwtDecode(accessToken);
      const userId = decoded.sub || decoded.nameid || decoded.id || decoded.name;
      const rawRole = decoded.role || decoded[ROLE_CLAIM_URI] || 'Admin';
      const role = Array.isArray(rawRole) ? rawRole[0] : rawRole;

      let user;
      try {
        if (role === 'Customer') {
          const { data } = await api.get(`/customers/${userId}`);
          const [firstName, ...rest] = (data.name || '').split(' ');
          user = {
            id: data.id || userId,
            firstName: firstName || 'Customer',
            lastName: rest.join(' '),
            email: data.email || email,
            role: 'Customer',
            profilePicture: data.profilePicture,
          };
        } else {
          const { data } = await api.get(`/employees/${userId}`);
          user = {
            id: data.id || userId,
            firstName: data.firstName || data.name || 'Super',
            lastName: data.lastName || 'Admin',
            email: data.email || email,
            role: role || 'Admin',
            departmentId: data.departmentId,
            departmentName: data.departmentName,
            profilePicture: data.profilePicture,
            isActive: data.isActive ?? true,
          };
        }
      } catch {
        user = {
          id: userId,
          firstName: 'Super',
          lastName: 'Admin',
          email: email,
          role: role || 'Admin'
        };
      }

      localStorage.setItem('user', JSON.stringify(user));
      navigate(user.role === 'Customer' ? '/portal/tickets' : '/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid email or password. Please check your credentials.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-screen">
      <div className="login-glass-box">
        <div className="brand-header">
          <div className="icon-badge">
            <Building2 size={28} />
          </div>
          <h1>SCIQUS AMS</h1>
          <p>Enterprise Asset & Support Management System</p>
        </div>

        <div className="mode-tabs">
          <button
            type="button"
            className={`tab-btn ${mode === 'employee' ? 'active' : ''}`}
            onClick={() => setMode('employee')}
          >
            <ShieldCheck size={16} /> Staff & Admin
          </button>
          <button
            type="button"
            className={`tab-btn ${mode === 'customer' ? 'active' : ''}`}
            onClick={() => setMode('customer')}
          >
            Customer Portal
          </button>
        </div>

        {error && <div className="error-alert">{error}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="input-group">
            <label><Mail size={14} /> Email Address</label>
            <input
              type="email"
              placeholder="e.g. admin@sciqustickets.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="input-group">
            <label><Lock size={14} /> Password</label>
            <input
              type="password"
              placeholder="Enter your password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="submit-btn" disabled={loading}>
            {loading ? 'Authenticating...' : <>Sign In to Platform <ArrowRight size={16} /></>}
          </button>
        </form>

        <div className="quick-credentials">
          <small>Default Admin Credentials for Demo:</small>
          <code>admin@sciqustickets.com / Admin@123</code>
        </div>
      </div>
    </div>
  );
}
