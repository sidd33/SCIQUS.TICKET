import React, { useState, useEffect } from 'react';
import { AlertOctagon, Clock, AlertCircle } from 'lucide-react';
import api from '../../api/axios';

export default function PriorityImpactModal({ ticket, onClose, onSuccess }) {
  const [priorities, setPriorities] = useState([]);
  const [impacts, setImpacts] = useState([]);
  const [selectedPriorityId, setSelectedPriorityId] = useState(ticket?.priorityId || '');
  const [selectedImpactId, setSelectedImpactId] = useState(ticket?.businessImpactId || '');
  const [reason, setReason] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadMasterData() {
      try {
        const [priRes, impRes] = await Promise.all([
          api.get('/ticketpriorities', { params: { includeInactive: false } }),
          api.get('/ticketbusinessimpacts', { params: { includeInactive: false } })
        ]);
        setPriorities(priRes.data || []);
        setImpacts(impRes.data || []);
      } catch (err) {
        // Fallback
      }
    }
    loadMasterData();
  }, []);

  const selectedPriorityObj = priorities.find(p => p.id === selectedPriorityId || p.priorityId === selectedPriorityId);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!reason.trim()) return;

    setLoading(true);
    setError('');
    try {
      await api.patch(`/tickets/${ticket.id || ticket.ticketId}/priority-impact`, {
        priorityId: selectedPriorityId || null,
        businessImpactId: selectedImpactId || null,
        reason: reason
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update priority/impact');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3><AlertOctagon size={18} style={{ marginRight: '6px', verticalAlign: 'middle' }} /> Change Priority & Business Impact</h3>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            {error && <div className="field-error"><AlertCircle size={14} /> {error}</div>}

            <div className="field">
              <label>Priority Severity</label>
              <select
                value={selectedPriorityId}
                onChange={(e) => setSelectedPriorityId(e.target.value)}
              >
                {priorities.map(p => (
                  <option key={p.id || p.priorityId} value={p.id || p.priorityId}>
                    {p.name} ({p.slaInHours ? `${p.slaInHours}h SLA` : 'No SLA'})
                  </option>
                ))}
              </select>
            </div>

            {selectedPriorityObj && (
              <div className="sla-preview-box" style={{ background: 'rgba(59, 130, 246, 0.08)', border: '1px solid rgba(59, 130, 246, 0.2)', padding: '0.75rem', borderRadius: '6px', marginBottom: '1rem' }}>
                <span style={{ fontSize: '0.85rem', color: '#1d4ed8', display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <Clock size={14} /> SLA Due Date Live Preview: {selectedPriorityObj.slaInHours > 0 ? `${selectedPriorityObj.slaInHours} hours from creation` : 'N/A (No SLA Target)'}
                </span>
              </div>
            )}

            <div className="field">
              <label>Business Impact Level</label>
              <select
                value={selectedImpactId}
                onChange={(e) => setSelectedImpactId(e.target.value)}
              >
                {impacts.map(imp => (
                  <option key={imp.id || imp.businessImpactId} value={imp.id || imp.businessImpactId}>
                    {imp.description || imp.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label>Mandatory Change Reason *</label>
              <textarea
                required
                rows={3}
                placeholder="Reason for escalating/de-escalating priority..."
                value={reason}
                onChange={(e) => setReason(e.target.value)}
              />
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn btn--secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={loading || !reason.trim()}>
              {loading ? 'Saving...' : 'Update & Recalculate SLA'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
