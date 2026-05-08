# PROTOTYPE_ONLY_BOUNDARY

## Purpose
Clear boundary between working modules and placeholder modules. This document defines which features are PROTOTYPE_ONLY (placeholder pages with no working API/UI), DB_FOUNDATION_ONLY (domain exists but no working UI/API), and EXCLUDED (not in scope for rescue v1).

---

## PROTOTYPE_ONLY Pages

These pages exist as placeholders using the `<PrototypePlaceholder />` component. They have no working service/adapter/API.

### 1. /org/tasks Aggregate Board

**Current File**: `frontend/src/pages/org/OrgTasksPlaceholderPage.jsx`

**Why Placeholder Only**:
- Task CRUD is **CORE** inside the EventDetail tree (Event → Milestone → EventCategory → OrgTask)
- The `/org/tasks` aggregate board would require a complex cross-event task aggregation endpoint
- This endpoint is not in the approved route list for base prototype
- Task management is fully functional inside EventDetail; aggregate board is a future enhancement

**No Service/Adapter/API**:
- ❌ No `getOrgTasks()` service function
- ❌ No aggregate task board service
- ❌ No `GET /api/organizations/{orgId}/tasks` endpoint
- ❌ No `GET /api/tasks` aggregate endpoint
- ❌ No task board adapter

**Future Condition to Implement**:
- User explicitly requests aggregate task board feature
- Design cross-event task aggregation logic
- Create backend endpoint for aggregate task list
- Create frontend service/adapter for aggregate board
- Implement board UI with filters/grouping/sorting

**Current Status**: ⚠️ Placeholder only

---

### 2. /org/resources

**Current File**: `frontend/src/pages/org/OrgResourcesPlaceholderPage.jsx`

**Why Placeholder Only**:
- `Resource` entity exists in DB foundation (Phase 3B.2 completed)
- Resource management requires complex inventory/booking/allocation logic
- No working Resource UI/API in base prototype
- Resource module is DB_FOUNDATION_ONLY

**No Service/Adapter/API**:
- ❌ No `resourceService.js`
- ❌ No `resourceAdapter.js`
- ❌ No Resource CRUD endpoints
- ❌ No Resource booking/allocation endpoints

**Future Condition to Implement**:
- User explicitly requests Resource management feature
- Design resource booking/allocation workflow
- Create backend Resource CRUD endpoints
- Create frontend service/adapter for resources
- Implement resource management UI

**Current Status**: ⚠️ Placeholder only

---

### 3. /org/reports

**Current File**: `frontend/src/pages/org/OrgReportsPlaceholderPage.jsx`

**Why Placeholder Only**:
- `EventReport` entity exists in DB foundation (Phase 3B.2 completed)
- Report generation requires complex aggregation/analytics logic
- No working EventReport UI/API in base prototype
- EventReport module is DB_FOUNDATION_ONLY

**No Service/Adapter/API**:
- ❌ No `reportService.js`
- ❌ No `reportAdapter.js`
- ❌ No EventReport CRUD endpoints
- ❌ No report generation/export endpoints

**Future Condition to Implement**:
- User explicitly requests Report generation feature
- Design report templates and data aggregation logic
- Create backend EventReport CRUD endpoints
- Create frontend service/adapter for reports
- Implement report generation/export UI

**Current Status**: ⚠️ Placeholder only

---

### 4. /org/finance

**Current File**: `frontend/src/pages/org/OrgFinancePlaceholderPage.jsx`

**Why Placeholder Only**:
- Finance-specific module is **EXCLUDED** from rescue v1
- No finance-specific domain entities (FinanceTransaction, FinanceBudget, etc.)
- Finance logic requires complex accounting/ledger/payment integration
- Not in scope for base prototype

**No Service/Adapter/API**:
- ❌ No `financeService.js`
- ❌ No `financeAdapter.js`
- ❌ No finance domain entities
- ❌ No finance CRUD endpoints
- ❌ No payment/ledger/budget endpoints

