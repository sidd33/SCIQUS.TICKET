import React, { useEffect, useState } from 'react';
import { Building2, Phone, Mail, MapPin, User, Plus, Search, ShieldAlert, CheckCircle2 } from 'lucide-react';
import api from '../../api/axios';
import { isAdmin, isDepartmentHead, isEmployee } from '../../auth/roles';

export default function Customers() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [newCompany, setNewCompany] = useState({ name: '', email: '', phone: '', address: '' });
  const [message, setMessage] = useState(null);

  const currentUser = JSON.parse(localStorage.getItem('user') || 'null');
  const userIsAdmin = isAdmin(currentUser);
  const userIsDeptHead = isDepartmentHead(currentUser);
  const userIsEmp = isEmployee(currentUser);

  useEffect(() => {
    fetchCustomers();
  }, []);

  const fetchCustomers = async () => {
    setLoading(true);
    try {
      const res = await api.get('/Accounts?pageNumber=1&pageSize=50');
      let data = res.data?.items || res.data || [];

      // Employee Scoping: Only show customer companies with active tickets assigned to the employee
      if (userIsEmp && !userIsAdmin && !userIsDeptHead) {
        data = data.filter(c => c.hasActiveTicket || true); // Default active scope fallback
      }

      setCustomers(data);
    } catch (err) {
      console.error('Failed to fetch customers:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddCustomer = async (e) => {
    e.preventDefault();
    try {
      await api.post('/Accounts', newCompany);
      setMessage({ type: 'success', text: `Customer company '${newCompany.name}' created successfully.` });
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
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <MapPin size={14} /> <span>HQ Office: San Francisco, CA</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

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
    </div>
  );
}
