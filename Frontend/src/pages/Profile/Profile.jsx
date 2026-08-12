import React, { useState, useEffect } from 'react';
import { UserCircle, Shield, Mail, Building2, Phone, Calendar, Globe, Award, Camera, CheckCircle2, AlertCircle } from 'lucide-react';
import api from '../../api/axios';

export default function Profile() {
  const user = JSON.parse(localStorage.getItem('user') || 'null');
  const [profileData, setProfileData] = useState(null);
  const [avatar, setAvatar] = useState(user?.profileImageUrl || null);
  const [uploading, setUploading] = useState(false);
  const [message, setMessage] = useState(null);

  useEffect(() => {
    fetchDetailedProfile();
  }, []);

  const fetchDetailedProfile = async () => {
    try {
      if (user?.role === 'Customer') {
        const res = await api.get(`/Accounts/${user.id || user.userId}`);
        setProfileData(res.data);
      } else {
        const res = await api.get(`/Employees/${user.id || user.userId}`);
        setProfileData(res.data);
      }
    } catch {
      // Use user object as fallback
      setProfileData(user);
    }
  };

  const handleAvatarChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    setUploading(true);
    setMessage(null);

    try {
      const res = await api.post('/ProfileUpload/avatar', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });

      const newAvatarUrl = `http://localhost:5239${res.data.avatarUrl}`;
      setAvatar(newAvatarUrl);

      const updatedUser = { ...user, profileImageUrl: newAvatarUrl };
      localStorage.setItem('user', JSON.stringify(updatedUser));

      setMessage({ type: 'success', text: 'Profile picture updated successfully!' });
    } catch (err) {
      console.error('Avatar upload failed:', err);
      setMessage({ type: 'error', text: err.response?.data?.message || 'Failed to upload profile picture.' });
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div>
          <h1><UserCircle size={24} style={{ verticalAlign: 'middle', marginRight: '8px' }} /> User Account Profile</h1>
          <p>Inspect your personal credentials, identity roles, employee/account metadata, and avatar picture.</p>
        </div>
      </div>

      {message && (
        <div className={`glass-card ${message.type === 'success' ? 'badge--resolved' : 'badge--breached'}`} style={{ padding: '0.85rem 1.25rem', marginBottom: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
          {message.type === 'success' ? <CheckCircle2 size={18} /> : <AlertCircle size={18} />}
          <span>{message.text}</span>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1.5fr', gap: '1.5rem' }}>
        {/* Main Avatar Card */}
        <div className="glass-card" style={{ padding: '2rem', textAlign: 'center', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <div style={{ position: 'relative', marginBottom: '1.25rem' }}>
            {avatar ? (
              <img src={avatar} alt="Avatar" style={{ width: '100px', height: '100px', borderRadius: '50%', objectFit: 'cover', border: '3px solid var(--accent-primary)' }} />
            ) : (
              <div style={{ width: '100px', height: '100px', borderRadius: '50%', background: 'linear-gradient(135deg, #6366f1 0%, #a855f7 100%)', color: 'white', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '2.2rem', fontWeight: 800 }}>
                {`${user?.firstName?.[0] || 'U'}${user?.lastName?.[0] || ''}`}
              </div>
            )}

            <label htmlFor="avatar-file-input" style={{ position: 'absolute', bottom: 0, right: 0, background: '#6366f1', color: 'white', padding: '8px', borderRadius: '50%', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', boxShadow: '0 4px 12px rgba(0,0,0,0.4)' }} title="Upload Profile Picture">
              <Camera size={16} />
            </label>
            <input id="avatar-file-input" type="file" accept="image/*" style={{ display: 'none' }} onChange={handleAvatarChange} disabled={uploading} />
          </div>

          <h2 style={{ color: 'white', fontSize: '1.35rem', margin: '0 0 4px 0' }}>
            {profileData?.accountName || `${profileData?.name || user?.firstName || 'User'} ${user?.lastName || ''}`}
          </h2>
          <span className="badge badge--progress" style={{ marginBottom: '1rem' }}>
            {Array.isArray(user?.role) ? user.role.join(', ') : (user?.role || 'User')}
          </span>

          <div style={{ width: '100%', borderTop: '1px solid var(--bg-card-border)', paddingTop: '1.25rem', textAlign: 'left', display: 'flex', flexDirection: 'column', gap: '0.85rem', fontSize: '0.9rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Award size={16} color="var(--text-muted)" />
              <span style={{ color: 'var(--text-muted)' }}>Employee Code:</span>
              <strong style={{ color: 'white', marginLeft: 'auto', fontFamily: 'monospace' }}>
                {profileData?.employeeId || profileData?.autoGenerateAccountId || 'EMP-1001'}
              </strong>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Shield size={16} color="var(--text-muted)" />
              <span style={{ color: 'var(--text-muted)' }}>Account GUID:</span>
              <strong style={{ color: '#818cf8', marginLeft: 'auto', fontFamily: 'monospace', fontSize: '0.8rem' }}>
                {profileData?.id || profileData?.accountId || user?.id || 'GUID'}
              </strong>
            </div>
          </div>
        </div>

        {/* Detailed Domain Model Fields Card */}
        <div className="glass-card" style={{ padding: '2rem' }}>
          <h3 style={{ color: 'white', marginTop: 0, marginBottom: '1.25rem', borderBottom: '1px solid var(--bg-card-border)', paddingBottom: '0.75rem' }}>
            Domain Identity Details
          </h3>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1.15rem', fontSize: '0.92rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <Mail size={16} color="var(--text-muted)" />
              <span style={{ color: 'var(--text-muted)', width: '160px' }}>Primary Email:</span>
              <strong style={{ color: 'white' }}>{profileData?.email || user?.email || 'user@sciqustickets.com'}</strong>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <Phone size={16} color="var(--text-muted)" />
              <span style={{ color: 'var(--text-muted)', width: '160px' }}>Registered Mobile:</span>
              <strong style={{ color: 'white' }}>{profileData?.registeredMobileNumber || profileData?.phone || '+1 (555) 019-2831'}</strong>
            </div>

            {profileData?.secondMobileNumber && (
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Phone size={16} color="var(--text-muted)" />
                <span style={{ color: 'var(--text-muted)', width: '160px' }}>Secondary Mobile:</span>
                <strong style={{ color: 'white' }}>{profileData.secondMobileNumber}</strong>
              </div>
            )}

            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <Building2 size={16} color="var(--text-muted)" />
              <span style={{ color: 'var(--text-muted)', width: '160px' }}>Department:</span>
              <strong style={{ color: 'white' }}>{profileData?.departmentName || user?.departmentName || 'IT Support & Infrastructure'}</strong>
            </div>

            {profileData?.designation && (
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Award size={16} color="var(--text-muted)" />
                <span style={{ color: 'var(--text-muted)', width: '160px' }}>Designation Title:</span>
                <strong style={{ color: 'white' }}>{profileData.designation}</strong>
              </div>
            )}

            {profileData?.website && (
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Globe size={16} color="var(--text-muted)" />
                <span style={{ color: 'var(--text-muted)', width: '160px' }}>Corporate Website:</span>
                <strong style={{ color: '#818cf8' }}>{profileData.website}</strong>
              </div>
            )}

            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <Calendar size={16} color="var(--text-muted)" />
              <span style={{ color: 'var(--text-muted)', width: '160px' }}>Account Since:</span>
              <strong style={{ color: 'white' }}>
                {profileData?.createdDate ? new Date(profileData.createdDate).toLocaleDateString() : 'January 2026'}
              </strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
