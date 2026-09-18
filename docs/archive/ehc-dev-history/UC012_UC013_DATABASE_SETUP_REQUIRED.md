# UC012 → UC013 Database Setup Required

## ❌ CRITICAL ISSUE IDENTIFIED

The **Property & Facilities Manager Review** responsibility type is **MISSING** from the database. This will cause the workflow routing to fail.

---

## Quick Fix (Run this NOW)

### Option 1: Master Setup Script (Recommended)
```sql
-- Run this one script to set up everything:
C8.eServices.Mvc\Scripts\MASTER_SETUP_UC012_UC013.sql
```

This will:
- ✅ Add `PropertyFacilitiesManagerReview` ResponsibilityType
- ✅ Add `PropertyFacilitiesManagerId` AppSetting (fallback user)
- ✅ Verify `MaintenanceManagerId` AppSetting exists
- ✅ Run complete verification

### Option 2: Individual Scripts
If you prefer step-by-step:

```sql
-- 1. Add ResponsibilityType (REQUIRED)
C8.eServices.Mvc\Scripts\add_facilities_manager_responsibility_type.sql

-- 2. Add AppSetting (REQUIRED)
C8.eServices.Mvc\Scripts\add_property_facilities_manager_appsetting.sql

-- 3. Verify everything (RECOMMENDED)
C8.eServices.Mvc\Scripts\verify_uc012_uc013_setup.sql
```

---

## What's Missing & Why It Matters

### 1. ResponsibilityType: `r_property_facilities_manager_review`

**Location in Code:**
```csharp
// PropertyLeaseApplicationController.cs - SaveMaintenanceSignature (line ~17600)
var responsibilityType = db.ResponsibilityTypes
    .FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.PropertyFacilitiesManagerReview);
```

**Why it fails:**
- `FirstOrDefault()` returns `null` when ResponsibilityType doesn't exist
- Code tries to use `responsibilityType.Id` → **NullReferenceException**
- Workflow stops, no queue created, no routing to Facilities Manager

**Impact:**
- ❌ UC012 signature submission fails
- ❌ No RoundRobinQueue created for UC013
- ❌ Facilities Manager never gets notification
- ❌ Application stuck in broken state

### 2. AppSetting: `PropertyFacilitiesManagerId`

**Location in Code:**
```csharp
// PropertyLeaseApplicationController.cs - SaveMaintenanceSignature (line ~17587)
var facilitiesManager = GetPropertyFacilitiesManagerId(db, maintenance.PropertyLeaseApplicationId);
var facilitiesManagerId = facilitiesManager?.Id ?? 
    Convert.ToInt32(db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.PropertyFacilitiesManager)?.Value ?? "0");
```

**Why it's needed:**
- Fallback when complex doesn't have assigned Facilities Manager
- Without it: `facilitiesManagerId = 0` → invalid user ID
- Queue created but not assigned to anyone

**Impact:**
- ⚠️  Queue created but unassigned (if complex has no Facilities Manager)
- ⚠️  No notification sent (invalid user ID)
- ⚠️  Work sits in queue with no assigned reviewer

---

## Expected Database State After Setup

### ResponsibilityTypes Table
```sql
Key                                    | Name                                      | IsActive
---------------------------------------|-------------------------------------------|----------
r_maintanance_job_sheet                | Maintenance Job Sheet                     | 1
r_property_facilities_manager_review   | Property & Facilities Manager Review      | 1
```

### AppSettings Table
```sql
Key                            | Value  | Description
-------------------------------|--------|------------------------------------------
MaintenanceManagerId           | <ID>   | Default Maintenance Manager for UC012
PropertyFacilitiesManagerId    | <ID>   | Default Facilities Manager for UC013
```

### PreferredComplexAreas Table (Complex Assignments)
```sql
ComplexName         | MaintenanceManagerId | PropertyFacilitiesManagerId
--------------------|----------------------|---------------------------
Airport Park        | 123                  | 456
Clayville           | 789                  | 456
...                 | ...                  | ...
```

---

## Verification Steps

### 1. Run Verification Script
```sql
-- Check all requirements:
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

### 2. Test Workflow
```sql
-- Use test application EHC2026032600001
Application ID: 5218
Maintenance ID: 2008
Defect Type: Habitable - Minor defects

-- After signing job card, verify:
1. Check signature created:
   SELECT * FROM MaintenanceJobCardSignatures WHERE AllocatedUnitMaintenanceEHCId = 2008

2. Check queue created:
   SELECT * FROM RoundRobinQueues 
   WHERE PropertyLeaseApplicationId = 5218 
     AND ResponsibilityTypeId = (SELECT Id FROM ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review')

3. Check notification sent:
   SELECT * FROM BackOfficeNotifications 
   WHERE PropertyLeaseApplicationId = 5218 
   ORDER BY Id DESC

4. Check application status:
   SELECT s.Name FROM PropertyLeaseApplications p
   INNER JOIN Status s ON p.StatusId = s.Id
   WHERE p.Id = 5218
   -- Expected: "In Awaiting Tenant Update Details" (unchanged for minor defects)
```

---

## Common Issues & Solutions

### Issue: "Sequence contains no elements"
**Cause:** ResponsibilityType doesn't exist  
**Solution:** Run `add_facilities_manager_responsibility_type.sql`

### Issue: "Value cannot be null. Parameter name: facilitiesManagerId"
**Cause:** AppSetting missing AND complex has no assigned Facilities Manager  
**Solution:** Run `add_property_facilities_manager_appsetting.sql`

### Issue: "Object reference not set to an instance of an object"
**Cause:** `responsibilityType` is null  
**Solution:** Run `MASTER_SETUP_UC012_UC013.sql`

---

## Rollback (If Needed)

```sql
-- Remove ResponsibilityType
DELETE FROM ResponsibilityTypes 
WHERE [Key] = 'r_property_facilities_manager_review'

-- Remove AppSetting
DELETE FROM AppSettings 
WHERE [Key] = 'PropertyFacilitiesManagerId'
```

---

## Next Steps After Setup

1. ✅ Run `MASTER_SETUP_UC012_UC013.sql`
2. ✅ Run `verify_uc012_uc013_setup.sql` to confirm
3. ✅ Navigate to `/PropertyLeaseApplication/MaintenanceJobSheet/2008`
4. ✅ Sign job card with test data
5. ✅ Verify PropertyFacilitiesManagerReview queue created
6. ✅ Verify Facilities Manager received notification
7. ✅ Implement UC013 signature capture (next phase)

---

## Files Created

1. **verify_uc012_uc013_setup.sql** - Comprehensive verification script
2. **add_property_facilities_manager_appsetting.sql** - Add fallback Facilities Manager
3. **MASTER_SETUP_UC012_UC013.sql** - Complete setup in one script
4. **UC012_UC013_DATABASE_SETUP_REQUIRED.md** - This documentation

---

## References

- **Controller:** `PropertyLeaseApplicationController.cs` (SaveMaintenanceSignature, lines 17540-17670)
- **Keys:** `ResponsibilityTypeKeys.cs` (PropertyFacilitiesManagerReview)
- **Keys:** `AppSettingKeys.cs` (PropertyFacilitiesManager)
- **Workflow Doc:** `UC012_TO_UC013_WORKFLOW_FIX.md`
- **Implementation Doc:** `UC012_IMPLEMENTATION_COMPLETE.md`
