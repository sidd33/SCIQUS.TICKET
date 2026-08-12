import React from 'react';
import { Navigate } from 'react-router-dom';
import { getAccessToken } from '../auth/tokenManager';

export default function ProtectedRoute({ children, allow = [] }) {
  const token = getAccessToken();
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  if (!token || !user) {
    return <Navigate to="/login" replace />;
  }

  if (allow.length > 0) {
    const userRole = Array.isArray(user.role) ? user.role : [user.role || 'User'];
    const hasRole = allow.some(role => userRole.includes(role) || (role === 'Admin' && userRole.includes('SuperAdmin')));
    if (!hasRole) {
      return <Navigate to={userRole.includes('Customer') ? '/portal/tickets' : '/dashboard'} replace />;
    }
  }

  return children;
}
