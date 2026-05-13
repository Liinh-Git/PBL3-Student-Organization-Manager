/**
 * OrgEventDetailPage.jsx - Organization event detail page
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from "react";
import { useParams, useSearchParams, useNavigate } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import { getEventById, updateEvent } from "../../services/eventService.js";
import {
  getEventMilestones,
  createMilestone,
  updateMilestone,
  deleteMilestone,
} from "../../services/milestoneService.js";
import {
  getMilestoneCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from "../../services/categoryService.js";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgEventDetailPage.css";

function OrgEventDetailPage() {
  const { eventId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const orgId = searchParams.get("orgId");
  const { permissions, isMember } = useOrgContext();

  const [event, setEvent] = useState(null);
  const [milestones, setMilestones] = useState([]);
  const [categoriesByMilestone, setCategoriesByMilestone] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  // Modal States
  const [showEditEvent, setShowEditEvent] = useState(false);
  const [showCreateMilestone, setShowCreateMilestone] = useState(false);
  const [showCreateCategory, setShowCreateCategory] = useState(false);
  const [editingMilestone, setEditingMilestone] = useState(null);
  const [editingCategory, setEditingCategory] = useState(null); // { id, milestoneId, categoryName, description }

  const [isSubmitting, setIsSubmitting] = useState(false);

  const getEventName = (eventData) => eventData?.name || eventData?.eventName;
  const canManage = permissions.includes("org.events.manage");

  useEffect(() => {
    if (!eventId || !orgId || !isMember) return;
    async function loadEventDetail() {
      setIsLoading(true);
      try {
        const eventData = await getEventById(eventId);
        setEvent(eventData);

        const milestonesData = await getEventMilestones(eventId);
        setMilestones(milestonesData);

        const categoriesMap = {};
        for (const milestone of milestonesData) {
          const categoriesData = await getMilestoneCategories(milestone.id);
          // Ensure tasks array exists
          const categoriesWithTasks = categoriesData.map((cat) => ({
            ...cat,
            tasks: (cat.tasks || []).filter(
              (task) =>
                task &&
                (task.eventCategoryId === cat.id ||
                  task.categoryId === cat.id) &&
                !task.deptId,
            ),
          }));
          categoriesMap[milestone.id] = categoriesWithTasks;
        }
        setCategoriesByMilestone(categoriesMap);
      } catch (err) {
        setError(err.message || "Failed to load event detail");
      } finally {
        setIsLoading(false);
      }
    }
    loadEventDetail();
  }, [eventId, orgId, isMember]);

  // --- HANDLERS EVENT ---
  const handleUpdateEvent = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    setIsSubmitting(true);
    const form = e.target;
    try {
      const updated = await updateEvent(eventId, {
        eventName: form.eventName.value,
        description: form.description.value || undefined,
        startDate: `${form.startDate.value}T${form.startTime.value || "00:00"}:00Z`,
        location: form.location.value || undefined,
        targetParticipants: form.targetParticipants.value
          ? parseInt(form.targetParticipants.value)
          : undefined,
        bannerUrl: form.bannerUrl.value || undefined,
        visibility: form.visibility.value,
      });
      setEvent(updated);
      setShowEditEvent(false);
    } catch (err) {
      alert(err.message || "Failed to update event");
    } finally {
      setIsSubmitting(false);
    }
  };

  // --- HANDLERS MILESTONE ---
  const handleCreateMilestone = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    setIsSubmitting(true);
    const form = e.target;
    try {
      const newMilestone = await createMilestone(eventId, {
        title: form.title.value,
        description: form.description.value || undefined,
        orderIndex: milestones.length + 1,
      });
      setMilestones((prev) => [...prev, newMilestone]);
      setCategoriesByMilestone((prev) => ({ ...prev, [newMilestone.id]: [] }));
      setShowCreateMilestone(false);
    } catch (err) {
      alert(err.message || "Failed to create milestone");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateMilestone = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    setIsSubmitting(true);
    const form = e.target;
    try {
      const updated = await updateMilestone(editingMilestone.id, {
        title: form.title.value,
        description: form.description.value || undefined,
      });
      setMilestones((prev) =>
        prev.map((m) =>
          m.id === editingMilestone.id
            ? { ...m, title: updated.title, description: updated.description }
            : m,
        ),
      );
      setEditingMilestone(null);
    } catch (err) {
      alert(err.message || "Failed to update milestone");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteMilestone = async (milestoneId) => {
    if (
      !canManage ||
      !window.confirm("Delete this milestone and all its contents?")
    )
      return;
    try {
      await deleteMilestone(milestoneId);
      setMilestones((prev) => prev.filter((m) => m.id !== milestoneId));
      setCategoriesByMilestone((prev) => {
        const updated = { ...prev };
        delete updated[milestoneId];
        return updated;
      });
    } catch (err) {
      alert(err.message);
    }
  };

  // --- HANDLERS CATEGORY ---
  const handleCreateCategory = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    setIsSubmitting(true);
    const form = e.target;
    const mId = form.milestoneId.value;
    try {
      const newCategory = await createCategory(mId, {
        categoryName: form.categoryName.value,
        description: form.description.value || undefined,
        orderIndex: (categoriesByMilestone[mId]?.length || 0) + 1,
      });
      setCategoriesByMilestone((prev) => ({
        ...prev,
        [mId]: [...(prev[mId] || []), newCategory],
      }));
      setShowCreateCategory(false);
    } catch (err) {
      alert(err.message || "Failed to create category");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateCategory = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    setIsSubmitting(true);
    const form = e.target;
    const mId = editingCategory.milestoneId;
    try {
      const updated = await updateCategory(editingCategory.id, {
        categoryName: form.categoryName.value,
        description: form.description.value || undefined,
      });
      setCategoriesByMilestone((prev) => ({
        ...prev,
        [mId]: prev[mId].map((c) =>
          c.id === editingCategory.id
            ? {
                ...c,
                categoryName: updated.categoryName,
                description: updated.description,
              }
            : c,
        ),
      }));
      setEditingCategory(null);
    } catch (err) {
      alert(err.message || "Failed to update category");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteCategory = async (categoryId, milestoneId, e) => {
    e.stopPropagation();
    if (!canManage || !window.confirm("Delete this category?")) return;
    try {
      await deleteCategory(categoryId);
      setCategoriesByMilestone((prev) => ({
        ...prev,
        [milestoneId]: prev[milestoneId].filter((c) => c.id !== categoryId),
      }));
    } catch (err) {
      alert(err.message);
    }
  };

  if (!eventId || !orgId)
    return <ErrorState message="Event ID and Organization ID are required" />;
  if (!isMember)
    return (
      <ForbiddenState message="You are not a member of this organization" />
    );
  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorState message={error} />;

  return (
    <div className="event-detail-container">
      {/* Banner */}
      <div className="ev-banner-wrapper">
        {event?.bannerUrl && (
          <img src={event.bannerUrl} alt="Banner" className="ev-banner-img" />
        )}
        <div className="ev-banner-content">
          <div className="ev-banner-text">
            <p>CHUYÊN MỤC SỰ KIỆN</p>
            <h1>{getEventName(event) || "Đang tải..."}</h1>
            <p>{event?.description || "Chưa có mô tả sự kiện."}</p>
          </div>
          {canManage && (
            <button
              onClick={() => setShowEditEvent(true)}
              className="ev-btn ev-btn-primary"
            >
              Cập nhật thông tin
            </button>
          )}
        </div>
      </div>

      {/* Stats */}
      <div className="ev-stats-row">
        <div className="ev-stat-card">
          <span className="ev-stat-label">Thời gian bắt đầu</span>
          <p className="ev-stat-value">
            {event?.startDate
              ? new Date(event.startDate).toLocaleDateString("vi-VN")
              : "-"}
          </p>
          <span className="ev-stat-sub">
            {event?.startDate && event.startDate.includes("T")
              ? event.startDate.split("T")[1].substring(0, 5)
              : ""}
          </span>
        </div>
        <div className="ev-stat-card">
          <span className="ev-stat-label">Trạng thái</span>
          <p className="ev-stat-value">{event?.status || "-"}</p>
          <span className="ev-stat-sub">
            Hiển thị: {event?.visibility || "Private"}
          </span>
        </div>
        <div className="ev-stat-card">
          <span className="ev-stat-label">Dự kiến tham gia</span>
          <p className="ev-stat-value">{event?.targetParticipants || 0}</p>
          <span className="ev-stat-sub">Thành viên / Khách</span>
        </div>
        <div className="ev-stat-card">
          <span className="ev-stat-label">Địa điểm</span>
          <p className="ev-stat-value" style={{ fontSize: "1.2rem" }}>
            {event?.location || "Trực tuyến"}
          </p>
        </div>
      </div>

      {/* Main Content */}
      <div className="ev-main-layout">
        <div className="layout-left">
          <div className="ev-section-header">
            <h2>HẠNG MỤC CÔNG VIỆC THEO GIAI ĐOẠN</h2>
            {canManage && (
              <button
                onClick={() => setShowCreateCategory(true)}
                className="ev-btn ev-btn-primary"
                style={{ padding: "6px 12px" }}
              >
                + Thêm Hạng mục
              </button>
            )}
          </div>

          {milestones.length === 0 ? (
            <div
              style={{
                padding: "2rem",
                textAlign: "center",
                background: "white",
                borderRadius: "16px",
              }}
            >
              Chưa có giai đoạn nào.
            </div>
          ) : (
            milestones.map((m, index) => (
              <div key={m.id} className="ev-phase-block">
                <div className="ev-phase-header">
                  <div className="ev-phase-badge">
                    {String(index + 1).padStart(2, "0")}
                  </div>
                  <div className="ev-phase-title">
                    <h3>{m.title}</h3>
                    <p>{m.description || "Chưa có mô tả"}</p>
                  </div>
                </div>

                <div className="ev-cat-list">
                  {!categoriesByMilestone[m.id] ||
                  categoriesByMilestone[m.id].length === 0 ? (
                    <p style={{ color: "var(--ink-500)", fontSize: "0.85rem" }}>
                      Chưa có hạng mục công việc.
                    </p>
                  ) : (
                    categoriesByMilestone[m.id].map((cat) => (
                      <div key={cat.id} className="ev-cat-row">
                        <div className="ev-cat-info">
                          <h4>{cat.categoryName}</h4>
                          <p>{cat.description || "Không có mô tả"}</p>
                        </div>
                        <div className="ev-phase-actions">
                          <button
                            className="ev-btn ev-btn-primary"
                            style={{ padding: "6px 16px", fontSize: "0.8rem" }}
                            onClick={() =>
                              navigate(
                                `/org/events/${eventId}/category/${cat.id}?orgId=${orgId}&milestoneId=${m.id}`,
                              )
                            }
                          >
                            Xem chi tiết
                          </button>
                          {canManage && (
                            <>
                              <button
                                className="ev-btn-ghost"
                                onClick={() =>
                                  setEditingCategory({
                                    ...cat,
                                    milestoneId: m.id,
                                  })
                                }
                              >
                                ✎
                              </button>
                              <button
                                className="ev-btn-ghost"
                                style={{ color: "red" }}
                                onClick={(e) =>
                                  handleDeleteCategory(cat.id, m.id, e)
                                }
                              >
                                ✕
                              </button>
                            </>
                          )}
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            ))
          )}
        </div>

        <div className="layout-right">
          <div className="ev-timeline-card">
            <div className="ev-section-header" style={{ marginBottom: 0 }}>
              <h2>DÒNG THỜI GIAN</h2>
              {canManage && (
                <button
                  onClick={() => setShowCreateMilestone(true)}
                  className="ev-btn ev-btn-secondary"
                  style={{ padding: "4px 10px" }}
                >
                  +
                </button>
              )}
            </div>

            <div className="ev-timeline-list">
              {milestones.map((m, index) => (
                <div key={m.id} className="ev-time-node">
                  <div className="ev-time-dot"></div>
                  <div className="ev-time-content">
                    <div
                      style={{
                        display: "flex",
                        justifyContent: "space-between",
                      }}
                    >
                      <h4>
                        Giai đoạn {index + 1}: {m.title}
                      </h4>
                      {canManage && (
                        <div style={{ display: "flex", gap: 4 }}>
                          <span
                            style={{ cursor: "pointer", fontSize: "12px" }}
                            onClick={() => setEditingMilestone(m)}
                          >
                            ✎
                          </span>
                          <span
                            style={{
                              cursor: "pointer",
                              fontSize: "12px",
                              color: "red",
                            }}
                            onClick={() => handleDeleteMilestone(m.id)}
                          >
                            ✕
                          </span>
                        </div>
                      )}
                    </div>
                    <p>{m.description}</p>
                  </div>
                </div>
              ))}
              {milestones.length === 0 && (
                <p style={{ color: "var(--ink-500)", fontSize: "0.85rem" }}>
                  Chưa có mốc thời gian.
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* MODAL SỬA EVENT */}
      {showEditEvent && canManage && (
        <div
          className="ev-modal-overlay"
          onClick={() => setShowEditEvent(false)}
        >
          <div className="ev-modal" onClick={(e) => e.stopPropagation()}>
            <div className="ev-modal-header">
              <h3>Cập nhật thông tin sự kiện</h3>
            </div>
            <div className="ev-modal-body">
              <form id="editEventForm" onSubmit={handleUpdateEvent}>
                <div className="form-group" style={{ marginBottom: "1rem" }}>
                  <label className="form-label">Tên sự kiện *</label>
                  <input
                    name="eventName"
                    className="form-input"
                    defaultValue={getEventName(event)}
                    required
                  />
                </div>
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "1rem",
                    marginBottom: "1rem",
                  }}
                >
                  <div className="form-group">
                    <label className="form-label">Ngày *</label>
                    <input
                      name="startDate"
                      type="date"
                      className="form-input"
                      defaultValue={
                        event?.startDate ? event.startDate.split("T")[0] : ""
                      }
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Giờ</label>
                    <input
                      name="startTime"
                      type="time"
                      className="form-input"
                      defaultValue={
                        event?.startDate && event.startDate.includes("T")
                          ? event.startDate.split("T")[1].substring(0, 5)
                          : "00:00"
                      }
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Dự kiến tham gia</label>
                    <input
                      name="targetParticipants"
                      type="number"
                      className="form-input"
                      defaultValue={event?.targetParticipants || ""}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Quyền xem</label>
                    <select
                      name="visibility"
                      defaultValue={event?.visibility || "Private"}
                      className="form-input"
                    >
                      <option value="Public">Công khai</option>
                      <option value="OrganizationOnly">Nội bộ</option>
                      <option value="Private">Riêng tư</option>
                    </select>
                  </div>
                </div>
                <div className="form-group" style={{ marginBottom: "1rem" }}>
                  <label className="form-label">Địa điểm</label>
                  <input
                    name="location"
                    className="form-input"
                    defaultValue={event?.location || ""}
                  />
                </div>
                <div className="form-group" style={{ marginBottom: "1rem" }}>
                  <label className="form-label">Ảnh Banner URL</label>
                  <input
                    name="bannerUrl"
                    className="form-input"
                    defaultValue={event?.bannerUrl || ""}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Mô tả</label>
                  <textarea
                    name="description"
                    className="form-input"
                    style={{ minHeight: 80 }}
                    defaultValue={event?.description || ""}
                  />
                </div>
              </form>
            </div>
            <div className="ev-modal-footer">
              <button
                onClick={() => setShowEditEvent(false)}
                className="ev-btn ev-btn-secondary"
              >
                Hủy
              </button>
              <button
                form="editEventForm"
                type="submit"
                disabled={isSubmitting}
                className="ev-btn ev-btn-primary"
              >
                Lưu thay đổi
              </button>
            </div>
          </div>
        </div>
      )}

      {/* MODAL THÊM / SỬA MILESTONE */}
      {(showCreateMilestone || editingMilestone) && canManage && (
        <div
          className="ev-modal-overlay"
          onClick={() => {
            setShowCreateMilestone(false);
            setEditingMilestone(null);
          }}
        >
          <div className="ev-modal" onClick={(e) => e.stopPropagation()}>
            <div className="ev-modal-header">
              <h3>
                {editingMilestone
                  ? "Sửa giai đoạn"
                  : "Thêm giai đoạn mới (Milestone)"}
              </h3>
            </div>
            <div className="ev-modal-body">
              <form
                id="msForm"
                onSubmit={
                  editingMilestone
                    ? handleUpdateMilestone
                    : handleCreateMilestone
                }
              >
                <div className="form-group" style={{ marginBottom: "1rem" }}>
                  <label className="form-label">Tên giai đoạn *</label>
                  <input
                    name="title"
                    className="form-input"
                    defaultValue={editingMilestone?.title || ""}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Mô tả</label>
                  <textarea
                    name="description"
                    className="form-input"
                    style={{ minHeight: 80 }}
                    defaultValue={editingMilestone?.description || ""}
                  />
                </div>
              </form>
            </div>
            <div className="ev-modal-footer">
              <button
                onClick={() => {
                  setShowCreateMilestone(false);
                  setEditingMilestone(null);
                }}
                className="ev-btn ev-btn-secondary"
              >
                Hủy
              </button>
              <button
                form="msForm"
                type="submit"
                disabled={isSubmitting}
                className="ev-btn ev-btn-primary"
              >
                {editingMilestone ? "Cập nhật" : "Tạo mới"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* MODAL THÊM / SỬA CATEGORY */}
      {(showCreateCategory || editingCategory) && canManage && (
        <div
          className="ev-modal-overlay"
          onClick={() => {
            setShowCreateCategory(false);
            setEditingCategory(null);
          }}
        >
          <div className="ev-modal" onClick={(e) => e.stopPropagation()}>
            <div className="ev-modal-header">
              <h3>
                {editingCategory
                  ? "Sửa hạng mục công việc"
                  : "Tạo hạng mục công việc (Category)"}
              </h3>
            </div>
            <div className="ev-modal-body">
              <form
                id="catForm"
                onSubmit={
                  editingCategory ? handleUpdateCategory : handleCreateCategory
                }
              >
                {!editingCategory && (
                  <div className="form-group" style={{ marginBottom: "1rem" }}>
                    <label className="form-label">
                      Chọn giai đoạn (Milestone) *
                    </label>
                    <select name="milestoneId" className="form-input" required>
                      {milestones.map((m) => (
                        <option key={m.id} value={m.id}>
                          {m.title}
                        </option>
                      ))}
                    </select>
                  </div>
                )}
                <div className="form-group" style={{ marginBottom: "1rem" }}>
                  <label className="form-label">Tên hạng mục *</label>
                  <input
                    name="categoryName"
                    className="form-input"
                    defaultValue={editingCategory?.categoryName || ""}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Mô tả</label>
                  <textarea
                    name="description"
                    className="form-input"
                    style={{ minHeight: 80 }}
                    defaultValue={editingCategory?.description || ""}
                  />
                </div>
              </form>
            </div>
            <div className="ev-modal-footer">
              <button
                onClick={() => {
                  setShowCreateCategory(false);
                  setEditingCategory(null);
                }}
                className="ev-btn ev-btn-secondary"
              >
                Hủy
              </button>
              <button
                form="catForm"
                type="submit"
                disabled={isSubmitting}
                className="ev-btn ev-btn-primary"
              >
                {editingCategory ? "Cập nhật" : "Tạo mới"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default OrgEventDetailPage;
