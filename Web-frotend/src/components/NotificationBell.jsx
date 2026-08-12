import React, { useState, useEffect } from 'react';
import { Bell, Check, ExternalLink } from 'lucide-react';
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
    <div className="notification-bell-container">
      <button
        className="bell-btn"
        onClick={() => setShowDropdown(!showDropdown)}
        title="Notifications"
      >
        <Bell size={20} />
        {unreadCount > 0 && <span className="bell-badge">{unreadCount > 9 ? '9+' : unreadCount}</span>}
      </button>

      {showDropdown && (
        <div className="notification-dropdown">
          <div className="dropdown-header">
            <h4>Notifications</h4>
            {unreadCount > 0 && (
              <span className="unread-pill">{unreadCount} unread</span>
            )}
          </div>

          <div className="dropdown-body">
            {notifications.length === 0 ? (
              <div className="empty-notifications">No new notifications</div>
            ) : (
              notifications.map(n => (
                <div key={n.id || n.notificationId} className={`notification-item ${!n.isRead ? 'unread' : ''}`}>
                  <div className="notification-content">
                    <p className="notification-text">{n.message || n.eventType || 'New Ticket Update'}</p>
                    <span className="notification-time">
                      {new Date(n.createdDate || Date.now()).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </span>
                  </div>

                  <div className="notification-actions">
                    {n.redirectUrl && (
                      <Link to={n.redirectUrl} className="action-btn" onClick={() => setShowDropdown(false)}>
                        <ExternalLink size={14} />
                      </Link>
                    )}
                    {!n.isRead && (
                      <button className="action-btn" onClick={() => handleMarkAsRead(n.id || n.notificationId)} title="Mark as read">
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
