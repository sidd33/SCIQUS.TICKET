import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Users, Ticket as TicketIcon, AlertTriangle, ArrowRight, CheckCircle2 } from 'lucide-react';
import api from '../../api/axios';
import { isAdmin as checkAdmin } from '../../auth/roles';
import './Dashboard.scss';

const STATUS_LABEL = {
  Unassigned: "Unassigned",
  Assigned: "Assigned",
  InProgress: "In Progress",
  Completed: "Completed",
  Closed: "Closed",
  Reopened: "Reopened",
};

export default function Dashboard() {
  const [user] = useState(() => JSON.parse(localStorage.getItem('user') || 'null'));
  const [stats, setStats] = useState({ employees: 0, tickets: 0, breached: 0 });
  const [attentionTickets, setAttentionTickets] = useState([]);
  const [loading, setLoading] = useState(true);

  const admin = checkAdmin(user);

  useEffect(() => {
    async function load() {
      try {
        const ticketRes = await api.get('/tickets', { params: { pageSize: 100 } });
        // TicketService.GetAllTicketsAsync returns a raw array, not PagedResponse
        const tickets = ticketRes.data || [];

        let employeeCount = 0;
        if (admin) {
          const empRes = await api.get('/employees', { params: { pageSize: 1 } });
          employeeCount = empRes.data.totalCount;
        }

        const active = tickets.filter((t) => t.status !== 'Completed' && t.status !== 'Closed');
        const needsAttention = active
          .filter((t) => t.isSlaBreached)
          .slice(0, 5);

        setStats({
          employees: employeeCount,
          tickets: tickets.length,
          breached: tickets.filter((t) => t.isSlaBreached).length,
        });
        setAttentionTickets(needsAttention);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [admin]);

  if (!user) return null;

  return (
    <div className="dashboard-page">
      <header className="dashboard-header">
        <h1>Dashboard</h1>
        <p className="dashboard-subtitle">Welcome back, {user.firstName}.</p>
      </header>

      <div className="stat-grid">
        {admin && (
          <div className="stat-card">
            <div className="stat-icon stat-icon-blue"><Users size={22} /></div>
            <div className="stat-body">
              <span className="stat-label">Total Employees</span>
              <span className="stat-value">{loading ? '—' : stats.employees}</span>
              <span className="stat-helper">Registered across your organization</span>
            </div>
          </div>
        )}

        <div className="stat-card">
          <div className="stat-icon stat-icon-amber"><TicketIcon size={22} /></div>
          <div className="stat-body">
            <span className="stat-label">{admin ? 'Total Tickets' : 'My Queue'}</span>
            <span className="stat-value">{loading ? '—' : stats.tickets}</span>
            <span className="stat-helper">{admin ? 'Across the organization' : 'Assigned to you'}</span>
          </div>
        </div>

        <div className={`stat-card ${!loading && stats.breached > 0 ? 'stat-card-alert' : ''}`}>
          <div className="stat-icon stat-icon-red"><AlertTriangle size={22} /></div>
          <div className="stat-body">
            <span className="stat-label">SLA Breached</span>
            <span className="stat-value">{loading ? '—' : stats.breached}</span>
            <span className="stat-helper">{stats.breached > 0 ? 'Needs immediate attention' : 'All within deadline'}</span>
          </div>
        </div>
      </div>

      <div className="attention-section">
        <div className="attention-header">
          <div>
            <h2>Needs Attention</h2>
            <p>Tickets currently breaching their SLA.</p>
          </div>
          <Link to="/tickets" className="view-all-link">View all <ArrowRight size={16} /></Link>
        </div>

        {loading ? (
          <p className="loading-text">Loading...</p>
        ) : attentionTickets.length === 0 ? (
          <div className="empty-attention">
            <CheckCircle2 size={28} />
            <p>All caught up — no tickets are currently breaching SLA.</p>
          </div>
        ) : (
          <div className="attention-list">
            {attentionTickets.map((t) => (
              <Link to={`/tickets/${t.id}`} className="attention-row" key={t.id}>
                <div className="attention-row-main">
                  <span className="sla-dot breached" />
                  <div>
                    <strong>{t.title}</strong>
                    <span className="attention-meta">{t.customerName || 'Unknown customer'}</span>
                  </div>
                </div>
                <div className="attention-row-right">
                  <span className="status-badge">{STATUS_LABEL[t.status] || t.status}</span>
                  <span className="sla-pill breached">⚠ SLA Breached</span>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}