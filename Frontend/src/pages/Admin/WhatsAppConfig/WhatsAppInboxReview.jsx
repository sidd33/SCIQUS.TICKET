import React from 'react';
import { MessageSquare } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function WhatsAppInboxReview() {
  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><MessageSquare size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> WhatsApp Inbound Conversation Inbox</h1>
          <p>Module 8 — View inbound WhatsApp messages, phone number sender matching, and auto-generated tickets.</p>
        </div>
        <Link to="/admin/whatsapp-config" className="btn btn--secondary">Back to Settings</Link>
      </div>

      <div className="glass-card table-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Customer Mobile No</th>
                <th>Message Content</th>
                <th>Received Time</th>
                <th>Associated Ticket</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>+1 (555) 987-6543</td>
                <td>Hi, our office printer is throwing error 504. Please assist.</td>
                <td>{new Date().toLocaleString()}</td>
                <td><Link to="/tickets" className="btn btn--secondary btn--sm">TKT-000104</Link></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
