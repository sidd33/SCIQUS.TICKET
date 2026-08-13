import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Search, Ticket } from 'lucide-react';
import api from '../../api/axios';
import SlaBadge from '../../components/SlaBadge';

export default function MyTickets() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadMyTickets() {
      try {
const res = await api.get('/tickets', {
  params: { pageSize: 50 }
});        setTickets(res.data.items || res.data || []);
      } catch {
        // fallback
      } finally {
        setLoading(false);
      }
    }
    loadMyTickets();
  }, []);

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1>My Support Tickets</h1>
          <p>Track, inspect, and confirm resolutions for your submitted support requests.</p>
        </div>
        <Link to="/portal/tickets/create" className="btn btn--primary">
          <Plus size={16} /> Raise Support Ticket
        </Link>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Ticket Code</th>
                <th>Subject Summary</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Target Resolution</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>Loading your tickets...</td></tr>
              ) : tickets.length === 0 ? (
                <tr><td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>You have not raised any support tickets yet.</td></tr>
              ) : (
                tickets.map(t => (
                  <tr key={t.id || t.ticketId}>
                    <td>
                      <Link to={`/portal/ticket/${t.id || t.ticketId}`} className="ticket-code">
                        {t.ticketNumber || `TKT-${(t.id || '').substring(0, 6)}`}
                      </Link>
                    </td>
                    <td>
                      <Link to={`/portal/ticket/${t.id || t.ticketId}`} className="ticket-title-link">
                        {t.title}
                      </Link>
                    </td>
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
