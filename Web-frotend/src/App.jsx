import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { getToken } from './auth/tokenManager';
import { getRoleLabel } from './auth/roles';

import Sidebar from './components/Sidebar';
import ProtectedRoute from './components/ProtectedRoute';
import NotificationBell from './components/NotificationBell';

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

import EmailConfig from './pages/Admin/EmailConfig/EmailConfig';
import EmailInboxReview from './pages/Admin/EmailConfig/EmailInboxReview';
import WhatsAppConfig from './pages/Admin/WhatsAppConfig/WhatsAppConfig';
import WhatsAppInboxReview from './pages/Admin/WhatsAppConfig/WhatsAppInboxReview';
import MasterConfig from './pages/Admin/MasterConfig/MasterConfig';
import SlaConfig from './pages/Admin/SlaConfig/SlaConfig';
import SupportPlans from './pages/Admin/SupportPlans/SupportPlans';
import DeptQueue from './pages/DeptQueue/DeptQueue';
import ReportingDashboard from './pages/Dashboard/ReportingDashboard';
import CreateTicket from './pages/Tickets/CreateTicket';

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
              <NotificationBell />
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
          <Route path="/tickets/create" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><CreateTicket /></ProtectedRoute>} />
          <Route path="/tickets/my-queue" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><Tickets /></ProtectedRoute>} />
          <Route path="/tickets/department-queue" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><DeptQueue /></ProtectedRoute>} />
          <Route path="/tickets/dashboard" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><ReportingDashboard /></ProtectedRoute>} />
          <Route path="/tickets/:ticketId" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><TicketDetails /></ProtectedRoute>} />

          {/* Portal Routes */}
          <Route path="/portal/tickets" element={<ProtectedRoute allow={['Customer']}><MyTickets /></ProtectedRoute>} />
          <Route path="/portal/tickets/create" element={<ProtectedRoute allow={['Customer']}><CreateTicket isPortal={true} /></ProtectedRoute>} />
          <Route path="/portal/ticket/:ticketId" element={<ProtectedRoute allow={['Customer']}><TicketDetails /></ProtectedRoute>} />

          <Route path="/my-tickets" element={<ProtectedRoute allow={['Customer']}><MyTickets /></ProtectedRoute>} />
          <Route path="/my-tickets/:ticketId" element={<ProtectedRoute allow={['Customer']}><TicketDetails /></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />

          <Route path="/department-queue" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee']}><DeptQueue /></ProtectedRoute>} />
          <Route path="/reports" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><ReportingDashboard /></ProtectedRoute>} />

          {/* Admin Routes */}
          <Route path="/admin/ticket-configuration" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><MasterConfig /></ProtectedRoute>} />
          <Route path="/admin/master-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><MasterConfig /></ProtectedRoute>} />
          <Route path="/admin/sla-configuration" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SlaConfig /></ProtectedRoute>} />
          <Route path="/admin/sla-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SlaConfig /></ProtectedRoute>} />
          <Route path="/admin/support-plans" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SupportPlans /></ProtectedRoute>} />
          <Route path="/admin/support-plans/assign" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SupportPlans /></ProtectedRoute>} />
          <Route path="/admin/email-ticket-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><EmailConfig /></ProtectedRoute>} />
          <Route path="/admin/email-inbox-review" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><EmailInboxReview /></ProtectedRoute>} />
          <Route path="/admin/whatsapp-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><WhatsAppConfig /></ProtectedRoute>} />
          <Route path="/admin/whatsapp-inbox-review" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><WhatsAppInboxReview /></ProtectedRoute>} />

          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;