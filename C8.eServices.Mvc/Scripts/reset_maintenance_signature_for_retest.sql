-- =============================================
-- RESET MAINTENANCE SIGNATURE FOR RE-TESTING
-- =============================================
-- Purpose: Clear signature to retest UC012 → UC013 workflow
-- Date: 2026-03-26
-- 
-- This script allows you to re-sign the job card and verify:
-- ✅ Signature saves correctly
-- ✅ RoundRobinQueue created for PropertyFacilitiesManagerReview
-- ✅ Notification sent to Facilities Manager
-- ✅ Application status remains unchanged (minor defects)
-- ✅ Maintenance Job Sheet queue marked finished
-- =============================================

-- STEP 1: Find your maintenance record
PRINT '=========================================='
PRINT 'STEP 1: Finding Maintenance Record...'
PRINT '=========================================='

DECLARE @MaintenanceId INT
DECLARE @ApplicationId INT
DECLARE @ApplicationRefNo NVARCHAR(50)
DECLARE @DefectType NVARCHAR(100)

-- Find most recent maintenance record with signature
SELECT TOP 1
    @MaintenanceId = m.Id,
    @ApplicationId = m.PropertyLeaseApplicationId,
    @ApplicationRefNo = pla.ApplicationReferenceNumber,
    @DefectType = rt.Name
FROM AllocatedUnitMaintenanceEHCs m
INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
INNER JOIN RCSActionTypes rt ON m.RCSActionTypeId = rt.Id
INNER JOIN MaintenanceJobCardSignatures sig ON sig.AllocatedUnitMaintenanceEHCId = m.Id
WHERE sig.IsDeleted = 0
  AND m.InspectionType = 'PreUnitInspection'
ORDER BY sig.ApprovalDate DESC

IF @MaintenanceId IS NULL
BEGIN
    PRINT '❌ ERROR: No signed maintenance records found!'
    PRINT 'Cannot proceed with reset.'
    RETURN
END

PRINT '✅ Found Maintenance Record:'
PRINT '   MaintenanceId: ' + CAST(@MaintenanceId AS NVARCHAR(10))
PRINT '   ApplicationId: ' + CAST(@ApplicationId AS NVARCHAR(10))
PRINT '   Application Ref: ' + @ApplicationRefNo
PRINT '   Defect Type: ' + @DefectType
PRINT ''

-- STEP 2: Show current state
PRINT '=========================================='
PRINT 'STEP 2: Current State Before Reset...'
PRINT '=========================================='

-- Signature details
SELECT 
    'Signature Record' AS RecordType,
    Id AS RecordId,
    OfficialNumber,
    ApprovalAction,
    Reason,
    ApprovalDate,
    SignedByCustomerId
FROM MaintenanceJobCardSignatures
WHERE AllocatedUnitMaintenanceEHCId = @MaintenanceId
  AND IsDeleted = 0

-- Maintenance record flags
SELECT 
    'Maintenance Flags' AS RecordType,
    JobCardSubmitted,
    UnitMaintenanceCompleted,
    JobCardSubmittedDate
FROM AllocatedUnitMaintenanceEHCs
WHERE Id = @MaintenanceId

-- Round Robin Queues
SELECT 
    'Round Robin Queues' AS RecordType,
    rrq.Id AS QueueId,
    rt.Name AS ResponsibilityType,
    rt.[Key] AS ResponsibilityKey,
    c.FirstName + ' ' + c.LastName AS AssignedTo,
    rrq.IsActive,
    rrq.CreatedDateTime
FROM RoundRobinQueues rrq
INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
LEFT JOIN Customers c ON rrq.ClerkId = c.Id
WHERE rrq.PropertyLeaseApplicationId = @ApplicationId
  AND rrq.IsDeleted = 0
ORDER BY rrq.Id DESC

-- Application status
SELECT 
    'Application Status' AS RecordType,
    s.Name AS CurrentStatus,
    s.[Key] AS StatusKey
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.Id = @ApplicationId

PRINT ''

-- STEP 3: Confirm reset
PRINT '=========================================='
PRINT 'STEP 3: Ready to Reset...'
PRINT '=========================================='
PRINT 'This will:'
PRINT '  ✓ Delete signature record (soft delete)'
PRINT '  ✓ Reset JobCardSubmitted = 0'
PRINT '  ✓ Reset UnitMaintenanceCompleted = 0'
PRINT '  ✓ Clear JobCardSubmittedDate'
PRINT '  ✓ KEEP RoundRobinQueue for Facilities Manager (to verify no duplicate)'
PRINT '  ✓ KEEP application status (to verify it doesn''t change again)'
PRINT ''
PRINT '⚠️  IMPORTANT: Run this ONLY if you want to re-sign the job card!'
PRINT ''
PRINT 'To proceed, uncomment the EXECUTE section below and re-run.'
PRINT ''

