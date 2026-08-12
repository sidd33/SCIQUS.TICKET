import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Building2, UserCheck, RefreshCw } from 'lucide-react';
import api from '../../api/axios';
import SlaBadge from '../../components/SlaBadge';
import ReassignModal from '../../components/ReassignModal';

export default function DeptQueue() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedTicketId, setSelectedTicketId] = useState(null);

  useEffect(() => {
    fetchDeptQueue();
  }, []);

  async function fetchDeptQueue() {
    setLoading(true);
    try {
      const res = await api.get('/tickets/department-queue', { params: { pageSize: 50 } });
      setTickets(res.data.items || res.data || []);
    } catch {
      // fallback
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Building2 size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Department Ticket Queue</h1>
          <p>Unassigned & active tickets assigned to your department queue.</p>
        </div>
        <button className="btn btn--secondary" onClick={fetchDeptQueue}>
          <RefreshCw size={16} /> Refresh Queue
        </button>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Ticket Code</th>
                <th>Subject Summary</th>
                <th>Assigned Agent</th>
                <th>Status</th>
                <th>Priority</th>
                <th>SLA Countdown</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={7} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>Loading department queue...</td></tr>
              ) : tickets.length === 0 ? (
                <tr><td colSpan={7} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>No open tickets in your department queue.</td></tr>
              ) : (
                tickets.map(t => (
                  <tr key={t.id || t.ticketId}>
                    <td>
                      <Link to={`/tickets/${t.id || t.ticketId}`} className="ticket-code">
                        {t.ticketNumber || `TKT-${(t.id || '').substring(0, 6)}`}
                      </Link>
                    </td>
                    <td>
                      <Link to={`/tickets/${t.id || t.ticketId}`} className="ticket-title-link">
                        {t.title}
                      </Link>
                    </td>
                    <td>{t.assignedToUserName || 'Unassigned Queue'}</td>
                    <td>
                      <span className={`badge badge--${(t.statusName || 'Open').toLowerCase().replace(' ', '')}`}>
                        {t.statusName || 'Open'}
                      </span>
                    </td>
                    <td><strong>{t.priorityName || 'Medium'}</strong></td>
                    <td>
                      <SlaBadge dueDate={t.slaDueDate} isBreached={t.isSlaBreached} statusName={t.statusName} />
                    </td>
                    <td>
                      <button className="btn btn--secondary btn--sm" onClick={() => setSelectedTicketId(t.id || t.ticketId)}>
                        <UserCheck size={14} /> Quick Assign
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {selectedTicketId && (
        <ReassignModal ticketId={selectedTicketId} onClose={() => setSelectedTicketId(null)} onSuccess={fetchDeptQueue} />
      )}
    </div>
  );
}
