import React, { useState, useEffect } from 'react';
import { BarChart3, TrendingUp, Clock, AlertTriangle, RefreshCw, Layers, PieChart } from 'lucide-react';
import api from '../../api/axios';
import './ReportingDashboard.scss';

export default function ReportingDashboard() {
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadReport() {
      try {
        const res = await api.get('/ticketreport/summary');
        setReport(res.data);
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to load reporting dashboard metrics');
      } finally {
        setLoading(false);
      }
    }
    loadReport();
  }, []);

  if (loading) return <div className="reporting-loading">Loading reporting analytics...</div>;

  const summary = report || {
    totalTickets: 0,
    openTickets: 0,
    closedTickets: 0,
    breachedTickets: 0,
    slaCompliancePercentage: 100,
    averageResolutionTimeHours: 0,
    reopenRatePercentage: 0,
    channelMix: [],
    departmentWorkload: []
  };

  return (
    <div className="reporting-dashboard-page">
      <div className="page-header">
        <div>
          <h1><BarChart3 size={22} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Ticket Analytics & Reporting Dashboard</h1>
          <p>Real-time SLA compliance, resolution speed, channel distribution, and department workload metrics.</p>
        </div>
      </div>

      {error && <div className="field-error">{error}</div>}

      {/* KPI Cards */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-icon kpi-icon--blue"><Layers size={22} /></div>
          <div className="kpi-body">
            <span className="kpi-label">Open vs Closed</span>
            <span className="kpi-value">{summary.openTickets} <small>/ {summary.closedTickets} Closed</small></span>
            <span className="kpi-sub">{summary.totalTickets} total tickets created</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon kpi-icon--green"><TrendingUp size={22} /></div>
          <div className="kpi-body">
            <span className="kpi-label">SLA Compliance</span>
            <span className="kpi-value">{summary.slaCompliancePercentage?.toFixed(1)}%</span>
            <span className="kpi-sub">Target met within deadline</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon kpi-icon--amber"><Clock size={22} /></div>
          <div className="kpi-body">
            <span className="kpi-label">Avg Resolution Speed</span>
            <span className="kpi-value">{summary.averageResolutionTimeHours?.toFixed(1)} <small>hours</small></span>
            <span className="kpi-sub">Average time to close</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon kpi-icon--purple"><RefreshCw size={22} /></div>
          <div className="kpi-body">
            <span className="kpi-label">Reopen Rate</span>
            <span className="kpi-value">{summary.reopenRatePercentage?.toFixed(1)}%</span>
            <span className="kpi-sub">Customer reopen frequency</span>
          </div>
        </div>
      </div>

      {/* Detailed Analytics Grid */}
      <div className="analytics-grid">
        {/* SLA Health Bar */}
        <div className="card analytics-card">
          <div className="card-header">
            <h3><TrendingUp size={18} /> SLA Performance Breakdown</h3>
          </div>
          <div className="card-body">
            <div className="progress-container">
              <div className="progress-label">
                <span>Met Target</span>
                <span>{summary.slaCompliancePercentage?.toFixed(1)}%</span>
              </div>
              <div className="progress-bar">
                <div className="progress-fill met" style={{ width: `${summary.slaCompliancePercentage}%` }} />
              </div>
            </div>

            <div className="progress-container" style={{ marginTop: '1rem' }}>
              <div className="progress-label">
                <span>SLA Breached / Overdue</span>
                <span>{(100 - (summary.slaCompliancePercentage || 100))?.toFixed(1)}%</span>
              </div>
              <div className="progress-bar">
                <div className="progress-fill breached" style={{ width: `${100 - (summary.slaCompliancePercentage || 100)}%` }} />
              </div>
            </div>
          </div>
        </div>

        {/* Channel Mix Breakdown */}
        <div className="card analytics-card">
          <div className="card-header">
            <h3><PieChart size={18} /> Channel Volume Mix</h3>
          </div>
          <div className="card-body">
            <ul className="channel-list">
              {(summary.channelMix || [
                { source: 'Portal', count: summary.totalTickets },
                { source: 'Email', count: 0 },
                { source: 'WhatsApp', count: 0 },
                { source: 'Internal', count: 0 }
              ]).map(c => (
                <li key={c.source || c.sourceType} className="channel-item">
                  <span className="channel-name">{c.source || c.sourceType}</span>
                  <span className="channel-count">{c.count} tickets</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
