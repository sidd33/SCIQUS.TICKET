import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  PlusCircle,
  ArrowLeft,
  Send,
  Paperclip,
  X
} from 'lucide-react';
import api from '../../api/axios';
import EntitlementBanner from '../../components/EntitlementBanner';
import FaqDeflectionPanel from '../../components/FaqDeflectionPanel';

export default function CreateTicket({ isPortal = false }) {
  const navigate = useNavigate();

  const [types, setTypes] = useState([]);
  const [subTypes, setSubTypes] = useState([]);
  const [filteredSubTypes, setFilteredSubTypes] = useState([]);
  const [priorities, setPriorities] = useState([]);
  const [impacts, setImpacts] = useState([]);
  const [accounts, setAccounts] = useState([]);

  const [selectedTypeId, setSelectedTypeId] = useState('');
  const [selectedSubTypeId, setSelectedSubTypeId] = useState('');
  const [selectedPriorityId, setSelectedPriorityId] = useState('');
  const [selectedImpactId, setSelectedImpactId] = useState('');
  const [selectedAccountId, setSelectedAccountId] = useState('');

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  // Attachments
  const [attachments, setAttachments] = useState([]);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  /*
   * Load ticket master data
   */
  useEffect(() => {
    async function loadMasterData() {
      try {
        const [
          typesRes,
          subTypesRes,
          prioritiesRes,
          impactsRes
        ] = await Promise.all([
          api.get('/TicketTypes'),
          api.get('/TicketSubTypes'),
          api.get('/TicketPriorities'),
          api.get('/TicketBusinessImpacts')
        ]);

        const typesData =
          typesRes.data.items || typesRes.data || [];

        const subTypesData =
          subTypesRes.data.items || subTypesRes.data || [];

        const prioritiesData =
          prioritiesRes.data.items || prioritiesRes.data || [];

        const impactsData =
          impactsRes.data.items || impactsRes.data || [];

        console.log('TYPES:', typesData);
        console.log('SUBTYPES:', subTypesData);
        console.log('PRIORITIES:', prioritiesData);
        console.log('IMPACTS:', impactsData);

        setTypes(typesData);
        setSubTypes(subTypesData);
        setPriorities(prioritiesData);
        setImpacts(impactsData);

        /*
         * Select first priority by default
         */
        if (prioritiesData.length > 0) {
          const firstPriority = prioritiesData[0];

          setSelectedPriorityId(
            firstPriority.ticketPriorityId ||
            firstPriority.id
          );
        }

        /*
         * Select first business impact by default
         */
        if (impactsData.length > 0) {
          const firstImpact = impactsData[0];

          setSelectedImpactId(
            firstImpact.ticketBusinessTypeImpactId ||
            firstImpact.businessImpactId ||
            firstImpact.id
          );
        }
      } catch (err) {
        console.error('Failed to load master data:', err);

        setError(
          err.response?.data?.message ||
          'Failed to load master data.'
        );
      }
    }

    loadMasterData();
  }, []);

  /*
   * Filter sub-types whenever ticket type changes
   */
  useEffect(() => {
    if (!selectedTypeId) {
      setFilteredSubTypes([]);
      setSelectedSubTypeId('');
      return;
    }

    const filtered = subTypes.filter(
      st =>
        String(st.ticketTypeId) ===
        String(selectedTypeId)
    );

    console.log('Selected Type ID:', selectedTypeId);
    console.log('Filtered SubTypes:', filtered);

    setFilteredSubTypes(filtered);

    if (filtered.length > 0) {
      setSelectedSubTypeId(
        filtered[0].ticketSubTypeId
      );
    } else {
      setSelectedSubTypeId('');
    }
  }, [selectedTypeId, subTypes]);

  /*
   * Handle file selection
   */
  const handleAttachmentChange = (e) => {
    const selectedFiles = Array.from(
      e.target.files || []
    );

    if (selectedFiles.length === 0) {
      return;
    }

    setAttachments(prev => [
      ...prev,
      ...selectedFiles
    ]);

    // Allow selecting the same file again
    e.target.value = '';
  };

  /*
   * Remove selected attachment before submission
   */
  const removeAttachment = (index) => {
    setAttachments(prev =>
      prev.filter((_, i) => i !== index)
    );
  };

  /*
   * Create ticket and then upload attachments
   */
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (
      !title.trim() ||
      !description.trim() ||
      !selectedTypeId ||
      !selectedSubTypeId ||
      !selectedPriorityId ||
      !selectedImpactId
    ) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      /*
       * Ticket creation payload
       *
       * Your current CreateTicketRequest is still a JSON DTO,
       * so create the ticket first as JSON.
       */
      const payload = {
        title: title.trim(),
        description: description.trim(),
        ticketTypeId: selectedTypeId,
        ticketSubTypeId: selectedSubTypeId,
        priorityId: selectedPriorityId,
        businessImpactId: selectedImpactId,
        accountId: selectedAccountId || null,
        isInternal:
          !selectedAccountId && !isPortal,
        sourceType: isPortal
          ? 'Portal'
          : 'Internal'
      };

      /*
       * Create ticket
       */
      const res = await api.post(
        '/tickets',
        payload
      );

      const newTicketId =
        res.data.id ||
        res.data.ticketId ||
        res.data.ticket?.id ||
        res.data.ticket?.ticketId;

      if (!newTicketId) {
        throw new Error(
          'Ticket was created, but the ticket ID was not returned.'
        );
      }

      /*
       * Upload attachments after ticket creation.
       */
      if (attachments.length > 0) {
        for (const file of attachments) {
          const formData = new FormData();

          formData.append('file', file);

          await api.post(
            `/tickets/${newTicketId}/attachments`,
            formData,
            {
              headers: {
                'Content-Type': 'multipart/form-data'
              }
            }
          );
        }
      }

      /*
       * Navigate to the newly created ticket
       */
      navigate(
        isPortal
          ? `/portal/ticket/${newTicketId}`
          : `/tickets/${newTicketId}`
      );
    } catch (err) {
      console.error(
        'Failed to create ticket:',
        err
      );

      setError(
        err.response?.data?.message ||
        err.message ||
        'Failed to create ticket'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="tickets-page">

      {/* Header */}
      <div className="page-header">
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '1rem'
          }}
        >
          <button
            className="btn btn--secondary btn--sm"
            onClick={() => navigate(-1)}
          >
            <ArrowLeft size={16} />
            Back
          </button>

          <h1>Raise Support Ticket</h1>
        </div>
      </div>

      <EntitlementBanner
        entitlement={{
          planName: 'Silver Support Plan',
          totalAllowed: 50,
          usedCount: 12
        }}
      />

      {/* Error */}
      {error && (
        <div
          style={{
            padding: '0.75rem',
            background:
              'rgba(239,68,68,0.15)',
            border:
              '1px solid rgba(239,68,68,0.3)',
            borderRadius: '8px',
            color: '#f87171',
            marginBottom: '1.25rem',
            fontSize: '0.85rem'
          }}
        >
          {error}
        </div>
      )}

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: '2fr 1fr',
          gap: '1.5rem'
        }}
      >

        {/* Ticket form */}
        <form
          onSubmit={handleSubmit}
          className="glass-card"
          style={{
            padding: '1.5rem'
          }}
        >
          <h3
            style={{
              margin: '0 0 1.25rem 0',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              gap: '8px'
            }}
          >
            <PlusCircle size={18} />
            Ticket Details & Classification
          </h3>

          {/* Type + Sub Type */}
          <div
            style={{
              display: 'grid',
              gridTemplateColumns:
                '1fr 1fr',
              gap: '1rem'
            }}
          >

            <div className="form-group">
              <label>
                Ticket Type *
              </label>

              <select
                required
                value={selectedTypeId}
                onChange={(e) =>
                  setSelectedTypeId(
                    e.target.value
                  )
                }
              >
                <option value="">
                  Select Category...
                </option>

                {types.map(t => (
                  <option
                    key={t.ticketTypeId}
                    value={t.ticketTypeId}
                  >
                    {t.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>
                Issue Sub-Type *
              </label>

              <select
                required
                value={selectedSubTypeId}
                onChange={(e) =>
                  setSelectedSubTypeId(
                    e.target.value
                  )
                }
                disabled={!selectedTypeId}
              >
                <option value="">
                  Select Specific Issue...
                </option>

                {filteredSubTypes.map(st => (
                  <option
                    key={st.ticketSubTypeId}
                    value={
                      st.ticketSubTypeId
                    }
                  >
                    {st.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Priority + Impact */}
          <div
            style={{
              display: 'grid',
              gridTemplateColumns:
                '1fr 1fr',
              gap: '1rem'
            }}
          >

            <div className="form-group">
              <label>
                Priority Severity *
              </label>

              <select
                required
                value={
                  selectedPriorityId
                }
                onChange={(e) =>
                  setSelectedPriorityId(
                    e.target.value
                  )
                }
              >
                {priorities.map(p => (
                  <option
                    key={
                      p.ticketPriorityId
                    }
                    value={
                      p.ticketPriorityId
                    }
                  >
                    {p.name} (
                    {p.slaInHours}h SLA)
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>
                Business Impact *
              </label>

              <select
                required
                value={
                  selectedImpactId
                }
                onChange={(e) =>
                  setSelectedImpactId(
                    e.target.value
                  )
                }
              >
                {impacts.map(imp => (
                  <option
                    key={
                      imp.ticketBusinessTypeImpactId
                    }
                    value={
                      imp.ticketBusinessTypeImpactId
                    }
                  >
                    {imp.description ||
                      imp.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Customer Account */}
          {!isPortal && (
            <div className="form-group">
              <label>
                Customer Account
                {' '}
                (Optional - Leave blank
                for Internal Ticket)
              </label>

              <select
                value={
                  selectedAccountId
                }
                onChange={(e) =>
                  setSelectedAccountId(
                    e.target.value
                  )
                }
              >
                <option value="">
                  Internal Ticket
                  (No Customer Account)
                </option>

                {accounts.map(acc => (
                  <option
                    key={
                      acc.id ||
                      acc.accountId
                    }
                    value={
                      acc.id ||
                      acc.accountId
                    }
                  >
                    {acc.accountName ||
                      acc.name}
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Title */}
          <div className="form-group">
            <label>
              Subject / Short Summary *
            </label>

            <input
              required
              placeholder="E.g., Cannot connect to corporate VPN after password change..."
              value={title}
              onChange={(e) =>
                setTitle(e.target.value)
              }
            />
          </div>

          {/* Description */}
          <div className="form-group">
            <label>
              Detailed Problem Description *
            </label>

            <textarea
              required
              rows={5}
              placeholder="Provide exact error messages, steps to reproduce, or affected systems..."
              value={description}
              onChange={(e) =>
                setDescription(
                  e.target.value
                )
              }
            />
          </div>

          {/* Attachments */}
          <div className="form-group">
            <label
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '6px'
              }}
            >
              <Paperclip size={16} />
              Attachments
            </label>

            <input
              type="file"
              multiple
              onChange={
                handleAttachmentChange
              }
              style={{
                marginTop: '0.5rem'
              }}
            />

            {attachments.length > 0 && (
              <div
                style={{
                  marginTop: '0.75rem',
                  display: 'flex',
                  flexDirection:
                    'column',
                  gap: '0.5rem'
                }}
              >
                {attachments.map(
                  (file, index) => (
                    <div
                      key={`${file.name}-${index}`}
                      style={{
                        display: 'flex',
                        alignItems:
                          'center',
                        justifyContent:
                          'space-between',
                        padding:
                          '0.6rem 0.75rem',
                        border:
                          '1px solid rgba(255,255,255,0.1)',
                        borderRadius:
                          '6px'
                      }}
                    >
                      <span
                        style={{
                          overflow:
                            'hidden',
                          textOverflow:
                            'ellipsis',
                          whiteSpace:
                            'nowrap',
                          marginRight:
                            '0.75rem'
                        }}
                      >
                        {file.name}
                      </span>

                      <button
                        type="button"
                        onClick={() =>
                          removeAttachment(
                            index
                          )
                        }
                        style={{
                          background:
                            'transparent',
                          border: 'none',
                          cursor:
                            'pointer',
                          color:
                            '#f87171',
                          display:
                            'flex',
                          alignItems:
                            'center'
                        }}
                        title="Remove attachment"
                      >
                        <X size={16} />
                      </button>
                    </div>
                  )
                )}
              </div>
            )}
          </div>

          {/* Buttons */}
          <div
            style={{
              display: 'flex',
              justifyContent:
                'flex-end',
              gap: '0.75rem',
              marginTop: '1.5rem'
            }}
          >
            <button
              type="button"
              className="btn btn--secondary"
              onClick={() =>
                navigate(-1)
              }
            >
              Cancel
            </button>

            <button
              type="submit"
              className="btn btn--primary"
              disabled={
                loading ||
                !title.trim() ||
                !description.trim() ||
                !selectedTypeId ||
                !selectedSubTypeId ||
                !selectedPriorityId ||
                !selectedImpactId
              }
            >
              <Send size={16} />

              {loading
                ? 'Submitting...'
                : 'Submit Ticket'}
            </button>
          </div>
        </form>

        {/* FAQ */}
        <div>
          <FaqDeflectionPanel
            query={title}
            onDeflect={() =>
              navigate(-1)
            }
          />
        </div>
      </div>
    </div>
  );
}
