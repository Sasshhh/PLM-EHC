# Maintenance Defect Workflow Implementation Summary

## Overview
This implementation adds a comprehensive maintenance defect workflow with two new roles:
- **Maintenance Manager** (per-complex assignment)
- **Property & Facilities Manager** (system-wide, AppSettings-based)

## Architecture Decision: Role Assignment Strategy

### Maintenance Manager (Per-Complex)
- **Storage**: `PreferredComplexArea.MaintenanceManagerId` (Column 16)
- **Scope**: Each complex/area can have its own Maintenance Manager
- **Fallback**: `AppSettings['u_maintenance_manager']` when complex-specific assignment is null
- **Method**: `GetMaintenanceManagerId(eServicesDbContext core, int Id)`

### Property & Facilities Manager (System-Wide)
- **Storage**: `AppSettings['u_property_facilities_manager']` ONLY
- **Scope**: Single manager for entire system
- **No Complex Assignment**: Removed from `PreferredComplexArea` model
- **Method**: `GetPropertyFacilitiesManagerId(eServicesDbContext core, int Id)`

## Workflow Logic

### Pre-Unit Inspection Flow

1. **Unit Inspection Conducted** → Housing Supervisor schedules, Letting Officer conducts
2. **Maintenance Job Sheet Created** → Maintenance Manager assigned via round robin
3. **Defect Classification**:
   
   **Minor Defects** (`HabitableMinorDefects`):
   - Application process continues automatically
   - Maintenance happens in background
   - No Property & Facilities Manager review required
   - Status remains in normal flow
   
   **Major Defects** (`NotHabitable`):
   - Application HALTS at `CustomerQueryPending` status
   - Property & Facilities Manager work queue item created
   - Notification sent to Property & Facilities Manager
   - **Requires approval before proceeding**

4. **Property & Facilities Manager Review** (Major Defects Only):
   - **Approved**: Returns application to `AwaitingInspectionScheduleSlots`
   - Routes back to Housing Supervisor for re-inspection scheduling
   - **Rejected**: Remains in `CustomerQueryPending`
   - Additional maintenance work required

## Database Schema Changes

### PreferredComplexArea Table
```sql
ALTER TABLE PreferredComplexAreas
ADD MaintenanceManagerId INT NULL,
CONSTRAINT FK_PreferredComplexAreas_MaintenanceManager 
FOREIGN KEY (MaintenanceManagerId) REFERENCES Customers(Id);
```

**Column Order**:
- Column 16: MaintenanceManagerId
- Column 17: RegionTypeId
- Column 18: CCCTypeId

**Removed**: PropertyFacilitiesManagerId (was never added to table)

### AppSettings Table
```sql
INSERT INTO AppSettings ([Key], [Value], [Description])
VALUES 
('u_maintenance_manager', '0', 'System-wide fallback Maintenance Manager'),
('u_property_facilities_manager', '0', 'System-wide Property & Facilities Manager');
```

### ResponsibilityTypes Table
```sql
INSERT INTO ResponsibilityTypes ([Key], [Name], [Description])
VALUES (
'r_property_facilities_manager_review', 
'Property & Facilities Manager Review',
'Reviews major defect maintenance completion before re-inspection'
);
```

## Code Changes

### Models Updated
1. **PreferredComplexArea.cs**
   - Added: `MaintenanceManagerId` (int?, Column 16)
   - Added: Navigation property `Customer MaintenanceManager`
   - Removed: `PropertyFacilitiesManagerId` (never added)

2. **PreferredComplexAreaAudit.cs**
   - Added: `MaintenanceManagerId` (int?, Column 16)
   - Column ordering synchronized with main model

### Keys Added
1. **AppSettingKeys.cs**
   - `MaintenanceManager = "u_maintenance_manager"`
   - `PropertyFacilitiesManager = "u_property_facilities_manager"`

2. **ResponsibilityTypeKeys.cs**
   - `PropertyFacilitiesManagerReview = "r_property_facilities_manager_review"`

