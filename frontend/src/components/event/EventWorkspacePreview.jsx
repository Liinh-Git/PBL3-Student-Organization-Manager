import { useEffect, useMemo, useRef, useState } from "react";
import "../../pages/user/EventDetailPage.css";
import "./EventWorkspacePreview.css";
import { uploadEventBanner } from "../../services/eventService.js";

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return url.startsWith("/") ? `${origin}${url}` : `${origin}/${url}`;
}

function toDateTimeLocalInput(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

function formatDateOnly(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleDateString("vi-VN", {
    weekday: "long",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

function formatTimeOnly(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" });
}

function toIsoUtcFromLocalInput(value) {
  if (!value) return null;
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return null;
  return date.toISOString();
}

function buildDraft(eventData) {
  return {
    eventName: eventData?.name || eventData?.eventName || "",
    description: eventData?.description || "",
    startDate: toDateTimeLocalInput(eventData?.startDate),
    endDate: toDateTimeLocalInput(eventData?.endDate),
    location: eventData?.location || "",
    bannerUrl: eventData?.bannerUrl || "",
    visibility: eventData?.visibility || "Private",
    targetParticipants:
      eventData?.targetParticipants === null || eventData?.targetParticipants === undefined
        ? ""
        : String(eventData.targetParticipants),
  };
}

function getStatusLabel(status) {
  const normalized = String(status || "").toLowerCase();
  if (["published", "active", "ongoing"].includes(normalized)) return "Đang diễn ra";
  if (["draft", "planned"].includes(normalized)) return "Bản nháp";
  if (["completed", "archived"].includes(normalized)) return "Đã kết thúc";
  if (["cancelled"].includes(normalized)) return "Đã hủy";
  return status || "Không rõ";
}

function getStatusTone(status) {
  const normalized = String(status || "").toLowerCase();
  if (["published", "active", "ongoing"].includes(normalized)) return "live";
  if (["draft", "planned"].includes(normalized)) return "draft";
  if (["cancelled"].includes(normalized)) return "danger";
  return "neutral";
}

export default function EventWorkspacePreview({ eventData, canEdit, isSaving, onSave }) {
  const [draft, setDraft] = useState(buildDraft(eventData));
  const [editingField, setEditingField] = useState(null);
  const [isUploadingBanner, setIsUploadingBanner] = useState(false);
  const bannerFileInputRef = useRef(null);

  useEffect(() => {
    setDraft(buildDraft(eventData));
    setEditingField(null);
  }, [eventData]);

  const participantSummary = useMemo(() => {
    const registered = Number(eventData?.registeredParticipants ?? 0);
    const safeRegistered = Number.isFinite(registered) ? Math.max(0, registered) : 0;
    const target =
      draft.targetParticipants === ""
        ? eventData?.targetParticipants
        : Number(draft.targetParticipants);
    return `${safeRegistered}/${Number.isFinite(Number(target)) ? Number(target) : "-"}`;
  }, [draft.targetParticipants, eventData?.registeredParticipants, eventData?.targetParticipants]);

  const bannerSrc = toAbsoluteMediaUrl(draft.bannerUrl || eventData?.bannerUrl);
  const statusTone = getStatusTone(eventData?.status);

  const saveField = async (fields) => {
    const patch = {};
    fields.forEach((field) => {
      patch[field] = draft[field];
    });
    await onSave(patch);
    setEditingField(null);
  };

  const handlePickBanner = () => {
    if (!canEdit || isUploadingBanner || isSaving) return;
    bannerFileInputRef.current?.click();
  };

  const handleBannerFileChange = async (e) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;

    setIsUploadingBanner(true);
    try {
      const uploadedUrl = await uploadEventBanner(file);
      setDraft((prev) => ({ ...prev, bannerUrl: uploadedUrl }));
      await onSave({ bannerUrl: uploadedUrl });
    } catch (err) {
      alert(err.message || "Không thể tải banner sự kiện");
    } finally {
      setIsUploadingBanner(false);
    }
  };

  return (
    <div className="event-workspace-preview-root">
      <section className="event-remix-hero">
        <div className="event-remix-glow" />
        <div className="event-remix-hero-container">
          <div className="event-remix-hero-content">
            <div className="event-remix-tag-group">
              <span className={`event-remix-chip event-remix-chip--accent event-remix-chip--${statusTone}`}>
                {getStatusLabel(eventData?.status)}
              </span>
              <span className="event-remix-chip event-remix-chip--outline">
                {draft.visibility || "Private"}
              </span>
              {canEdit && (
                <button
                  type="button"
                  className="preview-edit-icon-btn"
                  onClick={() => setEditingField("visibility")}
                  title="Sửa hiển thị"
                >
                  ✎
                </button>
              )}
            </div>

            {editingField === "eventName" ? (
              <div className="preview-inline-editor">
                <input
                  className="form-input event-remix-input"
                  value={draft.eventName}
                  onChange={(e) => setDraft((prev) => ({ ...prev, eventName: e.target.value }))}
                />
                <div className="preview-inline-actions">
                  <button type="button" className="event-remix-back-btn" onClick={() => saveField(["eventName"])} disabled={isSaving}>Lưu</button>
                  <button type="button" className="event-remix-back-btn" onClick={() => setEditingField(null)} disabled={isSaving}>Hủy</button>
                </div>
              </div>
            ) : (
              <div className="preview-row">
                <h1 className="event-remix-hero-title">{draft.eventName || "-"}</h1>
                {canEdit && (
                  <button type="button" className="preview-edit-icon-btn" onClick={() => setEditingField("eventName")} title="Sửa tên sự kiện">
                    ✎
                  </button>
                )}
              </div>
            )}

            
          </div>

          <div className="event-remix-hero-visual">
            <input
              ref={bannerFileInputRef}
              type="file"
              accept="image/*"
              className="preview-hidden-file-input"
              onChange={handleBannerFileChange}
            />
            {editingField === "bannerUrl" ? (
              <div className="preview-inline-editor preview-banner-editor">
                <input
                  className="form-input event-remix-input"
                  value={draft.bannerUrl}
                  onChange={(e) => setDraft((prev) => ({ ...prev, bannerUrl: e.target.value }))}
                  placeholder="Banner URL"
                />
                <div className="preview-inline-actions">
                  <button type="button" className="event-remix-back-btn" onClick={() => saveField(["bannerUrl"])} disabled={isSaving}>Lưu</button>
                  <button type="button" className="event-remix-back-btn" onClick={() => setEditingField(null)} disabled={isSaving}>Hủy</button>
                </div>
              </div>
            ) : bannerSrc ? (
              <img src={bannerSrc} alt="Event banner" />
            ) : (
              <div className="event-remix-hero-placeholder">Banner sự kiện</div>
            )}
            {canEdit && editingField !== "bannerUrl" && (
              <button
                type="button"
                className="preview-edit-icon-btn preview-banner-edit"
                onClick={handlePickBanner}
                title="Chọn ảnh banner từ máy"
                disabled={isUploadingBanner || isSaving}
              >
                {isUploadingBanner ? "…" : "✎"}
              </button>
            )}
          </div>
        </div>
      </section>

      <div className="event-remix-stats-wrapper">
        <div className="event-remix-stats">
          <div className="event-remix-stat-item">
            <span className="event-remix-stat-label">Người tham gia</span>
            <span className="event-remix-stat-val">{participantSummary}</span>
          </div>
          <div className="event-remix-stat-item">
            <span className="event-remix-stat-label">Đơn vị tổ chức</span>
            <span className="event-remix-stat-val">{eventData?.organizationName || "Student Organization"}</span>
          </div>
        </div>
      </div>

      <main className="event-remix-main-layout">
        <div className="event-remix-details-col">
          <section className="event-remix-section">
            <div className="preview-row">
              <h2 className="event-remix-section-title">Về sự kiện này</h2>
              {canEdit && editingField !== "description" && (
                <button
                  type="button"
                  className="preview-edit-icon-btn dark"
                  onClick={() => setEditingField("description")}
                  title="Sửa mô tả"
                >
                  ✎
                </button>
              )}
            </div>
            {editingField === "description" ? (
              <div className="preview-inline-editor">
                <textarea
                  className="form-input event-remix-input"
                  rows={5}
                  value={draft.description}
                  onChange={(e) => setDraft((prev) => ({ ...prev, description: e.target.value }))}
                />
                <div className="preview-inline-actions">
                  <button type="button" className="event-remix-back-btn" onClick={() => saveField(["description"])} disabled={isSaving}>Lưu</button>
                  <button type="button" className="event-remix-back-btn" onClick={() => setEditingField(null)} disabled={isSaving}>Hủy</button>
                </div>
              </div>
            ) : (
              <p className="event-remix-text-content">
                {draft.description || "Sự kiện chưa có mô tả chi tiết."}
              </p>
            )}
          </section>
        </div>

        <div className="event-remix-sidebar-col">
          <div className="event-remix-action-widget">
            <h3>Đăng ký tham gia</h3>
            <p>Đây là preview giao diện attendee nhìn thấy.</p>
            <button type="button" className="event-remix-btn-join" disabled>
              Bạn đã đăng ký (Registered)
            </button>
          </div>


          <div className="event-remix-action-widget">
            <h3>Check-in sự kiện</h3>
            <p>Chỉ mở trong cửa sổ thời gian hợp lệ của ngày diễn ra sự kiện.</p>
            <button type="button" className="event-remix-btn-join" disabled>
              Mở trước sự kiện 1 giờ
            </button>
          </div>
          <div className="event-remix-logistics-card">
            <div className="event-remix-logistic-row">
              <div className="event-remix-log-icon">▷</div>
              <div className="event-remix-log-info">
                <h4>Thời gian tổ chức</h4>
                {editingField === "startDate" || editingField === "endDate" ? (
                  <div className="preview-inline-editor">
                    <input
                      type="datetime-local"
                      className="form-input event-remix-input"
                      value={draft.startDate}
                      onChange={(e) => setDraft((prev) => ({ ...prev, startDate: e.target.value }))}
                    />
                    <div className="preview-inline-actions">
                      <button type="button" className="event-remix-back-btn" onClick={() => saveField(["startDate", "endDate"])} disabled={isSaving}>Lưu</button>
                      <button type="button" className="event-remix-back-btn" onClick={() => setEditingField(null)} disabled={isSaving}>Hủy</button>
                    </div>
                  </div>
                ) : (
                  <div className="preview-row">
                    <p>
                      {formatTimeOnly(eventData?.startDate)} - {formatTimeOnly(eventData?.endDate || eventData?.startDate)}
                      <br />
                      {formatDateOnly(eventData?.startDate)}
                    </p>
                    {canEdit && (
                      <button type="button" className="preview-edit-icon-btn dark" onClick={() => setEditingField("startDate")} title="Sửa thời gian">
                        ✎
                      </button>
                    )}
                  </div>
                )}
              </div>
            </div>

            <div className="event-remix-logistic-row">
              <div className="event-remix-log-icon">◎</div>
              <div className="event-remix-log-info">
                <h4>Địa điểm</h4>
                {editingField === "location" ? (
                  <div className="preview-inline-editor">
                    <input
                      className="form-input event-remix-input"
                      value={draft.location}
                      onChange={(e) => setDraft((prev) => ({ ...prev, location: e.target.value }))}
                    />
                    <div className="preview-inline-actions">
                      <button type="button" className="event-remix-back-btn" onClick={() => saveField(["location"])} disabled={isSaving}>Lưu</button>
                      <button type="button" className="event-remix-back-btn" onClick={() => setEditingField(null)} disabled={isSaving}>Hủy</button>
                    </div>
                  </div>
                ) : (
                  <div className="preview-row">
                    <p>{draft.location || "-"}</p>
                    {canEdit && (
                      <button type="button" className="preview-edit-icon-btn dark" onClick={() => setEditingField("location")} title="Sửa địa điểm">
                        ✎
                      </button>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
