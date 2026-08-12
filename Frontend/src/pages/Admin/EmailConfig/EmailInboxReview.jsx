import React from 'react';
import { Inbox, CheckCircle2 } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function EmailInboxReview() {
  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><Inbox size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> Raw Email Inbox Review Queue</h1>
          <p>Module 6 — Inspect unparsed or pending inbound support emails for ticket auto-creation.</p>
        </div>
        <Link to="/admin/email-ticket-config" className="btn btn--secondary">Back to Settings</Link>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Sender Address</th>
                <th>Email Subject</th>
                <th>Received Date</th>
                <th>Match Status</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>john.doe@acme.com</td>
                <td>VPN Disconnection issue on Laptop</td>
                <td>{new Date().toLocaleString()}</td>
                <td><span className="badge badge--resolved">Matched & Ticket Created</span></td>
                <td><Link to="/tickets" className="btn btn--secondary btn--sm">View Ticket</Link></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
