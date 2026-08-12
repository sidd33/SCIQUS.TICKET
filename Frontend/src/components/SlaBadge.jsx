import React from 'react';
import { Clock, AlertTriangle, CheckCircle, ShieldAlert } from 'lucide-react';
import './SlaBadge.scss';

export default function SlaBadge({ dueDate, isBreached, statusName, isMet }) {
  if (!dueDate) return <span className="sla-tag sla-tag--none">N/A</span>;

  if (isBreached) {
    return (
      <span className="sla-tag sla-tag--breached" title="SLA Breached">
        <ShieldAlert size={13} /> SLA Breached
      </span>
    );
  }

  if (isMet || statusName === 'Closed' || statusName === 'Resolved') {
    return (
      <span className="sla-tag sla-tag--met" title="SLA Met">
        <CheckCircle size={13} /> SLA Met
      </span>
    );
  }

  const dueTime = new Date(dueDate).getTime();
  const now = Date.now();
  const diffMs = dueTime - now;

  if (diffMs <= 0) {
    return (
      <span className="sla-tag sla-tag--breached" title="SLA Overdue">
        <AlertTriangle size={13} /> Overdue
      </span>
    );
  }

  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffMins = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

  const isWarning = diffHours < 2;

  return (
    <span className={`sla-tag ${isWarning ? 'sla-tag--warning' : 'sla-tag--normal'}`} title={`Due at ${new Date(dueDate).toLocaleString()}`}>
      <Clock size={13} />
      {diffHours > 0 ? `${diffHours}h ${diffMins}m left` : `${diffMins}m left`}
    </span>
  );
}
