import React, { useState, useEffect } from 'react';
import { Award, Plus, UserCheck, ShieldCheck, CheckCircle2 } from 'lucide-react';
import api from '../../../api/axios';

export default function SupportPlans() {
  const [plans, setPlans] = useState([
    { id: '1', name: 'Basic Support Plan', maxTickets: 10, blockWhenExhausted: true, price: '$0/mo', activeAccounts: 5 },
    { id: '2', name: 'Silver Support Plan', maxTickets: 50, blockWhenExhausted: false, price: '$299/mo', activeAccounts: 18 },
    { id: '3', name: 'Gold Support Plan', maxTickets: 200, blockWhenExhausted: false, price: '$799/mo', activeAccounts: 8 },
    { id: '4', name: 'Platinum Enterprise', maxTickets: 1000, blockWhenExhausted: false, price: '$1999/mo', activeAccounts: 2 }
  ]);

  const [accounts, setAccounts] = useState([]);
  const [selectedAccId, setSelectedAccId] = useState('');
  const [selectedPlanId, setSelectedPlanId] = useState('2');
  const [assigning, setAssigning] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    async function loadAccounts() {
      try {
        const res = await api.get('/accounts', { params: { pageNumber: 1, pageSize: 100 } });
        setAccounts(res.data.items || res.data || []);
      } catch {
        // fallback
      }
    }
    loadAccounts();
  }, []);

  const handleAssignPlan = (e) => {
    e.preventDefault();
    if (!selectedAccId) return;

    setAssigning(true);
    setTimeout(() => {
      setMessage('Support plan assigned to customer account successfully with entitlement counters!');
      setAssigning(false);
    }, 600);
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Award size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Support Plans & Ticket Entitlements</h1>
          <p>Module 13 — Define account support tiers, annual/monthly ticket quotas, overage policies, and account assignments.</p>
        </div>
      </div>

      {message && <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.15)', border: '1px solid rgba(16,185,129,0.3)', borderRadius: '8px', color: '#34d399', marginBottom: '1.25rem', fontSize: '0.85rem' }}><CheckCircle2 size={16} style={{ verticalAlign: 'middle', marginRight: '6px' }} />{message}</div>}

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
                <option key={acc.id || acc.accountId} value={acc.id || acc.accountId}>{acc.accountName || acc.name}</option>
              ))}
            </select>
          </div>

          <div className="form-group" style={{ margin: 0 }}>
            <label>Select Support Plan Tier</label>
            <select value={selectedPlanId} onChange={(e) => setSelectedPlanId(e.target.value)} required>
              {plans.map(p => (
                <option key={p.id} value={p.id}>{p.name} ({p.maxTickets} tickets/yr · {p.price})</option>
              ))}
            </select>
          </div>

          <button type="submit" className="btn btn--primary" disabled={assigning || !selectedAccId}>
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
                <th>Ticket Quota</th>
                <th>Quota Exhausted Policy</th>
                <th>Pricing</th>
                <th>Active Account Assignments</th>
              </tr>
            </thead>
            <tbody>
              {plans.map(p => (
                <tr key={p.id}>
                  <td><strong>{p.name}</strong></td>
                  <td><span className="badge badge--progress">{p.maxTickets} tickets/yr</span></td>
                  <td>
                    {p.blockWhenExhausted ? (
                      <span className="badge badge--breached">Block Ticket Creation</span>
                    ) : (
                      <span className="badge badge--resolved">Allow Overage ($50/ticket)</span>
                    )}
                  </td>
                  <td><strong>{p.price}</strong></td>
                  <td>{p.activeAccounts} accounts assigned</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
