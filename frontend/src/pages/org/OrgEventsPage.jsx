/**
 * OrgEventsPage.jsx - Organization events page
 *
 * UI refactor: Card design matching original demo (image_973323.jpg), Orange Theme.
 */

import { useRef, useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  getOrganizationEvents,
  createEvent,
  updateEvent,
  updateEventStatus,
  deleteEvent,
  uploadEventBanner,
} from "../../services/eventService.js";
import { getOrganizationMembers } from "../../services/memberService.js";
import PageHeader from "../../components/shared/PageHeader";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import "./OrgEventsPage.css";

// ── Icons ──
const IconClock = () => (
  <svg
    width="13"
    height="13"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <circle cx="12" cy="12" r="10" />
    <polyline points="12 6 12 12 16 14" />
  </svg>
);

const IconPin = () => (
  <svg
    width="13"
    height="13"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
    <circle cx="12" cy="10" r="3" />
  </svg>
);

const IconBriefcase = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <rect x="2" y="7" width="20" height="14" rx="2" ry="2" />
    <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16" />
  </svg>
);

const IconEdit = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const IconTrash = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <polyline points="3 6 5 6 21 6" />
    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
  </svg>
);

const IconGlobe = () => (
  <svg
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <circle cx="12" cy="12" r="10" />
    <line x1="2" y1="12" x2="22" y2="12" />
    <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1 4-10z" />
  </svg>
);

// ── Helpers ──

// ── Helpers ──
const DEFAULT_EVENT_BANNER =
  "data:image/svg+xml;utf8," +
  encodeURIComponent(
    "<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='360' viewBox='0 0 1200 360'>" +
      "<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>" +
      "<stop offset='0%' stop-color='#fed7aa'/><stop offset='100%' stop-color='#ffedd5'/>" +
      "</linearGradient></defs>" +
      "<rect width='1200' height='360' fill='url(#g)'/>" +
      "<text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#ea580c' font-family='Arial' font-size='42'>Banner sự kiện</text>" +
      "</svg>",
  );

