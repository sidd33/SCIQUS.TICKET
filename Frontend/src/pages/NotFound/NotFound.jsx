import React from 'react';
import { Link } from 'react-router-dom';
import { AlertTriangle } from 'lucide-react';

export default function NotFound() {
  return (
    <div className="tickets-page" style={{ textAlign: 'center', paddingTop: '4rem' }}>
      <div className="glass-card" style={{ maxWidth: '450px', margin: '0 auto', padding: '2.5rem' }}>
        <AlertTriangle size={48} color="#f87171" style={{ marginBottom: '1rem' }} />
        <h1 style={{ color: 'white', fontSize: '1.5rem', marginBottom: '0.5rem' }}>404 — Page Not Found</h1>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem', marginBottom: '1.5rem' }}>
          The page or route you are attempting to access does not exist or has been relocated.
        </p>
        <Link to="/dashboard" className="btn btn--primary">Return to Dashboard</Link>
      </div>
    </div>
  );
}
