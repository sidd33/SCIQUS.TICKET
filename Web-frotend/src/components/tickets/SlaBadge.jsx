import React from 'react';
import { Clock, AlertTriangle, CheckCircle2, MinusCircle } from 'lucide-react';
import './SlaBadge.scss';

export default function SlaBadge({ ticket }) {
  if (!ticket) return null;

  const { isSlaBreached, slaMetStatus, slaDueDate } = ticket;

  if (slaMetStatus === 'Not Applicable') {
    return (
      <span className="sla-badge sla-badge--na">
        <MinusCircle size={13} /> N/A
      </span>
    );
  }

  if (slaMetStatus === 'Met') {
    return (
      <span className="sla-badge sla-badge--met">
        <CheckCircle2 size={13} /> SLA Met
      </span>
    );
  }

  if (slaMetStatus === 'Missed' || isSlaBreached) {
    return (
      <span className="sla-badge sla-badge--breached">
        <AlertTriangle size={13} /> SLA Breached
      </span>
    );
  }

  // Calculate live time remaining
  if (slaDueDate) {
    const dueTime = new Date(slaDueDate).getTime();
    const now = Date.now();
    const diffMs = dueTime - now;

    if (diffMs <= 0) {
      return (
        <span className="sla-badge sla-badge--breached">
          <AlertTriangle size={13} /> Overdue
        </span>
      );
    }

    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffMins = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

    const isUrgent = diffHours < 4;

    return (
      <span className={`sla-badge ${isUrgent ? 'sla-badge--urgent' : 'sla-badge--due'}`}>
        <Clock size={13} /> {diffHours > 0 ? `${diffHours}h ${diffMins}m` : `${diffMins}m`} left
      </span>
    );
  }

  return (
    <span className="sla-badge sla-badge--on-track">
      <CheckCircle2 size={13} /> On Track
    </span>
  );
}
