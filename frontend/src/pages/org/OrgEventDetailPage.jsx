/**
 * OrgEventDetailPage.jsx - Organization event detail page (EventDetail tree root)
 *
 * UI refactor: split workspace sidebar + Kanban board, giữ nguyên backend API/handlers.
 */

import { useState, useEffect } from "react";
import { useParams, useSearchParams, useNavigate } from "react-router-dom";
import { useOrgContext } from "../../contexts/OrgContext.jsx";
import {
  addEventMembers,
  getEventAttendees,
  getEventById,
  getEventMembers,
  removeEventMember,
  updateEvent,
} from "../../services/eventService.js";
import { getEventMilestones } from "../../services/milestoneService.js";
import { getMilestoneCategories } from "../../services/categoryService.js";
import { getMyPermissions } from "../../services/roleService.js";
import {
  createTask,
  updateTask,
  updateTaskStatus,
  assignTask,
  deleteTask,
} from "../../services/taskService.js";
import { getOrganizationMembers } from "../../services/memberService.js";
import {
  createMilestone,
  updateMilestone,
  deleteMilestone,
} from "../../services/milestoneService.js";
import {
  createCategory,
  updateCategory,
  deleteCategory,
} from "../../services/categoryService.js";
import { reviewAttendeeCheckIn } from "../../services/attendeeService.js";
import PageHeader from "../../components/shared/PageHeader";
import LoadingSpinner from "../../components/shared/LoadingSpinner";
import EmptyState from "../../components/shared/EmptyState";
import ErrorState from "../../components/shared/ErrorState";
import ForbiddenState from "../../components/shared/ForbiddenState";
import EventWorkspacePreview from "../../components/event/EventWorkspacePreview.jsx";

