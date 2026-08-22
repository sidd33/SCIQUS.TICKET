import React, { useState, useEffect, useCallback } from 'react';
import { Users, Plus, Search, Edit2, Trash2, Clock, CalendarOff, X } from 'lucide-react';
import api from '../../api/axios';

const EMPTY_EMPLOYEE = {
  name: '',
  email: '',
  password: '',
  registeredMobileNumber: '',
  secondMobileNumber: '',
  employeeId: '',
  designation: '',
  reportsTo: '',
  departmentId: '',
  gradeId: '',
  profileImageUrl: ''
};

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
const EMPTY_WORKING_HOUR = { daysOfWeek: [], startTime: '09:00:00', endTime: '18:00:00', isWorkingDay: true };
const WEEKDAYS = [1, 2, 3, 4, 5];
const EMPTY_LEAVE = { startDate: '', endDate: '', leaveType: 'Casual' };

// Scoped fallback styling so form fields render correctly (label above input,
// proper spacing) even if the global .field / .modal-body rules are missing
// or mis-defined elsewhere in the app's stylesheet.
const FIELD_STYLES = `
.emp-field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
  margin-bottom: 1rem;
}
.emp-field label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--text-dim, #94a3b8);
}
.emp-field input,
.emp-field select {
  width: 100%;
  box-sizing: border-box;
  padding: 0.5rem 0.65rem;
  border-radius: 6px;
  border: 1px solid #334155;
  background: #0f172a;
  color: inherit;
}
.emp-field-hint {
  font-size: 0.75rem;
  color: var(--text-dim, #94a3b8);
  margin-top: 0.15rem;
}
.emp-checkbox-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
}
.emp-modal-backdrop {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 1rem;
  overflow-y: auto;
}
.emp-modal {
  display: flex;
  flex-direction: column;
  max-height: 100%;
  overflow: hidden;
}
.emp-modal-body-scroll {
  overflow-y: auto;
  flex: 1 1 auto;
}
.emp-form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0 1.25rem;
}
.emp-field-full {
  grid-column: 1 / -1;
}
@media (max-width: 560px) {
  .emp-form-grid {
    grid-template-columns: 1fr;
  }
  .emp-field-full {
    grid-column: auto;
  }
}
.emp-day-picker {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.emp-day-chip {
  padding: 0.4rem 0.75rem;
  border-radius: 999px;
  border: 1px solid #334155;
  background: #0f172a;
  color: var(--text-dim, #94a3b8);
  font-size: 0.8rem;
  cursor: pointer;
  user-select: none;
}
.emp-day-chip.active {
  background: #4f46e5;
  border-color: #4f46e5;
  color: #fff;
}
.emp-day-shortcuts {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
}
.emp-day-shortcuts button {
  font-size: 0.75rem;
  padding: 0.25rem 0.6rem;
  border-radius: 6px;
  border: 1px solid #334155;
  background: transparent;
  color: var(--text-dim, #94a3b8);
  cursor: pointer;
}
`;

