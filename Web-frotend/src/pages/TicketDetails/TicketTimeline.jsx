import { useEffect, useState } from "react";
import api from "../../api/axios";
import { getFriendlyErrorMessage } from "../../utils/apiErrors";
import "./TicketTimeline.scss";

const EVENT_ICONS = {
  Created: "➕",
  Edited: "✏️",
  StatusChanged: "🔄",
  Assigned: "👤",
  Transferred: "🏢",
  PriorityImpactChanged: "⚠️",
  Commented: "💬",
  AttachmentAdded: "📎",
  AttachmentRemoved: "🗑️",
  EmailSent: "📧",
  EmailReceived: "📨",
  WhatsAppSent: "💬",
  WhatsAppReceived: "📱",
  Reopened: "🔓",
  AutoClosed: "🔒",
  SlaBreached: "⏱️",
};

export default function TicketTimeline({ ticketId }) {
  const [timeline, setTimeline] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function fetchTimeline() {
      try {
        const res = await api.get(`/tickets/${ticketId}/timeline`);
        setTimeline(res.data);
      } catch (err) {
        setError(getFriendlyErrorMessage(err));
      } finally {
        setLoading(false);
      }
    }
    fetchTimeline();
  }, [ticketId]);

  if (loading) return <div className="timeline-loading">Loading timeline...</div>;
  if (error) return <div className="timeline-error">{error}</div>;
  if (!timeline.length) return <div className="timeline-empty">No history available for this ticket.</div>;

  return (
    <div className="ticket-timeline">
      <h3 className="timeline-title">Activity Timeline</h3>
      <div className="timeline-list">
        {timeline.map((event) => (
          <div key={event.eventId} className={`timeline-item ${event.isInternalNote ? 'internal-note' : ''}`}>
            <div className="timeline-icon">
              {EVENT_ICONS[event.changeType] || "📌"}
            </div>
            <div className="timeline-content">
              <div className="timeline-header">
                <strong>{event.actorName || event.actorId}</strong> 
                <span className="timeline-action">{event.changeType}</span>
                <span className="timeline-date">{new Date(event.timestamp).toLocaleString()}</span>
              </div>
              <div className="timeline-description">
                {event.description}
                {event.oldValue && event.newValue && (
                  <div className="timeline-values">
                    <span className="old-value">{event.oldValue}</span> ➡️ <span className="new-value">{event.newValue}</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
