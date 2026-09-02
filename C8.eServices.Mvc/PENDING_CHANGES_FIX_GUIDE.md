# =====================================================================
# Fix "Pending Changes" Error - Step-by-Step Guide
# =====================================================================

## 🔍 The Problem

```
Unable to update database to match the current model because there are 
pending changes and automatic migration is disabled.
```

**Translation:** Your C# model classes have changes that aren't captured in any migration file.

---

## ✅ SOLUTION

### Step 1: Check What's in the Database ✅ DONE
You already ran the cleanup scripts. Good!

### Step 2: Create a Migration for Pending Changes

**Run this in Package Manager Console** (not PowerShell terminal):

```powershell
Add-Migration SyncDatabaseModel -Verbose
```

This will create a new migration file showing what EF thinks is missing.

### Step 3: Inspect the Generated Migration

Open the file that was just created (will be named something like `202603XXXXXX_SyncDatabaseModel.cs`)

**Look for:**
- Is it trying to ADD columns that already exist? (Common issue)
- Is it trying to ALTER columns? 
- Is it empty (just empty `Up()` and `Down()` methods)?

### Step 4A: If Migration is Empty (Best Case)
```csharp
public override void Up()
{
    // Empty - no changes
}
```

**This means EF is confused about state. Solution:**
1. Delete the empty migration file
2. Run: `Add-Migration SyncDatabaseModel -IgnoreChanges`
3. This creates a "snapshot" migration that just syncs EF's internal state

### Step 4B: If Migration Has Changes (Common Case)

**Example - it tries to ADD HasDSTV:**
```csharp
public override void Up()
{
    AddColumn("dbo.PropertyLeaseApplications", "HasDSTV", c => c.Boolean());
}
```

**But columns already exist!** ⚠️

**Solution:** Make the migration idempotent (safe to run multiple times):

```csharp
public override void Up()
{
    // Only add if doesn't exist
    Sql(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns 
                      WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplications') 
                      AND name = 'HasDSTV')
        BEGIN
            ALTER TABLE dbo.PropertyLeaseApplications ADD HasDSTV BIT NULL
        END
    ");
}
```

### Step 5: Apply the Migration
```powershell
Update-Database -Verbose
```

---

## 🎯 QUICK FIX (Most Likely Scenario)

Your issue is probably that:
1. ✅ Columns exist in database (from SQL scripts)
2. ✅ Migrations exist in code
3. ❌ Migrations NOT recorded in `__MigrationHistory` table

**Verify this by running:**

```sql
-- In SQL Server Management Studio
USE [CRMPLMDEV_2025]
GO

-- Check if these migrations are recorded
SELECT MigrationId FROM [dbo].[__MigrationHistory]
WHERE MigrationId IN (
    '202603260709130_AddBankingDetailsOnly',
    '202603270800000_AddLeaseAgreementEnhancements',
    '202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication',
    '202603280900000_AddBankingDetailsToLeaseAgreementMaster',
    '202603280915000_MakeHasDSTVNullable'
)
ORDER BY MigrationId
```

**If these are MISSING from __MigrationHistory:**

You need to "fake" them as applied since columns already exist:

```powershell
# In Package Manager Console
Update-Database -TargetMigration:202603260709130_AddBankingDetailsOnly -Force
Update-Database -TargetMigration:202603270800000_AddLeaseAgreementEnhancements -Force
Update-Database -TargetMigration:202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication -Force
Update-Database -TargetMigration:202603280900000_AddBankingDetailsToLeaseAgreementMaster -Force
Update-Database -TargetMigration:202603280915000_MakeHasDSTVNullable -Force
```

---

## 🔥 NUCLEAR OPTION (If Nothing Works)

This manually inserts migration records without running SQL:

```sql
-- Check what timestamp format is used
SELECT TOP 1 MigrationId FROM [dbo].[__MigrationHistory] ORDER BY MigrationId DESC

-- Manually mark migrations as applied (USE WITH CAUTION)
-- Only do this if columns DEFINITELY exist!

DECLARE @ContextKey NVARCHAR(300) = 'C8.eServices.Mvc.DataAccessLayer.eServicesDbContext'

-- Insert missing migration records
IF NOT EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603260709130_AddBankingDetailsOnly')
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES ('202603260709130_AddBankingDetailsOnly', @ContextKey, 0x1F8B08..., '6.5.1-61114')

-- Repeat for each missing migration...
-- (You'd need to get the Model binary from the migration .resx files)
```

⚠️ **This is risky!** Only use if:
- Columns definitely exist
- You've backed up the database
- Other options failed

---

## 📊 What to Do RIGHT NOW

**Step 1:** Run this SQL to check migration status:
```sql
C8.eServices.Mvc\Scripts\Check_MigrationHistory_Status.sql
```

**Step 2:** Based on results, run in **Package Manager Console**:
```powershell
# If migrations are missing from __MigrationHistory
Update-Database -TargetMigration:202603280915000_MakeHasDSTVNullable -Force

# OR if there are truly pending changes
Add-Migration SyncDatabaseModel -Verbose
```

**Step 3:** Report back with:
- Output from Check_MigrationHistory_Status.sql
- Any error messages

Then I can give you the exact fix! 🎯