**Future Condition to Implement**:
- User explicitly requests Finance module
- Design finance domain model (FinanceTransaction, FinanceBudget, etc.)
- Create backend finance endpoints
- Create frontend service/adapter for finance
- Implement finance management UI

**Current Status**: ⚠️ Placeholder only

---

## DB_FOUNDATION_ONLY Modules

These modules have domain entities in the database (Phase 3B.2 completed) but no working UI/API in base prototype.

### 5. EventMembers

**Domain**: `EventMember.cs`, `EventRole.cs`

**Why DB Foundation Only**:
- EventMember represents event staff/organizer (internal event team)
- Different from Attendee (event participant/registration)
- Domain exists to preserve event staff/organizer concept
- No working EventMember UI/API in base prototype

**No Working UI/API**:
- ❌ No EventMember CRUD endpoints
- ❌ No EventMember service/adapter
- ❌ No EventMember management UI

**Possible Future Endpoints**:
- `GET /api/events/{eventId}/members` - List event staff
- `POST /api/events/{eventId}/members` - Add event staff
- `PUT /api/event-members/{id}/role` - Update event role
- `DELETE /api/event-members/{id}` - Remove event staff

**Explicit Warning**: Do not implement EventMember UI/API now. Wait for explicit user request.

**Current Status**: 📝 DB foundation only

---

### 6. Attendees

**Domain**: `Attendee.cs`, `AttendeeStatus.cs`

**Why DB Foundation Only**:
- Attendee represents event participant/registration/check-in
- Different from EventMember (event staff/organizer)
- Domain exists to preserve participant/registration concept
- No working Attendee UI/API in base prototype

**No Working UI/API**:
- ❌ No Attendee CRUD endpoints
- ❌ No Attendee service/adapter
- ❌ No Attendee registration/check-in UI

**Possible Future Endpoints**:
- `GET /api/events/{eventId}/attendees` - List attendees
- `POST /api/events/{eventId}/attendees` - Register attendee
- `PUT /api/attendees/{id}/check-in` - Check-in attendee
- `DELETE /api/attendees/{id}` - Cancel registration

**Explicit Warning**: Do not implement Attendee UI/API now. Wait for explicit user request.

**Current Status**: 📝 DB foundation only

---

### 7. DigitalAssets

**Domain**: `DigitalAsset.cs`, `FileType.cs`

**Why DB Foundation Only**:
- DigitalAsset represents event file/asset uploads
- Domain exists to preserve file/asset concept
- No working DigitalAsset UI/API in base prototype
- File upload requires storage integration (S3, Azure Blob, etc.)

**No Working UI/API**:
- ❌ No DigitalAsset CRUD endpoints
- ❌ No file upload/download endpoints
- ❌ No DigitalAsset service/adapter
- ❌ No file upload UI

**Possible Future Endpoints**:
- `GET /api/events/{eventId}/assets` - List event assets
- `POST /api/events/{eventId}/assets` - Upload asset
- `GET /api/assets/{id}/download` - Download asset
- `DELETE /api/assets/{id}` - Delete asset

**Explicit Warning**: Do not implement DigitalAsset UI/API now. Wait for explicit user request and storage integration decision.

**Current Status**: 📝 DB foundation only

---

### 8. EventRatings

**Domain**: `EventRating.cs`, `RatingAspect.cs`

**Why DB Foundation Only**:
- EventRating represents user rating for event
- Domain exists to support `Event.AverageRating` cached field
- No working EventRating UI/API in base prototype

**No Working UI/API**:
- ❌ No EventRating CRUD endpoints
- ❌ No EventRating service/adapter
- ❌ No event rating UI

**Possible Future Endpoints**:
- `GET /api/events/{eventId}/ratings` - List event ratings
- `POST /api/events/{eventId}/ratings` - Submit rating
- `PUT /api/ratings/{id}` - Update rating
- `DELETE /api/ratings/{id}` - Delete rating

**Explicit Warning**: Do not implement EventRating UI/API now. Wait for explicit user request.

**Current Status**: 📝 DB foundation only

---

### 9. EventReports

**Domain**: `EventReport.cs`

