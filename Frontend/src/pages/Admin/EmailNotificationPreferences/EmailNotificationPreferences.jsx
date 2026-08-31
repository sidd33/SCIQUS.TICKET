import React, { useState, useEffect, useCallback } from 'react';
import { Mail, X, Save, Bell, Search, Users, Settings, UserCheck } from 'lucide-react';
import api from '../../../api/axios';
 
const EVENT_FLAGS = [
  { key: 'assignment', category: 'Assignment', label: 'Assignment', hint: 'Ticket assigned or transferred to you' },
  { key: 'acceptance', category: 'Acceptance', label: 'Acceptance', hint: 'Acceptance pending / accepted' },
  { key: 'rejection', category: 'Rejection', label: 'Rejection', hint: 'Ticket rejected' },
  { key: 'expiry', category: 'Expiry', label: 'Acceptance Expiry', hint: 'Acceptance window expired' },
  { key: 'reassignment', category: 'Reassignment', label: 'Reassignment', hint: 'Ticket reassigned / fallback assigned' },
  { key: 'statusChange', category: 'StatusChange', label: 'Status Change', hint: 'In progress, pending, priority or department change' },
  { key: 'closure', category: 'Closure', label: 'Closure', hint: 'Ticket pending closure or closed' },
  { key: 'reopen', category: 'Reopen', label: 'Reopen', hint: 'Ticket reopened' }
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
 
const TABS = [
  { key: 'perEmployee', label: 'Per Employee', icon: Mail },
  { key: 'bulkByCategory', label: 'Bulk by Category', icon: Users },
  { key: 'defaultTemplate', label: 'Default Template', icon: Settings },
  { key: 'customer', label: 'Customer Preferences', icon: UserCheck }
];
 
const alertBoxStyle = (kind) => ({
  padding: '0.75rem 1rem',
  background: kind === 'error' ? 'rgba(239,68,68,0.15)' : 'rgba(16,185,129,0.15)',
  border: `1px solid ${kind === 'error' ? 'rgba(239,68,68,0.3)' : 'rgba(16,185,129,0.3)'}`,
  borderRadius: '8px',
  color: kind === 'error' ? '#f87171' : '#34d399',
  marginBottom: '1.25rem',
  fontSize: '0.85rem'
});
 
export default function EmailNotificationPreferences() {
  const [activeTab, setActiveTab] = useState('perEmployee');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
 
  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Bell size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Email Notification Preferences</h1>
          <p>Control which ticket event emails employees and customers receive.</p>
        </div>
      </div>
 
      {message && <div style={alertBoxStyle('success')}>{message}</div>}
      {error && <div style={alertBoxStyle('error')}>{error}</div>}
 
      <div className="glass-card" style={{ display: 'flex', gap: '0.5rem', padding: '0.5rem', marginBottom: '1.25rem' }}>
        {TABS.map((tab) => {
          const Icon = tab.icon;
          const active = activeTab === tab.key;
          return (
            <button
              key={tab.key}
              onClick={() => { setActiveTab(tab.key); setError(''); setMessage(''); }}
              className={active ? 'btn btn--primary' : 'btn'}
              style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem' }}
            >
              <Icon size={15} /> {tab.label}
            </button>
          );
        })}
      </div>
 
      {activeTab === 'perEmployee' && (
        <PerEmployeeTab setError={setError} setMessage={setMessage} />
      )}
 
      {activeTab === 'bulkByCategory' && (
        <BulkByCategoryTab setError={setError} setMessage={setMessage} />
      )}
 
      {activeTab === 'defaultTemplate' && (
        <DefaultTemplateTab setError={setError} setMessage={setMessage} />
      )}
 
      {activeTab === 'customer' && (
        <CustomerPreferencesTab setError={setError} setMessage={setMessage} />
      )}
    </div>
  );
}
 
// ============================================================
// TAB 1: PER EMPLOYEE (your original component, unchanged logic)
// ============================================================
 
