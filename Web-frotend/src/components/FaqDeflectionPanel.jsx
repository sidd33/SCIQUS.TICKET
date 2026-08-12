import React, { useState, useEffect } from 'react';
import { HelpCircle, ChevronRight, CheckCircle, ExternalLink } from 'lucide-react';
import './FaqDeflectionPanel.scss';

const MOCK_KNOWLEDGE_BASE = [
  { id: 1, title: 'How to reset your Corporate VPN Password', keywords: ['vpn', 'password', 'reset', 'network'], solution: 'Navigate to https://sso.sciqus.com/reset, enter your employee ID, and verify via 2FA authenticator.' },
  { id: 2, title: 'Configuring Outlook 365 Email on Mobile Devices', keywords: ['outlook', 'email', 'phone', 'mobile', 'mail'], solution: 'Download Microsoft Outlook app, log in using your work email address, and approve Microsoft Authenticator push notification.' },
  { id: 3, title: 'Requesting Second Monitor or Hardware Accessories', keywords: ['monitor', 'keyboard', 'mouse', 'hardware', 'laptop'], solution: 'Hardware requests must be submitted through your department head approval workflow before IT dispatch.' },
  { id: 4, title: 'Wi-Fi Connection Issues in Office', keywords: ['wifi', 'network', 'internet', 'connection'], solution: 'Connect to "SCIQUS-Corporate" Wi-Fi network using WPA2-Enterprise with your domain username & password.' }
];

export default function FaqDeflectionPanel({ query = '', onDeflect }) {
  const [matchingArticles, setMatchingArticles] = useState([]);
  const [selectedArticle, setSelectedArticle] = useState(null);

  useEffect(() => {
    if (!query || query.trim().length < 3) {
      setMatchingArticles([]);
      setSelectedArticle(null);
      return;
    }

    const lowerQuery = query.toLowerCase();
    const matches = MOCK_KNOWLEDGE_BASE.filter(article =>
      article.title.toLowerCase().includes(lowerQuery) ||
      article.keywords.some(k => lowerQuery.includes(k))
    );
    setMatchingArticles(matches);
  }, [query]);

  if (!query || query.trim().length < 3) {
    return (
      <div className="faq-deflection-panel empty">
        <HelpCircle size={22} className="deflection-icon" />
        <h4>Instant Solution Deflection</h4>
        <p>Type your ticket title above to see instant suggested solutions from our Knowledge Base.</p>
      </div>
    );
  }

  return (
    <div className="faq-deflection-panel">
      <div className="deflection-header">
        <HelpCircle size={18} />
        <h4>Suggested Knowledge Base Articles ({matchingArticles.length})</h4>
      </div>

      {matchingArticles.length === 0 ? (
        <p className="no-matches">No matching self-service articles found. Proceeding with ticket submission.</p>
      ) : (
        <div className="article-list">
          {matchingArticles.map(article => (
            <div key={article.id} className="article-card">
              <div className="article-title" onClick={() => setSelectedArticle(selectedArticle?.id === article.id ? null : article)}>
                <span>{article.title}</span>
                <ChevronRight size={16} className={`arrow ${selectedArticle?.id === article.id ? 'open' : ''}`} />
              </div>

              {selectedArticle?.id === article.id && (
                <div className="article-body">
                  <p>{article.solution}</p>
                  <button
                    type="button"
                    className="btn btn--success btn--sm"
                    onClick={() => {
                      if (onDeflect) onDeflect(article);
                      alert('Glad this solved your issue! Ticket submission cancelled.');
                    }}
                  >
                    <CheckCircle size={14} /> This Solved My Issue (Cancel Ticket)
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
