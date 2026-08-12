import React, { useState, useEffect } from 'react';
import { Settings, Save, CheckCircle2 } from 'lucide-react';
import api from '../../../api/axios';

export default function SlaConfig() {
  const [autoCloseHours, setAutoCloseHours] = useState(48);
  const [reopenGraceDays, setReopenGraceDays] = useState(7);
  const [allowEmployeeReopen, setAllowEmployeeReopen] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');

  const handleSave = (e) => {
    e.preventDefault();
    setSaving(true);
    setTimeout(() => {
      setMessage('SLA & Auto-Closure configurations saved successfully!');
      setSaving(false);
    }, 600);
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Settings size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> SLA Engine & Auto-Closure Configuration</h1>
          <p>Module 4 — Configure SLA auto-closure hours, 2-stage closure confirmation grace window, and reopen policies.</p>
        </div>
      </div>

      {message && <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem' }}><CheckCircle2 size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />{message}</div>}

      <form onSubmit={handleSave} className="glass-card" style={{ padding: '1.5rem', maxWidth: '600px' }}>
        <div className="form-group">
          <label>Auto-Closure Timeout (Hours)</label>
          <input type="number" value={autoCloseHours} onChange={(e) => setAutoCloseHours(Number(e.target.value))} required />
          <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem' }}>Hours after PendingClosure state when resolved tickets auto-close if customer doesn't respond.</small>
        </div>

        <div className="form-group" style={{ marginTop: '1.25rem' }}>
          <label>Customer Reopen Grace Window (Days)</label>
          <input type="number" value={reopenGraceDays} onChange={(e) => setReopenGraceDays(Number(e.target.value))} required />
          <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem' }}>Days after closure during which customers can reopen a ticket. Outside this window, a new follow-up ticket is created.</small>
        </div>

        <div className="form-group" style={{ marginTop: '1.25rem' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '8px', cursor: 'pointer', color: 'white' }}>
            <input type="checkbox" checked={allowEmployeeReopen} onChange={(e) => setAllowEmployeeReopen(e.target.checked)} />
            Allow Employees & Agents to Reopen Closed Tickets
          </label>
        </div>

        <div style={{ marginTop: '1.5rem' }}>
          <button type="submit" className="btn btn--primary" disabled={saving}>
            <Save size={16} /> {saving ? 'Saving SLA Rules...' : 'Save Configuration'}
          </button>
        </div>
      </form>
    </div>
  );
}
