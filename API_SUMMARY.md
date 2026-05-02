# 📋 Tóm tắt Tất cả API đã Triển khai

## 🔐 Authentication APIs

### 1. Đăng ký tài khoản
```
POST /api/auth/register
```
**Request:**
```json
{
  "fullName": "string",
  "email": "string",
  "password": "string"
}
```
**Response:** 201 Created - UserDto

---

### 2. Đăng nhập
```
POST /api/auth/login
```
**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```
**Response:** 200 OK
```json
{
  "accessToken": "string",
  "user": { UserDto }
}
```

---

### 3. Lấy thông tin người dùng hiện tại
```
GET /api/auth/me
```
**Response:** 200 OK - UserDto

---

## 👤 User Profile APIs

### 4. Xem hồ sơ người dùng
```
GET /api/users/{id}
```
**Response:** 200 OK - UserDto

**Logic:**
- Nếu là chính mình → cho phép xem
- Nếu profile là `Public` → ai cũng xem được
- Nếu profile là `Private` → chỉ chủ nhân xem được
- Nếu profile là `OrganizationOnly` → kiểm tra cùng tổ chức

---

### 5. Cập nhật hồ sơ cá nhân
```
PUT /api/users/me
```
**Request:**
```json
{
  "fullName": "string",
  "profileVisibility": "Public|OrganizationOnly|Private"
}
```
**Response:** 200 OK - UserDto

---

### 6. Đổi mật khẩu
```
PUT /api/users/me/change-password
```
**Request:**
```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```
**Response:** 204 No Content

**Validation:**
- Verify mật khẩu hiện tại bằng BCrypt
- Mật khẩu mới tối thiểu 6 ký tự
- Hash mật khẩu mới bằng BCrypt

---

### 7. Lấy thông tin nhiều người (Batch API)
```
POST /api/users/batch
```
**Request:**
```json
[
  "guid1",
  "guid2",
  "guid3"
]
```
**Response:** 200 OK
```json
{
  "items": [
    { UserDto },
    { UserDto }
  ]
}
```

**Features:**
- Tối đa 100 users
- Tự động filter theo profile visibility
- Chỉ trả về profiles mà caller có quyền xem

---

## 👥 Friend Request APIs

### 8. Gửi lời mời kết bạn
```
POST /api/users/{id}/friend-request
```
**Response:** 201 Created - FriendRequestDto

**Validation:**
- Không gửi cho chính mình
- Không gửi nếu đã là bạn
- Không gửi nếu đã có request pending

---

### 9. Lấy danh sách lời mời đến
```
GET /api/users/me/friend-requests
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "senderId": "guid",
      "senderName": "string",
      "status": "Pending|Accepted|Rejected|Cancelled",
      "createdAt": "datetime"
    }
  ]
}
```

---

### 10. Chấp nhận lời mời kết bạn
```
PUT /api/users/me/friend-requests/{id}/accept
```
**Response:** 200 OK - FriendRequestDto

**Action:**
- Cập nhật status thành `Accepted`
- Tạo friendship record

---

### 11. Từ chối/Hủy lời mời kết bạn
```
DELETE /api/users/me/friend-requests/{id}
```
**Response:** 204 No Content

**Logic:**
- Nếu là receiver → từ chối (status = Rejected)
- Nếu là sender → hủy (status = Cancelled)

---

### 12. Lấy danh sách bạn bè
```
GET /api/users/me/friends
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "fullName": "string",
      "email": "string",
      "profileVisibility": "string",
      "friendSince": "datetime"
    }
  ]
}
```

---

### 13. Hủy kết bạn
```
DELETE /api/users/me/friends/{id}
```
**Response:** 204 No Content

---

## 🏢 Organization APIs

### 14. Lấy danh sách tổ chức
```
GET /api/organizations
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "memberCount": "int"
    }
  ]
}
```

---

### 15. Tạo tổ chức mới
```
POST /api/organizations
```
**Request:**
```json
{
  "name": "string",
  "description": "string"
}
```
**Response:** 201 Created - OrganizationDto

---

### 16. Cập nhật tổ chức
```
PUT /api/organizations/{id}
```
**Request:**
```json
{
  "name": "string",
  "description": "string"
}
```
**Response:** 200 OK - OrganizationDto

---

### 17. Xóa tổ chức
```
DELETE /api/organizations/{id}
```
**Response:** 204 No Content

---

## 👨‍💼 Member Management APIs

### 18. Lấy danh sách thành viên
```
GET /api/organizations/{orgId}/members
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "fullName": "string",
      "email": "string",
      "role": "Member|Manager|VicePresident|President",
      "joinDate": "datetime"
    }
  ]
}
```

---

### 19. Thêm thành viên vào tổ chức
```
POST /api/organizations/{orgId}/members
```
**Request:**
```json
{
  "fullName": "string",
  "email": "string",
  "departmentId": "guid?"
}
```
**Response:** 201 Created - MemberDto

**Features:**
- Tự tạo User nếu chưa có
- Tái kích hoạt nếu tài khoản đã bị xóa mềm
- Gán vào phòng ban nếu có

---

### 20. Cập nhật vai trò thành viên
```
PUT /api/members/{id}/role
```
**Request:**
```json
{
  "role": "Member|Manager|VicePresident|President"
}
```
**Response:** 200 OK - MemberDto

**Features:**
- Tự tạo Role mới nếu chưa có
- Gửi thông báo cho thành viên

---

### 21. Phân công thành viên vào phòng ban
```
PUT /api/members/{id}/department
```
**Request:**
```json
{
  "departmentId": "guid?"
}
```
**Response:** 200 OK - MemberDto

---

### 22. Xóa thành viên khỏi tổ chức
```
DELETE /api/members/{id}
```
**Response:** 204 No Content

**Action:**
- Soft delete thành viên
- Cập nhật ManagerId = null cho các phòng ban đang quản lý
- Gửi thông báo cho thành viên

---

## 🏢 Department APIs

### 23. Lấy danh sách phòng ban
```
GET /api/organizations/{orgId}/departments
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "code": "string",
      "description": "string",
      "managerId": "guid?",
      "managerName": "string?",
      "memberCount": "int"
    }
  ]
}
```

---

### 24. Lấy chi tiết phòng ban
```
GET /api/departments/{id}
```
**Response:** 200 OK - DepartmentDto

---

### 25. Tạo phòng ban
```
POST /api/organizations/{orgId}/departments
```
**Request:**
```json
{
  "name": "string",
  "code": "string",
  "description": "string"
}
```
**Response:** 201 Created - DepartmentDto

---

### 26. Cập nhật phòng ban
```
PUT /api/departments/{id}
```
**Request:**
```json
{
  "name": "string",
  "code": "string",
  "description": "string"
}
```
**Response:** 200 OK - DepartmentDto

---

### 27. Gán quản lý phòng ban
```
PUT /api/departments/{id}/manager
```
**Request:**
```json
{
  "managerId": "guid"
}
```
**Response:** 200 OK - DepartmentDto

---

### 28. Xóa phòng ban
```
DELETE /api/departments/{id}
```
**Response:** 204 No Content

---

## 📅 Event APIs

### 29. Lấy danh sách sự kiện
```
GET /api/organizations/{orgId}/events
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "title": "string",
      "description": "string",
      "startTime": "datetime",
      "endTime": "datetime",
      "location": "string",
      "categoryId": "guid",
      "categoryName": "string",
      "attendeeCount": "int",
      "status": "Scheduled|Ongoing|Completed|Cancelled"
    }
  ]
}
```

---

### 30. Tạo sự kiện
```
POST /api/organizations/{orgId}/events
```
**Request:**
```json
{
  "title": "string",
  "description": "string",
  "startTime": "datetime",
  "endTime": "datetime",
  "location": "string",
  "categoryId": "guid"
}
```
**Response:** 201 Created - EventDto

---

### 31. Cập nhật sự kiện
```
PUT /api/events/{id}
```
**Request:**
```json
{
  "title": "string",
  "description": "string",
  "startTime": "datetime",
  "endTime": "datetime",
  "location": "string",
  "categoryId": "guid"
}
```
**Response:** 200 OK - EventDto

**Action:**
- Gửi thông báo cho tất cả attendees

---

### 32. Hủy sự kiện
```
DELETE /api/events/{id}
```
**Response:** 204 No Content

**Action:**
- Soft delete sự kiện
- Gửi thông báo cho tất cả attendees

---

### 33. Đăng ký tham dự sự kiện
```
POST /api/events/{id}/attendees
```
**Response:** 201 Created - AttendeeDto

---

### 34. Hủy đăng ký tham dự
```
DELETE /api/events/{id}/attendees
```
**Response:** 204 No Content

---

## 📂 Event Category APIs

### 35. Lấy danh sách danh mục sự kiện
```
GET /api/organizations/{orgId}/event-categories
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "description": "string"
    }
  ]
}
```

---

### 36. Tạo danh mục sự kiện
```
POST /api/organizations/{orgId}/event-categories
```
**Request:**
```json
{
  "name": "string",
  "description": "string"
}
```
**Response:** 201 Created - EventCategoryDto

---

### 37. Cập nhật danh mục sự kiện
```
PUT /api/event-categories/{id}
```
**Request:**
```json
{
  "name": "string",
  "description": "string"
}
```
**Response:** 200 OK - EventCategoryDto

---

### 38. Xóa danh mục sự kiện
```
DELETE /api/event-categories/{id}
```
**Response:** 204 No Content

---

## 📋 Task APIs

### 39. Lấy danh sách công việc
```
GET /api/organizations/{orgId}/tasks
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "title": "string",
      "description": "string",
      "status": "Todo|InProgress|Done",
      "priority": "Low|Medium|High",
      "assigneeId": "guid?",
      "assigneeName": "string?",
      "dueDate": "datetime?",
      "createdAt": "datetime"
    }
  ]
}
```

---

### 40. Tạo công việc
```
POST /api/organizations/{orgId}/tasks
```
**Request:**
```json
{
  "title": "string",
  "description": "string",
  "priority": "Low|Medium|High",
  "dueDate": "datetime?"
}
```
**Response:** 201 Created - TaskDto

---

### 41. Cập nhật công việc
```
PUT /api/tasks/{id}
```
**Request:**
```json
{
  "title": "string",
  "description": "string",
  "priority": "Low|Medium|High",
  "dueDate": "datetime?"
}
```
**Response:** 200 OK - TaskDto

---

### 42. Gán công việc cho thành viên
```
PUT /api/tasks/{id}/assign
```
**Request:**
```json
{
  "assigneeId": "guid"
}
```
**Response:** 200 OK - TaskDto

**Action:**
- Gửi thông báo cho người được gán

---

### 43. Cập nhật trạng thái công việc
```
PUT /api/tasks/{id}/status
```
**Request:**
```json
{
  "status": "Todo|InProgress|Done"
}
```
**Response:** 200 OK - TaskDto

**Action:**
- Gửi thông báo cho người được gán (nếu khác người thay đổi)

---

### 44. Xóa công việc
```
DELETE /api/tasks/{id}
```
**Response:** 204 No Content

---

## 📌 Milestone APIs

### 45. Lấy danh sách mục tiêu
```
GET /api/organizations/{orgId}/milestones
```
**Response:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "title": "string",
      "description": "string",
      "targetDate": "datetime",
      "status": "NotStarted|InProgress|Completed|Cancelled"
    }
  ]
}
```

