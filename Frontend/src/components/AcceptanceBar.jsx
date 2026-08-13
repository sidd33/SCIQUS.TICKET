import React, { useState } from 'react';
import { Clock, CheckCircle2, XCircle, AlertCircle } from 'lucide-react';
import api from '../api/axios';

export default function AcceptanceBar({
  ticketId,
  deadline,
  assignedToUserId,
  currentUserId,
  onAction
}) {  
  const [rejecting, setRejecting] = useState(false);
  const [reason, setReason] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const canAcceptReject =
  currentUserId &&
  assignedToUserId &&
  currentUserId === assignedToUserId;

  const handleAccept = async () => {
    setSubmitting(true);
    try {
      await api.post(`/tickets/${ticketId}/accept`);
      if (onAction) onAction();
    } catch {
      alert('Failed to accept assignment');
    } finally {
      setSubmitting(false);
    }
  };

  const handleReject = async (e) => {
    e.preventDefault();
    if (!reason.trim()) return;
    setSubmitting(true);
    try {
      await api.post(`/tickets/${ticketId}/reject`, { reason });
      if (onAction) onAction();
    } catch {
      alert('Failed to reject assignment');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="acceptance-bar glass-card" style={{ padding: '1rem', border: '1px solid rgba(245, 158, 11, 0.4)', background: 'rgba(245, 158, 11, 0.08)', marginBottom: '1.25rem' }}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '1rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <Clock size={20} color="#fbbf24" />
          <div>
            <strong style={{ color: '#fbbf24', fontSize: '0.9rem', display: 'block' }}>Agent Acceptance Required</strong>
            <span style={{ fontSize: '0.8rem', color: '#d1d5db' }}>
              Accept or reject assignment deadline: {deadline ? new Date(deadline).toLocaleString() : 'Pending'}
            </span>
          </div>
        </div>

        {canAcceptReject ? (
  !rejecting ? (
    <div style={{ display: 'flex', gap: '0.5rem' }}>
      <button
        className="btn btn--success btn--sm"
        onClick={handleAccept}
        disabled={submitting}
      >
        <CheckCircle2 size={14} /> Accept Ticket
      </button>

      <button
        className="btn btn--danger btn--sm"
        onClick={() => setRejecting(true)}
        disabled={submitting}
      >
        <XCircle size={14} /> Reject Ticket
      </button>
    </div>
  ) : (
    <form
      onSubmit={handleReject}
      style={{
        display: 'flex',
        gap: '0.5rem',
        width: '100%',
        marginTop: '0.5rem'
      }}
    >
      <input
        placeholder="Enter rejection reason..."
        value={reason}
        onChange={(e) => setReason(e.target.value)}
        required
        style={{
          flex: 1,
          padding: '0.4rem 0.75rem',
          background: '#0f172a',
          border: '1px solid #334155',
          borderRadius: '6px',
          color: 'white',
          fontSize: '0.8rem'
        }}
      />

      <button
        type="submit"
        className="btn btn--danger btn--sm"
        disabled={submitting || !reason.trim()}
      >
        Confirm Reject
      </button>

      <button
        type="button"
        className="btn btn--secondary btn--sm"
        onClick={() => setRejecting(false)}
      >
        Cancel
      </button>
    </form>
  )
) : null}
      </div>
    </div>
  );
}