function PerEmployeeTab({ setError, setMessage }) {
  const [employees, setEmployees] = useState([]);
  const [loadingEmployees, setLoadingEmployees] = useState(true);
  const [search, setSearch] = useState('');
 
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
  }, [setError]);
 
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
      await api.put(`/employees/${selectedEmployee.id}/email-notification-preferences`, preference);
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
    <>
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
          <div className="glass-card" style={{ width: '520px', maxWidth: '90vw', padding: '1.5rem', position: 'relative' }} onClick={(e) => e.stopPropagation()}>
            <button onClick={closeModal} style={{ position: 'absolute', top: '1rem', right: '1rem', background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}>
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
                <label style={{ display: 'flex', alignItems: 'center', gap: '10px', padding: '0.75rem', background: 'rgba(99,102,241,0.12)', border: '1px solid rgba(99,102,241,0.3)', borderRadius: '8px', marginBottom: '1rem', cursor: 'pointer' }}>
                  <input type="checkbox" checked={preference.receiveAll} onChange={toggleReceiveAll} />
                  <span style={{ color: 'white', fontWeight: 600 }}>Receive All Notifications</span>
                </label>
 
                <div style={{ display: 'grid', gap: '0.6rem', opacity: preference.receiveAll ? 0.5 : 1 }}>
                  {EVENT_FLAGS.map((flag) => (
                    <label key={flag.key} style={{ display: 'flex', alignItems: 'flex-start', gap: '10px', cursor: preference.receiveAll ? 'not-allowed' : 'pointer' }}>
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
 
                <button className="btn btn--primary" style={{ marginTop: '1.5rem', width: '100%' }} onClick={handleSave} disabled={saving}>
                  <Save size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />
                  {saving ? 'Saving...' : 'Save Preferences'}
                </button>
              </>
            )}
          </div>
        </div>
      )}
    </>
  );
}
 
// ============================================================
// TAB 2: BULK BY CATEGORY
// Pick a category, tick which employees get it — no need to
// open 20 individual records one at a time.
// ============================================================
 
function BulkByCategoryTab({ setError, setMessage }) {
  const [category, setCategory] = useState(EVENT_FLAGS[0].category);
  const [employees, setEmployees] = useState([]);
  const [selectedIds, setSelectedIds] = useState(new Set());
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
 
  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [empRes, selectedRes] = await Promise.all([
        api.get('/employees', { params: { pageSize: 200 } }),
        api.get(`/notificationpreferences/employees/bulk-by-category/${category}`)
      ]);
      setEmployees(empRes.data.items || empRes.data || []);
      setSelectedIds(new Set(selectedRes.data.employeeIds || []));
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load bulk selection data.');
    } finally {
      setLoading(false);
    }
  }, [category, setError]);
 
  useEffect(() => {
    loadData();
  }, [loadData]);
 
  const toggleEmployee = (id) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };
 
  const selectAll = () => setSelectedIds(new Set(filtered.map((e) => e.id)));
  const clearAll = () => setSelectedIds(new Set());
 
  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      const res = await api.put(`/notificationpreferences/employees/bulk-by-category/${category}`, {
        employeeIds: Array.from(selectedIds)
      });
      setMessage(`${res.data.selectedCount} employee(s) will now receive "${category}" emails.`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save bulk selection.');
    } finally {
      setSaving(false);
    }
  };
 
  const filtered = employees.filter((emp) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (emp.name || '').toLowerCase().includes(q) || (emp.email || '').toLowerCase().includes(q);
  });
 
  const activeFlag = EVENT_FLAGS.find((f) => f.category === category);
 
  return (
    <>
      <div className="glass-card filter-bar" style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'center' }}>
        <div>
          <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-dim)', marginBottom: '4px' }}>Notification Category</label>
          <select
  value={category}
  onChange={(e) => setCategory(e.target.value)}
  style={{
    padding: '0.5rem 0.75rem',
    borderRadius: '6px',
    background: 'rgba(255,255,255,0.05)',
    border: '1px solid rgba(255,255,255,0.1)',
    color: 'white',
    colorScheme: 'dark'
  }}
>
  {EVENT_FLAGS.map((f) => (
    <option key={f.category} value={f.category} style={{ background: '#1e293b', color: 'white' }}>
      {f.label}
    </option>
  ))}