### Controller Methods

#### PropertyLeaseApplicationController.cs

**New Methods**:
```csharp
GetMaintenanceManagerId(eServicesDbContext core, int Id)
// Returns per-complex Maintenance Manager with AppSettings fallback

GetPropertyFacilitiesManagerId(eServicesDbContext core, int Id)
// Returns system-wide Property & Facilities Manager from AppSettings ONLY

PropertyFacilitiesManagerReview(int? id) // GET
// Displays maintenance job sheet and defect details for review

PropertyFacilitiesManagerReview(int? id, string ApprovalStatusddl, string ReviewComment) // POST
// Processes approval/rejection decision
```

**Modified Methods**:
```csharp
MaintenanceJobSheet(int? id, string ApprovalStatusddl) // POST
// Lines 2096-2150: Added conditional routing based on defect severity
// Major defects → Property & Facilities Manager
// Minor defects → Continue automatically
```

**Round Robin Assignment**:
```csharp
EHCRoundRobin(..., bool UnitMaintenance, ...)
// Line ~7628: Updated MaintananceJobSheet section
// Uses GetMaintenanceManagerId() and AppSettingKeys.MaintenanceManager
```

### Views Created
1. **PropertyFacilitiesManagerReview.cshtml**
   - Location: `C8.eServices.Mvc\Views\PropertyLeaseApplication\`
   - Displays: Application details, defect classification, maintenance documents
   - Actions: Approve (return to inspection) or Reject (keep pending)
   - Includes: Review comment field, decision dropdown, documentation review

## SQL Scripts

1. **add_maintenance_manager_columns.sql**
   - Adds `MaintenanceManagerId` to `PreferredComplexAreas`
   - Creates foreign key constraint
   - Includes verification query

2. **backfill_maintenance_managers.sql**
   - Template for populating existing complexes
   - Safety condition: `1=0` requires manual Customer ID configuration
   - Includes verification query

3. **create_maintenance_roles_and_users.sql**
   - Creates "Maintenance Manager" and "Property & Facilities Manager" roles
   - Template for creating user accounts
   - Assigns users to roles

4. **add_facilities_manager_responsibility_type.sql**
   - Inserts PropertyFacilitiesManagerReview into ResponsibilityTypes
   - Includes duplicate check
   - Verification query

5. **add_maintenance_manager_appsettings.sql**
   - Adds both AppSettings entries with default value '0'
   - Includes update template for actual Customer IDs
   - Verification query

## Session Management

### Success Messages
- **MaintenanceJobSheetSession** (Minor Defects):
  - "Job sheet approved for application reference {ref}, due to minor defects there are no changes to application process flow."

- **MaintenanceJobSheetSession** (Major Defects):
  - "Job sheet approved for application reference {ref}. Major defects require Property & Facilities Manager review before re-inspection."

- **PropertyFacilitiesManagerReviewSession** (Approved):
  - "Maintenance review approved for application reference {ref}. Application sent back for re-inspection."

- **PropertyFacilitiesManagerReviewSession** (Rejected):
  - "Maintenance review rejected for application reference {ref}. Additional work required."

## Activity Tracker Messages

1. **Major Defect Approval**:
   ```
   "Unit Inspection Approved. Major defects require Property & Facilities Manager review before re-inspection."
   ```

2. **Facilities Manager Approval**:
   ```
   "Property & Facilities Manager approved maintenance completion. Application returned for re-inspection. Comment: {ReviewComment}"
   ```

3. **Facilities Manager Rejection**:
   ```
   "Property & Facilities Manager rejected maintenance completion. Reason: {ReviewComment}"
   ```

## Status Flow

### Minor Defects Path
```
ConductUnitInspection (Defects Found)
→ MaintenanceJobSheet (Maintenance Manager)
→ Approved with HabitableMinorDefects
→ Application continues to next stage
→ Maintenance happens in background
```

### Major Defects Path
```
ConductUnitInspection (Major Defects Found)
→ MaintenanceJobSheet (Maintenance Manager)
→ Approved with NotHabitable
→ Status: CustomerQueryPending
→ PropertyFacilitiesManagerReview (Property & Facilities Manager)
   ↓ Approved
   → Status: AwaitingInspectionScheduleSlots
   → ScheduleInspectionSlots (Housing Supervisor)
   → ConductUnitInspection (Letting Officer - Re-inspection)
   ↓ Rejected
   → Status: CustomerQueryPending (remains)
   → Additional maintenance required
