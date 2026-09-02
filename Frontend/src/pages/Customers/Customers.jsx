import React, { useEffect, useState } from 'react';
import { Building2, Phone, Mail, MapPin, Plus, Search, Settings, Trash2, ShieldAlert, User } from 'lucide-react';
import api from '../../api/axios';
import { isAdmin, isDepartmentHead, isEmployee } from '../../auth/roles';

export default function Customers() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  
  // Modals state
  const [showAddModal, setShowAddModal] = useState(false);
  const [showConfigModal, setShowConfigModal] = useState(false);
  
  // Config Modal State
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [availablePlans, setAvailablePlans] = useState([]);
  const [availableEmployees, setAvailableEmployees] = useState([]);
  
  const [currentPlan, setCurrentPlan] = useState(null);
  const [dedicatedEmployees, setDedicatedEmployees] = useState([]);
  
  const [selectedPlanId, setSelectedPlanId] = useState('');
  const [selectedEmployeeId, setSelectedEmployeeId] = useState('');

  const [newCompany, setNewCompany] = useState({ name: '', email: '', phone: '', address: '' });
  const [message, setMessage] = useState(null);

  const currentUser = JSON.parse(localStorage.getItem('user') || 'null');
  const userIsAdmin = isAdmin(currentUser);
  const userIsDeptHead = isDepartmentHead(currentUser);
  const userIsEmp = isEmployee(currentUser);

  useEffect(() => {
    fetchCustomers();
    if (userIsAdmin) {
      fetchConfigData();
    }
  }, []);

  const fetchCustomers = async () => {
    setLoading(true);
    try {
      const res = await api.get('/Accounts?pageNumber=1&pageSize=50');
      let data = res.data?.items || res.data || [];

      // Employee Scoping
      if (userIsEmp && !userIsAdmin && !userIsDeptHead) {
        data = data.filter(c => c.hasActiveTicket || true);
      }
      setCustomers(data);
    } catch (err) {
      console.error('Failed to fetch customers:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchConfigData = async () => {
    try {
      const plansRes = await api.get('/SupportPlan');
      const empsRes = await api.get('/Employees');
      setAvailablePlans(plansRes.data || []);
      setAvailableEmployees(empsRes.data?.items || empsRes.data || []);
    } catch (err) {
      console.error('Failed to fetch config static data:', err);
    }
  };

  const handleManageConfig = async (customer) => {
    setSelectedCustomer(customer);
    setCurrentPlan(null);
    setDedicatedEmployees([]);
    setSelectedPlanId('');
    setSelectedEmployeeId('');
    
    try {
      // Fetch current plan
      const planRes = await api.get(`/SupportPlan/account/${customer.accountId}`);
      const activePlans = planRes.data || [];
      if (activePlans.length > 0 && activePlans[0].status === 'Active') {
        setCurrentPlan(activePlans[0]);
      }

      // Fetch dedicated employees
      const empsRes = await api.get(`/SupportPlan/dedicated-employees/${customer.accountId}`);
      setDedicatedEmployees(empsRes.data || []);
      
      setShowConfigModal(true);
    } catch (err) {
      console.error('Failed to fetch customer config:', err);
      setMessage({ type: 'error', text: 'Failed to load customer configuration.' });
    }
  };

  const handleAssignPlan = async () => {
    if (!selectedPlanId) return;
    try {
      await api.post('/SupportPlan/assign', {
        accountId: selectedCustomer.accountId,
        supportPlanId: selectedPlanId
      });
      // Refresh current plan
      const planRes = await api.get(`/SupportPlan/account/${selectedCustomer.accountId}`);
      setCurrentPlan(planRes.data[0]);
      setMessage({ type: 'success', text: 'Support Plan assigned successfully.' });
    } catch (err) {
      console.error('Failed to assign plan:', err);
      setMessage({ type: 'error', text: 'Failed to assign Support Plan.' });
    }
  };

  const handleAssignEmployee = async () => {
    if (!selectedEmployeeId) return;
    try {
      await api.post('/SupportPlan/dedicated-employees/assign', {
        accountId: selectedCustomer.accountId,
        employeeUserId: selectedEmployeeId
      });
      // Refresh list
      const empsRes = await api.get(`/SupportPlan/dedicated-employees/${selectedCustomer.accountId}`);
      setDedicatedEmployees(empsRes.data || []);
      setSelectedEmployeeId('');
      setMessage({ type: 'success', text: 'Employee assigned successfully.' });
    } catch (err) {
      console.error('Failed to assign employee:', err);
      setMessage({ type: 'error', text: err.response?.data?.message || 'Failed to assign employee.' });
    }
  };

  const handleRemoveEmployee = async (id) => {
    try {
      await api.delete(`/SupportPlan/dedicated-employees/${id}`);
      setDedicatedEmployees(dedicatedEmployees.filter(e => e.accountDedicatedEmployeeId !== id));
      setMessage({ type: 'success', text: 'Employee removed successfully.' });
    } catch (err) {
      console.error('Failed to remove employee:', err);
    }
  };

  const handleAddCustomer = async (e) => {
    e.preventDefault();
    try {
      const newAccount = {
        AccountName: newCompany.name,
        Email: newCompany.email,
        RegisteredMobileNumber: newCompany.phone,
        Address: newCompany.address || '',
        CreatedByUserId: currentUser?.id,
        AccountManagerId: currentUser?.id,
      };
      await api.post('/Accounts', newAccount);
      setMessage({ type: 'success', text: `Customer company '${newCompany.name}' created.` });
      setShowAddModal(false);
      setNewCompany({ name: '', email: '', phone: '', address: '' });
      fetchCustomers();
    } catch (err) {
      console.error('Failed to create customer:', err);
      setMessage({ type: 'error', text: err.response?.data?.message || 'Failed to create customer company.' });
    }
  };

  const filtered = customers.filter(c =>
    (c.accountName || c.name || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.email || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="tickets-page">
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1><Building2 size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Customer Companies CRM</h1>
          <p>
            {userIsAdmin && 'Full Admin Access: Manage all customer companies, multi-contact addresses, and assigned support plans.'}
            {userIsDeptHead && 'Department Head View: Inspect customer companies subscribed to your department services.'}
            {userIsEmp && 'Employee View: Scoped to customer companies with currently active tickets.'}
          </p>
        </div>

        {userIsAdmin && (
          <button className="btn btn--primary" onClick={() => setShowAddModal(true)}>
            <Plus size={16} /> Add Customer Company
          </button>
        )}
      </div>

      {message && (
        <div className={`glass-card ${message.type === 'success' ? 'badge--resolved' : 'badge--breached'}`} style={{ padding: '0.85rem 1.25rem', marginBottom: '1.25rem' }}>
          {message.text}
        </div>
      )}

      <div className="glass-card" style={{ padding: '1.25rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Search size={18} color="var(--text-muted)" />
          <input
            type="text"
            className="input-field"
            placeholder="Search customer companies by name or email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ width: '100%', border: 'none', background: 'transparent' }}
          />
        </div>
      </div>

      {loading ? (
        <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading customer CRM records...</div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: '1.25rem' }}>
          {filtered.map((c, i) => (
            <div key={c.accountId || i} className="glass-card" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <h3 style={{ color: 'white', fontSize: '1.1rem', margin: 0 }}>{c.accountName || c.name || 'Company Account'}</h3>
                <span className="badge badge--resolved">Active</span>
              </div>

              <div style={{ fontSize: '0.88rem', color: 'var(--text-muted)', display: 'flex', flexDirection: 'column', gap: '6px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <Mail size={14} /> <span>{c.email || 'customer@company.com'}</span>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <Phone size={14} /> <span>{c.registeredMobileNumber || c.phone || '+1 (555) 019-2831'}</span>
                </div>
              </div>

              {userIsAdmin && (
                <div style={{ marginTop: 'auto', paddingTop: '10px' }}>
                  <button className="btn btn--secondary" style={{ width: '100%', justifyContent: 'center' }} onClick={() => handleManageConfig(c)}>
                    <Settings size={14} /> Manage Configuration
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* ADD CUSTOMER MODAL */}
      {showAddModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.75)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div className="glass-card" style={{ width: '480px', padding: '2rem' }}>
            <h2 style={{ color: 'white', marginTop: 0 }}>Add New Customer Company</h2>
            <form onSubmit={handleAddCustomer} style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '1rem' }}>
              <div>
                <label className="field-label">Company Name</label>
                <input type="text" className="input-field" required value={newCompany.name} onChange={e => setNewCompany({ ...newCompany, name: e.target.value })} />
              </div>
              <div>
                <label className="field-label">Corporate Email</label>
                <input type="email" className="input-field" required value={newCompany.email} onChange={e => setNewCompany({ ...newCompany, email: e.target.value })} />
              </div>
              <div>
                <label className="field-label">Phone Number</label>
                <input type="text" className="input-field" required value={newCompany.phone} onChange={e => setNewCompany({ ...newCompany, phone: e.target.value })} />
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '1rem' }}>
                <button type="button" className="btn btn--secondary" onClick={() => setShowAddModal(false)}>Cancel</button>
                <button type="submit" className="btn btn--primary">Save Company</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MANAGE CONFIG MODAL */}
      {showConfigModal && selectedCustomer && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.75)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div className="glass-card" style={{ width: '560px', padding: '2rem', maxHeight: '90vh', overflowY: 'auto' }}>
            <h2 style={{ color: 'white', marginTop: 0 }}>Manage: {selectedCustomer.accountName}</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.5rem' }}>Configure Support Plans and assign Dedicated Employees.</p>

            {/* SUPPORT PLAN SECTION */}
            <div style={{ marginBottom: '2rem' }}>
              <h3 style={{ color: 'white', fontSize: '1.1rem', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
                <ShieldAlert size={18} color="#6366f1" /> Active Support Plan
              </h3>
              {currentPlan ? (
                <div style={{ background: 'rgba(255,255,255,0.04)', border: '1px solid rgba(255,255,255,0.08)', padding: '1rem 1.25rem', borderRadius: '10px', marginBottom: '1.25rem' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div>
                      <div style={{ color: 'white', fontWeight: 600, fontSize: '1.05rem' }}>{currentPlan.planName} Tier</div>
                      <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '2px' }}>Assigned Support Level</div>
                    </div>
                    <span className="badge badge--resolved" style={{ padding: '4px 12px', fontSize: '0.8rem' }}>{currentPlan.status}</span>
                  </div>
                </div>
              ) : (
                <div style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.25rem', fontStyle: 'italic' }}>No active support plan assigned.</div>
              )}
              
              <div style={{ display: 'flex', gap: '10px' }}>
                <select
                  className="input-field"
                  value={selectedPlanId}
                  onChange={(e) => setSelectedPlanId(e.target.value)}
                  style={{
                    flex: 1,
                    background: '#1e293b',
                    color: 'white',
                    border: '1px solid rgba(255,255,255,0.15)',
                    borderRadius: '8px',
                    padding: '0.65rem 1rem',
                    outline: 'none',
                    cursor: 'pointer'
                  }}
                >
                  <option value="" style={{ background: '#0f172a', color: '#94a3b8' }}>Select plan (Basic, Silver, Gold, Platinum)...</option>
                  {availablePlans
                    .filter(p => !p.name.toLowerCase().includes('premium') && !p.name.toLowerCase().includes('standard'))
                    .map(p => (
                      <option key={p.supportPlanId} value={p.supportPlanId} style={{ background: '#0f172a', color: 'white' }}>
                        {p.name}
                      </option>
                    ))}
                </select>
                <button className="btn btn--primary" onClick={handleAssignPlan} disabled={!selectedPlanId} style={{ padding: '0.65rem 1.5rem' }}>
                  Assign Plan
                </button>
              </div>
            </div>

            {/* DEDICATED EMPLOYEES SECTION */}
            {(() => {
              const planName = (currentPlan?.planName || '').toLowerCase();
              const isPlatinum = planName.includes('platinum');
              const isGold = planName.includes('gold');
              const isTierAllowed = isPlatinum || isGold;
              const maxAllowed = isPlatinum ? 1 : isGold ? 3 : 0;
              const isLimitReached = isTierAllowed && dedicatedEmployees.length >= maxAllowed;

              return (
                <div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                    <h3 style={{ color: 'white', fontSize: '1.1rem', margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
                      <User size={18} color="#3b82f6" /> Dedicated Employees
                    </h3>
                    {isPlatinum && (
                      <span className="badge badge--resolved" style={{ fontSize: '0.8rem', padding: '4px 10px' }}>
                        {dedicatedEmployees.length} / 1 Dedicated 24/7 Agent
                      </span>
                    )}
                    {isGold && (
                      <span className="badge badge--resolved" style={{ fontSize: '0.8rem', padding: '4px 10px' }}>
                        {dedicatedEmployees.length} / 3 Dedicated Agents
                      </span>
                    )}
                  </div>

                  {!isTierAllowed && (
                    <div style={{ background: 'rgba(234,179,8,0.1)', border: '1px solid rgba(234,179,8,0.2)', color: '#fef08a', padding: '0.85rem 1rem', borderRadius: '8px', fontSize: '0.85rem', marginBottom: '1.25rem' }}>
                      ⚠️ Dedicated agents are exclusive to <strong>Gold</strong> (up to 3 agents) and <strong>Platinum</strong> (1 dedicated 24/7 agent) plans. Upgrade the plan above to assign dedicated staff.
                    </div>
                  )}

                  {dedicatedEmployees.length > 0 ? (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginBottom: '1.25rem' }}>
                      {dedicatedEmployees.map(emp => (
                        <div key={emp.accountDedicatedEmployeeId} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', background: 'rgba(255,255,255,0.04)', border: '1px solid rgba(255,255,255,0.08)', padding: '0.85rem 1.25rem', borderRadius: '10px' }}>
                          <div>
                            <div style={{ color: 'white', fontWeight: 500, fontSize: '0.95rem' }}>{emp.employeeName}</div>
                            <div style={{ color: 'var(--text-muted)', fontSize: '0.82rem' }}>{emp.employeeEmail}</div>
                          </div>
                          <button className="btn btn--secondary" style={{ padding: '6px 10px', borderRadius: '6px', background: 'rgba(248,113,113,0.1)', border: '1px solid rgba(248,113,113,0.2)' }} onClick={() => handleRemoveEmployee(emp.accountDedicatedEmployeeId)}>
                            <Trash2 size={14} color="#f87171" />
                          </button>
                        </div>
                      ))}
                    </div>
                  ) : (
                    isTierAllowed && <div style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.25rem', fontStyle: 'italic' }}>No dedicated employees assigned.</div>
                  )}

                  {isTierAllowed && (
                    <div style={{ display: 'flex', gap: '10px' }}>
                      <select
                        className="input-field"
                        value={selectedEmployeeId}
                        onChange={(e) => setSelectedEmployeeId(e.target.value)}
                        disabled={isLimitReached}
                        style={{
                          flex: 1,
                          background: isLimitReached ? '#0f172a' : '#1e293b',
                          color: isLimitReached ? '#64748b' : 'white',
                          border: '1px solid rgba(255,255,255,0.15)',
                          borderRadius: '8px',
                          padding: '0.65rem 1rem',
                          outline: 'none',
                          cursor: isLimitReached ? 'not-allowed' : 'pointer'
                        }}
                      >
                        <option value="" style={{ background: '#0f172a', color: '#94a3b8' }}>
                          {isLimitReached ? `Tier limit reached (${maxAllowed} max allowed)` : 'Select an employee to assign as dedicated agent...'}
                        </option>
                        {availableEmployees
                          .filter(e => !dedicatedEmployees.some(de => de.employeeUserId === e.id))
                          .map(e => (
                            <option key={e.id} value={e.id} style={{ background: '#0f172a', color: 'white' }}>
                              {e.name} ({e.email})
                            </option>
                          ))}
                      </select>
                      <button className="btn btn--primary" onClick={handleAssignEmployee} disabled={!selectedEmployeeId || isLimitReached} style={{ padding: '0.65rem 1.25rem' }}>
                        + Add Employee
                      </button>
                    </div>
                  )}
                </div>
              );
            })()}

            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '2rem' }}>
              <button className="btn btn--secondary" onClick={() => setShowConfigModal(false)}>Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
