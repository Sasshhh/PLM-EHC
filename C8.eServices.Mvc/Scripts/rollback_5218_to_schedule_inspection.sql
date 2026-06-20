-- =============================================
-- Rollback Application 5218 to Schedule Unit Inspection
-- This script reverses all changes made after unit inspection was scheduled
-- =============================================

USE CRMPLMDEV_2025
GO

DECLARE @AppId INT = 5218
DECLARE @AppRef NVARCHAR(50) = 'EHC2026032600001'

PRINT '=== Starting Rollback for Application ' + @AppRef + ' ==='
PRINT ''

-- Get current state
PRINT '--- CURRENT STATE ---'
SELECT 
    'Application' AS [Entity],
    s.Name AS [Current Status],
    pla.Id AS [App ID]
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.Id = @AppId

SELECT 
    'RoundRobinQueue' AS [Entity],
    COUNT(*) AS [Count],
    STRING_AGG(CAST(Id AS NVARCHAR(10)), ', ') AS [Queue IDs]
FROM RoundRobinQueues
WHERE PropertyLeaseApplicationId = @AppId

SELECT 
    'InspectionSchedules' AS [Entity],
    COUNT(*) AS [Count],
    STRING_AGG(CAST(Id AS NVARCHAR(10)), ', ') AS [Schedule IDs]
FROM InspectionSchedules
WHERE PropertyLeaseApplicationId = @AppId

SELECT 
    'ScheduledInspections' AS [Entity],
    COUNT(*) AS [Count],
    STRING_AGG(CAST(Id AS NVARCHAR(10)), ', ') AS [Scheduled IDs]
FROM ScheduledInspections
WHERE PropertyLeaseApplicationId = @AppId

PRINT ''

-- Begin Transaction
BEGIN TRANSACTION

BEGIN TRY
    
    -- Step 1: Delete ScheduledInspections
    PRINT '--- STEP 1: Deleting ScheduledInspections ---'
    
    DELETE FROM ScheduledInspections
    WHERE PropertyLeaseApplicationId = @AppId
    
    PRINT 'Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' ScheduledInspections'
    PRINT ''
    
    -- Step 2: Delete InspectionSchedules
    PRINT '--- STEP 2: Deleting InspectionSchedules ---'
    
    DELETE FROM InspectionSchedules
    WHERE PropertyLeaseApplicationId = @AppId
    
    PRINT 'Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' InspectionSchedules'
    PRINT ''
    
    -- Step 3: Delete RoundRobinQueue entries for "Conduct Unit Inspection"
    PRINT '--- STEP 3: Deleting RoundRobinQueue entries ---'
    
    -- Get the ResponsibilityType ID for "Conduct Unit Inspection"
    DECLARE @ConductInspectionRespId INT
    SELECT @ConductInspectionRespId = Id 
    FROM ResponsibilityTypes 
    WHERE [Key] = 'conduct_unit_inspection'
    
    DELETE FROM RoundRobinQueues
    WHERE PropertyLeaseApplicationId = @AppId
      AND ResponsibilityTypeId = @ConductInspectionRespId
    
    PRINT 'Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' RoundRobinQueue entries for Conduct Unit Inspection'
    PRINT ''
    
    -- Step 4: Reset Application Status to "Schedule Unit Inspection"
    PRINT '--- STEP 4: Resetting Application Status ---'
    
    DECLARE @ScheduleInspectionStatusId INT
    SELECT @ScheduleInspectionStatusId = Id 
    FROM Status 
    WHERE [Key] = 's_schedule_unit_inspection'
    
    UPDATE PropertyLeaseApplications
    SET StatusId = @ScheduleInspectionStatusId,
        ModifiedDateTime = GETDATE()
    WHERE Id = @AppId
    
    PRINT 'Updated application status to: Schedule Unit Inspection'
    PRINT ''
    
    -- Step 5: Add Activity Tracker Log
    PRINT '--- STEP 5: Adding Activity Tracker Log ---'
    
    DECLARE @CustomerId INT
    SELECT @CustomerId = CustomerId 
    FROM PropertyLeaseApplications 
    WHERE Id = @AppId
    
    INSERT INTO PLMApplicationHistortyLogs 
    (
        PropertyLeaseApplicationId,
        AuditAction,
        UserId,
        CreatedDateTime,
        IsActive,
        IsDeleted,
        IsLocked,
        CreatedBySystemUserId
    )
    VALUES
    (
        @AppId,
        'Application rolled back to Schedule Unit Inspection status for Housing Supervisor fix',
        @CustomerId,
        GETDATE(),
        1,
        0,
        0,
        @CustomerId
    )
    
    PRINT 'Added activity tracker log'
    PRINT ''
    
    -- Verify Final State
    PRINT '=== VERIFICATION - FINAL STATE ==='
    PRINT ''
    
    SELECT 
        'Application' AS [Entity],
        s.Name AS [Status After Rollback],
        pla.Id AS [App ID]
    FROM PropertyLeaseApplications pla
    INNER JOIN Status s ON pla.StatusId = s.Id
    WHERE pla.Id = @AppId
    
    SELECT 
        'RoundRobinQueue' AS [Entity],
        COUNT(*) AS [Remaining Count]
    FROM RoundRobinQueues
    WHERE PropertyLeaseApplicationId = @AppId
    
    SELECT 
        'InspectionSchedules' AS [Entity],
        COUNT(*) AS [Remaining Count]
    FROM InspectionSchedules
    WHERE PropertyLeaseApplicationId = @AppId
    
    SELECT 
        'ScheduledInspections' AS [Entity],
        COUNT(*) AS [Remaining Count]
    FROM ScheduledInspections
    WHERE PropertyLeaseApplicationId = @AppId
    
    PRINT ''
    PRINT '=== ROLLBACK SUCCESSFUL - Ready to re-test with Housing Supervisor fix ==='
    
    -- Commit Transaction
    COMMIT TRANSACTION
    
END TRY
BEGIN CATCH
    
    -- Rollback on error
    ROLLBACK TRANSACTION
    
    PRINT ''
    PRINT '!!! ERROR OCCURRED - TRANSACTION ROLLED BACK !!!'
    PRINT 'Error Message: ' + ERROR_MESSAGE()
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10))
    
END CATCH

GO
