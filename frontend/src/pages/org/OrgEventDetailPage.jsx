/**
 * OrgEventDetailPage.jsx - Organization event detail page (EventDetail tree root)
 * 
 * Phase 4B-1: Real backend API integration
 */

import { useState, useEffect } from 'react';
import { useParams, useSearchParams, useNavigate } from 'react-router-dom';
import { useOrgContext } from '../../contexts/OrgContext.jsx';
import { getEventById } from '../../services/eventService.js';
import { getEventMilestones } from '../../services/milestoneService.js';
import { getMilestoneCategories } from '../../services/categoryService.js';
import { createTask, updateTaskStatus, assignTask, deleteTask } from '../../services/taskService.js';
import { getOrganizationMembers } from '../../services/memberService.js';
import { createMilestone, updateMilestone, deleteMilestone } from '../../services/milestoneService.js';
import { createCategory, updateCategory, deleteCategory } from '../../services/categoryService.js';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
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
              <button disabled className="app-button app-button--primary">
                Edit Event
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
          <table>
            <tbody>
              <tr>
                <th>Name</th>
                <td>{event?.eventName || '-'}</td>
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
                <th>Status</th>
                <td>{event?.status || '-'}</td>
              </tr>
              <tr>
                <th>Visibility</th>
                <td>{event?.visibility || '-'}</td>
              </tr>
            </tbody>
          </table>
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
                    <button
                      onClick={() => handleDeleteMilestone(milestone.id)}
                      disabled={milestoneLoading[milestone.id]}
                      className="app-button app-button--danger"
                    >
                      {milestoneLoading[milestone.id] ? 'Deleting...' : 'Delete Milestone'}
                    </button>
                  )}
                </div>
                {milestone.description && <p style={{ marginBottom: '1rem' }}>{milestone.description}</p>}
                
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
                          <button
                            onClick={() => handleDeleteCategory(category.id, milestone.id)}
                            disabled={categoryLoading[category.id]}
                            className="app-button app-button--danger"
                          >
                            {categoryLoading[category.id] ? 'Deleting...' : 'Delete Category'}
                          </button>
                        )}
                      </div>
                      
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
                                <td>{task.taskName || '-'}</td>
                                <td>{task.description || '-'}</td>
                                <td>{task.priority || '-'}</td>
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
                                      value={task.assigneeId || ''}
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
                                    task.assignee?.user?.fullName || '-'
                                  )}
                                </td>
                                {canManage && (
                                  <td>
                                    <button
                                      onClick={() => handleDeleteTask(task.id, category.id)}
                                      disabled={taskLoading[task.id]}
                                      className="app-button app-button--danger"
                                    >
                                      {taskLoading[task.id] ? 'Deleting...' : 'Delete'}
                                    </button>
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
