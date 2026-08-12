import React, { useState, useEffect } from 'react';
import { HelpCircle, ChevronRight, CheckCircle } from 'lucide-react';
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
      <div className="faq-panel empty glass-card">
        <HelpCircle size={24} className="panel-icon" />
        <h4>Instant Knowledge Base Solution</h4>
        <p>Start typing your ticket subject above to view instant self-service articles before submitting.</p>
      </div>
    );
  }

  return (
    <div className="faq-panel glass-card">
      <div className="panel-header">
        <HelpCircle size={18} />
        <h4>Suggested Solution Articles ({matchingArticles.length})</h4>
      </div>

      {matchingArticles.length === 0 ? (
        <p className="no-articles">No matching self-service articles found. Proceed with ticket creation.</p>
      ) : (
        <div className="articles-list">
          {matchingArticles.map(article => (
            <div key={article.id} className="article-item">
              <div className="item-title" onClick={() => setSelectedArticle(selectedArticle?.id === article.id ? null : article)}>
                <span>{article.title}</span>
                <ChevronRight size={16} className={`arrow ${selectedArticle?.id === article.id ? 'open' : ''}`} />
              </div>

              {selectedArticle?.id === article.id && (
                <div className="item-content">
                  <p>{article.solution}</p>
                  <button
                    type="button"
                    className="btn btn--success btn--sm"
                    onClick={() => {
                      if (onDeflect) onDeflect(article);
                      alert('Glad this solved your issue! Ticket creation cancelled.');
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
