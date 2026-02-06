# ?? **CRITICAL FIX NEEDED** ??

## Issue Found in `PropertyLeaseApplicationController.cs`

### **Current Code (WRONG):**
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
        var rcsApps = cxt.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
        MeetingRequest meeting = dvm.MeetingRequest;
        meeting.PropertyLeaseApplicationId = (int)id;
        meeting.CustomerId = userID;
        cxt.MeetingRequests.Add(meeting);
        cxt.SaveChanges();

        int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InviteTenantForTraining).Id;
        EmailHelper.CustomerEmailNotification(cxt, rcsApps.Id, emailboodyId);
        
        // ? WRONG STATUS! This sets status to "Awaiting Inspection Schedule Slots"
        MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, rcsApps.Id);

        var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer).FirstOrDefault().Value);
        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault();
        MatchingHelper.RoundRobinMarkJobAsFinished(cxt, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);

        EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
        Session["ClientTrainingInviteSession"] = string.Format($"Tenant has been invited successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
        return RedirectToAction("PropertyLeaseTenantTraining");
    }
}
```

---

### **FIXED Code (CORRECT):**
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
        var rcsApps = cxt.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
        MeetingRequest meeting = dvm.MeetingRequest;
        meeting.PropertyLeaseApplicationId = (int)id;
        meeting.CustomerId = userID;
        cxt.MeetingRequests.Add(meeting);
        cxt.SaveChanges();

        int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InviteTenantForTraining).Id;
        EmailHelper.CustomerEmailNotification(cxt, rcsApps.Id, emailboodyId);
        
        // ? FIXED! Now sets status to "Awaiting Online Training"
        MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingOnlineTraining).Id, rcsApps.Id);

        var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer).FirstOrDefault().Value);
        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault();
        MatchingHelper.RoundRobinMarkJobAsFinished(cxt, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);

        EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
        Session["ClientTrainingInviteSession"] = string.Format($"Tenant has been invited successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
        return RedirectToAction("PropertyLeaseTenantTraining");
    }
}
```

---

## **What Changed:**
**Line to change:** ~Line 1885 in `PropertyLeaseApplicationController.cs`

**FROM:**
```csharp
MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, rcsApps.Id);
```

**TO:**
```csharp
MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingOnlineTraining).Id, rcsApps.Id);
```

---

## **Why This Matters:**

The old code was setting the status to **"AwaitingInspectionScheduleSlots"** instead of **"AwaitingOnlineTraining"**, which means:

? **Old behavior:**
- Training invitation sent
- Status changes to "Awaiting Inspection Schedule Slots" (WRONG!)
- Your training system never got triggered

? **New behavior:**
- Training invitation sent
- Status changes to "Awaiting Online Training" (CORRECT!)
- Tenant receives email with training link
- Training flow works as expected

---

## **How to Apply the Fix:**

1. Open `PropertyLeaseApplicationController.cs`
2. Find line ~1885 (search for "ClientTraining" method)
3. Change `StatusKeys.AwaitingInspectionScheduleSlots` to `StatusKeys.AwaitingOnlineTraining`
4. Save the file
5. Rebuild the project

---

## **Status Flow (After Fix):**

```
Assessment Fee Payment Approved
    ? [Admin: Invite to Training]
    ?
? Awaiting Online Training (NEW!)
    ? [Tenant: Clicks email link]
    ?
? Training In Progress
    ? [Tenant: Completes training]
    ?
? Training Completed
    ? [Tenant: Takes exam]
    ?
? Examination Passed OR Examination Failed
```

---

**File:** `PropertyLeaseApplicationController.cs`  
**Line:** ~1885  
**Priority:** ?? **CRITICAL** - Must be fixed for training system to work!
