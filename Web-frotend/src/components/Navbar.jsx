import { NavLink, useNavigate } from 'react-router-dom';
import './Navbar.scss';

function Navbar() {
  const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    navigate('/login');
  };

  if (!user) return null;

  return (
    <nav className="navbar">
      <div className="navbar-brand">EMS</div>

      <div className="navbar-links">
        <NavLink to="/dashboard" className={({ isActive }) => (isActive ? 'active' : '')}>
          Dashboard
        </NavLink>
        <NavLink to="/employees" className={({ isActive }) => (isActive ? 'active' : '')}>
          {user.role === 2 ? 'Employees' : 'My Profile'}
        </NavLink>
        <NavLink to="/tickets" className={({ isActive }) => (isActive ? 'active' : '')}>
          {user.role === 2 ? 'Tickets' : 'Create Ticket'}
        </NavLink>
      </div>

      <button className="navbar-logout" onClick={handleLogout}>
        Logout
      </button>
    </nav>
  );
}

export default Navbar;