// Import file CSS mới
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
  const [members, setMembers] = useState([]);
  const [attendees, setAttendees] = useState([]);
  const [eventMembers, setEventMembers] = useState([]);
  const [isEventMemberSubmitting, setIsEventMemberSubmitting] = useState(false);
  const [isAddMemberModalOpen, setIsAddMemberModalOpen] = useState(false);
  const [memberSearchKeyword, setMemberSearchKeyword] = useState("");
  const [pendingMemberIds, setPendingMemberIds] = useState([]);
  const [workspaceTab, setWorkspaceTab] = useState("kanban");
  const [myRoleName, setMyRoleName] = useState("");
  const [myOrgMemberId, setMyOrgMemberId] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [taskLoading, setTaskLoading] = useState({});
  const [milestoneLoading, setMilestoneLoading] = useState({});
  const [categoryLoading, setCategoryLoading] = useState({});
  const [showCreateMilestone, setShowCreateMilestone] = useState(false);
  const [showCreateCategory, setShowCreateCategory] = useState({});
  const [editingMilestone, setEditingMilestone] = useState(null);
  const [editingCategory, setEditingCategory] = useState(null);
  const [editingTask, setEditingTask] = useState(null);
  const [isEventUpdating, setIsEventUpdating] = useState(false);
  const [activeCategoryId, setActiveCategoryId] = useState(null);
  const [showCreateTask, setShowCreateTask] = useState(false);
  const [draggedTaskId, setDraggedTaskId] = useState(null);
  const [isRoadmapExpanded, setIsRoadmapExpanded] = useState(true);

  const getEventName = (eventData) => eventData?.name || eventData?.eventName;

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

        const membersData = await getOrganizationMembers(orgId);
        setMembers(membersData);

        const attendeesData = await getEventAttendees(eventId);
        setAttendees(Array.isArray(attendeesData) ? attendeesData : []);

        const eventMembersData = await getEventMembers(eventId);
        setEventMembers(
          Array.isArray(eventMembersData) ? eventMembersData : [],
        );

        const myPermData = await getMyPermissions(orgId);
        setMyRoleName(myPermData?.roleName || "");
        setMyOrgMemberId(myPermData?.memberId || null);
      } catch (err) {
        setError(err.message || "Không thể tải chi tiết sự kiện");
      } finally {
        setIsLoading(false);
      }
    }
    loadEventDetail();
  }, [eventId, orgId, isMember]);

  const refreshEventMemberBoard = async () => {
    const [membersData, eventMembersData] = await Promise.all([
      getOrganizationMembers(orgId),
      getEventMembers(eventId),
    ]);
    setMembers(membersData);
    setEventMembers(Array.isArray(eventMembersData) ? eventMembersData : []);
  };

  if (!eventId || !orgId) {
    return <ErrorState message="Event ID and Organization ID are required" />;
  }

  if (!isMember) {
    return (
      <div className="app-page">
        <PageHeader
          title="Chi tiết sự kiện"
          description="Quản lý mốc tiến độ, hạng mục và công việc của sự kiện"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Chi tiết sự kiện"
          description="Quản lý mốc tiến độ, hạng mục và công việc của sự kiện"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Chi tiết sự kiện"
          description="Quản lý mốc tiến độ, hạng mục và công việc của sự kiện"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  const canManage = permissions.includes("org.events.manage");
  const canEditPreviewByRole = ["president", "vice president"].includes(
    String(myRoleName || "")
      .trim()
      .toLowerCase(),
  );
  const isCurrentUserEventOrganizer = eventMembers.some((item) => {
    const memberId =
      item.memberId || item.organizationMemberId || item.member?.id;
    return !!myOrgMemberId && memberId === myOrgMemberId;
  });
  const canEditPreview = canEditPreviewByRole && isCurrentUserEventOrganizer;
  const canManageEventMembers = isCurrentUserEventOrganizer;
  const eventOrganizerMemberIds = new Set(
    eventMembers
      .map(
        (item) => item.memberId || item.organizationMemberId || item.member?.id,
      )
      .filter(Boolean),
  );
  const assignableEventMembers = members.filter((member) =>
    eventOrganizerMemberIds.has(member.id),
  );

  const getTaskAssigneeId = (task) =>
    task?.assigneeId ||
    task?.assignedMemberId ||
    task?.assignee?.id ||
    task?.assignee?.memberId ||
    task?.assignee?.userId ||
    "";

  const getTaskAssigneeName = (task) => {
    const directName =
      task?.assignee?.user?.fullName ||
      task?.assignee?.fullName ||
      task?.assigneeName;
    if (directName) return directName;

    const assigneeId = getTaskAssigneeId(task);
    if (!assigneeId) return "-";
    const matchedMember = members.find((member) => member.id === assigneeId);
    return matchedMember?.fullName || matchedMember?.email || "-";
  };

  const formatDate = (value) => {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return date.toLocaleDateString("vi-VN");
  };

  const formatShortDate = (value) => {
    if (!value) return "-";
    const formatted = formatDate(value);
    return formatted;
  };

  const formatTime = (value) => {
    if (!value || !String(value).includes("T")) return "-";
    return String(value).split("T")[1].substring(0, 5);
  };

  const getAttendeeCheckInLabel = (status) => {
    const normalized = String(status || "").toLowerCase();
    if (normalized === "checkedin") return "Đã check-in";
    if (normalized === "checkinpending") return "Xác thực check-in";
    if (normalized === "registered") return "Chưa check-in";
    return status || "-";
  };

  const handleReviewCheckIn = async (attendeeId, approve) => {
    if (!canManageEventMembers) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }
    setIsEventMemberSubmitting(true);
    try {
      const updated = await reviewAttendeeCheckIn(attendeeId, approve);
      setAttendees((prev) =>
        prev.map((item) =>
          item.id === attendeeId ? { ...item, ...updated } : item,
        ),
      );
    } catch (err) {
      alert(err.message || "Không thể duyệt check-in");
    } finally {
      setIsEventMemberSubmitting(false);
    }
  };

  const allowedTaskStatuses = ["Todo", "InProgress", "Done"];
  const normalizeTaskStatus = (status) =>
    allowedTaskStatuses.includes(status) ? status : "Todo";
  const statusMeta = {
    Todo: { label: "Cần làm", className: "status-todo" },
    InProgress: { label: "Đang làm", className: "status-progress" },
    Done: { label: "Hoàn thành", className: "status-done" },
  };

  const getStatusLabel = (status) =>
    statusMeta[normalizeTaskStatus(status)]?.label || "Chưa xác định";
  const getStatusClass = (status) =>
    statusMeta[normalizeTaskStatus(status)]?.className || "status-unknown";

  const getPriorityLabel = (priority) => {
    const labels = {
      Low: "THẤP",
      Medium: "TRUNG BÌNH",
      High: "CAO",
      Urgent: "KHẨN CẤP",
    };
    return labels[priority] || priority || "-";
  };

  const getPriorityClass = (priority) => {
    if (priority === "Urgent" || priority === "High") return "priority-high";
    if (priority === "Medium") return "priority-medium";
    return "priority-low";
  };

  const getMemberInitial = (name) => {
    if (!name || name === "-") return "?";
    return String(name).trim().charAt(0).toUpperCase();
  };

  const getAllCategories = () =>
    milestones.flatMap((milestone) =>
      (categoriesByMilestone[milestone.id] || []).map((category) => ({
        ...category,
        milestoneTitle: milestone.title,
        milestoneId: milestone.id,
      })),
    );

  const allCategories = getAllCategories();
  const activeCategory = allCategories.find(
    (category) => category.id === activeCategoryId,
  );
  const activeMilestone = activeCategory
    ? milestones.find(
        (milestone) => milestone.id === activeCategory.milestoneId,
      )
    : null;
  const activeTasks = activeCategory?.tasks || [];

  const statusColumns = [...allowedTaskStatuses];

  const canUpdateTaskStatusByRole = (task) => {
    if (canManage) return true;
    if (!myOrgMemberId) return false;
    return String(getTaskAssigneeId(task)) === String(myOrgMemberId);
  };

  // Task mutation handlers
  const handleCreateTask = async (categoryId, e) => {
    e.preventDefault();
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const form = e.target;
    const taskName = form.taskName.value;
    const description = form.description.value;
    const priority = form.priority.value;
    const deadline = form.deadline.value;

    if (!taskName) {
      alert("Task name is required");
      return;
    }

    setTaskLoading((prev) => ({ ...prev, [categoryId]: true }));
    try {
      const newTask = await createTask(categoryId, {
        taskName,
        description: description || undefined,
        priority,
        deadline: deadline || undefined,
      });

      setCategoriesByMilestone((prev) => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          const categoryIndex = updated[milestoneId].findIndex(
            (c) => c.id === categoryId,
          );
          if (categoryIndex !== -1) {
            updated[milestoneId] = updated[milestoneId].map((cat, idx) =>
              idx === categoryIndex
                ? { ...cat, tasks: [...(cat.tasks || []), newTask] }
                : cat,
            );
            break;
          }
        }
        return updated;
      });

      form.reset();
      setShowCreateTask(false);
    } catch (err) {
      alert(err.message || "Không thể tạo công việc");
    } finally {
      setTaskLoading((prev) => ({ ...prev, [categoryId]: false }));
    }
  };

  const handleUpdateStatus = async (taskId, newStatus, categoryId) => {
    const targetTask = allCategories
      .flatMap((cat) => cat.tasks || [])
      .find((task) => task.id === taskId);
    if (!targetTask || !canUpdateTaskStatusByRole(targetTask)) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    setTaskLoading((prev) => ({ ...prev, [taskId]: true }));
    try {
      const updatedTask = await updateTaskStatus(taskId, {
        status: normalizeTaskStatus(newStatus),
      });

      setCategoriesByMilestone((prev) => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          updated[milestoneId] = updated[milestoneId].map((cat) => ({
            ...cat,
            tasks:
              cat.tasks?.map((task) =>
                task.id === taskId ? updatedTask : task,
              ) || [],
          }));
        }
        return updated;
      });
    } catch (err) {
      alert(err.message || "Không thể cập nhật trạng thái công việc");
    } finally {
      setTaskLoading((prev) => ({ ...prev, [taskId]: false }));
    }
  };

  const handleAssignTask = async (taskId, assigneeId, categoryId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    setTaskLoading((prev) => ({ ...prev, [taskId]: true }));
    try {
      const updatedTask = await assignTask(taskId, {
        assigneeId: assigneeId || null,
        deptId: null,
      });

      setCategoriesByMilestone((prev) => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          updated[milestoneId] = updated[milestoneId].map((cat) => ({
            ...cat,
            tasks:
              cat.tasks?.map((task) =>
                task.id === taskId ? updatedTask : task,
              ) || [],
          }));
        }
        return updated;
      });
    } catch (err) {
      alert(err.message || "Không thể phân công công việc");
    } finally {
      setTaskLoading((prev) => ({ ...prev, [taskId]: false }));
    }
  };

  const handleDeleteTask = async (taskId, categoryId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    if (!window.confirm("Bạn có chắc muốn xóa nhiệm vụ này?")) {
      return;
    }

    setTaskLoading((prev) => ({ ...prev, [taskId]: true }));
    try {
      await deleteTask(taskId);

      setCategoriesByMilestone((prev) => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          updated[milestoneId] = updated[milestoneId].map((cat) => ({
            ...cat,
            tasks: cat.tasks?.filter((task) => task.id !== taskId) || [],
          }));
        }
        return updated;
      });
    } catch (err) {
      alert(err.message || "Không thể xóa công việc");
    } finally {
      setTaskLoading((prev) => ({ ...prev, [taskId]: false }));
    }
  };

  // Milestone mutation handlers
  const handleCreateMilestone = async (e) => {
    e.preventDefault();
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const form = e.target;
    const title = form.title.value;
    const description = form.description.value;
    const orderIndex = milestones.length + 1;

    if (!title) {
      alert("Title is required");
      return;
    }

    setMilestoneLoading((prev) => ({ ...prev, create: true }));
    try {
      const newMilestone = await createMilestone(eventId, {
        title,
        description: description || undefined,
        orderIndex,
      });

      setMilestones((prev) => [...prev, newMilestone]);
      setCategoriesByMilestone((prev) => ({
        ...prev,
        [newMilestone.id]: [],
      }));

      form.reset();
      setShowCreateMilestone(false);
    } catch (err) {
      alert(err.message || "Không thể tạo mốc tiến độ");
    } finally {
      setMilestoneLoading((prev) => ({ ...prev, create: false }));
    }
  };

  const handleDeleteMilestone = async (milestoneId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    if (
      !window.confirm(
        "Xóa milestone này? Tất cả category và task bên trong sẽ bị xóa.",
      )
    ) {
      return;
    }

    setMilestoneLoading((prev) => ({ ...prev, [milestoneId]: true }));
    try {
      await deleteMilestone(milestoneId);

      const deletedCategories = categoriesByMilestone[milestoneId] || [];
      setMilestones((prev) => prev.filter((m) => m.id !== milestoneId));
      setCategoriesByMilestone((prev) => {
        const updated = { ...prev };
        delete updated[milestoneId];
        return updated;
      });
      if (
        deletedCategories.some((category) => category.id === activeCategoryId)
      ) {
        setActiveCategoryId(null);
      }
    } catch (err) {
      alert(err.message || "Không thể xóa mốc tiến độ");
    } finally {
      setMilestoneLoading((prev) => ({ ...prev, [milestoneId]: false }));
    }
  };

  // Category mutation handlers
  const handleCreateCategory = async (milestoneId, e) => {
    e.preventDefault();
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    const form = e.target;
    const categoryName = form.categoryName.value;
    const description = form.description.value;
    const orderIndex = (categoriesByMilestone[milestoneId]?.length || 0) + 1;

    if (!categoryName) {
      alert("Category name is required");
      return;
    }

    setCategoryLoading((prev) => ({ ...prev, [milestoneId]: true }));
    try {
      const newCategory = await createCategory(milestoneId, {
        categoryName,
        description: description || undefined,
        orderIndex,
      });

      setCategoriesByMilestone((prev) => ({
        ...prev,
        [milestoneId]: [
          ...(prev[milestoneId] || []),
          { ...newCategory, tasks: [] },
        ],
      }));

      setActiveCategoryId(newCategory.id);
      form.reset();
      setShowCreateCategory((prev) => ({ ...prev, [milestoneId]: false }));
    } catch (err) {
      alert(err.message || "Không thể tạo hạng mục");
    } finally {
      setCategoryLoading((prev) => ({ ...prev, [milestoneId]: false }));
    }
  };

  const handleDeleteCategory = async (categoryId, milestoneId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    if (!window.confirm("Xóa category này? Tất cả task bên trong sẽ bị xóa.")) {
      return;
    }

    setCategoryLoading((prev) => ({ ...prev, [categoryId]: true }));
    try {
      await deleteCategory(categoryId);

      setCategoriesByMilestone((prev) => ({
        ...prev,
        [milestoneId]:
          prev[milestoneId]?.filter((c) => c.id !== categoryId) || [],
      }));
      if (activeCategoryId === categoryId) {
        setActiveCategoryId(null);
      }
    } catch (err) {
      alert(err.message || "Không thể xóa hạng mục");
    } finally {
      setCategoryLoading((prev) => ({ ...prev, [categoryId]: false }));
    }
  };

  const toIsoUtcFromLocalInput = (value) => {
    if (!value) return null;
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return null;
    return date.toISOString();
  };

  const handleUpdateEventPreview = async (partialDraft) => {
    if (!canEditPreview || !event) {
      alert(
        "Bạn không có quyền sửa sự kiện trong preview. Chỉ BTC của sự kiện mới được sửa.",
      );
      return;
    }
    setIsEventUpdating(true);
    try {
      const nextStartDate =
        partialDraft.startDate !== undefined
          ? toIsoUtcFromLocalInput(partialDraft.startDate)
          : event.startDate;
      const nextEndDate =
        partialDraft.endDate !== undefined
          ? toIsoUtcFromLocalInput(partialDraft.endDate)
          : event.endDate;

      if (partialDraft.startDate !== undefined && !nextStartDate) {
        throw new Error("Ngày bắt đầu không hợp lệ");
      }
      if (partialDraft.endDate !== undefined && !nextEndDate) {
        throw new Error("Ngày kết thúc không hợp lệ");
      }

      const nextTargetRaw =
        partialDraft.targetParticipants !== undefined
          ? partialDraft.targetParticipants
          : event.targetParticipants;

      const nextTarget =
        nextTargetRaw === "" ||
        nextTargetRaw === null ||
        nextTargetRaw === undefined
          ? undefined
          : Number(nextTargetRaw);

      if (nextTarget !== undefined && Number.isNaN(nextTarget)) {
        throw new Error("Số lượng người tham gia không hợp lệ");
      }

      const updated = await updateEvent(eventId, {
        eventName: partialDraft.eventName ?? getEventName(event),
        description:
          partialDraft.description !== undefined
            ? partialDraft.description || undefined
            : event.description || undefined,
        startDate: nextStartDate || event.startDate,
        endDate:
          nextEndDate || event.endDate || nextStartDate || event.startDate,
        location:
          partialDraft.location !== undefined
            ? partialDraft.location || undefined
            : event.location || undefined,
        targetParticipants: nextTarget,
        bannerUrl:
          partialDraft.bannerUrl !== undefined
            ? partialDraft.bannerUrl || undefined
            : event.bannerUrl || undefined,
        visibility: partialDraft.visibility ?? event.visibility ?? "Private",
      });
      setEvent(updated);
    } catch (err) {
      if (
        err?.response?.status === 403 ||
        String(err.message || "").includes("403")
      ) {
        alert(
          "Không đủ quyền cập nhật sự kiện (403). Chỉ thành viên BTC của sự kiện mới được sửa.",
        );
      } else {
        alert(err.message || "Không thể cập nhật sự kiện");
      }
    } finally {
      setIsEventUpdating(false);
    }
  };

  const handleUpdateMilestone = async (milestoneId, e) => {
    e.preventDefault();
    if (!canManage) return;

    const form = e.target;
    const title = form.title.value;
    const description = form.description.value;
    const currentMilestone = milestones.find((m) => m.id === milestoneId);
    const status = currentMilestone?.status || "Planned";

    setMilestoneLoading((prev) => ({ ...prev, [milestoneId]: true }));
    try {
      const updated = await updateMilestone(milestoneId, {
        title,
        description: description || undefined,
        status,
      });

      setMilestones((prev) =>
        prev.map((m) =>
          m.id === milestoneId
            ? { ...m, title: updated.title, description: updated.description }
            : m,
        ),
      );
      setEditingMilestone(null);
    } catch (err) {
      alert(err.message || "Không thể cập nhật mốc tiến độ");
    } finally {
      setMilestoneLoading((prev) => ({ ...prev, [milestoneId]: false }));
    }
  };

  const handleUpdateCategory = async (categoryId, milestoneId, e) => {
    e.preventDefault();
    if (!canManage) return;

    const form = e.target;
    const categoryName = form.categoryName.value;
    const description = form.description.value;

    setCategoryLoading((prev) => ({ ...prev, [categoryId]: true }));
    try {
      const updated = await updateCategory(categoryId, {
        categoryName,
        description: description || undefined,
      });

      setCategoriesByMilestone((prev) => ({
        ...prev,
        [milestoneId]: prev[milestoneId].map((c) =>
          c.id === categoryId
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
      alert(err.message || "Không thể cập nhật hạng mục");
    } finally {
      setCategoryLoading((prev) => ({ ...prev, [categoryId]: false }));
    }
  };

  const handleUpdateTask = async (taskId, e) => {
    e.preventDefault();
    if (!canManage) return;

    const form = e.target;
    const taskName = form.taskName.value;
    const description = form.description.value;
    const priority = form.priority.value;
    const deadline = form.deadline.value;

    setTaskLoading((prev) => ({ ...prev, [taskId]: true }));
    try {
      const updatedTask = await updateTask(taskId, {
        taskName,
        description: description || undefined,
        priority,
        deadline: deadline || undefined,
      });

      setCategoriesByMilestone((prev) => {
        const updated = { ...prev };
        for (const mId in updated) {
          updated[mId] = updated[mId].map((cat) => ({
            ...cat,
            tasks:
              cat.tasks?.map((task) =>
                task.id === taskId
                  ? {
                      ...task,
                      taskName: updatedTask.taskName,
                      description: updatedTask.description,
                      priority: updatedTask.priority,
                      deadline: updatedTask.deadline,
                    }
                  : task,
              ) || [],
          }));
        }
        return updated;
      });
      setEditingTask(null);
    } catch (err) {
      alert(err.message || "Không thể cập nhật công việc");
    } finally {
      setTaskLoading((prev) => ({ ...prev, [taskId]: false }));
    }
  };

  const handleTaskDrop = (status) => {
    if (!draggedTaskId || !activeCategory) return;
    const draggedTask = activeTasks.find((task) => task.id === draggedTaskId);
    if (!draggedTask || !canUpdateTaskStatusByRole(draggedTask)) {
      setDraggedTaskId(null);
      return;
    }
    if (
      normalizeTaskStatus(draggedTask?.status) !== normalizeTaskStatus(status)
    ) {
      handleUpdateStatus(draggedTaskId, status, activeCategory.id);
    }
    setDraggedTaskId(null);
  };

  const handleConfirmAddEventMembers = async () => {
    if (!canManageEventMembers) {
      alert("Chỉ thành viên BTC của sự kiện mới có thể thêm người vào BTC.");
      return;
    }
    if (pendingMemberIds.length === 0) {
      alert("Vui lòng chọn ít nhất 1 thành viên.");
      return;
    }

    setIsEventMemberSubmitting(true);
    try {
      await addEventMembers(eventId, { memberIds: pendingMemberIds });
      setPendingMemberIds([]);
      setMemberSearchKeyword("");
      setIsAddMemberModalOpen(false);
      await refreshEventMemberBoard();
    } catch (err) {
      alert(err.message || "Không thể thêm thành viên BTC");
    } finally {
      setIsEventMemberSubmitting(false);
    }
  };

  const handleRemoveEventMember = async (eventMemberId) => {
    if (!canManageEventMembers) {
      alert("Chỉ thành viên BTC của sự kiện mới có thể xóa thành viên BTC.");
      return;
    }
    if (!window.confirm("Xóa thành viên này khỏi BTC sự kiện?")) return;

    setIsEventMemberSubmitting(true);
    try {
      await removeEventMember(eventMemberId);
      setEventMembers((prev) =>
        prev.filter((item) => item.id !== eventMemberId),
      );
      await refreshEventMemberBoard();
    } catch (err) {
      alert(err.message || "Không thể xóa thành viên BTC");
    } finally {
      setIsEventMemberSubmitting(false);
    }
  };

  const Sidebar = () => (
    <aside className="event-sidebar">
      <div className="event-sidebar-header">
        <div className="event-title-row">
          <h1>{getEventName(event) || "Chi tiết sự kiện"}</h1>
          <button
            type="button"
            onClick={() => setWorkspaceTab("preview")}
            className="sidebar-eye-button"
            title="Xem trước & sửa sự kiện"
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
              <circle cx="12" cy="12" r="3"></circle>
            </svg>
          </button>
        </div>

        <div className="event-date-badge">
          <svg
            width="15"
            height="15"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
            <line x1="16" y1="2" x2="16" y2="6"></line>
            <line x1="8" y1="2" x2="8" y2="6"></line>
            <line x1="3" y1="10" x2="21" y2="10"></line>
          </svg>
          <span>{formatDate(event?.startDate)}</span>
        </div>

        <div className="workspace-tab-list">
          <button
            type="button"
            className={`workspace-tab-btn ${workspaceTab === "kanban" ? "active" : ""}`}
            onClick={() => setWorkspaceTab("kanban")}
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="12" cy="12" r="10"></circle>
              <circle cx="12" cy="12" r="6"></circle>
              <circle cx="12" cy="12" r="2"></circle>
            </svg>
            Lộ trình & Công việc
          </button>
          <button
            type="button"
            className={`workspace-tab-btn ${workspaceTab === "event-members" ? "active" : ""}`}
            onClick={() => setWorkspaceTab("event-members")}
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
              <circle cx="9" cy="7" r="4"></circle>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
            </svg>
            Ban tổ chức
          </button>
          <button
            type="button"
            className={`workspace-tab-btn ${workspaceTab === "attendees" ? "active" : ""}`}
            onClick={() => setWorkspaceTab("attendees")}
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
              <circle cx="9" cy="7" r="4"></circle>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
            </svg>
            Danh sách tham gia
          </button>
        </div>
      </div>

      <div className="event-roadmap">
        <div
          className="roadmap-heading roadmap-heading-clickable"
          onClick={() => setWorkspaceTab("kanban")}
          role="button"
          tabIndex={0}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              e.preventDefault();
              setWorkspaceTab("kanban");
            }
          }}
        >
          <div className="roadmap-heading-left">
            <button
              type="button"
              className="roadmap-toggle"
              onClick={(e) => {
                e.stopPropagation();
                setWorkspaceTab("kanban");
                setIsRoadmapExpanded((prev) => !prev);
              }}
              aria-label={
                isRoadmapExpanded
                  ? "Thu gọn tiến độ dự án"
                  : "Mở rộng tiến độ dự án"
              }
            >
              <svg
                width="18"
                height="18"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                style={{
                  transform: isRoadmapExpanded
                    ? "rotate(0deg)"
                    : "rotate(-90deg)",
                  transition: "transform 0.2s",
                }}
              >
                <polyline points="6 9 12 15 18 9"></polyline>
              </svg>
            </button>
            <span>TIẾN ĐỘ DỰ ÁN</span>
          </div>
          <div className="roadmap-heading-actions">
            {canManage && (
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  setShowCreateMilestone(true);
                }}
                className="roadmap-add-button"
                title="Thêm lộ trình"
              >
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <line x1="12" y1="5" x2="12" y2="19"></line>
                  <line x1="5" y1="12" x2="19" y2="12"></line>
                </svg>
              </button>
            )}
          </div>
        </div>

        {false && isRoadmapExpanded && showCreateMilestone && canManage && (
          <form onSubmit={handleCreateMilestone} className="sidebar-form">
            <input
              name="title"
              placeholder="Tên giai đoạn *"
              required
              className="sidebar-input"
            />
            <input
              name="description"
              placeholder="Mô tả"
              className="sidebar-input"
            />
            <div className="sidebar-form-actions">
              <button
                type="submit"
                disabled={milestoneLoading.create}
                className="mini-primary-button"
              >
                {milestoneLoading.create ? "Đang tạo..." : "Tạo"}
              </button>
              <button
                type="button"
                onClick={() => setShowCreateMilestone(false)}
                className="mini-ghost-button"
              >
                Hủy
              </button>
            </div>
          </form>
        )}

        {!isRoadmapExpanded ? null : milestones.length === 0 ? (
          <div className="sidebar-empty">
            <EmptyState message="Chưa có mốc tiến độ nào" />
          </div>
        ) : (
          <div className="milestone-list">
            {milestones.map((milestone) => {
              const milestoneCategories =
                categoriesByMilestone[milestone.id] || [];
              return (
                <section key={milestone.id} className="milestone-block">
                  <div className="milestone-header">
                    <button
                      type="button"
                      className="milestone-title-button"
                      onClick={() => {
                        setWorkspaceTab("kanban");
                        if (milestoneCategories[0])
                          setActiveCategoryId(milestoneCategories[0].id);
                      }}
                    >
                      <span className="flag-icon">
                        <svg
                          width="16"
                          height="16"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2.5"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        >
                          <path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z"></path>
                          <line x1="4" y1="22" x2="4" y2="15"></line>
                        </svg>
                      </span>
                      <span>{milestone.title || "-"}</span>
                    </button>
                    {canManage && (
                      <div className="milestone-actions">
                        <button
                          type="button"
                          onClick={() =>
                            setEditingMilestone(
                              editingMilestone === milestone.id
                                ? null
                                : milestone.id,
                            )
                          }
                          className="sidebar-icon-button"
                          title="Sửa giai đoạn"
                        >
                          ✎
                        </button>
                        <button
                          type="button"
                          onClick={() => handleDeleteMilestone(milestone.id)}
                          disabled={milestoneLoading[milestone.id]}
                          className="sidebar-icon-button danger"
                          title="Xóa giai đoạn"
                        >
                          ×
                        </button>
                      </div>
                    )}
                  </div>

                  {false && editingMilestone === milestone.id && canManage && (
                    <form
                      onSubmit={(e) => handleUpdateMilestone(milestone.id, e)}
                      className="sidebar-form nested-form"
                    >
                      <input
                        name="title"
                        defaultValue={milestone.title}
                        required
                        className="sidebar-input"
                      />
                      <input
                        name="description"
                        defaultValue={milestone.description || ""}
                        placeholder="Mô tả"
                        className="sidebar-input"
                      />
                      <div className="sidebar-form-actions">
                        <button
                          type="submit"
                          disabled={milestoneLoading[milestone.id]}
                          className="mini-primary-button"
                        >
                          {milestoneLoading[milestone.id]
                            ? "Đang lưu..."
                            : "Lưu"}
                        </button>
                        <button
                          type="button"
                          onClick={() => setEditingMilestone(null)}
                          className="mini-ghost-button"
                        >
                          Hủy
                        </button>
                      </div>
                    </form>
                  )}

                  <div className="category-tree">
                    {milestoneCategories.length === 0 && (
                      <div className="category-empty">Chưa có hạng mục.</div>
                    )}
                    {milestoneCategories.map((category) => (
                      <div key={category.id} className="category-row-wrap">
                        <div className="category-row">
                          <button
                            type="button"
                            onClick={() => {
                              setWorkspaceTab("kanban");
                              setActiveCategoryId(category.id);
                              setShowCreateTask(false);
                            }}
                            className={`category-button ${activeCategoryId === category.id ? "active" : ""}`}
                          >
                            {category.categoryName || "-"}
                          </button>
                          {canManage && (
                            <div className="category-actions">
                              <button
                                type="button"
                                onClick={() =>
                                  setEditingCategory(
                                    editingCategory === category.id
                                      ? null
                                      : category.id,
                                  )
                                }
                                className="sidebar-icon-button"
                                title="Sửa hạng mục"
                              >
                                ✎
                              </button>
                              <button
                                type="button"
                                onClick={() =>
                                  handleDeleteCategory(
                                    category.id,
                                    milestone.id,
                                  )
                                }
                                disabled={categoryLoading[category.id]}
                                className="sidebar-icon-button danger"
                                title="Xóa hạng mục"
                              >
                                ×
                              </button>
                            </div>
                          )}
                        </div>

                        {false && editingCategory === category.id && canManage && (
                          <form
                            onSubmit={(e) =>
                              handleUpdateCategory(category.id, milestone.id, e)
                            }
                            className="sidebar-form nested-form category-edit-form"
                          >
                            <input
                              name="categoryName"
                              defaultValue={category.categoryName}
                              required
                              className="sidebar-input"
                            />
                            <input
                              name="description"
                              defaultValue={category.description || ""}
                              placeholder="Mô tả"
                              className="sidebar-input"
                            />
                            <div className="sidebar-form-actions">
                              <button
                                type="submit"
                                disabled={categoryLoading[category.id]}
                                className="mini-primary-button"
                              >
                                {categoryLoading[category.id]
                                  ? "Đang lưu..."
                                  : "Lưu"}
                              </button>
                              <button
                                type="button"
                                onClick={() => setEditingCategory(null)}
                                className="mini-ghost-button"
                              >
                                Hủy
                              </button>
                            </div>
                          </form>
                        )}
                      </div>
                    ))}

                    {false && showCreateCategory[milestone.id] && canManage ? (
                      <form
                        onSubmit={(e) => handleCreateCategory(milestone.id, e)}
                        className="sidebar-form category-create-form"
                      >
                        <input
                          name="categoryName"
                          placeholder="Tên hạng mục *"
                          required
                          className="sidebar-input"
                        />
                        <input
                          name="description"
                          placeholder="Mô tả"
                          className="sidebar-input"
                        />
                        <div className="sidebar-form-actions">
                          <button
                            type="submit"
                            disabled={categoryLoading[milestone.id]}
                            className="mini-primary-button"
                          >
                            {categoryLoading[milestone.id]
                              ? "Đang tạo..."
                              : "Tạo"}
                          </button>
                          <button
                            type="button"
                            onClick={() =>
                              setShowCreateCategory((prev) => ({
                                ...prev,
                                [milestone.id]: false,
                              }))
                            }
                            className="mini-ghost-button"
                          >
                            Hủy
                          </button>
                        </div>
                      </form>
                    ) : (
                      canManage && (
                        <button
                          type="button"
                          onClick={() =>
                            setShowCreateCategory((prev) => ({
                              ...prev,
                              [milestone.id]: true,
                            }))
                          }
                          className="add-category-button"
                        >
                          <svg
                            width="15"
                            height="15"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2.5"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                          >
                            <line x1="12" y1="5" x2="12" y2="19"></line>
                            <line x1="5" y1="12" x2="19" y2="12"></line>
                          </svg>
                          Thêm hạng mục
                        </button>
                      )
                    )}
                  </div>
                </section>
              );
            })}
          </div>
        )}
      </div>
    </aside>
  );

  const TaskCard = ({ task }) => {
    const assigneeName = getTaskAssigneeName(task);

    const canUpdateStatus = canUpdateTaskStatusByRole(task);
    return (
      <article
        className={`task-card ${taskLoading[task.id] ? "is-loading" : ""}`}
        draggable={canUpdateStatus && !taskLoading[task.id]}
        onDragStart={() => setDraggedTaskId(task.id)}
        onDragEnd={() => setDraggedTaskId(null)}
      >
        <div className="task-card-top">
          <span className={`priority-badge ${getPriorityClass(task.priority)}`}>
            {getPriorityLabel(task.priority)}
          </span>
          {canManage && (
            <div className="task-card-actions">
              <button
                type="button"
                onClick={() => setEditingTask(task.id)}
                className="task-icon-button"
                title="Sửa task"
              >
                ✎
              </button>
              <button
                type="button"
                onClick={() => handleDeleteTask(task.id, activeCategory.id)}
                disabled={taskLoading[task.id]}
                className="task-icon-button danger"
                title="Xóa task"
              >
                ×
              </button>
            </div>
          )}
        </div>

        <h3>{task.taskName || "-"}</h3>
        {task.description && (
          <p className="task-description">{task.description}</p>
        )}

        <div className="task-status-row">
          <span className="task-status-text">
            {getStatusLabel(task.status)}
          </span>
        </div>

        <div className="task-footer">
          <div className="assignee-wrap">
            <span className="avatar-dot">{getMemberInitial(assigneeName)}</span>
            {canManage ? (
              <select
                value={getTaskAssigneeId(task)}
                onChange={(e) =>
                  handleAssignTask(task.id, e.target.value, activeCategory.id)
                }
                disabled={taskLoading[task.id]}
                className="assignee-select"
              >
                <option value="">Unassigned</option>
                {assignableEventMembers.map((member) => (
                  <option key={member.id} value={member.id}>
                    {member.fullName || member.email}
                  </option>
                ))}
              </select>
            ) : (
              <span className="assignee-name">{assigneeName}</span>
            )}
          </div>
          {task.deadline && (
            <span className="deadline-text">
              ◷ {formatShortDate(task.deadline)}
            </span>
          )}
        </div>
      </article>
    );
  };

  const WorkspaceEmptyState = () => (
    <div className="workspace-empty-state">
      <div className="workspace-empty-icon">▦</div>
      <h2>Không gian làm việc</h2>
      <p>
        Vui lòng chọn một Hạng mục công việc ở thanh bên trái để mở Bảng điều
        phối (Kanban) hoặc khởi tạo lộ trình mới cho dự án.
      </p>
    </div>
  );

  const KanbanBoard = () => {
    if (!activeCategory) return <WorkspaceEmptyState />;

    return (
      <div className="workspace-main-content">
        <header className="kanban-header">
          <div>
            <div className="breadcrumb-line">
              <span>
                {activeMilestone?.title ||
                  activeCategory.milestoneTitle ||
                  "Giai đoạn"}
              </span>
              <span>›</span>
              <span>Hạng mục</span>
            </div>
            <h2>{activeCategory.categoryName || "-"}</h2>
            {activeCategory.description && <p>{activeCategory.description}</p>}
          </div>
          {canManage && (
            <button
              type="button"
              onClick={() => setShowCreateTask(true)}
              className="workspace-button primary"
            >
              + Thêm tác vụ
            </button>
          )}
        </header>

        <section className="kanban-scroll" aria-label="Bảng Kanban">
          <div className="kanban-board">
            {statusColumns.map((status) => {
              const columnTasks = activeTasks.filter(
                (task) => normalizeTaskStatus(task.status) === status,
              );
              return (
                <div
                  key={status}
                  className="kanban-column"
                  onDragOver={(e) => {
                    const draggedTask = activeTasks.find(
                      (task) => task.id === draggedTaskId,
                    );
                    if (draggedTask && canUpdateTaskStatusByRole(draggedTask))
                      e.preventDefault();
                  }}
                  onDrop={() => handleTaskDrop(status)}
                >
                  <div className="kanban-column-header">
                    <h3>
                      <span
                        className={`status-dot ${getStatusClass(status)}`}
                      ></span>
                      {getStatusLabel(status)}
                    </h3>
                    <span className="task-count">{columnTasks.length}</span>
                  </div>

                  <div className="kanban-column-body">
                    {columnTasks.length === 0 ? (
                      <div className="column-empty">Kéo tác vụ vào đây</div>
                    ) : (
                      columnTasks.map((task) => (
                        <TaskCard key={task.id} task={task} />
                      ))
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </section>
      </div>
    );
  };

  const EventPreviewBoard = () => (
    <div className="workspace-main-content preview-mode">
      <header className="kanban-header">
        <div>
          <div className="breadcrumb-line">
            <span>Event</span>
            <span>›</span>
            <span>Xem trước</span>
          </div>
          <h2>{getEventName(event) || "-"}</h2>
          <p>
            {canEditPreview
              ? "Bạn có quyền chỉnh sửa trực tiếp trong preview."
              : "Bạn có quyền xem preview. Chỉ Vice President/President thuộc BTC sự kiện mới được sửa."}
          </p>
        </div>
      </header>
      <div className="workspace-preview-shell">
        <EventWorkspacePreview
          eventData={event}
          canEdit={canEditPreview}
          isSaving={isEventUpdating}
          onSave={handleUpdateEventPreview}
        />
      </div>
    </div>
  );

  const EventMembersBoard = () => {
    const existingMemberIds = new Set(
      eventMembers.map(
        (item) => item.memberId || item.organizationMemberId || item.userId,
      ),
    );
    const memberMap = new Map(members.map((m) => [m.id, m]));
    const selectableMembers = members.filter(
      (m) => !existingMemberIds.has(m.id),
    );
    const normalizedKeyword = memberSearchKeyword.trim().toLowerCase();
    const filteredSelectableMembers = selectableMembers.filter((m) => {
      if (!normalizedKeyword) return true;
      const fullName = String(
        m.fullName || m.user?.fullName || "",
      ).toLowerCase();
      const email = String(m.email || m.user?.email || "").toLowerCase();
      const dept = String(
        m.department?.departmentName || m.department?.deptName || "",
      ).toLowerCase();
      return (
        fullName.includes(normalizedKeyword) ||
        email.includes(normalizedKeyword) ||
        dept.includes(normalizedKeyword)
      );
    });
    const pendingMembers = pendingMemberIds
      .map((id) => members.find((m) => m.id === id))
      .filter(Boolean);
    const addPendingMember = (memberId) => {
      setPendingMemberIds((prev) =>
        prev.includes(memberId) ? prev : [...prev, memberId],
      );
    };
    const removePendingMember = (memberId) => {
      setPendingMemberIds((prev) => prev.filter((id) => id !== memberId));
    };

    return (
      <div className="workspace-main-content">
        <header className="kanban-header">
          <div>
            <h2>Ban tổ chức sự kiện</h2>
            <p>Danh sách thành viên tổ chức sự kiện trong workspace.</p>
          </div>
          <button
            type="button"
            className="workspace-button primary"
            onClick={() => setIsAddMemberModalOpen(true)}
            disabled={!canManageEventMembers || isEventMemberSubmitting}
          >
            Thêm thành viên BTC
          </button>
        </header>
        <div className="workspace-table-wrap">
          <table className="workspace-table">
            <thead>
              <tr>
                <th>Họ tên</th>
                <th>Email</th>
                <th>Số điện thoại</th>
                <th>Phòng ban</th>
                <th>Tham gia từ</th>
                <th>Hành động</th>
              </tr>
            </thead>
            <tbody>
              {eventMembers.length === 0 ? (
                <tr>
                  <td colSpan={6}>Chưa có thành viên BTC.</td>
                </tr>
              ) : (
                eventMembers.map((item) => (
                  <tr key={item.id}>
                    <td>{item.fullName || "-"}</td>
                    <td>{item.email || "-"}</td>
                    <td>
                      {memberMap.get(item.memberId)?.phoneNumber ||
                        memberMap.get(item.memberId)?.phone ||
                        memberMap.get(item.memberId)?.user?.phoneNumber ||
                        "-"}
                    </td>
                    <td>
                      {memberMap.get(item.memberId)?.department
                        ?.departmentName ||
                        memberMap.get(item.memberId)?.department?.deptName ||
                        "-"}
                    </td>
                    <td>
                      {formatDate(item.assignedAtUtc || item.createdAtUtc)}
                    </td>
                    <td>
                      {canManageEventMembers && (
                        <button
                          type="button"
                          className="mini-ghost-button"
                          onClick={() => handleRemoveEventMember(item.id)}
                          disabled={isEventMemberSubmitting}
                        >
                          Xóa
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        {isAddMemberModalOpen && (
          <div className="workspace-modal-backdrop" role="presentation">
            <div
              className="workspace-modal workspace-member-modal"
              role="dialog"
              aria-modal="true"
              aria-label="Thêm thành viên BTC"
              onKeyDown={(e) => e.stopPropagation()}
              onKeyUp={(e) => e.stopPropagation()}
            >
              <div className="workspace-modal-header">
                <div>
                  <p className="workspace-eyebrow">Event Organizers</p>
                  <h2>Thêm thành viên vào BTC</h2>
                </div>
                <button
                  type="button"
                  className="modal-close-button"
                  onClick={() => {
                    setIsAddMemberModalOpen(false);
                    setPendingMemberIds([]);
                    setMemberSearchKeyword("");
                  }}
                  aria-label="Đóng"
                >
                  ×
                </button>
              </div>

              <input
                type="text"
                className="workspace-input"
                placeholder="Tìm theo tên, email, phòng ban..."
                value={memberSearchKeyword}
                onChange={(e) => setMemberSearchKeyword(e.target.value)}
                autoFocus
                onKeyDown={(e) => e.stopPropagation()}
                onKeyUp={(e) => e.stopPropagation()}
              />

              {pendingMembers.length > 0 && (
                <div className="member-chip-list">
                  {pendingMembers.map((member) => (
                    <span key={member.id} className="member-chip">
                      <span className="member-chip-avatar">
                        {(member.fullName || member.email || "?")
                          .charAt(0)
                          .toUpperCase()}
                      </span>
                      <span>{member.fullName || member.email || "-"}</span>
                      <button
                        type="button"
                        onClick={() => removePendingMember(member.id)}
                        aria-label="Bỏ chọn"
                      >
                        ×
                      </button>
                    </span>
                  ))}
                </div>
              )}

              <div className="member-pick-list">
                {filteredSelectableMembers.length === 0 ? (
                  <p className="member-pick-empty">
                    Không có thành viên khả dụng.
                  </p>
                ) : (
                  filteredSelectableMembers.map((member) => {
                    const dept =
                      member.department?.departmentName ||
                      member.department?.deptName ||
                      "Chưa có phòng ban";
                    return (
                      <div key={member.id} className="member-pick-item">
                        <div className="member-pick-main">
                          <span className="member-pick-avatar">
                            {(member.fullName || member.email || "?")
                              .charAt(0)
                              .toUpperCase()}
                          </span>
                          <div>
                            <strong>
                              {member.fullName || member.email || "-"}
                            </strong>
                            <span>{dept}</span>
                          </div>
                        </div>
                        <button
                          type="button"
                          className="member-pick-add"
                          onClick={() => addPendingMember(member.id)}
                          disabled={pendingMemberIds.includes(member.id)}
                        >
                          +
                        </button>
                      </div>
                    );
                  })
                )}
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="workspace-button ghost"
                  onClick={() => {
                    setIsAddMemberModalOpen(false);
                    setPendingMemberIds([]);
                    setMemberSearchKeyword("");
                  }}
                >
                  Hủy
                </button>
                <button
                  type="button"
                  className="workspace-button primary"
                  onClick={handleConfirmAddEventMembers}
                  disabled={
                    isEventMemberSubmitting || pendingMemberIds.length === 0
                  }
                >
                  {isEventMemberSubmitting ? "Đang thêm..." : "Thêm"}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    );
  };

  const AttendeesBoard = () => (
    <div className="workspace-main-content">
      <header className="kanban-header">
        <div>
          <h2>Danh sách người tham gia</h2>
          <p>Danh sách attendee đã ghi danh sự kiện.</p>
        </div>
      </header>
      <div className="workspace-table-wrap">
        <table className="workspace-table">
          <thead>
            <tr>
              <th>Họ tên</th>
              <th>Email</th>
              <th>Số điện thoại</th>
              <th>Thời gian đăng ký</th>
              <th>Trạng thái check-in</th>
              <th>Hành động</th>
            </tr>
          </thead>
          <tbody>
            {attendees.length === 0 ? (
              <tr>
                <td colSpan={6}>Chưa có người tham gia.</td>
              </tr>
            ) : (
              attendees.map((attendee) => (
                <tr key={attendee.id}>
                  <td>{attendee.fullName || "-"}</td>
                  <td>{attendee.email || "-"}</td>
                  <td>{attendee.phoneNumber || "-"}</td>
                  <td>
                    {formatDate(
                      attendee.registeredAtUtc || attendee.createdAtUtc,
                    )}
                  </td>
                  <td>{getAttendeeCheckInLabel(attendee.status)}</td>
                  <td>
                    {String(attendee.status || "") === "CheckInPending" &&
                    canManageEventMembers ? (
                      <div className="attendee-action-group">
                        <button
                          type="button"
                          className="mini-primary-button"
                          onClick={() => handleReviewCheckIn(attendee.id, true)}
                          disabled={isEventMemberSubmitting}
                        >
                          Duyệt
                        </button>
                        <button
                          type="button"
                          className="mini-ghost-button"
                          onClick={() =>
                            handleReviewCheckIn(attendee.id, false)
                          }
                          disabled={isEventMemberSubmitting}
                        >
                          Từ chối
                        </button>
                      </div>
                    ) : (
                      "-"
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );

  const WorkspaceModalFrame = ({ title, children, onClose }) => (
    <div className="workspace-modal-backdrop" role="presentation">
      <div
        className="workspace-modal workspace-form-modal"
        role="dialog"
        aria-modal="true"
        aria-label={title}
        onKeyDown={(e) => e.stopPropagation()}
        onKeyUp={(e) => e.stopPropagation()}
      >
        <div className="workspace-modal-header">
          <div>
            <h2>{title}</h2>
          </div>
          <button
            type="button"
            className="modal-close-button"
            onClick={onClose}
            aria-label="Đóng"
          >
            ×
          </button>
        </div>
        {children}
      </div>
    </div>
  );

  const WorkspaceFormModals = () => {
    const createCategoryMilestoneId = Object.keys(showCreateCategory).find(
      (milestoneId) => showCreateCategory[milestoneId],
    );
    const editingMilestoneData = milestones.find(
      (milestone) => milestone.id === editingMilestone,
    );
    const editingCategoryData = allCategories.find(
      (category) => category.id === editingCategory,
    );
    const editingTaskData = allCategories
      .flatMap((category) =>
        (category.tasks || []).map((task) => ({
          ...task,
          __categoryId: category.id,
        })),
      )
      .find((task) => task.id === editingTask);

    if (showCreateMilestone && canManage) {
      return (
        <WorkspaceModalFrame
          title="Tạo giai đoạn dự án"
          onClose={() => setShowCreateMilestone(false)}
        >
          <form onSubmit={handleCreateMilestone} className="workspace-modal-form">
            <div className="workspace-form-grid">
              <label className="workspace-form-field">
                <span>Tên giai đoạn *</span>
                <input
                  name="title"
                  placeholder="VD: Chuẩn bị"
                  required
                  className="workspace-input"
                  autoFocus
                />
              </label>
              <label className="workspace-form-field">
                <span>Mô tả</span>
                <textarea
                  name="description"
                  placeholder="Mục tiêu hoặc ghi chú của giai đoạn"
                  className="workspace-input workspace-textarea"
                />
              </label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                onClick={() => setShowCreateMilestone(false)}
                className="workspace-button ghost"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={milestoneLoading.create}
                className="workspace-button primary"
              >
                {milestoneLoading.create ? "Đang tạo..." : "Tạo"}
              </button>
            </div>
          </form>
        </WorkspaceModalFrame>
      );
    }

    if (editingMilestoneData && canManage) {
      return (
        <WorkspaceModalFrame
          title="Sửa giai đoạn"
          onClose={() => setEditingMilestone(null)}
        >
          <form
            onSubmit={(e) => handleUpdateMilestone(editingMilestoneData.id, e)}
            className="workspace-modal-form"
          >
            <div className="workspace-form-grid">
              <label className="workspace-form-field">
                <span>Tên giai đoạn *</span>
                <input
                  name="title"
                  defaultValue={editingMilestoneData.title}
                  required
                  className="workspace-input"
                  autoFocus
                />
              </label>
              <label className="workspace-form-field">
                <span>Mô tả</span>
                <textarea
                  name="description"
                  defaultValue={editingMilestoneData.description || ""}
                  placeholder="Mô tả"
                  className="workspace-input workspace-textarea"
                />
              </label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                onClick={() => setEditingMilestone(null)}
                className="workspace-button ghost"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={milestoneLoading[editingMilestoneData.id]}
                className="workspace-button primary"
              >
                {milestoneLoading[editingMilestoneData.id]
                  ? "Đang lưu..."
                  : "Lưu"}
              </button>
            </div>
          </form>
        </WorkspaceModalFrame>
      );
    }

    if (createCategoryMilestoneId && canManage) {
      return (
        <WorkspaceModalFrame
          title="Thêm hạng mục"
          onClose={() => setShowCreateCategory({})}
        >
          <form
            onSubmit={(e) => handleCreateCategory(createCategoryMilestoneId, e)}
            className="workspace-modal-form"
          >
            <div className="workspace-form-grid">
              <label className="workspace-form-field">
                <span>Tên hạng mục *</span>
                <input
                  name="categoryName"
                  placeholder="VD: Hậu cần, Truyền thông..."
                  required
                  className="workspace-input"
                  autoFocus
                />
              </label>
              <label className="workspace-form-field">
                <span>Mô tả</span>
                <textarea
                  name="description"
                  placeholder="Mô tả ngắn cho hạng mục"
                  className="workspace-input workspace-textarea"
                />
              </label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                onClick={() => setShowCreateCategory({})}
                className="workspace-button ghost"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={categoryLoading[createCategoryMilestoneId]}
                className="workspace-button primary"
              >
                {categoryLoading[createCategoryMilestoneId]
                  ? "Đang tạo..."
                  : "Tạo"}
              </button>
            </div>
          </form>
        </WorkspaceModalFrame>
      );
    }

    if (editingCategoryData && canManage) {
      return (
        <WorkspaceModalFrame
          title="Sửa hạng mục"
          onClose={() => setEditingCategory(null)}
        >
          <form
            onSubmit={(e) =>
              handleUpdateCategory(
                editingCategoryData.id,
                editingCategoryData.milestoneId,
                e,
              )
            }
            className="workspace-modal-form"
          >
            <div className="workspace-form-grid">
              <label className="workspace-form-field">
                <span>Tên hạng mục *</span>
                <input
                  name="categoryName"
                  defaultValue={editingCategoryData.categoryName}
                  required
                  className="workspace-input"
                  autoFocus
                />
              </label>
              <label className="workspace-form-field">
                <span>Mô tả</span>
                <textarea
                  name="description"
                  defaultValue={editingCategoryData.description || ""}
                  placeholder="Mô tả"
                  className="workspace-input workspace-textarea"
                />
              </label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                onClick={() => setEditingCategory(null)}
                className="workspace-button ghost"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={categoryLoading[editingCategoryData.id]}
                className="workspace-button primary"
              >
                {categoryLoading[editingCategoryData.id]
                  ? "Đang lưu..."
                  : "Lưu"}
              </button>
            </div>
          </form>
        </WorkspaceModalFrame>
      );
    }

    if (showCreateTask && activeCategory && canManage) {
      return (
        <WorkspaceModalFrame
          title="Thêm tác vụ"
          onClose={() => setShowCreateTask(false)}
        >
          <form
            onSubmit={(e) => handleCreateTask(activeCategory.id, e)}
            className="workspace-modal-form"
          >
            <div className="workspace-form-grid two-columns">
              <label className="workspace-form-field span-2">
                <span>Tên tác vụ *</span>
                <input
                  name="taskName"
                  placeholder="Nhập tên tác vụ"
                  required
                  className="workspace-input"
                  autoFocus
                />
              </label>
              <label className="workspace-form-field span-2">
                <span>Mô tả</span>
                <textarea
                  name="description"
                  placeholder="Ghi chú thêm"
                  className="workspace-input workspace-textarea"
                />
              </label>
              <label className="workspace-form-field">
                <span>Độ ưu tiên</span>
                <select
                  name="priority"
                  defaultValue="Medium"
                  className="workspace-select"
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Urgent">Urgent</option>
                </select>
              </label>
              <label className="workspace-form-field">
                <span>Deadline</span>
                <input name="deadline" type="date" className="workspace-input" />
              </label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                onClick={() => setShowCreateTask(false)}
                className="workspace-button ghost"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={taskLoading[activeCategory.id]}
                className="workspace-button primary"
              >
                {taskLoading[activeCategory.id] ? "Đang tạo..." : "Thêm tác vụ"}
              </button>
            </div>
          </form>
        </WorkspaceModalFrame>
      );
    }

    if (editingTaskData && canManage) {
      return (
        <WorkspaceModalFrame
          title="Sửa tác vụ"
          onClose={() => setEditingTask(null)}
        >
          <form
            onSubmit={(e) => handleUpdateTask(editingTaskData.id, e)}
            className="workspace-modal-form"
          >
            <div className="workspace-form-grid two-columns">
              <label className="workspace-form-field span-2">
                <span>Tên tác vụ *</span>
                <input
                  name="taskName"
                  defaultValue={editingTaskData.taskName}
                  required
                  className="workspace-input"
                  autoFocus
                />
              </label>
              <label className="workspace-form-field span-2">
                <span>Mô tả</span>
                <textarea
                  name="description"
                  defaultValue={editingTaskData.description || ""}
                  placeholder="Mô tả"
                  className="workspace-input workspace-textarea"
                />
              </label>
              <label className="workspace-form-field">
                <span>Độ ưu tiên</span>
                <select
                  name="priority"
                  defaultValue={editingTaskData.priority || "Medium"}
                  className="workspace-select"
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Urgent">Urgent</option>
                </select>
              </label>
              <label className="workspace-form-field">
                <span>Deadline</span>
                <input
                  name="deadline"
                  type="date"
                  defaultValue={
                    editingTaskData.deadline
                      ? String(editingTaskData.deadline).split("T")[0]
                      : ""
                  }
                  className="workspace-input"
                />
              </label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                onClick={() => setEditingTask(null)}
                className="workspace-button ghost"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={taskLoading[editingTaskData.id]}
                className="workspace-button primary"
              >
                {taskLoading[editingTaskData.id] ? "Đang lưu..." : "Lưu"}
              </button>
            </div>
          </form>
        </WorkspaceModalFrame>
      );
    }

    return null;
  };

  return (
    <div className="event-workspace">
      <Sidebar />
      <main className="workspace-main">
        {workspaceTab === "kanban" && <KanbanBoard />}
        {workspaceTab === "preview" && <EventPreviewBoard />}
        {workspaceTab === "event-members" && <EventMembersBoard />}
        {workspaceTab === "attendees" && <AttendeesBoard />}
      </main>
      <WorkspaceFormModals />
    </div>
  );
}

export default OrgEventDetailPage;
