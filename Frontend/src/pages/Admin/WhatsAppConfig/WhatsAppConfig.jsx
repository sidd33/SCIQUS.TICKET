import React, { useState } from 'react';
import { MessageSquare, Save, CheckCircle2 } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function WhatsAppConfig() {
  const [phoneNumberId, setPhoneNumberId] = useState('10948291048192');
  const [verifyToken, setVerifyToken] = useState('SCIQUS_WHATSAPP_TOKEN_2026');
  const [accessToken, setAccessToken] = useState('EAAG...meta_cloud_access_token');
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');

  const handleSave = (e) => {
    e.preventDefault();
    setSaving(true);
    setTimeout(() => {
      setMessage('WhatsApp Cloud API integration settings saved!');
      setSaving(false);
    }, 600);
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><MessageSquare size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> WhatsApp Channel Configuration</h1>
          <p>Module 8 — Meta Cloud API webhook verification, Access tokens, and message queue review.</p>
        </div>
        <Link to="/admin/whatsapp-inbox-review" className="btn btn--secondary">
          Review WhatsApp Inbox
        </Link>
      </div>

      {message && <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem' }}><CheckCircle2 size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />{message}</div>}

      <form onSubmit={handleSave} className="glass-card" style={{ padding: '1.5rem', maxWidth: '600px' }}>
        <div className="form-group">
          <label>WhatsApp Business Phone Number ID</label>
          <input value={phoneNumberId} onChange={(e) => setPhoneNumberId(e.target.value)} required />
        </div>

        <div className="form-group">
          <label>Webhook Verify Token</label>
          <input value={verifyToken} onChange={(e) => setVerifyToken(e.target.value)} required />
        </div>

        <div className="form-group">
          <label>Meta Cloud API Permanent Access Token</label>
          <textarea rows={3} value={accessToken} onChange={(e) => setAccessToken(e.target.value)} required />
        </div>

        <div style={{ marginTop: '1.5rem' }}>
          <button type="submit" className="btn btn--primary" disabled={saving}>
            <Save size={16} /> {saving ? 'Saving...' : 'Save WhatsApp Settings'}
          </button>
        </div>
      </form>
    </div>
  );
}
