# ?? FIX: ClientTraining Method - Remove Premature Inspection Scheduling

**File:** `C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs`  
**Method:** `ClientTraining(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel dvm)`  
**Issue:** `EHCRoundRobin()` is being called **BEFORE** tenant completes training/exam  
**Solution:** **COMPLETELY REMOVE** the `EHCRoundRobin()` call from ClientTraining  

---

## ?? **THE PROBLEM**

Currently, `ClientTraining()` does this:

```csharp
[DecryptParameter]
[HttpPost]
public ActionResult ClientTraining(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel dvm)
{
    using (var cxt = new eServicesDbContext())
    {
        // ... initialization code ...
        
        // ? PROBLEM 1: Duplicate status changes
        MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, rcsApps.Id);
        MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingOnlineTraining).Id, rcsApps.Id);

        // ... mark training invite task as finished ...

        // ? PROBLEM 2: Schedules inspection BEFORE tenant completes training/exam
        EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, 
            true,  // ? ShechuleInspectionSlots = true (WRONG!)
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
            1, 
            false, false, 
            1);
        
        // ... redirect ...
    }
}
```

**This causes:**
1. ? Duplicate status changes (sets status twice)
2. ? Inspection task assigned **BEFORE** tenant takes exam
3. ? Wrong workflow sequence

---

## ? **THE SOLUTION**

**Remove BOTH issues:**

```csharp
[DecryptParameter]
[HttpPost]
public ActionResult ClientTraining(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel dvm)
{
    using (var cxt = new eServicesDbContext())
    {
        Initialise();
        var userID = Customer.Id;
        var Keys = cxt.Status;
        var rcsApps = cxt.PropertyLeaseApplications
            .Where(x => x.Id == id && x.IsDeleted == false)
            .Include(x => x.Customer)
            .FirstOrDefault();

        // Create MeetingRequest
        MeetingRequest meeting = dvm.MeetingRequest;
        meeting.PropertyLeaseApplicationId = (int)id;
        meeting.CustomerId = userID;
        cxt.MeetingRequests.Add(meeting);
        cxt.SaveChanges();

        // ? CREATE TENANT TRAINING RECORD
        var existingTraining = cxt.TenantTrainings
            .FirstOrDefault(t => t.PropertyLeaseApplicationId == id && !t.IsDeleted);

        if (existingTraining == null)
        {
            var token = Guid.NewGuid().ToString();
            var expiryDate = DateTime.Now.AddDays(30);

            var training = new TenantTraining
            {
                PropertyLeaseApplicationId = (int)id,
                InvitationToken = token,
                TokenExpiryDate = expiryDate,
                InvitationSentDate = DateTime.Now,
                CurrentSlideNumber = 0,
                IsTrainingCompleted = false,
                IsExamPassed = false,
                ExamAttempts = 0,
                CreatedDateTime = DateTime.Now,
                ModifiedDateTime = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                IsLocked = false
            };

            cxt.TenantTrainings.Add(training);
            cxt.SaveChanges();
        }

        // Send email
        int emailboodyId = cxt.EmailContentTypes
            .FirstOrDefault(x => x.Key == EmailContentKeys.InviteTenantForTraining).Id;
        EmailHelper.CustomerEmailNotification(cxt, rcsApps.Id, emailboodyId);

        // ? FIX 1: Only ONE status change - set to AwaitingOnlineTraining
        MatchingHelper.ChangeApplicationStatus(
            cxt, 
            cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingOnlineTraining).Id, 
            rcsApps.Id
        );

        // Mark "Invite to Training" task as finished
        var activeDirectoryOn = Convert.ToInt16(
            db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer)
                .FirstOrDefault().Value
        );
        var ResponsibilityTypeId = db.ResponsibilityTypes
            .Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining)
            .FirstOrDefault();
        MatchingHelper.RoundRobinMarkJobAsFinished(
            cxt, 
            (int)rcsApps.Id, 
            null, 
            ResponsibilityTypeId.Id, 
            activeDirectoryOn
        );

        // ? FIX 2: REMOVE EHCRoundRobin() call completely!
        // Inspection scheduling will happen AFTER exam pass in TenantTrainingController

        Session["ClientTrainingInviteSession"] = string.Format(
            $"Tenant has been invited successfully for application reference ,{rcsApps.ApplicationReferenceNumber}"
        );
        
        return RedirectToAction("PropertyLeaseTenantTraining");
    }
}
```

