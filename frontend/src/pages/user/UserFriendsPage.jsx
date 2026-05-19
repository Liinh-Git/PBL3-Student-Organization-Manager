/**
 * UserFriendsPage.jsx - User friends page
 * 
 * Phase 3C-4C: Page skeleton only
 * 
 * TODO Phase 3C-5+ Implementation:
 * - Load user's friends list
 * - Load friend requests (sent and received)
 * - Display friends list
 * - Display friend requests with accept/reject buttons
 * - Add "Send Friend Request" button
 * 
 * Future Service Usage:
 * - friendService.getFriends(params)
 * - friendService.getFriendRequests(params)
 * - friendService.acceptFriendRequest(id)
 * - friendService.rejectFriendRequest(id)
 * 
 * Future Adapter Usage:
 * - friendAdapter.toFriendViewModel()
 * - friendAdapter.toFriendRequestViewModel()
 * 
 * Permissions:
 * - JWT (authenticated user)
 * 
 * Route:
 * - /user/friends
 * 
 * State Management:
 * - TODO: useState for friends list
 * - TODO: useState for friend requests
 * - TODO: useEffect to load friends and requests
 * - TODO: Loading state
 * - TODO: Error state
 * - TODO: Empty state
 * 
 * IMPORTANT:
 * - No real API calls in Phase 3C
 * - No fake friend data
 * - No mock friend cards
 */

import { useState } from 'react';
import PageHeader from '../../components/shared/PageHeader';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import EmptyState from '../../components/shared/EmptyState';
import ErrorState from '../../components/shared/ErrorState';

function UserFriendsPage() {
  // TODO Phase 3C-5+: Add state management
  // const [friends, setFriends] = useState([]);
  // const [friendRequests, setFriendRequests] = useState([]);
  // const [isLoading, setIsLoading] = useState(false);
  // const [error, setError] = useState(null);

  // TODO Phase 3C-5+: Load friends and requests
  // useEffect(() => {
  //   async function loadData() {
  //     setIsLoading(true);
  //     try {
  //       const [friendsData, requestsData] = await Promise.all([
  //         friendService.getFriends(),
  //         friendService.getFriendRequests()
  //       ]);
  //       const adaptedFriends = friendsData.map(friendAdapter.toFriendViewModel);
  //       const adaptedRequests = requestsData.map(friendAdapter.toFriendRequestViewModel);
  //       setFriends(adaptedFriends);
  //       setFriendRequests(adaptedRequests);
  //     } catch (err) {
  //       setError(err.message);
  //     } finally {
  //       setIsLoading(false);
  //     }
  //   }
  //   loadData();
  // }, []);

  // TODO Phase 3C-5+: Handle accept friend request
  // const handleAccept = async (requestId) => {
  //   try {
  //     await friendService.acceptFriendRequest(requestId);
  //     // Reload data
  //   } catch (err) {
  //     // Show error
  //   }
  // };

  // TODO Phase 3C-5+: Handle reject friend request
  // const handleReject = async (requestId) => {
  //   try {
  //     await friendService.rejectFriendRequest(requestId);
  //     // Reload data
  //   } catch (err) {
  //     // Show error
  //   }
  // };

  return (
    <div className="user-friends-page">
      <PageHeader
        title="Bạn bè"
        description="Quản lý bạn bè và lời mời kết bạn"
      />

      {/* TODO Phase 3C-5+: Show LoadingSpinner when isLoading */}
      {/* TODO Phase 3C-5+: Show ErrorState when error */}

      {/* TODO Phase 3C-5+: Friend requests section */}
      <section className="friend-requests-section">
        <h2>Lời mời kết bạn</h2>
        <div className="friend-requests-list">
          {/* TODO: Show EmptyState when no requests */}
          {/* TODO: Render friend request cards with accept/reject buttons */}
        </div>
      </section>

      {/* TODO Phase 3C-5+: Friends list section */}
      <section className="friends-section">
        <h2>Bạn bè của tôi</h2>
        <div className="friends-list">
          {/* TODO: Show EmptyState when no friends */}
          {/* TODO: Render friend cards */}
        </div>
      </section>
    </div>
  );
}

export default UserFriendsPage;
