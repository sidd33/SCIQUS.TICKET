import React, { useState, useEffect } from 'react';
import { MessageSquare, Save, CheckCircle2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../../../api/axios';

export default function WhatsAppConfig() {
  const [phoneNumberId, setPhoneNumberId] = useState('');
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    const fetchConfig = async () => {
      try {
        const response = await api.get('/WhatsAppConfig');
        if (response.data) {
          setPhoneNumberId(response.data.businessPhoneNumberId || '');
        }
      } catch (error) {
        console.error('Failed to fetch WhatsApp config:', error);
      }
    };
    fetchConfig();
  }, []);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/WhatsAppConfig', {
        provider: 0,
        businessPhoneNumberId: phoneNumberId,
        encryptedApiToken: '',
        webhookVerifyToken: '',
        appSecret: ''
      });
      setMessage('WhatsApp Cloud API integration settings saved!');
    } catch (error) {
      console.error('Failed to save config', error);
      setMessage('Failed to save settings.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><MessageSquare size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> WhatsApp Channel Configuration</h1>
          <p>Module 8 — Meta Cloud API webhook verification, Access tokens, and message queue review.</p>
        </div>
        <Link to="/admin/whatsapp-review" className="btn btn--secondary">
          Review WhatsApp Inbox
        </Link>
      </div>

      {message && <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem' }}><CheckCircle2 size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />{message}</div>}

      <form onSubmit={handleSave} className="glass-card" style={{ padding: '1.5rem', maxWidth: '600px' }}>
        <div className="form-group">
          <label>WhatsApp Phone Number</label>
          <input value={phoneNumberId} onChange={(e) => setPhoneNumberId(e.target.value)} placeholder="+1 (555) 663-8753" required />
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
