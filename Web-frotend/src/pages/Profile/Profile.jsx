import { useEffect, useRef, useState } from 'react';
import api, { resolveImageUrl } from '../../api/axios';
import { getRoleLabel } from '../../auth/roles';
import './Profile.scss';

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];

export default function Profile() {
  const [user, setUser] = useState(() => JSON.parse(localStorage.getItem('user') || 'null'));
  const [photoUploading, setPhotoUploading] = useState(false);
  const [photoError, setPhotoError] = useState('');
  const fileInputRef = useRef(null);
  const isCustomer = user?.role === 'Customer';

  const [editing, setEditing] = useState(false);
  const [formData, setFormData] = useState({ firstName: '', lastName: '', email: '' });
  const [formError, setFormError] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});
  const [saving, setSaving] = useState(false);

  function syncUser(updated) {
    setUser(updated);
    localStorage.setItem('user', JSON.stringify(updated));
  }

  async function handleFileSelected(e) {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file || isCustomer) return;

    setPhotoError('');
    if (!ALLOWED_TYPES.includes(file.type)) { setPhotoError('Invalid image format.'); return; }
    if (file.size > MAX_FILE_SIZE) { setPhotoError('File is too large. Max 5MB.'); return; }

    const formDataUpload = new FormData();
    formDataUpload.append('file', file);

    try {
      setPhotoUploading(true);
      // Controller returns { profilePicture: path } only
      const { data } = await api.post(`/employees/${user.id}/profile-picture`, formDataUpload, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      syncUser({ ...user, profilePicture: data.profilePicture });
    } catch (err) {
      setPhotoError(err.response?.data?.message || 'Failed to upload photo.');
    } finally {
      setPhotoUploading(false);
    }
  }

  function openEdit() {
    setFormData({
      firstName: user.firstName || '',
      lastName: user.lastName || '',
      email: user.email || '',
    });
    setFormError(''); setFieldErrors({});
    setEditing(true);
  }

  function closeEdit() {
    setEditing(false); setFormError(''); setFieldErrors({});
  }

  function handleFormChange(e) {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  }

  async function handleSave(e) {
    e.preventDefault();
    setFormError(''); setFieldErrors({}); setSaving(true);

    try {
      // UpdateEmployeeDto: FirstName, LastName, Email, IsActive — no DepartmentId
      const { data } = await api.put(`/employees/${user.id}`, {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        isActive: user.isActive !== false,
      });
      syncUser({ ...user, ...data });
      setEditing(false);
    } catch (err) {
      const backendErrors = err.response?.data?.errors;
      if (backendErrors) {
        const mapped = {};
        Object.entries(backendErrors).forEach(([f, m]) => {
          mapped[f.charAt(0).toLowerCase() + f.slice(1)] = Array.isArray(m) ? m[0] : m;
        });
        setFieldErrors(mapped);
      } else {
        setFormError(err.response?.data?.message || 'Failed to update profile.');
      }
    } finally {
      setSaving(false);
    }
  }

  if (!user) return <div className="profile-page"><p className="error-text">Not signed in.</p></div>;

  const initials = `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`.toUpperCase();
  const photoUrl = !isCustomer ? resolveImageUrl(user.profilePicture) : null;

  return (
    <div className="profile-page">
      <div className="profile-header">
        <h1>My Profile</h1>
        <p>View and update your account information.</p>
      </div>

      <div className="profile-layout">
        <div className="profile-card profile-summary">
          <div
            className="profile-avatar"
            onClick={() => !isCustomer && fileInputRef.current?.click()}
            style={{ cursor: isCustomer ? 'default' : 'pointer', backgroundImage: photoUrl ? `url(${photoUrl})` : undefined, backgroundSize: 'cover', backgroundPosition: 'center' }}
          >
            {!photoUrl && initials}
          </div>

          {!isCustomer && (
            <>
              <input ref={fileInputRef} type="file" accept="image/*" style={{ display: 'none' }} onChange={handleFileSelected} />
              <button type="button" className="link-button" onClick={() => fileInputRef.current?.click()} disabled={photoUploading}>
                {photoUploading ? 'Uploading...' : 'Change photo'}
              </button>
              {photoError && <p className="error-text">{photoError}</p>}
            </>
          )}

          <h2>{user.firstName} {user.lastName}</h2>
          <span className="role-badge">{getRoleLabel(user)}</span>
          <p className="email">{user.email}</p>
        </div>

        <div className="profile-card profile-details">
          <div className="details-header">
            <div>
              <h2>Account Details</h2>
              <p>Your personal and role information.</p>
            </div>
            {!isCustomer && !editing && (
              <button type="button" className="edit-profile-btn" onClick={openEdit}>Edit</button>
            )}
          </div>

          {!editing ? (
            <div className="details-grid">
              <div className="detail-item"><span className="detail-label">First Name</span><span className="detail-value">{user.firstName}</span></div>
              <div className="detail-item"><span className="detail-label">Last Name</span><span className="detail-value">{user.lastName}</span></div>
              <div className="detail-item"><span className="detail-label">Email</span><span className="detail-value">{user.email}</span></div>
              {!isCustomer && <div className="detail-item"><span className="detail-label">Department</span><span className="detail-value">{user.departmentName || 'None'}</span></div>}
              <div className="detail-item">
                <span className="detail-label">Role</span>
                <span className="detail-value">{getRoleLabel(user)}</span>
                {!isCustomer && <span className="field-note">Managed by an administrator</span>}
              </div>
              {!isCustomer && (
                <div className="detail-item">
                  <span className="detail-label">Status</span>
                  <span className="detail-value">{user.isActive === false ? 'Inactive' : 'Active'}</span>
                  <span className="field-note">Managed by an administrator</span>
                </div>
              )}
            </div>
          ) : (
            <form className="edit-profile-form" onSubmit={handleSave} noValidate>
              {formError && <p className="error-text">{formError}</p>}

              <div className="form-row">
                <label>
                  First Name
                  <input name="firstName" value={formData.firstName} onChange={handleFormChange} className={fieldErrors.firstName ? 'input-error' : ''} />
                  {fieldErrors.firstName && <span className="field-error-text">{fieldErrors.firstName}</span>}
                </label>
                <label>
                  Last Name
                  <input name="lastName" value={formData.lastName} onChange={handleFormChange} className={fieldErrors.lastName ? 'input-error' : ''} />
                  {fieldErrors.lastName && <span className="field-error-text">{fieldErrors.lastName}</span>}
                </label>
              </div>

              <label>
                Email
                <input type="email" name="email" value={formData.email} onChange={handleFormChange} className={fieldErrors.email ? 'input-error' : ''} />
                {fieldErrors.email && <span className="field-error-text">{fieldErrors.email}</span>}
              </label>

              <p className="no-policies">
                Role and active status changes must be made by an administrator.
              </p>

              <div className="edit-form-actions">
                <button type="button" className="cancel-btn" onClick={closeEdit} disabled={saving}>Cancel</button>
                <button type="submit" className="save-btn" disabled={saving}>{saving ? 'Saving...' : 'Save Changes'}</button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}