import React from 'react';
import NotificationBell from './NotificationBell';
import './Header.scss';

export default function Header() {
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  const getRoleLabel = () => {
    if (!user) return 'User';
    const r = Array.isArray(user?.role) ? user.role : [user?.role || ''];
    if (r.includes('SuperAdmin')) return 'Super Administrator';
    if (r.includes('Admin')) return 'Administrator';
    if (user?.isDepartmentHead || r.includes('DepartmentHead')) return 'Department Head';
    if (r.includes('Customer')) return 'Customer Account';
    if (r.includes('Employee') || r.includes('SupportAgent')) return 'Support Agent';
    return 'User';
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
