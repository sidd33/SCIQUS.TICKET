import React from 'react';
import { BarChart3, PieChart, TrendingUp, Users, ShieldCheck, RefreshCw, Clock } from 'lucide-react';
import './Dashboard.scss';

export default function ReportingDashboard() {
  return (
    <div className="dashboard-view">
      <div className="page-title">
        <div>
          <h1><BarChart3 size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Analytics & Reporting Dashboard</h1>
          <p>Module 10 — Executive KPIs, SLA compliance rates, department workloads, channel mix, and reopen trends.</p>
        </div>
      </div>

      <div className="kpi-grid">
        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--emerald"><ShieldCheck size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">Overall SLA Met Rate</span>
            <span className="kpi-value">94.2%</span>
            <small className="kpi-subtext">Target SLA: 90%</small>
          </div>
        </div>

        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--purple"><Clock size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">Avg Resolution Time</span>
            <span className="kpi-value">4.8h</span>
            <small className="kpi-trend positive">-1.2h vs last month</small>
          </div>
        </div>

        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--blue"><Users size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">Active Agents</span>
            <span className="kpi-value">14</span>
            <small className="kpi-subtext">3 Departments</small>
          </div>
        </div>

        <div className="glass-card kpi-card">
          <div className="kpi-icon kpi-icon--rose"><RefreshCw size={22} /></div>
          <div className="kpi-content">
            <span className="kpi-label">Reopen Rate</span>
            <span className="kpi-value">2.4%</span>
            <small className="kpi-subtext">Grace Window: 7 Days</small>
          </div>
        </div>
      </div>

      <div className="glass-card table-card" style={{ marginTop: '1.5rem' }}>
        <h3>Department Workload Distribution</h3>
        <table className="data-table" style={{ marginTop: '1rem' }}>
          <thead>
            <tr>
              <th>Department</th>
              <th>Open Tickets</th>
              <th>Resolved This Month</th>
              <th>SLA Compliance</th>
              <th>Workload Weight</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td><strong>IT Support & Infrastructure</strong></td>
              <td>12</td>
              <td>84</td>
              <td><span className="badge badge--resolved">96.5%</span></td>
              <td><div style={{ background: '#4f46e5', height: '8px', borderRadius: '4px', width: '70%' }} /></td>
            </tr>
            <tr>
              <td><strong>Customer Success / Account Mgmt</strong></td>
              <td>5</td>
              <td>32</td>
              <td><span className="badge badge--resolved">91.0%</span></td>
              <td><div style={{ background: '#10b981', height: '8px', borderRadius: '4px', width: '35%' }} /></td>
            </tr>
            <tr>
              <td><strong>Product Engineering</strong></td>
              <td>1</td>
              <td>14</td>
              <td><span className="badge badge--resolved">100%</span></td>
              <td><div style={{ background: '#8b5cf6', height: '8px', borderRadius: '4px', width: '15%' }} /></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}
