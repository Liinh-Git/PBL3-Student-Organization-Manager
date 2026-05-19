# UI Concept: Unified Personal Dashboard
> Route đề xuất: `/user/dashboard` — thay thế hoặc bổ sung cho `/user/events`

---

## Tổng quan layout (3-panel)

```
┌──────────────────────────────────────────────────────────────────────────┐
│  TOPBAR (sticky)  — "Tổng quan của tôi"  [Tháng 5 2026]  [< Hôm nay >]  │
├─────────────────┬───────────────────────────────────────┬────────────────┤
│                 │                                       │                │
│  LEFT PANEL     │         CENTER: CALENDAR GRID         │  RIGHT PANEL   │
│  (280px fixed)  │         (flex-1, chiếm phần lớn)      │  (300px fixed) │
│                 │                                       │                │
│  ┌───────────┐  │  T2   T3   T4   T5   T6   T7   CN    │  📋 TASKS SẮP  │
│  │ Mini month│  │ ─────────────────────────────────     │  HẾT HẠN       │
│  │ calendar  │  │       │    │    │    │    │    │      │  ─────────────  │
│  │ (dạng dot)│  │  1    │  2 │  3 │  4 │  5 │  6 │     │  ● Task A      │
│  └───────────┘  │       │    │████│    │    │    │      │    deadline!   │
│                 │  7    │  8 │  9 │ 10 │ 11 │ 12 │     │  ● Task B      │
│  🏷️ LAYERS      │       │    │    │    │████│    │      │    In Progress │
│  ☑ 📅 Sự kiện   │  13   │ 14 │ 15 │ 16 │ 17 │ 18 │     │               │
│  ☑ ✅ Tasks     │       │    │    │    │    │    │      │  📅 SẮP TỚI    │
│  ─────────────  │ [19]  │ 20 │ 21 │ 22 │ 23 │ 24 │     │  ─────────────  │
│  🏢 FILTER ORG  │  TODAY│    │    │    │    │    │      │  → Event 1     │
│  ☑ Org Alpha    │       │    │    │    │    │    │      │    Thứ 4 - 21/5│
│  ☑ Org Beta     │  25   │ 26 │ 27 │ 28 │ 29 │ 30 │     │  → Event 2     │
│                 │       │    │    │    │    │    │      │    Thứ 6 - 23/5│
└─────────────────┴───────────────────────────────────────┴────────────────┘
```

---

## Chi tiết từng panel

### LEFT PANEL — "Control Center"
**Nền**: `#1a2d3d` (tông tối nhẹ hơn sidebar, glassmorphism với backdrop-filter)

#### 1. Mini Calendar (Month thumbnail)
- Grid 7 cột × 5–6 hàng, mỗi ô ~28px
- **Dot indicators** dưới số ngày: màu cam `#ff9b51` cho event, màu xanh `#60a5fa` cho task deadline
- Ngày hôm nay: vòng tròn nền accent `#ff9b51`, chữ trắng
- Click vào ngày → scroll/highlight ô ngày đó trong calendar chính

#### 2. Layer Toggles (bộ lọc hiển thị)
```
☑ 📅 Sự kiện của tôi          (màu cam #ff9b51)
☑ ✅ Task deadline             (màu xanh #60a5fa)
```
Toggle checkbox kiểu chip hiện đại — khi uncheck, loại item đó ẩn trên calendar.

#### 3. Filter theo Tổ chức
Checklist các org user đang tham gia (lấy từ `getMyOrganizations()`). Uncheck org nào → ẩn event/task của org đó.

---

### CENTER PANEL — Calendar Grid (Month View mặc định)

#### Toolbar
```
[< Prev]  Tháng 5, 2026  [Next >]   |  [Tháng] [Tuần] [Ngày] [Agenda]
```
- Nút view mode: pill selector giống Google Calendar

#### Grid ngày
- **7 cột** (T2→CN), **5–6 hàng**, border mỏng `1px solid #e2e8f0`
- Ngày hôm nay: background `rgba(255,155,81,0.08)`, số ngày có badge accent tròn

#### Event Chips (trong ô ngày)
Mỗi item chiếm 1 dòng (~20px cao), tối đa 3 hiển thị → "+2 more" nếu quá:

```
┌─── Ô ngày 21 ─────────────────┐
│  21                           │
│  [🟠 Hackathon CLB Tech     ] │  ← Event chip (cam)
│  [🔵 Task: Thiết kế poster  ] │  ← Task chip (xanh)
│  [🟠 Hội thảo kỹ năng mềm  ] │
│  +1 more                      │
└───────────────────────────────┘
```

**Event chip** (`type: event`):
- Màu nền: `rgba(255,155,81,0.15)`, border-left `3px solid #ff9b51`
- Icon: lịch nhỏ
- Text: Tên event (truncate 1 dòng)
- Hover: tooltip mini-card (tên, giờ bắt đầu, địa điểm, org)

**Task chip** (`type: task`):
- Màu nền: `rgba(96,165,250,0.12)`, border-left `3px solid #60a5fa`
- Icon: checkbox nhỏ
- Text: Tên task (truncate)
- **Overdue**: nền `rgba(239,68,68,0.1)`, border-left `#ef4444`
- Hover: tooltip (tên task, priority badge, milestone → category → event)

