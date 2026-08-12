import React, { useState } from 'react';
import { Inbox, Save, CheckCircle2 } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function EmailConfig() {
  const [email, setEmail] = useState('support@sciqustickets.com');
  const [host, setHost] = useState('outlook.office365.com');
  const [port, setPort] = useState(993);
  const [intervalMins, setIntervalMins] = useState(5);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');

  const handleSave = (e) => {
    e.preventDefault();
    setSaving(true);
    setTimeout(() => {
      setMessage('Email polling channel configuration saved successfully!');
      setSaving(false);
    }, 600);
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Inbox size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Email Channel Configuration</h1>
          <p>Module 6 — Configure IMAP/SMTP inbound support email polling and raw inbox review.</p>
        </div>
        <Link to="/admin/email-inbox-review" className="btn btn--secondary">
          Review Raw Email Inbox Queue
        </Link>
      </div>

      {message && <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem' }}><CheckCircle2 size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />{message}</div>}

      <form onSubmit={handleSave} className="glass-card" style={{ padding: '1.5rem', maxWidth: '600px' }}>
        <div className="form-group">
          <label>Support Email Address</label>
          <input value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>IMAP Server Host</label>
            <input value={host} onChange={(e) => setHost(e.target.value)} required />
          </div>
          <div className="form-group">
            <label>IMAP Port</label>
            <input type="number" value={port} onChange={(e) => setPort(Number(e.target.value))} required />
          </div>
        </div>

        <div className="form-group">
          <label>Polling Frequency (Minutes)</label>
          <input type="number" value={intervalMins} onChange={(e) => setIntervalMins(Number(e.target.value))} required />
        </div>

        <div style={{ marginTop: '1.5rem' }}>
          <button type="submit" className="btn btn--primary" disabled={saving}>
            <Save size={16} /> {saving ? 'Saving...' : 'Save Email Config'}
          </button>
        </div>
      </form>
    </div>
  );
}
