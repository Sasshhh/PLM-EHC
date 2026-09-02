# Maintenance Manager Assignment Fix - Unit Maintenance Workflow

## Issue Summary
When completing "minor defects" inspection outcome, the RoundRobin code was incorrectly assigning work to **Letting Officers** instead of **Maintenance Managers**, causing the PropertyLeaseInspections queue to show empty for maintenance managers despite successful RRQ creation.

## Root Cause
**File**: `PropertyLeaseApplicationController.cs`  
**Method**: `EHCRoundRobin` (UnitMaintenance branch, lines 8658-8690)

The code was using:
- `GetBackOfficeId(db, RCSAppID, false)` → Returns Letting Officer
- `AppSettingKeys.LettingOfficer` → Wrong user type for maintenance tasks

## The Fix
**Changed lines 8663-8664:**

### BEFORE (WRONG):
```csharp
var UserId = GetBackOfficeId(db, RCSAppID, false);
var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
```

### AFTER (CORRECT):
```csharp
var UserId = GetMaintenanceManagerId(db, RCSAppID);
var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.MaintenanceManager).FirstOrDefault().Value);
```

## Why This Matters

### PropertyLeaseInspections Queue Filters
The PropertyLeaseInspections action filters RoundRobinQueues by **4 ResponsibilityTypes**:
1. `ResponsibilityTypeKeys.Inspections` (Conduct Unit Inspection - Housing Supervisor)
2. `ResponsibilityTypeKeys.ScheduleInspectionSlots` (Schedule slots - Housing Supervisor)
3. `ResponsibilityTypeKeys.MaintananceJobSheet` (Maintenance Job Card)
4. `ResponsibilityTypeKeys.UnitMaintenanance` ← **This one**

### The Problem Flow
1. User completes unit inspection with "minor defects" outcome
2. RoundRobin creates RRQ with **ResponsibilityTypeKeys.UnitMaintenanance** ✅ (Correct)
3. But assigns to **Letting Officer** via `GetBackOfficeId()` ❌ (Wrong user type)
4. Maintenance Manager (coesolardev11) logs in
5. PropertyLeaseInspections queries: `ClerkId == UserId && ResponsibilityTypeId == UnitMaintenanance`
6. **Result**: Empty queue because RRQ is assigned to Letting Officer, not Maintenance Manager

### The Solution Flow
1. User completes unit inspection with "minor defects" outcome
2. RoundRobin creates RRQ with **ResponsibilityTypeKeys.UnitMaintenanance** ✅ (Correct)
3. Now assigns to **Maintenance Manager** via `GetMaintenanceManagerId()` ✅ (Correct)
4. Maintenance Manager (coesolardev11) logs in
5. PropertyLeaseInspections queries: `ClerkId == UserId && ResponsibilityTypeId == UnitMaintenanance`
6. **Result**: Work appears in queue! ✅

## Supporting Methods
Both methods already exist in the codebase:

- **GetMaintenanceManagerId** (line 7614): Gets Maintenance Manager for unit's complex
- **AppSettingKeys.MaintenanceManager** (`u_maintenance_manager`): Fallback maintenance manager

## Related Fixes
This is the **SAME BUG PATTERN** as the Housing Supervisor issue fixed earlier:
- **Housing Supervisor Fix** (lines 7872-7873): Changed from `GetBackOfficeId/LettingOfficer` to `GetHousingSupervisorId/HousingSupervisor`
- **Maintenance Manager Fix** (lines 8663-8664): Changed from `GetBackOfficeId/LettingOfficer` to `GetMaintenanceManagerId/MaintenanceManager`

Both fixes follow the principle: **Use the specialized getter method for the specific role, not the generic BackOffice method.**

## Testing Steps
1. **Restart debugging session** (code changes not hot-reloaded)
2. Log in as Housing Supervisor (e.g., coesolardev07)
3. Complete unit inspection with "minor defects" outcome
4. **Log in as Maintenance Manager** (e.g., coesolardev11)
5. Navigate to: `/PropertyLeaseApplication/PropertyLeaseInspections`
6. **Verify**: Work item appears in queue
7. **Verify RRQ**: Check database - ClerkId should match maintenance manager's Customer ID

## Database Verification Query
```sql
-- Check the latest RRQ for an application
SELECT TOP 1 
    rrq.Id,
    rrq.PropertyLeaseApplicationId,
    rrq.ClerkId,
    rt.Name as ResponsibilityName,
    rt.[Key] as ResponsibilityKey,
    c.Firstname + ' ' + c.Surname as AssignedTo,
    s.Name as StatusName
FROM RoundRobinQueues rrq
LEFT JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
LEFT JOIN Customers c ON rrq.ClerkId = c.Id
LEFT JOIN Status s ON rrq.StatusId = s.Id
WHERE rrq.PropertyLeaseApplicationId = <APPLICATION_ID>
    AND rrq.IsDeleted = 0
    AND rt.[Key] = 'r_unit_maintenance'
ORDER BY rrq.CreatedDate DESC
```

Expected result:
- ResponsibilityKey: `r_unit_maintenance`
- AssignedTo: Maintenance Manager name (e.g., coesolardev11's full name)
- StatusName: `Submitted`

## Build Status
✅ **Build Successful** - Code compiles without errors

## Code Quality Improvement
This fix improves code quality by:
1. **Using specialized methods**: Maintenance tasks assigned to Maintenance Managers
2. **Following existing patterns**: Same pattern as Housing Supervisor fix
3. **Correct role segregation**: Each workflow stage assigns to appropriate role
4. **Queue visibility**: Work appears for correct users

## Impact
- ✅ Maintenance Manager can now see assigned maintenance work
- ✅ PropertyLeaseInspections queue works correctly for maintenance workflow
- ✅ Consistent with Housing Supervisor assignment pattern
- ✅ No breaking changes to other workflows

---
**Fixed**: 2025-03-XX
**Build Status**: ✅ Successful
**Testing**: ⏳ Awaiting restart + user verification
