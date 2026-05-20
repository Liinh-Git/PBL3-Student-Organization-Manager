# Friends Services

## IFriendService / FriendService
**Methods**:
- `Task<List<UserDto>> ListFriendsAsync(Guid userId)`
- `Task<List<FriendRequestDto>> ListFriendRequestsAsync(Guid userId)`
- `Task<FriendRequestDto> SendFriendRequestAsync(Guid senderId, Guid receiverId)`
- `Task AcceptFriendRequestAsync(Guid requestId, Guid userId)`
- `Task RejectFriendRequestAsync(Guid requestId, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
