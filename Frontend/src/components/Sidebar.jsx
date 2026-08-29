import React, { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Ticket,
  UserCircle,
  LogOut,
  Building2,
  ChevronLeft,
  ChevronRight,
  Layers,
  Settings,
  BarChart3,
  Award,
  PlusCircle,
  Inbox,
  MessageSquare,
  CalendarOff,
  CalendarDays,
  Bell
} from 'lucide-react';
import { isAdmin, isCustomer, isEmployee } from '../auth/roles';
import { clearToken } from '../auth/tokenManager';
import './Sidebar.scss';

export default function Sidebar() {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  if (!user) return null;

  const admin = isAdmin(user);
  const customer = isCustomer(user);
  const employee = isEmployee(user);

  const handleLogout = () => {
    clearToken();
    localStorage.removeItem('user');
    navigate('/login');
  };

  return (
    <aside className={`app-sidebar ${isCollapsed ? 'collapsed' : ''}`}>
      <button
        className="collapse-toggle"
        onClick={() => setIsCollapsed(!isCollapsed)}
        title={isCollapsed ? 'Expand Sidebar' : 'Collapse Sidebar'}
      >
        {isCollapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
      </button>

      <div className="sidebar-brand">
        <div className="brand-logo">
          <Building2 size={24} />
        </div>
        {!isCollapsed && (
          <div className="brand-text">
            <span className="brand-title">SCIQUS AMS</span>
            <span className="brand-subtitle">Support System</span>
          </div>
        )}
      </div>

      <nav className="sidebar-nav">
        <NavLink
          to={customer ? '/portal/tickets' : '/dashboard'}
          className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
          title="Dashboard"
        >
          <LayoutDashboard size={20} />
          {!isCollapsed && <span>Dashboard</span>}
        </NavLink>

        {customer && (
          <>
            <NavLink
              to="/portal/tickets"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="My Support Tickets"
            >
              <Ticket size={20} />
              {!isCollapsed && <span>My Tickets</span>}
            </NavLink>
            <NavLink
              to="/portal/tickets/create"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Raise Ticket"
            >
              <PlusCircle size={20} />
              {!isCollapsed && <span>Raise Ticket</span>}
            </NavLink>
          </>
        )}

        {employee && !customer && (
          <>
            <NavLink
              to="/tickets"
              end
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Ticket Queue"
            >
              <Ticket size={20} />
              {!isCollapsed && <span>Ticket Queue</span>}
            </NavLink>
            <NavLink
              to="/tickets/create"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="New Ticket"
            >
              <PlusCircle size={20} />
              {!isCollapsed && <span>New Ticket</span>}
            </NavLink>
            <NavLink
            to="/my-leave"
            className={({ isActive }) =>
              isActive ? 'nav-link active' : 'nav-link'
            }
            title="My Leave"
          >
            
            <CalendarOff size={20} />
            {!isCollapsed && <span>My Leave</span>}
          </NavLink>

          <NavLink
            to="/my-holidays"
            className={({ isActive }) =>
              isActive ? 'nav-link active' : 'nav-link'
            }
            title="My Holidays"
          >
            <CalendarDays size={20} />
            {!isCollapsed && <span>My Holidays</span>}
          </NavLink>
            <NavLink
              to="/tickets/department-queue"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Dept Queue"
            >
              <Building2 size={20} />
              {!isCollapsed && <span>Dept Queue</span>}
            </NavLink>

            <NavLink
              to="/customers"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Customers CRM"
            >
              <Users size={20} />
              {!isCollapsed && <span>Customers CRM</span>}
            </NavLink>
          </>
        )}

        {admin && (
          <div className="nav-group">
            {!isCollapsed && <span className="group-label">ADMINISTRATION</span>}
            <NavLink
              to="/employees"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Employees"
            >
              <Users size={20} />
              {!isCollapsed && <span>Employees</span>}
            </NavLink>

            <NavLink
            to="/leave"
            className={({ isActive }) =>
              isActive ? 'nav-link active' : 'nav-link'
            }
            title="Leave Management"
          >
            <CalendarOff size={20} />
            {!isCollapsed && <span>Leave Management</span>}
          </NavLink>
                  <NavLink
          to="/holidays"
          className={({ isActive }) =>
            isActive ? 'nav-link active' : 'nav-link'
          }
          title="Holiday Management"
        >
          <CalendarDays size={20} />
          {!isCollapsed && <span>Holiday Management</span>}
        </NavLink>
            <NavLink
              to="/departments"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Departments"
            >
              <Building2 size={20} />
              {!isCollapsed && <span>Departments</span>}
            </NavLink>
            <NavLink
              to="/reports"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Analytics"
            >
              <BarChart3 size={20} />
              {!isCollapsed && <span>Analytics</span>}
            </NavLink>

            <NavLink
              to="/admin/master-config"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Master Setup"
            >
              <Layers size={20} />
              {!isCollapsed && <span>Master Setup</span>}
            </NavLink>
            <NavLink
              to="/admin/sla-config"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="SLA Rules"
            >
              <Settings size={20} />
              {!isCollapsed && <span>SLA Rules</span>}
            </NavLink>
            <NavLink
              to="/admin/support-plans"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Support Plans"
            >
              <Award size={20} />
              {!isCollapsed && <span>Support Plans</span>}
            </NavLink>

            <NavLink
              to="/admin/email-ticket-config"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="Email Channels"
            >
              <Inbox size={20} />
              {!isCollapsed && <span>Email Channels</span>}
            </NavLink>
            <NavLink
              to="/admin/whatsapp-config"
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              title="WhatsApp Channel"
            >
              <MessageSquare size={20} />
              {!isCollapsed && <span>WhatsApp Channel</span>}
            </NavLink>
            <NavLink
            to="/admin/email-notification-preferences"
            className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
            title="Notification Preferences"
          >
            <Bell size={20} />
            {!isCollapsed && <span>Notification Preferences</span>}
          </NavLink>
          </div>
        )}

        <NavLink
          to="/profile"
          className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
          title="My Profile"
        >
          <UserCircle size={20} />
          {!isCollapsed && <span>Profile</span>}
        </NavLink>
      </nav>

      <div className="sidebar-footer">
        <button className="logout-button" onClick={handleLogout} title="Logout">
          <LogOut size={18} />
          {!isCollapsed && <span>Sign Out</span>}
        </button>
      </div>
    </aside>
  );
}
