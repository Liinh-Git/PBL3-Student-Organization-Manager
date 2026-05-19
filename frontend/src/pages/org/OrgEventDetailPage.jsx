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
        setEventMembers(Array.isArray(eventMembersData) ? eventMembersData : []);

        const myPermData = await getMyPermissions(orgId);
        setMyRoleName(myPermData?.roleName || "");
        setMyOrgMemberId(myPermData?.memberId || null);
      } catch (err) {
        setError(err.message || "Failed to load event detail");
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
          title="Event Detail"
          description="Manage event milestones, categories, and tasks"
        />
        <ForbiddenState message="You are not a member of this organization" />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="app-page">
        <PageHeader
          title="Event Detail"
          description="Manage event milestones, categories, and tasks"
        />
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-page">
        <PageHeader
          title="Event Detail"
          description="Manage event milestones, categories, and tasks"
        />
        <ErrorState message={error} />
      </div>
    );
  }

  const canManage = permissions.includes("org.events.manage");
  const canEditPreviewByRole = ["president", "vice president"].includes(
    String(myRoleName || "").trim().toLowerCase(),
  );
  const isCurrentUserEventOrganizer = eventMembers.some((item) => {
    const memberId = item.memberId || item.organizationMemberId || item.member?.id;
    return !!myOrgMemberId && memberId === myOrgMemberId;
  });
  const canEditPreview = canEditPreviewByRole && isCurrentUserEventOrganizer;
  const canManageEventMembers = isCurrentUserEventOrganizer;
  const eventOrganizerMemberIds = new Set(
    eventMembers
      .map((item) => item.memberId || item.organizationMemberId || item.member?.id)
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
        prev.map((item) => (item.id === attendeeId ? { ...item, ...updated } : item)),
      );
    } catch (err) {
      alert(err.message || "Failed to review check-in");
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
      alert(err.message || "Failed to create task");
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
      alert(err.message || "Failed to update task status");
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
      alert(err.message || "Failed to assign task");
    } finally {
      setTaskLoading((prev) => ({ ...prev, [taskId]: false }));
    }
  };

  const handleDeleteTask = async (taskId, categoryId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    if (!window.confirm("Are you sure you want to delete this task?")) {
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
      alert(err.message || "Failed to delete task");
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
      alert(err.message || "Failed to create milestone");
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
        "Are you sure you want to delete this milestone? You can only delete it when it has no categories.",
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
      alert(err.message || "Failed to delete milestone");
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
      alert(err.message || "Failed to create category");
    } finally {
      setCategoryLoading((prev) => ({ ...prev, [milestoneId]: false }));
    }
  };

  const handleDeleteCategory = async (categoryId, milestoneId) => {
    if (!canManage) {
      alert("Bạn không có quyền thực hiện thao tác này");
      return;
    }

    if (
      !window.confirm(
        "Are you sure you want to delete this category? You can only delete it when it has no tasks.",
      )
    ) {
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
      alert(err.message || "Failed to delete category");
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
      alert("Bạn không có quyền sửa sự kiện trong preview. Chỉ BTC của sự kiện mới được sửa.");
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
        throw new Error("Start date is invalid");
      }
      if (partialDraft.endDate !== undefined && !nextEndDate) {
        throw new Error("End date is invalid");
      }

      const nextTargetRaw =
        partialDraft.targetParticipants !== undefined
          ? partialDraft.targetParticipants
          : event.targetParticipants;

      const nextTarget =
        nextTargetRaw === "" || nextTargetRaw === null || nextTargetRaw === undefined
          ? undefined
          : Number(nextTargetRaw);

      if (nextTarget !== undefined && Number.isNaN(nextTarget)) {
        throw new Error("Target participants is invalid");
      }

      const updated = await updateEvent(eventId, {
        eventName: partialDraft.eventName ?? getEventName(event),
        description:
          partialDraft.description !== undefined
            ? partialDraft.description || undefined
            : event.description || undefined,
        startDate: nextStartDate || event.startDate,
        endDate: nextEndDate || event.endDate || nextStartDate || event.startDate,
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
      if (err?.response?.status === 403 || String(err.message || "").includes("403")) {
        alert("Không đủ quyền cập nhật sự kiện (403). Chỉ thành viên BTC của sự kiện mới được sửa.");
      } else {
        alert(err.message || "Failed to update event");
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
      alert(err.message || "Failed to update milestone");
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
      alert(err.message || "Failed to update category");
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
      alert(err.message || "Failed to update task");
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
    if (normalizeTaskStatus(draggedTask?.status) !== normalizeTaskStatus(status)) {
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
      setEventMembers((prev) => prev.filter((item) => item.id !== eventMemberId));
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
        <button
          type="button"
          onClick={() => navigate(`/org/events?orgId=${orgId}`)}
          className="event-back-button"
        >
          ‹ Bảng điều khiển
        </button>
        <div className="event-title-row">
          <h1>{getEventName(event) || "Event Detail"}</h1>
          <button
            type="button"
            onClick={() => setWorkspaceTab("preview")}
            className="sidebar-icon-button"
            title="Preview & Sửa sự kiện"
            aria-label="Preview & Sửa sự kiện"
          >
            ⧉
          </button>
        </div>
        <div className="event-date-row">
          <span>□</span>
          <span>{formatDate(event?.startDate)}</span>
        </div>
        <div className="workspace-tab-list">
          <button
            type="button"
            className={`workspace-tab-btn ${workspaceTab === "preview" ? "active" : ""}`}
            onClick={() => setWorkspaceTab("preview")}
          >
            Preview & Sửa sự kiện
          </button>
          <button
            type="button"
            className={`workspace-tab-btn ${workspaceTab === "event-members" ? "active" : ""}`}
            onClick={() => setWorkspaceTab("event-members")}
          >
            Ban tổ chức
          </button>
          <button
            type="button"
            className={`workspace-tab-btn ${workspaceTab === "attendees" ? "active" : ""}`}
            onClick={() => setWorkspaceTab("attendees")}
          >
            Danh sách người tham gia
          </button>
        </div>
      </div>

      <div className="event-roadmap">
        <div className="roadmap-heading roadmap-heading-clickable" onClick={() => setWorkspaceTab("kanban")}>
          <span>Lộ trình dự án</span>
          <div className="roadmap-heading-actions">
            <button
              type="button"
              className="roadmap-toggle"
              onClick={(e) => {
                e.stopPropagation();
                setWorkspaceTab("kanban");
                setIsRoadmapExpanded((prev) => !prev);
              }}
              title={isRoadmapExpanded ? "Thu gọn lộ trình" : "Mở rộng lộ trình"}
            >
              {isRoadmapExpanded ? "▾" : "▸"}
            </button>
            {canManage && (
              <button
                type="button"
                onClick={() => setShowCreateMilestone(true)}
                className="roadmap-add-button"
                title="Thêm lộ trình"
              >
                +
              </button>
            )}
          </div>
        </div>

        {isRoadmapExpanded && showCreateMilestone && canManage && (
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
            <EmptyState message="No milestones found" />
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
                      <span className="flag-icon">⚑</span>
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

                  {editingMilestone === milestone.id && canManage && (
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

                  {milestone.description &&
                    editingMilestone !== milestone.id && (
                      <p className="milestone-description">
                        {milestone.description}
                      </p>
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

                        {editingCategory === category.id && canManage && (
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

                    {showCreateCategory[milestone.id] && canManage ? (
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
                          + Thêm Hạng mục
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

  const TaskCreatePanel = () => {
    if (!showCreateTask || !activeCategory || !canManage) return null;
    return (
      <form
        onSubmit={(e) => handleCreateTask(activeCategory.id, e)}
        className="task-create-panel"
      >
        <input
          name="taskName"
          placeholder="Task name *"
          required
          className="workspace-input"
        />
        <input
          name="description"
          placeholder="Description"
          className="workspace-input"
        />
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
        <input name="deadline" type="date" className="workspace-input" />
        <button
          type="submit"
          disabled={taskLoading[activeCategory.id]}
          className="workspace-button primary"
        >
          {taskLoading[activeCategory.id] ? "Creating..." : "Add Task"}
        </button>
        <button
          type="button"
          onClick={() => setShowCreateTask(false)}
          className="workspace-button ghost"
        >
          Cancel
        </button>
      </form>
    );
  };

  const TaskCard = ({ task }) => {
    if (editingTask === task.id) {
      return (
        <form
          onSubmit={(e) => handleUpdateTask(task.id, e)}
          className="task-card task-card-edit"
        >
          <input
            name="taskName"
            defaultValue={task.taskName}
            required
            className="workspace-input"
            placeholder="Task name"
          />
          <input
            name="description"
            defaultValue={task.description || ""}
            className="workspace-input"
            placeholder="Description"
          />
          <select
            name="priority"
            defaultValue={task.priority || "Medium"}
            className="workspace-select"
          >
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
            <option value="Urgent">Urgent</option>
          </select>
          <input
            name="deadline"
            type="date"
            defaultValue={
              task.deadline ? String(task.deadline).split("T")[0] : ""
            }
            className="workspace-input"
          />
          <div className="task-card-actions always-visible">
            <button
              type="submit"
              disabled={taskLoading[task.id]}
              className="mini-primary-button"
            >
              {taskLoading[task.id] ? "Saving..." : "Save"}
            </button>
            <button
              type="button"
              onClick={() => setEditingTask(null)}
              className="mini-ghost-button"
            >
              Cancel
            </button>
          </div>
        </form>
      );
    }

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
          <span className="task-status-text">{getStatusLabel(task.status)}</span>
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

        <TaskCreatePanel />

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
                    const draggedTask = activeTasks.find((task) => task.id === draggedTaskId);
                    if (draggedTask && canUpdateTaskStatusByRole(draggedTask)) e.preventDefault();
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
            <span>Preview</span>
          </div>
          <h2>{getEventName(event) || "-"}</h2>
          <p>
            {canEditPreview
              ? "Bạn có quyền chỉnh sửa trực tiếp trong preview."
              : "Bạn có quyền xem preview. Chỉ Vice President/President thuộc BTC sự kiện mới được sửa."}
          </p>
        </div>
      </header>
      <EventWorkspacePreview
        eventData={event}
        canEdit={canEditPreview}
        isSaving={isEventUpdating}
        onSave={handleUpdateEventPreview}
      />
    </div>
  );

  const EventMembersBoard = () => {
    const existingMemberIds = new Set(
      eventMembers.map((item) => item.memberId || item.organizationMemberId || item.userId),
    );
    const memberMap = new Map(members.map((m) => [m.id, m]));
    const selectableMembers = members.filter((m) => !existingMemberIds.has(m.id));
    const normalizedKeyword = memberSearchKeyword.trim().toLowerCase();
    const filteredSelectableMembers = selectableMembers.filter((m) => {
      if (!normalizedKeyword) return true;
      const fullName = String(m.fullName || m.user?.fullName || "").toLowerCase();
      const email = String(m.email || m.user?.email || "").toLowerCase();
      const dept = String(m.department?.departmentName || m.department?.deptName || "").toLowerCase();
      return fullName.includes(normalizedKeyword) || email.includes(normalizedKeyword) || dept.includes(normalizedKeyword);
    });
    const pendingMembers = pendingMemberIds
      .map((id) => members.find((m) => m.id === id))
      .filter(Boolean);
    const addPendingMember = (memberId) => {
      setPendingMemberIds((prev) => (prev.includes(memberId) ? prev : [...prev, memberId]));
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
                <tr><td colSpan={6}>Chưa có thành viên BTC.</td></tr>
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
                      {memberMap.get(item.memberId)?.department?.departmentName ||
                        memberMap.get(item.memberId)?.department?.deptName ||
                        "-"}
                    </td>
                    <td>{formatDate(item.assignedAtUtc || item.createdAtUtc)}</td>
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
                        {(member.fullName || member.email || "?").charAt(0).toUpperCase()}
                      </span>
                      <span>{member.fullName || member.email || "-"}</span>
                      <button type="button" onClick={() => removePendingMember(member.id)} aria-label="Bỏ chọn">×</button>
                    </span>
                  ))}
                </div>
              )}

              <div className="member-pick-list">
                {filteredSelectableMembers.length === 0 ? (
                  <p className="member-pick-empty">Không có thành viên khả dụng.</p>
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
                            {(member.fullName || member.email || "?").charAt(0).toUpperCase()}
                          </span>
                          <div>
                            <strong>{member.fullName || member.email || "-"}</strong>
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
                  disabled={isEventMemberSubmitting || pendingMemberIds.length === 0}
                >
                  {isEventMemberSubmitting ? "Đang thêm..." : "Add"}
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
              <tr><td colSpan={6}>Chưa có người tham gia.</td></tr>
            ) : (
              attendees.map((attendee) => (
                <tr key={attendee.id}>
                  <td>{attendee.fullName || "-"}</td>
                  <td>{attendee.email || "-"}</td>
                  <td>{attendee.phoneNumber || "-"}</td>
                  <td>{formatDate(attendee.registeredAtUtc || attendee.createdAtUtc)}</td>
                  <td>{getAttendeeCheckInLabel(attendee.status)}</td>
                  <td>
                    {String(attendee.status || "") === "CheckInPending" && canManageEventMembers ? (
                      <div style={{ display: "inline-flex", gap: "8px" }}>
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
                          onClick={() => handleReviewCheckIn(attendee.id, false)}
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

  return (
    <div className="event-workspace">
      <style>{`
        .event-workspace {
          display: flex;
          min-height: 100vh;
          height: 100vh;
          overflow: hidden;
          background: #FFFFFF;
          color: #0F172A;
        }

        .event-sidebar {
          width: 260px;
          flex: 0 0 260px;
          display: flex;
          flex-direction: column;
          min-height: 100%;
          background: #F9FBFF;
          border-right: 1px solid #E6EDF5;
        }

        .event-sidebar-header {
          padding: 18px 14px 14px;
          border-bottom: 1px solid #E6EDF5;
        }

        .event-back-button {
          display: inline-flex;
          align-items: center;
          gap: 4px;
          margin: 0 0 12px;
          padding: 0;
          border: 0;
          background: transparent;
          color: #64748B;
          font-size: 14px;
          font-weight: 650;
          cursor: pointer;
        }

        .event-back-button:hover {
          color: #0F172A;
        }

        .event-title-row {
          display: flex;
          align-items: flex-start;
          justify-content: space-between;
          gap: 8px;
        }

        .event-title-row h1 {
          margin: 0;
          color: #0F172A;
          font-size: 19px;
          line-height: 1.28;
          font-weight: 800;
          letter-spacing: -0.025em;
        }

        .event-date-row {
          display: flex;
          align-items: center;
          gap: 7px;
          margin-top: 10px;
          color: #526A8A;
          font-size: 13px;
          font-weight: 600;
        }

        .workspace-tab-list {
          display: grid;
          gap: 7px;
          margin-top: 12px;
        }

        .workspace-tab-btn {
          border: 1px solid #E2E8F0;
          background: #FDFEFF;
          color: #334155;
          border-radius: 9px;
          text-align: left;
          padding: 9px 10px;
          font-size: 12.5px;
          font-weight: 700;
          cursor: pointer;
          transition: all 140ms ease;
        }

        .workspace-tab-btn:hover {
          border-color: #CBD5E1;
          background: #F8FAFC;
        }

        .workspace-tab-btn.active {
          border-color: #FDBA74;
          background: #FFF3E8;
          color: #C2410C;
          box-shadow: inset 0 0 0 1px rgba(249, 115, 22, 0.12);
        }

        .event-roadmap {
          flex: 1;
          overflow-y: auto;
          padding: 14px 14px 20px;
        }

        .event-roadmap::-webkit-scrollbar,
        .kanban-scroll::-webkit-scrollbar,
        .kanban-column-body::-webkit-scrollbar {
          width: 6px;
          height: 6px;
        }

        .event-roadmap::-webkit-scrollbar-track,
        .kanban-scroll::-webkit-scrollbar-track,
        .kanban-column-body::-webkit-scrollbar-track {
          background: transparent;
        }

        .event-roadmap::-webkit-scrollbar-thumb,
        .kanban-scroll::-webkit-scrollbar-thumb,
        .kanban-column-body::-webkit-scrollbar-thumb {
          background: #CBD5E1;
          border-radius: 999px;
        }

        .roadmap-heading {
          display: flex;
          align-items: center;
          justify-content: space-between;
          margin-bottom: 10px;
          color: #94A3B8;
          font-size: 11px;
          font-weight: 800;
          letter-spacing: 0.07em;
          text-transform: uppercase;
        }

        .roadmap-heading-clickable {
          padding: 8px 10px;
          border: 1px solid #E2E8F0;
          border-radius: 10px;
          background: #FFFFFF;
          cursor: pointer;
          transition: all 140ms ease;
        }

        .roadmap-heading-clickable:hover {
          border-color: #CBD5E1;
          background: #F8FAFC;
        }

        .roadmap-heading-actions {
          display: inline-flex;
          align-items: center;
          gap: 4px;
        }

        .roadmap-toggle {
          width: 24px;
          height: 24px;
          border: 0;
          border-radius: 8px;
          background: transparent;
          color: #94A3B8;
          font-size: 15px;
          font-weight: 800;
          cursor: pointer;
          line-height: 1;
        }

        .roadmap-toggle:hover {
          background: #EEF2F7;
          color: #F97316;
        }

        .roadmap-add-button,
        .sidebar-icon-button,
        .task-icon-button,
        .modal-close-button {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          border: 0;
          background: transparent;
          cursor: pointer;
          transition: color 150ms ease, background 150ms ease;
        }

        .roadmap-add-button {
          width: 28px;
          height: 28px;
          border-radius: 8px;
          color: #94A3B8;
          font-size: 19px;
        }

        .roadmap-add-button:hover,
        .sidebar-icon-button:hover,
        .task-icon-button:hover {
          background: #EEF2F7;
          color: #F97316;
        }

        .sidebar-empty {
          padding: 20px 0;
        }

        .milestone-list {
          display: grid;
          gap: 18px;
        }

        .milestone-header {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 8px;
          margin-bottom: 6px;
        }

        .milestone-title-button {
          display: inline-flex;
          min-width: 0;
          align-items: center;
          gap: 8px;
          padding: 0;
          border: 0;
          background: transparent;
          color: #0F172A;
          font-size: 13px;
          font-weight: 800;
          text-align: left;
          cursor: pointer;
        }

        .milestone-title-button span:last-child {
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }

        .flag-icon {
          color: #F97316;
          flex: 0 0 auto;
        }

        .milestone-actions,
        .category-actions {
          display: flex;
          flex: 0 0 auto;
          align-items: center;
          gap: 2px;
          opacity: 0;
          transition: opacity 150ms ease;
        }

        .milestone-block:hover .milestone-actions,
        .category-row:hover .category-actions {
          opacity: 1;
        }

        .sidebar-icon-button {
          width: 24px;
          height: 24px;
          border-radius: 6px;
          color: #94A3B8;
          font-size: 13px;
          font-weight: 800;
        }

        .sidebar-icon-button.danger:hover,
        .task-icon-button.danger:hover,
        .modal-close-button:hover {
          background: #FEF2F2;
          color: #DC2626;
        }

        .milestone-description {
          margin: 4px 0 10px 24px;
          color: #64748B;
          font-size: 12px;
          line-height: 1.45;
        }

        .category-tree {
          display: grid;
          gap: 4px;
          margin-left: 6px;
          padding-left: 12px;
          border-left: 1px solid #DDE7F2;
        }

        .category-row {
          display: flex;
          align-items: center;
          gap: 4px;
        }

        .category-button {
          flex: 1;
          min-width: 0;
          padding: 8px 10px;
          border: 0;
          border-radius: 8px;
          background: transparent;
          color: #334155;
          font-size: 14px;
          font-weight: 650;
          text-align: left;
          cursor: pointer;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
          transition: background 150ms ease, color 150ms ease;
        }

        .category-button:hover {
          background: #EEF2F7;
          color: #0F172A;
        }

        .category-button.active {
          background: #FFF1E8;
          color: #F97316;
        }

        .category-empty {
          padding: 8px 12px;
          color: #94A3B8;
          font-size: 12px;
        }

        .add-category-button {
          width: 100%;
          margin-top: 2px;
          padding: 9px 12px;
          border: 0;
          border-radius: 8px;
          background: transparent;
          color: #94A3B8;
          font-size: 13px;
          font-weight: 700;
          text-align: left;
          cursor: pointer;
        }

        .add-category-button:hover {
          background: #EEF2F7;
          color: #334155;
        }

        .sidebar-form {
          display: grid;
          gap: 8px;
          margin-bottom: 16px;
          padding: 12px;
          border: 1px solid #E2E8F0;
          border-radius: 10px;
          background: #FFFFFF;
        }

        .sidebar-form.nested-form,
        .category-create-form {
          margin: 6px 0 8px;
        }

        .category-edit-form {
          margin-left: 0;
        }

        .sidebar-input,
        .workspace-input,
        .workspace-select,
        .task-status-select,
        .assignee-select {
          width: 100%;
          border: 1px solid #DDE7F2;
          border-radius: 8px;
          background: #FFFFFF;
          color: #0F172A;
          font-size: 13px;
          outline: none;
          transition: border-color 150ms ease, box-shadow 150ms ease;
        }

        .sidebar-input,
        .workspace-input,
        .workspace-select {
          min-height: 38px;
          padding: 8px 10px;
        }

        .sidebar-input:focus,
        .workspace-input:focus,
        .workspace-select:focus,
        .task-status-select:focus,
        .assignee-select:focus {
          border-color: #F97316;
          box-shadow: 0 0 0 3px rgba(249, 115, 22, 0.12);
        }

        .sidebar-form-actions {
          display: flex;
          gap: 8px;
        }

        .mini-primary-button,
        .mini-ghost-button,
        .workspace-button {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          gap: 8px;
          border-radius: 8px;
          border: 1px solid transparent;
          font-weight: 750;
          cursor: pointer;
          transition: background 150ms ease, color 150ms ease, border-color 150ms ease;
        }

        .mini-primary-button,
        .mini-ghost-button {
          min-height: 32px;
          padding: 0 10px;
          font-size: 12px;
        }

        .mini-primary-button,
        .workspace-button.primary {
          background: #F97316;
          color: #FFFFFF;
        }

        .mini-primary-button:hover:not(:disabled),
        .workspace-button.primary:hover:not(:disabled) {
          background: #EA580C;
        }

        .mini-ghost-button,
        .workspace-button.ghost {
          background: #FFFFFF;
          border-color: #E2E8F0;
          color: #334155;
        }

        .mini-ghost-button:hover:not(:disabled),
        .workspace-button.ghost:hover:not(:disabled) {
          background: #F8FAFC;
          border-color: #CBD5E1;
        }

        .mini-primary-button:disabled,
        .mini-ghost-button:disabled,
        .workspace-button:disabled {
          cursor: not-allowed;
          opacity: 0.65;
        }

        .workspace-main {
          flex: 1;
          min-width: 0;
          display: flex;
          flex-direction: column;
          background: #FFFFFF;
        }

        .workspace-main-content {
          display: flex;
          flex: 1;
          min-height: 0;
          flex-direction: column;
        }

        .workspace-main-content.preview-mode {
          overflow: auto;
          padding-bottom: 60px;
        }

        .workspace-preview-shell {
          border: 1px solid #E2E8F0;
          border-radius: 12px;
          background: #fff;
          overflow: hidden;
          margin: 16px 20px 20px;
          min-height: 0;
          flex: 1;
        }

        .workspace-preview-iframe {
          width: 128%;
          height: 128%;
          min-height: 720px;
          border: 0;
          display: block;
          transform: scale(0.78);
          transform-origin: top left;
        }

        .workspace-table-wrap {
          margin: 16px 20px 20px;
          border: 1px solid #E2E8F0;
          border-radius: 12px;
          background: #fff;
          overflow: auto;
        }

        .workspace-table {
          width: 100%;
          border-collapse: collapse;
          font-size: 14px;
        }

        .workspace-table th,
        .workspace-table td {
          text-align: left;
          padding: 12px;
          border-bottom: 1px solid #F1F5F9;
        }

        .workspace-table th {
          background: #F8FAFC;
          color: #475569;
          font-weight: 800;
          font-size: 12px;
          text-transform: uppercase;
          letter-spacing: 0.04em;
        }

        .workspace-member-modal {
          width: min(760px, 100%);
        }

        .member-chip-list {
          display: flex;
          flex-wrap: wrap;
          gap: 8px;
          margin-top: 12px;
        }

        .member-chip {
          display: inline-flex;
          align-items: center;
          gap: 8px;
          border: 1px solid #FDBA74;
          background: #FFF7ED;
          color: #9A3412;
          border-radius: 999px;
          padding: 4px 8px 4px 6px;
          font-size: 12px;
          font-weight: 700;
        }

        .member-chip-avatar,
        .member-pick-avatar {
          width: 24px;
          height: 24px;
          border-radius: 999px;
          display: inline-flex;
          align-items: center;
          justify-content: center;
          background: #FFEDD5;
          color: #C2410C;
          font-size: 12px;
          font-weight: 800;
          flex: 0 0 24px;
        }

        .member-chip button {
          width: 20px;
          height: 20px;
          border: 0;
          border-radius: 999px;
          background: #FED7AA;
          color: #9A3412;
          cursor: pointer;
          font-weight: 800;
          line-height: 1;
        }

        .member-pick-list {
          margin-top: 12px;
          border: 1px solid #E2E8F0;
          border-radius: 12px;
          max-height: 320px;
          overflow: auto;
          background: #fff;
        }

        .member-pick-empty {
          margin: 0;
          padding: 16px;
          color: #64748B;
          font-size: 13px;
        }

        .member-pick-item {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 10px;
          padding: 10px 12px;
          border-bottom: 1px solid #F1F5F9;
        }

        .member-pick-item:last-child {
          border-bottom: none;
        }

        .member-pick-main {
          display: flex;
          align-items: center;
          gap: 10px;
          min-width: 0;
        }

        .member-pick-main div {
          display: grid;
          min-width: 0;
        }

        .member-pick-main strong {
          color: #0F172A;
          font-size: 14px;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }

        .member-pick-main span {
          color: #64748B;
          font-size: 12px;
        }

        .member-pick-add {
          width: 28px;
          height: 28px;
          border-radius: 8px;
          border: 1px solid #FDBA74;
          background: #FFF7ED;
          color: #C2410C;
          font-size: 18px;
          font-weight: 800;
          cursor: pointer;
          line-height: 1;
        }

        .member-pick-add:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }

        .workspace-member-actions {
          display: grid;
          gap: 10px;
          margin: 0 20px 20px;
          max-width: 420px;
        }

        .kanban-header {
          display: flex;
          min-height: 80px;
          align-items: center;
          justify-content: space-between;
          gap: 24px;
          padding: 18px 32px;
          border-bottom: 1px solid #E2E8F0;
          background: #FFFFFF;
        }

        .breadcrumb-line {
          display: flex;
          align-items: center;
          gap: 8px;
          margin-bottom: 4px;
          color: #94A3B8;
          font-size: 12px;
          font-weight: 700;
        }

        .kanban-header h2 {
          margin: 0;
          color: #0F172A;
          font-size: 22px;
          line-height: 1.2;
          font-weight: 850;
          letter-spacing: -0.03em;
        }

        .kanban-header p {
          margin: 5px 0 0;
          color: #64748B;
          font-size: 13px;
        }

        .workspace-button {
          min-height: 40px;
          padding: 0 16px;
          font-size: 14px;
        }

        .task-create-panel {
          display: grid;
          grid-template-columns: 1.2fr 1.2fr 150px 150px auto auto;
          gap: 10px;
          align-items: center;
          padding: 16px 32px;
          border-bottom: 1px solid #E2E8F0;
          background: #FFFFFF;
        }

        .kanban-scroll {
          flex: 1;
          min-height: 0;
          overflow: auto;
          padding: 24px;
          background: #F8FAFC;
        }

        .kanban-board {
          display: grid;
          grid-template-columns: repeat(3, minmax(0, 1fr));
          align-items: stretch;
          gap: 16px;
          width: 100%;
          min-width: 0;
          height: 100%;
        }

        .kanban-column {
          width: auto;
          min-width: 0;
          display: flex;
          min-height: 420px;
          max-height: 100%;
          flex-direction: column;
          border: 1px solid #DDE7F2;
          border-radius: 12px;
          background: rgba(241, 245, 249, 0.66);
          overflow: hidden;
        }

        .kanban-column-header {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 12px;
          padding: 16px;
          border-bottom: 1px solid #DDE7F2;
        }

        .kanban-column-header h3 {
          display: flex;
          align-items: center;
          gap: 9px;
          margin: 0;
          color: #0F172A;
          font-size: 14px;
          font-weight: 800;
        }

        .status-dot {
          width: 10px;
          height: 10px;
          border-radius: 999px;
        }

        .status-todo { background: #E2E8F0; }
        .status-progress { background: #F97316; }
        .status-blocked { background: #EF4444; }
        .status-done { background: #10B981; }
        .status-cancelled { background: #94A3B8; }
        .status-unknown { background: #A855F7; }

        .task-count {
          display: inline-flex;
          min-width: 24px;
          height: 24px;
          align-items: center;
          justify-content: center;
          border-radius: 6px;
          background: #FFFFFF;
          color: #64748B;
          font-size: 12px;
          font-weight: 800;
          border: 1px solid #E2E8F0;
          box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
        }

        .kanban-column-body {
          flex: 1;
          min-height: 0;
          overflow-y: auto;
          padding: 14px;
        }

        .column-empty {
          display: flex;
          min-height: 92px;
          align-items: center;
          justify-content: center;
          border: 1px dashed #CBD5E1;
          border-radius: 10px;
          color: #94A3B8;
          font-size: 12px;
          font-weight: 650;
        }

        .task-card {
          position: relative;
          display: grid;
          gap: 12px;
          padding: 15px;
          margin-bottom: 12px;
          border: 1px solid #DDE7F2;
          border-radius: 12px;
          background: #FFFFFF;
          box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
          cursor: grab;
          transition: transform 150ms ease, box-shadow 150ms ease, border-color 150ms ease;
        }

        .task-card:hover {
          transform: translateY(-1px);
          border-color: #CBD5E1;
          box-shadow: 0 8px 18px rgba(15, 23, 42, 0.07);
        }

        .task-card.is-loading {
          opacity: 0.72;
          pointer-events: none;
        }

        .task-card-edit {
          cursor: default;
        }

        .task-card-top {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 10px;
        }

        .priority-badge {
          display: inline-flex;
          align-items: center;
          width: fit-content;
          min-height: 20px;
          padding: 4px 8px;
          border-radius: 5px;
          font-size: 10px;
          line-height: 1;
          font-weight: 850;
          letter-spacing: 0.04em;
        }

        .priority-high {
          background: #FFF1F2;
          color: #E11D48;
        }

        .priority-medium {
          background: #FFFBEB;
          color: #D97706;
        }

        .priority-low {
          background: #F1F5F9;
          color: #475569;
        }

        .task-card h3 {
          margin: 0;
          color: #0F172A;
          font-size: 14px;
          line-height: 1.45;
          font-weight: 760;
        }

        .task-description {
          margin: -4px 0 0;
          color: #64748B;
          font-size: 12px;
          line-height: 1.45;
        }

        .task-card-actions {
          display: flex;
          align-items: center;
          gap: 2px;
          opacity: 0;
          transition: opacity 150ms ease;
        }

        .task-card:hover .task-card-actions,
        .task-card-actions.always-visible {
          opacity: 1;
        }

        .task-icon-button {
          width: 24px;
          height: 24px;
          border-radius: 6px;
          color: #94A3B8;
          font-size: 13px;
          font-weight: 800;
        }

        .task-status-row {
          display: flex;
          align-items: center;
        }

        .task-status-select {
          min-height: 32px;
          padding: 6px 8px;
          font-size: 12px;
        }

        .task-status-text {
          color: #475569;
          font-size: 12px;
          font-weight: 700;
        }

        .task-footer {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 12px;
          padding-top: 10px;
          border-top: 1px solid #F1F5F9;
        }

        .assignee-wrap {
          display: flex;
          min-width: 0;
          align-items: center;
          gap: 8px;
          flex: 0 1 132px;
        }

        .avatar-dot {
          display: inline-flex;
          width: 24px;
          height: 24px;
          flex: 0 0 24px;
          align-items: center;
          justify-content: center;
          border: 1px solid #DDE7F2;
          border-radius: 999px;
          background: #F8FAFC;
          color: #64748B;
          font-size: 11px;
          font-weight: 800;
        }

        .assignee-select {
          width: 100%;
          min-width: 0;
          max-width: 100px;
          height: 30px;
          padding: 4px 6px;
          color: #64748B;
          font-size: 11px;
          border-color: transparent;
          background: transparent;
        }

        .assignee-select:hover {
          border-color: #DDE7F2;
          background: #FFFFFF;
        }

        .assignee-name {
          min-width: 0;
          color: #64748B;
          font-size: 11px;
          font-weight: 700;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }

        .deadline-text {
          flex: 0 0 auto;
          color: #94A3B8;
          font-size: 12px;
          font-weight: 800;
          white-space: nowrap;
        }

        .workspace-empty-state {
          display: flex;
          flex: 1;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          padding: 40px;
          text-align: center;
        }

        .workspace-empty-icon {
          display: flex;
          width: 80px;
          height: 80px;
          align-items: center;
          justify-content: center;
          margin-bottom: 24px;
          border: 1px solid #E2E8F0;
          border-radius: 999px;
          background: #F8FAFC;
          color: #CBD5E1;
          font-size: 36px;
          font-weight: 600;
        }

        .workspace-empty-state h2 {
          margin: 0 0 8px;
          color: #0F172A;
          font-size: 26px;
          font-weight: 850;
          letter-spacing: -0.035em;
        }

        .workspace-empty-state p {
          max-width: 420px;
          margin: 0;
          color: #64748B;
          font-size: 14px;
          line-height: 1.55;
        }

        .workspace-modal-backdrop {
          position: fixed;
          inset: 0;
          z-index: 60;
          display: flex;
          align-items: center;
          justify-content: center;
          padding: 24px;
          background: rgba(15, 23, 42, 0.34);
          backdrop-filter: blur(4px);
        }

        .workspace-modal {
          width: min(760px, 100%);
          max-height: calc(100vh - 48px);
          overflow-y: auto;
          padding: 24px;
          border-radius: 16px;
          background: #FFFFFF;
          box-shadow: 0 24px 64px rgba(15, 23, 42, 0.2);
        }

        .workspace-modal-header {
          display: flex;
          align-items: flex-start;
          justify-content: space-between;
          gap: 16px;
          margin-bottom: 20px;
        }

        .workspace-eyebrow {
          margin: 0 0 4px;
          color: #F97316;
          font-size: 11px;
          font-weight: 850;
          letter-spacing: 0.08em;
          text-transform: uppercase;
        }

        .workspace-modal-header h2 {
          margin: 0;
          color: #0F172A;
          font-size: 22px;
          font-weight: 850;
          letter-spacing: -0.025em;
        }

        .modal-close-button {
          width: 34px;
          height: 34px;
          border-radius: 10px;
          color: #94A3B8;
          font-size: 24px;
        }

        .event-edit-grid {
          display: grid;
          grid-template-columns: repeat(2, minmax(0, 1fr));
          gap: 16px;
        }

        .modal-actions {
          grid-column: 1 / -1;
          display: flex;
          justify-content: flex-end;
          gap: 10px;
        }

        @media (max-width: 860px) {
          .event-workspace {
            height: auto;
            min-height: 100vh;
            flex-direction: column;
            overflow: visible;
          }

          .event-sidebar {
            width: 100%;
            flex-basis: auto;
            max-height: none;
          }

          .workspace-main {
            min-height: 70vh;
          }

          .kanban-header {
            align-items: flex-start;
            flex-direction: column;
            padding: 18px;
          }

          .kanban-scroll {
            padding: 18px;
          }

          .task-create-panel {
            grid-template-columns: 1fr;
            padding: 16px 18px;
          }

          .event-edit-grid {
            grid-template-columns: 1fr;
          }

          .modal-actions {
            flex-direction: column-reverse;
          }
        }
      `}</style>

      <Sidebar />
      <main className="workspace-main">
        {workspaceTab === "kanban" && <KanbanBoard />}
        {workspaceTab === "preview" && <EventPreviewBoard />}
        {workspaceTab === "event-members" && <EventMembersBoard />}
        {workspaceTab === "attendees" && <AttendeesBoard />}
      </main>
    </div>
  );
}

export default OrgEventDetailPage;
