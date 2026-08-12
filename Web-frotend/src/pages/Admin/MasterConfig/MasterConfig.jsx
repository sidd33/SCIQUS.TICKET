import React, { useState, useEffect, useCallback } from 'react';
import { Layers, GitBranch, AlertOctagon, ShieldAlert, Plus, Edit2, Trash2, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import api from '../../../api/axios';
import './MasterConfig.scss';

export default function MasterConfig() {
  const [activeTab, setActiveTab] = useState('types'); // 'types' | 'subtypes' | 'priorities' | 'impacts'
  const [includeInactive, setIncludeInactive] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Master Lists Data
  const [types, setTypes] = useState([]);
  const [subTypes, setSubTypes] = useState([]);
  const [priorities, setPriorities] = useState([]);
  const [impacts, setImpacts] = useState([]);
  const [departments, setDepartments] = useState([]);

  // Modal State
  const [showModal, setShowModal] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [modalForm, setModalForm] = useState({});
  const [modalAgents, setModalAgents] = useState([]);
  const [saving, setSaving] = useState(false);
  const [modalError, setModalError] = useState('');

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = { includeInactive };
      const [tRes, stRes, pRes, iRes, dRes] = await Promise.all([
        api.get('/tickettypes', { params }),
        api.get('/ticketsubtypes', { params }),
        api.get('/ticketpriorities', { params }),
        api.get('/ticketbusinessimpacts', { params }),
        api.get('/departments', { params: { pageNumber: 1, pageSize: 100 } })
      ]);
      setTypes(tRes.data || []);
      setSubTypes(stRes.data || []);
      setPriorities(pRes.data || []);
      setImpacts(iRes.data || []);
      setDepartments(dRes.data.items || []);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load master configuration data');
    } finally {
      setLoading(false);
    }
  }, [includeInactive]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Load department agents for cascading Sub-Type form
  useEffect(() => {
    async function loadAgents() {
      if (activeTab === 'subtypes' && modalForm.departmentId) {
        try {
          const res = await api.get('/employees', {
            params: { pageNumber: 1, pageSize: 100, departmentId: modalForm.departmentId }
          });
          setModalAgents((res.data.items || []).filter(e => e.isActive));
        } catch (err) {
          setModalAgents([]);
        }
      } else {
        setModalAgents([]);
      }
    }
    loadAgents();
  }, [activeTab, modalForm.departmentId]);

  const handleOpenAdd = () => {
    setEditItem(null);
    setModalError('');
    if (activeTab === 'types') setModalForm({ name: '', description: '', status: true });
    if (activeTab === 'subtypes') setModalForm({ name: '', ticketTypeId: '', departmentId: '', defaultUserId: '', requiresAcceptance: false, acceptanceDeadlineHours: 2, manualOnly: false, status: true });
    if (activeTab === 'priorities') setModalForm({ name: '', slaInHours: 24, responseSlaInHours: 2, level: 1, manualOnly: false, status: true });
    if (activeTab === 'impacts') setModalForm({ name: '', description: '', status: true });
    setShowModal(true);
  };

  const handleOpenEdit = (item) => {
    setEditItem(item);
    setModalError('');
    setModalForm({ ...item });
    setShowModal(true);
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this master item?')) return;
    try {
      let endpoint = '';
      if (activeTab === 'types') endpoint = `/tickettypes/${id}`;
      if (activeTab === 'subtypes') endpoint = `/ticketsubtypes/${id}`;
      if (activeTab === 'priorities') endpoint = `/ticketpriorities/${id}`;
      if (activeTab === 'impacts') endpoint = `/ticketbusinessimpacts/${id}`;

      await api.delete(endpoint);
      loadData();
    } catch (err) {
      alert(err.response?.data?.message || 'Cannot delete master item: In use by open tickets.');
    }
  };

  const handleModalSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setModalError('');
    try {
      let endpoint = '';
      if (activeTab === 'types') endpoint = '/tickettypes';
      if (activeTab === 'subtypes') endpoint = '/ticketsubtypes';
      if (activeTab === 'priorities') endpoint = '/ticketpriorities';
      if (activeTab === 'impacts') endpoint = '/ticketbusinessimpacts';

      if (editItem) {
        await api.put(`${endpoint}/${editItem.id || editItem.ticketTypeId || editItem.ticketSubTypeId || editItem.priorityId || editItem.businessImpactId}`, modalForm);
      } else {
        await api.post(endpoint, modalForm);
      }
      setShowModal(false);
      loadData();
    } catch (err) {
      setModalError(err.response?.data?.message || 'Failed to save master item.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="master-config-page">
      <div className="page-header">
        <div>
          <h1>Ticket Master Configuration</h1>
          <p>Manage standard categories, issue sub-types, priority SLAs, and business impacts.</p>
        </div>
        <button className="btn btn--primary" onClick={handleOpenAdd}>
          <Plus size={16} /> Add New Item
        </button>
      </div>

      <div className="config-controls">
        <div className="tab-buttons">
          <button className={`tab-btn ${activeTab === 'types' ? 'active' : ''}`} onClick={() => setActiveTab('types')}>
            <Layers size={16} /> Ticket Types ({types.length})
          </button>
          <button className={`tab-btn ${activeTab === 'subtypes' ? 'active' : ''}`} onClick={() => setActiveTab('subtypes')}>
            <GitBranch size={16} /> Sub-Types ({subTypes.length})
          </button>
          <button className={`tab-btn ${activeTab === 'priorities' ? 'active' : ''}`} onClick={() => setActiveTab('priorities')}>
            <AlertOctagon size={16} /> Priorities & SLAs ({priorities.length})
          </button>
          <button className={`tab-btn ${activeTab === 'impacts' ? 'active' : ''}`} onClick={() => setActiveTab('impacts')}>
            <ShieldAlert size={16} /> Business Impacts ({impacts.length})
          </button>
        </div>

        <label className="toggle-inactive">
          <input
            type="checkbox"
            checked={includeInactive}
            onChange={(e) => setIncludeInactive(e.target.checked)}
          />
          Include Inactive Items
        </label>
      </div>

      {error && <div className="field-error">{error}</div>}

      <div className="card table-card">
        {loading ? (
          <div className="loading-state">Loading configuration data...</div>
        ) : (
          <table className="data-table">
            {activeTab === 'types' && (
              <>
                <thead>
                  <tr>
                    <th>Type Name</th>
                    <th>Description</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {types.map(t => (
                    <tr key={t.id || t.ticketTypeId}>
                      <td><strong>{t.name}</strong></td>
                      <td>{t.description || '—'}</td>
                      <td>
                        <span className={`status-pill ${t.status ? 'active' : 'inactive'}`}>
                          {t.status ? <CheckCircle size={12} /> : <XCircle size={12} />} {t.status ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        <div className="actions">
                          <button className="icon-btn" onClick={() => handleOpenEdit(t)}><Edit2 size={15} /></button>
                          <button className="icon-btn icon-btn--danger" onClick={() => handleDelete(t.id || t.ticketTypeId)}><Trash2 size={15} /></button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </>
            )}

            {activeTab === 'subtypes' && (
              <>
                <thead>
                  <tr>
                    <th>Sub-Type Name</th>
                    <th>Parent Type</th>
                    <th>Target Department</th>
                    <th>Default Agent</th>
                    <th>Acceptance Required</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {subTypes.map(st => (
                    <tr key={st.id || st.ticketSubTypeId}>
                      <td><strong>{st.name}</strong></td>
                      <td>{st.ticketTypeName || '—'}</td>
                      <td>{st.departmentName || '—'}</td>
                      <td>{st.defaultUserName || 'Auto-Route Queue'}</td>
                      <td>{st.requiresAcceptance ? `Yes (${st.acceptanceDeadlineHours || 2}h)` : 'No'}</td>
                      <td>
                        <span className={`status-pill ${st.status ? 'active' : 'inactive'}`}>
                          {st.status ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        <div className="actions">
                          <button className="icon-btn" onClick={() => handleOpenEdit(st)}><Edit2 size={15} /></button>
                          <button className="icon-btn icon-btn--danger" onClick={() => handleDelete(st.id || st.ticketSubTypeId)}><Trash2 size={15} /></button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </>
            )}

            {activeTab === 'priorities' && (
              <>
                <thead>
                  <tr>
                    <th>Priority Level</th>
                    <th>Severity Level</th>
                    <th>SLA Hours Target</th>
                    <th>Response SLA</th>
                    <th>Routing Mode</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {priorities.map(p => (
                    <tr key={p.id || p.priorityId}>
                      <td><strong>{p.name}</strong></td>
                      <td>Level {p.level}</td>
                      <td><span className="sla-badge">{p.slaInHours > 0 ? `${p.slaInHours} hours` : 'No SLA'}</span></td>
                      <td>{p.responseSlaInHours ? `${p.responseSlaInHours}h Response` : 'Standard'}</td>
                      <td>{p.manualOnly ? <span className="pill pill--warning">Manual Only</span> : 'Auto-Route'}</td>
                      <td>
                        <span className={`status-pill ${p.status ? 'active' : 'inactive'}`}>
                          {p.status ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        <div className="actions">
                          <button className="icon-btn" onClick={() => handleOpenEdit(p)}><Edit2 size={15} /></button>
                          <button className="icon-btn icon-btn--danger" onClick={() => handleDelete(p.id || p.priorityId)}><Trash2 size={15} /></button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </>
            )}

            {activeTab === 'impacts' && (
              <>
                <thead>
                  <tr>
                    <th>Impact Description</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {impacts.map(imp => (
                    <tr key={imp.id || imp.businessImpactId}>
                      <td><strong>{imp.description || imp.name}</strong></td>
                      <td>
                        <span className={`status-pill ${imp.status ? 'active' : 'inactive'}`}>
                          {imp.status ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        <div className="actions">
                          <button className="icon-btn" onClick={() => handleOpenEdit(imp)}><Edit2 size={15} /></button>
                          <button className="icon-btn icon-btn--danger" onClick={() => handleDelete(imp.id || imp.businessImpactId)}><Trash2 size={15} /></button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </>
            )}
          </table>
        )}
      </div>

      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>{editItem ? 'Edit Master Item' : 'Add New Master Item'}</h3>
              <button className="modal-close" onClick={() => setShowModal(false)}>×</button>
            </div>
            <form onSubmit={handleModalSubmit}>
              <div className="modal-body">
                {modalError && <div className="field-error"><AlertCircle size={14} /> {modalError}</div>}

                {/* Common Name / Description */}
                {(activeTab === 'types' || activeTab === 'subtypes' || activeTab === 'priorities') && (
                  <div className="field">
                    <label>Name *</label>
                    <input
                      required
                      value={modalForm.name || ''}
                      onChange={(e) => setModalForm({ ...modalForm, name: e.target.value })}
                    />
                  </div>
                )}

                {activeTab === 'impacts' && (
                  <div className="field">
                    <label>Business Impact Description *</label>
                    <input
                      required
                      value={modalForm.description || modalForm.name || ''}
                      onChange={(e) => setModalForm({ ...modalForm, description: e.target.value })}
                    />
                  </div>
                )}

                {/* Sub-Type Cascade: Type -> Dept -> Default Agent */}
                {activeTab === 'subtypes' && (
                  <>
                    <div className="field">
                      <label>Parent Ticket Type *</label>
                      <select
                        required
                        value={modalForm.ticketTypeId || ''}
                        onChange={(e) => setModalForm({ ...modalForm, ticketTypeId: e.target.value })}
                      >
                        <option value="">Select Ticket Type...</option>
                        {types.filter(t => t.status).map(t => (
                          <option key={t.id || t.ticketTypeId} value={t.id || t.ticketTypeId}>{t.name}</option>
                        ))}
                      </select>
                    </div>

                    <div className="field">
                      <label>Target Department *</label>
                      <select
                        required
                        value={modalForm.departmentId || ''}
                        onChange={(e) => setModalForm({ ...modalForm, departmentId: e.target.value })}
                      >
                        <option value="">Select Department...</option>
                        {departments.map(d => (
                          <option key={d.id} value={d.id}>{d.name}</option>
                        ))}
                      </select>
                    </div>

                    <div className="field">
                      <label>Default Agent (Optional)</label>
                      <select
                        value={modalForm.defaultUserId || ''}
                        onChange={(e) => setModalForm({ ...modalForm, defaultUserId: e.target.value })}
                        disabled={!modalForm.departmentId}
                      >
                        <option value="">Auto-Route Queue (No fixed agent)</option>
                        {modalAgents.map(ag => (
                          <option key={ag.id} value={ag.id}>{ag.firstName} {ag.lastName}</option>
                        ))}
                      </select>
                    </div>

                    <div className="field-checkbox">
                      <label>
                        <input
                          type="checkbox"
                          checked={modalForm.requiresAcceptance || false}
                          onChange={(e) => setModalForm({ ...modalForm, requiresAcceptance: e.target.checked })}
                        />
                        Requires Agent Acceptance Step
                      </label>
                    </div>
                  </>
                )}

                {/* Priority SLA & Level */}
                {activeTab === 'priorities' && (
                  <>
                    <div className="field">
                      <label>SLA Resolution Target (Hours) *</label>
                      <input
                        type="number"
                        min="0"
                        required
                        value={modalForm.slaInHours ?? 24}
                        onChange={(e) => setModalForm({ ...modalForm, slaInHours: parseInt(e.target.value) || 0 })}
                      />
                    </div>

                    <div className="field">
                      <label>Severity Ordering Level *</label>
                      <input
                        type="number"
                        min="1"
                        required
                        value={modalForm.level ?? 1}
                        onChange={(e) => setModalForm({ ...modalForm, level: parseInt(e.target.value) || 1 })}
                      />
                    </div>

                    <div className="field-checkbox">
                      <label>
                        <input
                          type="checkbox"
                          checked={modalForm.manualOnly || false}
                          onChange={(e) => setModalForm({ ...modalForm, manualOnly: e.target.checked })}
                        />
                        Manual Assignment Only (Excludes from Auto-Routing)
                      </label>
                    </div>
                  </>
                )}

                <div className="field-checkbox">
                  <label>
                    <input
                      type="checkbox"
                      checked={modalForm.status ?? true}
                      onChange={(e) => setModalForm({ ...modalForm, status: e.target.checked })}
                    />
                    Active Status
                  </label>
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn--secondary" onClick={() => setShowModal(false)}>Cancel</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>
                  {saving ? 'Saving...' : 'Save Master Item'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
