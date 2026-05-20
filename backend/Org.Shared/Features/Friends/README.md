# Friends Module Contracts

## Module Purpose
Friend request management (send, accept, reject).

## Scope Status
**SUPPORTING** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Friends/`

## Related Domain Entities
- `FriendRequest`, `User`, `FriendRequestStatus` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/friends` | JWT | None | `ApiResponse<ListResponse<FriendDto>>` |
| GET | `/api/friends/requests` | JWT | None | `ApiResponse<ListResponse<FriendRequestDto>>` |
| POST | `/api/friends/requests` | JWT | `SendFriendRequestRequest` | `ApiResponse<FriendRequestDto>` |
| POST | `/api/friends/requests/{id}/accept` | JWT | None | `ApiResponse<bool>` |
| POST | `/api/friends/requests/{id}/reject` | JWT | None | `ApiResponse<bool>` |

## Future Request DTO Names
- `SendFriendRequestRequest`

## Future Response DTO Names
- `FriendDto`, `FriendRequestDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/friendService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/friendAdapter.js` (if needed)

## Future Page/Component Files
- `frontend/org-frontend/src/pages/user/UserFriendsPage.jsx`

## Required Permissions
- All routes require JWT token (authenticated user)

## Contract Notes

### FriendDto
- **Fields**: `Id`, `FullName`, `Email`, `AvatarUrl?`, `Status`
- **Note**: Accepted friends list

### FriendRequestDto
- **Fields**: `Id`, `SenderId`, `SenderName`, `SenderAvatarUrl?`, `ReceiverId`, `ReceiverName`, `ReceiverAvatarUrl?`, `Status`, `CreatedAtUtc`, `RespondedAt?`
- **Note**: Pending/sent/received friend requests

### SendFriendRequestRequest
- **Fields**: `ReceiverId`
- **Validation**: SenderId != ReceiverId (service-level check)

## Validation Notes
- **ReceiverId**: Required, must exist, must not be self
- **SenderId != ReceiverId**: Service-level check

## Mapping Notes
- **Entity → DTO**: Map `FriendRequest` entity to DTOs, include user names/avatars
- **DTO → Entity**: Map request DTOs to `FriendRequest` entity

## What is NOT Implemented in This Phase
- ❌ No real friend request logic
- ❌ Only contract skeleton/TODO files

## Important Note
**SenderId != ReceiverId** is enforced at service level, not DB constraint.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Friends/`
- **Shared Contract**: `backend/Org.Shared/Features/Friends/FriendContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/friendService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/friendAdapter.js`
- **Frontend Pages**: `UserFriendsPage.jsx`

---

**End of Friends README.md**
