import React from 'react';
import { History, UserCheck, Tag, ArrowRightLeft, CheckCircle2, MessageSquare, AlertCircle } from 'lucide-react';
import './ActivityTimeline.scss';

export default function ActivityTimeline({ histories = [] }) {
  if (!histories || histories.length === 0) {
    return <div className="empty-timeline">No activity history recorded for this ticket yet.</div>;
  }

  const getEventIcon = (eventType = '') => {
    const type = eventType.toLowerCase();
    if (type.includes('assign') || type.includes('reassign')) return <UserCheck size={14} className="icon-assign" />;
    if (type.includes('status') || type.includes('state')) return <Tag size={14} className="icon-status" />;
    if (type.includes('priority') || type.includes('impact')) return <ArrowRightLeft size={14} className="icon-priority" />;
    if (type.includes('close') || type.includes('resolve')) return <CheckCircle2 size={14} className="icon-close" />;
    if (type.includes('comment') || type.includes('note')) return <MessageSquare size={14} className="icon-comment" />;
    return <History size={14} className="icon-default" />;
  };

  return (
    <div className="activity-timeline-container">
      {histories.map((h, i) => (
        <div key={h.id || h.historyId || i} className="timeline-node">
          <div className="node-marker">
            {getEventIcon(h.eventType || h.action)}
          </div>
          <div className="node-content">
            <div className="node-header">
              <span className="event-title">{h.eventType || h.action || 'Ticket Updated'}</span>
              <span className="event-date">{new Date(h.createdDate || h.timestamp || Date.now()).toLocaleString()}</span>
            </div>
            <p className="event-desc">{h.description || h.details || h.message || 'No additional details.'}</p>
            {h.performedByName && <span className="event-actor">By: {h.performedByName}</span>}
          </div>
        </div>
      ))}
    </div>
  );
}