---

### 46. Tạo mục tiêu
```
POST /api/organizations/{orgId}/milestones
```
**Request:**
```json
{
  "title": "string",
  "description": "string",
  "targetDate": "datetime"
}
```
**Response:** 201 Created - MilestoneDto

---

### 47. Cập nhật mục tiêu
```
PUT /api/milestones/{id}
```
**Request:**
```json
{
  "title": "string",
  "description": "string",
  "targetDate": "datetime"
}
```
**Response:** 200 OK - MilestoneDto

---

### 48. Xóa mục tiêu
```
DELETE /api/milestones/{id}
```
**Response:** 204 No Content

---

## 🔔 Notification APIs (Phase 3 - Real-time)

### 49. SignalR Hub Connection
```
WebSocket: ws://localhost:5058/hubs/notifications?access_token=YOUR_JWT_TOKEN
```

**Events:**
- `ReceiveNotification` - Nhận thông báo real-time

**Notification Message Format:**
```json
{
  "id": "guid",
  "title": "string",
  "message": "string (Vietnamese)",
  "type": "string",
  "actorId": "guid",
  "relatedEntityId": "guid?",
  "relatedEntityType": "string?",
  "actionUrl": "string?",
  "iconUrl": "string?",
  "timestamp": "datetime"
}
```

