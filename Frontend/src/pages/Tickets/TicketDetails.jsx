import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  UserCheck,
  ArrowRightLeft,
  ShieldAlert,
  CheckCircle2,
  RotateCcw,
  MessageSquare,
  Paperclip,
  History,
  Mail,
  Send,
  ChevronDown,
  Globe,
  MessageCircle
} from 'lucide-react';
import api from '../../api/axios';
import SlaBadge from '../../components/SlaBadge';
import AcceptanceBar from '../../components/AcceptanceBar';
import ConfirmationBanner from '../../components/ConfirmationBanner';
import ActivityTimeline from '../../components/ActivityTimeline';
import ReassignModal from '../../components/ReassignModal';
import TransferModal from '../../components/TransferModal';
import PriorityImpactModal from '../../components/PriorityImpactModal';
import './Tickets.scss';

const ASSIGNMENT_TYPE_COLORS = {
  Automatic: { text: '#5eead4', bg: 'rgba(94, 234, 212, 0.12)' },
  Manual: { text: '#fbbf24', bg: 'rgba(251, 191, 36, 0.12)' },
  Queue: { text: '#94a3b8', bg: 'rgba(148, 163, 184, 0.12)' },
};

function AssignmentReasonPanel({ assignmentReason }) {
  const [expanded, setExpanded] = useState(false);
  const [showCandidates, setShowCandidates] = useState(false);

  if (!assignmentReason) return null;

  const colors = ASSIGNMENT_TYPE_COLORS[assignmentReason.assignmentType] || ASSIGNMENT_TYPE_COLORS.Queue;
  const hasCandidates = (assignmentReason.candidates?.length ?? 0) > 0;

  return (
    <div>
      <span style={{ color: 'var(--text-muted)' }}>Why assigned:</span>

      <div
        style={{
          marginTop: '6px',
          padding: '0.6rem 0.75rem',
          background: 'rgba(99, 102, 241, 0.08)',
          border: '1px solid var(--bg-card-border)',
          borderRadius: '8px',
          fontSize: '0.8rem'
        }}
      >
        <div
          onClick={() => setExpanded(e => !e)}
          style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', cursor: 'pointer', gap: '8px' }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', minWidth: 0 }}>
            <span
  style={{
    display: 'inline-block',
    padding: '2px 8px',
    borderRadius: '999px',
    fontSize: '0.65rem',
    fontWeight: 600,
    textTransform: 'uppercase',
    letterSpacing: '0.03em',
    color: colors.text,
    background: colors.bg,
    flexShrink: 0
  }}
>
  {assignmentReason.assignmentType}
  {assignmentReason.algorithmName ? ` · ${assignmentReason.algorithmName}` : ''}
