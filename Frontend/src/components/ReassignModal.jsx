import React, { useState, useEffect } from 'react';
import { UserCheck, X } from 'lucide-react';
import api from '../api/axios';

export default function ReassignModal({ ticketId, currentAssigneeId, onClose, onSuccess }) {
  const [employees, setEmployees] = useState([]);
  const [selectedAgentId, setSelectedAgentId] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    async function loadAgents() {
      try {
        const res = await api.get('/employees', { params: { pageSize: 100 } });
        setEmployees(res.data.items || res.data || []);
      } catch {
        // fallback
      }
    }
    loadAgents();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!selectedAgentId) return;

    setSubmitting(true);
    try {
      await api.post(`/tickets/${ticketId}/reassign`, { assignedToUserId: selectedAgentId });
      if (onSuccess) onSuccess();
      onClose();
    } catch {
      alert('Failed to reassign ticket');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
      <div className="glass-card" style={{ width: '420px', padding: '1.5rem', background: '#0f172a' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h3 style={{ margin: 0, color: 'white', display: 'flex', alignItems: 'center', gap: '8px' }}><UserCheck size={18} /> Reassign Ticket Agent</h3>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: '#9ca3af', cursor: 'pointer' }}><X size={18} /></button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Select Agent</label>
            <select
              value={selectedAgentId}
              onChange={(e) => setSelectedAgentId(e.target.value)}
              required
            >
              <option value="">Select Target Agent...</option>
              {employees.map(emp => (
                <option key={emp.id} value={emp.id}>{emp.name || `${emp.firstName} ${emp.lastName}`}</option>
              ))}
            </select>
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1.25rem' }}>
            <button type="button" className="btn btn--secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={submitting || !selectedAgentId}>
              {submitting ? 'Reassigning...' : 'Reassign Ticket'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
