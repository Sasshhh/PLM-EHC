-- =============================================
-- Verify Housing Supervisor Assignment Fix
-- Run AFTER scheduling inspection for application 5218
-- =============================================

USE CRMPLMDEV_2025
GO

PRINT '=== Housing Supervisor Assignment Verification ==='
PRINT ''

DECLARE @AppId INT = 5218
DECLARE @AppRef NVARCHAR(50) = 'EHC2026032600001'

-- Check Application Status
PRINT '--- APPLICATION STATUS ---'
SELECT 
    pla.Id AS [App ID],
    pla.ApplicationReferenceNumber AS [Reference],
    s.Name AS [Current Status],
    s.[Key] AS [Status Key]
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.Id = @AppId

PRINT ''

-- Check Round Robin Assignment
PRINT '--- ROUND ROBIN ASSIGNMENT FOR CONDUCT UNIT INSPECTION ---'
SELECT 
    rrq.Id AS [Queue ID],
    rrq.PropertyLeaseApplicationId AS [App ID],
    pla.ApplicationReferenceNumber AS [Reference],
    rt.Name AS [Responsibility],
    rt.[Key] AS [Responsibility Key],
    c.Id AS [Clerk ID],
    su.FirstName + ' ' + su.LastName AS [Assigned To],
    r.Name AS [User Role],
    CASE 
        WHEN r.Name LIKE '%Housing Supervisor%' THEN '✅ CORRECT'
        WHEN r.Name LIKE '%Letting Officer%' THEN '❌ WRONG - BUG NOT FIXED'
        ELSE '⚠️ UNEXPECTED ROLE'
    END AS [Assignment Status],
    s.Name AS [Queue Status],
    rrq.CurrentTaskDateTime AS [Assigned DateTime]
FROM RoundRobinQueues rrq
INNER JOIN PropertyLeaseApplications pla ON rrq.PropertyLeaseApplicationId = pla.Id
INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
INNER JOIN Customers c ON rrq.ClerkId = c.Id
INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
INNER JOIN AspNetUsers anu ON su.AspNetUserId = anu.Id
INNER JOIN AspNetUserRoles anur ON anu.Id = anur.UserId
INNER JOIN AspNetRoles r ON anur.RoleId = r.Id
INNER JOIN Status s ON rrq.StatusId = s.Id
WHERE pla.Id = @AppId
  AND rt.[Key] = 'conduct_unit_inspection'
ORDER BY rrq.Id DESC

PRINT ''

-- Check Inspection Schedules
PRINT '--- INSPECTION SCHEDULES ---'
SELECT 
    iss.Id AS [Schedule ID],
    iss.PropertyLeaseApplicationId AS [App ID],
    dts.ShecduleDate AS [Scheduled Date],
    ts.Slot AS [Time Slot],
    iss.IsApproved AS [Approved],
    iss.IsInspected AS [Inspected]
FROM InspectionSchedules iss
INNER JOIN DateToSchedules dts ON iss.DateToScheduleId = dts.Id
INNER JOIN TimeSlots ts ON iss.TimeSlotId = ts.Id
WHERE iss.PropertyLeaseApplicationId = @AppId
ORDER BY iss.Id DESC

PRINT ''

-- Check Scheduled Inspections
PRINT '--- SCHEDULED INSPECTIONS ---'
SELECT 
    si.Id AS [Inspection ID],
    si.PropertyLeaseApplicationId AS [App ID],
    dts.ShecduleDate AS [Inspection Date],
    ts.Slot AS [Time Slot],
    c.Id AS [Housing Supervisor ID],
    su.FirstName + ' ' + su.LastName AS [Inspector Name],
    si.IsInspected AS [Completed]
FROM ScheduledInspections si
INNER JOIN DateToSchedules dts ON si.DateToScheduleId = dts.Id
INNER JOIN TimeSlots ts ON si.TimeSlotId = ts.Id
INNER JOIN Customers c ON si.HousingSupervisorId = c.Id
INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
WHERE si.PropertyLeaseApplicationId = @AppId
ORDER BY si.Id DESC

PRINT ''

-- Summary Check
PRINT '=== SUMMARY CHECK ==='
PRINT ''

IF EXISTS (
    SELECT 1 
    FROM RoundRobinQueues rrq
    INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    INNER JOIN Customers c ON rrq.ClerkId = c.Id
    INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
    INNER JOIN AspNetUsers anu ON su.AspNetUserId = anu.Id
    INNER JOIN AspNetUserRoles anur ON anu.Id = anur.UserId
    INNER JOIN AspNetRoles r ON anur.RoleId = r.Id
    WHERE rrq.PropertyLeaseApplicationId = @AppId
      AND rt.[Key] = 'conduct_unit_inspection'
      AND r.Name LIKE '%Housing Supervisor%'
)
BEGIN
    PRINT '✅ SUCCESS: Conduct Unit Inspection assigned to Housing Supervisor'
    PRINT '✅ FIX VERIFIED: Code change working correctly'
END
ELSE IF EXISTS (
    SELECT 1 
    FROM RoundRobinQueues rrq
    INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    INNER JOIN Customers c ON rrq.ClerkId = c.Id
    INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
    INNER JOIN AspNetUsers anu ON su.AspNetUserId = anu.Id
    INNER JOIN AspNetUserRoles anur ON anu.Id = anur.UserId
    INNER JOIN AspNetRoles r ON anur.RoleId = r.Id
    WHERE rrq.PropertyLeaseApplicationId = @AppId
      AND rt.[Key] = 'conduct_unit_inspection'
      AND r.Name LIKE '%Letting Officer%'
)
BEGIN
    PRINT '❌ FAILURE: Conduct Unit Inspection still assigned to Letting Officer'
    PRINT '❌ BUG NOT FIXED: Code change not applied or not working'
    PRINT ''
    PRINT 'TROUBLESHOOTING:'
    PRINT '1. Verify code change was saved in PropertyLeaseApplicationController.cs line 7873'
    PRINT '2. Rebuild solution (Ctrl+Shift+B)'
    PRINT '3. Restart debugging (Shift+F5, then F5)'
    PRINT '4. Re-test inspection scheduling'
END
ELSE
BEGIN
    PRINT '⚠️ WARNING: No RoundRobinQueue entry found for conduct_unit_inspection'
    PRINT ''
    PRINT 'POSSIBLE REASONS:'
    PRINT '1. Inspection not yet scheduled - schedule inspection first'
    PRINT '2. Application rolled back - check application status'
    PRINT '3. Different responsibility type used - check ResponsibilityTypes table'
END

PRINT ''
PRINT '=== END VERIFICATION ==='

GO
