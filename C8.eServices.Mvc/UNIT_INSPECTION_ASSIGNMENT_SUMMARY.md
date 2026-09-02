# Unit Inspection Assignment - User Assignment Logic

## Date: 2026-03-14
## Updated: 2026-03-14 - Changed to use Housing Supervisor (Client Services Supervisor)
## Summary: How users are assigned when scheduling unit inspections

---

## Overview

When a unit inspection is scheduled, the system assigns a **Housing Supervisor (Client Services Supervisor)** based on the **complex/property** where the unit is located.

---

## Assignment Flow

### 1. **When Inspection is Scheduled**

**Location:** `MatchingHelper.cs` → `ScheduleInspectionUnit()` (Line 2382)

```csharp
public static void ScheduleInspectionUnit(eServicesDbContext core, int ScheduleId)
{
    var schedule = core.InspectionSchedules.FirstOrDefault(x => x.Id == ScheduleId);
    schedule.IsApproved = true;
    core.SaveChanges();

    // Assigns user via Round Robin
    cc.EHCRoundRobin((int)schedule.PropertyLeaseApplicationId, 
        false, false, false, true, ... // 4th parameter = UnitInspections
    );
}
```

### 2. **Round Robin Assignment**

**Location:** `PropertyLeaseApplicationController.cs` → `EHCRoundRobin()` (Line 7555-7603)

```csharp
else if (UnitInspections)  // 4th parameter
{
    var RcsApplication = db.PropertyLeaseApplications
        .Include(x => x.Status)
        .FirstOrDefault(x => x.Id == RCSAppID);

    // Get the Housing Supervisor (Client Services Supervisor) for this unit's complex
    var UserId = GetBackOfficeId(db, RCSAppID, false);

    // Fallback to system default if not found
    var StoredUser = Convert.ToInt16(
        db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor)
            .FirstOrDefault().Value
    );

    var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

    var ResponsibilityTypeId = responsibilityTypes
        .Where(x => x.Key == ResponsibilityTypeKeys.Inspections)
        .FirstOrDefault();

    // Create Round Robin Queue entry
    var roundRobinQueue = new RoundRobinQueue
    {
        PropertyLeaseApplicationId = RcsApplication.Id,
        ResponsibilityTypeId = ResponsibilityTypeId.Id,
        CurrentTaskDateTime = DateTime.Now,
        ClerkId = activeDirectoryOn,  // ← ASSIGNED USER
        StatusId = StatusId
    };

    db.RoundRobinQueues.Add(roundRobinQueue);
    db.SaveChanges();

    // Send notification to assigned user
    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
}
```

### 3. **User Lookup Logic**

**Location:** `PropertyLeaseApplicationController.cs` → `GetBackOfficeId()` (Line 7282-7301)

```csharp
public static Customer GetBackOfficeId(eServicesDbContext core, int Id, bool LF)
{
    Customer UserId = new Customer();
    
    // Step 1: Get the ApplicantUnit
    ApplicantUnit AppUnit = core.ApplicantUnits
        .FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
    
    if (AppUnit != null)
    {
        // Step 2: Get the MatchedUnit
        MatchedUnits Match = core.MatchedUnits
            .FirstOrDefault(x => x.Id == AppUnit.MatchedID);
        
        if (Match != null)
        {
            // Step 3: Get the allocated property/unit
            ApplicationAllocatedProperty allocatedUnit = 
                core.ApplicationAllocatedProperty
                    .FirstOrDefault(a => a.Id == Match.ApplicationAllocatedPropertyId);
            
            // Step 4: Get the complex area
            PreferredComplexArea preferredComplexArea = 
                allocatedUnit != null 
                    ? core.PreferredComplexAreas
                        .FirstOrDefault(x => x.Id == allocatedUnit.OfferedComplexId) 
                    : null;

            // Step 5: Get the Housing Supervisor (Client Services Supervisor) assigned to that complex
            if (preferredComplexArea != null)
                UserId = core.Customers
                    .FirstOrDefault(x => x.Id == preferredComplexArea.HousingSuperId);
        }
    }

    return UserId;
}
```

---

## Assignment Logic Summary

### **Primary Assignment:**
1. Find the **unit/property** allocated to the applicant
2. Find the **complex** where that unit is located
3. Assign the **Housing Supervisor (Client Services Supervisor)** who manages that complex

### **Fallback Assignment:**
If no Housing Supervisor is found for the complex:
- Use the **default Housing Supervisor** from `AppSettings` table
- Key: `AppSettingKeys.HousingSupervisor`

---

## Database Tables Involved

### **1. ApplicantUnit**
- Links application to matched unit
- Fields: `PropertyLeaseApplicationId`, `MatchedID`

### **2. MatchedUnits**
- Links to the allocated property
- Fields: `Id`, `ApplicationAllocatedPropertyId`

### **3. ApplicationAllocatedProperty**
- Contains the unit details
- Fields: `Id`, `OfferedComplexId`

### **4. PreferredComplexArea**
- Contains complex details and assigned Housing Supervisor (Client Services Supervisor)
- Fields: `Id`, `HousingSuperId`