---

## ?? **WHAT WAS REMOVED**

### **1. Duplicate Status Change Line** ? REMOVED
```csharp
// ? REMOVED THIS LINE:
MatchingHelper.ChangeApplicationStatus(
    cxt, 
    cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, 
    rcsApps.Id
);
```

### **2. Entire EHCRoundRobin() Call** ? REMOVED
```csharp
// ? REMOVED THIS ENTIRE BLOCK:
EHCRoundRobin(
    rcsApps.Id, 
    false, false, false, false, false, false, false, false, 
    true,  // ShechuleInspectionSlots = true
    false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
    1, 
    false, false, 
    1
);
```

---

## ? **CORRECT WORKFLOW AFTER FIX**

```
??????????????????????????????????????????????????????????
?  1. ClientTraining() Method Called                     ?
?     ?? Admin invites tenant to training                ?
??????????????????????????????????????????????????????????
                        ?
??????????????????????????????????????????????????????????
?  2. Create TenantTraining Record                       ?
?     ?? Generate token, set expiry (30 days)            ?
??????????????????????????????????????????????????????????
                        ?
??????????????????????????????????????????????????????????
?  3. Send Training Invitation Email                     ?
?     ?? Includes link with unique token                 ?
??????????????????????????????????????????????????????????
                        ?
??????????????????????????????????????????????????????????
?  4. ? Set Status: AwaitingOnlineTraining               ?
?     ?? ONLY ONE status change now!                     ?
??????????????????????????????????????????????????????????
                        ?
??????????????????????????????????????????????????????????
?  5. Mark "Invite to Training" Task as Complete         ?
?     ?? MatchingHelper.RoundRobinMarkJobAsFinished()    ?
??????????????????????????????????????????????????????????
                        ?
??????????????????????????????????????????????????????????
?  6. ? NO EHCRoundRobin() Call!                         ?
?     ?? Wait for tenant to complete training + exam     ?
??????????????????????????????????????????????????????????
                        ?
                ? TENANT COMPLETES FLOW ?
                        ?
??????????????????????????????????????????????????????????
?  7. Tenant Completes Training (24 slides)              ?
?     ?? Status: TrainingCompleted                       ?
??????????????????????????????????????????????????????????
                        ?
??????????????????????????????????????????????????????????
?  8. Tenant Takes Examination                           ?
?     ?? TenantTrainingController.SubmitExamination()    ?
??????????????????????????????????????????????????????????
                        ?
            ?????????????????????????
            ?                       ?
      FAIL (<15/15)           PASS (15/15)
            ?                       ?
            ?                       ?
  ???????????????????  ????????????????????????????????
  ? Status:         ?  ?  9. ? Set Status:             ?
  ? Examination     ?  ?     AwaitingInspectionSlots  ?
  ? Failed          ?  ????????????????????????????????
  ?                 ?              ?
  ? Can Retake      ?  ????????????????????????????????
  ? Unlimited       ?  ?  10. ? NOW Call EHCRoundRobin?
  ???????????????????  ?      (from TenantTraining    ?
                       ?       Controller)             ?
                       ????????????????????????????????
                                   ?
                       ????????????????????????????????
                       ?  11. Back Office User        ?
                       ?      Assigned to Schedule    ?
                       ?      Inspection Slots        ?
                       ????????????????????????????????
```

---

## ?? **CHANGES SUMMARY**

| Change | Before | After |
|--------|--------|-------|
| **Status Changes** | 2 calls (duplicate) | 1 call only ? |
| **Final Status** | `AwaitingOnlineTraining` | `AwaitingOnlineTraining` ? |
| **EHCRoundRobin Call** | Called immediately | NOT called ? |
| **Inspection Scheduling** | Too early (before exam) | Happens after exam pass ? |

---

## ?? **WHERE INSPECTION SCHEDULING WILL HAPPEN**

The `EHCRoundRobin()` call for inspection scheduling will be added to:

**File:** `C8.eServices.Mvc\Controllers\TenantTrainingController.cs`  
**Method:** `SubmitExamination()`  
**Location:** After setting status to `AwaitingInspectionScheduleSlots` when exam is passed