---

## 🛠️ Admin APIs

### 50. Apply Database Migration
```
POST /api/admin/apply-migration
```
**Response:** 200 OK

---

## 📊 Tóm tắt Thống kê

| Loại API | Số lượng |
|----------|---------|
| Authentication | 3 |
| User Profile | 5 |
| Friend Request | 6 |
| Organization | 4 |
| Member Management | 5 |
| Department | 6 |
| Event | 6 |
| Event Category | 3 |
| Task | 6 |
| Milestone | 4 |
| Notification (Real-time) | 1 |
| Admin | 1 |
| **TỔNG CỘNG** | **50 APIs** |

---

## 🔐 Authentication

Tất cả API (trừ `/api/auth/register` và `/api/auth/login`) yêu cầu JWT token:

```
Authorization: Bearer YOUR_JWT_TOKEN
```

---

## 🚀 Notification Integration

Các API tích hợp thông báo real-time:

1. **Friend Request:**
   - SendFriendRequest → Gửi thông báo cho receiver
   - AcceptFriendRequest → Gửi thông báo cho sender
   - RejectFriendRequest → Gửi thông báo cho sender

2. **Member Management:**
   - UpdateMemberRole → Gửi thông báo cho member
   - DeleteMember → Gửi thông báo cho member

3. **Event:**
   - UpdateEvent → Gửi thông báo cho tất cả attendees
   - CancelEvent → Gửi thông báo cho tất cả attendees

4. **Task:**
   - AssignTask → Gửi thông báo cho assignee
   - UpdateTaskStatus → Gửi thông báo cho assignee

---

## 📝 Ghi chú

- Tất cả API sử dụng FastEndpoints framework
- Database: PostgreSQL
- Authentication: JWT Bearer Token
- Real-time: SignalR WebSocket
- Soft delete: Tất cả entities hỗ trợ soft delete
- Notification: Fire-and-forget pattern (không block business logic)
