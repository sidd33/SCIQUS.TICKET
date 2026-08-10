import React, { useState, useEffect } from 'react';
import axios from '../../api/axios';
import '../EmailConfig/EmailConfig.scss'; // Reusing styles

const WhatsAppConfig = () => {
    const [config, setConfig] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchConfig = async () => {
            try {
                const response = await axios.get('/api/WhatsAppConfig');
                setConfig(response.data);
            } catch (error) {
                setConfig({
                    provider: 0, // MetaCloudAPI
                    businessPhoneNumberId: '',
                    encryptedApiToken: '',
                    webhookVerifyToken: '',
                    isEnabled: false,
                    autoCreateEnabled: false
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
            [name]: type === 'checkbox' ? checked : (name === 'provider' ? parseInt(value) : value)
        }));
    };

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            await axios.post('/api/WhatsAppConfig', config);
            alert('WhatsApp config saved successfully!');
        } catch (error) {
            console.error(error);
            alert('Failed to save WhatsApp config.');
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <div className="email-config-page">
            <h2>WhatsApp Configuration</h2>
            <form onSubmit={handleSave} className="config-form">
                <div className="form-group">
                    <label>Provider</label>
                    <select name="provider" value={config.provider} onChange={handleChange}>
                        <option value={0}>Meta Cloud API</option>
                        <option value={1}>Twilio</option>
                        <option value={2}>Gupshup</option>
                    </select>
                </div>
                
                <div className="form-group">
                    <label>Business Phone Number ID</label>
                    <input type="text" name="businessPhoneNumberId" value={config.businessPhoneNumberId || ''} onChange={handleChange} required />
                </div>

                <div className="form-group">
                    <label>API Token (Encrypted)</label>
                    <input type="password" name="encryptedApiToken" value={config.encryptedApiToken || ''} onChange={handleChange} placeholder="Leave blank to keep existing" />
                </div>

                <div className="form-group">
                    <label>Webhook Verify Token</label>
                    <input type="password" name="webhookVerifyToken" value={config.webhookVerifyToken || ''} onChange={handleChange} placeholder="Leave blank to keep existing" />
                </div>
                
                <div className="form-group checkbox-group">
                    <label>
                        <input type="checkbox" name="isEnabled" checked={config.isEnabled || false} onChange={handleChange} />
                        Enable WhatsApp Channel
                    </label>
                </div>
                
                <div className="form-group checkbox-group">
                    <label>
                        <input type="checkbox" name="autoCreateEnabled" checked={config.autoCreateEnabled || false} onChange={handleChange} />
                        Auto-Create Tickets
                    </label>
                </div>

                <button type="submit" className="save-btn">Save Configuration</button>
            </form>
        </div>
    );
};

export default WhatsAppConfig;
