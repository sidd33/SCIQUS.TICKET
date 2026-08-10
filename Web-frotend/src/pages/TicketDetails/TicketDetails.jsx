import { useEffect, useState, useCallback } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import api from "../../api/axios";
import { getCurrentUser, isCustomer, isAdminOrAbove } from "../../auth/roles";import { getFriendlyErrorMessage } from "../../utils/apiErrors";
import "./TicketDetails.scss";

const STATUS_OPTIONS = ["Unassigned", "Assigned", "InProgress", "Completed", "Closed", "Reopened"];

const STATUS_BADGE = {
  Unassigned: "badge--critical",
  Assigned: "badge--warning",
  InProgress: "badge--high-priority",
  Completed: "badge--success",
  Closed: "badge--inactive",
  Reopened: "badge--warning",
};

const STATUS_LABEL = {
  Unassigned: "Unassigned",
  Assigned: "Assigned",
  InProgress: "In Progress",
  Completed: "Completed",
  Closed: "Closed",
  Reopened: "Reopened",
};

function TicketDetails() {
    const { ticketId } = useParams();
  const navigate = useNavigate();
  const currentUser = getCurrentUser();
  const customer = isCustomer(currentUser);
  const canManage = isAdminOrAbove(currentUser);

  const [ticket, setTicket] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState({ title: "", description: "", status: "" });
  const [saveError, setSaveError] = useState("");
  const [saving, setSaving] = useState(false);

  const [emailReply, setEmailReply] = useState("");
  const [whatsAppReply, setWhatsAppReply] = useState("");
  const [whatsAppTemplate, setWhatsAppTemplate] = useState("");
  const [sendingReply, setSendingReply] = useState(false);

  const loadTicket = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
    const res = await api.get(`/tickets/${ticketId}`);
      setTicket(res.data);
      setForm({
        title: res.data.title,
        description: res.data.description,
        status: res.data.status,
      });
    } catch (err) {
      if (err.response?.status === 403) {
        setError("You don't have permission to view this ticket.");
      } else if (err.response?.status === 404) {
        setError("This ticket doesn't exist.");
      } else {
        setError(getFriendlyErrorMessage(err));
      }
    } finally {
      setLoading(false);
    }
  }, [ticketId]);

  useEffect(() => {
    loadTicket();
  }, [loadTicket]);

  async function handleSave(e) {
    e.preventDefault();
    setSaveError("");
    setSaving(true);
    try {
      const payload = { title: form.title, description: form.description };
      if (!customer) payload.status = form.status;
const res = await api.put(`/tickets/${ticketId}`, payload);
      setTicket(res.data);
      setEditing(false);
    } catch (err) {
      setSaveError(getFriendlyErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  async function handleEmailReply(e) {
    e.preventDefault();
    setSendingReply(true);
    try {
      await api.post(`/tickets/${ticketId}/email-reply`, `"${emailReply}"`, { headers: { 'Content-Type': 'application/json' }});
      setEmailReply("");
      alert("Email sent!");
      loadTicket(); // Reload ticket to see history/comments
    } catch (err) {
      alert("Failed to send email");
    } finally {
      setSendingReply(false);
    }
  }

  async function handleWhatsAppReply(e) {
    e.preventDefault();
    setSendingReply(true);
    try {
      await api.post(`/tickets/${ticketId}/whatsapp-reply`, { body: whatsAppReply, templateName: whatsAppTemplate });
      setWhatsAppReply("");
      setWhatsAppTemplate("");
      alert("WhatsApp message sent!");
      loadTicket();
    } catch (err) {
      alert("Failed to send WhatsApp message");
    } finally {
      setSendingReply(false);
    }
  }

  if (loading) {
    return <div className="ticket-details-loading">Loading ticket...</div>;
  }

  if (error) {
    return (
      <div className="ticket-details-page">
        <button className="btn btn--ghost" onClick={() => navigate(-1)}>← Back</button>
        <div className="card empty-state">
          <h3>Unable to load ticket</h3>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="ticket-details-page">
      <button className="btn btn--ghost" onClick={() => navigate(-1)}>← Back</button>

      <div className="card ticket-details-card">
        <div className="ticket-details-header">
          <div>
            <span className={`badge ${STATUS_BADGE[ticket.status] || "badge--inactive"}`}>
              {STATUS_LABEL[ticket.status] || ticket.status}
            </span>
            {ticket.isSlaBreached && <span className="badge badge--critical">SLA Breached</span>}
          </div>
          {!editing && (
            <button className="btn btn--secondary" onClick={() => setEditing(true)}>Edit</button>
          )}
        </div>

        {editing ? (
          <form onSubmit={handleSave} className="ticket-edit-form">
            {saveError && <div className="field-error">{saveError}</div>}
            <div className="field">
              <label>Title</label>
              <input
                required
                value={form.title}
                onChange={(e) => setForm({ ...form, title: e.target.value })}
              />
            </div>
            <div className="field">
              <label>Description</label>
              <textarea
                required
                rows={5}
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
              />
            </div>
            {!customer && (
              <div className="field">
                <label>Status</label>
                <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
                  {STATUS_OPTIONS.map((s) => (
                    <option key={s} value={s}>{STATUS_LABEL[s]}</option>
                  ))}
                </select>
              </div>
            )}
            <div className="ticket-edit-actions">
              <button type="button" className="btn btn--secondary" onClick={() => setEditing(false)}>Cancel</button>
              <button type="submit" className="btn btn--primary" disabled={saving}>
                {saving ? "Saving..." : "Save Changes"}
              </button>
            </div>
          </form>
        ) : (
          <>
            <h1 className="ticket-title">{ticket.title}</h1>
            <p className="ticket-description">{ticket.description}</p>

            <div className="ticket-meta-grid">
              <div>
                <span className="ticket-meta-label">Customer</span>
                <span className="ticket-meta-value">{ticket.customerName}</span>
              </div>
              <div>
                <span className="ticket-meta-label">Customer email</span>
                <span className="ticket-meta-value">{ticket.customerEmail}</span>
              </div>
              <div>
                <span className="ticket-meta-label">Created</span>
                <span className="ticket-meta-value">{new Date(ticket.createdAt).toLocaleString()}</span>
              </div>
              <div>
                <span className="ticket-meta-label">SLA deadline</span>
                <span className="ticket-meta-value">{new Date(ticket.slaStartTime).toLocaleString()}</span>
              </div>
            </div>
          </>
        )}
      </div>

      {ticket && !editing && (
        <div className="card ticket-details-card" style={{ marginTop: '1rem' }}>
          <h3>Communication</h3>
          
          {/* Example conditional display. In a real app we'd check ticket.sourceType === 'Email' etc */}
          <div className="communication-section">
            <h4>Email Reply</h4>
            <form onSubmit={handleEmailReply}>
              <textarea 
                rows={3} 
                value={emailReply} 
                onChange={e => setEmailReply(e.target.value)}
                placeholder="Type email reply here..."
                required
                style={{ width: '100%', marginBottom: '0.5rem' }}
              />
              <button type="submit" className="btn btn--primary" disabled={sendingReply}>Send Email</button>
            </form>
          </div>

          <div className="communication-section" style={{ marginTop: '2rem' }}>
            <h4>WhatsApp Reply</h4>
            <form onSubmit={handleWhatsAppReply}>
              <input 
                type="text" 
                placeholder="Template Name (optional, required if outside 24h window)" 
                value={whatsAppTemplate}
                onChange={e => setWhatsAppTemplate(e.target.value)}
                style={{ width: '100%', marginBottom: '0.5rem', padding: '0.5rem' }}
              />
              <textarea 
                rows={3} 
                value={whatsAppReply} 
                onChange={e => setWhatsAppReply(e.target.value)}
                placeholder="Type WhatsApp reply here..."
                required
                style={{ width: '100%', marginBottom: '0.5rem' }}
              />
              <button type="submit" className="btn btn--primary" disabled={sendingReply}>Send WhatsApp</button>
            </form>
          </div>
        </div>
      )}

      {canManage && (
        <p className="ticket-manage-hint">
          Need to reassign this ticket? Do that from the <Link to="/tickets">Tickets</Link> list.
        </p>
      )}
    </div>
  );
}

export default TicketDetails;