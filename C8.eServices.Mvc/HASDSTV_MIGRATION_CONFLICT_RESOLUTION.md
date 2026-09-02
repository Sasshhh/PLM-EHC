# 🔥 HasDSTV Migration Conflict - RESOLUTION GUIDE

## 🚨 The Problem
You hit the classic **"mixed SQL + EF migrations"** conflict:

```
ALTER TABLE ALTER COLUMN failed because column 'HasDSTV' does not exist 
in table 'PropertyLeaseAgreementMasterAudits'.
```

### Root Cause
1. ✅ **AutomaticMigrationsEnabled = true** (BAD!)
2. ❌ Some columns added via SQL scripts
3. ❌ Some columns added via EF migrations
4. 💥 EF tried to ALTER a column that doesn't exist yet

---

## ✅ SOLUTION (Step-by-Step)

### Step 1: Run the Fix Script (SQL Server Management Studio)
```sql
-- Execute this script on your database
C8.eServices.Mvc\Scripts\Fix_HasDSTV_Migration_Conflict.sql
```

This will:
- Check which `HasDSTV` columns are missing
- Add them if they don't exist (as `BIT NULL`)
- Show you a summary

### Step 2: Verify Automatic Migrations are Disabled
✅ **DONE** - I've already updated `Configuration.cs`:
```csharp
AutomaticMigrationsEnabled = false; // Line 15
AutomaticMigrationDataLossAllowed = false; // Line 16
```

### Step 3: Clean Up __MigrationHistory
```sql
-- Remove the automatic migration that failed
DELETE FROM [dbo].[__MigrationHistory]
WHERE MigrationId LIKE '202603260831073_AutomaticMigration%'
```

### Step 4: Verify Existing Columns
```powershell
# In Visual Studio SQL Server Object Explorer, run:
C8.eServices.Mvc\Scripts\Check_Existing_Columns.sql
```

### Step 5: Run Update-Database
```powershell
# In Package Manager Console
Update-Database -Verbose
```

Expected result:
- ✅ No automatic migrations
- ✅ Applies explicit migration: `202603280915000_MakeHasDSTVNullable`
- ✅ Columns already exist (from Step 1), so migration just validates

---

## 📊 Current Migration Status

### Explicit Migrations (Good ✅)
- `202603260709130_AddBankingDetailsOnly`
- `202603270800000_AddLeaseAgreementEnhancements`
- `202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication`
- `202603280900000_AddBankingDetailsToLeaseAgreementMaster`
- `202603280915000_MakeHasDSTVNullable` ⬅️ **This should apply**

### Duplicate/Bad Migrations (Deleted ❌)
- ~~`202603260751524_nullablefieldsfix`~~ (deleted - wrong timestamp)
- ~~`202603260823293_nullablefieldsfix2`~~ (deleted - duplicate)

### Automatic Migration (Conflict 💥)
- `202603260831073_AutomaticMigration` ⬅️ **Delete from __MigrationHistory**

---

## 🎯 Prevention: Best Practices

### ❌ DON'T:
1. Enable automatic migrations in team projects
2. Mix SQL scripts with EF migrations for schema changes
3. Manually add columns then expect migrations to work

### ✅ DO:
1. Always use explicit migrations: `Add-Migration MigrationName`
2. Review generated migration before running
3. Keep `AutomaticMigrationsEnabled = false`
4. Document SQL scripts vs migrations in README

---

## 🔍 Troubleshooting

### If Update-Database still fails:
```powershell
# 1. Check what's pending
Get-Migrations

# 2. Check what's applied in DB
SELECT MigrationId, ContextKey 
FROM [dbo].[__MigrationHistory]
ORDER BY MigrationId DESC

# 3. Force specific migration
Update-Database -TargetMigration:202603280915000_MakeHasDSTVNullable -Verbose

# 4. Nuclear option: Reset to last working migration
Update-Database -TargetMigration:202603280900000_AddBankingDetailsToLeaseAgreementMaster
```

### If columns already exist error:
That's OK! It means Step 1 worked. The migration will see columns exist and skip the ADD.

---

## 📝 Summary

| Action | Status | Notes |
|--------|--------|-------|
| Disable auto migrations | ✅ DONE | Configuration.cs updated |
| Create fix script | ✅ DONE | Fix_HasDSTV_Migration_Conflict.sql |
| Run fix script | ⏳ TODO | Execute in SSMS |
| Clean __MigrationHistory | ⏳ TODO | Delete automatic migration entry |
| Update-Database | ⏳ TODO | Should work after above steps |

---

## ⚡ Quick Fix (TL;DR)

```powershell
# 1. Run in SSMS
.\C8.eServices.Mvc\Scripts\Fix_HasDSTV_Migration_Conflict.sql

# 2. Delete bad migration record
DELETE FROM [dbo].[__MigrationHistory]
WHERE MigrationId LIKE '202603260831073_%'

# 3. Try again in PMC
Update-Database -Verbose
```

That's it! 🎉
