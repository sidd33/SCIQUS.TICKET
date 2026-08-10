import React, { useState, useEffect } from 'react';
import axios from '../../api/axios';
import './EmailConfig.scss'; // Reusing some styles

const EmailInboxReview = () => {
    const [messages, setMessages] = useState([]);
    const [statusFilter, setStatusFilter] = useState('Pending');
    const [loading, setLoading] = useState(true);

    const fetchMessages = async () => {
        setLoading(true);
        try {
            const response = await axios.get(`/api/EmailTicketConfig/InboxReview?status=${statusFilter}`);
            setMessages(response.data);
        } catch (error) {
            console.error('Error fetching inbox messages:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchMessages();
    }, [statusFilter]);

    return (
        <div className="email-config-page">
            <h2>Email Inbox Review</h2>
            <div className="form-group" style={{ marginBottom: '1rem' }}>
                <label>Filter Status: </label>
                <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
                    <option value="Pending">Pending</option>
                    <option value="Processed">Processed</option>
                    <option value="Failed">Failed</option>
                </select>
            </div>

            {loading ? (
                <div>Loading...</div>
            ) : (
                <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '1rem' }}>
                    <thead>
                        <tr style={{ borderBottom: '2px solid #ccc', textAlign: 'left' }}>
                            <th>From</th>
                            <th>Subject</th>
                            <th>Date</th>
                            <th>Status / Reason</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {messages.length === 0 ? (
                            <tr><td colSpan="5">No messages found.</td></tr>
                        ) : (
                            messages.map(msg => (
                                <tr key={msg.emailInboxMessageId} style={{ borderBottom: '1px solid #eee' }}>
                                    <td>{msg.fromEmail}</td>
                                    <td>{msg.subject}</td>
                                    <td>{new Date(msg.emailReceivedDate).toLocaleString()}</td>
                                    <td>
                                        {msg.processingStatus}
                                        {msg.failureReason && <div style={{color:'red', fontSize:'0.85em'}}>{msg.failureReason}</div>}
                                    </td>
                                    <td>
                                        {msg.processingStatus === 'Failed' && (
                                            <button>Convert to Ticket</button> // Implementation needed
                                        )}
                                        {msg.processingStatus === 'Processed' && msg.createdTicketId && (
                                            <a href={`/tickets/${msg.createdTicketId}`}>View Ticket</a>
                                        )}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            )}
        </div>
    );
};

export default EmailInboxReview;
