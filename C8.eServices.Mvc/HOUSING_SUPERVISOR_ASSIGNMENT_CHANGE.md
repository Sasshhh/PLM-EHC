# Unit Inspection Assignment - Changed to Housing Supervisor

## Date: 2026-03-14
## Change Summary: Updated from Letting Officer to Housing Supervisor (Client Services Supervisor)

---

## What Changed

The system now assigns **Housing Supervisor (Client Services Supervisor)** instead of **Letting Officer** for:
- ✅ Unit Inspections
- ✅ Schedule Inspection Slots
- ✅ Maintenance Job Sheets

---

## Code Changes

### 1. **GetBackOfficeId Method**
**File:** `PropertyLeaseApplicationController.cs` (Line ~7282)

**Before:**
```csharp
UserId = core.Customers.FirstOrDefault(x => x.Id == preferredComplexArea.LettingOfficerId)
```

**After:**
```csharp
UserId = core.Customers.FirstOrDefault(x => x.Id == preferredComplexArea.HousingSuperId)
```

---

### 2. **Unit Inspections Assignment**
**File:** `PropertyLeaseApplicationController.cs` (Line ~7555)

**Before:**
```csharp
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.LettingOfficer)
    .FirstOrDefault().Value);
```

**After:**
```csharp
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.HousingSupervisor)
    .FirstOrDefault().Value);
```

---

### 3. **Schedule Inspection Slots Assignment**
**File:** `PropertyLeaseApplicationController.cs` (Line ~7499)

**Before:**
```csharp
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.LettingOfficer)
    .FirstOrDefault().Value);
```

**After:**
```csharp
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.HousingSupervisor)
    .FirstOrDefault().Value);
```

---

### 4. **Maintenance Job Sheet Assignment**
**File:** `PropertyLeaseApplicationController.cs` (Line ~7609)

**Before:**
```csharp
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.LettingOfficer)
    .FirstOrDefault().Value);
```

**After:**
```csharp
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.HousingSupervisor)
    .FirstOrDefault().Value);
```

---

## Database Configuration

### **PreferredComplexArea Table**
Uses existing column: `LettingOfficerId`
```sql
-- Already exists in the table
[LettingOfficerId] INT NULL
```

### **AppSettings Table**
Uses existing key: `u_client_services_officer`
```sql
-- Check current value
SELECT * FROM AppSettings 
WHERE [Key] = 'u_client_services_officer'

-- Update if needed
UPDATE AppSettings 
SET Value = '<CustomerId>'  -- Replace with valid Client Services Officer Customer ID
WHERE [Key] = 'u_client_services_officer'
```

---

## What Was NOT Changed (Safe)

❌ **Database Columns** - No schema changes
- `PreferredComplexArea.HousingSuperId` already exists
- `PreferredComplexArea.LettingOfficerId` still exists (untouched)

❌ **Keys/Constants** - No breaking changes
- `AppSettingKeys.HousingSupervisor` already exists (`u_housing_super_visor`)
- `AppSettingKeys.LettingOfficer` still exists (untouched)

❌ **Database Mappings** - All intact
- Foreign key relationships unchanged
- Entity Framework mappings unchanged

---

## Assignment Logic (Updated)

### **Primary Assignment:**
1. Find the allocated unit for the application
2. Find the complex where the unit is located
3. Get `PreferredComplexArea.LettingOfficerId`
4. Assign that user (Letting Officer for the complex)

### **Fallback Assignment:**
If `LettingOfficerId` is NULL or not found:
- Use default from `AppSettings`
- Key: `u_client_services_officer`
- Value: Customer ID of default Client Services Officer

---

## Testing Checklist

- [ ] **Verify AppSettings** - Ensure `u_client_services_officer` is configured
- [ ] **Verify Complex Setup** - Ensure each complex has `LettingOfficerId` set
- [ ] **Test Unit Inspection** - Verify correct user gets assigned
- [ ] **Test Schedule Slots** - Verify correct user gets assigned
- [ ] **Test Maintenance** - Verify correct user gets assigned
- [ ] **Test Fallback** - Remove `LettingOfficerId` from a complex and verify fallback works

---

## SQL Queries for Verification

### **Check Complex Configuration**
```sql
-- View all complexes and their Letting Officers
SELECT 
    pca.Id,
    pca.Name AS ComplexName,
    pca.LettingOfficerId,
    c.FullName AS LettingOfficerName
FROM PreferredComplexAreas pca
LEFT JOIN Customers c ON c.Id = pca.LettingOfficerId
ORDER BY pca.Name
```

