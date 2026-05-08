/**
 * OrgEventDetailPage.jsx - Organization event detail page (EventDetail tree root)
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useParams, useSearchParams, useNavigate } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getEventById, updateEvent } from '../../services/eventService.js';
import { getEventMilestones } from '../../services/milestoneService.js';
import { getMilestoneCategories } from '../../services/categoryService.js';
import { createTask, updateTask, updateTaskStatus, assignTask, deleteTask } from '../../services/taskService.js';
import { getOrganizationMembers } from '../../services/memberService.js';
import { createMilestone, updateMilestone, deleteMilestone } from '../../services/milestoneService.js';
import { createCategory, updateCategory, deleteCategory } from '../../services/categoryService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';
import ForbiddenState from '../../components/shared/ForbiddenState';

function OrgEventDetailPage() {
  const { eventId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const orgId = searchParams.get('orgId');
  const { permissions, isMember } = useOrgContext();

  const [event, setEvent] = useState(null);
  const [milestones, setMilestones] = useState([]);
  const [categoriesByMilestone, setCategoriesByMilestone] = useState({});
  const [members, setMembers] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [taskLoading, setTaskLoading] = useState({});
  const [milestoneLoading, setMilestoneLoading] = useState({});
  const [categoryLoading, setCategoryLoading] = useState({});
  const [showCreateMilestone, setShowCreateMilestone] = useState(false);
  const [showCreateCategory, setShowCreateCategory] = useState({});
  const [editingEvent, setEditingEvent] = useState(false);
  const [editingMilestone, setEditingMilestone] = useState(null);
  const [editingCategory, setEditingCategory] = useState(null);
  const [editingTask, setEditingTask] = useState(null);
  const [isEventUpdating, setIsEventUpdating] = useState(false);
  const getEventName = (eventData) => eventData?.name || eventData?.eventName;

  useEffect(() => {
    if (!eventId || !orgId || !isMember) return;
    async function loadEventDetail() {
      setIsLoading(true);
      try {
        // Load event
        const eventData = await getEventById(eventId);
        setEvent(eventData);

        // Load milestones
        const milestonesData = await getEventMilestones(eventId);
        setMilestones(milestonesData);

        // Load categories for each milestone
        const categoriesMap = {};
        for (const milestone of milestonesData) {
          const categoriesData = await getMilestoneCategories(milestone.id);
          // Ensure tasks array exists
          const categoriesWithTasks = categoriesData.map(cat => ({
            ...cat,
            tasks: cat.tasks || []
          }));
          categoriesMap[milestone.id] = categoriesWithTasks;
        }
        setCategoriesByMilestone(categoriesMap);

        // Load members for assignee dropdown
        const membersData = await getOrganizationMembers(orgId);
        setMembers(membersData);
      } catch (err) {
        setError(err.message || 'Failed to load event detail');
      } finally {
        setIsLoading(false);
      }
    }
    loadEventDetail();
  }, [eventId, orgId, isMember]);

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

  const canManage = permissions.includes('org.events.manage');
  const getTaskAssigneeId = (task) =>
    task?.assigneeId ||
    task?.assignedMemberId ||
    task?.assignee?.id ||
    task?.assignee?.memberId ||
    task?.assignee?.userId ||
    '';

  const getTaskAssigneeName = (task) => {
    const directName =
      task?.assignee?.user?.fullName ||
      task?.assignee?.fullName ||
      task?.assigneeName;
    if (directName) return directName;

    const assigneeId = getTaskAssigneeId(task);
    if (!assigneeId) return '-';
    const matchedMember = members.find((member) => member.id === assigneeId);
    return matchedMember?.fullName || matchedMember?.email || '-';
  };

  // Task mutation handlers
  const handleCreateTask = async (categoryId, e) => {
    e.preventDefault();
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const taskName = form.taskName.value;
    const description = form.description.value;
    const priority = form.priority.value;
    const deadline = form.deadline.value;
    
    if (!taskName) {
      alert('Task name is required');
      return;
    }

    setTaskLoading(prev => ({ ...prev, [categoryId]: true }));
    try {
      const newTask = await createTask(categoryId, {
        taskName,
        description: description || undefined,
        priority,
        deadline: deadline || undefined
      });
      
      // Optimistic update - append task to category
      setCategoriesByMilestone(prev => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          const categoryIndex = updated[milestoneId].findIndex(c => c.id === categoryId);
          if (categoryIndex !== -1) {
            updated[milestoneId] = updated[milestoneId].map((cat, idx) => 
              idx === categoryIndex 
                ? { ...cat, tasks: [...(cat.tasks || []), newTask] }
                : cat
            );
            break;
          }
        }
        return updated;
      });
      
      form.reset();
    } catch (err) {
      alert(err.message || 'Failed to create task');
    } finally {
      setTaskLoading(prev => ({ ...prev, [categoryId]: false }));
    }
  };

  const handleUpdateStatus = async (taskId, newStatus, categoryId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    setTaskLoading(prev => ({ ...prev, [taskId]: true }));
    try {
      const updatedTask = await updateTaskStatus(taskId, { status: newStatus });
      
      // Optimistic update - replace task in tree
      setCategoriesByMilestone(prev => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          updated[milestoneId] = updated[milestoneId].map(cat => ({
            ...cat,
            tasks: cat.tasks?.map(task => 
              task.id === taskId ? updatedTask : task
            ) || []
          }));
        }
        return updated;
      });
    } catch (err) {
      alert(err.message || 'Failed to update task status');
    } finally {
      setTaskLoading(prev => ({ ...prev, [taskId]: false }));
    }
  };

  const handleAssignTask = async (taskId, assigneeId, categoryId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    setTaskLoading(prev => ({ ...prev, [taskId]: true }));
    try {
      const updatedTask = await assignTask(taskId, { 
        assigneeId: assigneeId || null,
        deptId: null
      });
      
      // Optimistic update - replace task in tree
      setCategoriesByMilestone(prev => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          updated[milestoneId] = updated[milestoneId].map(cat => ({
            ...cat,
            tasks: cat.tasks?.map(task => 
              task.id === taskId ? updatedTask : task
            ) || []
          }));
        }
        return updated;
      });
    } catch (err) {
      alert(err.message || 'Failed to assign task');
    } finally {
      setTaskLoading(prev => ({ ...prev, [taskId]: false }));
    }
  };

  const handleDeleteTask = async (taskId, categoryId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    if (!window.confirm('Are you sure you want to delete this task?')) {
      return;
    }

    setTaskLoading(prev => ({ ...prev, [taskId]: true }));
    try {
      await deleteTask(taskId);
      
      // Optimistic update - remove task from tree
      setCategoriesByMilestone(prev => {
        const updated = { ...prev };
        for (const milestoneId in updated) {
          updated[milestoneId] = updated[milestoneId].map(cat => ({
            ...cat,
            tasks: cat.tasks?.filter(task => task.id !== taskId) || []
          }));
        }
        return updated;
      });
    } catch (err) {
      alert(err.message || 'Failed to delete task');
    } finally {
      setTaskLoading(prev => ({ ...prev, [taskId]: false }));
    }
  };

  // Milestone mutation handlers
  const handleCreateMilestone = async (e) => {
    e.preventDefault();
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const title = form.title.value;
    const description = form.description.value;
    const orderIndex = milestones.length + 1;
    
    if (!title) {
      alert('Title is required');
      return;
    }

    setMilestoneLoading(prev => ({ ...prev, create: true }));
    try {
      const newMilestone = await createMilestone(eventId, {
        title,
        description: description || undefined,
        orderIndex
      });
      
      setMilestones(prev => [...prev, newMilestone]);
      setCategoriesByMilestone(prev => ({
        ...prev,
        [newMilestone.id]: []
      }));
      
      form.reset();
      setShowCreateMilestone(false);
    } catch (err) {
      alert(err.message || 'Failed to create milestone');
    } finally {
      setMilestoneLoading(prev => ({ ...prev, create: false }));
    }
  };

  const handleDeleteMilestone = async (milestoneId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    if (!window.confirm('Are you sure you want to delete this milestone? This will also delete all categories and tasks within it.')) {
      return;
    }

    setMilestoneLoading(prev => ({ ...prev, [milestoneId]: true }));
    try {
      await deleteMilestone(milestoneId);
      
      setMilestones(prev => prev.filter(m => m.id !== milestoneId));
      setCategoriesByMilestone(prev => {
        const updated = { ...prev };
        delete updated[milestoneId];
        return updated;
      });
    } catch (err) {
      alert(err.message || 'Failed to delete milestone');
    } finally {
      setMilestoneLoading(prev => ({ ...prev, [milestoneId]: false }));
    }
  };

  // Category mutation handlers
  const handleCreateCategory = async (milestoneId, e) => {
    e.preventDefault();
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }
    
    const form = e.target;
    const categoryName = form.categoryName.value;
    const description = form.description.value;
    const orderIndex = (categoriesByMilestone[milestoneId]?.length || 0) + 1;
    
    if (!categoryName) {
      alert('Category name is required');
      return;
    }

    setCategoryLoading(prev => ({ ...prev, [milestoneId]: true }));
    try {
      const newCategory = await createCategory(milestoneId, {
        categoryName,
        description: description || undefined,
        orderIndex
      });
      
      setCategoriesByMilestone(prev => ({
        ...prev,
        [milestoneId]: [...(prev[milestoneId] || []), { ...newCategory, tasks: [] }]
      }));
      
      form.reset();
      setShowCreateCategory(prev => ({ ...prev, [milestoneId]: false }));
    } catch (err) {
      alert(err.message || 'Failed to create category');
    } finally {
      setCategoryLoading(prev => ({ ...prev, [milestoneId]: false }));
    }
  };

  const handleDeleteCategory = async (categoryId, milestoneId) => {
    if (!canManage) {
      alert('Bạn không có quyền thực hiện thao tác này');
      return;
    }

    if (!window.confirm('Are you sure you want to delete this category? This will also delete all tasks within it.')) {
      return;
    }

    setCategoryLoading(prev => ({ ...prev, [categoryId]: true }));
    try {
      await deleteCategory(categoryId);
      
      setCategoriesByMilestone(prev => ({
        ...prev,
        [milestoneId]: prev[milestoneId]?.filter(c => c.id !== categoryId) || []
      }));
    } catch (err) {
      alert(err.message || 'Failed to delete category');
    } finally {
      setCategoryLoading(prev => ({ ...prev, [categoryId]: false }));
    }
  };

  const handleUpdateEvent = async (e) => {
    e.preventDefault();
    if (!canManage) return;
    
    const form = e.target;
    const eventName = form.eventName.value;
    const description = form.description.value;
    const startDate = form.startDate.value;
    const endDate = form.endDate.value;
    const location = form.location.value;
    const bannerUrl = form.bannerUrl.value;
    const visibility = form.visibility.value;
    
    setIsEventUpdating(true);
    try {
      const updated = await updateEvent(eventId, {
        eventName,
        description: description || undefined,
        startDate,
        endDate: endDate || undefined,
        location: location || undefined,
        bannerUrl: bannerUrl || undefined,
        visibility
      });
      setEvent(updated);
      setEditingEvent(false);
    } catch (err) {
      alert(err.message || 'Failed to update event');
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
    
    setMilestoneLoading(prev => ({ ...prev, [milestoneId]: true }));
    try {
      const updated = await updateMilestone(milestoneId, {
        title,
        description: description || undefined
      });
      
      setMilestones(prev => prev.map(m => m.id === milestoneId ? {...m, title: updated.title, description: updated.description} : m));
      setEditingMilestone(null);
    } catch (err) {
      alert(err.message || 'Failed to update milestone');
    } finally {
      setMilestoneLoading(prev => ({ ...prev, [milestoneId]: false }));
    }
  };

  const handleUpdateCategory = async (categoryId, milestoneId, e) => {
    e.preventDefault();
    if (!canManage) return;
    
    const form = e.target;
    const categoryName = form.categoryName.value;
    const description = form.description.value;
    
    setCategoryLoading(prev => ({ ...prev, [categoryId]: true }));
    try {
      const updated = await updateCategory(categoryId, {
        categoryName,
        description: description || undefined
      });
      
      setCategoriesByMilestone(prev => ({
        ...prev,
        [milestoneId]: prev[milestoneId].map(c => c.id === categoryId ? {...c, categoryName: updated.categoryName, description: updated.description} : c)
      }));
      setEditingCategory(null);
    } catch (err) {
      alert(err.message || 'Failed to update category');
    } finally {
      setCategoryLoading(prev => ({ ...prev, [categoryId]: false }));
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
    
    setTaskLoading(prev => ({ ...prev, [taskId]: true }));
    try {
      const updatedTask = await updateTask(taskId, {
        taskName,
        description: description || undefined,
        priority,
        deadline: deadline || undefined
      });
      
      setCategoriesByMilestone(prev => {
        const updated = { ...prev };
        for (const mId in updated) {
          updated[mId] = updated[mId].map(cat => ({
            ...cat,
            tasks: cat.tasks?.map(task => 
              task.id === taskId ? { ...task, taskName: updatedTask.taskName, description: updatedTask.description, priority: updatedTask.priority, deadline: updatedTask.deadline } : task
            ) || []
          }));
        }
        return updated;
      });
      setEditingTask(null);
    } catch (err) {
      alert(err.message || 'Failed to update task');
    } finally {
      setTaskLoading(prev => ({ ...prev, [taskId]: false }));
    }
  };

  return (
    <div className="app-page">
      <PageHeader
        title="Event Detail"
        description="Manage event milestones, categories, and tasks"
        actions={
          <>
            <button 
              onClick={() => navigate(`/org/events?orgId=${orgId}`)}
              className="app-button app-button--ghost"
            >
              Back to Events
            </button>
            {canManage && (
              <button onClick={() => setEditingEvent(!editingEvent)} className="app-button app-button--primary">
                {editingEvent ? 'Cancel Edit' : 'Edit Event'}
              </button>
            )}
          </>
        }
      />

      <div className="app-section">
        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Event Information</h3>
          </div>
          
          {editingEvent ? (
            <form onSubmit={handleUpdateEvent} className="auth-form">
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: '0.9rem' }}>
                <div className="form-group">
                  <label htmlFor="eventName" className="form-label">Event Name *</label>
                  <input id="eventName" name="eventName" className="form-input" defaultValue={getEventName(event)} required />
                </div>
                <div className="form-group">
                  <label htmlFor="description" className="form-label">Description</label>
                  <input id="description" name="description" className="form-input" defaultValue={event?.description || ''} />
                </div>
                <div className="form-group">
                  <label htmlFor="startDate" className="form-label">Start Date *</label>
                  <input id="startDate" name="startDate" type="date" className="form-input" defaultValue={event?.startDate ? String(event.startDate).split('T')[0] : ''} required />
                </div>
                <div className="form-group">
                  <label htmlFor="endDate" className="form-label">End Date</label>
                  <input id="endDate" name="endDate" type="date" className="form-input" defaultValue={event?.endDate ? String(event.endDate).split('T')[0] : ''} />
                </div>
                <div className="form-group">
                  <label htmlFor="location" className="form-label">Location</label>
                  <input id="location" name="location" className="form-input" defaultValue={event?.location || ''} />
                </div>
                <div className="form-group">
                  <label htmlFor="bannerUrl" className="form-label">Banner URL</label>
                  <input id="bannerUrl" name="bannerUrl" className="form-input" defaultValue={event?.bannerUrl || ''} />
                </div>
                <div className="form-group">
                  <label htmlFor="visibility" className="form-label">Visibility</label>
                  <select id="visibility" name="visibility" defaultValue={event?.visibility || 'Private'} className="form-select">
                    <option value="Public">Public</option>
                    <option value="OrganizationOnly">Organization Only</option>
                    <option value="Private">Private</option>
                  </select>
                </div>
              </div>
              <div className="app-action-row" style={{ marginTop: '1rem' }}>
                <button type="submit" disabled={isEventUpdating} className="app-button app-button--primary">
                  {isEventUpdating ? 'Updating...' : 'Save Changes'}
                </button>
              </div>
            </form>
          ) : (
            <table>
              <tbody>
                <tr>
                  <th>Name</th>
                  <td>{getEventName(event) || '-'}</td>
                </tr>
                <tr>
                  <th>Description</th>
                  <td>{event?.description || '-'}</td>
                </tr>
                <tr>
                  <th>Start Date</th>
                  <td>{event?.startDate ? new Date(event.startDate).toLocaleDateString() : '-'}</td>
                </tr>
                <tr>
                  <th>End Date</th>
                  <td>{event?.endDate ? new Date(event.endDate).toLocaleDateString() : '-'}</td>
                </tr>
                <tr>
                  <th>Location</th>
                  <td>{event?.location || '-'}</td>
                </tr>
                <tr>
                  <th>Status</th>
                  <td>{event?.status || '-'}</td>
                </tr>
                <tr>
                  <th>Visibility</th>
                  <td>{event?.visibility || '-'}</td>
                </tr>
              </tbody>
            </table>
          )}
        </div>

        <div className="app-card">
          <div className="app-section-header">
            <h3 className="app-section-title">Milestones</h3>
            {canManage && (
              <div className="app-action-row">
                {showCreateMilestone ? (
                  <form onSubmit={handleCreateMilestone} className="auth-form">
                    <div className="form-group">
                      <label className="form-label">Title *</label>
                      <input
                        name="title"
                        placeholder="Title"
                        required
                        className="form-input"
                      />
                    </div>
                    <div className="form-group">
                      <label className="form-label">Description</label>
                      <input
                        name="description"
                        placeholder="Description"
                        className="form-input"
                      />
                    </div>
                    <div className="app-action-row">
                      <button type="submit" disabled={milestoneLoading.create} className="app-button app-button--primary">
                        {milestoneLoading.create ? 'Creating...' : 'Create'}
                      </button>
                      <button type="button" onClick={() => setShowCreateMilestone(false)} className="app-button app-button--ghost">
                        Cancel
                      </button>
                    </div>
                  </form>
                ) : (
                  <button onClick={() => setShowCreateMilestone(true)} className="app-button app-button--primary">
                    Add Milestone
                  </button>
                )}
              </div>
            )}
          </div>
          {milestones.length === 0 ? (
            <EmptyState message="No milestones found" />
          ) : (
            milestones.map((milestone) => (
              <div key={milestone.id} className="app-card" style={{ marginBottom: '1rem' }}>
                <div className="app-section-header">
                  <h4 className="app-section-title">{milestone.title || '-'}</h4>
                  {canManage && (
                    <div className="app-action-row">
                      <button
                        onClick={() => setEditingMilestone(editingMilestone === milestone.id ? null : milestone.id)}
                        className="app-button app-button--secondary"
                      >
                        {editingMilestone === milestone.id ? 'Cancel' : 'Edit'}
                      </button>
                      <button
                        onClick={() => handleDeleteMilestone(milestone.id)}
                        disabled={milestoneLoading[milestone.id]}
                        className="app-button app-button--danger"
                      >
                        {milestoneLoading[milestone.id] ? 'Deleting...' : 'Delete'}
                      </button>
                    </div>
                  )}
                </div>
                
                {editingMilestone === milestone.id ? (
                  <form onSubmit={(e) => handleUpdateMilestone(milestone.id, e)} className="auth-form" style={{ marginBottom: '1rem' }}>
                    <div className="app-action-row">
                      <input name="title" className="form-input" defaultValue={milestone.title} required style={{ width: '200px' }} />
                      <input name="description" className="form-input" defaultValue={milestone.description || ''} style={{ width: '300px' }} placeholder="Description" />
                      <button type="submit" disabled={milestoneLoading[milestone.id]} className="app-button app-button--primary">
                        {milestoneLoading[milestone.id] ? 'Saving...' : 'Save'}
                      </button>
                    </div>
                  </form>
                ) : (
                  milestone.description && <p style={{ marginBottom: '1rem' }}>{milestone.description}</p>
                )}
                
                <div className="app-section-header">
                  <h5 className="app-section-title">Categories</h5>
                  {canManage && (
                    <div className="app-action-row">
                      {showCreateCategory[milestone.id] ? (
                        <form onSubmit={(e) => handleCreateCategory(milestone.id, e)} className="auth-form">
                          <div className="form-group">
                            <label className="form-label">Category Name *</label>
                            <input
                              name="categoryName"
                              placeholder="Category name"
                              required
                              className="form-input"
                            />
                          </div>
                          <div className="form-group">
                            <label className="form-label">Description</label>
                            <input
                              name="description"
                              placeholder="Description"
                              className="form-input"
                            />
                          </div>
                          <div className="app-action-row">
                            <button type="submit" disabled={categoryLoading[milestone.id]} className="app-button app-button--primary">
                              {categoryLoading[milestone.id] ? 'Creating...' : 'Create'}
                            </button>
                            <button
                              type="button"
                              onClick={() => setShowCreateCategory(prev => ({ ...prev, [milestone.id]: false }))}
                              className="app-button app-button--ghost"
                            >
                              Cancel
                            </button>
                          </div>
                        </form>
                      ) : (
                        <button onClick={() => setShowCreateCategory(prev => ({ ...prev, [milestone.id]: true }))} className="app-button app-button--secondary">
                          Add Category
                        </button>
                      )}
                    </div>
                  )}
                </div>
                {categoriesByMilestone[milestone.id]?.length === 0 ? (
                  <EmptyState message="No categories found" />
                ) : (
                  categoriesByMilestone[milestone.id]?.map((category) => (
                    <div key={category.id} className="app-card" style={{ marginBottom: '1rem' }}>
                      <div className="app-section-header">
                        <h6 className="app-section-title">{category.categoryName || '-'}</h6>
                        {canManage && (
                          <div className="app-action-row">
                            <button
                              onClick={() => setEditingCategory(editingCategory === category.id ? null : category.id)}
                              className="app-button app-button--secondary"
                            >
                              {editingCategory === category.id ? 'Cancel' : 'Edit'}
                            </button>
                            <button
                              onClick={() => handleDeleteCategory(category.id, milestone.id)}
                              disabled={categoryLoading[category.id]}
                              className="app-button app-button--danger"
                            >
                              {categoryLoading[category.id] ? 'Deleting...' : 'Delete'}
                            </button>
                          </div>
                        )}
                      </div>
                      
                      {editingCategory === category.id && (
                        <form onSubmit={(e) => handleUpdateCategory(category.id, milestone.id, e)} className="auth-form" style={{ marginBottom: '1rem' }}>
                          <div className="app-action-row">
                            <input name="categoryName" className="form-input" defaultValue={category.categoryName} required style={{ width: '200px' }} />
                            <input name="description" className="form-input" defaultValue={category.description || ''} style={{ width: '300px' }} placeholder="Description" />
                            <button type="submit" disabled={categoryLoading[category.id]} className="app-button app-button--primary">
                              {categoryLoading[category.id] ? 'Saving...' : 'Save'}
                            </button>
                          </div>
                        </form>
                      )}

                      <div className="app-section-header">
                        <span className="app-section-title" style={{ fontSize: '0.9rem' }}>Tasks</span>
                        {canManage && (
                          <form onSubmit={(e) => handleCreateTask(category.id, e)} className="auth-form" style={{ display: 'inline' }}>
                            <div className="app-action-row">
                              <input
                                name="taskName"
                                placeholder="Task name *"
                                required
                                className="form-input"
                                style={{ width: '200px' }}
                              />
                              <input
                                name="description"
                                placeholder="Description"
                                className="form-input"
                                style={{ width: '200px' }}
                              />
                              <select name="priority" defaultValue="Medium" className="form-select">
                                <option value="Low">Low</option>
                                <option value="Medium">Medium</option>
                                <option value="High">High</option>
                                <option value="Urgent">Urgent</option>
                              </select>
                              <input
                                name="deadline"
                                type="date"
                                className="form-input"
                              />
                              <button type="submit" disabled={taskLoading[category.id]} className="app-button app-button--primary">
                                {taskLoading[category.id] ? 'Creating...' : 'Add'}
                              </button>
                            </div>
                          </form>
                        )}
                      </div>
                      {category.tasks?.length === 0 ? (
                        <EmptyState message="No tasks found" />
                      ) : (
                        <table>
                          <thead>
                            <tr>
                              <th>Task Name</th>
                              <th>Description</th>
                              <th>Priority</th>
                              <th>Status</th>
                              <th>Assignee</th>
                              {canManage && <th>Actions</th>}
                            </tr>
                          </thead>
                          <tbody>
                            {category.tasks?.map((task) => (
                              <tr key={task.id}>
                                <td>
                                  {editingTask === task.id ? (
                                    <input form={`edit-task-${task.id}`} name="taskName" defaultValue={task.taskName} required className="form-input" style={{ width: '150px' }} />
                                  ) : (
                                    task.taskName || '-'
                                  )}
                                </td>
                                <td>
                                  {editingTask === task.id ? (
                                    <input form={`edit-task-${task.id}`} name="description" defaultValue={task.description || ''} className="form-input" style={{ width: '150px' }} />
                                  ) : (
                                    task.description || '-'
                                  )}
                                </td>
                                <td>
                                  {editingTask === task.id ? (
                                    <select form={`edit-task-${task.id}`} name="priority" defaultValue={task.priority} className="form-select">
                                      <option value="Low">Low</option>
                                      <option value="Medium">Medium</option>
                                      <option value="High">High</option>
                                      <option value="Urgent">Urgent</option>
                                    </select>
                                  ) : (
                                    task.priority || '-'
                                  )}
                                </td>
                                <td>
                                  {canManage ? (
                                    <select
                                      value={task.status}
                                      onChange={(e) => handleUpdateStatus(task.id, e.target.value, category.id)}
                                      disabled={taskLoading[task.id]}
                                      className="form-select"
                                      style={{ minWidth: '100px' }}
                                    >
                                      <option value="Todo">Todo</option>
                                      <option value="InProgress">In Progress</option>
                                      <option value="Blocked">Blocked</option>
                                      <option value="Done">Done</option>
                                      <option value="Cancelled">Cancelled</option>
                                    </select>
                                  ) : (
                                    task.status || '-'
                                  )}
                                </td>
                                <td>
                                  {canManage ? (
                                    <select
                                      value={getTaskAssigneeId(task)}
                                      onChange={(e) => handleAssignTask(task.id, e.target.value, category.id)}
                                      disabled={taskLoading[task.id]}
                                      className="form-select"
                                      style={{ minWidth: '150px' }}
                                    >
                                      <option value="">Unassigned</option>
                                      {members.map(member => (
                                        <option key={member.id} value={member.id}>
                                          {member.fullName || member.email}
                                        </option>
                                      ))}
                                    </select>
                                  ) : (
                                    getTaskAssigneeName(task)
                                  )}
                                </td>
                                {canManage && (
                                  <td>
                                    <div className="app-action-row">
                                      <form id={`edit-task-${task.id}`} onSubmit={(e) => handleUpdateTask(task.id, e)} style={{ display: 'none' }}></form>
                                      {editingTask === task.id ? (
                                        <>
                                          <button form={`edit-task-${task.id}`} type="submit" disabled={taskLoading[task.id]} className="app-button app-button--primary">
                                            Save
                                          </button>
                                          <button onClick={() => setEditingTask(null)} type="button" className="app-button app-button--ghost">
                                            Cancel
                                          </button>
                                        </>
                                      ) : (
                                        <>
                                          <button
                                            onClick={() => setEditingTask(task.id)}
                                            className="app-button app-button--secondary"
                                          >
                                            Edit
                                          </button>
                                          <button
                                            onClick={() => handleDeleteTask(task.id, category.id)}
                                            disabled={taskLoading[task.id]}
                                            className="app-button app-button--danger"
                                          >
                                            {taskLoading[task.id] ? 'Deleting...' : 'Delete'}
                                          </button>
                                        </>
                                      )}
                                    </div>
                                  </td>
                                )}
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      )}
                    </div>
                  ))
                )}
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}

export default OrgEventDetailPage;
