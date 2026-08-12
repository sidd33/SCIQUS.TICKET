import React, { useState, useEffect } from 'react';
import { Bell, Check, ExternalLink, Ticket } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../api/axios';
import './NotificationBell.scss';

export default function NotificationBell() {
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [showDropdown, setShowDropdown] = useState(false);

  const fetchNotifications = async () => {
    try {
      const res = await api.get('/notifications', { params: { unreadOnly: true, limit: 10 } });
      const items = res.data.items || res.data || [];
      setNotifications(items);
      setUnreadCount(items.filter(n => !n.isRead).length);
    } catch {
      // fallback
    }
  };

  useEffect(() => {
    fetchNotifications();
    const interval = setInterval(fetchNotifications, 30000);
    return () => clearInterval(interval);
  }, []);

  const handleMarkAsRead = async (id) => {
    try {
      await api.patch(`/notifications/${id}/read`);
      fetchNotifications();
    } catch {
      // ignore
    }
  };

  return (
    <div className="notification-bell-wrapper">
      <button
        className="bell-trigger"
        onClick={() => setShowDropdown(!showDropdown)}
        title="Notifications"
      >
        <Bell size={20} />
        {unreadCount > 0 && <span className="unread-dot">{unreadCount > 9 ? '9+' : unreadCount}</span>}
      </button>

      {showDropdown && (
        <div className="notifications-popover">
          <div className="popover-header">
            <h4>Notifications</h4>
            {unreadCount > 0 && <span className="badge-pill">{unreadCount} new</span>}
          </div>

          <div className="popover-body">
            {notifications.length === 0 ? (
              <div className="empty-state">No unread notifications</div>
            ) : (
              notifications.map(n => (
                <div key={n.id || n.notificationId} className={`notification-card ${!n.isRead ? 'unread' : ''}`}>
                  <Ticket size={16} className="notification-icon" />
                  <div className="notification-text">
                    <p>{n.message || n.eventType || 'New Ticket Update'}</p>
                    <small>{new Date(n.createdDate || Date.now()).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</small>
                  </div>
                  <div className="notification-actions">
                    {n.redirectUrl && (
                      <Link to={n.redirectUrl} onClick={() => setShowDropdown(false)}>
                        <ExternalLink size={14} />
                      </Link>
                    )}
                    {!n.isRead && (
                      <button onClick={() => handleMarkAsRead(n.id || n.notificationId)}>
                        <Check size={14} />
                      </button>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
