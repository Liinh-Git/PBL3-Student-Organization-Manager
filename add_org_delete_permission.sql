-- Add org.delete permission to the database
-- This script adds the missing org.delete permission

-- First, check if the permission already exists
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "Permissions" WHERE "PermissionKey" = 'org.delete'
    ) THEN
        INSERT INTO "Permissions" ("Id", "PermissionKey", "DisplayName", "ModuleGroup", "Description", "CreatedAt", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            'org.delete',
            'Delete Organization',
            'Overview',
            'Permission: org.delete',
            NOW(),
            NOW()
        );
        
        RAISE NOTICE 'Added org.delete permission successfully';
    ELSE
        RAISE NOTICE 'org.delete permission already exists';
    END IF;
END $$;

-- Get the permission ID
DO $$
DECLARE
    v_permission_id UUID;
    v_president_role_id UUID;
BEGIN
    -- Get the org.delete permission ID
    SELECT "Id" INTO v_permission_id FROM "Permissions" WHERE "PermissionKey" = 'org.delete';
    
    IF v_permission_id IS NOT NULL THEN
        -- Find the President role for each organization and assign the permission
        FOR v_president_role_id IN 
            SELECT r."Id" FROM "Roles" r 
            WHERE r."RoleName" = 'President'
        LOOP
            -- Check if the role already has this permission
            IF NOT EXISTS (
                SELECT 1 FROM "RolePermissions" 
                WHERE "RoleId" = v_president_role_id AND "PermissionId" = v_permission_id
            ) THEN
                INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "CreatedAt", "UpdatedAt")
                VALUES (gen_random_uuid(), v_president_role_id, v_permission_id, NOW(), NOW());
                
                RAISE NOTICE 'Assigned org.delete permission to President role %', v_president_role_id;
            END IF;
        END LOOP;
    END IF;
END $$;
