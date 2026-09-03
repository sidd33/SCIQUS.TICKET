import React, { useState, useEffect } from 'react';
import { ArrowRightLeft, X } from 'lucide-react';
import api from '../api/axios';

export default function TransferModal({ ticketId, onClose, onSuccess }) {
  const [departments, setDepartments] = useState([]);
  const [selectedDeptId, setSelectedDeptId] = useState('');
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    async function loadDepts() {
      try {
        const res = await api.get('/departments', { params: { pageSize: 100 } });
        setDepartments(res.data.items || res.data || []);
      } catch {
        // fallback
      }
    }
    loadDepts();
  }, []);
const handleSubmit = async (e) => {
  e.preventDefault();
  if (!selectedDeptId) return;

  setSubmitting(true);

  try {
    await api.post(`/tickets/${ticketId}/transfer`, {
      departmentId: selectedDeptId,
      comment: comment
    });

    if (onSuccess) {
      await onSuccess();
    }

    onClose();
  } catch (err) {
    console.error(
      'Failed to transfer department:',
      err.response?.data || err
    );

    alert(
      err.response?.data?.message ||
      'Failed to transfer department'
    );
  } finally {
    setSubmitting(false);
  }
};

  return (
    <div className="modal-overlay" style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
      <div className="glass-card" style={{ width: '420px', padding: '1.5rem', background: '#0f172a' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h3 style={{ margin: 0, color: 'white', display: 'flex', alignItems: 'center', gap: '8px' }}><ArrowRightLeft size={18} /> Transfer Department</h3>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: '#9ca3af', cursor: 'pointer' }}><X size={18} /></button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Select Target Department</label>
            <select
              value={selectedDeptId}
              onChange={(e) => setSelectedDeptId(e.target.value)}
              required
            >
              <option value="">Select Department...</option>
              {departments.map(d => (
  <option key={d.departmentId} value={d.departmentId}>{d.name}</option>
))}
            </select>
          </div>

          <div className="form-group">
  <label>Comment</label>
  <textarea
    value={comment}
    onChange={(e) => setComment(e.target.value)}
    placeholder="Enter reason for transferring the ticket"
    required
  />
</div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1.25rem' }}>
            <button type="button" className="btn btn--secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={submitting || !selectedDeptId}>
              {submitting ? 'Transferring...' : 'Transfer Ticket'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
