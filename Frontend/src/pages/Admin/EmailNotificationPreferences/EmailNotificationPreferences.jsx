// pages/Admin/EmailNotificationPreferences/EmailNotificationPreferences.jsx
import React, { useState, useEffect, useCallback } from 'react';
import { Mail, X, Save, Bell, Search } from 'lucide-react';
import api from '../../../api/axios';

const EVENT_FLAGS = [
  { key: 'assignment', label: 'Assignment', hint: 'Ticket assigned or transferred to you' },
  { key: 'acceptance', label: 'Acceptance', hint: 'Acceptance pending / accepted' },
  { key: 'rejection', label: 'Rejection', hint: 'Ticket rejected' },
  { key: 'expiry', label: 'Acceptance Expiry', hint: 'Acceptance window expired' },
  { key: 'reassignment', label: 'Reassignment', hint: 'Ticket reassigned / fallback assigned' },
  { key: 'statusChange', label: 'Status Change', hint: 'In progress, pending, priority or department change' },
  { key: 'closure', label: 'Closure', hint: 'Ticket pending closure or closed' },
  { key: 'reopen', label: 'Reopen', hint: 'Ticket reopened' }
];

const EMPTY_PREFERENCE = {
  receiveAll: false,
  assignment: false,
  acceptance: false,
  rejection: false,
  expiry: false,
  reassignment: false,
  statusChange: false,
  closure: false,
  reopen: false
};

