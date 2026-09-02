# Production Deployment Checklist
## Maintenance Defect Workflow Implementation

---

## 📋 Pre-Deployment Checklist

### 1. Environment Verification
- [ ] Verify you're connected to the **PRODUCTION** database
- [ ] Confirm database backup has been taken
- [ ] Verify backup can be restored if needed
- [ ] Check current system uptime/maintenance window
- [ ] Notify stakeholders of deployment window

### 2. Database Connection
- [ ] Update database name in `PRODUCTION_DEPLOYMENT.sql` (Line 9)
  ```sql
  USE [eServicesDb]  -- UPDATE THIS WITH YOUR PRODUCTION DATABASE NAME
  ```
- [ ] Test connection with read-only query:
  ```sql
  SELECT DB_NAME() AS CurrentDatabase
  ```

### 3. Pre-Deployment Backup
- [ ] Take full database backup:
  ```sql
  BACKUP DATABASE [eServicesDb] 
  TO DISK = 'C:\Backups\eServicesDb_PreMaintenance_' + CONVERT(VARCHAR, GETDATE(), 112) + '.bak'
  WITH INIT, COMPRESSION, STATS = 10
  ```
- [ ] Verify backup file exists and size is reasonable
- [ ] Copy backup to secure location

### 4. Application Code Deployment
- [ ] Deploy updated application code to production server
- [ ] Files to deploy:
  - `PropertyLeaseApplicationController.cs` (updated methods)
  - `PreferredComplexArea.cs` (model changes)
  - `PreferredComplexAreaAudit.cs` (audit model)
  - `AppSettingKeys.cs` (new keys)
  - `ResponsibilityTypeKeys.cs` (new keys)
  - `PropertyFacilitiesManagerReview.cshtml` (new view)
  - `RCS_Layout.cshtml` (navigation updates)
- [ ] Stop application pool/IIS site before deployment
- [ ] Deploy files
- [ ] **DO NOT START** application yet (database changes must run first)

---

## 🚀 Deployment Steps

### Step 1: Execute Database Script (15-30 seconds)
```powershell
# Navigate to scripts directory
cd "C:\DeploymentScripts"

# Execute production deployment script
sqlcmd -S PRODUCTION_SERVER -d eServicesDb -E -i "PRODUCTION_DEPLOYMENT.sql" -o "deployment_log.txt"
```

**Expected Output:**
```
✓ DEPLOYMENT COMPLETED SUCCESSFULLY
```

**If errors occur:**
- Script will automatically **ROLLBACK** all changes
- Check `deployment_log.txt` for error details
- Review DeploymentLog table for failure point
- Fix issue and re-run

### Step 2: Verify Database Changes
```sql
-- Check schema changes
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PreferredComplexAreas' 
AND COLUMN_NAME = 'MaintenanceManagerId'

-- Check AppSettings
SELECT [Key], [Value], [Description]
FROM AppSettings
WHERE [Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')

-- Check ResponsibilityTypes
SELECT [Key], [Name]
FROM ResponsibilityTypes
WHERE [Key] = 'r_property_facilities_manager_review'

-- Check Roles
SELECT Name, Id
FROM AspNetRoles
WHERE Name IN ('Maintenance Manager', 'Property & Facilities Manager')

-- Check deployment log
SELECT TOP 10 * FROM DeploymentLog ORDER BY ExecutedAt DESC
```

### Step 3: Start Application
- [ ] Start IIS application pool/site
- [ ] Verify application starts without errors
- [ ] Check application event log for errors
- [ ] Test homepage loads

---

## 👥 Post-Deployment Configuration

### Step 4: Create User Accounts

#### Option A: Via Application UI (Recommended)
1. [ ] Login as administrator
2. [ ] Navigate to User Management
3. [ ] Create new user: **Maintenance Manager**
   - Full Name: [Enter Name]
   - Email: [Enter Email]
   - Username: [Enter Username]
   - Role: Maintenance Manager
