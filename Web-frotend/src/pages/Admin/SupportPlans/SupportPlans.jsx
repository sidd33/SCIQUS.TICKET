import React, { useState, useEffect } from 'react';
import { Award, Plus, CheckCircle2, UserCheck, ShieldCheck, Loader } from 'lucide-react';
import api from '../../../api/axios';
import './SupportPlans.scss';

export default function SupportPlans() {
  const [plans, setPlans] = useState([]);
  const [plansLoading, setPlansLoading] = useState(true);
  const [accounts, setAccounts] = useState([]);
  const [selectedAccId, setSelectedAccId] = useState('');
  const [selectedPlanId, setSelectedPlanId] = useState('');
  const [startDate, setStartDate] = useState('');
  const [assigning, setAssigning] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    async function load() {
      try {
        const [plansRes, accsRes] = await Promise.all([
          api.get('/supportplan'),
          api.get('/accounts', { params: { pageNumber: 1, pageSize: 100 } }),
        ]);
        setPlans(plansRes.data || []);
        setAccounts(accsRes.data.items || accsRes.data || []);
        if ((plansRes.data || []).length > 0) {
          setSelectedPlanId(plansRes.data[0].supportPlanId || plansRes.data[0].id);
        }
      } catch {
        setError('Failed to load support plans or accounts.');
      } finally {
        setPlansLoading(false);
      }
    }
    load();
  }, []);

  const handleAssignPlan = async (e) => {
    e.preventDefault();
    if (!selectedAccId || !selectedPlanId) return;
    setAssigning(true);
    setMessage('');
    setError('');
    try {
      await api.post('/supportplan/assign', {
        accountId: selectedAccId,
        supportPlanId: selectedPlanId,
        startDate: startDate || new Date().toISOString(),
      });
      setMessage('Support plan assigned to account successfully with quota tracking!');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to assign support plan.');
    } finally {
      setAssigning(false);
    }
  };

  return (
    <div className="support-plans-page">
      <div className="page-header">
        <div>
          <h1><Award size={22} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Support Plans &amp; Ticket Entitlements</h1>
          <p>Module 13 — Define account support tiers, annual/monthly ticket quotas, overage policies, and account assignments.</p>
        </div>
      </div>

      {message && <div className="success-banner"><CheckCircle2 size={16} /> {message}</div>}
      {error && <div className="field-error">{error}</div>}

      {/* Plan Assignment Box */}
      <div className="card assign-card">
        <h3><UserCheck size={18} /> Assign Support Plan to Customer Account</h3>
        <form onSubmit={handleAssignPlan} className="assign-form">
          <div className="field">
            <label>Select Customer Account</label>
            <select value={selectedAccId} onChange={(e) => setSelectedAccId(e.target.value)} required>
              <option value="">Select Account...</option>
              {accounts.map(acc => (
                <option key={acc.accountId || acc.id} value={acc.accountId || acc.id}>
                  {acc.accountName || acc.name}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label>Select Support Plan Tier</label>
            <select value={selectedPlanId} onChange={(e) => setSelectedPlanId(e.target.value)} required>
              <option value="">Select Plan...</option>
              {plans.map(p => (
                <option key={p.supportPlanId || p.id} value={p.supportPlanId || p.id}>
                  {p.name} ({p.maxTicketsPerYear ?? p.maxTickets} tickets/yr)
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label>Start Date</label>
            <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
          </div>

          <button type="submit" className="btn btn--primary" disabled={assigning || !selectedAccId || !selectedPlanId}>
            {assigning ? 'Assigning Plan...' : 'Assign Plan & Entitlements'}
          </button>
        </form>
      </div>

      {/* Plans List Table */}
      <div className="card table-card">
        <h3><ShieldCheck size={18} /> Active Support Plan Definitions</h3>
        {plansLoading ? (
          <div style={{ textAlign: 'center', padding: '2rem' }}><Loader size={20} /></div>
        ) : plans.length === 0 ? (
          <p style={{ color: 'var(--text-muted)', padding: '1rem' }}>No support plans configured yet.</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Plan Name</th>
                <th>Annual Quota</th>
                <th>Exhausted Policy</th>
                <th>Duration (months)</th>
              </tr>
            </thead>
            <tbody>
              {plans.map(p => (
                <tr key={p.supportPlanId || p.id}>
                  <td><strong>{p.name}</strong></td>
                  <td><span className="badge badge--info">{p.maxTicketsPerYear ?? p.maxTickets} tickets</span></td>
                  <td>
                    {p.blockWhenExhausted ? (
                      <span className="badge badge--danger">Block Ticket Creation</span>
                    ) : (
                      <span className="badge badge--success">Allow Overage</span>
                    )}
                  </td>
                  <td>{p.durationMonths ?? 'N/A'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