```

## Testing Checklist

- [ ] Verify Maintenance Manager assignment in EHCRoundRobin
- [ ] Test minor defect flow (application continues)
- [ ] Test major defect flow (halts for approval)
- [ ] Verify Property & Facilities Manager receives notification
- [ ] Test PropertyFacilitiesManagerReview approval path
- [ ] Test PropertyFacilitiesManagerReview rejection path
- [ ] Verify re-inspection scheduling after approval
- [ ] Check session messages display correctly
- [ ] Validate activity tracker entries
- [ ] Confirm navigation menu access for new roles

## Pending Work

1. **Navigation Menu Updates** (RCS_Layout.cshtml)
   - Add "Property & Facilities Manager Review" menu item (role-based)
   - Update "Maintenance Manager" menu items
   - Review Housing Supervisor, Letting Officer, Client Services Officer menu visibility

2. **Role Creation in Database**
   - Run `create_maintenance_roles_and_users.sql` to create roles
   - Create actual user accounts for both roles
   - Assign users to roles

3. **AppSettings Configuration**
   - Update `u_maintenance_manager` with actual Customer ID
   - Update `u_property_facilities_manager` with actual Customer ID

4. **Complex Assignment**
   - Run `backfill_maintenance_managers.sql` to assign Maintenance Managers to complexes
   - Update query with actual Customer ID before execution

5. **ResponsibilityType and AppSettings**
   - Run `add_facilities_manager_responsibility_type.sql`
   - Run `add_maintenance_manager_appsettings.sql`

6. **End-to-End Testing**
   - Create test application through full workflow
   - Test both minor and major defect paths
   - Verify round robin assignments
   - Validate email notifications

## Notes

- Property & Facilities Manager is intentionally system-wide, not per-complex
- Maintenance Manager follows same pattern as Housing Supervisor and Letting Officer (per-complex with AppSettings fallback)
- Major defects create a hard stop requiring senior approval
- Minor defects allow workflow to continue with background maintenance
- All audit models synchronized with main models
- Build successful - no compilation errors

## Related Files

**Controllers**:
- `C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs`

**Models**:
- `C8.eServices.Mvc\Models\PreferredComplexArea.cs`
- `C8.eServices.Mvc\Models\Audits\PreferredComplexAreaAudit.cs`

**Views**:
- `C8.eServices.Mvc\Views\PropertyLeaseApplication\PropertyFacilitiesManagerReview.cshtml`
- `C8.eServices.Mvc\Views\PropertyLeaseApplication\MaintenanceJobSheet.cshtml`
- `C8.eServices.Mvc\Views\PropertyLeaseApplication\ConductUnitInspection.cshtml`

**Keys**:
- `C8.eServices.Mvc\Keys\AppSettingKeys.cs`
- `C8.eServices.Mvc\Keys\ResponsibilityTypeKeys.cs`
- `C8.eServices.Mvc\Keys\RCSActionTypeKeys.cs`
- `C8.eServices.Mvc\Keys\StatusKeys.cs`

**Scripts**:
- `C8.eServices.Mvc\Scripts\add_maintenance_manager_columns.sql`
- `C8.eServices.Mvc\Scripts\backfill_maintenance_managers.sql`
- `C8.eServices.Mvc\Scripts\create_maintenance_roles_and_users.sql`
- `C8.eServices.Mvc\Scripts\add_facilities_manager_responsibility_type.sql`
- `C8.eServices.Mvc\Scripts\add_maintenance_manager_appsettings.sql`
