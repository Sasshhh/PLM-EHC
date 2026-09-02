# Unit Inspection Assignment Logic

## Date: 2026-03-14
## Summary: Dual Assignment Strategy for Scheduling vs Conducting Inspections

---

## Assignment Strategy Overview

The system now supports **two different assignment methods** depending on the task:

| Task | Method | Primary Assignment | Fallback |
|------|--------|-------------------|----------|
| **Schedule Inspection Slots** | `GetHousingSupervisorId()` | `PreferredComplexArea.HousingSuperId` | `AppSettings.u_housing_super_visor` |
| **Conduct Unit Inspection** | `GetBackOfficeId()` | `PreferredComplexArea.LettingOfficerId` | `AppSettings.u_letting_officer` |
| **Maintenance Job Sheet** | `GetBackOfficeId()` | `PreferredComplexArea.LettingOfficerId` | `AppSettings.u_letting_officer` |

---

## Method Details

### **GetBackOfficeId()**
**File:** `PropertyLeaseApplicationController.cs` (Line ~7282)
**Purpose:** Get the **Letting Officer** assigned to the complex

**Assignment Flow:**
```
1. Find ApplicantUnit for application
2. Find MatchedUnit
3. Find ApplicationAllocatedProperty
4. Find PreferredComplexArea
5. Return Customer based on LettingOfficerId
```

**Returns:** Customer with `LettingOfficerId` from the complex

**Usage:**
- Conduct Unit Inspection (UnitInspections)
- Maintenance Job Sheet (MaintananceJobSheet)
- Most other inspection-related tasks

---

### **GetHousingSupervisorId()** (NEW)
**File:** `PropertyLeaseApplicationController.cs` (Line ~7302)
**Purpose:** Get the **Housing Supervisor** assigned to the complex

**Assignment Flow:**
```
1. Find ApplicantUnit for application
2. Find MatchedUnit
3. Find ApplicationAllocatedProperty
4. Find PreferredComplexArea
5. Return Customer based on HousingSuperId (if not null)
```

**Returns:** Customer with `HousingSuperId` from the complex, or empty Customer if null

**Usage:**
- Schedule Inspection Slots (ShechuleInspectionSlots)

---

## EHCRoundRobin Implementation

### **ShechuleInspectionSlots** (Line ~7499)
**Who:** Housing Supervisor (Client Services Supervisor)

```csharp
var UserId = GetHousingSupervisorId(db, RCSAppID);
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.HousingSupervisor)
    .FirstOrDefault().Value);
var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;
```

**Assignment Logic:**
1. **Primary:** Use `HousingSuperId` from the complex where unit is located
2. **Fallback:** Use default from `AppSettings.u_housing_super_visor`

**Role:** Housing Supervisor schedules available time slots for inspections

---

### **UnitInspections** (Line ~7555)
**Who:** Letting Officer

```csharp
var UserId = GetBackOfficeId(db, RCSAppID, false);
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.LettingOfficer)
    .FirstOrDefault().Value);
var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;
```

**Assignment Logic:**
1. **Primary:** Use `LettingOfficerId` from the complex where unit is located
2. **Fallback:** Use default from `AppSettings.u_letting_officer`

**Role:** Letting Officer conducts the actual unit inspection

---

### **MaintananceJobSheet** (Line ~7628)
**Who:** Letting Officer

```csharp
var UserId = GetBackOfficeId(db, RCSAppID, false);
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.LettingOfficer)
    .FirstOrDefault().Value);
var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;
```

**Assignment Logic:**
1. **Primary:** Use `LettingOfficerId` from the complex where unit is located
2. **Fallback:** Use default from `AppSettings.u_letting_officer`

**Role:** Letting Officer handles maintenance job sheets

---

## Database Configuration

### **PreferredComplexArea Table**

Each complex has two officer assignments:

| Column | Purpose | Used By |
|--------|---------|---------|
| `HousingSuperId` | Housing Supervisor for scheduling | `GetHousingSupervisorId()` |
| `LettingOfficerId` | Letting Officer for inspections | `GetBackOfficeId()` |

```sql
-- View complex configuration
SELECT 
    pca.Id,
    pca.Name AS ComplexName,
    pca.HousingSuperId,
    hs.FullName AS HousingSupervisorName,
    pca.LettingOfficerId,
    lo.FullName AS LettingOfficerName
FROM PreferredComplexAreas pca
LEFT JOIN Customers hs ON hs.Id = pca.HousingSuperId
LEFT JOIN Customers lo ON lo.Id = pca.LettingOfficerId
ORDER BY pca.Name
```

---

### **AppSettings Table**

System-wide fallback configurations:

