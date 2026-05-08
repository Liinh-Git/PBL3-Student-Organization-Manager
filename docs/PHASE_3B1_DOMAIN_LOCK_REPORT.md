# PHASE_3B1_DOMAIN_LOCK_REPORT

## Files Read

- `PBL3_SYSTEM_DESIGN_AND_PROTOTYPE_HANDOFF_FINAL_CLEAN.md`
- `PBL3-rescue/docs/PHASE_3A_REPO_FOUNDATION_REPORT.md`
- `PBL3-rescue/docs/PHASE_3_SCOPE_LOCK.md`
- `PBL3-rescue/docs/DO_NOT_IMPLEMENT_YET.md`
- `PBL3-rescue/docs/NEXT_PHASE_INPUT.md`
- `PBL3-rescue/docs/REPO_STRUCTURE_LOCK.md`
- `PBL3-refactor/Docs/00_AUDIT_INDEX.md`
- `PBL3-refactor/Docs/02_BACKEND_FACTS.md`
- `PBL3-refactor/Docs/05_BE_FE_MAPPING_FACTS.md`
- `PBL3-refactor/Docs/06_MODULE_FACTS.md`
- `PBL3-refactor/Docs/07_UNKNOWN_AND_UNVERIFIED.md`

## Main Corrections Compared to NEXT_PHASE_INPUT.md

`NEXT_PHASE_INPUT.md` chỉ là starter input nên đã bị đơn giản hóa quá mức. Tôi đã hiệu chỉnh lại theo FINAL CLEAN blueprint và audit facts như sau:

- thêm `BaseEntity` làm nền bắt buộc cho soft-delete và audit timestamp;
- thêm các entity bị bỏ sót khỏi input giản lược: `EventMember`, `Attendee`, `DigitalAsset`, `EventRating`, `EventReport`, `Resource`, `ActivityHistory`;
- tách rõ `EventMember` với `Attendee` để không lẫn staff nội bộ với người tham dự;
- khóa lại chain chuẩn `Organization → Member → Event → Milestone → EventCategory → OrgTask`;
- chốt rằng `OrgTask` nằm dưới `EventCategory`, không phải task board riêng theo organization;
- đưa `EventReport` và `Resource` vào nhóm should-have DB v1 nhưng không có working UI/API;
- giữ `OrganizationPost`, `PostComment`, Messages/Chat và Finance-specific tables ngoài scope v1;
- bổ sung delete behavior, index, unique constraint, enum values và DbContext plan đầy đủ;
- làm rõ chiến lược storage enum là string, thay vì để mơ hồ như input starter;
- làm rõ các field bị giản lược trước đó như `SocialLinks`, `ProfileVisibility`, `LastLoginAt`, `TargetParticipants`, `Tags`, `AverageRating`, `ActionUrl`, `RelatedEntityType`, `RelatedEntityId`.

## Entities Added

Các entity sau được đưa vào domain lock v1 vì audit facts và FINAL CLEAN đều xác nhận:

- `EventMember`
- `Attendee`
- `DigitalAsset`
- `EventRating`
- `EventReport`
- `Resource`
- `ActivityHistory`

## Entities Explicitly Excluded

Các nhóm sau không được đưa vào rescue v1:

- `OrganizationPost`
- `PostComment`
- `Message` / `ChatThread`
- finance-specific ledger/payment/budget entities

Lý do:

- Posts/Comments là hard-excluded.
- Messages/Finance vẫn chỉ là prototype-only, chưa có contract đủ chắc để làm DB module working.
- Report/Resource vẫn được giữ ở DB v1 nhưng không có working UI/API base prototype.

## Key Risks

- Unique constraint cho `Organization.OrgName` cần chốt implementation PostgreSQL trước migration để tránh xung đột case-insensitive/soft-delete.
- `Department.Code` cần partial unique index hoặc strategy chuẩn hóa `null/empty` rõ ràng.
- Các enum nên persist bằng string để an toàn; nếu đổi sang int sau này sẽ có rủi ro ordering.
- Một số entity should-have như `EventRating`, `EventReport`, `Resource`, `ActivityHistory` sẽ tạo áp lực để viết API/UI sớm; document này cố ý chặn việc đó.
- `RolePermission` phải giữ composite key và không inherit `BaseEntity`, nếu không sẽ làm sai mô hình join table thuần.

## Can Phase 3B.2 Start?

Phase 3B.2 có thể bắt đầu sau khi user xác nhận 3 quyết định EF/migration nhỏ: Organization.OrgName uniqueness strategy, Department.Code uniqueness strategy, và MemberRole persistence strategy.

## Requires User Approval Before Applying Entity Code

Phase 3B.2 có thể bắt đầu sau khi user xác nhận 3 quyết định EF/migration nhỏ: Organization.OrgName uniqueness strategy, Department.Code uniqueness strategy, và MemberRole persistence strategy.

Chỉ có 3 điểm nên xác nhận khi đến bước sinh migration SQL:

1. Organization.OrgName uniqueness: dùng normalized column/service validation trước, không dùng citext nếu chưa cấu hình extension.
2. Department.Code uniqueness: dùng service-level validation hoặc filtered unique index nếu EF/PostgreSQL config rõ.
3. MemberRole: không persist riêng trong Member v1; RoleId là canonical. MemberRole chỉ dùng cho enum/hierarchy mapping nếu cần.
