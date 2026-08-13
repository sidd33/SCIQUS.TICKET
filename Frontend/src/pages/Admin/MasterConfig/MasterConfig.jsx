import React, { useState, useEffect } from 'react';
import { Layers, Plus, Trash2, Edit2 } from 'lucide-react';
import api from '../../../api/axios';

export default function MasterConfig() {
  const [activeTab, setActiveTab] = useState('types');
  const [types, setTypes] = useState([]);
  const [subTypes, setSubTypes] = useState([]);
  const [priorities, setPriorities] = useState([]);
  const [impacts, setImpacts] = useState([]);

  useEffect(() => {
    loadMasterData();
  }, []);

  async function loadMasterData() {
    try {
      const [tRes, stRes, pRes, iRes] = await Promise.all([
        api.get('/TicketTypes', { params: { includeInactive: true } }),
        api.get('/TicketSubTypes', { params: { includeInactive: true } }),
        api.get('/TicketPriorities', { params: { includeInactive: true } }),
        api.get('/TicketBusinessImpacts', { params: { includeInactive: true } })
      ]);
      setTypes(tRes.data.items || []);
setSubTypes(stRes.data.items || []);
setPriorities(pRes.data.items || []);
setImpacts(iRes.data.items || []);
    } catch {
      // fallback
    }
  }

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Layers size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Master Ticket Configuration</h1>
          <p>Module 1 — Ticket Types, Sub-Types (Type → Dept → Default Agent cascade), SLA Priorities, and Business Impacts.</p>
        </div>
      </div>

      <div className="glass-card" style={{ padding: '1rem', marginBottom: '1.25rem' }}>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className={`btn btn--sm ${activeTab === 'types' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('types')}>
            Ticket Types ({types.length})
          </button>
          <button className={`btn btn--sm ${activeTab === 'subtypes' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('subtypes')}>
            Sub-Types & Routing ({subTypes.length})
          </button>
          <button className={`btn btn--sm ${activeTab === 'priorities' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('priorities')}>
            SLA Priorities ({priorities.length})
          </button>
          <button className={`btn btn--sm ${activeTab === 'impacts' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('impacts')}>
            Business Impacts ({impacts.length})
          </button>
        </div>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          {activeTab === 'types' && (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Type Name</th>
                  <th>Description</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {types.map(t => (
                  <tr key={t.id || t.ticketTypeId}>
                    <td><strong>{t.name}</strong></td>
                    <td>{t.description || 'No description'}</td>
                    <td><span className="badge badge--resolved">Active</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {activeTab === 'subtypes' && (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Sub-Type Name</th>
                  <th>Parent Category</th>
                  <th>Assigned Department</th>
                  <th>Default Fixed Agent</th>
                </tr>
              </thead>
              <tbody>
                {subTypes.map(st => (
                  <tr key={st.id || st.ticketSubTypeId}>
                    <td><strong>{st.name}</strong></td>
                    <td>{st.ticketTypeName || 'Software & Apps'}</td>
                    <td>{st.departmentName || 'IT Support'}</td>
                    <td>{st.defaultUserName || 'Auto-Routing Engine'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {activeTab === 'priorities' && (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Priority Level</th>
                  <th>Severity Rank</th>
                  <th>Resolution SLA Target</th>
                  <th>Response SLA</th>
                </tr>
              </thead>
              <tbody>
                {priorities.map(p => (
  <tr key={p.ticketPriorityId}>
    <td><strong>{p.name}</strong></td>
    <td>Level {p.level}</td>
    <td>
      <span className="badge badge--progress">
        {p.slaInHours} Hours
      </span>
    </td>
    <td>Not configured</td>
  </tr>
))}
              </tbody>
            </table>
          )}

          {activeTab === 'impacts' && (
  <table className="data-table">
    <thead>
      <tr>
        <th>Impact Title</th>
        <th>Scope Description</th>
        <th>Status</th>
      </tr>
    </thead>

    <tbody>
      {impacts.map(imp => (
        <tr key={imp.ticketBusinessTypeImpactId}>
          <td>
            <strong>{imp.name}</strong>
          </td>

          <td>
            {imp.description || "—"}
          </td>

          <td>
            <span className={`badge ${imp.status ? "badge--resolved" : "badge--error"}`}>
              {imp.status ? "Active" : "Inactive"}
            </span>
          </td>
        </tr>
      ))}
    </tbody>
  </table>
)}
        </div>
      </div>
    </div>
  );
}