| Key | Purpose | Used When |
|-----|---------|-----------|
| `u_housing_super_visor` | Default Housing Supervisor | Complex has no `HousingSuperId` |
| `u_letting_officer` | Default Letting Officer | Complex has no `LettingOfficerId` |

```sql
-- View fallback configuration
SELECT 
    [Key],
    Value AS CustomerID,
    c.FullName AS DefaultOfficer
FROM AppSettings
LEFT JOIN Customers c ON c.Id = CAST(Value AS INT)
WHERE [Key] IN ('u_housing_super_visor', 'u_letting_officer')
```

---

## Workflow Example

### **Scenario: Unit Inspection for Application 5216**

**Step 1: Schedule Inspection Slots**
- Housing Supervisor creates available time slots
- Assignment via `GetHousingSupervisorId()`
- Uses `PreferredComplexArea.HousingSuperId`
- Round Robin Queue: `ResponsibilityTypeKeys.ScheduleInspectionSlots`

**Step 2: Applicant Selects Time Slot**
- Applicant chooses from available slots
- Time slot reserved

**Step 3: Conduct Unit Inspection**
- Letting Officer conducts the inspection
- Assignment via `GetBackOfficeId()`
- Uses `PreferredComplexArea.LettingOfficerId`
- Round Robin Queue: `ResponsibilityTypeKeys.Inspections`

**Step 4: Maintenance (if needed)**
- Letting Officer handles maintenance job sheet
- Assignment via `GetBackOfficeId()`
- Uses `PreferredComplexArea.LettingOfficerId`
- Round Robin Queue: `ResponsibilityTypeKeys.MaintananceJobSheet`

---

## Setup Instructions

### **1. Configure System-Wide Defaults**

```sql
-- Set default Housing Supervisor
UPDATE AppSettings 
SET Value = '42'  -- Replace with Housing Supervisor Customer ID
WHERE [Key] = 'u_housing_super_visor';

-- Set default Letting Officer
UPDATE AppSettings 
SET Value = '43'  -- Replace with Letting Officer Customer ID
WHERE [Key] = 'u_letting_officer';
```

### **2. Configure Complex Assignments**

```sql
-- Set both officers for each complex
UPDATE PreferredComplexAreas 
SET 
    HousingSuperId = 42,     -- Housing Supervisor Customer ID
    LettingOfficerId = 43    -- Letting Officer Customer ID
WHERE Id = 1;  -- Complex ID

-- Repeat for all complexes
```

### **3. Verify User Roles**

```sql
-- Ensure users have appropriate roles
SELECT 
    c.Id,
    c.FullName,
    su.UserName,
    r.Name AS RoleName
FROM Customers c
JOIN SystemUsers su ON su.Id = c.SystemUserId
JOIN AspNetUserRoles aur ON aur.UserId = su.UserId
JOIN AspNetRoles r ON r.Id = aur.RoleId
WHERE c.Id IN (42, 43)  -- Officer Customer IDs
ORDER BY c.Id, r.Name;
```

---

## Testing Checklist

### **Schedule Inspection Slots**
- [ ] Verify `HousingSuperId` is set for each complex
- [ ] Verify `AppSettings.u_housing_super_visor` is configured
- [ ] Test scheduling inspection slots for an application
- [ ] Verify Housing Supervisor receives Round Robin assignment
- [ ] Test fallback when `HousingSuperId` is NULL

### **Conduct Unit Inspection**
- [ ] Verify `LettingOfficerId` is set for each complex
- [ ] Verify `AppSettings.u_letting_officer` is configured
- [ ] Test conducting unit inspection for an application
- [ ] Verify Letting Officer receives Round Robin assignment
- [ ] Test fallback when `LettingOfficerId` is NULL

### **Maintenance Job Sheet**
- [ ] Test creating maintenance job sheet
- [ ] Verify Letting Officer receives Round Robin assignment
- [ ] Test fallback when `LettingOfficerId` is NULL

---

## SQL Verification Queries

### **Check Inspection Assignment for Application**