#### Click vào chip → Popup Detail Card
**Event popup**:
```
┌──────────────────────────────────────┐
│  🟠 Hackathon CLB Tech               │
│  📅 Thứ 4, 21/05/2026  09:00–17:00   │
│  📍 Hội trường A, ĐN               │
│  🏢 CLB Công nghệ ĐHBK              │
│  🎫 Vai trò: Thành viên tổ chức      │
│                                      │
│  [Xem chi tiết]  [Vào workspace]     │
└──────────────────────────────────────┘
```

**Task popup**:
```
┌──────────────────────────────────────┐
│  ✅ Thiết kế poster sự kiện          │
│  ⚡ Độ ưu tiên: High                 │
│  📊 Trạng thái: InProgress           │
│  ⏰ Deadline: 21/05/2026             │
│  📂 Hackathon → Milestone 1 → Design│
│  🏢 CLB Công nghệ ĐHBK              │
│                                      │
│  [Xem trong workspace]               │
└──────────────────────────────────────┘
```

#### Week View
- 7 cột × timeline 24h (hoặc 7:00–23:00)
- Event hiển thị như Google Calendar: block màu theo thời gian thực
- Task deadline: icon flag ở đầu ngày, không block timeline

#### Agenda View (List)
- Danh sách theo ngày, group theo date header
- Mỗi item là 1 row (event/task), có icon phân biệt

---

### RIGHT PANEL — "Quick Overview"
**Nền**: card trắng với border nhẹ

#### Tasks sắp hết hạn
Top 5 tasks theo deadline gần nhất, sort ascending:
```
[!] Thiết kế poster    → deadline hôm nay   (badge đỏ)
[ ] Viết báo cáo       → còn 2 ngày         (badge cam)
[ ] Review code        → còn 5 ngày
```
- Click → navigate `/org/events/:eventId?orgId=...` (tab milestone/task)

#### Upcoming Events
Top 5 events sắp tới:
```
📅 Hackathon CLB Tech
   Thứ 4, 21/05 · 09:00 · Hội trường A
   
📅 Hội thảo kỹ năng mềm
   Thứ 6, 23/05 · 14:00
```

---

## Color System (map theo design tokens hiện có)

| Loại item | Chip color | Border | Badge |
|---|---|---|---|
| Event (member) | `rgba(255,155,81,0.15)` | `#ff9b51` (accent-500) | cam |
| Event (attendee) | `rgba(255,155,81,0.08)` | `#ff9b51` dashed | cam nhạt |
| Task (InProgress) | `rgba(96,165,250,0.12)` | `#60a5fa` | xanh |
| Task (overdue) | `rgba(239,68,68,0.10)` | `#ef4444` | đỏ |
| Task (Done) | `rgba(22,133,87,0.08)` | `#168557` (success-500) | xanh lá |
| Task (Blocked) | `rgba(180,35,24,0.10)` | `#b42318` (danger-500) | đỏ đậm |

---

## Phân tích API khả thi

### ✅ Events → HOÀN TOÀN SẴN SÀNG
- `getMyEvents()` → `GET /users/me/events`
- Trả về: `id`, `name`, `startDate`, `endDate`, `location`, `organizationName`, `organizationId`, `participationRole`, `status`
- **Đủ dữ liệu để render** event chips + popup

### ⚠️ Tasks → CẦN KIỂM TRA BACKEND
Backend có thể có `GET /api/users/me/tasks` → trả về tasks assigned to me.

**Nếu KHÔNG có endpoint này:**
- **Phương án thay thế khả thi**: Sau khi load events (member role), lazy-load milestones của từng event → categories → tasks, filter `assigneeId === me.id`
- **Nhược điểm**: N+1 requests, chậm với nhiều event
- **Giải pháp graceful**: Chỉ load task cho event đang Active/Upcoming (lọc status), giới hạn 3 event gần nhất → performance acceptable

**Strategy đề xuất**:
1. Gọi `getMyEvents()` → lấy events có role `OrganizationMember`
2. Với mỗi event active, gọi `getEventMilestones(eventId)` 
3. Với mỗi milestone, gọi `getCategories(milestoneId)` → categories đã có tasks[]
4. Filter task nào có `assigneeId === currentUser.id`
5. Map task → kèm context (eventName, milestoneName, categoryName, orgName)

Toàn bộ dữ liệu context đã đủ để render breadcrumb đẹp.

---

## Không dùng thư viện ngoài
Calendar tự xây bằng:
- `display: grid; grid-template-columns: repeat(7, 1fr)` cho month view
- `display: grid; grid-template-columns: repeat(8, 1fr)` (+ time gutter) cho week view
- CSS Variables từ design system hiện tại
- Tổng ~400 dòng CSS + ~600 dòng JSX → hoàn toàn kiểm soát được

---

## Tóm tắt khả thi

| Phần | Khả thi | Ghi chú |
|---|---|---|
| Calendar grid (month/week/agenda) | ✅ | Tự build bằng CSS Grid |
| Event chips trên calendar | ✅ | API sẵn có |
| Task chips trên calendar | ⚠️ | Cần confirm API hoặc dùng cascade load |
| Popup detail | ✅ | Dùng state + positioning |
| Mini calendar (left panel) | ✅ | Logic đơn giản |
| Layer toggles | ✅ | State management đơn giản |
| Org filter | ✅ | getMyOrganizations() đã có |
| Right panel tasks | ⚠️ | Phụ thuộc task API |
| Right panel events | ✅ | Sort từ getMyEvents() |
