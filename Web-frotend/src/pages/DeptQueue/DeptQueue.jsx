import React, { useState, useEffect, useCallback } from 'react';
import { Building2, UserPlus, Eye, AlertCircle, CheckCircle2, Clock } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../../api/axios';
import SlaBadge from '../../components/tickets/SlaBadge';
import ReassignModal from '../../components/tickets/ReassignModal';
import './DeptQueue.scss';

export default function DeptQueue() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [reassignTicket, setReassignTicket] = useState(null);

  const loadDeptQueue = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await api.get('/tickets/department-queue');
      setTickets(res.data.items || res.data || []);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load department queue');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDeptQueue();
  }, [loadDeptQueue]);

  return (
    <div className="dept-queue-page">
      <div className="page-header">
        <div>
          <h1><Building2 size={22} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Department Ticket Queue</h1>
          <p>Unassigned and active tickets within your department.</p>
        </div>
      </div>

      {error && <div className="field-error">{error}</div>}

      <div className="card table-card">
        {loading ? (
          <div className="loading-state">Loading department queue...</div>
        ) : tickets.length === 0 ? (
          <div className="empty-state">
            <CheckCircle2 size={32} color="#10b981" />
            <h3>Department Queue is Clear!</h3>
            <p>There are no open or unassigned tickets currently waiting in your department.</p>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Ticket Info</th>
                <th>Origin / Customer</th>
                <th>Current Assignee</th>
                <th>SLA Target</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {tickets.map(t => (
                <tr key={t.ticketId || t.id}>
                  <td>
                    <Link to={`/tickets/${t.ticketId || t.id}`} className="ticket-id-link">
                      {t.ticketNumber || `TKT-${t.id}`}
                    </Link>
                    <div className="ticket-title">{t.title}</div>
                    <span className="ticket-subname">{t.ticketSubTypeName || t.ticketTypeName}</span>
                  </td>

                  <td>{t.accountName || t.raisedByEmployeeName || 'Portal User'}</td>

                  <td>
                    {t.assignedToUserName ? (
                      <span className="assignee-tag assigned">{t.assignedToUserName}</span>
                    ) : (
                      <span className="assignee-tag unassigned">Unassigned</span>
                    )}
                  </td>

                  <td>
                    <SlaBadge ticket={t} />
                  </td>

                  <td>
                    <div className="actions">
                      <Link to={`/tickets/${t.ticketId || t.id}`} className="btn btn--secondary btn--sm">
                        <Eye size={14} /> View
                      </Link>
                      <button
                        className="btn btn--primary btn--sm"
                        onClick={() => setReassignTicket(t)}
                      >
                        <UserPlus size={14} /> Quick Assign
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {reassignTicket && (
        <ReassignModal
          ticket={reassignTicket}
          onClose={() => setReassignTicket(null)}
          onSuccess={loadDeptQueue}
        />
      )}
    </div>
  );
}
