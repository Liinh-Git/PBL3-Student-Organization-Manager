/**
 * OrgEventsPage.jsx - Organization events page
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  getOrganizationEvents,
  createEvent,
} from "../../services/eventService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgEventsPage.css";

function OrgEventsPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const orgId = searchParams.get("orgId");
  const { permissions, isMember } = useOrgContext();

  const [events, setEvents] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadEvents() {
      setIsLoading(true);
      try {
        const data = await getOrganizationEvents(orgId);
        setEvents(data);
      } catch (err) {
        setError(err.message || "Tải danh sách sự kiện thất bại");
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, [orgId, isMember]);

  if (!orgId)
    return <ErrorState message="Cần có ID tổ chức để xem trang này" />;
  if (!isMember)
    return (
      <ForbiddenState message="Bạn không phải thành viên của tổ chức này" />
    );
  if (isLoading)
    return <LoadingSpinner message="Đang tải danh sách sự kiện..." />;
  if (error) return <ErrorState message={error} />;

  const canCreate = permissions.includes("org.events.create");

  const getEventId = (event) => event?.id || event?.eventId;
  const getEventName = (event) => event?.name || event?.eventName;

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!canCreate) return;

    const form = e.target;
    const eventName = form.eventName.value;
    const description = form.description.value;
    const startDate = form.startDate.value;
    const startTime = form.startTime.value || "00:00";
    const location = form.location.value;
    const targetParticipants = form.targetParticipants?.value;
    const visibility = form.visibility.value;

    setIsSubmitting(true);
    try {
      // Logic cũ: ghép date và time thành ISO string
      const fullStartDate = `${startDate}T${startTime}:00Z`;

      const newEvent = await createEvent(orgId, {
        eventName: eventName,
        description: description || undefined,
        startDate: fullStartDate,
        location: location || undefined,
        targetParticipants: targetParticipants
          ? parseInt(targetParticipants)
          : undefined,
        visibility: visibility,
      });

      setEvents((prev) => [...prev, newEvent]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || "Tạo sự kiện thất bại");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="event-page-container">
      {/* Header */}
      <div className="event-header">
        <h1>Quản lý sự kiện</h1>
      </div>

      <div className="event-toolbar">
        <div className="filter-pill active">Tất cả</div>
        <div className="filter-pill">Sắp tới</div>
        <div className="filter-pill">Đã kết thúc</div>
      </div>

      <div className="event-grid">
        {/* Card Tạo sự kiện mới */}
        {canCreate && (
          <div
            className="event-create-card"
            onClick={() => setShowCreateForm(true)}
          >
            <div className="create-icon-circle">
              <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
              >
                <line x1="12" y1="5" x2="12" y2="19" />
                <line x1="5" y1="12" x2="19" y2="12" />
              </svg>
            </div>
            <h3
              style={{
                fontSize: "1.1rem",
                fontWeight: 800,
                margin: "0 0 8px 0",
                color: "var(--ink-900)",
              }}
            >
              Tạo sự kiện mới
            </h3>
            <p
              style={{
                fontSize: "0.85rem",
                color: "var(--ink-600)",
                margin: 0,
              }}
            >
              Lên lịch chiến dịch, cuộc họp hoặc hội thảo cho tổ chức.
            </p>
          </div>
        )}

        {/* Danh sách thẻ sự kiện */}
        {events.length === 0 && !canCreate ? (
          <EmptyState message="Chưa có sự kiện nào được tạo." />
        ) : (
          events.map((event) => {
            const eventId = getEventId(event);
            return (
              <div
                key={eventId}
                className="event-card"
                onClick={() =>
                  navigate(`/org/events/${eventId}?orgId=${orgId}`)
                }
              >
                <div className="event-banner">
                  <span className="status-tag">
                    {event.status || "Đang diễn ra"}
                  </span>
                  {event.bannerUrl || event.coverUrl ? (
                    <img
                      src={event.bannerUrl || event.coverUrl}
                      alt="Event Banner"
                    />
                  ) : (
                    <div
                      style={{
                        width: "100%",
                        height: "100%",
                        background:
                          "linear-gradient(135deg, var(--brand-700) 0%, var(--brand-500) 100%)",
                      }}
                    ></div>
                  )}
                </div>

                <div className="event-content">
                  <h3 className="event-title">
                    {getEventName(event) || "Sự kiện không tên"}
                  </h3>

                  <div className="event-info-row">
                    <svg
                      width="16"
                      height="16"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <circle cx="12" cy="12" r="10" />
                      <polyline points="12 6 12 12 16 14" />
                    </svg>
                    {event.startDate
                      ? new Date(event.startDate).toLocaleString("vi-VN", {
                          dateStyle: "short",
                          timeStyle: "short",
                        })
                      : "Chưa xác định thời gian"}
                  </div>

                  <div className="event-info-row">
                    <svg
                      width="16"
                      height="16"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                      <circle cx="12" cy="10" r="3" />
                    </svg>
                    {event.location || "Chưa xác định địa điểm"}
                  </div>

                  <div className="event-footer">
                    <div className="participant-stat">
                      {event.targetParticipants || 0}{" "}
                      <span
                        style={{ color: "var(--ink-600)", fontWeight: 500 }}
                      >
                        Người tham gia
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Modal Thêm sự kiện mới đồng bộ */}
      {showCreateForm && canCreate && (
        <div
          className="org-modal-overlay"
          onClick={() => setShowCreateForm(false)}
        >
          <div className="org-modal" onClick={(e) => e.stopPropagation()}>
            <div className="org-modal-header">
              <h3>Tạo sự kiện mới</h3>
              <p>
                Điền các thông tin cơ bản để bắt đầu lên kế hoạch cho sự kiện.
              </p>
            </div>

            <div className="org-modal-body">
              <form id="createEventForm" onSubmit={handleCreate}>
                <div className="form-grid">
                  <div className="form-group" style={{ gridColumn: "1 / -1" }}>
                    <label className="form-label">Tên sự kiện *</label>
                    <input
                      id="eventName"
                      name="eventName"
                      className="org-input"
                      placeholder="Ví dụ: Ngày Hội Xanh 2024"
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Ngày tổ chức *</label>
                    <input
                      id="startDate"
                      name="startDate"
                      type="date"
                      className="org-input"
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Giờ bắt đầu</label>
                    <input
                      id="startTime"
                      name="startTime"
                      type="time"
                      className="org-input"
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Địa điểm</label>
                    <input
                      id="location"
                      name="location"
                      className="org-input"
                      placeholder="Hội trường, link Zoom..."
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Chế độ hiển thị</label>
                    <select
                      id="visibility"
                      name="visibility"
                      className="org-select"
                    >
                      <option value="Public">Công khai</option>
                      <option value="OrganizationOnly">Nội bộ tổ chức</option>
                      <option value="Private">Riêng tư</option>
                    </select>
                  </div>

                  <div className="form-group" style={{ gridColumn: "1 / -1" }}>
                    <label className="form-label">
                      Số lượng tham gia dự kiến
                    </label>
                    <input
                      id="targetParticipants"
                      name="targetParticipants"
                      type="number"
                      className="org-input"
                      placeholder="Ví dụ: 100"
                    />
                  </div>

                  <div className="form-group" style={{ gridColumn: "1 / -1" }}>
                    <label className="form-label">Mô tả sự kiện</label>
                    <textarea
                      id="description"
                      name="description"
                      className="org-input"
                      placeholder="Thông tin chi tiết về sự kiện..."
                      style={{ minHeight: "100px", resize: "vertical" }}
                    />
                  </div>
                </div>
              </form>
            </div>

            <div className="org-modal-footer">
              <button
                type="button"
                onClick={() => setShowCreateForm(false)}
                className="org-btn org-btn-secondary"
                disabled={isSubmitting}
              >
                Hủy bỏ
              </button>
              <button
                type="submit"
                form="createEventForm"
                className="org-btn org-btn-primary"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Đang tạo..." : "Tạo sự kiện ngay"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrgEventsPage;
