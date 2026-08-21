import React, { useState, useEffect } from 'react';
import { Inbox, RefreshCw, AlertCircle, Eye } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../../../api/axios';

export default function EmailInboxReview() {
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [statusFilter, setStatusFilter] = useState('All');
  const [error, setError] = useState('');

  const fetchMessages = async (statusVal) => {
    setLoading(true);
    setError('');
    try {
      const response = await api.get(`/EmailTicketConfig/InboxReview?status=${statusVal}`);
      setMessages(response.data || []);
    } catch (err) {
      console.error("Failed to fetch inbox messages", err);
      setError("Failed to load email inbox queue. Make sure the backend is running.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMessages(statusFilter);
  }, [statusFilter]);

  const handleRefresh = () => {
    fetchMessages(statusFilter);
  };

  return (
    <div className="tickets-page">
      <style>{`
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
        .spin-icon {
          animation: spin 1s linear infinite;
        }
      `}</style>

      <div className="page-header">
        <div>
          <h1>
            <Inbox size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> 
            Raw Email Inbox Review Queue
          </h1>
          <p style={{ marginTop: '0.25rem', color: '#9ca3af' }}>
            Module 6 — Inspect unparsed or pending inbound support emails for ticket auto-creation.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '0.75rem' }}>
          <button 
            onClick={handleRefresh} 
            className="btn btn--secondary" 
            style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}
            disabled={loading}
          >
            <RefreshCw size={14} className={loading ? "spin-icon" : ""} /> Refresh
          </button>
          <Link to="/admin/email-ticket-config" className="btn btn--secondary">
            Back to Settings
          </Link>
        </div>
      </div>

      {error && (
        <div style={{ padding: '1rem', background: 'rgba(239, 68, 68, 0.1)', border: '1px solid rgba(239, 68, 68, 0.2)', borderRadius: '6px', color: '#f87171', marginBottom: '1.5rem' }}>
          {error}
        </div>
      )}

      <div className="glass-card" style={{ padding: '1rem 1.5rem', marginBottom: '1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <span style={{ fontSize: '0.9rem', color: '#9ca3af' }}>Filter status:</span>
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            {['All', 'Pending', 'Processed', 'Failed'].map((status) => (
              <button
                key={status}
                onClick={() => setStatusFilter(status)}
                className={`btn btn--sm ${statusFilter === status ? 'btn--primary' : 'btn--secondary'}`}
                style={{ padding: '0.25rem 0.75rem', fontSize: '0.8rem' }}
              >
                {status}
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Sender Address</th>
                <th>Sender Name</th>
                <th>Email Subject</th>
                <th>Received Date</th>
                <th>Match Status</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="6" style={{ textAlign: 'center', padding: '3rem', color: '#9ca3af' }}>
                    <RefreshCw size={24} className="spin-icon" style={{ margin: '0 auto 1rem' }} />
                    Loading messages...
                  </td>
                </tr>
              ) : messages.length === 0 ? (
                <tr>
                  <td colSpan="6" style={{ textAlign: 'center', padding: '3rem', color: '#9ca3af' }}>
                    <AlertCircle size={24} style={{ margin: '0 auto 1rem', color: '#9ca3af' }} />
                    No inbox messages found in this status queue.
                  </td>
                </tr>
              ) : (
                messages.map((msg) => {
                  let statusBadgeClass = "badge badge--in-progress";
                  let statusText = msg.processingStatus;
                  if (msg.processingStatus === "Processed") {
                    statusBadgeClass = "badge badge--resolved";
                    statusText = "Matched & Ticket Created";
                  } else if (msg.processingStatus === "Failed") {
                    statusBadgeClass = "badge badge--critical";
                    statusText = `Failed: ${msg.failureReason || 'Unknown error'}`;
                  }

                  return (
                    <tr key={msg.emailInboxMessageId}>
                      <td style={{ fontWeight: '500' }}>{msg.fromEmail}</td>
                      <td>{msg.fromName || <span style={{ color: '#6b7280', fontStyle: 'italic' }}>None</span>}</td>
                      <td>{msg.subject}</td>
                      <td>{new Date(msg.emailReceivedDate).toLocaleString()}</td>
                      <td>
                        <span className={statusBadgeClass}>{statusText}</span>
                      </td>
                      <td>
                        {msg.createdTicketId ? (
                          <Link 
                            to="/tickets" 
                            className="btn btn--secondary btn--sm" 
                            style={{ display: 'inline-flex', alignItems: 'center', gap: '0.25rem' }}
                          >
                            <Eye size={12} /> View Ticket
                          </Link>
                        ) : (
                          <span style={{ color: '#6b7280' }}>-</span>
                        )}
                      </td>
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
