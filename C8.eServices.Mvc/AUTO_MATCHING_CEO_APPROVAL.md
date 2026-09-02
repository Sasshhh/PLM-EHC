# Automatic Unit Matching After CEO Approval

## Overview
Implemented automatic unit matching/allocation functionality that triggers immediately after CEO approves an application during the Risk Assessment step.

## What Was Added

### 1. **Modified CEO Approval Logic** (Lines 607-644 in RiskAssessmentOutcomesController.cs)
When the CEO approves an application:
1. Application is added to the waiting list
2. **NEW:** System automatically attempts to match an available unit
3. If a match is found → Unit is allocated and applicant is notified
4. If no match → Applicant remains in waiting list

### 2. **New Method: TryAutoMatchUnit** (Lines ~720-820 in RiskAssessmentOutcomesController.cs)
```csharp
private bool TryAutoMatchUnit(eServicesDbContext context, int appId, int complexId, int typologyId)
```

**What it does:**
- Searches for available units matching the applicant's preferences (complex + typology)
- If found:
  - Locks the unit (IsTaken = true)
  - Creates a MatchedUnits record
  - Updates waiting list status to "Offered"
  - Changes application status to "Awaited" (awaiting customer acceptance)
  - Records allocation history
  - Sends email notification
  - Returns `true`
- If not found:
  - Applicant stays in "Waiting" status
  - Returns `false`

## Behavior

### **Scenario 1: Unit Available**
```
CEO Approves → Add to Waiting List → Auto-Match Found → Unit Allocated → Email Sent
Status: "Awaited" (Waiting for customer to accept unit)
Message: "Application Approved & Unit Automatically Matched!"
```

### **Scenario 2: No Unit Available**
```
CEO Approves → Add to Waiting List → No Match Found → Stay in Queue
Status: "s_added_to_waiting_list"
Message: "Application Approved & Added to Waiting List."
```

## Technical Details

### Matching Criteria
The system matches units based on:
1. **Preferred Complex** (`OfferedComplexId` = `PreferredComplexAreaId`)
2. **Preferred Typology** (`HumanEHCOptionId` = `HumanEHCOptionsId`)
3. **Unit Availability** (`IsTaken = false` AND `IsActive = true`)

### Database Updates
When a match is found:
- **ApplicationAllocatedProperty**: 
  - `AllocatedByUserId` = Current CEO's SystemUser.Id
  - `PropertyLeaseApplicationId` = Application ID
  - `IsTaken` = true
- **MatchedUnits**: New record created with `IsAccepted = false`
- **PropertyLeaseWaitingLists**: `QueueStatus` = "Offered", `OfferedUnitId` = Unit ID
- **PropertyLeaseApplications**: `StatusId` = "awaited" status
- **UnitAllocationHistories**: New history entry with "Unit Auto-Allocated by CEO Approval"

### Email Notification
Uses existing email template: `EmailContentKeys.AtUnitMatchApplication`

## Error Handling
- Wrapped in try-catch block
- If auto-matching fails:
  - Error is logged (console)
  - Applicant remains in waiting list
  - CEO approval still completes successfully
  - No crash or data loss

## Benefits

### 1. **Instant Allocation**
- No manual intervention needed if units are available
- Applicants get units immediately upon approval

### 2. **Graceful Fallback**
- If no match → Traditional waiting list flow continues
- No disruption to existing process

### 3. **Transparent**
- Session message tells CEO if a match was made or not
- Full audit trail in UnitAllocationHistories

### 4. **FIFO Respected**
- Since applicant is added to waiting list first, queue order is maintained
- If multiple applicants approved simultaneously, proper order is preserved

## Testing Checklist

### Test Case 1: Available Unit Match
- [ ] CEO approves application
- [ ] Unit with matching complex/typology exists (IsTaken=false)
- [ ] Verify unit is allocated
- [ ] Verify application status = "awaited"
- [ ] Verify email sent to applicant
- [ ] Verify success message shows "Unit Automatically Matched!"

### Test Case 2: No Available Unit
- [ ] CEO approves application
- [ ] No matching units available
- [ ] Verify application added to waiting list
- [ ] Verify status = "s_added_to_waiting_list"
- [ ] Verify message shows "Added to Waiting List."

### Test Case 3: Multiple Simultaneous Approvals
- [ ] CEO approves multiple applications for same complex/typology
- [ ] Only 1 unit available
- [ ] First applicant gets unit
- [ ] Others remain in waiting list

### Test Case 4: Error Handling
- [ ] Simulate database error during matching
- [ ] Verify CEO approval still completes
- [ ] Verify applicant in waiting list
- [ ] Verify no data corruption

## Future Enhancements

### Possible Improvements:
1. **Priority Matching**: Consider applicant score/ranking when multiple waiting
2. **Partial Match Alert**: Notify admin if unit is almost matching (different complex but same typology)
3. **Batch Retry**: Periodically retry matching for waiting applicants when new units become available
4. **Dashboard Indicator**: Show "Auto-Matched" badge in CEO inbox

## Configuration

### Key Status Values (Must exist in Status table):
- `s_added_to_waiting_list` - For applicants in queue
- `StatusKeys.awaited` - For applicants offered a unit

### Key Email Template (Must exist in EmailContentTypes):
- `EmailContentKeys.AtUnitMatchApplication` - Unit offer notification

### Required Tables:
- `ApplicationAllocatedProperty` (Units)
- `PropertyLeaseWaitingLists` (Queue)
- `MatchedUnits` (Offers)
- `PropertyLeaseApplications` (Applications)
- `UnitAllocationHistories` (Audit trail)

## Files Modified
- `C8.eServices.Mvc\Controllers\RiskAssessmentOutcomesController.cs`
  - Modified: `RiskAssessmentCEO` POST method (Line ~610-644)
  - Added: `TryAutoMatchUnit` private method (Line ~720-820)

## Deployment Notes
1. ✅ No database changes required
2. ✅ No web.config changes required
3. ✅ Uses existing email templates
4. ✅ Compatible with existing workflow
5. ⚠️ Stop debugger and rebuild solution
6. ⚠️ Restart IIS/application pool

## Rollback
To disable automatic matching, comment out line ~632:
```csharp
// bool unitMatched = TryAutoMatchUnit(context, rcsAppId, complexId, typologyId);
```

Applicants will go to waiting list as before.

---
**Implementation Date**: 2026-03-26  
**Developer**: GitHub Copilot  
**Status**: ✅ Ready for Testing
