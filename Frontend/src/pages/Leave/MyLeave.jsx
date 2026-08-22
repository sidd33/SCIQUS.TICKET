import React, { useEffect, useState } from 'react';
import { CalendarOff, PlusCircle, X, RefreshCw, CheckCircle2, AlertCircle } from 'lucide-react';
import api from '../../api/axios';

// Scoped styling — self-contained so the page renders correctly regardless
// of what's currently in MyLeave.scss. Prefixed with `ml-` to avoid clashing
// with global classes elsewhere in the app.
const LEAVE_STYLES = `
.ml-page {
  padding: 1.5rem;
  max-width: 960px;
  margin: 0 auto;
}
.ml-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}
.ml-title-row {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  color: #818cf8;
}
.ml-title-row h1 {
  font-size: 1.5rem;
  font-weight: 700;
  color: #f1f5f9;
  margin: 0;
}
.ml-header p {
  margin: 0.35rem 0 0;
  color: #94a3b8;
  font-size: 0.9rem;
}
.ml-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1.1rem;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.9rem;
  border: 1px solid transparent;
  cursor: pointer;
  transition: opacity 0.15s ease, transform 0.05s ease;
  white-space: nowrap;
}
.ml-btn:active { transform: scale(0.98); }
.ml-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.ml-btn--primary {
  background: #4f46e5;
  color: #fff;
}
.ml-btn--primary:hover:not(:disabled) { background: #4338ca; }
.ml-btn--secondary {
  background: transparent;
  color: #cbd5e1;
  border-color: #334155;
}
.ml-btn--secondary:hover:not(:disabled) { background: #1e293b; }
.ml-btn--icon {
  padding: 0.5rem;
  border-radius: 8px;
  border: 1px solid #334155;
  background: transparent;
  color: #94a3b8;
  cursor: pointer;
  display: inline-flex;
}
.ml-btn--icon:hover:not(:disabled) { background: #1e293b; color: #e2e8f0; }
.ml-spin { animation: ml-spin 0.9s linear infinite; }
@keyframes ml-spin { to { transform: rotate(360deg); } }

.ml-alert {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.88rem;
  margin-bottom: 1.25rem;
}
.ml-alert--success {
  background: rgba(16, 185, 129, 0.12);
  border: 1px solid rgba(16, 185, 129, 0.35);
  color: #6ee7b7;
}
.ml-alert--error {
  background: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.35);
  color: #fca5a5;
}

.ml-card {
  background: rgba(15, 23, 42, 0.6);
  border: 1px solid #1e293b;
  border-radius: 14px;
  margin-bottom: 1.5rem;
  overflow: hidden;
}
.ml-form-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 1.25rem 1.5rem 0.75rem;
}
.ml-form-header h2 {
  margin: 0;
  font-size: 1.1rem;
  color: #f1f5f9;
}
.ml-form-header p {
  margin: 0.25rem 0 0;
  color: #94a3b8;
  font-size: 0.85rem;
}
.ml-close-btn {
  background: transparent;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 6px;
  display: inline-flex;
}
.ml-close-btn:hover { background: #1e293b; color: #e2e8f0; }

.ml-form-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  padding: 0.5rem 1.5rem 1.25rem;
}
@media (max-width: 700px) {
  .ml-form-grid { grid-template-columns: 1fr; }
}
.ml-field {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.ml-field label {
  font-size: 0.82rem;
  font-weight: 500;
  color: #94a3b8;
}
.ml-field input,
.ml-field select {
  width: 100%;
  box-sizing: border-box;
  padding: 0.55rem 0.7rem;
  border-radius: 7px;
  border: 1px solid #334155;
  background: #0f172a;
  color: #e2e8f0;
  font-size: 0.9rem;
}
.ml-field input:focus,
.ml-field select:focus {
  outline: none;
  border-color: #6366f1;
}
.ml-form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.6rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid #1e293b;
}

.ml-section-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #1e293b;
}
.ml-section-header h2 {
  margin: 0;
  font-size: 1.05rem;
  color: #f1f5f9;
}
.ml-section-header p {
  margin: 0.2rem 0 0;
  color: #94a3b8;
  font-size: 0.85rem;
}

.ml-empty-state {
  text-align: center;
  padding: 3rem 1.5rem;
  color: #94a3b8;
}
.ml-empty-state svg {
  color: #475569;
  margin-bottom: 0.75rem;
}
.ml-empty-state h3 {
  margin: 0 0 0.35rem;
  color: #e2e8f0;
  font-size: 1rem;
}
.ml-empty-state p {
  margin: 0;
  font-size: 0.88rem;
}

.ml-table-wrapper {
  overflow-x: auto;
}
.ml-table {
  width: 100%;
  border-collapse: collapse;
}
.ml-table th {
  text-align: left;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #64748b;
  padding: 0.85rem 1.5rem;
  border-bottom: 1px solid #1e293b;
}
.ml-table td {
  padding: 0.85rem 1.5rem;
  font-size: 0.9rem;
  color: #e2e8f0;
  border-bottom: 1px solid #1e293b;
}
.ml-table tr:last-child td { border-bottom: none; }

.ml-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.65rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
}
.ml-badge--approved { background: rgba(16, 185, 129, 0.15); color: #6ee7b7; }
.ml-badge--rejected { background: rgba(239, 68, 68, 0.15); color: #fca5a5; }
.ml-badge--pending { background: rgba(245, 158, 11, 0.15); color: #fcd34d; }
.ml-badge--default { background: rgba(148, 163, 184, 0.15); color: #cbd5e1; }
`;

