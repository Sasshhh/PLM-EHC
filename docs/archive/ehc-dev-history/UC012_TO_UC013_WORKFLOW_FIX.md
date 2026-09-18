# UC012 → UC013 Workflow Integration Fix

**Date:** 2025-01-XX  
**Status:** ✅ COMPLETE - Ready for Testing

---

## Problem Statement

Previously, **SaveMaintenanceSignature** only routed **major defects (NotHabitable)** to Property & Facilities Manager for review. Minor defects were NOT routed to UC013, violating the requirement that **ALL maintenance** must be reviewed by Facilities Manager.

---

## Solution Implemented

Updated **SaveMaintenanceSignature** (AJAX endpoint) to handle **complete workflow routing** for UC012 → UC013 transition.

### Key Changes

**File:** `C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs`  
**Method:** `SaveMaintenanceSignature` (lines ~17584-17670)

#### ✅ **1. Route ALL Maintenance to Facilities Manager (UC013)**

```csharp
// Create RoundRobinQueue for PropertyFacilitiesManagerReview
// Send notification to Facilities Manager
// Works for BOTH HabitableMinorDefects AND NotHabitable
```

**What Happens:**
- Creates work queue item with `ResponsibilityTypeKeys.PropertyFacilitiesManagerReview`
- Assigns to Property & Facilities Manager (via `GetPropertyFacilitiesManagerId` or AppSettings fallback)
- Sends `BackOfficeNotification` to Facilities Manager
- Marks current Maintenance Job Sheet queue as finished

---

#### ✅ **2. Differentiated Status Logic**

| Defect Type | Application Status | Workflow Behavior |
|-------------|-------------------|-------------------|
| **Major Defects (NotHabitable)** | `CustomerQueryPending` | ❌ **Application BLOCKED** - cannot progress until Facilities Manager approves |
| **Minor Defects (HabitableMinorDefects)** | *No change* | ✅ **Application CONTINUES** - normal workflow proceeds while Facilities Manager reviews in parallel |

**Code Logic:**
```csharp
if (actionType.Key == RCSActionTypeKeys.NotHabitable) {
    // Block application
    application.StatusId = CustomerQueryPending;
    ActivityTracker: "application blocked pending approval"
}
else if (actionType.Key == RCSActionTypeKeys.HabitableMinorDefects) {
    // Don't block application
    // NO status change
    ActivityTracker: "application continues while review happens"
}
```

---

#### ✅ **3. Proper Queue Management**

- Marks Maintenance Job Sheet RoundRobinQueue as **finished** via `MatchingHelper.RoundRobinMarkJobAsFinished`
- Creates new PropertyFacilitiesManagerReview queue with `StatusKeys.Submitted`
- Includes `CurrentTaskDateTime` for proper tracking

---

## Workflow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│ UC012: Maintenance Manager Completes Job Card                   │
│  - Adds tasks                                                   │
│  - Uploads before/after images                                 │
│  - Uploads task documents                                      │
│  - Captures signature with official number & reason           │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       │ SaveMaintenanceSignature (AJAX)
                       │
                       ▼
        ┌──────────────────────────────────┐
        │ 1. Save signature to database    │
        │ 2. Update JobCardSubmitted=true  │
        │ 3. UnitMaintenanceCompleted=true│
        └──────────────┬───────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────┐
        │ 4. Route to Facilities Manager   │
        │    - Create RoundRobinQueue      │
        │    - ResponsibilityType: UC013   │
        │    - Send notification           │
        └──────────────┬───────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────┐
        │ 5. Check Defect Type            │
        └──────────────┬───────────────────┘
                       │
           ┌───────────┴────────────┐
           │                        │
           ▼                        ▼
    ┌──────────────┐      ┌─────────────────┐
    │ MAJOR        │      │ MINOR           │
    │ (NotHabitable)│     │ (HabitableMinor)│
    └──────┬───────┘      └────────┬────────┘
           │                       │
           ▼                       ▼
    Status:                  Status:
    CustomerQueryPending     No Change
    (BLOCKS app)             (App continues)
           │                       │
           └───────────┬───────────┘
                       │
                       ▼
        ┌──────────────────────────────────┐
        │ 6. Mark UC012 queue finished     │
        │ 7. Activity tracker audit        │
        │ 8. Return success to client      │
        └──────────────────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────┐
        │ UC013: Property & Facilities     │
        │ Manager Reviews Maintenance      │
        │ (Next workflow step)             │
        └──────────────────────────────────┘
```

---

## Testing Scenarios

### ✅ **Scenario 1: Minor Defects (Your Current Record)**

**Given:** Maintenance record with `RCSActionType = HabitableMinorDefects`

**When:** Maintenance Manager signs job card

**Then:**
- ✅ Signature saved
- ✅ RoundRobinQueue created for PropertyFacilitiesManagerReview
- ✅ Notification sent to Facilities Manager
- ✅ Maintenance Job Sheet queue marked finished
- ✅ **Application status UNCHANGED** (continues normal workflow)
- ✅ Activity tracker: "application continues while review happens"

**Expected Outcome:**
- Application can proceed to next workflow step (e.g., schedule re-inspection)
- Facilities Manager sees job card in their inbox for review
- No application blocking

---

### ✅ **Scenario 2: Major Defects**

**Given:** Maintenance record with `RCSActionType = NotHabitable`

**When:** Maintenance Manager signs job card

**Then:**
- ✅ Signature saved
- ✅ RoundRobinQueue created for PropertyFacilitiesManagerReview
- ✅ Notification sent to Facilities Manager
- ✅ Maintenance Job Sheet queue marked finished
- ✅ **Application status = CustomerQueryPending** (blocks progression)
- ✅ Activity tracker: "application blocked pending approval"

**Expected Outcome:**
- Application **BLOCKED** until Facilities Manager approves
- Facilities Manager sees job card in their inbox for review
- Application cannot proceed until UC013 completes

---

## Safety Checks ✅

### ✅ **No Double Processing**
- Signature check prevents duplicate submissions (line 17548-17554)
- RoundRobinQueue creation only happens once per signature
- MatchingHelper marks old queue as finished

### ✅ **Safe to Re-run**
- If signature already exists, returns error without processing
- Database transaction ensures atomicity
- Activity tracker audit provides clear trail

### ✅ **No Breaking Changes**
- Only modified SaveMaintenanceSignature workflow routing
- View changes limited to UI improvements (signature box size, bottom form hiding)
- No changes to database schema
- No changes to UC013 (PropertyFacilitiesManagerReview) yet

---

## Verification Steps

**1. Check Current Maintenance Record:**
```sql
SELECT 
    m.Id,
    m.PropertyLeaseApplicationId,
    rt.Name AS DefectType,
    rt.Key AS DefectKey,
    m.JobCardSubmitted,
    m.UnitMaintenanceCompleted
