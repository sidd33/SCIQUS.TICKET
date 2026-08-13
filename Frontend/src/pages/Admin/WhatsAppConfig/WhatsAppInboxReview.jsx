import React, { useState, useEffect } from 'react';
import { MessageSquare } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../../../api/axios';

export default function WhatsAppInboxReview() {
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchMessages = async () => {
      try {
        const response = await api.get('/WhatsAppConfig/InboxReview');
        setMessages(response.data);
      } catch (error) {
        console.error('Failed to fetch WhatsApp inbox messages:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchMessages();
  }, []);

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
              {loading ? (
                <tr>
                  <td colSpan="4" style={{ textAlign: 'center', padding: '2rem' }}>Loading messages...</td>
                </tr>
              ) : messages.length === 0 ? (
                <tr>
                  <td colSpan="4" style={{ textAlign: 'center', padding: '2rem' }}>No messages found in inbox.</td>
                </tr>
              ) : (
                messages.map(msg => (
                  <tr key={msg.whatsAppInboxMessageId}>
                    <td>{msg.fromPhone}</td>
                    <td>{msg.body}</td>
                    <td>{new Date(msg.receivedDate).toLocaleString()}</td>
                    <td>
                      {msg.createdTicketId ? (
                        <Link to={`/tickets/${msg.createdTicketId}`} className="btn btn--secondary btn--sm">View Ticket</Link>
                      ) : (
                        <span style={{ color: '#9ca3af' }}>Unassigned</span>
                      )}
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
