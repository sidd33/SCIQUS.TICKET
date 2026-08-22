import React, { useState, useEffect } from 'react';
import { Layers, Plus, Trash2, Edit2, X } from 'lucide-react';
import api from '../../../api/axios';

const EMPTY_TYPE = { name: '', description: '' };
const EMPTY_SUBTYPE = { name: '', description: '', ticketTypeId: '', departmentId: '', defaultUserId: '' };
const EMPTY_PRIORITY = { name: '', level: 1, slaInHours: 0 };
const EMPTY_IMPACT = { name: '', description: '' };

export default function MasterConfig() {
  const [activeTab, setActiveTab] = useState('types');
  const [types, setTypes] = useState([]);
  const [subTypes, setSubTypes] = useState([]);
  const [priorities, setPriorities] = useState([]);
  const [impacts, setImpacts] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [employees, setEmployees] = useState([]);

  // Integrations state
  const [apiToken, setApiToken] = useState('');
  const [webhookToken, setWebhookToken] = useState('');
  const [appSecret, setAppSecret] = useState('');

  const [modal, setModal] = useState(null); // { kind: 'type'|'subtype'|'priority'|'impact', mode: 'create'|'edit', data: {...} }
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadMasterData();
    loadSupportingData();
  }, []);

  async function loadMasterData() {
    try {
      const [tRes, stRes, pRes, iRes, wRes] = await Promise.all([
        api.get('/TicketTypes', { params: { includeInactive: true } }),
        api.get('/TicketSubTypes', { params: { includeInactive: true } }),
        api.get('/TicketPriorities', { params: { includeInactive: true } }),
        api.get('/TicketBusinessImpacts', { params: { includeInactive: true } }),
        api.get('/WhatsAppConfig').catch(() => ({ data: null }))
      ]);

      console.log('TicketTypes response:', tRes.data);
      console.log('TicketSubTypes response:', stRes.data);

      setTypes(tRes.data?.items || []);
      setSubTypes(stRes.data?.items || []);
      setPriorities(pRes.data?.items || []);
      setImpacts(iRes.data?.items || []);

      if (wRes.data) {
        setApiToken(wRes.data.encryptedApiToken || '');
        setWebhookToken(wRes.data.webhookVerifyToken || '');
        setAppSecret(wRes.data.appSecret || '');
      }
    } catch (err) {
      console.error('Failed to load master data:', err);
      setError(err.response?.data?.message || 'Failed to load master data.');
    }
  }

  async function loadSupportingData() {
    try {
      const [dRes, eRes] = await Promise.all([
        api.get('/departments', { params: { pageSize: 100 } }),
        api.get('/employees', { params: { pageSize: 200 } })
      ]);
      setDepartments(dRes.data.items || []);
      setEmployees(eRes.data.items || []);
    } catch {
      // fallback
    }
  }

  function openCreate(kind) {
    const defaults = { type: EMPTY_TYPE, subtype: EMPTY_SUBTYPE, priority: EMPTY_PRIORITY, impact: EMPTY_IMPACT };
    setError('');
    setModal({ kind, mode: 'create', data: { ...defaults[kind] } });
  }

  function openEdit(kind, item) {
    setError('');
    const dataMap = {
      type: { name: item.name, description: item.description || '', status: item.status },
      subtype: {
        name: item.name,
        description: item.description || '',
        ticketTypeId: item.ticketTypeId,
        departmentId: item.departmentId,
        defaultUserId: item.defaultUserId || '',
        status: item.status
      },
      priority: { name: item.name, level: item.level, slaInHours: item.slaInHours, status: item.status },
      impact: { name: item.name, description: item.description || '', status: item.status }
    };
    setModal({ kind, mode: 'edit', id: idFor(kind, item), data: dataMap[kind] });
  }

  function idFor(kind, item) {
    if (kind === 'type') return item.id || item.ticketTypeId;
    if (kind === 'subtype') return item.id || item.ticketSubTypeId;
    if (kind === 'priority') return item.id || item.ticketPriorityId;
    return item.id || item.ticketBusinessTypeImpactId;
  }

  function endpointFor(kind) {
    return { type: '/TicketTypes', subtype: '/TicketSubTypes', priority: '/TicketPriorities', impact: '/TicketBusinessImpacts' }[kind];
  }

  async function handleSave(e) {
    e.preventDefault();
    if (!modal) return;

    setSaving(true);
    setError('');

    try {
      const url = endpointFor(modal.kind);

      const payload = { ...modal.data };

      if (modal.kind === 'subtype') {
        payload.defaultUserId = modal.data.defaultUserId || null;
      }

      if (modal.mode === 'create') {
        await api.post(url, payload);
      } else {
        await api.put(`${url}/${modal.id}`, {
          ...payload,
          status: payload.status ?? true
        });
      }

      setModal(null);
      await loadMasterData();

    } catch (err) {
      setError(err.response?.data?.message || 'Save failed.');
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(kind, item) {
    const id = idFor(kind, item);
    if (!window.confirm(`Delete "${item.name}"? This cannot be undone.`)) return;
    try {
      await api.delete(`${endpointFor(kind)}/${id}`);
      await loadMasterData();
    } catch (err) {
      alert(err.response?.data?.message || 'Delete failed — it may be in use by an open ticket.');
    }
  }

  async function handleSaveIntegrations(e) {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      await api.post('/WhatsAppConfig', {
        provider: 0,
        businessPhoneNumberId: '',
        encryptedApiToken: apiToken,
        webhookVerifyToken: webhookToken,
        appSecret: appSecret
      });
      alert('System Integration settings saved securely.');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save integration settings.');
    } finally {
      setSaving(false);
    }
  }

  const renderModalForm = () => {
    if (!modal) return null;
    const { kind, data } = modal;
    const set = (field, value) => setModal({ ...modal, data: { ...data, [field]: value } });

    return (
      <div className="modal-backdrop" onClick={() => setModal(null)}>
        <div className="modal" onClick={(e) => e.stopPropagation()}>
          <div className="modal-header">
            <h2>{modal.mode === 'create' ? 'Add' : 'Edit'} {kind === 'subtype' ? 'Sub-Type' : kind.charAt(0).toUpperCase() + kind.slice(1)}</h2>
            <button className="modal-close" onClick={() => setModal(null)}><X size={18} /></button>
          </div>
          <form onSubmit={handleSave}>
            <div className="modal-body">
              {error && <div className="field-error">{error}</div>}

              <div className="form-group">
                <label>Name</label>
                <input required value={data.name} onChange={(e) => set('name', e.target.value)} />
              </div>

              {(kind === 'type' || kind === 'subtype' || kind === 'impact') && (
                <div className="form-group">
                  <label>Description</label>
                  <textarea rows={3} value={data.description} onChange={(e) => set('description', e.target.value)} />
                </div>
              )}

              {kind === 'subtype' && (
                <>
                  <div className="form-group">
                    <label>Parent Type</label>
                    <select required value={data.ticketTypeId} onChange={(e) => set('ticketTypeId', e.target.value)}>
                      <option value="">Select type</option>
                      {types.map((t) => (
                        <option key={t.id || t.ticketTypeId} value={t.id || t.ticketTypeId}>{t.name}</option>
                      ))}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Department</label>
                    <select required value={data.departmentId} onChange={(e) => set('departmentId', e.target.value)}>
                      <option value="">Select department</option>
                      {departments.map((d) => (
                        <option key={d.id || d.departmentId} value={d.id || d.departmentId}>{d.name}</option>
                      ))}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Default Agent (optional)</label>
                    <select value={data.defaultUserId} onChange={(e) => set('defaultUserId', e.target.value)}>
                      <option value="">Auto-routing (no fixed agent)</option>
                      {employees
                        .filter((e) => e.departmentId === data.departmentId)
                        .map((e) => (
                          <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>
                        ))}
                    </select>
                  </div>
                </>
              )}

              {kind === 'priority' && (
                <>
                  <div className="form-group">
                    <label>Level (severity order — 1 = highest)</label>
                    <input required type="number" min="1" value={data.level} onChange={(e) => set('level', Number(e.target.value))} />
                  </div>
                  <div className="form-group">
                    <label>SLA Hours</label>
                    <input required type="number" min="0" value={data.slaInHours} onChange={(e) => set('slaInHours', Number(e.target.value))} />
                  </div>
                </>
              )}

              {modal.mode === 'edit' && (
                <div className="form-group">
                  <label>
                    <input
                      type="checkbox"
                      checked={data.status !== false}
                      onChange={(e) => set('status', e.target.checked)}
                      style={{ marginRight: '0.5rem' }}
                    />
                    Active
                  </label>
                </div>
              )}
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn--secondary" onClick={() => setModal(null)}>Cancel</button>
              <button type="submit" className="btn btn--primary" disabled={saving}>
                {saving ? 'Saving...' : 'Save'}
              </button>
            </div>
          </form>
        </div>
      </div>
    );
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Layers size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Master Ticket Configuration</h1>
          <p>Module 1 — Ticket Types, Sub-Types (Type → Dept → Default Agent cascade), SLA Priorities, and Business Impacts.</p>
        </div>
      </div>

      <div className="glass-card" style={{ padding: '1rem', marginBottom: '1.25rem' }}>
        <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button className={`btn btn--sm ${activeTab === 'types' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('types')}>
              Ticket Types ({types.length})
            </button>
            <button className={`btn btn--sm ${activeTab === 'subtypes' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('subtypes')}>
              Sub-Types & Routing ({subTypes.length})
            </button>
            <button className={`btn btn--sm ${activeTab === 'priorities' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('priorities')}>
              SLA Priorities ({priorities.length})
            </button>
            <button className={`btn btn--sm ${activeTab === 'impacts' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('impacts')}>
              Business Impacts ({impacts.length})
            </button>
            <button className={`btn btn--sm ${activeTab === 'integrations' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('integrations')}>
              System Integrations
            </button>
          </div>
          {activeTab !== 'integrations' && (
            <button
              className="btn btn--primary btn--sm"
              onClick={() => {
                const kindMap = {
                  types: 'type',
                  subtypes: 'subtype',
                  priorities: 'priority',
                  impacts: 'impact'
                };

                openCreate(kindMap[activeTab]);
              }}
            >
              <Plus size={14} /> Add
            </button>
          )}
        </div>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          {activeTab === 'types' && (
            <table className="data-table">
              <thead>
                <tr><th>Type Name</th><th>Description</th><th>Status</th><th></th></tr>
              </thead>
              <tbody>
                {types.map((t) => (
                  <tr key={t.id || t.ticketTypeId}>
                    <td><strong>{t.name}</strong></td>
                    <td>{t.description || 'No description'}</td>
                    <td><span className={`badge ${t.status ? 'badge--resolved' : 'badge--error'}`}>{t.status ? 'Active' : 'Inactive'}</span></td>
                    <td>
                      <div className="row-actions">
                        <button className="icon-btn" onClick={() => openEdit('type', t)}><Edit2 size={14} /></button>
                        <button className="icon-btn" onClick={() => handleDelete('type', t)}><Trash2 size={14} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {activeTab === 'subtypes' && (
            <table className="data-table">
              <thead>
                <tr><th>Sub-Type Name</th><th>Parent Category</th><th>Assigned Department</th><th>Default Fixed Agent</th><th></th></tr>
              </thead>
              <tbody>
                {subTypes.map((st) => (
                  <tr key={st.id || st.ticketSubTypeId}>
                    <td><strong>{st.name}</strong></td>
                    <td>{st.ticketTypeName || '—'}</td>
                    <td>{st.departmentName || '—'}</td>
                    <td>{st.defaultUserName || 'Auto-Routing Engine'}</td>
                    <td>
                      <div className="row-actions">
                        <button className="icon-btn" onClick={() => openEdit('subtype', st)}><Edit2 size={14} /></button>
                        <button className="icon-btn" onClick={() => handleDelete('subtype', st)}><Trash2 size={14} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {activeTab === 'priorities' && (
            <table className="data-table">
              <thead>
                <tr><th>Priority Level</th><th>Severity Rank</th><th>SLA Target</th><th></th></tr>
              </thead>
              <tbody>
                {priorities.map((p) => (
                  <tr key={p.id || p.ticketPriorityId}>
                    <td><strong>{p.name}</strong></td>
                    <td>Level {p.level}</td>
                    <td><span className="badge badge--progress">{p.slaInHours} Hours</span></td>
                    <td>
                      <div className="row-actions">
                        <button className="icon-btn" onClick={() => openEdit('priority', p)}><Edit2 size={14} /></button>
                        <button className="icon-btn" onClick={() => handleDelete('priority', p)}><Trash2 size={14} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {activeTab === 'impacts' && (
            <table className="data-table">
              <thead>
                <tr><th>Impact Title</th><th>Scope Description</th><th>Status</th><th></th></tr>
              </thead>
              <tbody>
                {impacts.map((imp) => (
                  <tr key={imp.id || imp.ticketBusinessTypeImpactId}>
                    <td><strong>{imp.name}</strong></td>
                    <td>{imp.description || '—'}</td>
                    <td><span className={`badge ${imp.status ? 'badge--resolved' : 'badge--error'}`}>{imp.status ? 'Active' : 'Inactive'}</span></td>
                    <td>
                      <div className="row-actions">
                        <button className="icon-btn" onClick={() => openEdit('impact', imp)}><Edit2 size={14} /></button>
                        <button className="icon-btn" onClick={() => handleDelete('impact', imp)}><Trash2 size={14} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {activeTab === 'integrations' && (
            <div style={{ padding: '1rem', maxWidth: '600px' }}>
              <div style={{ marginBottom: '1.5rem', fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                <strong style={{ color: 'var(--text-primary)' }}>🔒 IT Administrator Settings</strong>
                <p style={{ marginTop: '0.4rem' }}>
                  Configure secure API tokens and secrets for external channels. These tokens grant backend system access and are hidden from regular support staff.
                </p>
                {error && <div className="field-error" style={{ marginTop: '1rem' }}>{error}</div>}
              </div>

              <form onSubmit={handleSaveIntegrations}>
                <div className="form-group">
                  <label>Meta Cloud API Permanent Access Token</label>
                  <textarea rows={3} value={apiToken} onChange={(e) => setApiToken(e.target.value)} placeholder="Leave blank to keep existing" />
                </div>

                <div className="form-group">
                  <label>App Secret</label>
                  <input type="password" value={appSecret} onChange={(e) => setAppSecret(e.target.value)} placeholder="Leave blank to keep existing" />
                </div>

                <div className="form-group">
                  <label>Webhook Verify Token</label>
                  <input value={webhookToken} onChange={(e) => setWebhookToken(e.target.value)} placeholder="Leave blank to keep existing" />
                </div>

                <div style={{ marginTop: '1.5rem' }}>
                  <button type="submit" className="btn btn--primary" disabled={saving}>
                    {saving ? 'Saving...' : 'Save System Integrations'}
                  </button>
                </div>
              </form>
            </div>
          )}
        </div>
      </div>

      {renderModalForm()}
    </div>
  );
}