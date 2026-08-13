import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusCircle, ArrowLeft, Send, AlertCircle } from 'lucide-react';
import api from '../../api/axios';
import EntitlementBanner from '../../components/EntitlementBanner';
import FaqDeflectionPanel from '../../components/FaqDeflectionPanel';

export default function CreateTicket({ isPortal = false }) {
  const navigate = useNavigate();

  const [types, setTypes] = useState([]);
  const [subTypes, setSubTypes] = useState([]);
  const [filteredSubTypes, setFilteredSubTypes] = useState([]);
  const [priorities, setPriorities] = useState([]);
  const [impacts, setImpacts] = useState([]);
  const [accounts, setAccounts] = useState([]);

  const [selectedTypeId, setSelectedTypeId] = useState('');
  const [selectedSubTypeId, setSelectedSubTypeId] = useState('');
  const [selectedPriorityId, setSelectedPriorityId] = useState('');
  const [selectedImpactId, setSelectedImpactId] = useState('');
  const [selectedAccountId, setSelectedAccountId] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadMasterData() {
      try {
       const [typesRes, subTypesRes, prioritiesRes, impactsRes] = await Promise.all([
  api.get("/TicketTypes"),
  api.get("/TicketSubTypes"),
  api.get("/TicketPriorities"),
  api.get("/TicketBusinessImpacts")
]);

setTypes(typesRes.data.items || []);
setSubTypes(subTypesRes.data.items || []);
setPriorities(prioritiesRes.data.items || []);
setImpacts(impactsRes.data.items || []);

if (prioritiesRes.data.items?.length > 0) {
  const firstPriority = prioritiesRes.data.items[0];
  setSelectedPriorityId(
    firstPriority.ticketPriorityId || firstPriority.id
  );
}

if (impactsRes.data.items?.length > 0) {
  const firstImpact = impactsRes.data.items[0];
  setSelectedImpactId(
    firstImpact.ticketBusinessTypeImpactId || firstImpact.businessImpactId || firstImpact.id
  );
}
      } catch {
        // fallback
      }
    }
    loadMasterData();
  }, []);

  useEffect(() => {
    if (!selectedTypeId) {
      setFilteredSubTypes([]);
      setSelectedSubTypeId('');
      return;
    }
    const filtered = subTypes.filter(st => (st.ticketTypeId === selectedTypeId || st.typeId === selectedTypeId));
    setFilteredSubTypes(filtered);
    if (filtered.length > 0) setSelectedSubTypeId(filtered[0].id || filtered[0].ticketSubTypeId);
  }, [selectedTypeId, subTypes]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!title.trim() || !description.trim() || !selectedSubTypeId) return;

    setLoading(true);
    setError('');
    try {
      const payload = {
  title,
  description,
  ticketTypeId: selectedTypeId,
  ticketSubTypeId: selectedSubTypeId,
  priorityId: selectedPriorityId,
  businessImpactId: selectedImpactId,
  accountId: selectedAccountId || null,
  isInternal: !selectedAccountId && !isPortal
};
      const res = await api.post('/tickets', payload);
      const newTicketId = res.data.id || res.data.ticketId;

      navigate(isPortal ? `/portal/ticket/${newTicketId}` : `/tickets/${newTicketId}`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create ticket');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <button className="btn btn--secondary btn--sm" onClick={() => navigate(-1)}>
            <ArrowLeft size={16} /> Back
          </button>
          <h1>Raise Support Ticket</h1>
        </div>
      </div>

      <EntitlementBanner entitlement={{ planName: 'Silver Support Plan', totalAllowed: 50, usedCount: 12 }} />

      {error && <div style={{ padding: '0.75rem', background: 'rgba(239,68,68,0.15)', border: '1px solid rgba(239,68,68,0.3)', borderRadius: '8px', color: '#f87171', marginBottom: '1.25rem', fontSize: '0.85rem' }}>{error}</div>}

      <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: '1.5rem' }}>
        <form onSubmit={handleSubmit} className="glass-card" style={{ padding: '1.5rem' }}>
          <h3 style={{ margin: '0 0 1.25rem 0', color: 'white', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <PlusCircle size={18} /> Ticket Details & Classification
          </h3>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <div className="form-group">
              <label>Ticket Type *</label>
              <select required value={selectedTypeId} onChange={(e) => setSelectedTypeId(e.target.value)}>
                <option value="">Select Category...</option>
                {types.map(t => (
                  <option key={t.id || t.ticketTypeId} value={t.id || t.ticketTypeId}>{t.name}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>Issue Sub-Type *</label>
              <select required value={selectedSubTypeId} onChange={(e) => setSelectedSubTypeId(e.target.value)} disabled={!selectedTypeId}>
                <option value="">Select Specific Issue...</option>
                {filteredSubTypes.map(st => (
                  <option key={st.id || st.ticketSubTypeId} value={st.id || st.ticketSubTypeId}>{st.name}</option>
                ))}
              </select>
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <div className="form-group">
              <label>Priority Severity *</label>
              <select required value={selectedPriorityId} onChange={(e) => setSelectedPriorityId(e.target.value)}>
                {priorities.map(p => (
  <option
    key={p.ticketPriorityId}
    value={p.ticketPriorityId}
  >
    {p.name} ({p.slaInHours}h SLA)
  </option>
))}
              </select>
            </div>

            <div className="form-group">
              <label>Business Impact *</label>
              <select required value={selectedImpactId} onChange={(e) => setSelectedImpactId(e.target.value)}>
               {impacts.map(imp => (
  <option
    key={imp.ticketBusinessTypeImpactId}
    value={imp.ticketBusinessTypeImpactId}
  >
    {imp.description || imp.name}
  </option>
))}
              </select>
            </div>
          </div>

          {!isPortal && (
            <div className="form-group">
              <label>Customer Account (Optional - Leave blank for Internal Ticket)</label>
              <select value={selectedAccountId} onChange={(e) => setSelectedAccountId(e.target.value)}>
                <option value="">Internal Ticket (No Customer Account)</option>
                {accounts.map(acc => (
                  <option key={acc.id || acc.accountId} value={acc.id || acc.accountId}>{acc.accountName || acc.name}</option>
                ))}
              </select>
            </div>
          )}

          <div className="form-group">
            <label>Subject / Short Summary *</label>
            <input required placeholder="E.g., Cannot connect to corporate VPN after password change..." value={title} onChange={(e) => setTitle(e.target.value)} />
          </div>

          <div className="form-group">
            <label>Detailed Problem Description *</label>
            <textarea required rows={5} placeholder="Provide exact error messages, steps to reproduce, or affected systems..." value={description} onChange={(e) => setDescription(e.target.value)} />
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1.5rem' }}>
            <button type="button" className="btn btn--secondary" onClick={() => navigate(-1)}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={loading || !title.trim() || !description.trim()}>
              <Send size={16} /> {loading ? 'Submitting...' : 'Submit Ticket'}
            </button>
          </div>
        </form>

        <div>
          <FaqDeflectionPanel query={title} onDeflect={() => navigate(-1)} />
        </div>
      </div>
    </div>
  );
}
