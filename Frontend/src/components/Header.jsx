import React from 'react';
import NotificationBell from './NotificationBell';
import './Header.scss';

export default function Header() {
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  const getRoleLabel = () => {
    if (!user || !user.role) return 'User';
    const r = Array.isArray(user.role) ? user.role : [user.role];
    if (r.includes('Admin') || r.includes('SuperAdmin')) return 'System Administrator';
    if (r.includes('Customer')) return 'Customer Account';
    return 'Support Agent / Employee';
  };

  const initials = `${user?.firstName?.[0] || 'U'}${user?.lastName?.[0] || ''}`;

  return (
    <header className="app-header">
      <div className="header-left">
        <h2>SCIQUS Ticketing Platform</h2>
        <p>Welcome back, <strong>{user?.firstName || 'User'}</strong> 👋</p>
      </div>

      <div className="header-right">
        <NotificationBell />

        <div className="user-profile-badge">
          <div className="avatar-circle">{initials}</div>
          <div className="user-meta">
            <span className="user-name">{user?.firstName} {user?.lastName}</span>
            <span className="user-role">{getRoleLabel()}</span>
          </div>
        </div>
      </div>
    </header>
  );
}
