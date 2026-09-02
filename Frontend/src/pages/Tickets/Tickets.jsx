import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Ticket,
  Plus,
  Search,
  Filter,
  RefreshCw,
  Trash2
} from 'lucide-react';
import api from '../../api/axios';
import SlaBadge from '../../components/SlaBadge';
import { isAdmin } from '../../auth/roles';
import './Tickets.scss';

export default function Tickets() {
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  useEffect(() => {
    fetchTickets();
  }, [statusFilter]);

  async function fetchTickets() {
    setLoading(true);

    try {
      const endpoint = isAdmin(user)
        ? '/tickets'
        : '/tickets/my-queue';

      const res = await api.get(endpoint, {
        params: {
          pageSize: 50,
          statusName: statusFilter || undefined
        }
      });

      setTickets(res.data.items || res.data || []);
    } catch (error) {
      console.error('Failed to fetch tickets:', error);
      setTickets([]);
    } finally {
      setLoading(false);
    }
  }

  async function handleDeleteTicket(ticketId) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this ticket? This action cannot be undone.'
    );

    if (!confirmed) return;

    try {
      await api.delete(`/tickets/${ticketId}`);

      setTickets(prev =>
        prev.filter(t => (t.id || t.ticketId) !== ticketId)
      );

      alert('Ticket deleted successfully.');
    } catch (error) {
      console.error(
        'Failed to delete ticket:',
        error.response?.data || error
      );

      alert(
        error.response?.data?.message ||
        'Failed to delete ticket.'
      );
    }
  }

  const filtered = tickets.filter(t => {
    if (!search) return true;

    const q = search.toLowerCase();

    return (
      t.title?.toLowerCase().includes(q) ||
      t.ticketNumber?.toLowerCase().includes(q) ||
      t.departmentName?.toLowerCase().includes(q)
    );
  });

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1>Ticket Workspace</h1>
          <p>
            Manage, inspect, assign, and resolve support tickets across
            departments.
          </p>
        </div>

        <Link to="/tickets/create" className="btn btn--primary">
          <Plus size={16} /> Raise Ticket
        </Link>
      </div>

      {/* Filter Bar */}
      <div className="glass-card filter-bar">
        <div className="search-input">
          <Search size={16} />

          <input
            placeholder="Search by ticket number, summary, department..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        <div className="filter-group">
          <Filter size={16} color="var(--text-muted)" />

          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">All Statuses</option>
            <option value="Open">Open</option>
            <option value="In Progress">In Progress</option>
            <option value="Pending">Pending</option>
            <option value="Resolved">Resolved</option>
            <option value="Closed">Closed</option>
            <option value="PendingClosure">Pending Closure</option>
          </select>

          <button
            className="btn btn--secondary btn--sm"
            onClick={fetchTickets}
            title="Refresh"
          >
            <RefreshCw size={14} />
          </button>
        </div>
      </div>

      {/* Table List */}
      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Ticket Number</th>
                <th>Subject Summary</th>
                <th>Department</th>
                <th>Assigned Agent</th>
                <th>Status</th>
                <th>Priority</th>
                <th>SLA Countdown</th>

                {isAdmin(user) && <th>Actions</th>}
              </tr>
            </thead>

            <tbody>
              {loading ? (
                <tr>
                  <td
                    colSpan={isAdmin(user) ? 8 : 7}
                    style={{
                      textAlign: 'center',
                      color: 'var(--text-dim)',
                      padding: '2rem'
                    }}
                  >
                    Loading tickets from server...
                  </td>
                </tr>
              ) : filtered.length === 0 ? (
                <tr>
                  <td
                    colSpan={isAdmin(user) ? 8 : 7}
                    style={{
                      textAlign: 'center',
                      color: 'var(--text-dim)',
                      padding: '2rem'
                    }}
                  >
                    No tickets found matching current filter criteria.
                  </td>
                </tr>
              ) : (
                filtered.map(t => {
                  const ticketId = t.id || t.ticketId;

                  return (
                    <tr key={ticketId}>
                      <td>
                        <Link
                          to={`/tickets/${ticketId}`}
                          className="ticket-code"
                        >
                          {t.ticketNumber ||
                            `TKT-${(t.id || '').substring(0, 6)}`}
                        </Link>
                      </td>

                      <td>
                        <Link
                          to={`/tickets/${ticketId}`}
                          className="ticket-title-link"
                        >
                          {t.title}
                        </Link>
                      </td>

                      <td>
                        {t.departmentName || 'IT Support'}
                      </td>

                      <td>
                        {t.assignedToUserName || 'Unassigned'}
                      </td>

                      <td>
                        {t.acceptanceStatus === 'Pending' ? (
                          <span className="badge badge--awaiting-acceptance">
                            Awaiting Acceptance
                          </span>
                        ) : (
                          <span
                            className={`badge badge--${(
                              t.statusName || 'Open'
                            )
                              .toLowerCase()
                              .replace(' ', '')}`}
                          >
                            {t.statusName || 'Open'}
                          </span>
                        )}
                      </td>

                      <td>
                        <strong>
                          {t.priorityName || 'Medium'}
                        </strong>
                      </td>

                      <td>
                        <SlaBadge
                          dueDate={t.slaDueDate}
                          isBreached={t.isSlaBreached}
                          statusName={t.statusName}
                        />
                      </td>

                      {isAdmin(user) && (
                        <td>
                          <button
                            type="button"
                            className="btn btn--secondary btn--sm"
                            onClick={() =>
                              handleDeleteTicket(ticketId)
                            }
                            title="Delete Ticket"
                            aria-label={`Delete ${
                              t.ticketNumber || 'ticket'
                            }`}
                            style={{
                              color: '#f87171',
                              borderColor:
                                'rgba(248, 113, 113, 0.35)'
                            }}
                          >
                            <Trash2 size={14} />
                          </button>
                        </td>
                      )}
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}