```sql
-- See who would be assigned for scheduling
DECLARE @ApplicationId INT = 5216;

SELECT 
    'Scheduling' AS TaskType,
    pla.ApplicationReferenceNumber,
    pca.Name AS ComplexName,
    pca.HousingSuperId AS AssignedOfficerId,
    hs.FullName AS AssignedOfficerName,
    'Housing Supervisor' AS Role
FROM PropertyLeaseApplications pla
JOIN ApplicantUnits au ON au.PropertyLeaseApplicationId = pla.Id
JOIN MatchedUnits mu ON mu.Id = au.MatchedID
JOIN ApplicationAllocatedProperty aap ON aap.Id = mu.ApplicationAllocatedPropertyId
JOIN PreferredComplexAreas pca ON pca.Id = aap.OfferedComplexId
LEFT JOIN Customers hs ON hs.Id = pca.HousingSuperId
WHERE pla.Id = @ApplicationId

UNION ALL

-- See who would be assigned for inspection
SELECT 
    'Inspection' AS TaskType,
    pla.ApplicationReferenceNumber,
    pca.Name AS ComplexName,
    pca.LettingOfficerId AS AssignedOfficerId,
    lo.FullName AS AssignedOfficerName,
    'Letting Officer' AS Role
FROM PropertyLeaseApplications pla
JOIN ApplicantUnits au ON au.PropertyLeaseApplicationId = pla.Id
JOIN MatchedUnits mu ON mu.Id = au.MatchedID
JOIN ApplicationAllocatedProperty aap ON aap.Id = mu.ApplicationAllocatedPropertyId
JOIN PreferredComplexAreas pca ON pca.Id = aap.OfferedComplexId
LEFT JOIN Customers lo ON lo.Id = pca.LettingOfficerId
WHERE pla.Id = @ApplicationId;
```

### **Check Round Robin Queue Assignments**

```sql
-- View recent inspection-related assignments
SELECT 
    rrq.Id,
    rrq.PropertyLeaseApplicationId,
    pla.ApplicationReferenceNumber,
    rt.Name AS ResponsibilityType,
    c.FullName AS AssignedTo,
    s.Name AS Status,
    rrq.CurrentTaskDateTime
FROM RoundRobinQueues rrq
JOIN PropertyLeaseApplications pla ON pla.Id = rrq.PropertyLeaseApplicationId
JOIN ResponsibilityTypes rt ON rt.Id = rrq.ResponsibilityTypeId
JOIN Customers c ON c.Id = rrq.ClerkId
JOIN [Status] s ON s.Id = rrq.StatusId
WHERE rt.[Key] IN ('schedule_inspection_slots', 'inspections', 'maintanance_job_sheet')
ORDER BY rrq.CurrentTaskDateTime DESC;
```

---

## Benefits of Dual Assignment Strategy

✅ **Role Separation** - Different officers handle scheduling vs conducting inspections  
✅ **Workload Distribution** - Tasks distributed appropriately by role  
✅ **Per-Complex Flexibility** - Each complex can have different officers assigned  
✅ **System-Wide Fallback** - Ensures work is always assigned even if complex setup incomplete  
✅ **Clear Responsibility** - Each officer knows their specific tasks  
✅ **Scalable** - Easy to add new complexes and assign appropriate officers  

---

## Common Issues & Solutions

### **Issue: No one assigned to schedule inspection slots**
**Solution:** 
1. Check `PreferredComplexArea.HousingSuperId` is populated
2. Check `AppSettings.u_housing_super_visor` is configured
3. Verify the Customer exists and is not deleted

### **Issue: No one assigned to conduct inspection**
**Solution:**
1. Check `PreferredComplexArea.LettingOfficerId` is populated
2. Check `AppSettings.u_letting_officer` is configured
3. Verify the Customer exists and is not deleted

### **Issue: Wrong person assigned**
**Solution:**
1. Verify the complex configuration is correct
2. Check the application's allocated unit belongs to the correct complex
3. Review Round Robin Queue for manual reassignment

---

## Files Modified

| File | Changes |
|------|---------|
| `PropertyLeaseApplicationController.cs` | Added `GetHousingSupervisorId()` method |
| `PropertyLeaseApplicationController.cs` | Updated `ShechuleInspectionSlots` to use Housing Supervisor |
| `PropertyLeaseApplicationController.cs` | Updated `UnitInspections` to use Letting Officer |
| `PropertyLeaseApplicationController.cs` | Updated `MaintananceJobSheet` to use Letting Officer |
| `INSPECTION_ASSIGNMENT_LOGIC.md` | Created documentation |

---

## Summary

✅ **Implementation Complete**
- Two assignment methods: `GetBackOfficeId()` and `GetHousingSupervisorId()`
- Schedule inspection slots → Housing Supervisor
- Conduct inspections → Letting Officer
- Maintenance job sheets → Letting Officer
- Per-complex assignment with system-wide fallback
- All code compiles successfully

**Next Steps:**
1. Configure `AppSettings.u_housing_super_visor`
2. Configure `AppSettings.u_letting_officer`
3. Set both `HousingSuperId` and `LettingOfficerId` for each complex
4. Test scheduling and inspection assignment
5. Verify Round Robin Queue assignments

---

## Implementation Date: 2026-03-14 ✅
