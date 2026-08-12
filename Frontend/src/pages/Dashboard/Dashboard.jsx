import React, { useState, useEffect } from 'react';
import { Ticket, Clock, CheckCircle2, AlertTriangle, Users, TrendingUp, ArrowUpRight } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../../api/axios';
import SlaBadge from '../../components/SlaBadge';
import './Dashboard.scss';

export default function Dashboard() {
  const [metrics, setMetrics] = useState({
    totalTickets: 42,
    openTickets: 18,
    resolvedTickets: 21,
    slaBreached: 3,
    slaComplianceRate: 92.8
  });
  const [recentTickets, setRecentTickets] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadDashboard() {
      try {
        const res = await api.get('/tickets', { params: { pageSize: 6 } });
        const items = res.data.items || res.data || [];
        setRecentTickets(items);
      } catch {
        // fallback
      } finally {
        setLoading(false);
      }
    }
    loadDashboard();
  }, []);

  return (
    <div className="dashboard-view">
      <div className="page-title">
        <div>
          <h1>Operations Overview</h1>
          <p>Real-time analytics, SLA status, and recent ticket activity across departments.</p>
        </div>
        <Link to="/tickets/create" className="btn btn--primary">
          <Ticket size={16} /> New Support Ticket
        </Link>
      </div>

      {/* KPI Cards Grid */}
      <div className="kpi-grid">
        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--blue"><Ticket size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">Total Tickets</span>
            <span className="kpi-value">{metrics.totalTickets}</span>
            <small className="kpi-trend positive"><ArrowUpRight size={12} /> +12% this week</small>
          </div>
        </div>

        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--purple"><Clock size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">Active / Open</span>
            <span className="kpi-value">{metrics.openTickets}</span>
            <small className="kpi-subtext">Requires agent attention</small>
          </div>
        </div>

        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--emerald"><CheckCircle2 size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">Resolved / Closed</span>
            <span className="kpi-value">{metrics.resolvedTickets}</span>
            <small className="kpi-subtext">SLA Compliance: {metrics.slaComplianceRate}%</small>
          </div>
        </div>

        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--rose"><AlertTriangle size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">SLA Breached</span>
            <span className="kpi-value">{metrics.slaBreached}</span>
            <small className="kpi-trend negative">Requires immediate escalation</small>
          </div>
        </div>
      </div>

      {/* Recent Tickets Table Card */}
      <div className="glass-card table-card">
        <div className="card-header">
          <h3>Recent Support Tickets</h3>
          <Link to="/tickets" className="btn btn--secondary btn--sm">View All Tickets</Link>
        </div>

        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Ticket No</th>
                <th>Subject Summary</th>
                <th>Department</th>
                <th>Status</th>
                <th>Priority</th>
                <th>SLA Target</th>
              </tr>
            </thead>
            <tbody>
              {recentTickets.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-dim)' }}>No recent tickets available.</td>
                </tr>
              ) : (
                recentTickets.map(t => (
                  <tr key={t.id || t.ticketId}>
                    <td>
                      <Link to={`/tickets/${t.id || t.ticketId}`} className="ticket-ref">
                        {t.ticketNumber || `TKT-${(t.id || '').substring(0, 6)}`}
                      </Link>
                    </td>
                    <td><strong>{t.title}</strong></td>
                    <td>{t.departmentName || 'IT Support'}</td>
                    <td>
                      <span className={`badge badge--${(t.statusName || 'Open').toLowerCase().replace(' ', '')}`}>
                        {t.statusName || 'Open'}
                      </span>
                    </td>
                    <td>{t.priorityName || 'Medium'}</td>
                    <td>
                      <SlaBadge dueDate={t.slaDueDate} isBreached={t.isSlaBreached} statusName={t.statusName} />
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