4. [ ] Create new user: **Property & Facilities Manager**
   - Full Name: [Enter Name]
   - Email: [Enter Email]
   - Username: [Enter Username]
   - Role: Property & Facilities Manager
5. [ ] Note the Customer IDs for both users

#### Option B: Via SQL (Advanced Users Only)
```sql
-- Example only - adjust as needed
-- This requires knowledge of your user creation process
-- Recommended to use application UI instead
```

### Step 5: Update AppSettings with Customer IDs

```sql
-- Get Customer IDs first
SELECT 
    c.Id AS CustomerId,
    c.FullName,
    u.UserName,
    r.Name AS RoleName
FROM Customers c
INNER JOIN AspNetUsers u ON c.SystemUserId = u.Id
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name IN ('Maintenance Manager', 'Property & Facilities Manager')

-- Update AppSettings (replace CUSTOMER_IDs)
UPDATE AppSettings 
SET [Value] = '12345'  -- Replace with actual Maintenance Manager Customer ID
WHERE [Key] = 'u_maintenance_manager'

UPDATE AppSettings 
SET [Value] = '67890'  -- Replace with actual Property & Facilities Manager Customer ID
WHERE [Key] = 'u_property_facilities_manager'

-- Verify update
SELECT [Key], [Value], [Description]
FROM AppSettings
WHERE [Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')
```

### Step 6: Assign Maintenance Managers to Complexes

```sql
-- Get list of active complexes
SELECT Id, Name, LettingOfficerId, HousingSuperId, MaintenanceManagerId, IsActive
FROM PreferredComplexAreas
WHERE IsDeleted = 0 AND IsActive = 1
ORDER BY Name

-- Assign to all active complexes (use Maintenance Manager Customer ID)
UPDATE PreferredComplexAreas
SET MaintenanceManagerId = 12345  -- Replace with actual Customer ID
WHERE IsActive = 1 
AND IsDeleted = 0
AND MaintenanceManagerId IS NULL

-- Verify assignment
SELECT 
    pca.Name AS ComplexName,
    c.FullName AS MaintenanceManagerName,
    pca.IsActive
FROM PreferredComplexAreas pca
LEFT JOIN Customers c ON pca.MaintenanceManagerId = c.Id
WHERE pca.IsDeleted = 0 AND pca.IsActive = 1
ORDER BY pca.Name
```

---

## ✅ Post-Deployment Testing

### Step 7: Functional Testing

#### Test 1: Navigation Menu Visibility
- [ ] Login as Maintenance Manager user
- [ ] Verify "Maintenance" menu appears in navigation
- [ ] Click "Maintenance Job Sheets" link - should load successfully

- [ ] Login as Property & Facilities Manager user
- [ ] Verify "Facilities Management" menu appears
- [ ] Click "Major Defect Reviews" link - should load successfully

#### Test 2: Round Robin Assignment
- [ ] Create test application (or use existing one)
- [ ] Progress application to ConductUnitInspection stage
- [ ] Mark inspection as "Major Defects" (NotHabitable)
- [ ] Verify Maintenance Manager receives work queue assignment
- [ ] Check RoundRobinQueue table for correct assignment

#### Test 3: Maintenance Job Sheet Workflow
- [ ] Login as Maintenance Manager
- [ ] Open assigned maintenance job sheet
- [ ] Upload maintenance completion documents
- [ ] Approve with "Major Defects" classification
- [ ] Verify Property & Facilities Manager receives work queue assignment

#### Test 4: Facilities Manager Review
- [ ] Login as Property & Facilities Manager
- [ ] Open major defect review
- [ ] Verify maintenance documents are visible
- [ ] Approve the review
- [ ] Verify application returns to unit inspection scheduling

#### Test 5: Minor Defects Workflow
- [ ] Create another test application
- [ ] Progress to maintenance job sheet
- [ ] Approve with "Minor Defects" classification
- [ ] Verify application continues automatically (no facilities review)

---

## 📊 Monitoring & Verification

