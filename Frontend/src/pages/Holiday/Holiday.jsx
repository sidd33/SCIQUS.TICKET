import React, { useCallback, useEffect, useState } from 'react';
import { CalendarDays, Plus, Edit2, Trash2, X } from 'lucide-react';
import api from '../../api/axios';
import { isAdmin } from '../../auth/roles';

const EMPTY_HOLIDAY = {
  name: '',
  date: '',
  isRecurringYearly: false,
  description: ''
};

const FIELD_STYLES = `
.holiday-field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
  margin-bottom: 1rem;
}

.holiday-field label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--text-dim, #94a3b8);
}

.holiday-field input,
.holiday-field textarea {
  width: 100%;
  box-sizing: border-box;
  padding: 0.65rem 0.75rem;
  border-radius: 6px;
  border: 1px solid #334155;
  background: #0f172a;
  color: inherit;
  font-family: inherit;
}

.holiday-form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0 1rem;
}

.holiday-field-full {
  grid-column: 1 / -1;
}

.holiday-checkbox-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.holiday-checkbox-field label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--text-dim, #94a3b8);
}

.holiday-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.6rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
  background: rgba(99, 102, 241, 0.15);
  color: #a5b4fc;
}

@media (max-width: 600px) {
  .holiday-form-grid {
    grid-template-columns: 1fr;
  }

  .holiday-field-full {
    grid-column: auto;
  }
}
`;

