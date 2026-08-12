import React, { useState, useEffect } from 'react';
import { Users, Plus, Search, UserCheck, Shield } from 'lucide-react';
import api from '../../api/axios';

export default function Employees() {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    async function loadEmployees() {
      try {
        const res = await api.get('/employees', { params: { pageSize: 100 } });
        setEmployees(res.data.items || res.data || []);
      } catch {
        // fallback
      } finally {
        setLoading(false);
      }
    }
    loadEmployees();
  }, []);

  const filtered = employees.filter(e => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (
      (e.name || `${e.firstName} ${e.lastName}`).toLowerCase().includes(q) ||
      e.email?.toLowerCase().includes(q) ||
      e.departmentName?.toLowerCase().includes(q)
    );
  });

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Users size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Employee Roster & IAM Roles</h1>
          <p>Manage employee profiles, department assignments, and system access rights.</p>
        </div>
      </div>

      <div className="glass-card filter-bar">
        <div className="search-input">
          <Search size={16} />
          <input placeholder="Search employees by name, email, department..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Employee Code</th>
                <th>Full Name</th>
                <th>Email Address</th>
                <th>Department</th>
                <th>System Role</th>
                <th>Login Access</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>Loading workforce roster...</td></tr>
              ) : filtered.length === 0 ? (
                <tr><td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem' }}>No employee records found.</td></tr>
              ) : (
                filtered.map(emp => (
                  <tr key={emp.id}>
                    <td><code>{emp.employeeId || emp.autoGenrateId || 'EMP-001'}</code></td>
                    <td><strong>{emp.name || `${emp.firstName || ''} ${emp.lastName || ''}`}</strong></td>
                    <td>{emp.email}</td>
                    <td>{emp.departmentName || 'IT Support & Infrastructure'}</td>
                    <td><span className="badge badge--progress">{emp.role || 'Support Agent'}</span></td>
                    <td>
                      <span className="badge badge--resolved">Active</span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