### Step 8: System Health Checks

```sql
-- Check for any errors in deployment
SELECT * FROM DeploymentLog 
WHERE DeploymentName = 'Maintenance_Defect_Workflow_v1.0'
AND Status = 'FAILED'

-- Check work queue assignments
SELECT 
    rq.Id,
    rq.PropertyLeaseApplicationId,
    rt.Name AS ResponsibilityType,
    c.FullName AS AssignedTo,
    s.Name AS Status,
    rq.CapturedDateTime
FROM RoundRobinQueue rq
INNER JOIN ResponsibilityTypes rt ON rq.ResponsibilityTypeId = rt.Id
INNER JOIN Customers c ON rq.ClerkId = c.Id
INNER JOIN Status s ON rq.StatusId = s.Id
WHERE rt.[Key] = 'r_property_facilities_manager_review'
ORDER BY rq.CapturedDateTime DESC

-- Check complex assignments
SELECT 
    COUNT(*) AS TotalActive,
    SUM(CASE WHEN MaintenanceManagerId IS NULL THEN 1 ELSE 0 END) AS UnassignedCount,
    SUM(CASE WHEN MaintenanceManagerId IS NOT NULL THEN 1 ELSE 0 END) AS AssignedCount
FROM PreferredComplexAreas
WHERE IsActive = 1 AND IsDeleted = 0
```

---

## 🔙 Rollback Procedure (If Needed)

### Emergency Rollback Steps

1. **Stop Application**
   ```powershell
   Stop-Website -Name "YourSiteName"
   ```

2. **Execute Rollback Script**
   ```powershell
   sqlcmd -S PRODUCTION_SERVER -d eServicesDb -E -i "PRODUCTION_ROLLBACK.sql"
   ```

3. **Restore Previous Application Code**
   - Redeploy previous version of DLLs
   - Replace modified views with backups

4. **Restart Application**
   ```powershell
   Start-Website -Name "YourSiteName"
   ```

5. **Verify Rollback**
   ```sql
   -- Verify column removed
   SELECT COUNT(*) 
   FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'PreferredComplexAreas' 
   AND COLUMN_NAME = 'MaintenanceManagerId'
   -- Should return 0
   ```

---

## 📝 Documentation Updates

### Step 9: Update System Documentation
- [ ] Update user manuals for Maintenance Manager role
- [ ] Update user manuals for Property & Facilities Manager role
- [ ] Document new workflow in process documentation
- [ ] Update training materials
- [ ] Notify support team of new features

---

## 🎯 Success Criteria

Deployment is considered successful when:

- ✅ All database changes deployed without errors
- ✅ Application starts and runs without errors
- ✅ Both new roles can login and access their menus
- ✅ Work queue assignments route correctly
- ✅ Minor defect workflow continues automatically
- ✅ Major defect workflow halts for facilities review
- ✅ Email notifications are sent correctly
- ✅ No existing functionality is broken

---

## 📞 Support & Escalation

### If Issues Occur:
1. Check `DeploymentLog` table for errors
2. Review application event logs
3. Check IIS logs for runtime errors
4. Review database transaction log
5. If critical issues: Execute rollback procedure
6. Contact: [Your Support Contact]

---

## 📅 Deployment Record

**Date Deployed:** _________________  
**Deployed By:** _________________  
**Database Name:** _________________  
**Server:** _________________  
**Backup Location:** _________________  
**Issues Encountered:** _________________  
**Resolution:** _________________  
**Sign-off:** _________________

---

## ✅ Final Checklist

- [ ] All deployment steps completed
- [ ] All tests passed
- [ ] Users can access new features
- [ ] No critical errors in logs
- [ ] Stakeholders notified of completion
- [ ] Documentation updated
- [ ] Deployment record signed off

---

**Deployment Status:** [ ] Success [ ] Partial [ ] Rollback Required

**Notes:**
_______________________________________________________________________________
_______________________________________________________________________________
_______________________________________________________________________________
