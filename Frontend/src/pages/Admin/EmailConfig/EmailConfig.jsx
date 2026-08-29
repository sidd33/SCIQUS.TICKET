import React, { useState, useEffect } from 'react';
import { Inbox, Save, CheckCircle2, Wifi, Clock, RefreshCw, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../../../api/axios';

export default function EmailConfig() {
  const queryParams = new URLSearchParams(window.location.search);
  const code = queryParams.get('code');
  const outlookLinked = queryParams.get('outlook_linked');

  const [provider, setProvider] = useState('Google Workspace API (OAuth)');
  const [email, setEmail] = useState('siddharthaswamy01@gmail.com');
  const [intervalMins, setIntervalMins] = useState(15);
  const [enableEmail, setEnableEmail] = useState(true);
  const [autoCreate, setAutoCreate] = useState(true);
  
  const [isLinked, setIsLinked] = useState(false);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');
  const [showErrorBanner, setShowErrorBanner] = useState(false);

  useEffect(() => {
    const fetchConfig = async () => {
      try {
        const response = await api.get('/EmailTicketConfig');
        if (response.data) {
          setEmail(response.data.emailAddress || 'siddharthaswamy01@gmail.com');
          setProvider(response.data.provider || 'Google Workspace API (OAuth)');
          setIntervalMins(response.data.pollingIntervalMinutes || 15);
          setEnableEmail(response.data.isEnabled ?? true);
          setAutoCreate(response.data.autoCreateEnabled ?? true);
          
          const active = response.data.isAuthValid && (response.data.provider?.includes('Google') || response.data.provider?.includes('OAuth'));
          setIsLinked(active);
          setShowErrorBanner(!response.data.isAuthValid && (response.data.provider?.includes('Google') || response.data.provider?.includes('OAuth')));
          if (active) {
            setMessage('Google account linked successfully! Polling channel is now active.');
          }
        }
      } catch (err) {
        console.error("Failed to load existing config", err);
      }
    };
    fetchConfig();
  }, []);

  useEffect(() => {
    if (code) {
      const linkAccount = async () => {
        try {
          setMessage('Linking Google account with backend...');
          await api.post('/EmailTicketConfig/LinkAccount', { code });
          setMessage('Google account linked successfully! Polling channel is now active.');
          setIsLinked(true);
          setShowErrorBanner(false);
          // Remove the code from the URL so page refreshes don't re-trigger it
          window.history.replaceState({}, document.title, window.location.pathname);
        } catch (err) {
          console.error(err);
          setMessage('Failed to link Google account. Ensure client ID/secret match Google console.');
          setShowErrorBanner(true);
        }
      };
      linkAccount();
    }
  }, [code]);

  useEffect(() => {
    if (outlookLinked === 'true') {
      setMessage('Microsoft 365 account linked successfully! Polling channel is now active.');
      setIsLinked(true);
      setShowErrorBanner(false);
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }, [outlookLinked]);

  const handlePoll = async () => {
    try {
      setMessage('Checking for new emails...');
      await api.post('/EmailTicketConfig/Poll');
      setMessage('Inbox polled successfully! Check the raw inbox review queue.');
    } catch (err) {
      console.error(err);
      setMessage('Failed to trigger inbox poll. Ensure your backend is running.');
    }
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      // 1. Post settings first to save them to the DB
      await api.post('/EmailTicketConfig', {
        provider: provider,
        emailAddress: email,
        pollingIntervalMinutes: parseInt(intervalMins),
        isEnabled: enableEmail,
        autoCreateEnabled: autoCreate
      });

      // 2. If Google OAuth is selected, redirect to Google consent screen
      if (provider.includes('Google') || provider.includes('OAuth')) {
        const googleClientId = '766142405662-bo5ksif7bd0u743v6epant533b644b3j.apps.googleusercontent.com';
        const redirectUri = 'http://localhost:5174/admin/email-ticket-config';
        const scope = 'https://mail.google.com/';
        const oauthUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${googleClientId}&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=code&scope=${encodeURIComponent(scope)}&access_type=offline&prompt=consent`;
        
        window.location.href = oauthUrl;
        return;
      }

      if (provider.includes('Microsoft') || provider.includes('365')) {
        window.location.href = 'https://localhost:7219/api/OutlookEmail/Login';
        setMessage('Redirecting to Microsoft 365 login...');
        setSaving(false);
        return;
      }

      setMessage('Email polling channel configuration saved successfully!');
    } catch (err) {
      console.error(err);
      setMessage('Failed to save configuration settings.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Inbox size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Email Channel Configuration</h1>
          <p style={{ marginTop: '0.25rem', color: '#9ca3af' }}>Configure IMAP inbound support email polling and auto-ticket creation.</p>
        </div>
        <Link to="/admin/email-inbox-review" className="btn btn--secondary">
          Review Raw Email Inbox Queue
        </Link>
      </div>

      {showErrorBanner && (
        <div style={{ 
          padding: '1rem', 
          background: 'rgba(220, 38, 38, 0.05)', 
          border: '1px solid rgba(220, 38, 38, 0.2)', 
          borderRadius: '6px', 
          color: '#ef4444', 
          marginBottom: '1.5rem', 
          display: 'flex', 
          alignItems: 'center',
          fontSize: '0.9rem'
        }}>
          <span style={{ marginRight: '8px', cursor: 'pointer', color: '#ef4444' }} onClick={() => setShowErrorBanner(false)}><X size={16} /></span>
          Failed to link account. Please check client IDs and try again.
        </div>
      )}

      {message && <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem', display: 'flex', alignItems: 'center' }}><CheckCircle2 size={16} style={{ marginRight: '6px' }} />{message}</div>}

      <div className="glass-card" style={{ padding: '1rem 1.5rem', marginBottom: '1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', flexWrap: 'wrap' }}>
          <div style={{ color: '#10b981', display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem' }}>
            <Wifi size={16} /> IMAP Connected
          </div>
          <div style={{ color: '#9ca3af', display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem' }}>
            <Clock size={16} /> Last polled: {isLinked ? 'Just now' : 'Never'}
          </div>
          {!isLinked && (
            <div style={{ color: '#ef4444', display: 'flex', alignItems: 'center', fontSize: '0.9rem' }}>
              Error: Authentication failed: Authentication failed.
            </div>
          )}
          {isLinked && (
            <div style={{ color: '#10b981', display: 'flex', alignItems: 'center', fontSize: '0.9rem', fontWeight: '500' }}>
              Status: Active & Authorized
            </div>
          )}
        </div>
        <button 
          type="button"
          onClick={handlePoll}
          className="btn btn--secondary" 
          style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.85rem', padding: '0.5rem 1rem' }}
        >
          <RefreshCw size={14} /> Poll Inbox Now
        </button>
      </div>

      <form onSubmit={handleSave} className="glass-card" style={{ padding: '2rem', maxWidth: '700px' }}>
        <div className="form-group" style={{ marginBottom: '1.5rem' }}>
          <label>Support Email Address (Receiving Inbox)</label>
          <input 
            value={email} 
            onChange={(e) => setEmail(e.target.value)} 
            required 
          />
          <small style={{ color: '#9ca3af', display: 'block', marginTop: '0.5rem' }}>This is the inbox the system polls for incoming support emails.</small>
        </div>

        <div className="form-group" style={{ marginBottom: '1.5rem' }}>
          <label>Email Provider</label>
          <select 
            value={provider} 
            onChange={(e) => setProvider(e.target.value)}
            style={{ width: '100%', padding: '0.75rem', background: 'rgba(255, 255, 255, 0.05)', border: '1px solid rgba(255, 255, 255, 0.1)', borderRadius: '6px', color: 'white', marginTop: '0.25rem', appearance: 'auto' }}
          >
            <option style={{ color: 'black' }}>Google Workspace API (OAuth)</option>
            <option style={{ color: 'black' }}>Microsoft 365</option>
            <option style={{ color: 'black' }}>Basic IMAP</option>
          </select>
        </div>

        <div className="form-group" style={{ marginBottom: '1.5rem' }}>
          <label>Polling Frequency (Minutes)</label>
          <input 
            type="number" 
            value={intervalMins} 
            onChange={(e) => setIntervalMins(Number(e.target.value))} 
            required 
          />
          <small style={{ color: '#9ca3af', display: 'block', marginTop: '0.5rem' }}>How often the backend checks for new emails. Minimum: 5 minutes.</small>
        </div>

        <div style={{ marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <input 
            type="checkbox" 
            id="enableEmail"
            checked={enableEmail} 
            onChange={(e) => setEnableEmail(e.target.checked)} 
            style={{ width: 'auto', margin: 0 }}
          />
          <label htmlFor="enableEmail" style={{ fontSize: '0.9rem', marginBottom: 0 }}>Enable Email Channel</label>
        </div>

        <div style={{ marginBottom: '2rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <input 
            type="checkbox" 
            id="autoCreate"
            checked={autoCreate} 
            onChange={(e) => setAutoCreate(e.target.checked)} 
            style={{ width: 'auto', margin: 0 }}
          />
          <label htmlFor="autoCreate" style={{ fontSize: '0.9rem', marginBottom: 0 }}>Auto-Create Tickets</label>
        </div>

        <div>
          <button type="submit" className="btn btn--primary" disabled={saving}>
            <Save size={16} style={{ marginRight: '8px' }} /> {saving ? 'Saving...' : 'Save Email Config'}
          </button>
        </div>
      </form>
    </div>
  );
}