```csharp
if (passed)
{
    training.IsExamPassed = true;
    training.ExamPassedDate = DateTime.Now;

    // Update status to awaiting inspection schedule slots
    training.PropertyLeaseApplication.StatusId = db.Status
        .FirstOrDefault(s => s.Key == StatusKeys.AwaitingInspectionScheduleSlots)?.Id 
        ?? training.PropertyLeaseApplication.StatusId;

    // ? NOW call EHCRoundRobin for inspection (TO BE ADDED)
    // See TODO_TRAINING_WORKFLOW_FIXES.md for implementation

    // Send pass email
    SendExamPassEmail(training.PropertyLeaseApplication, score, correctCount, questions.Count);
}
```

---

## ? **IMPLEMENTATION STEPS**

### **Step 1: Open the File**
```
C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs
```

### **Step 2: Find the ClientTraining Method**
Search for:
```csharp
[DecryptParameter]
[HttpPost]
public ActionResult ClientTraining(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel dvm)
```

### **Step 3: Delete Line ~48** (First Status Change)
Remove this line:
```csharp
MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, rcsApps.Id);
```

### **Step 4: Delete Lines ~56-63** (EHCRoundRobin Call)
Remove these lines:
```csharp
EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
```

### **Step 5: Save File**
Press `Ctrl+S`

### **Step 6: Build Solution**
Press `Ctrl+Shift+B`

### **Step 7: Test**
1. Log in as admin
2. Invite tenant to training
3. Check status: Should be `AwaitingOnlineTraining` ?
4. Check Round Robin: Should NOT have inspection task yet ?

---

## ?? **DATABASE VERIFICATION**

After inviting tenant to training, run this query:

```sql
-- Check Status (Should be AwaitingOnlineTraining)
SELECT 
    pla.ApplicationReferenceNumber,
    s.Name AS CurrentStatus,
    s.[Key] AS StatusKey,
    tt.InvitationSentDate,
    tt.IsTrainingCompleted,
    tt.IsExamPassed
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
LEFT JOIN TenantTrainings tt ON pla.Id = tt.PropertyLeaseApplicationId
WHERE pla.ApplicationReferenceNumber = 'EHC2023101800005';

-- Check Round Robin (Should NOT have inspection task yet)
SELECT 
    rrq.Id AS QueueId,
    pla.ApplicationReferenceNumber,
    rt.Name AS ResponsibilityType,
    rt.[Key] AS ResponsibilityKey,
    rrq.IsDone,
    rrq.CurrentTaskDateTime AS AssignedDate
FROM RoundRobinQueues rrq
INNER JOIN PropertyLeaseApplications pla ON rrq.PropertyLeaseApplicationId = pla.Id
INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
WHERE pla.ApplicationReferenceNumber = 'EHC2023101800005'
ORDER BY rrq.CreatedDateTime DESC;
```

**Expected Results:**

| Check | Expected Value | Reason |
|-------|----------------|--------|
| Status | `s_awaiting_online_training` | Correct workflow ? |
| RoundRobin: `rt_invite_to_client_training` | IsDone = 1 | Task finished ? |
| RoundRobin: `rt_schedule_inspection_slots` | **NOT FOUND** | Not created yet ? |

---

## ?? **WHAT SHOULD NOT HAPPEN**

After this fix:
- ? Status should NOT be `s_awaiting_inspection_schedule_slots`
- ? Round Robin should NOT have inspection task
- ? No back-office user should be assigned inspection scheduling
- ? Tenant should NOT see "Schedule Inspection" option

All of these should only happen **AFTER** the tenant passes the exam!

---

## ? **WHAT SHOULD HAPPEN**

After this fix:
- ? Status: `s_awaiting_online_training`
- ? Tenant receives email with training link
- ? Tenant can start training
- ? No inspection scheduling yet
- ? Workflow waits for tenant to complete training + exam

---

**Status:** ?? **READY TO IMPLEMENT**  
**Priority:** ?? **HIGH**  
**Complexity:** ?? **LOW** (Just delete 2 sections)  
**Risk:** ?? **LOW** (Simple removal, no complex logic)  

**Next Step:** Implement this fix, test, then move to adding inspection scheduling in `TenantTrainingController.SubmitExamination()`
