-- Check final table counts after seeding
SELECT 'Users' as table_name, COUNT(*) as count FROM "Users" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Organizations', COUNT(*) FROM "Organizations" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Permissions', COUNT(*) FROM "Permissions" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Roles', COUNT(*) FROM "Roles" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'RolePermissions', COUNT(*) FROM "RolePermissions"
UNION ALL
SELECT 'Members', COUNT(*) FROM "Members" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Departments', COUNT(*) FROM "Departments" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Events', COUNT(*) FROM "Events" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Milestones', COUNT(*) FROM "Milestones" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'EventCategories', COUNT(*) FROM "EventCategories" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'OrgTasks', COUNT(*) FROM "OrgTasks" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Requests', COUNT(*) FROM "Requests" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'Notifications', COUNT(*) FROM "Notifications" WHERE NOT "IsDeleted"
UNION ALL
SELECT 'FriendRequests', COUNT(*) FROM "FriendRequests" WHERE NOT "IsDeleted"
ORDER BY table_name;

-- Check admin role
SELECT u."Email", r."RoleName" 
FROM "Users" u
JOIN "Members" m ON u."Id" = m."UserId" AND NOT m."IsDeleted"
JOIN "Roles" r ON m."RoleId" = r."Id" AND NOT r."IsDeleted"
WHERE u."Email" = 'admin@example.com' AND NOT u."IsDeleted";

-- Check role permission counts
SELECT r."RoleName", COUNT(rp."PermissionId") as permission_count
FROM "Roles" r
LEFT JOIN "RolePermissions" rp ON r."Id" = rp."RoleId"
WHERE NOT r."IsDeleted"
GROUP BY r."Id", r."RoleName"
ORDER BY r."RoleName";

-- Check for members with null RoleId
SELECT COUNT(*) as members_with_null_role
FROM "Members" 
WHERE NOT "IsDeleted" AND "RoleId" IS NULL;