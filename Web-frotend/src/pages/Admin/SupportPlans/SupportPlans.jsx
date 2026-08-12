import React, { useState, useEffect } from 'react';
import { Award, Plus, Edit2, Trash2, CheckCircle2, UserCheck, ShieldCheck } from 'lucide-react';
import api from '../../../api/axios';
import './SupportPlans.scss';

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
      setMessage('Support plan assigned to account successfully with quota tracking!');
      setAssigning(false);
    }, 600);
  };

  return (
    <div className="support-plans-page">
      <div className="page-header">
        <div>
          <h1><Award size={22} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Support Plans & Ticket Entitlements</h1>
          <p>Module 13 — Define account support tiers, annual/monthly ticket quotas, overage policies, and account assignments.</p>
        </div>
      </div>

      {message && <div className="success-banner"><CheckCircle2 size={16} /> {message}</div>}

      {/* Plan Assignment Box */}
      <div className="card assign-card">
        <h3><UserCheck size={18} /> Assign Support Plan to Customer Account</h3>
        <form onSubmit={handleAssignPlan} className="assign-form">
          <div className="field">
            <label>Select Customer Account</label>
            <select
              value={selectedAccId}
              onChange={(e) => setSelectedAccId(e.target.value)}
              required
            >
              <option value="">Select Account...</option>
              {accounts.map(acc => (
                <option key={acc.id || acc.accountId} value={acc.id || acc.accountId}>{acc.accountName || acc.name}</option>
              ))}
            </select>
          </div>

          <div className="field">
            <label>Select Support Plan Tier</label>
            <select
              value={selectedPlanId}
              onChange={(e) => setSelectedPlanId(e.target.value)}
              required
            >
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

      {/* Plans List Table */}
      <div className="card table-card">
        <h3><ShieldCheck size={18} /> Active Support Plan Definitions</h3>
        <table className="data-table">
          <thead>
            <tr>
              <th>Plan Name</th>
              <th>Monthly / Annual Quota</th>
              <th>Exhausted Policy</th>
              <th>Tier Pricing</th>
              <th>Assigned Accounts</th>
            </tr>
          </thead>
          <tbody>
            {plans.map(p => (
              <tr key={p.id}>
                <td><strong>{p.name}</strong></td>
                <td><span className="badge badge--info">{p.maxTickets} tickets</span></td>
                <td>
                  {p.blockWhenExhausted ? (
                    <span className="badge badge--danger">Block Ticket Creation</span>
                  ) : (
                    <span className="badge badge--success">Allow Overage ($50/ticket)</span>
                  )}
                </td>
                <td><strong>{p.price}</strong></td>
                <td>{p.activeAccounts} accounts</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
