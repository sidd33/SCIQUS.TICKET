import React, { useState, useEffect } from 'react';
import { ArrowRightLeft, AlertCircle } from 'lucide-react';
import api from '../../api/axios';

export default function TransferModal({ ticket, onClose, onSuccess }) {
  const [departments, setDepartments] = useState([]);
  const [targetDeptId, setTargetDeptId] = useState('');
  const [targetAgents, setTargetAgents] = useState([]);
  const [targetAgentId, setTargetAgentId] = useState('');
  const [comment, setComment] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadDepts() {
      try {
        const res = await api.get('/departments', { params: { pageNumber: 1, pageSize: 100 } });
        setDepartments(res.data.items || []);
      } catch (err) {
        setDepartments([]);
      }
    }
    loadDepts();
  }, []);

  useEffect(() => {
    async function loadDeptAgents() {
      if (!targetDeptId) {
        setTargetAgents([]);
        return;
      }
      try {
        const res = await api.get('/employees', {
          params: { pageNumber: 1, pageSize: 100, departmentId: targetDeptId }
        });
        setTargetAgents((res.data.items || []).filter(e => e.isActive));
      } catch (err) {
        setTargetAgents([]);
      }
    }
    loadDeptAgents();
  }, [targetDeptId]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!targetDeptId || !comment.trim()) return;

    setLoading(true);
    setError('');
    try {
      await api.post(`/tickets/${ticket.id || ticket.ticketId}/transfer`, {
        departmentId: targetDeptId,
        targetAssigneeUserId: targetAgentId || null,
        comment: comment
      });
      if (onSuccess) onSuccess();
      onClose();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to transfer ticket');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3><ArrowRightLeft size={18} style={{ marginRight: '6px', verticalAlign: 'middle' }} /> Transfer Department</h3>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            {error && <div className="field-error"><AlertCircle size={14} /> {error}</div>}

            <div className="field">
              <label>Target Department *</label>
              <select
                required
                value={targetDeptId}
                onChange={(e) => setTargetDeptId(e.target.value)}
              >
                <option value="">Select department...</option>
                {departments.map(dept => (
                  <option key={dept.id} value={dept.id}>{dept.name}</option>
                ))}
              </select>
            </div>

            <div className="field">
              <label>Target Agent (Optional - leave blank for Auto-Routing)</label>
              <select
                value={targetAgentId}
                onChange={(e) => setTargetAgentId(e.target.value)}
                disabled={!targetDeptId}
              >
                <option value="">Auto-Route via Department Rules</option>
                {targetAgents.map(agent => (
                  <option key={agent.id} value={agent.id}>
                    {agent.firstName} {agent.lastName}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label>Mandatory Transfer Comment *</label>
              <textarea
                required
                rows={3}
                placeholder="Reason for transferring to this department..."
                value={comment}
                onChange={(e) => setComment(e.target.value)}
              />
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn btn--secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={loading || !targetDeptId || !comment.trim()}>
              {loading ? 'Transferring...' : 'Transfer Ticket'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