</span>
            <span style={{ color: 'white', fontSize: '0.78rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {assignmentReason.shortReason || assignmentReason.reason}
            </span>
          </div>
          <ChevronDown
            size={13}
            style={{ transform: expanded ? 'rotate(180deg)' : 'none', transition: 'transform 0.15s', flexShrink: 0, color: 'var(--text-dim)' }}
          />
        </div>

        {expanded && (
          <div style={{ marginTop: '8px', paddingTop: '8px', borderTop: '1px solid var(--bg-card-border)' }}>
            <div style={{ color: 'white', lineHeight: '1.5' }}>
              {assignmentReason.reason}
            </div>

            {assignmentReason.context && (
              <div style={{ marginTop: '6px', color: 'var(--text-dim)', fontSize: '0.72rem', fontStyle: 'italic' }}>
                {assignmentReason.context}
              </div>
            )}

            {hasCandidates && (
              <div style={{ marginTop: '0.6rem', borderTop: '1px solid var(--bg-card-border)', paddingTop: '0.5rem' }}>
                <button
                  onClick={() => setShowCandidates(s => !s)}
                  style={{ display: 'flex', alignItems: 'center', gap: '4px', background: 'none', border: 'none', color: '#818cf8', fontSize: '0.75rem', fontWeight: 500, cursor: 'pointer', padding: 0 }}
                >
                  Why this employee over others ({assignmentReason.candidates.length} eligible)
                  <ChevronDown size={12} style={{ transform: showCandidates ? 'rotate(180deg)' : 'none', transition: 'transform 0.15s' }} />
                </button>

                {showCandidates && (
                  <table style={{ width: '100%', marginTop: '0.5rem', borderCollapse: 'collapse', fontSize: '0.75rem' }}>
                    <thead>
                      <tr style={{ color: 'var(--text-dim)', textAlign: 'left' }}>
                        <th style={{ paddingBottom: '4px', fontWeight: 500 }}>Employee</th>
                        <th style={{ paddingBottom: '4px', fontWeight: 500, textAlign: 'right' }}>Score</th>
                        <th style={{ paddingBottom: '4px', fontWeight: 500, textAlign: 'right' }}>Open</th>
                        <th style={{ paddingBottom: '4px', fontWeight: 500, textAlign: 'right' }}>Severity</th>
                        <th style={{ paddingBottom: '4px', fontWeight: 500, textAlign: 'right' }}>Idle (h)</th>
                      </tr>
                    </thead>
                    <tbody>
                      {assignmentReason.candidates.map((c) => (
                        <tr key={c.employeeId} style={{ borderTop: '1px solid var(--bg-card-border)', background: c.selected ? 'rgba(94, 234, 212, 0.06)' : 'transparent' }}>
                          <td style={{ padding: '4px 0', color: c.selected ? 'white' : 'var(--text-dim)', fontWeight: c.selected ? 600 : 400 }}>
                            {c.selected && <CheckCircle2 size={11} style={{ marginRight: '4px', verticalAlign: '-2px', color: '#5eead4' }} />}
                            {c.employeeName}
                          </td>
                          <td style={{ padding: '4px 0', textAlign: 'right', color: 'var(--text-dim)' }}>{c.score.toFixed(2)}</td>
                          <td style={{ padding: '4px 0', textAlign: 'right', color: 'var(--text-dim)' }}>{c.openTicketCount}</td>
                          <td style={{ padding: '4px 0', textAlign: 'right', color: 'var(--text-dim)' }}>{c.severityLoad}</td>
                          <td style={{ padding: '4px 0', textAlign: 'right', color: 'var(--text-dim)' }}>{c.hoursSinceLastAssignment.toFixed(1)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

function getSourceInfo(sourceType) {
  switch (sourceType?.toLowerCase()) {
    case 'email':
      return {
        label: 'Email',
        icon: Mail
      };

    case 'whatsapp':
      return {
        label: 'WhatsApp',
        icon: MessageCircle
      };

    case 'internal':
      return {
        label: 'Internal',
        icon: UserCheck
      };

    case 'portal':
    default:
      return {
        label: 'Portal',
        icon: Globe
      };
  }
}

export default function TicketDetails() {
  const { ticketId } = useParams();
  const navigate = useNavigate();

  const [ticket, setTicket] = useState(null);
  const [comments, setComments] = useState([]);
  const [histories, setHistories] = useState([]);
  const [assignmentReason, setAssignmentReason] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('details');

  const [showReassignModal, setShowReassignModal] = useState(false);
  const [showTransferModal, setShowTransferModal] = useState(false);
  const [showPriorityModal, setShowPriorityModal] = useState(false);

  const [newComment, setNewComment] = useState('');
  const [isInternalComment, setIsInternalComment] = useState(false);
  const [submittingComment, setSubmittingComment] = useState(false);

  const storedUser = JSON.parse(localStorage.getItem('user') || 'null');
  const currentUserId = storedUser?.id;

  useEffect(() => {
    loadTicketDetails();
  }, [ticketId]);

  async function loadTicketDetails() {
    setLoading(true);

    try {
      // Load the ticket itself first
      const tRes = await api.get(`/tickets/${ticketId}`);
      setTicket(tRes.data);

      // Load comments separately
      try {
        const cRes = await api.get(`/tickets/${ticketId}/comments`);
        setComments(cRes.data?.items || cRes.data || []);
      } catch (err) {
        console.error('Failed to load comments:', err);
        setComments([]);
      }

      // Load timeline separately
      try {
        const hRes = await api.get(`/tickets/${ticketId}/timeline`);
        setHistories(hRes.data?.items || hRes.data || []);
      } catch (err) {
        console.error('Failed to load ticket timeline:', err);
        setHistories([]);
      }

      // Load assignment reason separately
      try {
        const arRes = await api.get(`/tickets/${ticketId}/assignment-reason`);
        setAssignmentReason(arRes.data);
      } catch (err) {
        console.error('Failed to load assignment reason:', err);
        setAssignmentReason(null);
      }

    } catch (err) {
      console.error('Failed to load ticket:', err);
      setTicket(null);
    } finally {
      setLoading(false);
    }
  }

  const handlePostComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim()) return;

    setSubmittingComment(true);
    try {
      await api.post(`/tickets/${ticketId}/comments`, {
        commentText: newComment,
        isInternal: isInternalComment
      });
      setNewComment('');
      loadTicketDetails();
    } catch {
      alert('Failed to post comment');
    } finally {
      setSubmittingComment(false);
    }
  };

  const handleStatusTransition = async (statusName) => {
    try {
      const statusIds = {
        'Open': '10000000-0000-0000-0000-000000000001',
        'In Progress': '10000000-0000-0000-0000-000000000002',
        'Pending': '10000000-0000-0000-0000-000000000003',
        'Resolved': '10000000-0000-0000-0000-000000000004',
        'Closed': '10000000-0000-0000-0000-000000000005',
        'PendingClosure': '10000000-0000-0000-0000-000000000006',
        'Reopened': '10000000-0000-0000-0000-000000000007'
      };

      await api.patch(`/tickets/${ticketId}/status`, {
        statusId: statusIds[statusName]
      });

      await loadTicketDetails();
    } catch (err) {
      console.error('Failed to update status:', err.response?.data || err);
      alert(err.response?.data?.message || 'Failed to update status');
    }
  };

  if (loading) {
    return <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-dim)' }}>Loading ticket details...</div>;
  }

  if (!ticket) {
    return <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-dim)' }}>Ticket not found.</div>;
  }

  return (
    <div className="tickets-page">
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <button className="btn btn--secondary btn--sm" onClick={() => navigate(-1)}>
            <ArrowLeft size={16} /> Back
          </button>
          <div>
            <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
              <span>{ticket.ticketNumber || `TKT-${ticket.id?.substring(0, 6)}`}</span>
              <span
                className={`badge badge--${
                  ticket.acceptanceStatus === 'Pending'
                    ? 'awaiting-acceptance'
                    : (ticket.statusName || 'Open').toLowerCase().replace(' ', '')
                }`}
              >
                {ticket.acceptanceStatus === 'Pending'
                  ? 'Awaiting Acceptance'
                  : ticket.statusName || 'Open'}
              </span>
            </h1>
            <p style={{ margin: '2px 0 0 0', fontSize: '0.9rem', color: 'white' }}>{ticket.title}</p>
          </div>
        </div>

        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
          <button
            className="btn btn--secondary btn--sm"
            onClick={() => setShowReassignModal(true)}
          >
            <UserCheck size={14} /> Assign / Reassign
          </button>

          <button
            className="btn btn--secondary btn--sm"
            onClick={() => setShowTransferModal(true)}
          >
            <ArrowRightLeft size={14} /> Transfer Dept
          </button>

          <button
            className="btn btn--secondary btn--sm"
            onClick={() => setShowPriorityModal(true)}
          >
            <ShieldAlert size={14} /> Priority / SLA
          </button>

          {/* Request Closure */}
          {(
            ticket.statusName === 'Open' ||
            ticket.statusName === 'In Progress' ||
            ticket.statusName === 'Pending' ||
            ticket.statusName === 'Resolved'
          ) && (
            <button
              className="btn btn--primary btn--sm"
              onClick={() => handleStatusTransition('PendingClosure')}
            >
              <CheckCircle2 size={14} /> Request Closure
            </button>
          )}

          {/* Reopen */}
          {ticket.statusName === 'Closed' && (
            <button
              className="btn btn--secondary btn--sm"
              onClick={async () => {
                try {
                  const reason = window.prompt('Why are you reopening this ticket?');

                  if (!reason?.trim()) return;

                  await api.post(
                    `/tickets/${ticketId}/reopen`,
                    reason.trim()
                  );

                  await loadTicketDetails();
                } catch (err) {
                  console.error(
                    'Failed to reopen ticket:',
                    err.response?.data || err
                  );

                  alert(
                    err.response?.data?.message ||
                    'Failed to reopen ticket'
                  );
                }
              }}
            >
              <RotateCcw size={14} /> Reopen
            </button>
          )}
        </div>
      </div>

      {ticket.acceptanceStatus === 'Pending' && (
        <AcceptanceBar
          ticketId={ticket.id || ticketId}
          deadline={ticket.acceptanceDeadlineAt}
          assignedToUserId={ticket.assignedToUserId}
          currentUserId={currentUserId}
          onAction={loadTicketDetails}
        />
      )}

      {ticket.statusName === 'PendingClosure' && (
        <ConfirmationBanner ticketId={ticket.id || ticketId} onAction={loadTicketDetails} />
      )}

      {/* Main Workspace Card */}
      <div className="glass-card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
        {/* Workspace Nav Tabs */}
        <div style={{ display: 'flex', gap: '0.5rem', borderBottom: '1px solid var(--bg-card-border)', paddingBottom: '0.75rem', marginBottom: '1.5rem' }}>
          <button className={`btn btn--sm ${activeTab === 'details' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('details')}>
            Ticket Overview
          </button>
          <button className={`btn btn--sm ${activeTab === 'comments' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('comments')}>
            <MessageSquare size={14} /> Comments & Notes ({comments.length})
          </button>
          <button className={`btn btn--sm ${activeTab === 'timeline' ? 'btn--primary' : 'btn--secondary'}`} onClick={() => setActiveTab('timeline')}>
            <History size={14} /> Audit History ({histories.length})
          </button>
        </div>

        {activeTab === 'details' && (
          <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: '1.5rem' }}>
            <div>
              <h4 style={{ color: 'var(--text-muted)', marginBottom: '0.5rem', fontSize: '0.85rem' }}>Problem Description</h4>
              <div style={{ background: 'rgba(0,0,0,0.3)', padding: '1rem', borderRadius: '8px', border: '1px solid var(--bg-card-border)', color: 'white', lineHeight: '1.6', fontSize: '0.9rem', minHeight: '120px' }}>
                {ticket.description}
              </div>
            </div>

            <div className="glass-card" style={{ padding: '1rem', background: 'rgba(15, 23, 42, 0.4)' }}>
              <h4 style={{ color: 'white', marginBottom: '1rem', fontSize: '0.9rem' }}>Ticket Attributes</h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', fontSize: '0.825rem' }}>
                              {(() => {
  const source = getSourceInfo(ticket.sourceType);
  const SourceIcon = source.icon;

  return (
    <div>
      <span style={{ color: 'var(--text-muted)' }}>Source:</span>

      <strong
        style={{
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          gap: '6px',
          marginTop: '4px'
        }}
      >
        <SourceIcon size={15} />
        {source.label}
      </strong>
    </div>
  );
})()}

{ticket.sourceMessageId && (
  <div>
    <span style={{ color: 'var(--text-muted)' }}>Source Message ID:</span>

    <strong
      style={{
        color: 'white',
        display: 'block',
        marginTop: '4px',
        wordBreak: 'break-all'
      }}
    >
      {ticket.sourceMessageId}
    </strong>
  </div>
)}

{ticket.emailReceivedDate && (
  <div>
    <span style={{ color: 'var(--text-muted)' }}>Received:</span>

    <strong
      style={{
        color: 'white',
        display: 'block',
        marginTop: '4px'
      }}
    >
      {new Date(ticket.emailReceivedDate).toLocaleString()}
    </strong>
  </div>
)}
                <div><span style={{ color: 'var(--text-muted)' }}>Department:</span> <strong style={{ color: 'white', display: 'block' }}>{ticket.departmentName || 'IT Support'}</strong></div>
                <div><span style={{ color: 'var(--text-muted)' }}>Assigned Agent:</span> <strong style={{ color: 'white', display: 'block' }}>{ticket.assignedToUserName || 'Unassigned'}</strong></div>

               <AssignmentReasonPanel assignmentReason={assignmentReason} />

                <div><span style={{ color: 'var(--text-muted)' }}>Priority Severity:</span> <strong style={{ color: '#fbbf24', display: 'block' }}>{ticket.priorityName || 'Medium'}</strong></div>
                <div><span style={{ color: 'var(--text-muted)' }}>Business Impact:</span> <strong style={{ color: 'white', display: 'block' }}>{ticket.businessImpactName || 'Single User'}</strong></div>

                <div><span style={{ color: 'var(--text-muted)' }}>SLA Countdown:</span> <div style={{ marginTop: '4px' }}><SlaBadge dueDate={ticket.slaDueDate} isBreached={ticket.isSlaBreached} statusName={ticket.statusName} /></div></div>

                <div>
                  <span style={{ color: 'var(--text-muted)' }}>Assignment Log:</span>

                  <div
                    style={{
                      marginTop: '6px',
                      padding: '0.75rem',
                      background: 'rgba(99, 102, 241, 0.08)',
                      border: '1px solid var(--bg-card-border)',
                      borderRadius: '8px',
                      fontSize: '0.8rem'
                    }}
                  >
                    {histories.filter(h =>
                      (h.changeDescription || h.description || '')
                        .toLowerCase()
                        .includes('assign')
                    ).length === 0 ? (
                      <span style={{ color: 'var(--text-dim)' }}>
                        No assignment history available
                      </span>
                    ) : (
                      histories
                        .filter(h =>
                          (h.changeDescription || h.description || '')
                            .toLowerCase()
                            .includes('assign')
                        )
                        .map((h, index) => (
                          <div
                            key={h.id || h.ticketHistoryId || index}
                            style={{
                              marginBottom: index === 0 ? '0' : '0.6rem',
                              paddingBottom: index === 0 ? '0' : '0.6rem',
                              borderBottom:
                                index === 0 ? 'none' : '1px solid var(--bg-card-border)'
                            }}
                          >
                            <div style={{ color: 'white' }}>
                              ✓ {h.changeDescription || h.description}
                            </div>

                            <div
                              style={{
                                color: 'var(--text-dim)',
                                marginTop: '3px',
                                fontSize: '0.75rem'
                              }}
                            >
                              {h.createdDate
                                ? new Date(h.createdDate).toLocaleString()
                                : ''}
                            </div>
                          </div>
                        ))
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'comments' && (
          <div>
            <form onSubmit={handlePostComment} style={{ marginBottom: '1.5rem' }}>
              <div className="form-group">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <label>Add Response / Comment</label>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '6px', cursor: 'pointer', color: '#818cf8', fontSize: '0.8rem' }}>
                    <input type="checkbox" checked={isInternalComment} onChange={(e) => setIsInternalComment(e.target.checked)} />
                    Internal Staff Note Only
                  </label>
                </div>
                <textarea rows={3} placeholder="Type your message here..." value={newComment} onChange={(e) => setNewComment(e.target.value)} required />
              </div>
              <button type="submit" className="btn btn--primary btn--sm" disabled={submittingComment || !newComment.trim()}>
                <Send size={14} /> Post Response
              </button>
            </form>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {comments.length === 0 ? (
                <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem' }}>No comments recorded on this ticket yet.</p>
              ) : (
                comments.map(c => (
                  <div key={c.id || c.commentId} style={{ background: c.isInternal ? 'rgba(99, 102, 241, 0.08)' : 'rgba(15, 23, 42, 0.4)', border: '1px solid var(--bg-card-border)', borderRadius: '8px', padding: '1rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px', fontSize: '0.8rem' }}>
                      <strong style={{ color: 'white' }}>{c.authorName || 'Staff Member'}</strong>
                      <span style={{ color: 'var(--text-dim)' }}>{new Date(c.createdDate || Date.now()).toLocaleString()}</span>
                    </div>
                    <p style={{ color: 'var(--text-main)', fontSize: '0.85rem', margin: 0 }}>{c.commentText || c.text}</p>
                  </div>
                ))
              )}
            </div>
          </div>
        )}

        {activeTab === 'timeline' && (
          <ActivityTimeline histories={histories} />
        )}
      </div>

      {showReassignModal && <ReassignModal ticketId={ticket.id || ticketId} currentAssigneeId={ticket.assignedToUserId} onClose={() => setShowReassignModal(false)} onSuccess={loadTicketDetails} />}
      {showTransferModal && <TransferModal ticketId={ticket.id || ticketId} onClose={() => setShowTransferModal(false)} onSuccess={loadTicketDetails} />}
      {showPriorityModal && <PriorityImpactModal ticketId={ticket.id || ticketId} currentPriorityId={ticket.priorityId} currentImpactId={ticket.businessImpactId} onClose={() => setShowPriorityModal(false)} onSuccess={loadTicketDetails} />}
    </div>
  );
}