export default function Employees() {
  const [employees, setEmployees] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [grades, setGrades] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');

  const [empModal, setEmpModal] = useState(null); // { mode: 'create'|'edit', id, data }
  const [saving, setSaving] = useState(false);

  const [scheduleModal, setScheduleModal] = useState(null); // { employee, tab: 'hours'|'leave' }
  const [workingHours, setWorkingHours] = useState([]);
  const [leaves, setLeaves] = useState([]);
  const [whForm, setWhForm] = useState(null); // { mode, id, data }
  const [leaveForm, setLeaveForm] = useState(null); // { mode, id, data }

  const loadEmployees = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.get('/employees', { params: { pageSize: 100 } });
      setEmployees(res.data.items || res.data || []);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load employees.');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadSupportingData = useCallback(async () => {
    try {
      const [dRes, gRes] = await Promise.all([
        api.get('/departments', { params: { pageSize: 100 } }),
        api.get('/grades', { params: { pageSize: 100 } })
      ]);
      setDepartments(dRes.data.items || dRes.data || []);
      setGrades(gRes.data.items || gRes.data || []);
    } catch {
      // fallback
    }
  }, []);

  useEffect(() => {
    loadEmployees();
    loadSupportingData();
  }, [loadEmployees, loadSupportingData]);

  const filtered = employees.filter((e) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (
      (e.name || '').toLowerCase().includes(q) ||
      (e.email || '').toLowerCase().includes(q) ||
      (e.departmentName || '').toLowerCase().includes(q)
    );
  });

  // ── Employee CRUD ────────────────────────────────────────────────

  function openCreateEmployee() {
    setError('');
    setEmpModal({ mode: 'create', data: { ...EMPTY_EMPLOYEE } });
  }

  function openEditEmployee(emp) {
    setError('');
    setEmpModal({
      mode: 'edit',
      id: emp.id,
      data: {
        name: emp.name || '',
        registeredMobileNumber: emp.registeredMobileNumber || '',
        secondMobileNumber: emp.secondMobileNumber || '',
        designation: emp.designation || '',
        reportsTo: emp.reportsTo || '',
        departmentId: emp.departmentId || '',
        gradeId: emp.gradeId || '',
        profileImageUrl: emp.profileImageUrl || ''
      }
    });
  }

  async function handleSaveEmployee(e) {
  e.preventDefault();
  setSaving(true);
  setError('');

  try {
    if (empModal.mode === 'create') {
      const body = {
        ...empModal.data,
        gradeId: empModal.data.gradeId || null,
        reportsTo: empModal.data.reportsTo || null,
        secondMobileNumber: empModal.data.secondMobileNumber || null,
        profileImageUrl: empModal.data.profileImageUrl || null
      };

      await api.post('/employees', body);
    } else {
      await api.put(`/employees/${empModal.id}`, {
        ...empModal.data,
        gradeId: empModal.data.gradeId || null,
        reportsTo: empModal.data.reportsTo || null,
        secondMobileNumber: empModal.data.secondMobileNumber || null,
        profileImageUrl: empModal.data.profileImageUrl || null
      });
    }

    setEmpModal(null);
    await loadEmployees();
  } catch (err) {
    setError(
      err.response?.data?.message ||
      err.response?.data?.errors?.request?.[0] ||
      err.message ||
      'Save failed.'
    );
  } finally {
    setSaving(false);
  }
}

  async function handleDeleteEmployee(emp) {
    if (!window.confirm(`Remove "${emp.name}" from the roster? This cannot be undone.`)) return;
    try {
      await api.delete(`/employees/${emp.id}`);
      await loadEmployees();
    } catch (err) {
      alert(err.response?.data?.message || 'Delete failed.');
    }
  }

  // ── Schedule (Working Hours + Leave) ────────────────────────────

  async function openSchedule(emp) {
    setScheduleModal({ employee: emp, tab: 'hours' });
    setWhForm(null);
    setLeaveForm(null);
    try {
      const [whRes, lvRes] = await Promise.all([
        api.get(`/employees/${emp.id}/working-hours`),
        api.get(`/employees/${emp.id}/leaves`)
      ]);
      setWorkingHours(whRes.data || []);
      setLeaves(lvRes.data || []);
    } catch {
      setWorkingHours([]);
      setLeaves([]);
    }
  }

  function isOnLeaveToday(employeeLeaves) {
    const today = new Date().toISOString().slice(0, 10);
    return employeeLeaves.some((l) => {
      if (l.isDeleted) return false;
      if (l.status && !['Approved', 'approved'].includes(l.status)) return false;
      const start = (l.startDate || '').slice(0, 10);
      const end = (l.endDate || '').slice(0, 10);
      return start <= today && today <= end;
    });
  }

  async function refreshSchedule() {
    if (!scheduleModal) return;
    const id = scheduleModal.employee.id;
    const [whRes, lvRes] = await Promise.all([
      api.get(`/employees/${id}/working-hours`),
      api.get(`/employees/${id}/leaves`)
    ]);
    setWorkingHours(whRes.data || []);
    setLeaves(lvRes.data || []);
  }

  // Working hours
  function openCreateWh() {
    setWhForm({ mode: 'create', data: { ...EMPTY_WORKING_HOUR } });
  }
  function openEditWh(wh) {
    setWhForm({ mode: 'edit', id: wh.id, data: { daysOfWeek: [wh.dayOfWeek], startTime: wh.startTime, endTime: wh.endTime, isWorkingDay: wh.isWorkingDay } });
  }
  function toggleWhDay(day) {
    setWhForm((prev) => {
      const has = prev.data.daysOfWeek.includes(day);
      const daysOfWeek = has ? prev.data.daysOfWeek.filter((d) => d !== day) : [...prev.data.daysOfWeek, day];
      return { ...prev, data: { ...prev.data, daysOfWeek } };
    });
  }
  function setWhDays(days) {
    setWhForm((prev) => ({ ...prev, data: { ...prev.data, daysOfWeek: days } }));
  }
  async function handleSaveWh(e) {
    e.preventDefault();
    const empId = scheduleModal.employee.id;
    const { daysOfWeek, startTime, endTime, isWorkingDay } = whForm.data;
    if (!daysOfWeek.length) {
      alert('Select at least one day.');
      return;
    }
    try {
      if (whForm.mode === 'create') {
        await Promise.all(
          daysOfWeek.map((dayOfWeek) => api.post(`/employees/${empId}/working-hours`, { dayOfWeek, startTime, endTime, isWorkingDay }))
        );
      } else {
        await api.put(`/employees/${empId}/working-hours/${whForm.id}`, { dayOfWeek: daysOfWeek[0], startTime, endTime, isWorkingDay });
      }
      setWhForm(null);
      await refreshSchedule();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to save working hours.');
    }
  }
  async function handleDeleteWh(wh) {
    if (!window.confirm('Remove this working hour entry?')) return;
    try {
      await api.delete(`/employees/${scheduleModal.employee.id}/working-hours/${wh.id}`);
      await refreshSchedule();
    } catch (err) {
      alert(err.response?.data?.message || 'Delete failed.');
    }
  }

  // Leave
  function openCreateLeave() {
    setLeaveForm({ mode: 'create', data: { ...EMPTY_LEAVE } });
  }
  function openEditLeave(lv) {
    setLeaveForm({
      mode: 'edit',
      id: lv.id,
      data: { startDate: (lv.startDate || '').slice(0, 10), endDate: (lv.endDate || '').slice(0, 10), leaveType: lv.leaveType || '', status: lv.status || 'Pending' }
    });
  }
  async function handleSaveLeave(e) {
    e.preventDefault();
    const empId = scheduleModal.employee.id;
    try {
      if (leaveForm.mode === 'create') {
        await api.post(`/employees/${empId}/leaves`, leaveForm.data);
      } else {
        await api.put(`/employees/${empId}/leaves/${leaveForm.id}`, leaveForm.data);
      }
      setLeaveForm(null);
      await refreshSchedule();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to save leave.');
    }
  }
  async function handleDeleteLeave(lv) {
    if (!window.confirm('Remove this leave entry?')) return;
    try {
      await api.delete(`/employees/${scheduleModal.employee.id}/leaves/${lv.id}`);
      await refreshSchedule();
    } catch (err) {
      alert(err.response?.data?.message || 'Delete failed.');
    }
  }

  // ── Render ───────────────────────────────────────────────────────

  return (
    <div className="tickets-page">
      <style>{FIELD_STYLES}</style>

      <div className="page-header">
        <div>
          <h1><Users size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Employee Roster & Scheduling</h1>
          <p>Manage employee profiles, department assignments, working hours, and leave.</p>
        </div>
        <button className="btn btn--primary" onClick={openCreateEmployee}>
          <Plus size={16} /> Add Employee
        </button>
      </div>

      <div className="glass-card filter-bar">
        <div className="search-input">
          <Search size={16} />
          <input placeholder="Search employees by name, email, department..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>

      {error && <div className="tickets-error">{error}</div>}

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Employee Code</th>
                <th>Full Name</th>
                <th>Email Address</th>
                <th>Department</th>
                <th>Grade</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>Loading workforce roster...</td></tr>
              ) : filtered.length === 0 ? (
                <tr><td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>No employee records found.</td></tr>
              ) : (
                filtered.map((emp) => (
                  <tr key={emp.id}>
                    <td><code>{emp.employeeId || '—'}</code></td>
                    <td><strong>{emp.name}</strong>{emp.designation && <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>{emp.designation}</div>}</td>
                    <td>{emp.email}</td>
                    <td>{emp.departmentName || '—'}</td>
                    <td>{emp.gradeLevel ? `Level ${emp.gradeLevel}` : '—'}</td>
                    <td>
                      <div className="row-actions">
                        <button className="icon-btn" title="Manage Schedule" onClick={() => openSchedule(emp)}><Clock size={14} /></button>
                        <button className="icon-btn" title="Edit" onClick={() => openEditEmployee(emp)}><Edit2 size={14} /></button>
                        <button className="icon-btn" title="Delete" onClick={() => handleDeleteEmployee(emp)}><Trash2 size={14} /></button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create / Edit Employee Modal */}
      {empModal && (
        <div className="modal-backdrop emp-modal-backdrop" onClick={() => setEmpModal(null)}>
          <div className="modal emp-modal" style={{ maxWidth: '700px' }} onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{empModal.mode === 'create' ? 'Add Employee' : 'Edit Employee'}</h2>
              <button className="modal-close" onClick={() => setEmpModal(null)}><X size={18} /></button>
            </div>
            <form onSubmit={handleSaveEmployee} style={{ display: 'flex', flexDirection: 'column', minHeight: 0, flex: '1 1 auto' }}>
              <div className="modal-body emp-modal-body-scroll emp-form-grid">
                {error && <div className="field-error emp-field-full">{error}</div>}

                {empModal.mode === 'create' && (
  <>
    <div className="emp-field">
      <label>Email</label>
      <input
        required
        type="email"
        value={empModal.data.email}
        onChange={(e) =>
          setEmpModal({
            ...empModal,
            data: {
              ...empModal.data,
              email: e.target.value
            }
          })
        }
        placeholder="employee@company.com"
      />
    </div>

    <div className="emp-field">
      <label>Password</label>
      <input
        required
        type="password"
        minLength={6}
        value={empModal.data.password}
        onChange={(e) =>
          setEmpModal({
            ...empModal,
            data: {
              ...empModal.data,
              password: e.target.value
            }
          })
        }
        placeholder="Create login password"
      />
    </div>
  </>
)}
                <div className="emp-field">
                  <label>Full Name</label>
                  <input required value={empModal.data.name} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, name: e.target.value } })} />
                </div>

                <div className="emp-field">
                  <label>Employee ID (optional)</label>
                  <input value={empModal.data.employeeId || ''} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, employeeId: e.target.value } })} />
                </div>

                <div className="emp-field">
                  <label>Designation</label>
                  <input value={empModal.data.designation || ''} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, designation: e.target.value } })} />
                </div>

                <div className="emp-field">
                  <label>Primary Mobile</label>
                  <input value={empModal.data.registeredMobileNumber || ''} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, registeredMobileNumber: e.target.value } })} />
                </div>

                <div className="emp-field">
                  <label>Secondary Mobile</label>
                  <input value={empModal.data.secondMobileNumber || ''} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, secondMobileNumber: e.target.value } })} />
                </div>

                <div className="emp-field">
                  <label>Department</label>
                  <select required={empModal.mode === 'create'} value={empModal.data.departmentId || ''} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, departmentId: e.target.value } })}>
                    <option value="">Select department</option>
                    {departments.map((d) => (
                      <option key={d.id || d.departmentId} value={d.id || d.departmentId}>{d.name}</option>
                    ))}
                  </select>
                </div>

                <div className="emp-field">
                  <label>Grade (optional)</label>
                  <select value={empModal.data.gradeId || ''} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, gradeId: e.target.value } })}>
                    <option value="">No grade</option>
                    {grades.map((g) => (
                      <option key={g.id} value={g.id}>Level {g.gradeLevel}{g.description ? ` — ${g.description}` : ''}</option>
                    ))}
                  </select>
                </div>

                <div className="emp-field emp-field-full">
                  <label>Reports To (optional — Employee ID)</label>
                  <select value={empModal.data.reportsTo || ''} onChange={(e) => setEmpModal({ ...empModal, data: { ...empModal.data, reportsTo: e.target.value } })}>
                    <option value="">No manager</option>
                    {employees.map((e) => (
                      <option key={e.id} value={e.id}>{e.name}</option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn--secondary" onClick={() => setEmpModal(null)}>Cancel</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Saving...' : 'Save'}</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Schedule Modal */}
      {scheduleModal && (
        <div className="modal-backdrop emp-modal-backdrop" onClick={() => setScheduleModal(null)}>
          <div className="modal emp-modal" style={{ maxWidth: '640px' }} onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{scheduleModal.employee.name} — Schedule</h2>
              <button className="modal-close" onClick={() => setScheduleModal(null)}><X size={18} /></button>
            </div>

            <div style={{ padding: '0 1.25rem', flexShrink: 0 }}>
              <span
                className={`badge ${isOnLeaveToday(leaves) ? 'badge--error' : 'badge--resolved'}`}
                style={{ marginBottom: '1rem', display: 'inline-block' }}
              >
                {isOnLeaveToday(leaves) ? 'On Leave Today' : 'Available Today'}
              </span>

              <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
                <button className={`btn btn--sm ${scheduleModal.tab === 'hours' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setScheduleModal({ ...scheduleModal, tab: 'hours' })}>
                  <Clock size={14} /> Working Hours
                </button>
                <button className={`btn btn--sm ${scheduleModal.tab === 'leave' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setScheduleModal({ ...scheduleModal, tab: 'leave' })}>
                  <CalendarOff size={14} /> Leave
                </button>
              </div>
            </div>

            <div className="modal-body emp-modal-body-scroll">
              {scheduleModal.tab === 'hours' && (
                <>
                  <table className="data-table">
                    <thead><tr><th>Day</th><th>Start</th><th>End</th><th>Working?</th><th></th></tr></thead>
                    <tbody>
                      {workingHours.length === 0 ? (
                        <tr><td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-dim)' }}>No working hours set.</td></tr>
                      ) : (
                        workingHours.map((wh) => (
                          <tr key={wh.id}>
                            <td>{DAYS[wh.dayOfWeek]}</td>
                            <td>{wh.startTime?.slice(0, 5)}</td>
                            <td>{wh.endTime?.slice(0, 5)}</td>
                            <td>{wh.isWorkingDay ? 'Yes' : 'Off'}</td>
                            <td>
                              <div className="row-actions">
                                <button className="icon-btn" onClick={() => openEditWh(wh)}><Edit2 size={14} /></button>
                                <button className="icon-btn" onClick={() => handleDeleteWh(wh)}><Trash2 size={14} /></button>
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                  <button className="btn btn--secondary btn--sm" style={{ marginTop: '0.75rem' }} onClick={openCreateWh}>
                    <Plus size={14} /> Add Working Hour
                  </button>

                  {whForm && (
                    <form onSubmit={handleSaveWh} style={{ marginTop: '1rem', padding: '1rem', border: '1px solid #334155', borderRadius: '8px' }}>
                      <div className="emp-field">
                        <label>{whForm.mode === 'create' ? 'Day(s)' : 'Day'}</label>
                        {whForm.mode === 'create' ? (
                          <>
                            <div className="emp-day-picker">
                              {DAYS.map((d, i) => (
                                <span
                                  key={i}
                                  className={`emp-day-chip ${whForm.data.daysOfWeek.includes(i) ? 'active' : ''}`}
                                  onClick={() => toggleWhDay(i)}
                                >
                                  {d.slice(0, 3)}
                                </span>
                              ))}
                            </div>
                            <div className="emp-day-shortcuts">
                              <button type="button" onClick={() => setWhDays(WEEKDAYS)}>Weekdays (Mon–Fri)</button>
                              <button type="button" onClick={() => setWhDays([0, 1, 2, 3, 4, 5, 6])}>All days</button>
                              <button type="button" onClick={() => setWhDays([])}>Clear</button>
                            </div>
                          </>
                        ) : (
                          <select value={whForm.data.daysOfWeek[0]} onChange={(e) => setWhDays([Number(e.target.value)])}>
                            {DAYS.map((d, i) => <option key={i} value={i}>{d}</option>)}
                          </select>
                        )}
                      </div>
                      <div className="emp-field">
                        <label>Start Time</label>
                        <input type="time" value={whForm.data.startTime.slice(0, 5)} onChange={(e) => setWhForm({ ...whForm, data: { ...whForm.data, startTime: e.target.value + ':00' } })} />
                      </div>
                      <div className="emp-field">
                        <label>End Time</label>
                        <input type="time" value={whForm.data.endTime.slice(0, 5)} onChange={(e) => setWhForm({ ...whForm, data: { ...whForm.data, endTime: e.target.value + ':00' } })} />
                      </div>
                      <div className="emp-checkbox-field">
                        <input type="checkbox" id="wh-is-working" checked={whForm.data.isWorkingDay} onChange={(e) => setWhForm({ ...whForm, data: { ...whForm.data, isWorkingDay: e.target.checked } })} />
                        <label htmlFor="wh-is-working" style={{ margin: 0 }}>Working Day</label>
                      </div>
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button type="button" className="btn btn--secondary btn--sm" onClick={() => setWhForm(null)}>Cancel</button>
                        <button type="submit" className="btn btn--primary btn--sm">Save</button>
                      </div>
                    </form>
                  )}
                </>
              )}

              {scheduleModal.tab === 'leave' && (
                <>
                  <table className="data-table">
                    <thead><tr><th>Start</th><th>End</th><th>Type</th><th>Status</th><th></th></tr></thead>
                    <tbody>
                      {leaves.length === 0 ? (
                        <tr><td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-dim)' }}>No leave records.</td></tr>
                      ) : (
                        leaves.map((lv) => (
                          <tr key={lv.id}>
                            <td>{(lv.startDate || '').slice(0, 10)}</td>
                            <td>{(lv.endDate || '').slice(0, 10)}</td>
                            <td>{lv.leaveType || '—'}</td>
                            <td>{lv.status || 'Pending'}</td>
                            <td>
                              <div className="row-actions">
                                <button className="icon-btn" onClick={() => openEditLeave(lv)}><Edit2 size={14} /></button>
                                <button className="icon-btn" onClick={() => handleDeleteLeave(lv)}><Trash2 size={14} /></button>
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                  <button className="btn btn--secondary btn--sm" style={{ marginTop: '0.75rem' }} onClick={openCreateLeave}>
                    <Plus size={14} /> Add Leave
                  </button>

                  {leaveForm && (
                    <form onSubmit={handleSaveLeave} style={{ marginTop: '1rem', padding: '1rem', border: '1px solid #334155', borderRadius: '8px' }}>
                      <div className="emp-field">
                        <label>Start Date</label>
                        <input required type="date" value={leaveForm.data.startDate} onChange={(e) => setLeaveForm({ ...leaveForm, data: { ...leaveForm.data, startDate: e.target.value } })} />
                      </div>
                      <div className="emp-field">
                        <label>End Date</label>
                        <input required type="date" value={leaveForm.data.endDate} onChange={(e) => setLeaveForm({ ...leaveForm, data: { ...leaveForm.data, endDate: e.target.value } })} />
                      </div>
                      <div className="emp-field">
                        <label>Leave Type</label>
                        <select value={leaveForm.data.leaveType} onChange={(e) => setLeaveForm({ ...leaveForm, data: { ...leaveForm.data, leaveType: e.target.value } })}>
                          <option value="Casual">Casual</option>
                          <option value="Sick">Sick</option>
                          <option value="Earned">Earned</option>
                          <option value="Unpaid">Unpaid</option>
                        </select>
                      </div>
                      {leaveForm.mode === 'edit' && (
                        <div className="emp-field">
                          <label>Status</label>
                          <select value={leaveForm.data.status} onChange={(e) => setLeaveForm({ ...leaveForm, data: { ...leaveForm.data, status: e.target.value } })}>
                            <option value="Pending">Pending</option>
                            <option value="Approved">Approved</option>
                            <option value="Rejected">Rejected</option>
                            <option value="Cancelled">Cancelled</option>
                          </select>
                        </div>
                      )}
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button type="button" className="btn btn--secondary btn--sm" onClick={() => setLeaveForm(null)}>Cancel</button>
                        <button type="submit" className="btn btn--primary btn--sm">Save</button>
                      </div>
                    </form>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}