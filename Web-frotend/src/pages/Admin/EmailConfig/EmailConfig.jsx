import React, { useState, useEffect } from 'react';
import axios from '../../../api/axios';
import './EmailConfig.scss';

const EmailConfig = () => {
    const [config, setConfig] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchConfig = async () => {
            try {
                const response = await axios.get('/api/EmailTicketConfig');
                setConfig(response.data);
            } catch (error) {
                // Not found is expected if not configured yet
                setConfig({
                    provider: 'Google',
                    emailAddress: '',
                    isEnabled: false,
                    autoCreateEnabled: false,
                    pollingIntervalMinutes: 15
                });
            } finally {
                setLoading(false);
            }
        };
        fetchConfig();
    }, []);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setConfig(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            await axios.post('/api/EmailTicketConfig', config);
            alert('Email config saved successfully!');
        } catch (error) {
            console.error(error);
            alert('Failed to save email config.');
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <div className="email-config-page">
            <h2>Email Ticket Configuration</h2>
            <form onSubmit={handleSave} className="config-form">
                <div className="form-group">
                    <label>Provider</label>
                    <select name="provider" value={config.provider} onChange={handleChange}>
                        <option value="Google">Google (OAuth)</option>
                        <option value="Outlook">Outlook (OAuth)</option>
                    </select>
                </div>
                
                <div className="form-group">
                    <label>Email Address</label>
                    <input type="email" name="emailAddress" value={config.emailAddress || ''} onChange={handleChange} required />
                </div>
                
                <div className="form-group checkbox-group">
                    <label>
                        <input type="checkbox" name="isEnabled" checked={config.isEnabled || false} onChange={handleChange} />
                        Enable Email Polling
                    </label>
                </div>
                
                <div className="form-group checkbox-group">
                    <label>
                        <input type="checkbox" name="autoCreateEnabled" checked={config.autoCreateEnabled || false} onChange={handleChange} />
                        Auto-Create Tickets
                    </label>
                </div>

                <div className="form-group">
                    <label>Polling Interval (mins)</label>
                    <input type="number" name="pollingIntervalMinutes" value={config.pollingIntervalMinutes || 15} onChange={handleChange} min="15" />
                </div>

                {/* Additional inputs for default ticket mappings (Priority, Impact, Department, Type, SubType, Assignee) would go here */}

                <button type="submit" className="save-btn">Save Configuration</button>
            </form>
        </div>
    );
};

export default EmailConfig;