FROM AllocatedUnitMaintenanceEHCs m
INNER JOIN RCSActionTypes rt ON m.RCSActionTypeId = rt.Id
WHERE m.PropertyLeaseApplicationId = [YOUR_APP_ID]
ORDER BY m.Id DESC
```

**2. Check Signature Status:**
```sql
SELECT * FROM MaintenanceJobCardSignatures 
WHERE AllocatedUnitMaintenanceEHCId = [MAINTENANCE_ID]
AND IsDeleted = 0
```

**3. Check RoundRobinQueue:**
```sql
SELECT 
    rrq.*,
    rt.Name AS ResponsibilityType,
    rt.Key AS ResponsibilityKey,
    s.Name AS Status
FROM RoundRobinQueues rrq
INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
INNER JOIN Status s ON rrq.StatusId = s.Id
WHERE rrq.PropertyLeaseApplicationId = [YOUR_APP_ID]
ORDER BY rrq.Id DESC
```

**4. Check Application Status:**
```sql
SELECT 
    pla.Id,
    pla.ApplicationReferenceNumber,
    s.Name AS CurrentStatus,
    s.Key AS StatusKey
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.Id = [YOUR_APP_ID]
```

---

## Expected Database State After Signature

**For Minor Defects Application:**

| Table | Expected State |
|-------|---------------|
| **MaintenanceJobCardSignatures** | 1 new record with `AllocatedUnitMaintenanceEHCId` |
| **AllocatedUnitMaintenanceEHCs** | `JobCardSubmitted=1`, `UnitMaintenanceCompleted=1` |
| **RoundRobinQueues** | New queue with `ResponsibilityTypeKeys.PropertyFacilitiesManagerReview` |
| **RoundRobinQueues** | Old queue with `ResponsibilityTypeKeys.MaintananceJobSheet` marked finished |
| **PropertyLeaseApplications** | **Status UNCHANGED** (not CustomerQueryPending) |
| **ActivityTrackers** | New audit entry: "application continues while review happens" |

---

## Next Steps

1. ✅ **Test Minor Defects Workflow** (Your Current Record)
   - Sign job card via signature capture
   - Verify queue created for Facilities Manager
   - Verify application status UNCHANGED
   - Verify job card appears in Facilities Manager inbox

2. ⏳ **Implement UC013 Signature Capture** (PropertyFacilitiesManagerReview)
   - Add signature section to PropertyFacilitiesManagerReview.cshtml
   - Update POST method to handle signature-based approval
   - Implement status updates per UC013 requirements
   - Add notifications to CSO & Maintenance Manager
   - Implement CEO oversight functionality

3. ⏳ **Test Major Defects Workflow**
   - Create test record with NotHabitable defect type
   - Verify application blocked (CustomerQueryPending)
   - Verify Facilities Manager review workflow

---

## Files Modified

### Controller Changes
- ✅ `C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs`
  - Method: `SaveMaintenanceSignature` (lines ~17584-17670)
  - Added: Complete workflow routing for UC012 → UC013
  - Added: Differentiated logic for major vs. minor defects
  - Added: Queue management with MatchingHelper

### View Changes (UI Only)
- ✅ `C8.eServices.Mvc\Views\PropertyLeaseApplication\MaintenanceJobSheet.cshtml`
  - Fixed: JavaScript function name collision
  - Updated: Signature canvas size (600x150)
  - Added: Bottom form auto-hide when signed

---

## Rollback Instructions

If issues arise, revert commit with:
```bash
git diff HEAD~1 C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs
git checkout HEAD~1 -- C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs
```

Or manually revert lines 17584-17670 to previous logic.

---

## Status Summary

| Component | Status | Notes |
|-----------|--------|-------|
| SaveMaintenanceSignature routing | ✅ COMPLETE | Routes ALL maintenance to UC013 |
| Minor defects logic | ✅ COMPLETE | Application continues (no status block) |
| Major defects logic | ✅ COMPLETE | Application blocked (CustomerQueryPending) |
| Queue management | ✅ COMPLETE | Marks old queue finished, creates new |
| Notifications | ✅ COMPLETE | Facilities Manager notified |
| Activity tracker | ✅ COMPLETE | Clear audit trail |
| Safety checks | ✅ COMPLETE | No double processing, safe re-run |
| Compilation | ✅ COMPLETE | No errors |
| UC013 implementation | ⏳ PENDING | Next phase |

---

**READY FOR TESTING! 🚀**

Your application with minor defects should now route to Facilities Manager (UC013) without blocking the application status.
