import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusCircle, ArrowLeft, Send, Paperclip, AlertCircle } from 'lucide-react';
import api from '../../api/axios';
import EntitlementBanner from '../../components/EntitlementBanner';
import FaqDeflectionPanel from '../../components/FaqDeflectionPanel';
import './CreateTicket.scss';

export default function CreateTicket({ isPortal = false }) {
  const navigate = useNavigate();

  // Form Fields
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
  const [attachments, setAttachments] = useState([]);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadMasterData() {
      try {
        const [tRes, stRes, pRes, iRes, accRes] = await Promise.all([
          api.get('/tickettypes', { params: { includeInactive: false } }),
          api.get('/ticketsubtypes', { params: { includeInactive: false } }),
          api.get('/ticketpriorities', { params: { includeInactive: false } }),
          api.get('/ticketbusinessimpacts', { params: { includeInactive: false } }),
          api.get('/accounts', { params: { pageNumber: 1, pageSize: 100 } })
        ]);
        setTypes(tRes.data || []);
        setSubTypes(stRes.data || []);
        setPriorities(pRes.data || []);
        setImpacts(iRes.data || []);
        setAccounts(accRes.data.items || accRes.data || []);

        if (pRes.data?.length > 0) setSelectedPriorityId(pRes.data[0].id || pRes.data[0].priorityId);
        if (iRes.data?.length > 0) setSelectedImpactId(iRes.data[0].id || iRes.data[0].businessImpactId);
      } catch {
        // fallback
      }
    }
    loadMasterData();
  }, []);

  // Filter Sub-Types on Type Selection
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
    <div className="create-ticket-page">
      <div className="page-header">
        <button className="btn btn--secondary btn--sm" onClick={() => navigate(-1)}>
          <ArrowLeft size={16} /> Back
        </button>
        <h1>Raise Support Ticket</h1>
      </div>

      <EntitlementBanner entitlement={{ planName: 'Silver Support Plan', totalAllowed: 50, usedCount: 12 }} />

      {error && <div className="field-error"><AlertCircle size={16} /> {error}</div>}

      <div className="create-grid">
        <form onSubmit={handleSubmit} className="card create-form">
          <h3><PlusCircle size={18} /> Ticket Details</h3>

          <div className="grid-2">
            <div className="field">
              <label>Ticket Type *</label>
              <select
                required
                value={selectedTypeId}
                onChange={(e) => setSelectedTypeId(e.target.value)}
              >
                <option value="">Select Category...</option>
                {types.map(t => (
                  <option key={t.id || t.ticketTypeId} value={t.id || t.ticketTypeId}>{t.name}</option>
                ))}
              </select>
            </div>

            <div className="field">
              <label>Issue Sub-Type *</label>
              <select
                required
                value={selectedSubTypeId}
                onChange={(e) => setSelectedSubTypeId(e.target.value)}
                disabled={!selectedTypeId}
              >
                <option value="">Select Specific Issue...</option>
                {filteredSubTypes.map(st => (
                  <option key={st.id || st.ticketSubTypeId} value={st.id || st.ticketSubTypeId}>{st.name}</option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid-2">
            <div className="field">
              <label>Priority Severity *</label>
              <select
                required
                value={selectedPriorityId}
                onChange={(e) => setSelectedPriorityId(e.target.value)}
              >
                {priorities.map(p => (
                  <option key={p.id || p.priorityId} value={p.id || p.priorityId}>
                    {p.name} ({p.slaInHours ? `${p.slaInHours}h SLA` : 'No SLA'})
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label>Business Impact *</label>
              <select
                required
                value={selectedImpactId}
                onChange={(e) => setSelectedImpactId(e.target.value)}
              >
                {impacts.map(imp => (
                  <option key={imp.id || imp.businessImpactId} value={imp.id || imp.businessImpactId}>
                    {imp.description || imp.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {!isPortal && (
            <div className="field">
              <label>Customer Account (Optional - Leave blank for Internal Ticket)</label>
              <select
                value={selectedAccountId}
                onChange={(e) => setSelectedAccountId(e.target.value)}
              >
                <option value="">Internal Ticket (No Customer Account)</option>
                {accounts.map(acc => (
                  <option key={acc.id || acc.accountId} value={acc.id || acc.accountId}>{acc.accountName || acc.name}</option>
                ))}
              </select>
            </div>
          )}

          <div className="field">
            <label>Subject / Short Summary *</label>
            <input
              required
              placeholder="E.g., Cannot connect to corporate VPN after password change..."
              value={title}
              onChange={(e) => setTitle(e.target.value)}
            />
          </div>

          <div className="field">
            <label>Detailed Problem Description *</label>
            <textarea
              required
              rows={5}
              placeholder="Provide exact error messages, steps to reproduce, or affected systems..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn--secondary" onClick={() => navigate(-1)}>Cancel</button>
            <button type="submit" className="btn btn--primary" disabled={loading || !title.trim() || !description.trim()}>
              <Send size={16} /> {loading ? 'Submitting...' : 'Submit Ticket'}
            </button>
          </div>
        </form>

        <div className="deflection-sidebar">
          <FaqDeflectionPanel query={title} onDeflect={() => navigate(-1)} />
        </div>
      </div>
    </div>
  );
}
