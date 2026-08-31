import React, { useEffect, useState } from 'react';
import { CalendarDays, RefreshCw, CheckCircle2, AlertCircle, Check, X as XIcon, Pencil } from 'lucide-react';
import api from '../../api/axios';
 
const HOLIDAY_STYLES = `
.mh-page {
  padding: 1.5rem;
  max-width: 960px;
  margin: 0 auto;
}
.mh-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}
.mh-title-row {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  color: #818cf8;
}
.mh-title-row h1 {
  font-size: 1.5rem;
  font-weight: 700;
  color: #f1f5f9;
  margin: 0;
}
.mh-header p {
  margin: 0.35rem 0 0;
  color: #94a3b8;
  font-size: 0.9rem;
}
.mh-alert {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.88rem;
  margin-bottom: 1.25rem;
}
.mh-alert--success {
  background: rgba(16, 185, 129, 0.12);
  border: 1px solid rgba(16, 185, 129, 0.35);
  color: #6ee7b7;
}
.mh-alert--error {
  background: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.35);
  color: #fca5a5;
}
.mh-card {
  background: rgba(15, 23, 42, 0.6);
  border: 1px solid #1e293b;
  border-radius: 14px;
  margin-bottom: 1.5rem;
  overflow: hidden;
}
.mh-section-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #1e293b;
}
.mh-section-header h2 {
  margin: 0;
  font-size: 1.05rem;
  color: #f1f5f9;
}
.mh-section-header p {
  margin: 0.2rem 0 0;
  color: #94a3b8;
  font-size: 0.85rem;
}
.mh-btn--icon {
  padding: 0.5rem;
  border-radius: 8px;
  border: 1px solid #334155;
  background: transparent;
  color: #94a3b8;
  cursor: pointer;
  display: inline-flex;
}
.mh-btn--icon:hover:not(:disabled) { background: #1e293b; color: #e2e8f0; }
.mh-spin { animation: mh-spin 0.9s linear infinite; }
@keyframes mh-spin { to { transform: rotate(360deg); } }
.mh-empty-state {
  text-align: center;
  padding: 3rem 1.5rem;
  color: #94a3b8;
}
.mh-empty-state svg {
  color: #475569;
  margin-bottom: 0.75rem;
}
.mh-empty-state h3 {
  margin: 0 0 0.35rem;
  color: #e2e8f0;
  font-size: 1rem;
}
.mh-empty-state p {
  margin: 0;
  font-size: 0.88rem;
}
.mh-table-wrapper {
  overflow-x: auto;
}
.mh-table {
  width: 100%;
  border-collapse: collapse;
}
.mh-table th {
  text-align: left;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #64748b;
  padding: 0.85rem 1.5rem;
  border-bottom: 1px solid #1e293b;
}
.mh-table td {
  padding: 0.85rem 1.5rem;
  font-size: 0.9rem;
  color: #e2e8f0;
  border-bottom: 1px solid #1e293b;
  vertical-align: middle;
}
.mh-table tr:last-child td { border-bottom: none; }
.mh-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.65rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
}
.mh-badge--available { background: rgba(16, 185, 129, 0.15); color: #6ee7b7; }
.mh-badge--unavailable { background: rgba(239, 68, 68, 0.15); color: #fca5a5; }
.mh-badge--pending { background: rgba(245, 158, 11, 0.15); color: #fcd34d; }
.mh-confirm-actions {
  display: flex;
  gap: 0.5rem;
}
.mh-confirm-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.4rem 0.75rem;
  border-radius: 7px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid transparent;
  transition: opacity 0.15s ease;
}
.mh-confirm-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.mh-confirm-btn--available {
  background: rgba(16, 185, 129, 0.15);
  color: #6ee7b7;
  border-color: rgba(16, 185, 129, 0.35);
}
.mh-confirm-btn--available:hover:not(:disabled) { background: rgba(16, 185, 129, 0.25); }
.mh-confirm-btn--unavailable {
  background: rgba(239, 68, 68, 0.15);
  color: #fca5a5;
  border-color: rgba(239, 68, 68, 0.35);
}
.mh-confirm-btn--unavailable:hover:not(:disabled) { background: rgba(239, 68, 68, 0.25); }
.mh-confirmed-response {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}
.mh-edit-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.4rem 0.75rem;
  border-radius: 7px;
  border: 1px solid #334155;
  background: transparent;
  color: #94a3b8;
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: 600;
}
.mh-edit-btn:hover { background: #1e293b; color: #e2e8f0; }
`;
 
