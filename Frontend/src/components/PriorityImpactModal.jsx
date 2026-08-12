import React, { useState, useEffect } from 'react';
import { ShieldAlert, Clock, X } from 'lucide-react';
import api from '../api/axios';

export default function PriorityImpactModal({ ticketId, currentPriorityId, currentImpactId, onClose, onSuccess }) {
  const [priorities, setPriorities] = useState([]);
  const [impacts, setImpacts] = useState([]);
  const [selectedPriorityId, setSelectedPriorityId] = useState(currentPriorityId || '');
  const [selectedImpactId, setSelectedImpactId] = useState(currentImpactId || '');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    async function loadMaster() {
      try {
        const [pRes, iRes] = await Promise.all([
          api.get('/ticketpriorities', { params: { includeInactive: false } }),
          api.get('/ticketbusinessimpacts', { params: { includeInactive: false } })
        ]);
        setPriorities(pRes.data || []);
        setImpacts(iRes.data || []);
      } catch {
        // fallback
      }
    }
    loadMaster();
  }, []);

  const selectedPriorityObj = priorities.find(p => (p.id || p.priorityId) === selectedPriorityId);
  const slaHours = selectedPriorityObj?.slaInHours || 24;
  const previewDueDate = new Date(Date.now() + slaHours * 60 * 60 * 1000).toLocaleString();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await api.patch(`/tickets/${ticketId}/priority-impact`, {
        priorityId: selectedPriorityId,
        businessImpactId: selectedImpactId
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch {
      alert('Failed to update priority');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
      <div className="glass-card" style={{ width: '450px', padding: '1.5rem', background: '#0f172a' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h3 style={{ margin: 0, color: 'white', display: 'flex', alignItems: 'center', gap: '8px' }}><ShieldAlert size={18} /> Update Priority & Business Impact</h3>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: '#9ca3af', cursor: 'pointer' }}><X size={18} /></button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Priority Level</label>
            <select
              value={selectedPriorityId}
              onChange={(e) => setSelectedPriorityId(e.target.value)}
              required
            >
              {priorities.map(p => (
                <option key={p.id || p.priorityId} value={p.id || p.priorityId}>
                  {p.name} ({p.slaInHours ? `${p.slaInHours}h SLA` : 'Default SLA'})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Business Impact Level</label>
            <select
              value={selectedImpactId}
              onChange={(e) => setSelectedImpactId(e.target.value)}
              required
            >
              {impacts.map(i => (
                <option key={i.id || i.businessImpactId} value={i.id || i.businessImpactId}>
                  {i.name || i.description}
                </option>
              ))}
            </select>
          </div>

          <div style={{ padding: '0.75rem', background: 'rgba(99, 102, 241, 0.1)', border: '1px solid rgba(99, 102, 241, 0.3)', borderRadius: '8px', marginBottom: '1rem', fontSize: '0.8rem', color: '#818cf8', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Clock size={16} />
            <span>Live SLA Due Date Preview: <strong>{previewDueDate}</strong></span>
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem' }}>
            <button type="button" className="btn btn--secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={submitting}>
              {submitting ? 'Updating...' : 'Update & Recalculate SLA'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
