import React, { useState } from 'react';
import { HelpCircle, CheckCircle2, RotateCcw } from 'lucide-react';
import api from '../api/axios';

export default function ConfirmationBanner({ ticketId, onAction }) {
  const [loading, setLoading] = useState(false);

  const handleConfirm = async () => {
    setLoading(true);
    try {
      await api.post(`/tickets/${ticketId}/confirm-closure`);
      if (onAction) onAction();
    } catch {
      alert('Failed to confirm resolution');
    } finally {
      setLoading(false);
    }
  };

  const handleReopen = async () => {
    const reason = prompt('Please provide reason for reopening this ticket:');
    if (!reason) return;
    setLoading(true);
    try {
      await api.post(`/tickets/${ticketId}/reopen`, { reason });
      if (onAction) onAction();
    } catch {
      alert('Failed to reopen ticket');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="confirmation-banner glass-card" style={{ padding: '1.25rem', border: '1px solid rgba(16, 185, 129, 0.4)', background: 'rgba(16, 185, 129, 0.08)', marginBottom: '1.25rem' }}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '1rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <HelpCircle size={22} color="#34d399" />
          <div>
            <strong style={{ color: '#34d399', fontSize: '0.95rem', display: 'block' }}>Resolution Confirmation Requested</strong>
            <span style={{ fontSize: '0.8rem', color: '#d1d5db' }}>
              Support staff has marked this ticket as resolved. Please confirm if your issue is resolved.
            </span>
          </div>
        </div>

        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className="btn btn--success btn--sm" onClick={handleConfirm} disabled={loading}>
            <CheckCircle2 size={14} /> Confirm & Close Ticket
          </button>
          <button className="btn btn--secondary btn--sm" onClick={handleReopen} disabled={loading}>
            <RotateCcw size={14} /> Reopen Ticket
          </button>
        </div>
      </div>
    </div>
  );
}
