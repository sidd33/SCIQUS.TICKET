import { useEffect, useMemo, useState } from 'react';
import { Plus, Trash2, Edit2, Building2, Search, Code2, Users, Banknote, Megaphone, Landmark, Scale } from 'lucide-react';
import api from '../../api/axios';
import { getFriendlyErrorMessage } from '../../utils/apiErrors';
import './Departments.scss';

// Distinct dynamic themes to match reference UI
const THEMES = [
  { class: 'theme-purple', icon: Code2, badgeColor: '#e0e7ff', badgeText: '#4338ca' },
  { class: 'theme-teal', icon: Users, badgeColor: '#d1fae5', badgeText: '#047857' },
  { class: 'theme-blue', icon: Banknote, badgeColor: '#e0e7ff', badgeText: '#4338ca' },
  { class: 'theme-amber', icon: Megaphone, badgeColor: '#fef3c7', badgeText: '#b45309' },
  { class: 'theme-slate', icon: Landmark, badgeColor: '#f1f5f9', badgeText: '#475569' },
  { class: 'theme-rose', icon: Scale, badgeColor: '#ffe4e6', badgeText: '#be123c' },
];

export default function Departments() {
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [formError, setFormError] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});
  const [saving, setSaving] = useState(false);
  const [listError, setListError] = useState('');
  const [editingDepartment, setEditingDepartment] = useState(null);

  useEffect(() => {
    fetchDepartments();
  }, []);

  async function fetchDepartments() {
    setLoading(true);
    try {
      const res = await api.get('/departments', { params: { pageSize: 100 } });
      setDepartments(res.data.items || []);
    } finally {
      setLoading(false);
    }
  }

  function closeForm() {
    setShowForm(false);
    setEditingDepartment(null);
    setName('');
    setDescription('');
    setFormError('');
    setFieldErrors({});
  }

  function openEdit(department) {
    setEditingDepartment(department);
    setName(department.name);
    setDescription(department.description || '');
    setFormError('');
    setFieldErrors({});
    setShowForm(true);
  }

  async function handleSave(e) {
    e.preventDefault();
    setFormError('');
    setFieldErrors({});

    if (!name.trim()) {
      setFieldErrors({ name: 'Department name is required.' });
      return;
    }

    setSaving(true);

    try {
      if (editingDepartment) {
        await api.put(`/departments/${editingDepartment.id}`, {
          name,
          description,
        });
      } else {
        await api.post('/departments', {
          name,
          description,
        });
      }

      closeForm();
      fetchDepartments();
    } catch (err) {
      const backendErrors = err.response?.data?.errors;

      if (backendErrors) {
        const mapped = {};
        Object.entries(backendErrors).forEach(([f, m]) => {
          mapped[f.charAt(0).toLowerCase() + f.slice(1)] = Array.isArray(m) ? m[0] : m;
        });
        setFieldErrors(mapped);
      } else {
        setFormError(getFriendlyErrorMessage(err));
      }
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id, name) {
    if (!window.confirm(`Delete "${name}"? This can't be undone.`)) return;
    setListError('');
    try {
      await api.delete(`/departments/${id}`);
      fetchDepartments();
    } catch (err) {
      setListError(getFriendlyErrorMessage(err));
    }
  }

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return departments;
    return departments.filter(
      (d) =>
        d.name.toLowerCase().includes(q) ||
        d.description?.toLowerCase().includes(q)
    );
  }, [departments, search]);

  return (
    <div className="departments-page">
      {/* Page Header matching design */}
      <div className="page-header">
        <div>
          <div className="title-row">
            <h1>Departments</h1>
            <span className="count-pill">{departments.length} departments</span>
          </div>
          <p className="subtitle">
            Manage organizational units, descriptions, and workforce distribution.
          </p>
        </div>
        <button className="btn-primary" onClick={() => setShowForm(true)}>
          <Plus size={18} /> Add Department
        </button>
      </div>

      {departments.length > 0 && (
        <div className="dept-search-box">
          <Search size={16} />
          <input
            type="text"
            placeholder="Search departments..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      )}

      {listError && <p className="error-text">{listError}</p>}

      {loading ? (
        <p className="loading-text">Loading departments...</p>
      ) : filtered.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">
            <Building2 size={32} />
          </div>
          <h3>{departments.length === 0 ? 'No departments yet' : 'No matches found'}</h3>
          <p>
            {departments.length === 0
              ? 'Add your first department to start organizing employees.'
              : 'Try a different search term.'}
          </p>
        </div>
      ) : (
        <div className="dept-grid">
          {filtered.map((d, i) => {
            const theme = THEMES[i % THEMES.length];
            const ThemeIcon = theme.icon;
            const empCount = d.employeeCount ?? d.employeesCount ?? 0;

            return (
              <div className="dept-card" key={d.id}>
                <div className="card-top">
                  <div className={`dept-icon ${theme.class}`}>
                    <ThemeIcon size={20} />
                  </div>
                  <div className="dept-actions">
                    <button
                      className="icon-btn"
                      onClick={() => openEdit(d)}
                      title="Edit Department"
                    >
                      <Edit2 size={15} />
                    </button>
                    <button
                      className="icon-btn delete-btn"
                      onClick={() => handleDelete(d.id, d.name)}
                      title="Delete Department"
                    >
                      <Trash2 size={15} />
                    </button>
                  </div>
                </div>

                <div className="dept-info">
                  <h3>{d.name}</h3>
                  <p>{d.description || 'No description provided.'}</p>
                </div>

                <div className="card-footer">
                  <span
                    className="emp-badge"
                    style={{
                      backgroundColor: theme.badgeColor,
                      color: theme.badgeText,
                    }}
                  >
                    {empCount} EMPLOYEES
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {showForm && (
        <div className="form-overlay" onClick={closeForm}>
          <form
            className="dept-form"
            onSubmit={handleSave}
            noValidate
            onClick={(e) => e.stopPropagation()}
          >
            <div className="form-header">
              <div>
                <h2>{editingDepartment ? 'Edit Department' : 'Add Department'}</h2>
                <p>
                  {editingDepartment
                    ? 'Update department details.'
                    : 'Create a new team or department.'}
                </p>
              </div>
              <button type="button" className="close-button" onClick={closeForm}>
                ×
              </button>
            </div>

            {formError && <p className="error-text">{formError}</p>}

            <label>
              Department Name
              <input
                placeholder="e.g. Engineering"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className={fieldErrors.name ? 'input-error' : ''}
              />
              {fieldErrors.name && (
                <span className="field-error-text">{fieldErrors.name}</span>
              )}
            </label>

            <label>
              Description
              <textarea
                placeholder="Technical product development, architecture, etc."
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </label>

            <div className="form-actions">
              <button
                type="button"
                className="btn-secondary"
                onClick={closeForm}
                disabled={saving}
              >
                Cancel
              </button>
              <button type="submit" className="btn-primary" disabled={saving}>
                {saving
                  ? 'Saving...'
                  : editingDepartment
                  ? 'Update Department'
                  : 'Save'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}