### **5. Customer (Housing Supervisor / Client Services Supervisor)**
- Contains the user details
- Fields: `Id`, `SystemUserId`, `FullName`

### **6. RoundRobinQueue**
- Work queue for back office tasks
- Fields: `PropertyLeaseApplicationId`, `ClerkId`, `ResponsibilityTypeId`

### **7. AppSettings**
- System configuration
- Key: `HousingSupervisor` (default user ID for Client Services Supervisor)

---

## Example Scenario

### Scenario: Unit Inspection for Application #5216

**Data Flow:**
```
Application #5216
  ↓
ApplicantUnit (MatchedID = 123)
  ↓
MatchedUnits (ApplicationAllocatedPropertyId = 456)
  ↓
ApplicationAllocatedProperty (OfferedComplexId = 789)
  ↓
PreferredComplexArea (HousingSuperId = 42)
  ↓
Customer #42 (John Smith - Client Services Supervisor)
  ↓
RoundRobinQueue (ClerkId = 42)
```

**Result:** John Smith gets assigned the unit inspection task

---

## Configuration Required

### **1. Complex Setup**
Each `PreferredComplexArea` must have:
- ✅ `HousingSuperId` set to a valid Customer ID
- ✅ Customer must have proper role/permissions

### **2. Fallback Configuration**
In `AppSettings` table:
```sql
Key: 'u_housing_super_visor'  -- AppSettingKeys.HousingSupervisor
Value: '<DefaultCustomerID>'  -- e.g., '42'
```

### **3. Responsibility Type**
Must exist in `ResponsibilityTypes`:
```sql
Key: ResponsibilityTypeKeys.Inspections
Name: 'Unit Inspections' (or similar)
```

---

## SQL Query to Check Assignment

```sql
-- Check which Housing Supervisor (Client Services Supervisor) would be assigned for an application
SELECT 
    pla.Id AS ApplicationId,
    pla.ApplicationReferenceNumber,
    au.MatchedID,
    mu.ApplicationAllocatedPropertyId,
    aap.OfferedComplexId,
    pca.Name AS ComplexName,
    pca.HousingSuperId,
    c.FullName AS HousingSupervisorName
FROM PropertyLeaseApplications pla
LEFT JOIN ApplicantUnits au ON au.PropertyLeaseApplicationId = pla.Id
LEFT JOIN MatchedUnits mu ON mu.Id = au.MatchedID
LEFT JOIN ApplicationAllocatedProperty aap ON aap.Id = mu.ApplicationAllocatedPropertyId
LEFT JOIN PreferredComplexAreas pca ON pca.Id = aap.OfferedComplexId
LEFT JOIN Customers c ON c.Id = pca.HousingSuperId
WHERE pla.Id = 5216  -- Replace with your application ID
```

---

## Key Points

✅ **Assignment is based on COMPLEX/PROPERTY location**, not application details  
✅ **Housing Supervisor (Client Services Supervisor) is pre-configured per complex** in `PreferredComplexArea` table  
✅ **Fallback mechanism** uses system default from `AppSettings` (Key: `u_housing_super_visor`)  
✅ **Notification is sent** to assigned user via `BackOfficeNotification()`  
✅ **Round Robin Queue** entry is created for tracking  

---

## Related Workflows

This same logic (`GetBackOfficeId()`) is also used for:
- **Schedule Inspection Slots** (parameter 10 in EHCRoundRobin)
- **Maintenance Job Sheet** (parameter 11 in EHCRoundRobin)

All use the **same Housing Supervisor (Client Services Supervisor)** based on the complex.

---

## Troubleshooting

### ❌ **Issue: No user gets assigned**

**Check:**
1. Is `PreferredComplexArea.HousingSuperId` set?
2. Is the `HousingSuperId` a valid Customer ID?
3. Is the fallback `AppSettings.HousingSupervisor` (key: `u_housing_super_visor`) configured?

**SQL Fix:**
```sql
-- Set Housing Supervisor (Client Services Supervisor) for a complex
UPDATE PreferredComplexAreas 
SET HousingSuperId = 42  -- Replace with valid Customer ID
WHERE Id = 789  -- Complex ID

-- Set default fallback
UPDATE AppSettings 
SET Value = '42'  -- Replace with valid Customer ID
WHERE [Key] = 'u_housing_super_visor'
```

### ❌ **Issue: Wrong user gets assigned**

**Check:**
1. Verify the complex assignment in `ApplicationAllocatedProperty.OfferedComplexId`
2. Verify the Housing Supervisor in `PreferredComplexArea.HousingSuperId`

---

## Summary

**When a unit inspection is scheduled:**
- System finds the **complex** where the unit is located
- Assigns the **Housing Supervisor (Client Services Supervisor)** who manages that complex
- Creates a **Round Robin Queue** entry
- Sends a **notification** to the assigned user
- User can then conduct the inspection

**The assignment is automatic and based on property location!** 🏢

---

## Implementation Complete ✅

**Updated:** Changed from Letting Officer to Housing Supervisor (Client Services Supervisor)
- Database field: `PreferredComplexArea.HousingSuperId`
- AppSettings key: `u_housing_super_visor`
- All inspection-related tasks now assigned to Housing Supervisor
