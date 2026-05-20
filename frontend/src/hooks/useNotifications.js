/**
 * useNotifications.js - Notifications hook
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Implement useNotifications hook for notification management
 * - Use notificationService to fetch notifications
 * - REST unread count first (GET /notifications/unread-count)
 * - SignalR optional future enhancement
 * - No real API calls in this task
 * 
 * IMPORTANT RULES:
 * - Start with REST API only
 * - SignalR is optional future enhancement
 * - No mock data, no fake counts
 */

import { useCallback, useEffect, useState } from 'react';
import {
  getNotifications,
  getUnreadCount,
  markAllNotificationsRead,
  markNotificationRead,
} from '../services/notificationService.js';
import { toNotificationListViewModel, toNotificationViewModel } from '../adapters/notificationAdapter.js';

export function useNotifications() {
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  const fetchUnreadCount = useCallback(async () => {
    const count = await getUnreadCount();
    setUnreadCount(Number.isFinite(count) ? count : 0);
    return count;
  }, []);

  const fetchNotifications = useCallback(async () => {
    setIsLoading(true);
    try {
      const items = await getNotifications();
      const mapped = toNotificationListViewModel(items);
      setNotifications(mapped);
      return mapped;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const markAsRead = useCallback(async (notificationId) => {
    const updated = await markNotificationRead(notificationId);
    const mapped = toNotificationViewModel(updated);
    setNotifications((prev) => prev.map((item) => (item.id === notificationId ? { ...item, ...(mapped || {}), isRead: true } : item)));
    setUnreadCount((prev) => Math.max(prev - 1, 0));
  }, []);

  const markAllAsRead = useCallback(async () => {
    await markAllNotificationsRead();
    setNotifications((prev) => prev.map((item) => ({ ...item, isRead: true })));
    setUnreadCount(0);
  }, []);

  useEffect(() => {
    fetchUnreadCount().catch(() => setUnreadCount(0));
    fetchNotifications().catch(() => setNotifications([]));
  }, [fetchNotifications, fetchUnreadCount]);

  return {
    unreadCount,
    notifications,
    isLoading,
    fetchUnreadCount,
    fetchNotifications,
    markAsRead,
    markAllAsRead,
  };
}
