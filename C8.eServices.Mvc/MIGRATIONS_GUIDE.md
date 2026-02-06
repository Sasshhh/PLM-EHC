# ?? ENTITY FRAMEWORK MIGRATIONS - QUICK REFERENCE

## ? **RECOMMENDED APPROACH: Use Package Manager Console**

Instead of manually running SQL scripts, use Entity Framework Migrations for a cleaner, version-controlled database schema.

---

## **?? STEP-BY-STEP INSTRUCTIONS**

### **Step 1: Open Package Manager Console**
1. In Visual Studio, go to: **Tools ? NuGet Package Manager ? Package Manager Console**
2. Ensure **Default project** is set to: `C8.eServices.Mvc`

### **Step 2: Add Migration**

Run this command in Package Manager Console:

```powershell
Add-Migration AddTenantTrainingTables -Verbose
```

**What this does:**
- ? Scans your `eServicesDbContext` for new `DbSet` properties
- ? Detects 8 new tables (4 main + 4 audit)
- ? Generates a migration file in `Migrations\` folder with timestamp
- ? Creates `Up()` method (to apply changes) and `Down()` method (to rollback)

**Expected Output:**
```
Scaffolding migration 'AddTenantTrainingTables'.
The Designer Code for this migration file includes a snapshot of your current Code First model.
This snapshot is used to calculate the changes to your model when you scaffold the next migration.
If you make additional changes to your model that you want to include in this migration, then you can re-scaffold it by running 'Add-Migration AddTenantTrainingTables' again.
```

### **Step 3: Review Generated Migration (Optional)**

Check the generated file: `Migrations\[timestamp]_AddTenantTrainingTables.cs`

You should see tables being created:
```csharp
public partial class AddTenantTrainingTables : DbMigration
{
    public override void Up()
    {
        CreateTable("dbo.TenantTrainings", c => new { ... });
        CreateTable("dbo.TrainingSlides", c => new { ... });
        CreateTable("dbo.ExaminationQuestions", c => new { ... });
        CreateTable("dbo.TenantExamAnswers", c => new { ... });
        // + 4 audit tables
    }
    
    public override void Down()
    {
        DropTable("dbo.TenantExamAnswerAudits");
        DropTable("dbo.ExaminationQuestionAudits");
        DropTable("dbo.TrainingSlideAudits");
        DropTable("dbo.TenantTrainingAudits");
        DropTable("dbo.TenantExamAnswers");
        DropTable("dbo.ExaminationQuestions");
        DropTable("dbo.TrainingSlides");
        DropTable("dbo.TenantTrainings");
    }
}
```

### **Step 4: Update Database**

Run this command to apply the migration:

```powershell
Update-Database -Verbose
```

**What this does:**
- ? Connects to your database (connection string from `Web.config`)
- ? Creates all 8 tables with proper columns, indexes, and foreign keys
- ? Seeds all data (24 slides + 16 questions + 5 status keys) via `Configuration.Seed()` method
- ? Updates `__MigrationHistory` table to track applied migrations

**Expected Output:**
```
Target database is: 'CRMPLMDEV_2025' (DataSource: localhost, Provider: System.Data.SqlClient).
Applying explicit migrations: [202501150123456_AddTenantTrainingTables].
Applying explicit migration: 202501150123456_AddTenantTrainingTables.
Running Seed method.
```

---

## **? VERIFICATION STEPS**

After `Update-Database` completes successfully, verify in SQL Server Management Studio:

### **1. Check Tables Created:**
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Tenant%' OR TABLE_NAME LIKE '%Training%' OR TABLE_NAME LIKE '%Examination%'
ORDER BY TABLE_NAME
```

**Expected Result (8 tables):**
- ? TenantTrainings
- ? TrainingSlides
- ? ExaminationQuestions
- ? TenantExamAnswers
- ? TenantTrainingAudits
- ? TrainingSlideAudits
- ? ExaminationQuestionAudits
- ? TenantExamAnswerAudits

### **2. Check Data Seeded:**
```sql
-- Should return 24 slides
SELECT COUNT(*) AS SlideCount FROM TrainingSlides

-- Should return 16 questions (1 example + 15 actual)
SELECT COUNT(*) AS QuestionCount FROM ExaminationQuestions

-- Should return 5 new status keys
SELECT * FROM Status WHERE [Key] LIKE 's_%training%' OR [Key] LIKE 's_%examination%'
```

**Expected Results:**
- ? SlideCount: 24
- ? QuestionCount: 16
- ? Status Keys: 5

### **3. Verify Correct Answers:**
```sql
-- View all exam questions with correct answers
SELECT QuestionOrder, 
       LEFT(QuestionText, 50) + '...' AS Question, 
       CorrectAnswer 
FROM ExaminationQuestions 
WHERE IsExampleQuestion = 0
ORDER BY QuestionOrder
```

