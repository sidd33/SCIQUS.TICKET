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
  const [entitlement, setEntitlement] = useState(null);

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

  // Client-side validation errors
  const [validationErrors, setValidationErrors] = useState({});

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
console.table(
  prioritiesData.map(p => ({
    id: p.ticketPriorityId,
    name: p.name,
    slaInHours: p.slaInHours
  }))
);        console.log('IMPACTS:', impactsData);

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
 * Load current customer's support plan entitlement
 */
useEffect(() => {
  async function loadEntitlement() {
    if (!isPortal) {
      return;
    }

    try {
      const user = JSON.parse(
        localStorage.getItem('user') || 'null'
      );

      const accountId = user?.id;

      if (!accountId) {
        setEntitlement(null);
        return;
      }

      const res = await api.get(
        `/SupportPlan/account/${accountId}`
      );

      const plans = res.data || [];

      const activePlan = plans.find(
        plan =>
          String(plan.status || '').toLowerCase() === 'active'
      );

      if (!activePlan) {
        setEntitlement(null);
        return;
      }

      setEntitlement({
        planName: activePlan.planName,
        totalAllowed: activePlan.ticketQuota,
        usedCount: activePlan.consumedQuota,
        isBlocked:
          activePlan.ticketQuota > 0 &&
          activePlan.remainingQuota <= 0
      });
    } catch (err) {
      console.error(
        'Failed to load support plan entitlement:',
        err
      );

      setEntitlement(null);
    }
  }

  loadEntitlement();
}, [isPortal]);

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
   * Clear validation error for a specific field
   */
  const clearValidationError = (field) => {
    setValidationErrors(prev => {
      if (!prev[field]) {
        return prev;
      }

      const updated = { ...prev };
      delete updated[field];

      return updated;
    });
  };

  /*
   * Validate mandatory fields before submission
   */
  const validateForm = () => {
    const errors = {};

    if (!title.trim()) {
      errors.title =
        'Subject / Short Summary is required.';
    }

    if (!description.trim()) {
      errors.description =
        'Detailed Problem Description is required.';
    }

    if (!selectedTypeId) {
      errors.ticketType =
        'Please select a Ticket Type.';
    }

    if (!selectedSubTypeId) {
      errors.ticketSubType =
        'Please select an Issue Sub-Type.';
    }

    if (!selectedPriorityId) {
      errors.priority =
        'Please select a Priority Severity.';
    }

    if (!selectedImpactId) {
      errors.impact =
        'Please select a Business Impact.';
    }

    setValidationErrors(errors);

    return Object.keys(errors).length === 0;
  };

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

    // Clear previous server/API error
    setError('');

    /*
     * Validate form instead of silently returning.
     */
    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      /*
       * Ticket creation payload
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

  /*
   * Return error styling when a field is invalid
   */
  const getFieldStyle = (field) => {
    if (!validationErrors[field]) {
      return {};
    }

    return {
      border: '1px solid #ef4444',
      boxShadow:
        '0 0 0 1px rgba(239, 68, 68, 0.15)'
    };
  };

  /*
   * Reusable validation message
   */
  const ValidationMessage = ({ field }) => {
    if (!validationErrors[field]) {
      return null;
    }

    return (
      <div
        style={{
          color: '#f87171',
          fontSize: '0.75rem',
          marginTop: '0.35rem'
        }}
      >
        {validationErrors[field]}
      </div>
    );
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
            type="button"
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
  entitlement={entitlement}
/>

      {/* Validation Error Banner */}
      {Object.keys(validationErrors).length > 0 && (
        <div
          style={{
            padding: '0.85rem 1rem',
            background:
              'rgba(239,68,68,0.15)',
            border:
              '1px solid rgba(239,68,68,0.35)',
            borderRadius: '8px',
            color: '#f87171',
            marginBottom: '1.25rem',
            fontSize: '0.85rem'
          }}
        >
          <strong>
            Please complete all required fields.
          </strong>

          <div
            style={{
              marginTop: '0.4rem'
            }}
          >
            {Object.values(validationErrors).join(' ')}
          </div>
        </div>
      )}

      {/* Server/API Error */}
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

            {/* Ticket Type */}
            <div className="form-group">
              <label>
                Ticket Type *
              </label>

              <select
                required
                value={selectedTypeId}
                onChange={(e) => {
                  setSelectedTypeId(
                    e.target.value
                  );

                  clearValidationError(
                    'ticketType'
                  );

                  clearValidationError(
                    'ticketSubType'
                  );
                }}
                style={getFieldStyle(
                  'ticketType'
                )}
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

              <ValidationMessage
                field="ticketType"
              />
            </div>

            {/* Issue Sub-Type */}
            <div className="form-group">
              <label>
                Issue Sub-Type *
              </label>

              <select
                required
                value={selectedSubTypeId}
                onChange={(e) => {
                  setSelectedSubTypeId(
                    e.target.value
                  );

                  clearValidationError(
                    'ticketSubType'
                  );
                }}
                disabled={!selectedTypeId}
                style={getFieldStyle(
                  'ticketSubType'
                )}
              >
                <option value="">
                  Select Specific Issue...
                </option>

                {filteredSubTypes.map(st => (
                  <option
                    key={st.ticketSubTypeId}
                    value={st.ticketSubTypeId}
                  >
                    {st.name}
                  </option>
                ))}
              </select>

              <ValidationMessage
                field="ticketSubType"
              />
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

            {/* Priority */}
            <div className="form-group">
              <label>
                Priority Severity *
              </label>

              <select
                required
                value={selectedPriorityId}
                onChange={(e) => {
                  setSelectedPriorityId(
                    e.target.value
                  );

                  clearValidationError(
                    'priority'
                  );
                }}
                style={getFieldStyle(
                  'priority'
                )}
              >
                {priorities.length === 0 && (
                  <option value="">
                    Select Priority...
                  </option>
                )}

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

              <ValidationMessage
                field="priority"
              />
            </div>

            {/* Business Impact */}
            <div className="form-group">
              <label>
                Business Impact *
              </label>

              <select
                required
                value={selectedImpactId}
                onChange={(e) => {
                  setSelectedImpactId(
                    e.target.value
                  );

                  clearValidationError(
                    'impact'
                  );
                }}
                style={getFieldStyle(
                  'impact'
                )}
              >
                {impacts.length === 0 && (
                  <option value="">
                    Select Business Impact...
                  </option>
                )}

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

              <ValidationMessage
                field="impact"
              />
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
              onChange={(e) => {
                setTitle(e.target.value);

                clearValidationError(
                  'title'
                );
              }}
              style={getFieldStyle('title')}
            />

            <ValidationMessage
              field="title"
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
              onChange={(e) => {
                setDescription(
                  e.target.value
                );

                clearValidationError(
                  'description'
                );
              }}
              style={getFieldStyle(
                'description'
              )}
            />

            <ValidationMessage
              field="description"
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
              disabled={loading}
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