-- STEP 4: Execute Reset (COMMENTED OUT FOR SAFETY)
-- =============================================
-- UNCOMMENT THIS SECTION TO EXECUTE RESET
-- =============================================
/*
BEGIN TRANSACTION

BEGIN TRY
    PRINT '=========================================='
    PRINT 'STEP 4: Executing Reset...'
    PRINT '=========================================='
    
    -- 4.1 Soft delete signature
    UPDATE MaintenanceJobCardSignatures
    SET IsDeleted = 1,
        IsActive = 0,
        ModifiedDateTime = GETDATE()
    WHERE AllocatedUnitMaintenanceEHCId = @MaintenanceId
      AND IsDeleted = 0
    
    PRINT '✅ Signature soft-deleted'
    
    -- 4.2 Reset maintenance flags
    UPDATE AllocatedUnitMaintenanceEHCs
    SET JobCardSubmitted = 0,
        UnitMaintenanceCompleted = 0,
        JobCardSubmittedDate = NULL,
        ModifiedDateTime = GETDATE()
    WHERE Id = @MaintenanceId
    
    PRINT '✅ Maintenance flags reset'
    
    -- 4.3 Optionally remove PropertyFacilitiesManagerReview queue
    -- UNCOMMENT if you want to test queue creation from scratch
    /*
    DELETE FROM RoundRobinQueues
    WHERE PropertyLeaseApplicationId = @ApplicationId
      AND ResponsibilityTypeId = (
          SELECT Id FROM ResponsibilityTypes 
          WHERE [Key] = 'PropertyFacilitiesManagerReview'
      )
      AND IsDeleted = 0

    PRINT '✅ PropertyFacilitiesManagerReview queue deleted'
    */
    
    COMMIT TRANSACTION
    
    PRINT ''
    PRINT '=========================================='
    PRINT '✅ RESET COMPLETE!'
    PRINT '=========================================='
    PRINT 'You can now:'
    PRINT '  1. Refresh the MaintenanceJobSheet page'
    PRINT '  2. Re-sign the job card'
    PRINT '  3. Verify workflow routing to UC013'
    PRINT ''
    PRINT 'Expected Results After Re-Signing:'
    PRINT '  ✅ New signature record created'
    PRINT '  ✅ JobCardSubmitted = 1'
    PRINT '  ✅ UnitMaintenanceCompleted = 1'
    PRINT '  ✅ NEW RoundRobinQueue for PropertyFacilitiesManagerReview'
    PRINT '  ✅ Notification sent to Facilities Manager'
    PRINT '  ✅ Application status UNCHANGED (if minor defects)'
    PRINT '  ✅ Maintenance Job Sheet queue marked finished'
    PRINT ''
    
    -- Show final state
    PRINT 'Final State:'
    SELECT 
        'After Reset' AS Stage,
        JobCardSubmitted,
        UnitMaintenanceCompleted,
        JobCardSubmittedDate
    FROM AllocatedUnitMaintenanceEHCs
    WHERE Id = @MaintenanceId
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    
    PRINT ''
    PRINT '=========================================='
    PRINT '❌ ERROR OCCURRED!'
    PRINT '=========================================='
    PRINT ERROR_MESSAGE()
    PRINT ''
    PRINT 'Transaction rolled back - no changes made.'
END CATCH
*/

-- =============================================
-- VERIFICATION QUERIES
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'VERIFICATION QUERIES (Run After Reset):'
PRINT '=========================================='
PRINT ''
PRINT '-- 1. Check signature status:'
PRINT 'SELECT * FROM MaintenanceJobCardSignatures WHERE AllocatedUnitMaintenanceEHCId = ' + CAST(@MaintenanceId AS NVARCHAR(10))
PRINT ''
PRINT '-- 2. Check maintenance flags:'
PRINT 'SELECT JobCardSubmitted, UnitMaintenanceCompleted, JobCardSubmittedDate FROM AllocatedUnitMaintenanceEHCs WHERE Id = ' + CAST(@MaintenanceId AS NVARCHAR(10))
PRINT ''
PRINT '-- 3. Check RoundRobinQueues:'
PRINT 'SELECT rrq.*, rt.Name, rt.[Key] FROM RoundRobinQueues rrq'
PRINT 'INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id'
PRINT 'WHERE rrq.PropertyLeaseApplicationId = ' + CAST(@ApplicationId AS NVARCHAR(10))
PRINT 'ORDER BY rrq.Id DESC'
PRINT ''
PRINT '-- 4. Check application status:'
PRINT 'SELECT pla.ApplicationReferenceNumber, s.Name, s.[Key] FROM PropertyLeaseApplications pla'
PRINT 'INNER JOIN Status s ON pla.StatusId = s.Id'
PRINT 'WHERE pla.Id = ' + CAST(@ApplicationId AS NVARCHAR(10))
PRINT ''

-- =============================================
-- ALTERNATIVE: RESET SPECIFIC APPLICATION
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'ALTERNATIVE: Reset Specific Application'
PRINT '=========================================='
PRINT 'If you want to reset a DIFFERENT application, use:'
PRINT ''
PRINT '-- Example for specific application reference:'
PRINT '/*'
PRINT 'DECLARE @AppRef NVARCHAR(50) = ''EHC2026020600001'' -- Change this!'
PRINT ''
PRINT 'UPDATE MaintenanceJobCardSignatures'
PRINT 'SET IsDeleted = 1, IsActive = 0, ModifiedDateTime = GETDATE()'
PRINT 'WHERE AllocatedUnitMaintenanceEHCId IN ('
PRINT '    SELECT Id FROM AllocatedUnitMaintenanceEHCs'
PRINT '    WHERE PropertyLeaseApplicationId = ('
PRINT '        SELECT Id FROM PropertyLeaseApplications'
PRINT '        WHERE ApplicationReferenceNumber = @AppRef'
PRINT '    )'
PRINT ')'
PRINT ''
PRINT 'UPDATE AllocatedUnitMaintenanceEHCs'
PRINT 'SET JobCardSubmitted = 0, UnitMaintenanceCompleted = 0,'
PRINT '    JobCardSubmittedDate = NULL, ModifiedDateTime = GETDATE()'
PRINT 'WHERE PropertyLeaseApplicationId = ('
PRINT '    SELECT Id FROM PropertyLeaseApplications'
PRINT '    WHERE ApplicationReferenceNumber = @AppRef'
PRINT ')'
PRINT '*/'
PRINT ''

PRINT '=========================================='
PRINT 'Script Complete - Review Output Above'
PRINT '=========================================='
