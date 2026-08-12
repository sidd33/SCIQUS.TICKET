import React, { useState, useEffect } from 'react';
import { Settings, Save, Clock, Sliders, ShieldCheck, CheckCircle2, AlertCircle } from 'lucide-react';
import api from '../../../api/axios';
import './SlaConfig.scss';

export default function SlaConfig() {
  const [config, setConfig] = useState({
    autoClosureHours: 48,
    allowEmployeeReopen: true,
    reopenGraceDays: 7,
    maxFallbackAttempts: 3,
    w_Load: 1.0,
    w_Severity: 0.5,
    w_Recency: 0.1,
    recencyCapHours: 48,
    ticketAutoAssignMethod: 'Auto_assignment_custom'
  });

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadSlaConfig() {
      try {
        const res = await api.get('/slaconfigurations/current');
        if (res.data) {
          setConfig(prev => ({ ...prev, ...res.data }));
        }
      } catch (err) {
        // use default fallback
      } finally {
        setLoading(false);
      }
    }
    loadSlaConfig();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setMessage('');
    setError('');
    try {
      await api.post('/slaconfigurations', config);
      setMessage('SLA and Auto-Assignment parameters updated successfully!');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save SLA configuration');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="sla-config-loading">Loading SLA parameters...</div>;

  return (
    <div className="sla-config-page">
      <div className="page-header">
        <div>
          <h1><Settings size={22} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Global SLA & Routing Configuration</h1>
          <p>Configure auto-closure confirmation windows, reopen policies, agent acceptance timeouts, and auto-assignment formula weights.</p>
        </div>
      </div>

      {message && <div className="success-banner"><CheckCircle2 size={16} /> {message}</div>}
      {error && <div className="error-banner"><AlertCircle size={16} /> {error}</div>}

      <form onSubmit={handleSubmit} className="config-form">
        {/* Section 1: Auto-Closure & Reopen Rules */}
        <div className="card config-card">
          <div className="card-header">
            <h3><Clock size={18} /> Module 4 — Auto-Closure & Reopen Rules</h3>
          </div>
          <div className="card-body">
            <div className="field">
              <label>Auto-Closure Confirmation Window (Hours) *</label>
              <input
                type="number"
                min="1"
                required
                value={config.autoClosureHours || 48}
                onChange={e => setConfig({ ...config, autoClosureHours: parseInt(e.target.value) || 1 })}
              />
              <span className="field-hint">Hours a resolved ticket stays in PendingClosure before system auto-confirms closure.</span>
            </div>

            <div className="field-checkbox">
              <label>
                <input
                  type="checkbox"
                  checked={config.allowEmployeeReopen ?? true}
                  onChange={e => setConfig({ ...config, allowEmployeeReopen: e.target.checked })}
                />
                Allow Customers to Reopen Closed Tickets
              </label>
            </div>

            <div className="field">
              <label>Post-Closure Reopen Grace Window (Days) *</label>
              <input
                type="number"
                min="1"
                required
                value={config.reopenGraceDays || 7}
                onChange={e => setConfig({ ...config, reopenGraceDays: parseInt(e.target.value) || 1 })}
              />
              <span className="field-hint">Days after ClosedDate during which customers may reopen. Beyond this, a linked follow-up ticket is created.</span>
            </div>
          </div>
        </div>

        {/* Section 2: Auto-Assignment Custom Formula Weights */}
        <div className="card config-card">
          <div className="card-header">
            <h3><Sliders size={18} /> Module 3 — Auto-Assignment Multi-Factor Scoring Formula</h3>
          </div>
          <div className="card-body">
            <div className="formula-box">
              <code>
                Score(e) = (W_load × OpenCount(e)) + (W_severity × SeverityLoad(e)) − (W_recency × min(HoursSinceLastAssigned(e), RecencyCapHours))
              </code>
            </div>

            <div className="grid-2">
              <div className="field">
                <label>Workload Weight (W_load) *</label>
                <input
                  type="number"
                  step="0.1"
                  required
                  value={config.w_Load ?? 1.0}
                  onChange={e => setConfig({ ...config, w_Load: parseFloat(e.target.value) || 0 })}
                />
              </div>

              <div className="field">
                <label>Severity Weight (W_severity) *</label>
                <input
                  type="number"
                  step="0.1"
                  required
                  value={config.w_Severity ?? 0.5}
                  onChange={e => setConfig({ ...config, w_Severity: parseFloat(e.target.value) || 0 })}
                />
              </div>

              <div className="field">
                <label>Recency Discount Weight (W_recency) *</label>
                <input
                  type="number"
                  step="0.05"
                  required
                  value={config.w_Recency ?? 0.1}
                  onChange={e => setConfig({ ...config, w_Recency: parseFloat(e.target.value) || 0 })}
                />
              </div>

              <div className="field">
                <label>Recency Cap Hours *</label>
                <input
                  type="number"
                  min="1"
                  required
                  value={config.recencyCapHours || 48}
                  onChange={e => setConfig({ ...config, recencyCapHours: parseInt(e.target.value) || 1 })}
                />
              </div>
            </div>
          </div>
        </div>

        {/* Section 3: Acceptance Fallback Limit */}
        <div className="card config-card">
          <div className="card-header">
            <h3><ShieldCheck size={18} /> Module 12 — Acceptance Rotation Fallback Limit</h3>
          </div>
          <div className="card-body">
            <div className="field">
              <label>Maximum Acceptance Rotation Attempts *</label>
              <input
                type="number"
                min="1"
                required
                value={config.maxFallbackAttempts || 3}
                onChange={e => setConfig({ ...config, maxFallbackAttempts: parseInt(e.target.value) || 1 })}
              />
              <span className="field-hint">Max number of fallback agent rotations on rejection/expiry before parking in department queue.</span>
            </div>
          </div>
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn--primary" disabled={saving}>
            <Save size={16} /> {saving ? 'Saving Parameters...' : 'Save Configuration'}
          </button>
        </div>
      </form>
    </div>
  );
}