### **Check Fallback Configuration**
```sql
-- View default Client Services Officer setting
SELECT 
    [Key],
    Value AS CustomerID,
    c.FullName AS DefaultClientServicesOfficer
FROM AppSettings
LEFT JOIN Customers c ON c.Id = CAST(Value AS INT)
WHERE [Key] = 'u_client_services_officer'
```

### **Check Inspection Assignment**
```sql
-- View who would be assigned for a specific application
SELECT 
    pla.ApplicationReferenceNumber,
    pca.Name AS ComplexName,
    pca.LettingOfficerId,
    c.FullName AS AssignedTo
FROM PropertyLeaseApplications pla
JOIN ApplicantUnits au ON au.PropertyLeaseApplicationId = pla.Id
JOIN MatchedUnits mu ON mu.Id = au.MatchedID
JOIN ApplicationAllocatedProperty aap ON aap.Id = mu.ApplicationAllocatedPropertyId
JOIN PreferredComplexAreas pca ON pca.Id = aap.OfferedComplexId
LEFT JOIN Customers c ON c.Id = pca.LettingOfficerId
WHERE pla.Id = 5216  -- Replace with your application ID
```

---

## Setup Instructions

### **1. Configure Default Client Services Officer**
```sql
-- Set system-wide default
UPDATE AppSettings 
SET Value = '42'  -- Replace 42 with actual Client Services Officer Customer ID
WHERE [Key] = 'u_client_services_officer'
```

### **2. Assign Letting Officers to Complexes**
```sql
-- Set Letting Officer for each complex
UPDATE PreferredComplexAreas 
SET LettingOfficerId = 42  -- Replace with appropriate Customer ID
WHERE Id = 1  -- Complex ID

-- Do this for all complexes
```

### **3. Verify User Has Correct Role**
```sql
-- Ensure user has appropriate role
SELECT 
    c.Id,
    c.FullName,
    su.UserName,
    r.Name AS RoleName
FROM Customers c
JOIN SystemUsers su ON su.Id = c.SystemUserId
JOIN AspNetUserRoles aur ON aur.UserId = su.UserId
JOIN AspNetRoles r ON r.Id = aur.RoleId
WHERE c.Id = 42  -- Customer ID
```

---

## Display Name Changes (UI Only)

The term **"Housing Supervisor"** can be displayed as **"Client Services Supervisor"** in the UI.

**Note:** This is for DISPLAY ONLY. All database fields, keys, and code still use `HousingSupervisor` internally to maintain compatibility.

Example locations where you might want to update labels:
- Views that display inspection assignments
- Round Robin Queue dashboards
- User role displays
- Complex management pages

---

## Benefits of This Change

✅ **More Accurate Role Assignment** - Housing Supervisors handle inspections  
✅ **Better Work Distribution** - Correct person gets the task  
✅ **No Breaking Changes** - Uses existing database columns  
✅ **Backward Compatible** - Letting Officer columns still exist  
✅ **Flexible** - Can switch back if needed  

---

## Migration Path (If Needed)

If complexes currently use `LettingOfficerId` and you want to copy to `HousingSuperId`:

```sql
-- Copy Letting Officer IDs to Housing Supervisor IDs
UPDATE PreferredComplexAreas 
SET HousingSuperId = LettingOfficerId
WHERE HousingSuperId IS NULL 
  AND LettingOfficerId IS NOT NULL
```

---

## Alternative Fallback (If Needed)

To use a different fallback setting:

```csharp
// In EHCRoundRobin sections, change fallback from ClientServicesOfficer to another key:
var StoredUser = Convert.ToInt16(db.AppSettings
    .Where(x => x.Key == AppSettingKeys.LettingOfficer)  // Or HousingSupervisor
    .FirstOrDefault().Value);
```

---

## Files Modified

| File | Changes |
|------|---------|
| `PropertyLeaseApplicationController.cs` | Updated GetBackOfficeId() and 3 EHCRoundRobin sections |
| `UNIT_INSPECTION_ASSIGNMENT_SUMMARY.md` | Updated documentation |

---

## Summary

✅ **Change Complete**
- Unit inspections now assigned to Housing Supervisor
- Uses `PreferredComplexArea.HousingSuperId`
- Fallback uses `AppSettings.u_housing_super_visor`
- No database schema changes required
- All code compiles successfully
- Backward compatible

**Next Steps:**
1. Configure `AppSettings.u_housing_super_visor`
2. Set `HousingSuperId` for each complex
3. Test inspection assignments
4. Update UI labels to "Client Services Supervisor" if desired

---

## Implementation Date: 2026-03-14 ✅
