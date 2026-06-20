-- =============================================
-- Check and Create Schedule Inspection Slots RRQ for 5218
-- Application should have ScheduleInspectionSlots round robin, not UnitInspections!
-- =============================================

USE CRMPLMDEV_2025
GO

PRINT '=== WORKFLOW UNDERSTANDING CHECK ==='
PRINT ''
PRINT 'Correct Workflow:'
PRINT '1. Status: Schedule Unit Inspection'
PRINT '2. RRQ Responsibility: ScheduleInspectionSlots (Housing Supervisor creates time slots)'
PRINT '3. Applicant picks a time slot'
PRINT '4. RRQ Responsibility: UnitInspections (Housing Supervisor conducts inspection)'
PRINT ''

-- Check current state
PRINT '--- CURRENT STATE FOR APPLICATION 5218 ---'
SELECT 
    pla.Id,
    pla.ApplicationReferenceNumber,
    s.Name AS [Status],
    s.[Key] AS [Status Key]
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.Id = 5218

PRINT ''

-- Check existing round robin queues
PRINT '--- EXISTING ROUND ROBIN QUEUES ---'
SELECT 
    rrq.Id,
    rt.Name AS [Responsibility],
    rt.[Key] AS [Responsibility Key],
    c.Id AS [Clerk ID],
    su.FirstName + ' ' + su.LastName AS [Assigned To],
    r.Name AS [Role],
    s.Name AS [Status]
FROM RoundRobinQueues rrq
INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
INNER JOIN Customers c ON rrq.ClerkId = c.Id
INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
INNER JOIN AspNetUsers anu ON su.AspNetUserId = anu.Id
INNER JOIN AspNetUserRoles anur ON anu.Id = anur.UserId
INNER JOIN AspNetRoles r ON anur.RoleId = r.Id
INNER JOIN Status s ON rrq.StatusId = s.Id
WHERE rrq.PropertyLeaseApplicationId = 5218
  AND rrq.EndTaskDateTime IS NULL
ORDER BY rrq.Id DESC

PRINT ''
PRINT '=== FIXING MISSING ROUND ROBIN QUEUE ==='
PRINT ''

-- Get required IDs
DECLARE @AppId INT = 5218
DECLARE @ScheduleSlotsRespId INT
DECLARE @SubmittedStatusId INT
DECLARE @HousingSupervisorId INT

-- Get ScheduleInspectionSlots responsibility type
SELECT @ScheduleSlotsRespId = Id 
FROM ResponsibilityTypes 
WHERE [Key] = 'schedule_inspection_slots'

-- Get Submitted status
SELECT @SubmittedStatusId = Id
FROM Status
WHERE [Key] = 's_submitted'

-- Get Housing Supervisor for Airport Park (Complex ID = 1)
-- First try to get from ApplicantUnits -> MatchedUnits -> ApplicationAllocatedProperty -> PreferredComplexArea
SELECT TOP 1 @HousingSupervisorId = pca.HousingSuperId
FROM ApplicantUnits au
INNER JOIN MatchedUnits mu ON au.MatchedID = mu.Id
INNER JOIN ApplicationAllocatedProperty aap ON mu.ApplicationAllocatedPropertyId = aap.Id
INNER JOIN PreferredComplexAreas pca ON aap.OfferedComplexId = pca.Id
WHERE au.PropertyLeaseApplicationId = @AppId
  AND au.IsActive = 1
  AND au.IsDeleted = 0
  AND mu.IsAccepted = 1

-- If not found, get from application's preferred complex
IF @HousingSupervisorId IS NULL
BEGIN
    SELECT TOP 1 @HousingSupervisorId = pca.HousingSuperId
    FROM PropertyLeaseApplications pla
    INNER JOIN PreferredComplexAreas pca ON pla.PreferredComplexAreaId = pca.Id
    WHERE pla.Id = @AppId
END

-- Fallback to AppSettings default
IF @HousingSupervisorId IS NULL
BEGIN
    SELECT @HousingSupervisorId = CAST([Value] AS INT)
    FROM AppSettings
    WHERE [Key] = 'housing_supervisor'
END

PRINT 'Retrieved IDs:'
PRINT '  Application ID: ' + CAST(@AppId AS VARCHAR(10))
PRINT '  Schedule Slots Responsibility ID: ' + ISNULL(CAST(@ScheduleSlotsRespId AS VARCHAR(10)), 'NULL')
PRINT '  Submitted Status ID: ' + ISNULL(CAST(@SubmittedStatusId AS VARCHAR(10)), 'NULL')
PRINT '  Housing Supervisor ID: ' + ISNULL(CAST(@HousingSupervisorId AS VARCHAR(10)), 'NULL')
PRINT ''

-- Check if ScheduleInspectionSlots RRQ already exists
IF EXISTS (
    SELECT 1 
    FROM RoundRobinQueues 
    WHERE PropertyLeaseApplicationId = @AppId
      AND ResponsibilityTypeId = @ScheduleSlotsRespId
      AND EndTaskDateTime IS NULL
)
BEGIN
    PRINT '✅ ScheduleInspectionSlots RoundRobinQueue already exists - no action needed'
END
ELSE
BEGIN
    PRINT '⚠️ ScheduleInspectionSlots RoundRobinQueue MISSING - creating now...'
    
    -- Create the missing round robin queue entry
    INSERT INTO RoundRobinQueues (
        PropertyLeaseApplicationId,
        ResponsibilityTypeId,
        ClerkId,
        StatusId,
        CurrentTaskDateTime,
        CreatedDateTime,
        IsActive,
        IsDeleted,
        IsLocked
    )
    VALUES (
        @AppId,
        @ScheduleSlotsRespId,
        @HousingSupervisorId,
        @SubmittedStatusId,
        GETDATE(),
        GETDATE(),
        1,
        0,
        0
    )
    
    PRINT '✅ Created ScheduleInspectionSlots RoundRobinQueue'
    PRINT '   Assigned to Housing Supervisor ID: ' + CAST(@HousingSupervisorId AS VARCHAR(10))
    
    -- Add activity tracker entry
    INSERT INTO PLMApplicationHistortyLogs (
        PropertyLeaseApplicationId,
        AuditAction,
        UserId,
        CreatedDateTime,
        IsActive,
        IsDeleted,
        IsLocked,
        CreatedBySystemUserId
    )
    VALUES (
        @AppId,
        'Created missing ScheduleInspectionSlots round robin queue for Housing Supervisor after rollback',
        @HousingSupervisorId,
        GETDATE(),
        1,
        0,
        0,
        @HousingSupervisorId
    )
END

PRINT ''
PRINT '=== FINAL VERIFICATION ==='
PRINT ''

-- Show final state
SELECT 
    rrq.Id AS [RRQ ID],
    pla.ApplicationReferenceNumber,
    rt.Name AS [Responsibility],
    rt.[Key] AS [Responsibility Key],
    c.Id AS [Clerk ID],
    su.FirstName + ' ' + su.LastName AS [Assigned To],
    r.Name AS [Role],
    s.Name AS [Status],
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
WHERE rrq.PropertyLeaseApplicationId = 5218
  AND rrq.EndTaskDateTime IS NULL
ORDER BY rrq.Id DESC

PRINT ''
PRINT '=== NEXT STEPS ==='
PRINT 'Housing Supervisor should now see this application in their queue at:'
PRINT 'http://localhost:3450/PropertyLeaseApplication/PropertyLeaseInspections'
PRINT ''
PRINT 'They should click on it to SCHEDULE available time slots for the applicant to pick.'
PRINT ''

GO