**Why DB Foundation Only**:
- EventReport represents event summary report (one-to-one with Event)
- Domain exists to preserve report concept
- No working EventReport UI/API in base prototype
- Reports page is PROTOTYPE_ONLY placeholder

**No Working UI/API**:
- ❌ No EventReport CRUD endpoints
- ❌ No report generation endpoints
- ❌ No EventReport service/adapter
- ❌ No report generation UI

**Possible Future Endpoints**:
- `GET /api/events/{eventId}/report` - Get event report
- `POST /api/events/{eventId}/report` - Generate report
- `PUT /api/reports/{id}` - Update report
- `GET /api/reports/{id}/export` - Export report

**Explicit Warning**: Do not implement EventReport UI/API now. Wait for explicit user request.

**Current Status**: 📝 DB foundation only

---

### 10. Resources

**Domain**: `Resource.cs`, `ResourceStatus.cs`

**Why DB Foundation Only**:
- Resource represents organization resource (equipment, venue, etc.)
- Domain exists to preserve resource concept
- No working Resource UI/API in base prototype
- Resources page is PROTOTYPE_ONLY placeholder

**No Working UI/API**:
- ❌ No Resource CRUD endpoints
- ❌ No resource booking/allocation endpoints
- ❌ No Resource service/adapter
- ❌ No resource management UI

**Possible Future Endpoints**:
- `GET /api/organizations/{orgId}/resources` - List resources
- `POST /api/organizations/{orgId}/resources` - Create resource
- `PUT /api/resources/{id}` - Update resource
- `DELETE /api/resources/{id}` - Delete resource
- `POST /api/resources/{id}/book` - Book resource

**Explicit Warning**: Do not implement Resource UI/API now. Wait for explicit user request.

**Current Status**: 📝 DB foundation only

---

### 11. ActivityHistory

**Domain**: `ActivityHistory.cs`, `ActivityType.cs`

**Why DB Foundation Only**:
- ActivityHistory represents organization activity feed/log
- Domain exists to preserve activity tracking concept
- No working ActivityHistory UI/API in base prototype

**No Working UI/API**:
- ❌ No ActivityHistory CRUD endpoints
- ❌ No activity feed endpoints
- ❌ No ActivityHistory service/adapter
- ❌ No activity feed UI

**Possible Future Endpoints**:
- `GET /api/organizations/{orgId}/activities` - List activities
- `POST /api/organizations/{orgId}/activities` - Log activity
- `GET /api/activities/public` - Public activity feed

**Explicit Warning**: Do not implement ActivityHistory UI/API now. Wait for explicit user request.

**Current Status**: 📝 DB foundation only

---

## EXCLUDED Modules

These modules are **hard-excluded** from rescue v1. No domain entities, no backend, no frontend.

### 12. Posts

**Why Excluded**:
- Posts module is hard-excluded from rescue v1
- Original PBL3 had Posts/Comments, but rescue v1 focuses on event management
- Posts/Comments add complexity without core value for event management

**No Domain/Backend/Frontend**:
- ❌ No `OrganizationPost` entity
- ❌ No Posts CRUD endpoints
- ❌ No Posts service/adapter
- ❌ No Posts pages/components

**Explicit Warning**: Do not create Posts module. If user requests Posts, confirm scope change first.

**Current Status**: ❌ Excluded

---

### 13. Comments

**Why Excluded**:
- Comments module is hard-excluded from rescue v1
- Comments depend on Posts module
- Not in scope for event management focus

**No Domain/Backend/Frontend**:
- ❌ No `PostComment` entity
- ❌ No Comments CRUD endpoints
- ❌ No Comments service/adapter
- ❌ No Comments components

**Explicit Warning**: Do not create Comments module. If user requests Comments, confirm scope change first.

**Current Status**: ❌ Excluded

---

### 14. Messages/Chat Working Module

**Why Excluded**:
- Messages/Chat working module is excluded from base prototype
- Real-time chat requires SignalR/WebSocket integration
- Complex message threading/notification logic
- Not in scope for base prototype

