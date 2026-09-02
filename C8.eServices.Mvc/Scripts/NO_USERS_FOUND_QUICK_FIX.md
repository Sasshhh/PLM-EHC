# ✅ QUICK FIX - No Users Found Issue

## What Happened:
The master setup script couldn't find any users with these roles:
- `Property Manager`
- `Facilities Manager`
- `Housing Supervisor`

## ✅ QUICK FIX (Run This Now):

```sql
-- Option 1: Find existing users (RECOMMENDED FIRST)
C8.eServices.Mvc\Scripts\diagnose_users_and_roles.sql

-- Option 2: Manually set AppSettings with ANY user (QUICK FIX)
C8.eServices.Mvc\Scripts\manual_set_manager_appsettings.sql

-- Then verify everything is ready:
C8.eServices.Mvc\Scripts\verify_uc012_uc013_setup.sql
```

## Step-by-Step:

### 1. Run Diagnostic (See what users exist)
```sql
-- Open and run this in SSMS:
C8.eServices.Mvc\Scripts\diagnose_users_and_roles.sql
```

This will show you:
- ✅ All roles in your system
- ✅ All active users
- ✅ Which users could be used for managers
- ✅ Recommendations

### 2. Pick Your Solution:

#### Option A: You Have Suitable Users
If the diagnostic found users, note their IDs and manually insert:
```sql
-- Use the user IDs from the diagnostic
INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime)
VALUES ('PropertyFacilitiesManagerId', '<USER_ID>', 'Facilities Manager', 1, 0, GETDATE(), GETDATE())

INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime)
VALUES ('MaintenanceManagerId', '<USER_ID>', 'Maintenance Manager', 1, 0, GETDATE(), GETDATE())
```

#### Option B: No Suitable Users (QUICK FIX)
```sql
-- This will use ANY active user as a temporary fix:
C8.eServices.Mvc\Scripts\manual_set_manager_appsettings.sql
```

This is **OKAY FOR DEV/TESTING** but you'll want to:
- Create proper roles later
- Assign specific users
- Update AppSettings with correct IDs

### 3. Verify Setup
```sql
-- Check everything is ready:
C8.eServices.Mvc\Scripts\verify_uc012_uc013_setup.sql
```

Expected output:
```
✅ PropertyFacilitiesManagerReview ResponsibilityType exists
✅ PropertyFacilitiesManagerId AppSetting exists
✅ MaintenanceManagerId AppSetting exists
✅ MaintenanceJobSheet ResponsibilityType exists

🎉 ALL REQUIREMENTS MET!
```

### 4. Test Workflow
Once verification passes:
```
1. Navigate to: /PropertyLeaseApplication/MaintenanceJobSheet/2008
2. Sign job card
3. Verify routing works!
```

## Current Status:
- ✅ ResponsibilityType added: `PropertyFacilitiesManagerReview`
- ✅ MaintenanceJobSheet ResponsibilityType exists
- ❌ AppSettings missing (fixing now)

## After Quick Fix:
- ✅ All requirements met
- ✅ Ready to test workflow
- ⚠️  Same user assigned to both roles (temporary)

---

**TL;DR: Run `manual_set_manager_appsettings.sql` then `verify_uc012_uc013_setup.sql`** 🚀
