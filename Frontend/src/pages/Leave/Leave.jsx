import React, { useCallback, useEffect, useState } from 'react';
import { CalendarOff, Plus, Edit2, Trash2, X } from 'lucide-react';
import api from '../../api/axios';
import { isAdmin, isEmployee } from '../../auth/roles';

const EMPTY_LEAVE = {
  startDate: '',
  endDate: '',
  leaveType: 'Casual'
};

const FIELD_STYLES = `
.leave-field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
  margin-bottom: 1rem;
}

.leave-field label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--text-dim, #94a3b8);
}

.leave-field input,
.leave-field select {
  width: 100%;
  box-sizing: border-box;
  padding: 0.65rem 0.75rem;
  border-radius: 6px;
  border: 1px solid #334155;
  background: #0f172a;
  color: inherit;
}

.leave-form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0 1rem;
}

.leave-field-full {
  grid-column: 1 / -1;
}

.leave-status {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.6rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.leave-status--pending {
  background: rgba(234, 179, 8, 0.15);
  color: #facc15;
}

.leave-status--approved {
  background: rgba(34, 197, 94, 0.15);
  color: #4ade80;
}

.leave-status--rejected {
  background: rgba(239, 68, 68, 0.15);
  color: #f87171;
}

.leave-status--cancelled {
  background: rgba(148, 163, 184, 0.15);
  color: #94a3b8;
}

@media (max-width: 600px) {
  .leave-form-grid {
    grid-template-columns: 1fr;
  }

  .leave-field-full {
    grid-column: auto;
  }
}
`;

