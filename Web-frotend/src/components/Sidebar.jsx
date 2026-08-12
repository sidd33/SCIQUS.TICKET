import { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Ticket,
  UserCircle,
  LogOut,
  Building2,
  Building,
  ChevronLeft,
  ChevronRight,
  Layers,
  Settings,
  BarChart3
} from 'lucide-react';
import { isAdmin, isCustomer } from '../auth/roles';
import { clearToken } from '../auth/tokenManager';
import './Sidebar.scss';

function Sidebar() {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  if (!user) return null;

  const admin = isAdmin(user);
  const customer = isCustomer(user);

  const handleLogout = () => {
    clearToken();
    localStorage.removeItem('user');
    navigate('/login');
  };

  return (
    <aside className={`sidebar ${isCollapsed ? 'collapsed' : ''}`}>
      {/* Floating Toggle Button on Border Line */}
      <button
        className="toggle-btn"
        onClick={() => setIsCollapsed(!isCollapsed)}
        title={isCollapsed ? 'Expand Sidebar' : 'Collapse Sidebar'}
      >
        {isCollapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
      </button>

      <div className="sidebar-top">
        <div className="logo">
          <div className="logo-icon"><Building2 size={22} /></div>
        </div>

        <nav>
          <NavLink
            to={customer ? '/my-tickets' : '/dashboard'}
            className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
            title="Dashboard"
          >
            <LayoutDashboard size={21} />
            {!isCollapsed && <span className="nav-text">Dashboard</span>}
          </NavLink>

          {admin && (
            <NavLink
              to="/employees"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="Employees"
            >
              <Users size={21} />
              {!isCollapsed && <span className="nav-text">Employees</span>}
            </NavLink>
          )}

          {admin && (
            <NavLink
              to="/departments"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="Departments"
            >
              <Building size={21} />
              {!isCollapsed && <span className="nav-text">Departments</span>}
            </NavLink>
          )}

          {!customer && (
            <NavLink
              to="/tickets"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title={admin ? 'Tickets' : 'My Queue'}
            >
              <Ticket size={21} />
              {!isCollapsed && (
                <span className="nav-text">{admin ? 'Tickets' : 'My Queue'}</span>
              )}
            </NavLink>
          )}

          {!customer && (
            <NavLink
              to="/department-queue"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="Dept Queue"
            >
              <Building2 size={21} />
              {!isCollapsed && <span className="nav-text">Dept Queue</span>}
            </NavLink>
          )}

          {admin && (
            <NavLink
              to="/reports"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="Analytics"
            >
              <BarChart3 size={21} />
              {!isCollapsed && <span className="nav-text">Analytics</span>}
            </NavLink>
          )}

          {admin && (
            <NavLink
              to="/admin/master-config"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="Master Setup"
            >
              <Layers size={21} />
              {!isCollapsed && <span className="nav-text">Master Setup</span>}
            </NavLink>
          )}

          {admin && (
            <NavLink
              to="/admin/support-plans"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="Support Plans"
            >
              <Settings size={21} />
              {!isCollapsed && <span className="nav-text">Support Plans</span>}
            </NavLink>
          )}

          {admin && (
            <NavLink
              to="/admin/sla-config"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="SLA Rules"
            >
              <Settings size={21} />
              {!isCollapsed && <span className="nav-text">SLA Rules</span>}
            </NavLink>
          )}

          {customer && (
            <NavLink
              to="/my-tickets"
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
              title="My Tickets"
            >
              <Ticket size={21} />
              {!isCollapsed && <span className="nav-text">My Tickets</span>}
            </NavLink>
          )}

          <NavLink
            to="/profile"
            className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
            title="Profile"
          >
            <UserCircle size={21} />
            {!isCollapsed && <span className="nav-text">Profile</span>}
          </NavLink>
        </nav>
      </div>

      <div className="sidebar-bottom">
        <button className="logout-btn" onClick={handleLogout} title="Logout">
          <LogOut size={20} />
          {!isCollapsed && <span className="logout-text">Logout</span>}
        </button>
      </div>
    </aside>
  );
}

export default Sidebar;