export default function EmailNotificationPreferences() {
  const [employees, setEmployees] = useState([]);
  const [loadingEmployees, setLoadingEmployees] = useState(true);
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const [selectedEmployee, setSelectedEmployee] = useState(null);
  const [preference, setPreference] = useState(null);
  const [loadingPreference, setLoadingPreference] = useState(false);
  const [saving, setSaving] = useState(false);

  const loadEmployees = useCallback(async () => {
    setLoadingEmployees(true);
    setError('');
    try {
      const res = await api.get('/employees', { params: { pageSize: 200 } });
      setEmployees(res.data.items || res.data || []);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load employees.');
    } finally {
      setLoadingEmployees(false);
    }
  }, []);

  useEffect(() => {
    loadEmployees();
  }, [loadEmployees]);

  const openPreferences = async (emp) => {
    setSelectedEmployee(emp);
    setPreference(null);
    setLoadingPreference(true);
    setError('');
    try {
      const res = await api.get(`/employees/${emp.id}/email-notification-preferences`);
      const p = res.data;
      setPreference({
        receiveAll: !!p.receiveAll,
        assignment: !!p.assignment,
        acceptance: !!p.acceptance,
        rejection: !!p.rejection,
        expiry: !!p.expiry,
        reassignment: !!p.reassignment,
        statusChange: !!p.statusChange,
        closure: !!p.closure,
        reopen: !!p.reopen
      });
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load preferences.');
      setPreference({ ...EMPTY_PREFERENCE });
    } finally {
      setLoadingPreference(false);
    }
  };

  const closeModal = () => {
    setSelectedEmployee(null);
    setPreference(null);
  };

  const toggleFlag = (key) => {
    setPreference((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  const toggleReceiveAll = () => {
    setPreference((prev) => ({ ...prev, receiveAll: !prev.receiveAll }));
  };

  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      await api.put(`/employees/${selectedEmployee.id}/email-notification-preferences`, {
        receiveAll: preference.receiveAll,
        assignment: preference.assignment,
        acceptance: preference.acceptance,
        rejection: preference.rejection,
        expiry: preference.expiry,
        reassignment: preference.reassignment,
        statusChange: preference.statusChange,
        closure: preference.closure,
        reopen: preference.reopen
      });
      setMessage(`Notification preferences saved for ${selectedEmployee.name}.`);
      closeModal();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save preferences.');
    } finally {
      setSaving(false);
    }
  };

  const filtered = employees.filter((emp) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (emp.name || '').toLowerCase().includes(q) || (emp.email || '').toLowerCase().includes(q);
  });

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Bell size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Email Notification Preferences</h1>
          <p>Control which ticket event emails each employee receives.</p>
        </div>
      </div>

      {message && (
        <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem' }}>
          {message}
        </div>
      )}

      {error && (
        <div style={{ padding: '0.75rem 1rem', background: 'rgba(239,68,68,0.15)', border: '1px solid rgba(239,68,68,0.3)', borderRadius: '8px', color: '#f87171', marginBottom: '1.25rem', fontSize: '0.85rem' }}>
          {error}
        </div>
      )}

      <div className="glass-card filter-bar">
        <div className="search-input">
          <Search size={16} />
          <input placeholder="Search employees by name or email..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Full Name</th>
                <th>Email Address</th>
                <th>Department</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {loadingEmployees ? (
                <tr><td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>Loading employees...</td></tr>
              ) : filtered.length === 0 ? (
                <tr><td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>No employees found.</td></tr>
              ) : (
                filtered.map((emp) => (
                  <tr key={emp.id} onClick={() => openPreferences(emp)} style={{ cursor: 'pointer' }}>
                    <td><strong>{emp.name}</strong>{emp.designation && <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>{emp.designation}</div>}</td>
                    <td>{emp.email}</td>
                    <td>{emp.departmentName || '—'}</td>
                    <td style={{ textAlign: 'right', color: 'var(--text-dim)', fontSize: '0.8rem' }}>Manage →</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {selectedEmployee && (
        <div
          style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.6)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000, padding: '2rem 1rem' }}
          onClick={closeModal}
        >
          <div
            className="glass-card"
            style={{ width: '520px', maxWidth: '90vw', padding: '1.5rem', position: 'relative' }}
            onClick={(e) => e.stopPropagation()}
          >
            <button
              onClick={closeModal}
              style={{ position: 'absolute', top: '1rem', right: '1rem', background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}
            >
              <X size={20} />
            </button>

            <h3 style={{ color: 'white', fontSize: '1.15rem', margin: '0 0 0.25rem 0', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Mail size={18} /> {selectedEmployee.name}
            </h3>
            <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem', margin: '0 0 1.25rem 0' }}>{selectedEmployee.email}</p>

            {loadingPreference || !preference ? (
              <p style={{ color: 'var(--text-dim)' }}>Loading preferences...</p>
            ) : (
              <>
                <label
                  style={{
                    display: 'flex', alignItems: 'center', gap: '10px', padding: '0.75rem',
                    background: 'rgba(99,102,241,0.12)', border: '1px solid rgba(99,102,241,0.3)',
                    borderRadius: '8px', marginBottom: '1rem', cursor: 'pointer'
                  }}
                >
                  <input type="checkbox" checked={preference.receiveAll} onChange={toggleReceiveAll} />
                  <span style={{ color: 'white', fontWeight: 600 }}>Receive All Notifications</span>
                </label>

                <div style={{ display: 'grid', gap: '0.6rem', opacity: preference.receiveAll ? 0.5 : 1 }}>
                  {EVENT_FLAGS.map((flag) => (
                    <label
                      key={flag.key}
                      style={{ display: 'flex', alignItems: 'flex-start', gap: '10px', cursor: preference.receiveAll ? 'not-allowed' : 'pointer' }}
                    >
                      <input
                        type="checkbox"
                        checked={preference.receiveAll || preference[flag.key]}
                        disabled={preference.receiveAll}
                        onChange={() => toggleFlag(flag.key)}
                        style={{ marginTop: '3px' }}
                      />
                      <span>
                        <div style={{ color: '#e2e8f0', fontSize: '0.9rem' }}>{flag.label}</div>
                        <div style={{ color: '#64748b', fontSize: '0.75rem' }}>{flag.hint}</div>
                      </span>
                    </label>
                  ))}
                </div>

                <button
                  className="btn btn--primary"
                  style={{ marginTop: '1.5rem', width: '100%' }}
                  onClick={handleSave}
                  disabled={saving}
                >
                  <Save size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />
                  {saving ? 'Saving...' : 'Save Preferences'}
                </button>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}