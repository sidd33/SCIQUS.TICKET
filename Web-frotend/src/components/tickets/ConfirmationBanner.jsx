import React, { useState } from 'react';
import { CheckCircle2, RotateCcw, Clock, ShieldCheck } from 'lucide-react';
import api from '../../api/axios';
import './ConfirmationBanner.scss';

export default function ConfirmationBanner({ ticket, isCustomer, onRefresh }) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!ticket || (ticket.status !== 'PendingClosure' && ticket.statusName !== 'PendingClosure')) {
    return null;
  }

  const handleConfirm = async () => {
    setLoading(true);
    setError('');
    try {
      await api.post(`/tickets/${ticket.id || ticket.ticketId}/confirm-closure`);
      if (onRefresh) onRefresh();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to confirm resolution');
    } finally {
      setLoading(false);
    }
  };

  const handleReject = async () => {
    setLoading(true);
    setError('');
    try {
      await api.post(`/tickets/${ticket.id || ticket.ticketId}/reject-closure`);
      if (onRefresh) onRefresh();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to reject resolution');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="confirmation-banner">
      <div className="banner-info">
        <ShieldCheck className="banner-icon" size={22} />
        <div>
          <strong className="banner-title">
            {isCustomer ? 'Please Confirm Ticket Resolution' : 'Awaiting Customer Confirmation'}
          </strong>
          <p className="banner-sub">
            The support agent has marked this issue as resolved. If no response is received, the ticket will auto-close automatically.
          </p>
        </div>
      </div>

      {isCustomer && (
        <div className="banner-actions">
          <button
            className="btn btn--confirm"
            onClick={handleConfirm}
            disabled={loading}
          >
            <CheckCircle2 size={16} /> Yes, Issue Resolved
          </button>
          <button
            className="btn btn--reopen"
            onClick={handleReject}
            disabled={loading}
          >
            <RotateCcw size={16} /> No, Reopen Ticket
          </button>
        </div>
      )}

      {error && <div className="banner-error">{error}</div>}
    </div>
  );
}