export default function Holiday() {
  const user = JSON.parse(localStorage.getItem('user') || 'null');
  const admin = isAdmin(user);

  const [holidays, setHolidays] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [holidayModal, setHolidayModal] = useState(null);
  const [saving, setSaving] = useState(false);

  const loadHolidays = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const res = await api.get('/Holidays');
      setHolidays(res.data || []);
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Failed to load holidays.'
      );
      setHolidays([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadHolidays();
  }, [loadHolidays]);

  function openCreateHoliday() {
    setError('');

    setHolidayModal({
      mode: 'create',
      data: { ...EMPTY_HOLIDAY }
    });
  }

  function openEditHoliday(holiday) {
    setError('');

    setHolidayModal({
      mode: 'edit',
      id: holiday.id,
      data: {
        name: holiday.name || '',
        date: (holiday.date || '').slice(0, 10),
        isRecurringYearly: !!holiday.isRecurringYearly,
        description: holiday.description || ''
      }
    });
  }

  async function handleSaveHoliday(e) {
    e.preventDefault();

    if (!holidayModal.data.name || !holidayModal.data.date) {
      setError('Name and date are required.');
      return;
    }

    setSaving(true);
    setError('');

    try {
      if (holidayModal.mode === 'create') {
        await api.post('/Holidays', {
          name: holidayModal.data.name,
          date: holidayModal.data.date,
          isRecurringYearly: holidayModal.data.isRecurringYearly,
          description: holidayModal.data.description
        });
      } else {
        await api.put(`/Holidays/${holidayModal.id}`, {
          name: holidayModal.data.name,
          date: holidayModal.data.date,
          isRecurringYearly: holidayModal.data.isRecurringYearly,
          description: holidayModal.data.description
        });
      }

      setHolidayModal(null);
      await loadHolidays();
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Failed to save holiday.'
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleDeleteHoliday(holiday) {
    if (
      !window.confirm(
        `Are you sure you want to remove "${holiday.name}"?`
      )
    ) {
      return;
    }

    try {
      await api.delete(`/Holidays/${holiday.id}`);
      await loadHolidays();
    } catch (err) {
      alert(
        err.response?.data?.message ||
        'Failed to delete holiday.'
      );
    }
  }

  function formatDate(date) {
    if (!date) return '-';

    return new Date(date).toLocaleDateString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      weekday: 'short'
    });
  }

  if (!admin) {
    return null;
  }

  return (
    <div className="tickets-page">
      <style>{FIELD_STYLES}</style>

      <div className="page-header">
        <div>
          <h1>
            <CalendarDays
              size={24}
              style={{
                verticalAlign: 'middle',
                marginRight: '8px'
              }}
            />
            Holiday Management
          </h1>

          <p>Manage the company-wide holiday calendar.</p>
        </div>

        <button
          className="btn btn--primary"
          onClick={openCreateHoliday}
        >
          <Plus size={16} />
          Add Holiday
        </button>
      </div>

      {error && (
        <div className="tickets-error">
          {error}
        </div>
      )}

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Date</th>
                <th>Recurring</th>
                <th>Description</th>
                <th></th>
              </tr>
            </thead>

            <tbody>
              {loading ? (
                <tr>
                  <td
                    colSpan={5}
                    style={{
                      textAlign: 'center',
                      color: 'var(--text-dim)',
                      padding: '2rem'
                    }}
                  >
                    Loading holidays...
                  </td>
                </tr>
              ) : holidays.length === 0 ? (
                <tr>
                  <td
                    colSpan={5}
                    style={{
                      textAlign: 'center',
                      color: 'var(--text-dim)',
                      padding: '2rem'
                    }}
                  >
                    No holidays configured yet.
                  </td>
                </tr>
              ) : (
                holidays.map((holiday) => (
                  <tr key={holiday.id}>
                    <td>
                      <strong>{holiday.name}</strong>
                    </td>

                    <td>
                      {formatDate(holiday.date)}
                    </td>

                    <td>
                      {holiday.isRecurringYearly ? (
                        <span className="holiday-badge">Yearly</span>
                      ) : (
                        '—'
                      )}
                    </td>

                    <td>
                      {holiday.description || '—'}
                    </td>

                    <td>
                      <div className="row-actions">
                        <button
                          className="icon-btn"
                          title="Edit"
                          onClick={() => openEditHoliday(holiday)}
                        >
                          <Edit2 size={14} />
                        </button>

                        <button
                          className="icon-btn"
                          title="Delete"
                          onClick={() => handleDeleteHoliday(holiday)}
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

      {holidayModal && (
        <div
          className="modal-backdrop"
          onClick={() => setHolidayModal(null)}
        >
          <div
            className="modal"
            style={{ maxWidth: '550px' }}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2>
                {holidayModal.mode === 'create'
                  ? 'Add Holiday'
                  : 'Edit Holiday'}
              </h2>

              <button
                className="modal-close"
                onClick={() => setHolidayModal(null)}
              >
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleSaveHoliday}>
              <div className="modal-body">
                {error && (
                  <div className="field-error">
                    {error}
                  </div>
                )}

                <div className="holiday-form-grid">
                  <div className="holiday-field holiday-field-full">
                    <label>Holiday Name</label>

                    <input
                      required
                      type="text"
                      placeholder="e.g. Independence Day"
                      value={holidayModal.data.name}
                      onChange={(e) =>
                        setHolidayModal({
                          ...holidayModal,
                          data: {
                            ...holidayModal.data,
                            name: e.target.value
                          }
                        })
                      }
                    />
                  </div>

                  <div className="holiday-field holiday-field-full">
                    <label>Date</label>

                    <input
                      required
                      type="date"
                      value={holidayModal.data.date}
                      onChange={(e) =>
                        setHolidayModal({
                          ...holidayModal,
                          data: {
                            ...holidayModal.data,
                            date: e.target.value
                          }
                        })
                      }
                    />
                  </div>

                  <div className="holiday-checkbox-field holiday-field-full">
                    <input
                      type="checkbox"
                      id="isRecurringYearly"
                      checked={holidayModal.data.isRecurringYearly}
                      onChange={(e) =>
                        setHolidayModal({
                          ...holidayModal,
                          data: {
                            ...holidayModal.data,
                            isRecurringYearly: e.target.checked
                          }
                        })
                      }
                    />
                    <label htmlFor="isRecurringYearly">
                      Recurs every year on this date
                    </label>
                  </div>

                  <div className="holiday-field holiday-field-full">
                    <label>Description (optional)</label>

                    <textarea
                      rows={3}
                      value={holidayModal.data.description}
                      onChange={(e) =>
                        setHolidayModal({
                          ...holidayModal,
                          data: {
                            ...holidayModal.data,
                            description: e.target.value
                          }
                        })
                      }
                    />
                  </div>
                </div>
              </div>

              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn--secondary"
                  onClick={() => setHolidayModal(null)}
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="btn btn--primary"
                  disabled={saving}
                >
                  {saving ? 'Saving...' : 'Save'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}