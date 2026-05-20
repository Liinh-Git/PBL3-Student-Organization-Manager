# DB_RESET_AND_SEED_NOTES

## Database Reset History

### User-Initiated Reset (Before Phase 4A-0)

The user manually reset the PostgreSQL schema before this task:

```sql
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
```

**Note**: The `CREATE SCHEMA public;` may not have completed successfully or permissions weren't set, which caused the initial migration failure. The user had to run:

```sql
CREATE SCHEMA IF NOT EXISTS public;
GRANT ALL ON SCHEMA public TO org_admin;
```

---

## Connection String Configuration

### Template Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=[PASSWORD]"
  }
}
```
This is a **template/sample** only and should NOT be committed with real passwords.

### Development Connection String (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=SecretPassword123@@"
  }
}
```
This is for **development only**.

⚠️ **WARNING**: Do NOT commit real production secrets. For production:
1. Use environment variables
2. Use user secrets (`dotnet user-secrets`)
3. Use Azure Key Vault or similar secret management

---

## How to Rerun Migration

### Option 1: Via EF CLI
```bash
cd PBL3-rescue
dotnet ef database update --project backend/Org.Backend/Org.Backend.csproj --startup-project backend/Org.Backend/Org.Backend.csproj
```

### Option 2: Via Application Startup
Simply run the application in Development environment:
```bash
dotnet run --project backend/Org.Backend/Org.Backend.csproj
```
The application will automatically apply pending migrations on startup.

### Option 3: Create New Migration (After Model Changes)
```bash
cd PBL3-rescue
dotnet ef migrations add YourMigrationName --project backend/Org.Backend/Org.Backend.csproj --startup-project backend/Org.Backend/Org.Backend.csproj --output-dir Infrastructure/Persistence/Migrations
dotnet ef database update --project backend/Org.Backend/Org.Backend.csproj --startup-project backend/Org.Backend/Org.Backend.csproj
```

---

## How to Rerun Seed

### Option 1: Via Application Startup
Simply run the application in Development environment. The seeder runs automatically and is **idempotent**:
```bash
dotnet run --project backend/Org.Backend/Org.Backend.csproj
```

### Option 2: Manual Seed Script (Future)
If a manual seed script is needed, create an endpoint or CLI command that:
```csharp
using var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
await seeder.SeedAsync();
```

---

## How to Reset Database Again

### ⚠️ WARNING: This Will Delete All Data

```sql
-- Connect to PostgreSQL
psql -U postgres

-- Drop and recreate schema
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO org_admin;
GRANT ALL ON SCHEMA public TO public;

-- Exit psql
\q
```

Then reapply migrations:
```bash
cd PBL3-rescue
dotnet ef database update --project backend/Org.Backend/Org.Backend.csproj --startup-project backend/Org.Backend/Org.Backend.csproj
```

Or run the application:
```bash
dotnet run --project backend/Org.Backend/Org.Backend.csproj
```

---

## Seeded Data Summary

### What IS Seeded
| Entity | Count | Notes |
|---|---|---|
| Permissions | 15 | Canonical permission keys |
| Users | 6 | Admin + 5 demo users |
| Organizations | 1 | Default "Student Organization" |
| Roles | 3 | President, Manager, Member |
| RolePermissions | 28 | Role-permission mappings |
| Members | 6 | User-organization memberships |
| Departments | 3 | Tech, Events, Marketing |
| Events | 1 | Annual Tech Summit 2026 |
| Milestones | 3 | Planning, Execution, Wrap-up |
| EventCategories | 3 | Venue, Speaker, Marketing |
| OrgTasks | 5 | Demo tasks in Venue category |
| Requests | 1 | Demo join request |
| Notifications | 3 | Demo notifications for admin |
| FriendRequests | 2 | Demo friend requests |

**Total Records**: ~75 seed records

### What is Intentionally NOT Seeded
| Entity | Reason |
|---|---|
| Posts | Hard-excluded from rescue v1 |
| Comments | Hard-excluded from rescue v1 |
| Messages/Chat | Placeholder only, no working module |
| Finance data | Finance module excluded |
| EventMembers | DB foundation only, no working UI/API |
| Attendees | DB foundation only, no working UI/API |
| DigitalAssets | DB foundation only, no working UI/API |
| EventRatings | DB foundation only, no working UI/API |
| EventReports | DB foundation only, no working UI/API |
| Resources | DB foundation only, no working UI/API |
| ActivityHistory | DB foundation only, no working UI/API |

---

## Seed Data Credentials

### Admin Account
- **Email**: `admin@example.com`
- **Password**: `Admin@123456`
- **Role**: President (all permissions)

### Demo Member Accounts
All use password `User@123456`:
- `member1@example.com` (John Doe)
- `member2@example.com` (Jane Smith)
- `member3@example.com` (Bob Johnson)
- `member4@example.com` (Alice Williams)
- `member5@example.com` (Charlie Brown)

---

## Troubleshooting

### "no schema has been selected to create in"
**Cause**: The `public` schema doesn't exist or user lacks permissions.

**Fix**:
```sql
CREATE SCHEMA IF NOT EXISTS public;
GRANT ALL ON SCHEMA public TO org_admin;
GRANT ALL ON SCHEMA public TO public;
```

### "relation already exists"
**Cause**: Migration was partially applied.

**Fix**: Reset the database (see above) and reapply migrations.

### "password authentication failed"
**Cause**: Database user credentials are incorrect.

**Fix**: Verify connection string in `appsettings.Development.json` matches your PostgreSQL configuration.

### Seeder Not Running
**Cause**: Application not running in Development environment.

**Fix**: Ensure `ASPNETCORE_ENVIRONMENT=Development` or run via `dotnet run` without specifying environment.

---

**End of DB_RESET_AND_SEED_NOTES.md**