</select>
        </div>
 
        <div className="search-input" style={{ flex: 1, minWidth: '220px' }}>
          <Search size={16} />
          <input placeholder="Search employees..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
 
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className="btn" onClick={selectAll}>Select All</button>
          <button className="btn" onClick={clearAll}>Clear</button>
        </div>
      </div>
 
      {activeFlag && (
        <p style={{ color: 'var(--text-dim)', fontSize: '0.8rem', margin: '0 0 1rem 0' }}>{activeFlag.hint}</p>
      )}
 
      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th style={{ width: '40px' }}></th>
                <th>Full Name</th>
                <th>Email Address</th>
                <th>Department</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>Loading...</td></tr>
              ) : filtered.length === 0 ? (
                <tr><td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>No employees found.</td></tr>
              ) : (
                filtered.map((emp) => (
                  <tr key={emp.id} onClick={() => toggleEmployee(emp.id)} style={{ cursor: 'pointer' }}>
                    <td>
                      <input type="checkbox" checked={selectedIds.has(emp.id)} onChange={() => toggleEmployee(emp.id)} onClick={(e) => e.stopPropagation()} />
                    </td>
                    <td><strong>{emp.name}</strong></td>
                    <td>{emp.email}</td>
                    <td>{emp.departmentName || '—'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
 
      <div style={{ marginTop: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <span style={{ color: 'var(--text-dim)', fontSize: '0.85rem' }}>{selectedIds.size} of {filtered.length} selected</span>
        <button className="btn btn--primary" onClick={handleSave} disabled={saving || loading}>
          <Save size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />
          {saving ? 'Saving...' : `Save "${activeFlag?.label}" Selection`}
        </button>
      </div>
    </>
  );
}
 
// ============================================================
// TAB 3: DEFAULT TEMPLATE
// Single template used to seed preferences for new employees.
// ============================================================
 
function DefaultTemplateTab({ setError, setMessage }) {
  const [template, setTemplate] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
 
  const loadTemplate = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await api.get('/notificationpreferences/default-template');
      const t = res.data;
      setTemplate({
        receiveAll: !!t.receiveAll,
        assignment: !!t.assignment,
        acceptance: !!t.acceptance,
        rejection: !!t.rejection,
        expiry: !!t.expiry,
        reassignment: !!t.reassignment,
        statusChange: !!t.statusChange,
        closure: !!t.closure,
        reopen: !!t.reopen
      });
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load default template.');
      setTemplate({ ...EMPTY_PREFERENCE });
    } finally {
      setLoading(false);
    }
  }, [setError]);
 
  useEffect(() => {
    loadTemplate();
  }, [loadTemplate]);
 
  const toggleFlag = (key) => setTemplate((prev) => ({ ...prev, [key]: !prev[key] }));
  const toggleReceiveAll = () => setTemplate((prev) => ({ ...prev, receiveAll: !prev.receiveAll }));
 
  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      await api.put('/notificationpreferences/default-template', template);
      setMessage('Default preference template saved. This applies to newly created employees only.');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save default template.');
    } finally {
      setSaving(false);
    }
  };
 
  if (loading || !template) {
    return <div className="glass-card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-dim)' }}>Loading...</div>;
  }
 
  return (
    <div className="glass-card" style={{ padding: '1.5rem', maxWidth: '560px' }}>
      <h3 style={{ color: 'white', fontSize: '1.05rem', margin: '0 0 0.25rem 0' }}>Default Notification Preferences</h3>
      <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem', margin: '0 0 1.25rem 0' }}>
        Used to seed preferences when a new employee is created. Editing this does not change existing employees' preferences.
      </p>
 
      <label style={{ display: 'flex', alignItems: 'center', gap: '10px', padding: '0.75rem', background: 'rgba(99,102,241,0.12)', border: '1px solid rgba(99,102,241,0.3)', borderRadius: '8px', marginBottom: '1rem', cursor: 'pointer' }}>
        <input type="checkbox" checked={template.receiveAll} onChange={toggleReceiveAll} />
        <span style={{ color: 'white', fontWeight: 600 }}>Receive All Notifications (default)</span>
      </label>
 
      <div style={{ display: 'grid', gap: '0.6rem', opacity: template.receiveAll ? 0.5 : 1 }}>
        {EVENT_FLAGS.map((flag) => (
          <label key={flag.key} style={{ display: 'flex', alignItems: 'flex-start', gap: '10px', cursor: template.receiveAll ? 'not-allowed' : 'pointer' }}>
            <input
              type="checkbox"
              checked={template.receiveAll || template[flag.key]}
              disabled={template.receiveAll}
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
 
      <button className="btn btn--primary" style={{ marginTop: '1.5rem', width: '100%' }} onClick={handleSave} disabled={saving}>
        <Save size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />
        {saving ? 'Saving...' : 'Save Default Template'}
      </button>
    </div>
  );
}
 
// ============================================================
// TAB 4: CUSTOMER PREFERENCES
// Global — same setting applies to every customer.
// ============================================================
 
function CustomerPreferencesTab({ setError, setMessage }) {
  const [prefs, setPrefs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [savingCategory, setSavingCategory] = useState(null);
 
  const loadPrefs = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await api.get('/notificationpreferences/customer');
      setPrefs(res.data || []);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load customer preferences.');
    } finally {
      setLoading(false);
    }
  }, [setError]);
 
  useEffect(() => {
    loadPrefs();
  }, [loadPrefs]);
 
  const toggleCategory = async (category, currentValue) => {
    setSavingCategory(category);
    setError('');
    try {
      await api.put(`/notificationpreferences/customer/${category}`, { isEnabled: !currentValue });
 
      // Upsert locally: the backend creates a row for this category the
      // first time it's toggled, but our initial GET only returns rows
      // that already existed. If we only .map() here, a category with no
      // existing row never gets reflected in the UI even though the save
      // succeeded (checkbox stays "Disabled" forever after the first toggle).
      setPrefs((prev) => {
        const exists = prev.some((p) => p.category === category);
        if (exists) {
          return prev.map((p) =>
            p.category === category ? { ...p, isEnabled: !currentValue } : p
          );
        }
        return [...prev, { category, isEnabled: !currentValue }];
      });
 
      setMessage(`Customer emails for "${category}" ${!currentValue ? 'enabled' : 'disabled'}.`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update customer preference.');
    } finally {
      setSavingCategory(null);
    }
  };
 
  if (loading) {
    return <div className="glass-card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-dim)' }}>Loading...</div>;
  }
 
  return (
    <div className="glass-card" style={{ padding: '1.5rem', maxWidth: '560px' }}>
      <h3 style={{ color: 'white', fontSize: '1.05rem', margin: '0 0 0.25rem 0' }}>Customer Notification Preferences</h3>
      <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem', margin: '0 0 1.25rem 0' }}>
        These settings apply to every customer — there is no per-customer configuration.
      </p>
 
      <div style={{ display: 'grid', gap: '0.75rem' }}>
        {EVENT_FLAGS.map((flag) => {
          const pref = prefs.find((p) => p.category === flag.category);
          const isEnabled = pref?.isEnabled || false;
          const isSaving = savingCategory === flag.category;
 
          return (
            <div
              key={flag.category}
              style={{
                display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                padding: '0.75rem', border: '1px solid rgba(255,255,255,0.08)', borderRadius: '8px'
              }}
            >
              <div>
                <div style={{ color: '#e2e8f0', fontSize: '0.9rem' }}>{flag.label}</div>
                <div style={{ color: '#64748b', fontSize: '0.75rem' }}>{flag.hint}</div>
              </div>
              <label style={{ display: 'flex', alignItems: 'center', gap: '8px', cursor: isSaving ? 'not-allowed' : 'pointer' }}>
                <input
                  type="checkbox"
                  checked={isEnabled}
                  disabled={isSaving}
                  onChange={() => toggleCategory(flag.category, isEnabled)}
                />
                <span style={{ fontSize: '0.8rem', color: isEnabled ? '#34d399' : 'var(--text-dim)' }}>
                  {isSaving ? 'Saving...' : isEnabled ? 'Enabled' : 'Disabled'}
                </span>
              </label>
            </div>
          );
        })}
      </div>
    </div>
  );
}
 