function toAbsoluteMediaUrl(url) {
  if (!url) return "";
  if (/^https?:\/\//i.test(url)) return url;
  const apiBase =
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
  const origin = apiBase.replace(/\/api\/?$/, "");
  return url.startsWith("/") ? `${origin}${url}` : `${origin}/${url}`;
}

function parseDateBadge(dateStr) {
  if (!dateStr) return { month: "-", day: "-" };
  const d = new Date(dateStr);
  return { month: d.getMonth() + 1, day: d.getDate() };
}

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
  const [editingEvent, setEditingEvent] = useState(null);
  const [orgMembers, setOrgMembers] = useState([]);

  useEffect(() => {
    if (!orgId || !isMember) return;
    async function loadEvents() {
      setIsLoading(true);
      try {
        const data = await getOrganizationEvents(orgId);
        setEvents(data);
        const membersData = await getOrganizationMembers(orgId);
        setOrgMembers(Array.isArray(membersData) ? membersData : []);
      } catch (err) {
        setError(err.message || "Không thể tải danh sách sự kiện");
      } finally {
        setIsLoading(false);
      }
    }
    loadEvents();
  }, [orgId, isMember]);

  if (!orgId) {
    return <ErrorState message="Thiếu mã tổ chức" />;
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader title="Sự kiện" description="Quản lý sự kiện của tổ chức" />
        <ForbiddenState message="Bạn không phải thành viên của tổ chức này" />
      </div>
    );
  }

  const canCreate = permissions.includes("org.events.create");
  const canManage = permissions.includes("org.events.manage");
  const getEventId = (event) => event?.id || event?.eventId;
  const getEventName = (event) => event?.name || event?.eventName;
  const normalizeVisibility = (visibility) =>
    visibility === "OrganizationOnly" ? "Private" : visibility || "Private";

  const getVisibilityDescription = (visibility) => {
    const normalized = normalizeVisibility(visibility);
    return normalized === "Public"
      ? "Công khai: mọi người đều có thể xem"
      : "Riêng tư: chỉ thành viên tổ chức có thể xem";
  };

  const getStatusMeta = (status) => {
    const normalized = String(status || "").toLowerCase();
    if (normalized === "published")
      return { label: "Đã công khai", class: "status-published" };
    if (normalized === "draft") return { label: "Nháp", class: "status-draft" };
    if (normalized === "completed")
      return { label: "Hoàn thành", class: "status-completed" };
    if (normalized === "cancelled")
      return { label: "Đã hủy", class: "status-cancelled" };
    if (normalized === "ongoing")
      return { label: "Đang diễn ra", class: "status-ongoing" };
    return { label: status || "Không rõ", class: "status-default" };
  };

  const formatDate = (value) => {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return date.toLocaleDateString("vi-VN");
  };

  const formatTime = (value) => {
    if (!value || !String(value).includes("T")) return "-";
    return String(value).split("T")[1].substring(0, 5);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!canCreate) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const form = e.target;
    const eventName = form.eventName.value;
    const description = form.description.value;
    const startDate = form.startDate.value;
    const startTime = form.startTime.value;
    const location = form.location.value;
    const targetParticipants = form.targetParticipants.value;
    const bannerUrl = form.bannerUrl.value;
    const visibility = normalizeVisibility(form.visibility.value);
    const initialMemberIds = Array.from(
      form.initialMemberIds?.selectedOptions || [],
    )
      .map((opt) => opt.value)
      .filter(Boolean);

    if (!eventName || !startDate) {
      alert("Vui lòng nhập tên sự kiện và ngày tổ chức");
      return;
    }

    setIsSubmitting(true);
    try {
      const newEvent = await createEvent(orgId, {
        eventName,
        description: description || undefined,
        startDate: `${startDate}T${startTime || "00:00"}:00Z`,
        location: location || undefined,
        targetParticipants: targetParticipants
          ? parseInt(targetParticipants, 10)
          : undefined,
        bannerUrl: bannerUrl || undefined,
        visibility,
        initialMemberIds,
      });
      setEvents((prev) => [...prev, newEvent]);
      form.reset();
      setShowCreateForm(false);
    } catch (err) {
      alert(err.message || "Không thể tạo sự kiện");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!canManage || !editingEvent) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const form = e.target;
    const eventName = form.eventName.value;
    const description = form.description.value;
    const startDate = form.startDate.value;
    const startTime = form.startTime.value;
    const location = form.location.value;
    const targetParticipants = form.targetParticipants.value;
    const bannerUrl = form.bannerUrl.value;
    const visibility = normalizeVisibility(form.visibility.value);

    if (!eventName || !startDate) {
      alert("Vui lòng nhập tên sự kiện và ngày tổ chức");
      return;
    }

    setIsSubmitting(true);
    try {
      const editingEventId = getEventId(editingEvent);
      if (!editingEventId) {
        alert("Thiếu mã sự kiện");
        return;
      }
      const updated = await updateEvent(editingEventId, {
        eventName,
        description: description || undefined,
        startDate: `${startDate}T${startTime || "00:00"}:00Z`,
        location: location || undefined,
        targetParticipants: targetParticipants
          ? parseInt(targetParticipants, 10)
          : undefined,
        bannerUrl: bannerUrl || undefined,
        visibility,
      });
      setEvents((prev) =>
        prev.map((ev) => (getEventId(ev) === editingEventId ? updated : ev)),
      );
      setEditingEvent(null);
    } catch (err) {
      alert(err.message || "Không thể cập nhật sự kiện");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handlePublishEvent = async (event) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const eventId = getEventId(event);
    if (!eventId) {
      alert("Thiếu mã sự kiện");
      return;
    }

    if (event?.status !== "Draft") {
      alert("Chỉ sự kiện ở trạng thái Nháp mới có thể công khai.");
      return;
    }

    const confirmMessage = [
      `Hiện tại sự kiện đang ở mức ${getVisibilityDescription(event?.visibility)}.`,
      "Nếu công khai, trạng thái sẽ chuyển từ Nháp sang Đã công khai.",
      normalizeVisibility(event?.visibility) === "Public"
        ? "Sự kiện sẽ hiển thị ở trang Khám phá cho người ngoài tổ chức."
        : "Sự kiện vẫn chỉ hiển thị cho thành viên trong tổ chức.",
      "",
      "Xác nhận công khai?",
    ].join("\n");

    if (!window.confirm(confirmMessage)) return;

    setIsSubmitting(true);
    try {
      const updated = await updateEventStatus(eventId, "Published");
      setEvents((prev) =>
        prev.map((ev) => (getEventId(ev) === eventId ? updated : ev)),
      );
    } catch (err) {
      alert(err.message || "Không thể công khai sự kiện");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleManageEventFromClose = async (event) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const eventId = getEventId(event);
    if (!eventId) {
      alert("Thiếu mã sự kiện");
      return;
    }

    const visibility = normalizeVisibility(event?.visibility);
    const status = event?.status;

    if (status === "Published" && visibility !== "Public") {
      const backToDraft = window.confirm(
        "Sự kiện đang ở trạng thái Đã công khai nhưng chưa đặt Công khai hiển thị.\nBạn có muốn đưa sự kiện về Nháp không?",
      );
      if (backToDraft) {
        setIsSubmitting(true);
        try {
          const updated = await updateEventStatus(eventId, "Draft");
          setEvents((prev) =>
            prev.map((ev) => (getEventId(ev) === eventId ? updated : ev)),
          );
        } catch (err) {
          alert(err.message || "Không thể chuyển sự kiện về nháp");
        } finally {
          setIsSubmitting(false);
        }
        return;
      }
    }

    const action = window.prompt(
      "Chọn hành động:\n1 - Hủy sự kiện\n2 - Xóa hoàn toàn khỏi hệ thống\nNhập 1 hoặc 2:",
      "1",
    );
    if (!action) return;

    if (action === "1") {
      if (!window.confirm("Xác nhận hủy sự kiện này?")) return;
      setIsSubmitting(true);
      try {
        const updated = await updateEventStatus(eventId, "Cancelled");
        setEvents((prev) =>
          prev.map((ev) => (getEventId(ev) === eventId ? updated : ev)),
        );
      } catch (err) {
        alert(err.message || "Không thể hủy sự kiện");
      } finally {
        setIsSubmitting(false);
      }
      return;
    }

    if (action === "2") {
      if (
        !window.confirm(
          "Xóa hoàn toàn sự kiện? Dữ liệu liên quan sẽ bị ẩn khỏi hệ thống.",
        )
      )
        return;
      setIsSubmitting(true);
      try {
        await deleteEvent(eventId, true);
        setEvents((prev) => prev.filter((ev) => getEventId(ev) !== eventId));
      } catch (err) {
        alert(err.message || "Không thể xóa vĩnh viễn sự kiện");
      } finally {
        setIsSubmitting(false);
      }
    }
  };

  const closeForms = () => {
    setShowCreateForm(false);
    setEditingEvent(null);
  };

  const openWorkspace = (event) => {
    const selectedEventId = getEventId(event);
    if (!selectedEventId) {
      alert("Thiếu mã sự kiện");
      return;
    }
    navigate(`/org/events/${selectedEventId}?orgId=${orgId}`);
  };

  const EventForm = ({ mode, event }) => {
    const isEdit = mode === "edit";
    const fileInputRef = useRef(null);
    const [bannerValue, setBannerValue] = useState(
      isEdit ? event?.bannerUrl || "" : "",
    );
    const [pendingBannerUrl, setPendingBannerUrl] = useState("");
    const [isUploadingBanner, setIsUploadingBanner] = useState(false);
    const [memberSearch, setMemberSearch] = useState("");
    const [selectedInitialMembers, setSelectedInitialMembers] = useState([]);

    const handleBannerUpload = async (uploadEvent) => {
      const file = uploadEvent.target.files?.[0];
      if (!file) return;

      setIsUploadingBanner(true);
      try {
        const uploadedUrl = await uploadEventBanner(file);
        setPendingBannerUrl(uploadedUrl || "");
      } catch (err) {
        alert(err.message || "Không thể tải ảnh banner");
      } finally {
        setIsUploadingBanner(false);
        uploadEvent.target.value = "";
      }
    };

    const acceptPendingBanner = () => {
      setBannerValue(pendingBannerUrl);
      setPendingBannerUrl("");
    };

    const rejectPendingBanner = () => {
      setPendingBannerUrl("");
    };

    const normalizedSearch = memberSearch.trim().toLowerCase();
    const filteredMembers = orgMembers.filter((member) => {
      if (!normalizedSearch) return true;
      const fullName = String(member?.fullName || "").toLowerCase();
      const email = String(member?.email || "").toLowerCase();
      const deptName = String(
        member?.department?.departmentName ||
          member?.department?.deptName ||
          "",
      ).toLowerCase();
      return (
        fullName.includes(normalizedSearch) ||
        email.includes(normalizedSearch) ||
        deptName.includes(normalizedSearch)
      );
    });

    const addInitialMember = (member) => {
      if (!member?.id) return;
      setSelectedInitialMembers((prev) => {
        if (prev.some((m) => m.id === member.id)) return prev;
        return [...prev, member];
      });
    };

    const removeInitialMember = (memberId) => {
      setSelectedInitialMembers((prev) =>
        prev.filter((m) => m.id !== memberId),
      );
    };

    return (
      <div className="oe-modal-backdrop" role="dialog" aria-modal="true">
        <div className="oe-modal">
          <div className="oe-form-panel">
            <div className="oe-form-header">
              <div>
                <p className="oe-eyebrow">
                  {isEdit ? "Cập nhật dự án" : "Tạo dự án"}
                </p>
                <h2>{isEdit ? "Sửa sự kiện" : "Tạo sự kiện mới"}</h2>
              </div>
              <button
                type="button"
                onClick={closeForms}
                className="oe-icon-btn"
                aria-label="Đóng form"
              >
                ×
              </button>
            </div>

            <form
              onSubmit={isEdit ? handleUpdate : handleCreate}
              className="oe-form-grid"
            >
              <div className="form-group">
                <label
                  htmlFor={isEdit ? "editEventName" : "eventName"}
                  className="oe-label"
                >
                  Tên sự kiện *
                </label>
                <input
                  id={isEdit ? "editEventName" : "eventName"}
                  name="eventName"
                  className="oe-input"
                  defaultValue={isEdit ? getEventName(event) : ""}
                  placeholder="Nhập tên sự kiện"
                  required
                />
              </div>
              <div className="form-group">
                <label
                  htmlFor={isEdit ? "editDescription" : "description"}
                  className="oe-label"
                >
                  Mô tả
                </label>
                <input
                  id={isEdit ? "editDescription" : "description"}
                  name="description"
                  className="oe-input"
                  defaultValue={isEdit ? event?.description || "" : ""}
                  placeholder="Nhập mô tả ngắn"
                />
              </div>
              <div className="form-group">
                <label
                  htmlFor={isEdit ? "editStartDate" : "startDate"}
                  className="oe-label"
                >
                  Ngày tổ chức *
                </label>
                <input
                  id={isEdit ? "editStartDate" : "startDate"}
                  name="startDate"
                  type="date"
                  className="oe-input"
                  defaultValue={
                    isEdit && event?.startDate
                      ? String(event.startDate).split("T")[0]
                      : ""
                  }
                  required
                />
              </div>
              <div className="form-group">
                <label
                  htmlFor={isEdit ? "editStartTime" : "startTime"}
                  className="oe-label"
                >
                  Giờ bắt đầu
                </label>
                <input
                  id={isEdit ? "editStartTime" : "startTime"}
                  name="startTime"
                  type="time"
                  className="oe-input"
                  defaultValue={
                    isEdit &&
                    event?.startDate &&
                    String(event.startDate).includes("T")
                      ? String(event.startDate).split("T")[1].substring(0, 5)
                      : "00:00"
                  }
                />
              </div>
              <div className="form-group">
                <label
                  htmlFor={
                    isEdit ? "editTargetParticipants" : "targetParticipants"
                  }
                  className="oe-label"
                >
                  Số lượng tham gia
                </label>
                <input
                  id={isEdit ? "editTargetParticipants" : "targetParticipants"}
                  name="targetParticipants"
                  type="number"
                  className="oe-input"
                  defaultValue={isEdit ? event?.targetParticipants || "" : ""}
                  placeholder="Ví dụ: 100"
                />
              </div>
              <div className="form-group">
                <label
                  htmlFor={isEdit ? "editLocation" : "location"}
                  className="oe-label"
                >
                  Địa điểm
                </label>
                <input
                  id={isEdit ? "editLocation" : "location"}
                  name="location"
                  className="oe-input"
                  defaultValue={isEdit ? event?.location || "" : ""}
                  placeholder="Nhập địa điểm"
                />
              </div>

              {!isEdit && (
                <div className="form-group oe-form-full">
                  <label htmlFor="memberSearch" className="oe-label">
                    Thành viên BTC ban đầu
                  </label>
                  <input
                    id="memberSearch"
                    type="text"
                    className="oe-input"
                    value={memberSearch}
                    onChange={(e) => setMemberSearch(e.target.value)}
                    placeholder="Tìm theo tên, email hoặc phòng ban"
                  />

                  <select
                    id="initialMemberIds"
                    name="initialMemberIds"
                    multiple
                    hidden
                    value={selectedInitialMembers.map((m) => m.id)}
                    readOnly
                  >
                    {selectedInitialMembers.map((member) => (
                      <option key={member.id} value={member.id}>
                        {member.fullName || member.email || member.id}
                      </option>
                    ))}
                  </select>

                  {selectedInitialMembers.length > 0 && (
                    <div className="oe-chip-list">
                      {selectedInitialMembers.map((member) => (
                        <span key={member.id} className="oe-chip">
                          <span>
                            {member.fullName || member.email || member.id}
                          </span>
                          <button
                            type="button"
                            onClick={() => removeInitialMember(member.id)}
                            aria-label="Xóa thành viên đã chọn"
                          >
                            ×
                          </button>
                        </span>
                      ))}
                    </div>
                  )}

                  <div className="oe-table-wrap">
                    <table className="oe-table">
                      <thead>
                        <tr>
                          <th>Họ tên</th>
                          <th>Email</th>
                          <th>Phòng ban</th>
                          <th />
                        </tr>
                      </thead>
                      <tbody>
                        {filteredMembers.map((member) => {
                          const isPicked = selectedInitialMembers.some(
                            (m) => m.id === member.id,
                          );
                          return (
                            <tr key={member.id}>
                              <td>{member.fullName || "-"}</td>
                              <td>{member.email || "-"}</td>
                              <td>
                                {member.department?.departmentName ||
                                  member.department?.deptName ||
                                  "-"}
                              </td>
                              <td>
                                <button
                                  type="button"
                                  className="oe-icon-btn"
                                  disabled={isPicked}
                                  onClick={() => addInitialMember(member)}
                                  title={isPicked ? "Đã chọn" : "Thêm vào BTC"}
                                >
                                  {isPicked ? "✓" : "+"}
                                </button>
                              </td>
                            </tr>
                          );
                        })}
                        {filteredMembers.length === 0 && (
                          <tr>
                            <td className="oe-table-empty" colSpan={4}>
                              Không có thành viên phù hợp.
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}

              <div className="form-group">
                <label
                  htmlFor={isEdit ? "editBannerUrl" : "bannerUrl"}
                  className="oe-label"
                >
                  Liên kết banner
                </label>
                <input
                  id={isEdit ? "editBannerUrl" : "bannerUrl"}
                  name="bannerUrl"
                  className="oe-input"
                  value={bannerValue}
                  onChange={(e) => setBannerValue(e.target.value)}
                  placeholder="Dán liên kết ảnh banner"
                />
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  onChange={handleBannerUpload}
                  hidden
                />
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isUploadingBanner}
                  className="oe-btn oe-btn-outline"
                  style={{ marginTop: "0.5rem" }}
                >
                  {isUploadingBanner ? "Đang tải ảnh..." : "Tải ảnh banner"}
                </button>
              </div>

              <div className="form-group">
                <label
                  htmlFor={isEdit ? "editVisibility" : "visibility"}
                  className="oe-label"
                >
                  Phạm vi hiển thị
                </label>
                <select
                  id={isEdit ? "editVisibility" : "visibility"}
                  name="visibility"
                  defaultValue={
                    isEdit
                      ? normalizeVisibility(event?.visibility || "Private")
                      : "Private"
                  }
                  className="oe-select"
                >
                  <option value="Public">Công khai</option>
                  <option value="Private">Riêng tư</option>
                </select>
              </div>

              <div className="oe-form-actions">
                <button
                  type="button"
                  onClick={closeForms}
                  className="oe-btn oe-btn-outline"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="oe-btn oe-btn-primary"
                >
                  {isSubmitting
                    ? isEdit
                      ? "Đang cập nhật..."
                      : "Đang tạo..."
                    : isEdit
                      ? "Lưu thay đổi"
                      : "Tạo sự kiện"}
                </button>
              </div>
            </form>

            {pendingBannerUrl && (
              <div
                className="oe-upload-modal-backdrop"
                role="dialog"
                aria-modal="true"
              >
                <div className="oe-upload-modal">
                  <h3>Xác nhận chọn ảnh này?</h3>
                  <img
                    src={toAbsoluteMediaUrl(pendingBannerUrl)}
                    alt="Ảnh banner vừa upload"
                  />
                  <p>{pendingBannerUrl}</p>
                  <div className="oe-upload-modal-actions">
                    <button
                      type="button"
                      onClick={rejectPendingBanner}
                      className="oe-btn oe-btn-outline"
                    >
                      Hủy
                    </button>
                    <button
                      type="button"
                      onClick={acceptPendingBanner}
                      className="oe-btn oe-btn-primary"
                    >
                      Đồng ý
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    );
  };

  if (isLoading) {
    return (
      <div className="app-page org-events-page">
        <PageHeader
          title="Danh sách Dự án"
          description="Quản lý tổng quan các sự kiện và dự án của tổ chức."
        />
        <LoadingSpinner message="Đang tải danh sách sự kiện..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page org-events-page">
        <PageHeader
          title="Danh sách Dự án"
          description="Quản lý tổng quan các sự kiện và dự án của tổ chức."
        />
        <ErrorState message={error} />
      </div>
    );
  }

  return (
    <div className="org-events-page">
      <header className="oe-header">
        <div>
          <h1>Danh sách Dự án</h1>
          <p>Quản lý tổng quan các sự kiện và dự án của tổ chức.</p>
        </div>
        {canCreate && (
          <button
            type="button"
            onClick={() => {
              setEditingEvent(null);
              setShowCreateForm(true);
            }}
            className="oe-btn oe-btn-primary"
          >
            + Tạo sự kiện mới
          </button>
        )}
      </header>

      {showCreateForm && canCreate && <EventForm mode="create" />}
      {editingEvent && canManage && (
        <EventForm mode="edit" event={editingEvent} />
      )}

      {events.length === 0 ? (
        <EmptyState message="Chưa có sự kiện nào" />
      ) : (
        <div className="oe-grid">
          {events.map((event) => {
            const eventId = getEventId(event);
            const title = getEventName(event) || "Sự kiện chưa đặt tên";
            const bannerSrc =
              toAbsoluteMediaUrl(
                event?.bannerUrl || event?.coverUrl || event?.avatarUrl,
              ) || DEFAULT_EVENT_BANNER;
            const statusMeta = getStatusMeta(event?.status);
            const isDraft = event?.status === "Draft";

            const { month, day } = parseDateBadge(event?.startDate);
            const timeStr = formatTime(event?.startDate);
            const dateStr = formatDate(event?.startDate);
            const location = event?.location || "Chưa xác định";

            return (
              <div key={eventId} className="oe-card">
                {/* Ảnh Cover & Badges */}
                <div className="oe-card-cover">
                  <img src={bannerSrc} alt={title} />

                  <div className="oe-badge-date">
                    <span className="oe-bd-month">THÁNG {month}</span>
                    <span className="oe-bd-day">{day}</span>
                  </div>

                  <div className={`oe-badge-status ${statusMeta.class}`}>
                    {statusMeta.label}
                  </div>
                </div>

                {/* Nội dung Text */}
                <div className="oe-card-body">
                  <h3 className="oe-card-title">{title}</h3>
                  <p className="oe-card-desc">
                    {event.description || "Không có mô tả."}
                  </p>

                  <div className="oe-card-info-row">
                    <IconClock />
                    <span>
                      {timeStr !== "-" ? `${timeStr} - ${dateStr}` : dateStr}
                    </span>
                  </div>
                  <div className="oe-card-info-row">
                    <IconPin />
                    <span className="truncate-text">{location}</span>
                  </div>
                </div>

                {/* Khối nút bấm (Footer actions) */}
                <div className="oe-card-footer">
                  <button
                    type="button"
                    onClick={() => openWorkspace(event)}
                    className="oe-btn oe-btn-primary oe-btn-full"
                  >
                    <IconBriefcase /> Vào Workspace
                  </button>

                  {canManage && (
                    <div className="oe-card-actions-row">
                      <button
                        type="button"
                        onClick={() => {
                          setShowCreateForm(false);
                          setEditingEvent(event);
                        }}
                        disabled={isSubmitting}
                        className="oe-btn oe-btn-outline oe-btn-flex"
                      >
                        <IconEdit /> Sửa
                      </button>

                      {isDraft && (
                        <button
                          type="button"
                          onClick={() => handlePublishEvent(event)}
                          disabled={isSubmitting}
                          className="oe-btn oe-btn-publish oe-btn-flex"
                        >
                          <IconGlobe /> Công khai
                        </button>
                      )}

                      <button
                        type="button"
                        onClick={() => handleManageEventFromClose(event)}
                        disabled={isSubmitting}
                        className="oe-btn oe-btn-danger oe-btn-icon-only"
                        title="Xóa sự kiện"
                      >
                        <IconTrash />
                      </button>
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default OrgEventsPage;
