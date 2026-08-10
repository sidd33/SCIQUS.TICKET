import { Navigate } from 'react-router-dom';
import { getToken } from '../auth/tokenManager';
import { getRoleLabel } from '../auth/roles';

export default function ProtectedRoute({ children, allow }) {
  const token = getToken();
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (allow && user) {
    // Reuse the same role-labeling logic as everywhere else in the app,
    // instead of a second copy that didn't know about SuperAdmin (role 3).
    const role = getRoleLabel(user);

    if (!allow.includes(role)) {
      const fallback = role === 'Customer' ? '/my-tickets' : '/dashboard';
      return <Navigate to={fallback} replace />;
    }
  }

  return children;
}