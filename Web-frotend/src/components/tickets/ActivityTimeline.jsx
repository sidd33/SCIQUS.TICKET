import React from 'react';
import {
  PlusCircle,
  RefreshCw,
  UserCheck,
  ArrowRightLeft,
  AlertOctagon,
  MessageSquare,
  Paperclip,
  Trash2,
  Mail,
  AlertTriangle,
  CheckCircle2,
  Clock
} from 'lucide-react';
import './ActivityTimeline.scss';

const EVENT_CONFIG = {
  Created: { icon: PlusCircle, color: '#3b82f6', label: 'Ticket Created' },
  Edited: { icon: RefreshCw, color: '#6b7280', label: 'Ticket Details Edited' },
  StatusChanged: { icon: RefreshCw, color: '#8b5cf6', label: 'Status Changed' },
  Assigned: { icon: UserCheck, color: '#10b981', label: 'Ticket Assigned' },
  Transferred: { icon: ArrowRightLeft, color: '#f59e0b', label: 'Transferred Department' },
  PriorityImpactChanged: { icon: AlertOctagon, color: '#ef4444', label: 'Priority / Impact Changed' },
  Commented: { icon: MessageSquare, color: '#06b6d4', label: 'Comment Added' },
  AttachmentAdded: { icon: Paperclip, color: '#3b82f6', label: 'Attachment Uploaded' },
  AttachmentRemoved: { icon: Trash2, color: '#ef4444', label: 'Attachment Removed' },
  EmailSent: { icon: Mail, color: '#3b82f6', label: 'Outbound Email Sent' },
  WhatsAppSent: { icon: MessageSquare, color: '#10b981', label: 'Outbound WhatsApp Message' },
  SlaBreached: { icon: AlertTriangle, color: '#ef4444', label: 'SLA Clock Breached' },
  AutoClosed: { icon: CheckCircle2, color: '#6b7280', label: 'Auto-Closed by System' }
};

export default function ActivityTimeline({ events = [] }) {
  if (!events || events.length === 0) {
    return (
      <div className="empty-timeline">
        <Clock size={24} />
        <p>No activity records available yet.</p>
      </div>
    );
  }

  return (
    <div className="activity-timeline">
      {events.map((event, index) => {
        const config = EVENT_CONFIG[event.changeType || event.type] || {
          icon: Clock,
          color: '#6b7280',
          label: event.changeType || event.type || 'Activity'
        };
        const IconComponent = config.icon;

        return (
          <div key={event.id || index} className="timeline-item">
            <div className="timeline-icon-wrapper" style={{ backgroundColor: `${config.color}15`, color: config.color }}>
              <IconComponent size={16} />
            </div>

            <div className="timeline-content">
              <div className="timeline-header">
                <span className="timeline-title">{config.label}</span>
                <span className="timeline-time">
                  {new Date(event.createdDate || event.timestamp || Date.now()).toLocaleString()}
                </span>
              </div>

              {event.changeDescription && (
                <p className="timeline-desc">{event.changeDescription}</p>
              )}

              {event.reason && (
                <div className="timeline-reason">
                  <strong>Reason:</strong> {event.reason}
                </div>
              )}

              {(event.oldValue || event.newValue) && (
                <div className="timeline-diff">
                  {event.oldValue && <span className="diff-old">{event.oldValue}</span>}
                  {event.oldValue && event.newValue && <span className="diff-arrow">→</span>}
                  {event.newValue && <span className="diff-new">{event.newValue}</span>}
                </div>
              )}

              <div className="timeline-actor">
                By: {event.changedByUserName || event.actorName || 'System'}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