**Expected Answers:**
| Q# | Answer |
|----|--------|
| 1  | B      |
| 2  | A      |
| 3  | B      |
| 4  | C      |
| 5  | A      |
| 6  | D      |
| 7  | B      |
| 8  | A      |
| 9  | B      |
| 10 | B      |
| 11 | C      |
| 12 | B      |
| 13 | B      |
| 14 | C      |
| 15 | B      |

---

## **?? TROUBLESHOOTING**

### **Issue: "No migrations configuration type was found"**

**Solution:**
```powershell
Enable-Migrations -ContextTypeName C8.eServices.Mvc.DataAccessLayer.eServicesDbContext -Force
```

### **Issue: "Unable to generate an explicit migration"**

**Solution:** You have `AutomaticMigrationsEnabled = true` in Configuration.cs, so just run:
```powershell
Update-Database -Verbose
```
This will automatically create and apply the migration.

### **Issue: Database connection error**

**Solution:** Check `Web.config` connection string:
```xml
<add name="eServicesDbContext" 
     connectionString="Server=localhost;Database=CRMPLMDEV_2025;Integrated Security=True;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

### **Issue: "Cannot insert duplicate key"**

**Solution:** Data already exists. You can either:
1. **Option A:** Drop and recreate the tables:
   ```sql
   DROP TABLE IF EXISTS TenantExamAnswers
   DROP TABLE IF EXISTS ExaminationQuestions
   DROP TABLE IF EXISTS TrainingSlides
   DROP TABLE IF EXISTS TenantTrainings
   -- Then run Update-Database again
   ```

2. **Option B:** Use `Update-Database -Force` to re-run the migration

---

## **?? COMPARISON: Migrations vs SQL Scripts**

| Feature | Entity Framework Migrations | Manual SQL Scripts |
|---------|------------------------------|-------------------|
| **Version Control** | ? Built-in via `__MigrationHistory` | ? Manual tracking |
| **Rollback Support** | ? `Update-Database -TargetMigration:Previous` | ? Must write own rollback scripts |
| **Team Collaboration** | ? Migrations committed to Git | ?? SQL scripts may conflict |
| **Auto Schema Sync** | ? Detects model changes automatically | ? Must manually update scripts |
| **Data Seeding** | ? `Configuration.Seed()` method | ?? Separate INSERT scripts |
| **CI/CD Integration** | ? `Update-Database` in pipeline | ?? Custom deployment scripts |
| **Complexity** | ??? Medium | ????? High (for large schemas) |

**Recommendation:** ? **Use Entity Framework Migrations for all future changes**

---

## **?? NEXT STEPS AFTER MIGRATION**

Once `Update-Database` completes successfully:

1. ? **Verify data:** Run the verification queries above
2. ? **Screenshot slides:** Save 24 PowerPoint slides as JPG files
3. ? **Place images:** Copy to `C8.eServices.Mvc/Content/Training Slides/`
4. ? **Build solution:** Ensure no compilation errors
5. ? **Commit changes:**
   ```bash
   git add .
   git commit -m "Add Tenant Training System - EF Migrations"
   git push origin main
   ```

6. ? **Implement Controller & Views:** Follow the IMPLEMENTATION_GUIDE.md

---

## **?? IMPORTANT NOTES**

1. **Connection String:** Migrations use the `eServicesDbContext` connection string from `Web.config`
   - Current: `Server=localhost;Database=CRMPLMDEV_2025;Integrated Security=True`
   - Ensure SQL Server is running and database exists

2. **AutomaticMigrations:** Your `Configuration.cs` has:
   ```csharp
   AutomaticMigrationsEnabled = true;
   AutomaticMigrationDataLossAllowed = true;
   ```
   This means you can run `Update-Database` without explicitly calling `Add-Migration`

3. **Data Loss Warning:** `AutomaticMigrationDataLossAllowed = true` means EF can drop columns/tables during migrations. Be careful in production!

4. **Seed Method:** Runs **every time** you call `Update-Database`. The `AddOrUpdate()` method prevents duplicates.

---

## **? FINAL CHECKLIST**

- [ ] Package Manager Console opened
- [ ] Default project set to `C8.eServices.Mvc`
- [ ] Database connection working
- [ ] Run `Add-Migration AddTenantTrainingTables` (or skip if AutomaticMigrations enabled)
- [ ] Run `Update-Database -Verbose`
- [ ] Verify 8 tables created
- [ ] Verify 24 slides seeded
- [ ] Verify 16 questions seeded
- [ ] Verify 5 status keys seeded
- [ ] Commit migration files to Git

---

**?? YOU'RE DONE! The database is now ready for the training system.**

Next: Implement `TenantTrainingController` and Views (see IMPLEMENTATION_GUIDE.md)

---

Generated: 2025-01-15
System: Entity Framework Migrations Guide
Version: 1.0
