import React, { useState, useEffect } from 'react';
import { Building2, Plus, Edit2, UserPlus, Users, Crown, Mail, Phone, ShieldCheck, X, CheckCircle2, AlertCircle } from 'lucide-react';
import api from '../../api/axios';

export default function Departments() {
  const [departments, setDepartments] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);

  // Department edit/add modal
  const [showDeptForm, setShowDeptForm] = useState(false);
  const [editingDept, setEditingDept] = useState(null);
  const [deptName, setDeptName] = useState('');
  const [deptDescription, setDeptDescription] = useState('');
  const [assignMethod, setAssignMethod] = useState('Auto_assignment_custom');

  // Selected Department Detail Modal
  const [selectedDept, setSelectedDept] = useState(null);
  const [showAddEmpModal, setShowAddEmpModal] = useState(false);

  // New Employee Form State
  const [newEmp, setNewEmp] = useState({
    name: '',
    email: '',
    designation: 'Support Specialist',
    phone: '',
    password: 'Employee@123'
  });

  const [message, setMessage] = useState(null);

  useEffect(() => {
    fetchDepartments();
    fetchEmployees();
  }, []);

  const fetchDepartments = async () => {
    setLoading(true);
    try {
      const res = await api.get('/departments', { params: { pageSize: 100 } });
      setDepartments(res.data.items || res.data || []);
    } catch (err) {
      console.error('Failed to fetch departments:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchEmployees = async () => {
    try {
      const res = await api.get('/Employees', { params: { pageSize: 200 } });
      setEmployees(res.data.items || res.data || []);
    } catch (err) {
      console.error('Failed to fetch employees:', err);
    }
  };

  const handleSaveDept = async (e) => {
    e.preventDefault();
    if (!deptName.trim()) return;

    try {
      if (editingDept) {
        await api.put(`/departments/${editingDept.id}`, { name: deptName, description: deptDescription, ticketAutoAssignMethod: assignMethod });
        setMessage({ type: 'success', text: `Department '${deptName}' updated successfully.` });
      } else {
        await api.post('/departments', { name: deptName, description: deptDescription, ticketAutoAssignMethod: assignMethod });
        setMessage({ type: 'success', text: `Department '${deptName}' created successfully.` });
      }
      setShowDeptForm(false);
      setDeptName('');
      setDeptDescription('');
      setEditingDept(null);
      fetchDepartments();
    } catch {
      setMessage({ type: 'error', text: 'Failed to save department details.' });
    }
  };

  const handleCreateEmployee = async (e) => {
    e.preventDefault();
    if (!selectedDept) return;

    try {
      await api.post('/RoleManagement/dept-employee', {
        name: newEmp.name,
        email: newEmp.email,
        departmentId: selectedDept.id || selectedDept.departmentId,
        designation: newEmp.designation,
        phone: newEmp.phone,
        password: newEmp.password
      });

      setMessage({ type: 'success', text: `Employee '${newEmp.name}' created and assigned to ${selectedDept.name}.` });
      setShowAddEmpModal(false);
      setNewEmp({ name: '', email: '', designation: 'Support Specialist', phone: '', password: 'Employee@123' });
      fetchEmployees();
      fetchDepartments();
    } catch (err) {
      setMessage({ type: 'error', text: err.response?.data?.message || 'Failed to create employee.' });
    }
  };

  const handlePromoteDeptHead = async (empId) => {
    try {
      await api.post(`/RoleManagement/promote-dept-head/${empId}`);
      setMessage({ type: 'success', text: 'Promoted staff member to Department Head!' });
      fetchDepartments();
      fetchEmployees();
    } catch (err) {
      setMessage({ type: 'error', text: err.response?.data?.message || 'Failed to promote employee.' });
    }
  };

  const deptEmployees = selectedDept
    ? employees.filter(e => e.departmentId === (selectedDept.id || selectedDept.departmentId))
    : [];

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Building2 size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Department & Staff Operations</h1>
          <p>Inspect department structures, manage staff assignments, and create new employees.</p>
        </div>
        <button className="btn btn--primary" onClick={() => { setEditingDept(null); setDeptName(''); setDeptDescription(''); setShowDeptForm(true); }}>
          <Plus size={16} /> Add Department
        </button>
      </div>

      {message && (
        <div className={`glass-card ${message.type === 'success' ? 'badge--resolved' : 'badge--breached'}`} style={{ padding: '0.85rem 1.25rem', marginBottom: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
          {message.type === 'success' ? <CheckCircle2 size={18} /> : <AlertCircle size={18} />}
          <span>{message.text}</span>
        </div>
      )}

      {/* Grid of Departments */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1.25rem' }}>
        {departments.map(d => {
          const empList = employees.filter(e => e.departmentId === (d.id || d.departmentId));
          const deptHead = employees.find(e => e.id === d.departmentHeadId);

          return (
            <div
              key={d.id || d.departmentId}
              className="glass-card"
              style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', justifyContent: 'space-between', cursor: 'pointer', border: '1px solid var(--bg-card-border)', transition: 'transform 0.2s, border-color 0.2s' }}
              onClick={() => setSelectedDept(d)}
            >
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                  <h3 style={{ color: 'white', margin: 0, fontSize: '1.15rem' }}>{d.name}</h3>
                  <span className="badge badge--progress" style={{ fontSize: '0.75rem' }}>
                    {d.ticketAutoAssignMethod === 'RoundRobin' ? 'Round-Robin' : d.ticketAutoAssignMethod === 'LoadBalanced' ? 'Load-Balanced' : 'Weighted Score'}
                  </span>
                </div>

                <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', marginBottom: '1rem', minHeight: '36px' }}>
                  {d.description || 'Primary organizational operational unit.'}
                </p>

                {deptHead && (
                  <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.82rem', color: '#fbbf24', marginBottom: '0.85rem' }}>
                    <Crown size={14} />
                    <span>Dept Head: <strong>{deptHead.name}</strong></span>
                  </div>
                )}
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingTop: '0.85rem', borderTop: '1px solid var(--bg-card-border)', fontSize: '0.82rem', color: 'var(--text-dim)' }}>
                <span><Users size={14} style={{ verticalAlign: 'middle', marginRight: '4px' }} /> {empList.length} Staff Members</span>
                <div style={{ display: 'flex', gap: '6px' }} onClick={(e) => e.stopPropagation()}>
                  <button className="btn btn--secondary btn--sm" onClick={() => { setEditingDept(d); setDeptName(d.name); setDeptDescription(d.description || ''); setAssignMethod(d.ticketAutoAssignMethod || 'Auto_assignment_custom'); setShowDeptForm(true); }}>
                    <Edit2 size={13} /> Edit
                  </button>
                  <button className="btn btn--primary btn--sm" onClick={() => setSelectedDept(d)}>
                    Inspect
                  </button>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Selected Department Detailed Drawer/Modal */}
      {selectedDept && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.75)', backdropFilter: 'blur(6px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000, padding: '1.5rem' }}>
          <div className="glass-card" style={{ width: '100%', maxWidth: '750px', maxHeight: '85vh', overflowY: 'auto', padding: '2rem', background: '#0f172a', border: '1px solid var(--accent-primary)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', borderBottom: '1px solid var(--bg-card-border)', paddingBottom: '1rem', marginBottom: '1.25rem' }}>
              <div>
                <h2 style={{ color: 'white', margin: 0 }}>{selectedDept.name}</h2>
                <p style={{ color: 'var(--text-muted)', margin: '4px 0 0 0', fontSize: '0.9rem' }}>{selectedDept.description}</p>
              </div>
              <button className="btn btn--secondary btn--sm" onClick={() => setSelectedDept(null)}>
                <X size={18} />
              </button>
            </div>

            {/* Department Actions & Summary */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', background: 'rgba(255,255,255,0.03)', padding: '1rem', borderRadius: '8px', marginBottom: '1.5rem' }}>
              <div>
                <span style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>Routing Method: </span>
                <strong style={{ color: 'white' }}>
                  {selectedDept.ticketAutoAssignMethod === 'RoundRobin' ? 'Round-Robin Rotation' : selectedDept.ticketAutoAssignMethod === 'LoadBalanced' ? 'Load-Balanced' : 'Weighted Multi-Factor'}
                </strong>
              </div>
              <button className="btn btn--primary btn--sm" onClick={() => setShowAddEmpModal(true)}>
                <UserPlus size={15} /> Create & Add Employee
              </button>
            </div>

            {/* Employee Roster Table */}
            <h3 style={{ color: 'white', fontSize: '1.05rem', marginBottom: '0.85rem' }}>
              Department Staff Roster ({deptEmployees.length})
            </h3>

            {deptEmployees.length === 0 ? (
              <div style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '2rem', background: 'rgba(255,255,255,0.02)', borderRadius: '8px' }}>
                No employees currently assigned to this department. Click 'Create & Add Employee' above to add staff members.
              </div>
            ) : (
              <div className="table-container">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Employee Code</th>
                      <th>Full Name</th>
                      <th>Designation</th>
                      <th>Email Address</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {deptEmployees.map(emp => {
                      const isHead = emp.id === selectedDept.departmentHeadId;
                      return (
                        <tr key={emp.id}>
                          <td style={{ fontFamily: 'monospace', color: '#818cf8' }}>{emp.employeeId || emp.autoGenrateId || 'EMP-1000'}</td>
                          <td style={{ color: 'white', fontWeight: 600 }}>
                            {emp.name} {isHead && <Crown size={14} color="#fbbf24" style={{ verticalAlign: 'middle', marginLeft: '4px' }} title="Department Head" />}
                          </td>
                          <td>{emp.designation || 'Support Specialist'}</td>
                          <td>{emp.email}</td>
                          <td>
                            {!isHead && (
                              <button className="btn btn--secondary btn--sm" style={{ fontSize: '0.75rem' }} onClick={() => handlePromoteDeptHead(emp.id)} title="Promote to Dept Head">
                                <Crown size={12} /> Promote Head
                              </button>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Add Employee Modal */}
      {showAddEmpModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.8)', backdropFilter: 'blur(6px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1100 }}>
          <div className="glass-card" style={{ width: '480px', padding: '1.75rem', background: '#0f172a', border: '1px solid var(--accent-primary)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <h3 style={{ color: 'white', margin: 0 }}><UserPlus size={18} style={{ verticalAlign: 'middle', marginRight: '6px' }} /> Create New Employee</h3>
              <button className="btn btn--secondary btn--sm" onClick={() => setShowAddEmpModal(false)}><X size={16} /></button>
            </div>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', marginBottom: '1.25rem' }}>
              Assigning to department: <strong style={{ color: 'white' }}>{selectedDept?.name}</strong>
            </p>
            <form onSubmit={handleCreateEmployee}>
              <div className="form-group">
                <label>Full Employee Name *</label>
                <input required placeholder="e.g. Robert Vance" value={newEmp.name} onChange={(e) => setNewEmp({ ...newEmp, name: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Corporate Email *</label>
                <input required type="email" placeholder="e.g. robert.vance@sciqustickets.com" value={newEmp.email} onChange={(e) => setNewEmp({ ...newEmp, email: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Designation Title</label>
                <input placeholder="e.g. Senior Network Engineer" value={newEmp.designation} onChange={(e) => setNewEmp({ ...newEmp, designation: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Phone Number</label>
                <input placeholder="e.g. +1 (555) 234-5678" value={newEmp.phone} onChange={(e) => setNewEmp({ ...newEmp, phone: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Initial Login Password</label>
                <input type="password" value={newEmp.password} onChange={(e) => setNewEmp({ ...newEmp, password: e.target.value })} />
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1.25rem' }}>
                <button type="button" className="btn btn--secondary" onClick={() => setShowAddEmpModal(false)}>Cancel</button>
                <button type="submit" className="btn btn--primary">Create & Register Staff</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Create / Edit Department Modal */}
      {showDeptForm && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.75)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div className="glass-card" style={{ width: '450px', padding: '1.5rem', background: '#0f172a' }}>
            <h3 style={{ margin: '0 0 1rem 0', color: 'white' }}>{editingDept ? 'Edit Department' : 'Create Department'}</h3>
            <form onSubmit={handleSaveDept}>
              <div className="form-group">
                <label>Department Name *</label>
                <input required placeholder="e.g. IT Support & Infrastructure" value={deptName} onChange={(e) => setDeptName(e.target.value)} />
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea rows={3} placeholder="Department responsibilities..." value={deptDescription} onChange={(e) => setDeptDescription(e.target.value)} />
              </div>
              <div className="form-group">
                <label>Per-Department Auto-Assign Routing Method</label>
                <select value={assignMethod} onChange={(e) => setAssignMethod(e.target.value)}>
                  <option value="Auto_assignment_custom">Weighted Score (Multi-Factor Formula)</option>
                  <option value="RoundRobin">Round-Robin Rotation</option>
                  <option value="LoadBalanced">Load-Balanced (Fewest Open Tickets)</option>
                </select>
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1.25rem' }}>
                <button type="button" className="btn btn--secondary" onClick={() => setShowDeptForm(false)}>Cancel</button>
                <button type="submit" className="btn btn--primary">Save Department</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
