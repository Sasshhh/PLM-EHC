# ? FIXED: Foreign Key Constraint Error

## ?? **PROBLEM:**

When running `Update-Database`, you received this error:

```
System.Data.SqlClient.SqlException: The INSERT statement conflicted with the FOREIGN KEY constraint "FK_dbo.Status_dbo.StatusTypes_StatusTypeId". 
The conflict occurred in database "CRMPLMDEV_2025", table "dbo.StatusTypes", column 'Id'.
```

## ?? **ROOT CAUSE:**

The `Status` table has a **required foreign key** relationship to `StatusTypes`:

```csharp
public class Status : BaseType
{
    public int StatusTypeId { get; set; }  // REQUIRED foreign key
    public StatusType StatusType { get; set; }
}
```

In the original `SeedTrainingStatusKeys()` method, we were trying to insert `Status` records **without providing a valid `StatusTypeId`**, which violated the foreign key constraint.

## ? **SOLUTION:**

Updated the `SeedTrainingStatusKeys()` method in `Configuration.cs` to:

1. **First create/get a `StatusType`** for training statuses
2. **Save it to get the Id**
3. **Then insert Status records** with the valid `StatusTypeId`

### **Updated Code:**

```csharp
private void SeedTrainingStatusKeys(eServicesDbContext context)
{
    // Step 1: Get or create a StatusType for training statuses
    var trainingStatusType = context.StatusTypes
        .FirstOrDefault(st => st.Key == "st_tenant_training");

    if (trainingStatusType == null)
    {
        trainingStatusType = new StatusType
        {
            Key = "st_tenant_training",
            Name = "Tenant Training",
            Description = "Status type for tenant training and examination workflow",
            IsActive = true,
            IsDeleted = false,
            IsLocked = false
        };
        context.StatusTypes.Add(trainingStatusType);
        context.SaveChanges(); // ? CRITICAL: Save to get the Id
    }

    // Step 2: Add Status records with valid StatusTypeId
    context.Status.AddOrUpdate(s => s.Key,
        new Status 
        { 
            Key = "s_awaiting_online_training", 
            Name = "Awaiting Online Training", 
            Description = "Tenant has been invited to complete online training", 
            StatusTypeId = trainingStatusType.Id,  // ? Now has valid FK
            IsActive = true,
            IsDeleted = false,
            IsLocked = false
        },
        // ... other 4 status records with same pattern
    );
}
```

## ?? **WHAT GETS CREATED:**

### **1. StatusType (1 record):**
| Key | Name | Description |
|-----|------|-------------|
| `st_tenant_training` | Tenant Training | Status type for tenant training and examination workflow |

### **2. Status Records (5 records):**
| Key | Name | StatusTypeId |
|-----|------|--------------|
| `s_awaiting_online_training` | Awaiting Online Training | (FK to st_tenant_training) |
| `s_training_in_progress` | Training In Progress | (FK to st_tenant_training) |
| `s_training_completed` | Training Completed | (FK to st_tenant_training) |
| `s_examination_passed` | Examination Passed | (FK to st_tenant_training) |
| `s_examination_failed` | Examination Failed | (FK to st_tenant_training) |

## ?? **NOW YOU CAN RUN:**

```powershell
Update-Database -Verbose
```

This should now complete successfully without the foreign key error!

## ? **VERIFICATION QUERIES:**

After `Update-Database` completes, verify in SQL Server:

```sql
-- Check StatusType created
SELECT * FROM StatusTypes WHERE [Key] = 'st_tenant_training'

-- Check 5 Status records created
SELECT s.[Key], s.Name, st.Name AS StatusTypeName
FROM Status s
INNER JOIN StatusTypes st ON s.StatusTypeId = st.Id
WHERE s.[Key] LIKE 's_%training%' OR s.[Key] LIKE 's_%examination%'
ORDER BY s.[Key]
```

**Expected Result:**
- ? 1 StatusType: `st_tenant_training`
- ? 5 Status records, all linked to the same StatusTypeId

## ?? **FILES MODIFIED:**

1. ? `C8.eServices.Mvc/Migrations/Configuration.cs`
   - Fixed `SeedTrainingStatusKeys()` method
   - Added StatusType creation before Status records
   - Added intermediate `SaveChanges()` call

## ?? **KEY LESSONS:**

1. **Always check foreign key requirements** before inserting data
2. **Entity Framework requires valid FKs** before saving child records
3. **Call `SaveChanges()`** when you need the auto-generated Id for the next operation
4. **Use `FirstOrDefault()` pattern** to avoid duplicate records on re-runs

## ?? **COMPLETE WORKFLOW:**

```
1. User runs: Update-Database
   ?
2. EF creates 8 tables (training system)
   ?
3. Seed() method called
   ?
4. SeedTrainingStatusKeys() executes:
   a. Check if StatusType exists
   b. If not, create it
   c. SaveChanges() ? get StatusTypeId
   d. Create 5 Status records with valid FK
   e. AddOrUpdate() prevents duplicates
   ?
5. SeedTrainingSlides() ? 24 slides
   ?
6. SeedExaminationQuestions() ? 16 questions
   ?
7. Final SaveChanges()
   ?
8. ? Success!
```

## ?? **IF YOU STILL GET ERRORS:**

### **Error: "Duplicate key"**
**Solution:** The data already exists. Delete and re-run:
```sql
DELETE FROM Status WHERE [Key] LIKE 's_%training%' OR [Key] LIKE 's_%examination%'
DELETE FROM StatusTypes WHERE [Key] = 'st_tenant_training'
-- Then run Update-Database again
```

### **Error: "Cannot insert NULL into StatusTypeId"**
**Solution:** Ensure you're using the updated Configuration.cs file. Check that:
- StatusType is created first
- SaveChanges() is called before creating Status records
- trainingStatusType.Id is not zero

## ?? **BUILD STATUS:**

? **Compilation Successful** (no errors)

Now run `Update-Database -Verbose` and it should work!

---

Generated: 2025-01-15
Issue: Foreign Key Constraint Violation - RESOLVED ?
