/**
 * OrgTasksPlaceholderPage.jsx - Aggregate task board placeholder
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * PROTOTYPE_ONLY: This page is a placeholder for the aggregate task board.
 * 
 * IMPORTANT:
 * - Task is CORE inside EventDetail tree (OrgEventDetailPage)
 * - Only /org/tasks aggregate board is PROTOTYPE_ONLY
 * - No service file for aggregate board
 * - No adapter file for aggregate board
 * - No API calls
 * - No fake board
 * - No fake task cards
 * 
 * Route: /org/tasks?orgId=
 */

import { useSearchParams } from 'react-router-dom';
import PageHeader from '../../components/shared/PageHeader';
import PrototypePlaceholder from '../../components/shared/PrototypePlaceholder';
import ErrorState from '../../components/shared/ErrorState';

function OrgTasksPlaceholderPage() {
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId');

  if (!orgId) {
    return <ErrorState message="Organization ID is required" />;
  }

  return (
    <div className="org-tasks-placeholder-page">
      <PageHeader
        title="Task Board"
        description="Aggregate task board across all events"
      />

      <PrototypePlaceholder
        title="Aggregate Task Board"
        description="This feature displays all tasks across all events in a unified board view"
        status="PROTOTYPE_ONLY"
        notes="Task CRUD is fully available inside the EventDetail tree (Event → Milestone → Category → Task). This aggregate board view is not implemented in the base prototype."
      />
    </div>
  );
}

export default OrgTasksPlaceholderPage;
