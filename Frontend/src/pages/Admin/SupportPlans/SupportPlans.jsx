import React, { useState, useEffect } from 'react';
import { Award, UserCheck, CheckCircle2, AlertCircle, Plus, X, Edit2, Save } from 'lucide-react';
import api from '../../../api/axios';

export default function SupportPlans() {
  const [plans, setPlans] = useState([]);
  const [accounts, setAccounts] = useState([]);
  const [loadingPlans, setLoadingPlans] = useState(true);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  // Assign form state
  const [selectedAccId, setSelectedAccId] = useState('');
  const [selectedPlanId, setSelectedPlanId] = useState('');
  const [assigning, setAssigning] = useState(false);

  // Create plan form state
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [creating, setCreating] = useState(false);
  const [newPlan, setNewPlan] = useState({
    name: '',
    description: '',
    ticketQuota: 50,
    periodType: 'Monthly',
    validityDays: 30,
    blockWhenExhausted: false
  });

  // Detail / edit modal state
  const [selectedPlan, setSelectedPlan] = useState(null); // plan object or null
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadPlans();
    loadAccounts();
  }, []);

  async function loadPlans() {
    setLoadingPlans(true);
    setError('');
    try {
      const res = await api.get('/SupportPlan');
      const list = res.data.items || res.data || [];
      setPlans(list);
      if (list.length > 0) setSelectedPlanId(list[0].supportPlanId);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load support plans.');
    } finally {
      setLoadingPlans(false);
    }
  }

  async function loadAccounts() {
    try {
      const res = await api.get('/accounts', { params: { pageNumber: 1, pageSize: 100 } });
      setAccounts(res.data.items || res.data || []);
    } catch (err) {
      console.error('failed to load accounts:', err);
    }
  }

  const handleCreatePlan = async (e) => {
    e.preventDefault();
    setCreating(true);
    setError('');
    try {
      await api.post('/SupportPlan', {
        name: newPlan.name,
        description: newPlan.description,
        ticketQuota: Number(newPlan.ticketQuota),
        periodType: newPlan.periodType,
        validityDays: Number(newPlan.validityDays),
        blockWhenExhausted: newPlan.blockWhenExhausted
      });
      setMessage(`Support plan "${newPlan.name}" created successfully.`);
      setNewPlan({ name: '', description: '', ticketQuota: 50, periodType: 'Monthly', validityDays: 30, blockWhenExhausted: false });
      setShowCreateForm(false);
      await loadPlans();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create support plan.');
    } finally {
      setCreating(false);
    }
  };

  const handleAssignPlan = async (e) => {
    e.preventDefault();
    if (!selectedAccId || !selectedPlanId) return;

    setAssigning(true);
    setError('');
    setMessage('');
    try {
      await api.post('/SupportPlan/assign', {
        accountId: selectedAccId,
        supportPlanId: selectedPlanId
      });
      setMessage('Support plan assigned to customer account successfully with entitlement counters!');
      setSelectedAccId('');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to assign support plan.');
    } finally {
      setAssigning(false);
    }
  };

  const openPlanDetails = (plan) => {
    setSelectedPlan(plan);
    setEditForm({ ...plan });
    setIsEditing(false);
  };

  const closeModal = () => {
    setSelectedPlan(null);
    setEditForm(null);
    setIsEditing(false);
  };

  const handleSaveEdit = async () => {
    setSaving(true);
    setError('');
    try {
      await api.put(`/SupportPlan/${selectedPlan.supportPlanId}`, {
        name: editForm.name,
        description: editForm.description,
        ticketQuota: Number(editForm.ticketQuota),
        periodType: editForm.periodType,
        validityDays: Number(editForm.validityDays),
        blockWhenExhausted: editForm.blockWhenExhausted
      });
      setMessage(`Support plan "${editForm.name}" updated successfully.`);
      await loadPlans();
      closeModal();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update support plan.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Award size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Support Plans & Ticket Entitlements</h1>
          <p>Define account support tiers, ticket quotas, overage policies, and account assignments.</p>
        </div>
        <button className="btn btn--primary" onClick={() => setShowCreateForm(v => !v)}>
          {showCreateForm ? <><X size={16} /> Cancel</> : <><Plus size={16} /> New Plan</>}
        </button>
      </div>

      {message && (
        <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem' }}>
          <CheckCircle2 size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />{message}
        </div>
      )}

      {error && (
        <div style={{ padding: '0.75rem 1rem', background: 'rgba(239,68,68,0.15)', border: '1px solid rgba(239,68,68,0.3)', borderRadius: '8px', color: '#f87171', marginBottom: '1.25rem', fontSize: '0.85rem' }}>
          <AlertCircle size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />{error}
        </div>
      )}

      {showCreateForm && (
        <div className="glass-card" style={{ padding: '1.25rem', marginBottom: '1.5rem' }}>
          <h3 style={{ color: 'white', fontSize: '1.05rem', margin: '0 0 1rem 0' }}>Create New Support Plan</h3>
          <form onSubmit={handleCreatePlan} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <div className="form-group" style={{ margin: 0 }}>
              <label>Plan Name</label>
              <input
                type="text"
                value={newPlan.name}
                onChange={(e) => setNewPlan({ ...newPlan, name: e.target.value })}
                required
              />
            </div>
            <div className="form-group" style={{ margin: 0 }}>
              <label>Description</label>
              <input
                type="text"
                value={newPlan.description}
                onChange={(e) => setNewPlan({ ...newPlan, description: e.target.value })}
              />
            </div>
            <div className="form-group" style={{ margin: 0 }}>
              <label>Ticket Quota</label>
              <input
                type="number"
                min="0"
                value={newPlan.ticketQuota}
                onChange={(e) => setNewPlan({ ...newPlan, ticketQuota: e.target.value })}
                required
              />
            </div>
            <div className="form-group" style={{ margin: 0 }}>
              <label>Period Type</label>
              <select
                value={newPlan.periodType}
                onChange={(e) => setNewPlan({ ...newPlan, periodType: e.target.value })}
              >
                <option value="Monthly">Monthly</option>
                <option value="Annual">Annual</option>
              </select>
            </div>
            <div className="form-group" style={{ margin: 0 }}>
              <label>Validity (Days)</label>
              <input
                type="number"
                min="0"
                value={newPlan.validityDays}
                onChange={(e) => setNewPlan({ ...newPlan, validityDays: e.target.value })}
              />
            </div>
            <div className="form-group" style={{ margin: 0 }}>
              <label>Exhausted Policy</label>
              <select
                value={newPlan.blockWhenExhausted ? 'block' : 'overage'}
                onChange={(e) => setNewPlan({ ...newPlan, blockWhenExhausted: e.target.value === 'block' })}
              >
                <option value="overage">Allow Overage</option>
                <option value="block">Block Ticket Creation</option>
              </select>
            </div>
            <div style={{ gridColumn: '1 / -1' }}>
              <button type="submit" className="btn btn--primary" disabled={creating}>
                {creating ? 'Creating...' : 'Create Plan'}
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="glass-card" style={{ padding: '1.25rem', marginBottom: '1.5rem' }}>
        <h3 style={{ color: 'white', fontSize: '1.05rem', margin: '0 0 1rem 0', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <UserCheck size={18} /> Assign Support Plan to Customer Account
        </h3>
        <form onSubmit={handleAssignPlan} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr auto', gap: '1rem', alignItems: 'end' }}>
          <div className="form-group" style={{ margin: 0 }}>
            <label>Select Customer Account</label>
            <select value={selectedAccId} onChange={(e) => setSelectedAccId(e.target.value)} required>
              <option value="">Select Account...</option>
              {accounts.map(acc => (
                <option key={acc.accountId} value={acc.accountId}>
                  {acc.accountName}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group" style={{ margin: 0 }}>
            <label>Select Support Plan Tier</label>
            <select value={selectedPlanId} onChange={(e) => setSelectedPlanId(e.target.value)} required disabled={loadingPlans || plans.length === 0}>
              {plans.length === 0 && <option value="">No plans available</option>}
              {plans.map(p => (
                <option key={p.supportPlanId} value={p.supportPlanId}>
                  {p.name} ({p.ticketQuota} tickets / {p.periodType})
                </option>
              ))}
            </select>
          </div>

          <button type="submit" className="btn btn--primary" disabled={assigning || !selectedAccId || !selectedPlanId}>
            {assigning ? 'Assigning Plan...' : 'Assign Plan & Entitlements'}
          </button>
        </form>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Plan Tier Title</th>
                <th>Description</th>
                <th>Ticket Quota</th>
                <th>Period</th>
                <th>Validity</th>
                <th>Quota Exhausted Policy</th>
              </tr>
            </thead>
            <tbody>
              {loadingPlans ? (
                <tr><td colSpan={6}>Loading plans...</td></tr>
              ) : plans.length === 0 ? (
                <tr><td colSpan={6}>No support plans found.</td></tr>
              ) : (
                plans.map(p => (
                  <tr
                    key={p.supportPlanId}
                    onClick={() => openPlanDetails(p)}
                    style={{ cursor: 'pointer' }}
                  >
                    <td><strong>{p.name}</strong></td>
                    <td>{p.description}</td>
                    <td><span className="badge badge--progress">{p.ticketQuota} tickets</span></td>
                    <td>{p.periodType}</td>
                    <td>{p.validityDays} days</td>
                    <td>
                      {p.blockWhenExhausted ? (
                        <span className="badge badge--breached">Block Ticket Creation</span>
                      ) : (
                        <span className="badge badge--resolved">Allow Overage</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {selectedPlan && editForm && (
        <div
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.6)',
            display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
          }}
          onClick={closeModal}
        >
          <div
            className="glass-card"
            style={{ width: '520px', maxWidth: '90vw', padding: '1.5rem', position: 'relative' }}
            onClick={(e) => e.stopPropagation()}
          >
            <button
              onClick={closeModal}
              style={{ position: 'absolute', top: '1rem', right: '1rem', background: 'none', border: 'none', color: '#94a3b8', cursor: 'pointer' }}
            >
              <X size={20} />
            </button>

            <h3 style={{ color: 'white', fontSize: '1.15rem', margin: '0 0 1.25rem 0', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Award size={18} />
              {isEditing ? 'Edit Support Plan' : selectedPlan.name}
            </h3>

            {isEditing ? (
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="form-group" style={{ margin: 0, gridColumn: '1 / -1' }}>
                  <label>Plan Name</label>
                  <input
                    type="text"
                    value={editForm.name}
                    onChange={(e) => setEditForm({ ...editForm, name: e.target.value })}
                  />
                </div>
                <div className="form-group" style={{ margin: 0, gridColumn: '1 / -1' }}>
                  <label>Description</label>
                  <input
                    type="text"
                    value={editForm.description}
                    onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
                  />
                </div>
                <div className="form-group" style={{ margin: 0 }}>
                  <label>Ticket Quota</label>
                  <input
                    type="number"
                    min="0"
                    value={editForm.ticketQuota}
                    onChange={(e) => setEditForm({ ...editForm, ticketQuota: e.target.value })}
                  />
                </div>
                <div className="form-group" style={{ margin: 0 }}>
                  <label>Period Type</label>
                  <select
                    value={editForm.periodType}
                    onChange={(e) => setEditForm({ ...editForm, periodType: e.target.value })}
                  >
                    <option value="Monthly">Monthly</option>
                    <option value="Annual">Annual</option>
                  </select>
                </div>
                <div className="form-group" style={{ margin: 0 }}>
                  <label>Validity (Days)</label>
                  <input
                    type="number"
                    min="0"
                    value={editForm.validityDays}
                    onChange={(e) => setEditForm({ ...editForm, validityDays: e.target.value })}
                  />
                </div>
                <div className="form-group" style={{ margin: 0 }}>
                  <label>Exhausted Policy</label>
                  <select
                    value={editForm.blockWhenExhausted ? 'block' : 'overage'}
                    onChange={(e) => setEditForm({ ...editForm, blockWhenExhausted: e.target.value === 'block' })}
                  >
                    <option value="overage">Allow Overage</option>
                    <option value="block">Block Ticket Creation</option>
                  </select>
                </div>

                <div style={{ gridColumn: '1 / -1', display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                  <button className="btn btn--primary" onClick={handleSaveEdit} disabled={saving}>
                    <Save size={16} style={{ verticalAlign: 'middle', marginRight: '4px' }} />
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                  <button
                    className="btn"
                    onClick={() => { setEditForm({ ...selectedPlan }); setIsEditing(false); }}
                    disabled={saving}
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <div style={{ display: 'grid', gap: '0.85rem', fontSize: '0.9rem', color: '#e2e8f0' }}>
                <div><span style={{ color: '#94a3b8' }}>Description: </span>{selectedPlan.description || '—'}</div>
                <div><span style={{ color: '#94a3b8' }}>Ticket Quota: </span>{selectedPlan.ticketQuota} tickets</div>
                <div><span style={{ color: '#94a3b8' }}>Period Type: </span>{selectedPlan.periodType}</div>
                <div><span style={{ color: '#94a3b8' }}>Validity: </span>{selectedPlan.validityDays} days</div>
                <div>
                  <span style={{ color: '#94a3b8' }}>Exhausted Policy: </span>
                  {selectedPlan.blockWhenExhausted ? (
                    <span className="badge badge--breached">Block Ticket Creation</span>
                  ) : (
                    <span className="badge badge--resolved">Allow Overage</span>
                  )}
                </div>
                <div><span style={{ color: '#94a3b8' }}>Status: </span>{selectedPlan.status ? 'Active' : 'Inactive'}</div>
                <div><span style={{ color: '#94a3b8', fontSize: '0.75rem' }}>Plan ID: {selectedPlan.supportPlanId}</span></div>

                <button
                  className="btn btn--primary"
                  style={{ marginTop: '0.5rem', width: 'fit-content' }}
                  onClick={() => setIsEditing(true)}
                >
                  <Edit2 size={16} style={{ verticalAlign: 'middle', marginRight: '4px' }} />
                  Edit Plan
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}