**No Domain/Backend/Frontend**:
- ❌ No `Message`, `ChatThread` entities
- ❌ No Messages/Chat CRUD endpoints
- ❌ No SignalR hub for real-time chat
- ❌ No Messages/Chat service/adapter
- ❌ No Messages/Chat working UI

**Possible Placeholder**:
- Could add placeholder page if visible in nav
- Use `<PrototypePlaceholder />` component

**Explicit Warning**: Do not create Messages/Chat working module. If user requests Messages/Chat, confirm scope change and SignalR integration first.

**Current Status**: ❌ Excluded (placeholder optional)

---

### 15. Finance-Specific Ledger/Payment/Budget Logic

**Why Excluded**:
- Finance-specific module is excluded from rescue v1
- Complex accounting/ledger/payment integration required
- Not in scope for event management focus
- Finance page is PROTOTYPE_ONLY placeholder

**No Domain/Backend/Frontend**:
- ❌ No `FinanceTransaction`, `FinanceBudget`, `FinancePayment` entities
- ❌ No finance CRUD endpoints
- ❌ No payment gateway integration
- ❌ No finance service/adapter
- ❌ No finance working UI

**Explicit Warning**: Do not create Finance working module. If user requests Finance, confirm scope change and payment integration first.

**Current Status**: ❌ Excluded (placeholder only)

---

## Summary Table

| Module | Status | Domain Exists | Backend Exists | Frontend Exists | Notes |
|---|---|---|---|---|---|
| /org/tasks aggregate board | ⚠️ PROTOTYPE_ONLY | ✅ (OrgTask) | ❌ | ⚠️ Placeholder | Task CRUD is CORE inside EventDetail |
| /org/resources | ⚠️ PROTOTYPE_ONLY | ✅ (Resource) | ❌ | ⚠️ Placeholder | DB_FOUNDATION_ONLY |
| /org/reports | ⚠️ PROTOTYPE_ONLY | ✅ (EventReport) | ❌ | ⚠️ Placeholder | DB_FOUNDATION_ONLY |
| /org/finance | ⚠️ PROTOTYPE_ONLY | ❌ | ❌ | ⚠️ Placeholder | EXCLUDED |
| EventMembers | 📝 DB_FOUNDATION_ONLY | ✅ | 📝 Notes only | ❌ | Event staff/organizer |
| Attendees | 📝 DB_FOUNDATION_ONLY | ✅ | 📝 Notes only | ❌ | Event participant/registration |
| DigitalAssets | 📝 DB_FOUNDATION_ONLY | ✅ | 📝 Notes only | ❌ | Event file/asset |
| EventRatings | 📝 DB_FOUNDATION_ONLY | ✅ | 📝 Notes only | ❌ | Event rating |
| EventReports | 📝 DB_FOUNDATION_ONLY | ✅ | 📝 Notes only | ❌ | Event summary report |
| Resources | 📝 DB_FOUNDATION_ONLY | ✅ | 📝 Notes only | ❌ | Organization resource |
| ActivityHistory | 📝 DB_FOUNDATION_ONLY | ✅ | 📝 Notes only | ❌ | Activity feed/log |
| Posts | ❌ EXCLUDED | ❌ | ❌ | ❌ | Hard-excluded from rescue v1 |
| Comments | ❌ EXCLUDED | ❌ | ❌ | ❌ | Hard-excluded from rescue v1 |
| Messages/Chat | ❌ EXCLUDED | ❌ | ❌ | ❌ | Placeholder optional |
| Finance working | ❌ EXCLUDED | ❌ | ❌ | ❌ | Placeholder only |

---

## Implementation Decision Tree

```
User requests feature
    ↓
Is it PROTOTYPE_ONLY?
    ├─ Yes → Confirm scope change → Design → Implement
    └─ No
        ↓
    Is it DB_FOUNDATION_ONLY?
        ├─ Yes → Confirm scope change → Design → Implement
        └─ No
            ↓
        Is it EXCLUDED?
            ├─ Yes → Confirm scope change → Design domain → Implement
            └─ No → Feature is CORE/SUPPORTING → Implement normally
```

---

**End of PROTOTYPE_ONLY_BOUNDARY.md**
