import React from 'react';
import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import Sidebar from './components/Sidebar';
import Header from './components/Header';
import ProtectedRoute from './components/ProtectedRoute';
import { getAccessToken } from './auth/tokenManager';

import Login from './pages/Login/Login';
import Register from './pages/Register/Register';
import Dashboard from './pages/Dashboard/Dashboard';
import ReportingDashboard from './pages/Dashboard/ReportingDashboard';
import Tickets from './pages/Tickets/Tickets';
import CreateTicket from './pages/Tickets/CreateTicket';
import TicketDetails from './pages/Tickets/TicketDetails';
import MyTickets from './pages/Tickets/MyTickets';
import DeptQueue from './pages/DeptQueue/DeptQueue';
import Employees from './pages/Employees/Employees';
import Departments from './pages/Departments/Departments';
import MasterConfig from './pages/Admin/MasterConfig/MasterConfig';
import SlaConfig from './pages/Admin/SlaConfig/SlaConfig';
import SupportPlans from './pages/Admin/SupportPlans/SupportPlans';
import EmailConfig from './pages/Admin/EmailConfig/EmailConfig';
import EmailInboxReview from './pages/Admin/EmailConfig/EmailInboxReview';
import WhatsAppConfig from './pages/Admin/WhatsAppConfig/WhatsAppConfig';
import WhatsAppInboxReview from './pages/Admin/WhatsAppConfig/WhatsAppInboxReview';
import Profile from './pages/Profile/Profile';
import Customers from './pages/Customers/Customers';
import NotFound from './pages/NotFound/NotFound';
import Leave from './pages/Leave/Leave';
import MyLeave from './pages/Leave/MyLeave';
import Holiday from './pages/Holiday/Holiday';
import MyHoliday from './pages/Holiday/MyHoliday';
import EmailNotificationPreferences from './pages/Admin/EmailNotificationPreferences/EmailNotificationPreferences';

export default function App() {
  const token = getAccessToken();
  const location = useLocation();
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  const isAuthPage = location.pathname === '/login' || location.pathname === '/register';
  const showShell = token && user && !isAuthPage;

  return (
    <div style={{ display: 'flex', minHeight: '100vh', background: 'var(--bg-dark)' }}>
      {showShell && <Sidebar />}

      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
        {showShell && <Header />}

        <main style={{ flex: 1, overflowY: 'auto' }}>
          <Routes>
            <Route path="/" element={<Navigate to={token ? (user?.role === 'Customer' ? '/portal/tickets' : '/dashboard') : '/login'} replace />} />
            <Route path="/login" element={token ? <Navigate to="/dashboard" replace /> : <Login />} />
            <Route path="/register" element={<Register />} />

            <Route path="/dashboard" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee', 'SupportAgent', 'DepartmentHead']}><Dashboard /></ProtectedRoute>} />
            <Route path="/reports" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><ReportingDashboard /></ProtectedRoute>} />

            <Route path="/tickets" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee', 'SupportAgent', 'DepartmentHead']}><Tickets /></ProtectedRoute>} />
            <Route path="/tickets/my-queue" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee', 'SupportAgent', 'DepartmentHead']}><Tickets /></ProtectedRoute>} />
            <Route path="/tickets/create" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee', 'SupportAgent', 'DepartmentHead']}><CreateTicket /></ProtectedRoute>} />
            <Route path="/tickets/department-queue" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee', 'SupportAgent', 'DepartmentHead']}><DeptQueue /></ProtectedRoute>} />
            <Route path="/tickets/:ticketId" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'Employee', 'SupportAgent', 'DepartmentHead']}><TicketDetails /></ProtectedRoute>} />

            {/* Portal Routes */}
            <Route path="/portal/tickets" element={<ProtectedRoute allow={['Customer']}><MyTickets /></ProtectedRoute>} />
            <Route path="/portal/tickets/create" element={<ProtectedRoute allow={['Customer']}><CreateTicket isPortal={true} /></ProtectedRoute>} />
            <Route path="/portal/ticket/:ticketId" element={<ProtectedRoute allow={['Customer']}><TicketDetails /></ProtectedRoute>} />

            <Route path="/employees" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><Employees /></ProtectedRoute>} />
            <Route
  path="/leave"
  element={
    <ProtectedRoute
      allow={[
        'Admin',
        'SuperAdmin',
        'Employee',
        'SupportAgent',
        'DepartmentHead'
      ]}
    >
      <Leave />
    </ProtectedRoute>
  }
/>

<Route
  path="/my-leave"
  element={
    <ProtectedRoute>
      <MyLeave />
    </ProtectedRoute>
  }
/>
<Route
  path="/holidays"
  element={
    <ProtectedRoute allow={['Admin', 'SuperAdmin']}>
      <Holiday />
    </ProtectedRoute>
  }
/>

<Route
  path="/my-holidays"
  element={
    <ProtectedRoute>
      <MyHoliday />
    </ProtectedRoute>
  }
/>


            <Route path="/departments" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><Departments /></ProtectedRoute>} />
            <Route path="/customers" element={<ProtectedRoute allow={['Admin', 'SuperAdmin', 'DepartmentHead', 'Employee']}><Customers /></ProtectedRoute>} />
            <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />

            {/* Admin Configuration Routes */}
            <Route path="/admin/ticket-configuration" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><MasterConfig /></ProtectedRoute>} />
            <Route path="/admin/master-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><MasterConfig /></ProtectedRoute>} />
            <Route path="/admin/sla-configuration" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SlaConfig /></ProtectedRoute>} />
            <Route path="/admin/sla-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SlaConfig /></ProtectedRoute>} />
            <Route path="/admin/support-plans" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SupportPlans /></ProtectedRoute>} />
            <Route path="/admin/support-plans/assign" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><SupportPlans /></ProtectedRoute>} />
            <Route path="/admin/email-ticket-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><EmailConfig /></ProtectedRoute>} />
            <Route path="/admin/email-inbox-review" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><EmailInboxReview /></ProtectedRoute>} />
            <Route path="/admin/whatsapp-config" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><WhatsAppConfig /></ProtectedRoute>} />
            <Route path="/admin/whatsapp-review" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><WhatsAppInboxReview /></ProtectedRoute>} />
            <Route path="/admin/email-notification-preferences" element={<ProtectedRoute allow={['Admin', 'SuperAdmin']}><EmailNotificationPreferences /></ProtectedRoute>} />
            
            <Route path="*" element={<NotFound />} />
          </Routes>
        </main>
      </div>
    </div>
  );
}
