# Friends Module

## Module Purpose
Friend request management between users.

## Scope Status
**SUPPORTING** - Full backend skeleton required

## Related Domain Entities
- `FriendRequest`, `User`
- Enums: `FriendRequestStatus`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/friends` | List user's friends |
| GET | `/api/friends/requests` | List friend requests (sent and received) |
| POST | `/api/friends/requests` | Send friend request |
| POST | `/api/friends/requests/{id}/accept` | Accept friend request |
| POST | `/api/friends/requests/{id}/reject` | Reject friend request |

## Required Permissions
- Valid JWT token (user-scoped endpoints)

## Important Notes
- Supporting module for social features
- Related entity: FriendRequest
- SenderId != ReceiverId (enforce at service level)

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Friends/FriendContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/friendService.js`
- Future adapter: `frontend/org-frontend/src/adapters/friendAdapter.js`
- Future page: `UserFriendsPage.jsx`
- Permissions: Valid JWT token
- Status: **SUPPORTING**
