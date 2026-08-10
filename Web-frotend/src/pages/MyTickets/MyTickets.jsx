import { useEffect, useState, useCallback } from "react";
import { Link } from "react-router-dom";
import api from "../../api/axios";
import { getFriendlyErrorMessage } from "../../utils/apiErrors";
import { getCurrentUser } from "../../auth/roles";
import "./MyTickets.scss";

const STATUS_CONFIG = {
  Unassigned: { label: "Not Assigned", color: "#64748b", icon: "⊝" },
  Assigned: { label: "Assigned", color: "#475569"},
  InProgress: { label: "In Progress", color: "#2563eb", icon: "⊖" },
  Completed: { label: "Resolved", color: "#16a34a", icon: "⊙" },
  Closed: { label: "Closed", color: "#475569", icon: "🔒" },
  Reopened: { label: "Reopened", color: "#ea580c", icon: "↺" },
};

const EMPTY_FORM = { title: "", description: "", departmentId: "" };

function MyTickets() {
  const [tickets, setTickets] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);

  const loadTickets = useCallback(async () => {
    const user = getCurrentUser();
    const userId = user?.id || user?.userId || user?._id;

    if (!userId) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");
    try {
      const res = await api.get(`/customers/${userId}`);
      setTickets(res.data?.tickets || []);
    } catch (err) {
      setError(getFriendlyErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, []);

  const loadDepartments = useCallback(async () => {
    try {
      const res = await api.get("/departments", { params: { pageNumber: 1, pageSize: 100 } });
      setDepartments(res.data?.items || []);
    } catch {
      // fallback
    }
  }, []);

  useEffect(() => {
    loadTickets();
    loadDepartments();
  }, [loadTickets, loadDepartments]);

  function openCreateModal() {
    setForm(EMPTY_FORM);
    setFormError("");
    setShowCreateModal(true);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const user = getCurrentUser();
    const userId = user?.id || user?.userId || user?._id;

    setFormError("");
    setSaving(true);
    try {
      await api.post("/tickets", { ...form, customerId: userId });
      setShowCreateModal(false);
      loadTickets();
    } catch (err) {
      setFormError(getFriendlyErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="tickets-page">
      {/* Header section */}
      <div className="tickets-header">
        <div>
          <h1>My Tickets</h1>
          <p className="tickets-subtitle">
            Showing {tickets.length} support requests submitted by you
          </p>
        </div>
        <button className="btn btn--primary btn-new-ticket" onClick={openCreateModal}>
          + New Ticket
        </button>
      </div>

      {error && <div className="tickets-error">{error}</div>}

      {/* Main Table Container */}
      <div className="tickets-table-card">
        {loading ? (
          <div className="tickets-loading">Loading tickets...</div>
        ) : tickets.length === 0 ? (
          <div className="empty-state">
            <h3>No tickets found</h3>
            <p>Submit a ticket and our team will handle it.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="tickets-table">
              <thead>
                <tr>
                  <th>Ticket Info</th>
                  <th>Status</th>
                  <th>SLA Health</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {tickets.map((ticket) => {
                  const statusInfo = STATUS_CONFIG[ticket.status] || {
                    label: ticket.status,
                    color: "#475569",
                    icon: "•",
                  };
                  const shortId = `TKT-${ticket.id.slice(0, 4).toUpperCase()}`;

                  return (
                    <tr key={ticket.id}>
                      {/* Ticket Info */}
                      <td>
                        <div className="ticket-info-cell">
                          <span className="ticket-id">{shortId}</span>
                          <Link to={`/tickets/${ticket.id}`} className="ticket-title">
                            {ticket.title || "Untitled Ticket"}
                          </Link>
                          <span className="ticket-meta">
                            Created {new Date(ticket.createdAt).toLocaleDateString()}
                          </span>
                        </div>
                      </td>

                      {/* Status */}
                      <td>
                        <div className="status-cell" style={{ color: statusInfo.color }}>
                          <span className="status-icon">{statusInfo.icon}</span>
                          <span className="status-label">{statusInfo.label}</span>
                        </div>
                      </td>

                      {/* SLA Health */}
                      <td>
                        <div className="sla-cell">
                          {ticket.isSlaBreached ? (
                            <span className="sla-status sla-breached">⏰ Overdue</span>
                          ) : (
                            <span className="sla-status sla-ok">✓ Within SLA</span>
                          )}
                        </div>
                      </td>

                      {/* Actions */}
                      <td className="text-right">
                        <div className="actions-cell">
                          <Link
                            to={`/tickets/${ticket.id}`}
                            className="action-icon"
                            title="View Ticket"
                          >
                            👁
                          </Link>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal Dialog */}
      {showCreateModal && (
        <div className="modal-backdrop" onClick={() => setShowCreateModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>New Ticket</h2>
              <button className="modal-close" onClick={() => setShowCreateModal(false)}>
                ×
              </button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="modal-body">
                {formError && <div className="field-error">{formError}</div>}
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
                    rows={4}
                    value={form.description}
                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                  />
                </div>
                <div className="field">
                  <label>Department</label>
                  <select
                    required
                    value={form.departmentId}
                    onChange={(e) => setForm({ ...form, departmentId: e.target.value })}
                  >
                    <option value="">Select department</option>
                    {departments.map((d) => (
                      <option key={d.id} value={d.id}>
                        {d.name}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn--secondary"
                  onClick={() => setShowCreateModal(false)}
                >
                  Cancel
                </button>
                <button type="submit" className="btn btn--primary" disabled={saving}>
                  {saving ? "Submitting..." : "Submit Ticket"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default MyTickets;