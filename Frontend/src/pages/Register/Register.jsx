import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Building2, User, Mail, Lock, Phone } from 'lucide-react';
import api from '../../api/axios';

export default function Register() {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [phone, setPhone] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await api.post('/auth/register-customer', { name, email, password, phoneNumber: phone });
      alert('Registration successful! Please log in.');
      navigate('/login');
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-screen">
      <div className="login-glass-box">
        <div className="brand-header">
          <div className="icon-badge"><Building2 size={28} /></div>
          <h1>Create Account</h1>
          <p>Customer Portal Registration</p>
        </div>

        {error && <div className="error-alert">{error}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="input-group">
            <label><User size={14} /> Full Name</label>
            <input required placeholder="Acme Corp" value={name} onChange={(e) => setName(e.target.value)} />
          </div>

          <div className="input-group">
            <label><Mail size={14} /> Email Address</label>
            <input required type="email" placeholder="customer@acme.com" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>

          <div className="input-group">
            <label><Phone size={14} /> Phone Number</label>
            <input placeholder="+1 (555) 000-0000" value={phone} onChange={(e) => setPhone(e.target.value)} />
          </div>

          <div className="input-group">
            <label><Lock size={14} /> Password</label>
            <input required type="password" placeholder="Min 8 characters" value={password} onChange={(e) => setPassword(e.target.value)} />
          </div>

          <button type="submit" className="submit-btn" disabled={loading}>
            {loading ? 'Creating Account...' : 'Register Customer Account'}
          </button>
        </form>

        <div style={{ marginTop: '1.25rem', textAlign: 'center', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
          Already registered? <Link to="/login" style={{ color: '#818cf8', fontWeight: 600 }}>Sign In</Link>
        </div>
      </div>
    </div>
  );
}
