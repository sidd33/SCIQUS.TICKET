export function getSlaStatus(ticket) {
  if (ticket.status === 4 || ticket.status === 5) {
    return { label: 'Resolved', isBreached: false, isUrgent: false };
  }

  if (ticket.isSlaBreached) {
    return { label: 'SLA Breached', isBreached: true, isUrgent: false };
  }

  if (!ticket.slaDeadline) {
    return { label: 'No SLA', isBreached: false, isUrgent: false };
  }

  // Backend sends UTC timestamps without a trailing 'Z' — force UTC parsing
  const deadlineStr = ticket.slaDeadline.endsWith('Z') ? ticket.slaDeadline : ticket.slaDeadline + 'Z';
  const deadline = new Date(deadlineStr).getTime();
  const now = Date.now();
  const diffMs = deadline - now;

  if (diffMs <= 0) {
    return { label: 'SLA Breached', isBreached: true, isUrgent: false };
  }

  const diffMins = Math.floor(diffMs / 60000);
  const hours = Math.floor(diffMins / 60);
  const mins = diffMins % 60;

  const label = hours > 0 ? `${hours}h ${mins}m left` : `${mins}m left`;
  const isUrgent = diffMins <= 30;

  return { label, isBreached: false, isUrgent };
}