export default function Leave() {
  const user = JSON.parse(localStorage.getItem('user') || 'null');

  const admin = isAdmin(user);
  const employee = isEmployee(user);

  const [employeeRecord, setEmployeeRecord] = useState(null);
  const [employees, setEmployees] = useState([]);
  // '' means "All Employees" for admins. For non-admin employees this is
  // always pinned to their own employee id.
  const [selectedEmployeeId, setSelectedEmployeeId] = useState('');

  // Leaves for the currently-selected employee (used by the non-admin flow,
  // and by admins once they filter down to one employee).
  const [leaves, setLeaves] = useState([]);
  // Combined leaves across every employee, tagged with who they belong to.
  // Only populated/used for admins.
  const [allLeaves, setAllLeaves] = useState([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [leaveModal, setLeaveModal] = useState(null);
  const [saving, setSaving] = useState(false);

  /*
   * Load employee records.
   *
   * For normal employees the backend should return their own
   * employee record.
   *
   * Admin can receive the complete employee list.
   */
  const loadEmployees = useCallback(async () => {
    try {
      const res = await api.get('/employees', {
        params: { pageSize: 100 }
      });

      const items = res.data.items || res.data || [];

      setEmployees(items);

      if (employee && !admin) {
        const current = items[0];

        if (current) {
          setEmployeeRecord(current);
          setSelectedEmployeeId(current.id);
        }
      }

      return items;
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Failed to load employee information.'
      );
      return [];
    }
  }, [employee, admin]);

  const loadLeaves = useCallback(async (employeeId) => {
    if (!employeeId) {
      setLeaves([]);
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const res = await api.get(
        `/employees/${employeeId}/leaves`
      );

      setLeaves(res.data || []);
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Failed to load leave records.'
      );
      setLeaves([]);
    } finally {
      setLoading(false);
    }
  }, []);

  /*
   * Admins land on an aggregate view of every employee's leave requests,
   * rather than having to pick someone first. There's no single "all
   * leaves" endpoint, so this fetches each employee's leaves in parallel
   * and merges them, tagging each row with the employee it belongs to.
   */
  const loadAllLeaves = useCallback(async (employeeList) => {
    if (!employeeList.length) {
      setAllLeaves([]);
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const results = await Promise.all(
        employeeList.map(async (emp) => {
          try {
            const res = await api.get(`/employees/${emp.id}/leaves`);
            return (res.data || []).map((lv) => ({
              ...lv,
              employeeId: emp.id,
              employeeName: emp.name,
              employeeCode: emp.employeeId
            }));
          } catch {
            return [];
          }
        })
      );

      const merged = results
        .flat()
        .sort((a, b) => (b.startDate || '').localeCompare(a.startDate || ''));

      setAllLeaves(merged);
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Failed to load leave records.'
      );
      setAllLeaves([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    (async () => {
      const items = await loadEmployees();

      if (admin) {
        await loadAllLeaves(items);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    // Only the non-admin, single-employee flow needs its own fetch;
    // admins filter the already-loaded allLeaves list client-side instead.
    if (!admin && selectedEmployeeId) {
      loadLeaves(selectedEmployeeId);
    }
  }, [admin, selectedEmployeeId, loadLeaves]);

  /*
   * Admin can select an employee, or go back to "All Employees".
   */
  function handleEmployeeChange(e) {
    const id = e.target.value;
    setSelectedEmployeeId(id);

    const selected = employees.find((emp) => emp.id === id);
    setEmployeeRecord(selected || null);
  }

  async function refreshLeaves() {
    if (admin) {
      await loadAllLeaves(employees);
    } else {
      await loadLeaves(selectedEmployeeId);
    }
  }

  function openCreateLeave() {
    setError('');

    setLeaveModal({
      mode: 'create',
      data: { ...EMPTY_LEAVE }
    });
  }

  function openEditLeave(leave) {
    setError('');

    setLeaveModal({
      mode: 'edit',
      id: leave.id,
      // The leave itself carries which employee it belongs to (set when
      // merging for the admin "All Employees" view; falls back to the
      // currently-selected employee for the single-employee flow).
      employeeId: leave.employeeId || selectedEmployeeId,
      data: {
        startDate: (leave.startDate || '').slice(0, 10),
        endDate: (leave.endDate || '').slice(0, 10),
        leaveType: leave.leaveType || 'Casual',
        status: leave.status || 'Pending'
      }
    });
  }

  async function handleSaveLeave(e) {
    e.preventDefault();

    const targetEmployeeId =
      leaveModal.mode === 'edit'
        ? leaveModal.employeeId
        : selectedEmployeeId;

    if (!targetEmployeeId) {
      setError('No employee selected.');
      return;
    }

    if (
      !leaveModal.data.startDate ||
      !leaveModal.data.endDate
    ) {
      setError('Start date and end date are required.');
      return;
    }

    if (leaveModal.data.endDate < leaveModal.data.startDate) {
      setError('End date cannot be before start date.');
      return;
    }

    setSaving(true);
    setError('');

    try {
      if (leaveModal.mode === 'create') {
        await api.post(
          `/employees/${targetEmployeeId}/leaves`,
          {
            startDate: leaveModal.data.startDate,
            endDate: leaveModal.data.endDate,
            leaveType: leaveModal.data.leaveType
          }
        );
      } else {
        await api.put(
          `/employees/${targetEmployeeId}/leaves/${leaveModal.id}`,
          leaveModal.data
        );
      }

      setLeaveModal(null);

      await refreshLeaves();
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Failed to save leave.'
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleDeleteLeave(leave) {
    if (
      !window.confirm(
        'Are you sure you want to remove this leave request?'
      )
    ) {
      return;
    }

    const targetEmployeeId = leave.employeeId || selectedEmployeeId;

    try {
      await api.delete(
        `/employees/${targetEmployeeId}/leaves/${leave.id}`
      );

      await refreshLeaves();
    } catch (err) {
      alert(
        err.response?.data?.message ||
        'Failed to delete leave.'
      );
    }
  }

  function getStatusClass(status) {
    switch ((status || 'Pending').toLowerCase()) {
      case 'approved':
        return 'leave-status leave-status--approved';

      case 'rejected':
        return 'leave-status leave-status--rejected';

      case 'cancelled':
        return 'leave-status leave-status--cancelled';

      default:
        return 'leave-status leave-status--pending';
    }
  }

  /*
   * Safety check.
   * App.jsx already protects the route, but this prevents rendering
   * if somebody somehow reaches the component without a valid role.
   */
  if (!admin && !employee) {
    return null;
  }

  // What actually renders in the table: for admins, either every leave
  // (no filter selected) or just the chosen employee's, filtered from the
  // already-loaded aggregate. For non-admin employees, their own list.
  const showingAll = admin && !selectedEmployeeId;
  const displayedLeaves = admin
    ? (selectedEmployeeId
        ? allLeaves.filter((lv) => lv.employeeId === selectedEmployeeId)
        : allLeaves)
    : leaves;

  const canApply = admin ? !!selectedEmployeeId : !!selectedEmployeeId;

  return (
    <div className="tickets-page">
      <style>{FIELD_STYLES}</style>

      <div className="page-header">
        <div>
          <h1>
            <CalendarOff
              size={24}
              style={{
                verticalAlign: 'middle',
                marginRight: '8px'
              }}
            />
            Leave Management
          </h1>

          <p>
            {admin
              ? 'View and manage employee leave applications.'
              : 'Apply for leave and view your leave history.'}
          </p>
        </div>

        {canApply && (
          <button
            className="btn btn--primary"
            onClick={openCreateLeave}
          >
            <Plus size={16} />
            Apply for Leave
          </button>
        )}
      </div>

      {admin && (
        <div
          className="glass-card"
          style={{
            padding: '1rem',
            marginBottom: '1rem'
          }}
        >
          <div className="leave-field" style={{ marginBottom: 0 }}>
            <label>Filter by Employee</label>

            <select
              value={selectedEmployeeId}
              onChange={handleEmployeeChange}
            >
              <option value="">
                All Employees
              </option>

              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.name}
                  {emp.employeeId
                    ? ` (${emp.employeeId})`
                    : ''}
                </option>
              ))}
            </select>
          </div>
        </div>
      )}

      {error && (
        <div className="tickets-error">
          {error}
        </div>
      )}

      {employeeRecord && selectedEmployeeId && (
        <div
          className="glass-card"
          style={{
            padding: '1rem',
            marginBottom: '1rem'
          }}
        >
          <strong>{employeeRecord.name}</strong>

          {employeeRecord.employeeId && (
            <span
              style={{
                marginLeft: '0.75rem',
                color: 'var(--text-dim)'
              }}
            >
              {employeeRecord.employeeId}
            </span>
          )}

          {employeeRecord.departmentName && (
            <div
              style={{
                marginTop: '0.25rem',
                color: 'var(--text-dim)',
                fontSize: '0.85rem'
              }}
            >
              {employeeRecord.departmentName}
            </div>
          )}
        </div>
      )}

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                {showingAll && <th>Employee</th>}
                <th>Start Date</th>
                <th>End Date</th>
                <th>Leave Type</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>

            <tbody>
              {loading ? (
                <tr>
                  <td
                    colSpan={showingAll ? 6 : 5}
                    style={{
                      textAlign: 'center',
                      color: 'var(--text-dim)',
                      padding: '2rem'
                    }}
                  >
                    Loading leave records...
                  </td>
                </tr>
              ) : displayedLeaves.length === 0 ? (
                <tr>
                  <td
                    colSpan={showingAll ? 6 : 5}
                    style={{
                      textAlign: 'center',
                      color: 'var(--text-dim)',
                      padding: '2rem'
                    }}
                  >
                    No leave records found.
                  </td>
                </tr>
              ) : (
                displayedLeaves.map((leave) => (
                  <tr key={`${leave.employeeId || selectedEmployeeId}-${leave.id}`}>
                    {showingAll && (
                      <td>
                        <strong>{leave.employeeName || '—'}</strong>
                        {leave.employeeCode && (
                          <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>
                            {leave.employeeCode}
                          </div>
                        )}
                      </td>
                    )}

                    <td>
                      {(leave.startDate || '').slice(0, 10)}
                    </td>

                    <td>
                      {(leave.endDate || '').slice(0, 10)}
                    </td>

                    <td>
                      {leave.leaveType || '—'}
                    </td>

                    <td>
                      <span
                        className={getStatusClass(
                          leave.status
                        )}
                      >
                        {leave.status || 'Pending'}
                      </span>
                    </td>

                    <td>
                      <div className="row-actions">
                        <button
                          className="icon-btn"
                          title="Edit"
                          onClick={() =>
                            openEditLeave(leave)
                          }
                        >
                          <Edit2 size={14} />
                        </button>

                        <button
                          className="icon-btn"
                          title="Delete"
                          onClick={() =>
                            handleDeleteLeave(leave)
                          }
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {leaveModal && (
        <div
          className="modal-backdrop"
          onClick={() => setLeaveModal(null)}
        >
          <div
            className="modal"
            style={{ maxWidth: '550px' }}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2>
                {leaveModal.mode === 'create'
                  ? 'Apply for Leave'
                  : 'Edit Leave'}
              </h2>

              <button
                className="modal-close"
                onClick={() => setLeaveModal(null)}
              >
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleSaveLeave}>
              <div className="modal-body">
                {error && (
                  <div className="field-error">
                    {error}
                  </div>
                )}

                <div className="leave-form-grid">
                  <div className="leave-field">
                    <label>Start Date</label>

                    <input
                      required
                      type="date"
                      value={leaveModal.data.startDate}
                      onChange={(e) =>
                        setLeaveModal({
                          ...leaveModal,
                          data: {
                            ...leaveModal.data,
                            startDate:
                              e.target.value
                          }
                        })
                      }
                    />
                  </div>

                  <div className="leave-field">
                    <label>End Date</label>

                    <input
                      required
                      type="date"
                      value={leaveModal.data.endDate}
                      onChange={(e) =>
                        setLeaveModal({
                          ...leaveModal,
                          data: {
                            ...leaveModal.data,
                            endDate:
                              e.target.value
                          }
                        })
                      }
                    />
                  </div>

                  <div className="leave-field leave-field-full">
                    <label>Leave Type</label>

                    <select
                      value={leaveModal.data.leaveType}
                      onChange={(e) =>
                        setLeaveModal({
                          ...leaveModal,
                          data: {
                            ...leaveModal.data,
                            leaveType:
                              e.target.value
                          }
                        })
                      }
                    >
                      <option value="Casual">
                        Casual
                      </option>

                      <option value="Sick">
                        Sick
                      </option>

                      <option value="Earned">
                        Earned
                      </option>

                      <option value="Unpaid">
                        Unpaid
                      </option>
                    </select>
                  </div>

                  {leaveModal.mode === 'edit' &&
                    admin && (
                      <div className="leave-field leave-field-full">
                        <label>Status</label>

                        <select
                          value={
                            leaveModal.data.status ||
                            'Pending'
                          }
                          onChange={(e) =>
                            setLeaveModal({
                              ...leaveModal,
                              data: {
                                ...leaveModal.data,
                                status:
                                  e.target.value
                              }
                            })
                          }
                        >
                          <option value="Pending">
                            Pending
                          </option>

                          <option value="Approved">
                            Approved
                          </option>

                          <option value="Rejected">
                            Rejected
                          </option>

                          <option value="Cancelled">
                            Cancelled
                          </option>
                        </select>
                      </div>
                    )}
                </div>
              </div>

              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn--secondary"
                  onClick={() => setLeaveModal(null)}
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="btn btn--primary"
                  disabled={saving}
                >
                  {saving ? 'Saving...' : 'Submit'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}