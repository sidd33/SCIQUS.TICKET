import React, { useState } from 'react';
import { UserCheck, UserX, Clock, AlertCircle } from 'lucide-react';
import api from '../../api/axios';
import './AcceptanceBar.scss';

export default function AcceptanceBar({ ticket, onRefresh }) {
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!ticket || ticket.acceptanceStatus !== 'Pending') {
    return null;
  }

  const handleAccept = async () => {
    setLoading(true);
    setError('');
    try {
      await api.post(`/tickets/${ticket.id || ticket.ticketId}/accept`);
      if (onRefresh) onRefresh();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to accept ticket');
    } finally {
      setLoading(false);
    }
  };

  const handleRejectSubmit = async (e) => {
    e.preventDefault();
    if (!rejectionReason.trim()) return;

    setLoading(true);
    setError('');
    try {
      await api.post(`/tickets/${ticket.id || ticket.ticketId}/reject`, {
        reason: rejectionReason
      });
      setShowRejectModal(false);
      if (onRefresh) onRefresh();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to reject ticket');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="acceptance-bar">
      <div className="acceptance-info">
        <Clock className="acceptance-icon" size={20} />
        <div>
          <strong className="acceptance-title">Awaiting Your Acceptance</strong>
          <p className="acceptance-sub">
            Accept this ticket to start working, or reject with a reason to pass it to the next agent.
          </p>
        </div>
      </div>

      <div className="acceptance-actions">
        <button
          className="btn btn--success"
          onClick={handleAccept}
          disabled={loading}
        >
          <UserCheck size={16} /> Accept Ticket
        </button>
        <button
          className="btn btn--danger"
          onClick={() => setShowRejectModal(true)}
          disabled={loading}
        >
          <UserX size={16} /> Reject Ticket
        </button>
      </div>

      {error && <div className="acceptance-error"><AlertCircle size={14} /> {error}</div>}

      {showRejectModal && (
        <div className="modal-backdrop" onClick={() => setShowRejectModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Reject Ticket Assignment</h3>
              <button className="modal-close" onClick={() => setShowRejectModal(false)}>×</button>
            </div>
            <form onSubmit={handleRejectSubmit}>
              <div className="modal-body">
                <p className="modal-hint">
                  Please provide a mandatory reason for rejecting this assignment. The ticket will be automatically re-routed to the next eligible agent in the department.
                </p>
                <div className="field">
                  <label>Rejection Reason *</label>
                  <textarea
                    required
                    rows={3}
                    placeholder="E.g., Currently handling 5 high priority incidents, or subject matter expert required..."
                    value={rejectionReason}
                    onChange={(e) => setRejectionReason(e.target.value)}
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn--secondary" onClick={() => setShowRejectModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn--danger" disabled={loading || !rejectionReason.trim()}>
                  {loading ? 'Rejecting...' : 'Confirm Rejection'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
