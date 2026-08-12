import React, { useState, useEffect } from 'react';
import { UserCheck, AlertCircle } from 'lucide-react';
import api from '../../api/axios';

export default function ReassignModal({ ticket, onClose, onSuccess }) {
  const [employees, setEmployees] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [remarks, setRemarks] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadDeptEmployees() {
      try {
        const deptId = ticket.departmentId;
        const res = await api.get('/employees', {
          params: { pageNumber: 1, pageSize: 100, departmentId: deptId }
        });
        const activeEmps = (res.data.items || []).filter(e => e.isActive);
        setEmployees(activeEmps);
      } catch (err) {
        setEmployees([]);
      }
    }
    if (ticket) loadDeptEmployees();
  }, [ticket]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!selectedUserId) return;

    setLoading(true);
    setError('');
    try {
      await api.post(`/tickets/${ticket.id || ticket.ticketId}/reassign`, {
        assignedToUserId: selectedUserId,
        remarks: remarks
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to reassign ticket');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3><UserCheck size={18} style={{ marginRight: '6px', verticalAlign: 'middle' }} /> Reassign Ticket</h3>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            {error && <div className="field-error"><AlertCircle size={14} /> {error}</div>}
            
            <div className="field">
              <label>Select Department Agent *</label>
              <select
                required
                value={selectedUserId}
                onChange={(e) => setSelectedUserId(e.target.value)}
              >
                <option value="">Select agent...</option>
                {employees.map(emp => (
                  <option key={emp.id} value={emp.id}>
                    {emp.firstName} {emp.lastName} ({emp.email})
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label>Remarks / Note (Optional)</label>
              <textarea
                rows={3}
                placeholder="Reason for reassignment..."
                value={remarks}
                onChange={(e) => setRemarks(e.target.value)}
              />
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn btn--secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={loading || !selectedUserId}>
              {loading ? 'Reassigning...' : 'Confirm Reassign'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