export default function MyLeave() {
  const [leaves, setLeaves] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [form, setForm] = useState({
    startDate: '',
    endDate: '',
    leaveType: 'Casual Leave',
  });

  const user = JSON.parse(localStorage.getItem('user') || 'null');
  const employeeId = user?.id;

  const loadLeaves = async () => {
    if (!employeeId) {
      setError('Unable to identify the logged-in employee.');
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError('');

      const response = await api.get(`/Employees/${employeeId}/leaves`);

      setLeaves(response.data || []);
    } catch (err) {
      console.error('Failed to load leaves:', err);

      setError(
        err.response?.data?.message ||
        'Failed to load your leave records.'
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadLeaves();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;

    setForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const openForm = () => {
    setShowForm(true);
    setError('');
    setSuccess('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    setError('');
    setSuccess('');

    if (!form.startDate || !form.endDate) {
      setError('Please select both start and end dates.');
      return;
    }

    if (form.endDate < form.startDate) {
      setError('End date cannot be before the start date.');
      return;
    }

    if (!employeeId) {
      setError('Unable to identify the logged-in employee.');
      return;
    }

    try {
      setSubmitting(true);

      await api.post(`/Employees/${employeeId}/leaves`, {
        startDate: form.startDate,
        endDate: form.endDate,
        leaveType: form.leaveType,
      });

      setSuccess('Leave application submitted successfully.');

      setForm({
        startDate: '',
        endDate: '',
        leaveType: 'Casual Leave',
      });

      setShowForm(false);

      await loadLeaves();
    } catch (err) {
      console.error('Failed to apply for leave:', err);

      setError(
        err.response?.data?.message ||
        'Failed to submit the leave application.'
      );
    } finally {
      setSubmitting(false);
    }
  };

  const getStatusClass = (status) => {
    switch ((status || '').toLowerCase()) {
      case 'approved':
        return 'ml-badge--approved';
      case 'rejected':
        return 'ml-badge--rejected';
      case 'pending':
        return 'ml-badge--pending';
      default:
        return 'ml-badge--default';
    }
  };

  const formatDate = (date) => {
    if (!date) return '-';

    return new Date(date).toLocaleDateString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  };

  return (
    <div className="ml-page">
      <style>{LEAVE_STYLES}</style>

      <div className="ml-header">
        <div>
          <div className="ml-title-row">
            <CalendarOff size={26} />
            <h1>My Leave</h1>
          </div>
          <p>View your leave history and apply for new leave.</p>
        </div>

        <button className="ml-btn ml-btn--primary" onClick={openForm}>
          <PlusCircle size={18} />
          Apply for Leave
        </button>
      </div>

      {success && (
        <div className="ml-alert ml-alert--success">
          <CheckCircle2 size={16} />
          {success}
        </div>
      )}

      {error && (
        <div className="ml-alert ml-alert--error">
          <AlertCircle size={16} />
          {error}
        </div>
      )}

      {showForm && (
        <div className="ml-card">
          <div className="ml-form-header">
            <div>
              <h2>Apply for Leave</h2>
              <p>Submit a new leave request for approval.</p>
            </div>

            <button
              className="ml-close-btn"
              onClick={() => setShowForm(false)}
              type="button"
              title="Close"
            >
              <X size={20} />
            </button>
          </div>

          <form onSubmit={handleSubmit}>
            <div className="ml-form-grid">
              <div className="ml-field">
                <label htmlFor="startDate">Start Date</label>
                <input
                  id="startDate"
                  name="startDate"
                  type="date"
                  value={form.startDate}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="ml-field">
                <label htmlFor="endDate">End Date</label>
                <input
                  id="endDate"
                  name="endDate"
                  type="date"
                  value={form.endDate}
                  onChange={handleChange}
                  min={form.startDate || undefined}
                  required
                />
              </div>

              <div className="ml-field">
                <label htmlFor="leaveType">Leave Type</label>
                <select
                  id="leaveType"
                  name="leaveType"
                  value={form.leaveType}
                  onChange={handleChange}
                >
                  <option value="Casual Leave">Casual Leave</option>
                  <option value="Sick Leave">Sick Leave</option>
                  <option value="Earned Leave">Earned Leave</option>
                  <option value="Privilege Leave">Privilege Leave</option>
                  <option value="Other">Other</option>
                </select>
              </div>
            </div>

            <div className="ml-form-actions">
              <button
                type="button"
                className="ml-btn ml-btn--secondary"
                onClick={() => setShowForm(false)}
                disabled={submitting}
              >
                Cancel
              </button>

              <button
                type="submit"
                className="ml-btn ml-btn--primary"
                disabled={submitting}
              >
                {submitting ? 'Submitting...' : 'Submit Leave Request'}
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="ml-card">
        <div className="ml-section-header">
          <div>
            <h2>Leave History</h2>
            <p>Your submitted leave requests</p>
          </div>

          <button
            className="ml-btn--icon"
            onClick={loadLeaves}
            disabled={loading}
            title="Refresh"
          >
            <RefreshCw size={17} className={loading ? 'ml-spin' : ''} />
          </button>
        </div>

        {loading ? (
          <div className="ml-empty-state">
            <p>Loading your leave records...</p>
          </div>
        ) : leaves.length === 0 ? (
          <div className="ml-empty-state">
            <CalendarOff size={40} />
            <h3>No leave records</h3>
            <p>You haven't submitted any leave requests yet.</p>
          </div>
        ) : (
          <div className="ml-table-wrapper">
            <table className="ml-table">
              <thead>
                <tr>
                  <th>Start Date</th>
                  <th>End Date</th>
                  <th>Leave Type</th>
                  <th>Status</th>
                </tr>
              </thead>

              <tbody>
                {leaves.map((leave) => (
                  <tr key={leave.id}>
                    <td>{formatDate(leave.startDate)}</td>
                    <td>{formatDate(leave.endDate)}</td>
                    <td>{leave.leaveType || '-'}</td>
                    <td>
                      <span className={`ml-badge ${getStatusClass(leave.status)}`}>
                        {leave.status || 'Unknown'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}