export default function MyHoliday() {
  const [confirmations, setConfirmations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [respondingId, setRespondingId] = useState(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [editingIds, setEditingIds] = useState(new Set());
 
  const user = JSON.parse(localStorage.getItem('user') || 'null');
  const employeeId = user?.id;
 
  const isConfirmed = (status) =>
    ['available', 'unavailable'].includes((status || '').toLowerCase());
 
  const loadConfirmations = async () => {
    if (!employeeId) {
      setError('Unable to identify the logged-in employee.');
      setLoading(false);
      return;
    }
 
    try {
      setLoading(true);
      setError('');
 
      const response = await api.get(
        `/Holidays/employees/${employeeId}/confirmations`
      );
 
      setConfirmations(response.data || []);
    } catch (err) {
      console.error('Failed to load holidays:', err);
 
      setError(
        err.response?.data?.message ||
        'Failed to load your holiday calendar.'
      );
    } finally {
      setLoading(false);
    }
  };
 
  useEffect(() => {
    loadConfirmations();
  }, []);
 
  const handleConfirm = async (holidayId, isAvailable) => {
    if (!employeeId) {
      setError('Unable to identify the logged-in employee.');
      return;
    }
 
    setError('');
    setSuccess('');
    setRespondingId(holidayId);
 
    try {
      await api.post(
        `/Holidays/${holidayId}/confirmations/${employeeId}`,
        { isAvailable }
      );
 
      setSuccess(
        isAvailable
          ? 'Marked as available. Thanks for confirming!'
          : 'Marked as unavailable for this holiday.'
      );
 
      setEditingIds((prev) => {
        const next = new Set(prev);
        next.delete(holidayId);
        return next;
      });
 
      await loadConfirmations();
    } catch (err) {
      console.error('Failed to confirm holiday:', err);
 
      setError(
        err.response?.data?.message ||
        'Failed to submit your response.'
      );
    } finally {
      setRespondingId(null);
    }
  };
 
  const toggleEdit = (holidayId) => {
    setEditingIds((prev) => {
      const next = new Set(prev);
      if (next.has(holidayId)) {
        next.delete(holidayId);
      } else {
        next.add(holidayId);
      }
      return next;
    });
  };
 
  const getStatusClass = (status) => {
    switch ((status || '').toLowerCase()) {
      case 'available':
        return 'mh-badge--available';
      case 'unavailable':
        return 'mh-badge--unavailable';
      default:
        return 'mh-badge--pending';
    }
  };
 
  const formatDate = (date) => {
    if (!date) return '-';
 
    return new Date(date).toLocaleDateString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      weekday: 'short',
    });
  };
 
  return (
    <div className="mh-page">
      <style>{HOLIDAY_STYLES}</style>
 
      <div className="mh-header">
        <div>
          <div className="mh-title-row">
            <CalendarDays size={26} />
            <h1>Holiday Calendar</h1>
          </div>
          <p>Confirm your availability for upcoming company holidays.</p>
        </div>
      </div>
 
      {success && (
        <div className="mh-alert mh-alert--success">
          <CheckCircle2 size={16} />
          {success}
        </div>
      )}
 
      {error && (
        <div className="mh-alert mh-alert--error">
          <AlertCircle size={16} />
          {error}
        </div>
      )}
 
      <div className="mh-card">
        <div className="mh-section-header">
          <div>
            <h2>Company Holidays</h2>
            <p>Unconfirmed holidays are treated as unavailable by default.</p>
          </div>
 
          <button
            className="mh-btn--icon"
            onClick={loadConfirmations}
            disabled={loading}
            title="Refresh"
          >
            <RefreshCw size={17} className={loading ? 'mh-spin' : ''} />
          </button>
        </div>
 
        {loading ? (
          <div className="mh-empty-state">
            <p>Loading holiday calendar...</p>
          </div>
        ) : confirmations.length === 0 ? (
          <div className="mh-empty-state">
            <CalendarDays size={40} />
            <h3>No holidays scheduled</h3>
            <p>There are no company holidays on the calendar yet.</p>
          </div>
        ) : (
          <div className="mh-table-wrapper">
            <table className="mh-table">
              <thead>
                <tr>
                  <th>Holiday</th>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Response</th>
                </tr>
              </thead>
 
              <tbody>
                {confirmations.map((c) => {
                  const confirmed = isConfirmed(c.status);
                  const showButtons = !confirmed || editingIds.has(c.holidayId);
 
                  return (
                    <tr key={c.id}>
                      <td>{c.holidayName}</td>
                      <td>{formatDate(c.holidayDate)}</td>
                      <td>
                        <span className={`mh-badge ${getStatusClass(c.status)}`}>
                          {c.status}
                        </span>
                      </td>
                      <td>
                        {showButtons ? (
                          <div className="mh-confirm-actions">
                            <button
                              className="mh-confirm-btn mh-confirm-btn--available"
                              onClick={() => handleConfirm(c.holidayId, true)}
                              disabled={respondingId === c.holidayId}
                            >
                              <Check size={14} />
                              Available
                            </button>
 
                            <button
                              className="mh-confirm-btn mh-confirm-btn--unavailable"
                              onClick={() => handleConfirm(c.holidayId, false)}
                              disabled={respondingId === c.holidayId}
                            >
                              <XIcon size={14} />
                              Unavailable
                            </button>
                          </div>
                        ) : (
                          <div className="mh-confirmed-response">
                            <button
                              className="mh-edit-btn"
                              onClick={() => toggleEdit(c.holidayId)}
                              title="Change response"
                            >
                              <Pencil size={14} />
                              <span>Edit</span>
                            </button>
                          </div>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
 