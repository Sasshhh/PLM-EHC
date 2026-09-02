# Maintenance Job Card Enhancement - Implementation Summary

## Overview
Implementing UC012 - Manage Job Card with task capture, image uploads, and signature functionality for the Maintenance Manager role.

## Changes Made

### 1. Database Models Created ✅

#### MaintenanceJobCardTask.cs
- Stores individual maintenance tasks with:
  - Start/End dates
  - Activity description
  - Materials, Quantity, Labour
  - Total costs
  - Task comments
  - Supporting document reference

#### MaintenanceJobCardSignature.cs
- Stores Maintenance Manager signature
- Official number, approval action, reason
  - Signature data (base64 image)
  - Approval date

#### AllocatedUnitMaintenance.cs - Updated
- Added BeforeImageId (Document FK)
- Added AfterImageId (Document FK)
- Added JobCardSubmitted (bool)
- Added JobCardSubmittedDate (DateTime)
- Added Inspection (bool)
- Added navigation properties for JobCardTasks and JobCardSignatures

### 2. Migration Created ✅
- `202603140900000_AddMaintenanceJobCardEnhancements.cs`
- Creates MaintenanceJobCardTasks table
- Creates MaintenanceJobCardSignatures table
- Adds new columns to AllocatedUnitMaintenanceEHCs

### 3. DbContext Updated ✅
- Added `DbSet<MaintenanceJobCardTask>`
- Added `DbSet<MaintenanceJobCardSignature>`

## Pending Work

### 4. Controller Updates 🔄
- [ ] Add action methods to save tasks
- [ ] Add action methods to upload before/after images
- [ ] Add action methods to save signature
- [ ] Update MaintenanceJobSheet POST to handle new workflow
- [ ] Add document download action for new inspection form

### 5. View Updates 🔄
- [ ] Add Task Capture section with repeatable form
- [ ] Add Before/After Image upload section
- [ ] Add Signature capture section  
- [ ] Add download button for Pre-inspection Form v2.pdf
- [ ] Update form submission logic

### 6. Document Setup 🔄
- [ ] Add Pre-inspection Form v2.pdf to Content folder
- [ ] Create document download action

## Workflow

```
Step 1: View Maintenance Job Sheets list
Step 2: Click "Capture Job Card"
Step 3: Add Tasks (repeatable):
        - Start/End dates
        - Activity, Materials, Labour
        - Costs, Comments
        - Upload supporting document
Step 4: Upload Before/After Images
Step 5: Submit for Sign-Off
Step 6: Enter Official Number
Step 7: Capture Signature
Step 8: Submit → Notify Property & Facilities Manager
```

## Files Created
- `Models/MaintenanceJobCardTask.cs`
- `Models/MaintenanceJobCardSignature.cs`
- `Migrations/202603140900000_AddMaintenanceJobCardEnhancements.cs`

## Files Modified
- `Models/AllocatedUnitMaintenance.cs`
- `DataAccessLayer/eServicesDbContext.cs`

## Next Steps
1. Update MaintenanceJobSheet controller actions
2. Update MaintenanceJobSheet view with task capture UI
3. Add signature pad functionality
4. Test complete workflow
5. Run migration on dev database
