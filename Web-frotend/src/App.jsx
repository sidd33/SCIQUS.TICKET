import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { getToken } from './auth/tokenManager';
import { getRoleLabel } from './auth/roles';

import Sidebar from './components/Sidebar';
import ProtectedRoute from './components/ProtectedRoute';

import Login from './pages/Login/Login';
import Register from './pages/Register/Register';
import Dashboard from './pages/Dashboard/Dashboard';
import Employees from './pages/Employees/Employees';
import Departments from './pages/Departments/Departments';
import Tickets from './pages/Tickets/Tickets';
import TicketDetails from './pages/TicketDetails/TicketDetails';
import MyTickets from './pages/MyTickets/MyTickets';
import Profile from './pages/Profile/Profile';
import NotFound from './pages/NotFound/NotFound';

function App() {
  const token = getToken();
  const user = JSON.parse(localStorage.getItem('user') || 'null');
  const location = useLocation();

  const isAuthRoute = location.pathname === '/login';
  const showShell = token && !isAuthRoute;

  return (
    <div className={showShell ? 'app-shell' : ''}>
      {showShell && <Sidebar />}

      <main className="app-content">
        {showShell && (
          <header className="top-navbar">
            <div className="top-navbar-left">
              <h2>Employee Management System</h2>
              <p>Welcome back, {user?.firstName} 👋</p>
            </div>
            <div className="top-navbar-right">
              <div className="top-user">
                <div className="top-avatar">
                  {`${user?.firstName?.[0] ?? ''}${user?.lastName?.[0] ?? ''}`}
                </div>
                <div className="top-user-info">
                  <span>{user?.firstName} {user?.lastName}</span>
                  <small>{getRoleLabel(user)}</small>
                </div>
              </div>
            </div>
          </header>
        )}

        <Routes>
          <Route path="/" element={<Navigate to={token ? (user?.role === 'Customer' ? '/my-tickets' : '/dashboard') : '/login'} replace />} />
          <Route path="/login" element={token ? <Navigate to="/dashboard" replace /> : <Login />} />
          <Route path="/register" element={<Register />} />

          <Route path="/dashboard" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><Dashboard /></ProtectedRoute>} />
          <Route path="/employees" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><Employees /></ProtectedRoute>} />
          <Route path="/departments" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><Departments /></ProtectedRoute>} />
          <Route path="/tickets" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><Tickets /></ProtectedRoute>} />
          <Route path="/tickets/:ticketId" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><TicketDetails /></ProtectedRoute>} />
          <Route path="/my-tickets" element={<ProtectedRoute allow={['Customer']}><MyTickets /></ProtectedRoute>} />
          <Route path="/my-tickets/:ticketId" element={<ProtectedRoute allow={['Customer']}><TicketDetails /></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />

          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;