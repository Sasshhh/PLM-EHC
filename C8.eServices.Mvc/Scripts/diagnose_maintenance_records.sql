-- =============================================
-- DIAGNOSE MAINTENANCE RECORDS
-- =============================================
-- Purpose: Find ALL maintenance records and signatures (including soft-deleted)
-- to determine why reset script can't find signed records
-- =============================================

PRINT '=========================================='
PRINT 'DIAGNOSTIC REPORT: Maintenance Records'
PRINT '=========================================='
PRINT ''

-- =============================================
-- 1. ALL Maintenance Records (Last 10)
-- =============================================
PRINT '-------------------------------------------'
PRINT '1. ALL Maintenance Records (Last 10):'
PRINT '-------------------------------------------'
SELECT TOP 10
    m.Id AS MaintenanceId,
    m.PropertyLeaseApplicationId,
    pla.ApplicationReferenceNumber,
    m.InspectionType,
    rt.Name AS DefectType,
    rt.[Key] AS DefectTypeKey,
    m.JobCardSubmitted,
    m.UnitMaintenanceCompleted,
    m.JobCardSubmittedDate,
    m.IsDeleted AS MaintenanceDeleted,
    m.CreatedDateTime
FROM AllocatedUnitMaintenanceEHCs m
INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
LEFT JOIN RCSActionTypes rt ON m.RCSActionTypeId = rt.Id
ORDER BY m.Id DESC

PRINT ''

-- =============================================
-- 2. ALL Signatures (Including Soft-Deleted)
-- =============================================
PRINT '-------------------------------------------'
PRINT '2. ALL Signatures (Including Soft-Deleted):'
PRINT '-------------------------------------------'
SELECT 
    sig.Id AS SignatureId,
    sig.AllocatedUnitMaintenanceEHCId,
    m.PropertyLeaseApplicationId,
    pla.ApplicationReferenceNumber,
    sig.OfficialNumber,
    sig.ApprovalAction,
    sig.Reason,
    sig.ApprovalDate,
    sig.SignedByCustomerId,
    sig.IsDeleted AS SignatureDeleted,
    sig.IsActive,
    m.InspectionType
FROM MaintenanceJobCardSignatures sig
INNER JOIN AllocatedUnitMaintenanceEHCs m ON sig.AllocatedUnitMaintenanceEHCId = m.Id
INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
ORDER BY sig.Id DESC

PRINT ''

-- =============================================
-- 3. Maintenance Records WITH Tasks
-- =============================================
PRINT '-------------------------------------------'
PRINT '3. Maintenance Records WITH Tasks:'
PRINT '-------------------------------------------'
SELECT 
    m.Id AS MaintenanceId,
    pla.ApplicationReferenceNumber,
    m.InspectionType,
    COUNT(t.Id) AS TaskCount,
    m.JobCardSubmitted,
    m.UnitMaintenanceCompleted
FROM AllocatedUnitMaintenanceEHCs m
INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
LEFT JOIN MaintenanceJobCardTasks t ON t.AllocatedUnitMaintenanceEHCId = m.Id AND t.IsDeleted = 0
WHERE m.IsDeleted = 0
GROUP BY m.Id, pla.ApplicationReferenceNumber, m.InspectionType, m.JobCardSubmitted, m.UnitMaintenanceCompleted
HAVING COUNT(t.Id) > 0
ORDER BY m.Id DESC

PRINT ''

-- =============================================
-- 4. Find Your Most Recent Application
-- =============================================
PRINT '-------------------------------------------'
PRINT '4. Most Recent Application with Maintenance:'
PRINT '-------------------------------------------'
SELECT TOP 1
    pla.Id AS ApplicationId,
    pla.ApplicationReferenceNumber,
    s.Name AS CurrentStatus,
    s.[Key] AS StatusKey,
    m.Id AS MaintenanceId,
    m.InspectionType,
    rt.Name AS DefectType,
    m.JobCardSubmitted,
    m.UnitMaintenanceCompleted
FROM PropertyLeaseApplications pla
INNER JOIN AllocatedUnitMaintenanceEHCs m ON m.PropertyLeaseApplicationId = pla.Id
LEFT JOIN Status s ON pla.StatusId = s.Id
LEFT JOIN RCSActionTypes rt ON m.RCSActionTypeId = rt.Id
WHERE m.IsDeleted = 0
ORDER BY m.Id DESC

PRINT ''

-- =============================================
-- 5. RoundRobinQueues for Recent Applications
-- =============================================
PRINT '-------------------------------------------'
PRINT '5. Recent RoundRobinQueues:'
PRINT '-------------------------------------------'
SELECT TOP 20
    rrq.Id AS QueueId,
    rrq.PropertyLeaseApplicationId,
    pla.ApplicationReferenceNumber,
    rt.Name AS ResponsibilityType,
    rt.[Key] AS ResponsibilityKey,
    c.FirstName + ' ' + c.LastName AS AssignedTo,
    s.Name AS QueueStatus,
    rrq.IsActive,
    rrq.CreatedDateTime
FROM RoundRobinQueues rrq
INNER JOIN PropertyLeaseApplications pla ON rrq.PropertyLeaseApplicationId = pla.Id
INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
LEFT JOIN Customers c ON rrq.ClerkId = c.Id
LEFT JOIN Status s ON rrq.StatusId = s.Id
WHERE rrq.IsDeleted = 0
ORDER BY rrq.Id DESC

PRINT ''

-- =============================================
-- 6. Inspection Types in Use
-- =============================================
PRINT '-------------------------------------------'
PRINT '6. Inspection Types in Use:'
PRINT '-------------------------------------------'
SELECT DISTINCT 
    InspectionType,
    COUNT(*) AS RecordCount
FROM AllocatedUnitMaintenanceEHCs
WHERE IsDeleted = 0
GROUP BY InspectionType
ORDER BY RecordCount DESC

PRINT ''

-- =============================================
-- DECISION HELPER
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'DECISION HELPER:'
PRINT '=========================================='
PRINT ''

-- Check if ANY signatures exist
IF EXISTS (SELECT 1 FROM MaintenanceJobCardSignatures WHERE IsDeleted = 0)
BEGIN
    PRINT '✅ ACTIVE signatures found!'
    PRINT '   → Use standard reset script'
    PRINT ''
    
    -- Show which one to reset
    SELECT TOP 1
        'RESET THIS ONE:' AS Recommendation,
        sig.Id AS SignatureId,
        sig.AllocatedUnitMaintenanceEHCId AS MaintenanceId,
        m.PropertyLeaseApplicationId AS ApplicationId,
        pla.ApplicationReferenceNumber
    FROM MaintenanceJobCardSignatures sig
    INNER JOIN AllocatedUnitMaintenanceEHCs m ON sig.AllocatedUnitMaintenanceEHCId = m.Id
    INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
    WHERE sig.IsDeleted = 0
    ORDER BY sig.ApprovalDate DESC
END
ELSE IF EXISTS (SELECT 1 FROM MaintenanceJobCardSignatures WHERE IsDeleted = 1)
BEGIN
    PRINT '⚠️  SOFT-DELETED signatures found!'
    PRINT '   → Signature was already reset'
    PRINT '   → You can re-sign the job card now'
    PRINT ''
    
    -- Show which maintenance record to use
    SELECT TOP 1
        'RE-SIGN THIS ONE:' AS Recommendation,
        m.Id AS MaintenanceId,
        m.PropertyLeaseApplicationId AS ApplicationId,
        pla.ApplicationReferenceNumber,
        m.InspectionType,
        m.JobCardSubmitted,
        m.UnitMaintenanceCompleted
    FROM MaintenanceJobCardSignatures sig
    INNER JOIN AllocatedUnitMaintenanceEHCs m ON sig.AllocatedUnitMaintenanceEHCId = m.Id
    INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
    WHERE sig.IsDeleted = 1
    ORDER BY sig.ApprovalDate DESC
END
ELSE
BEGIN
    PRINT '❌ NO signatures found at all!'
    PRINT '   → No maintenance job cards have been signed yet'
    PRINT '   → Sign a job card first to test workflow'
    PRINT ''
    
    -- Show available maintenance records to sign
    SELECT TOP 5
        'AVAILABLE TO SIGN:' AS Recommendation,
        m.Id AS MaintenanceId,
        m.PropertyLeaseApplicationId AS ApplicationId,
        pla.ApplicationReferenceNumber,
        m.InspectionType,
        rt.Name AS DefectType,
        COUNT(t.Id) AS TaskCount
    FROM AllocatedUnitMaintenanceEHCs m
    INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
    LEFT JOIN RCSActionTypes rt ON m.RCSActionTypeId = rt.Id
    LEFT JOIN MaintenanceJobCardTasks t ON t.AllocatedUnitMaintenanceEHCId = m.Id AND t.IsDeleted = 0
    WHERE m.IsDeleted = 0
      AND m.JobCardSubmitted = 0
    GROUP BY m.Id, m.PropertyLeaseApplicationId, pla.ApplicationReferenceNumber, m.InspectionType, rt.Name
    ORDER BY m.Id DESC
END

PRINT ''
PRINT '=========================================='
PRINT 'NEXT STEPS:'
PRINT '=========================================='
PRINT ''

-- Check maintenance record state
DECLARE @RecentMaintenanceId INT
DECLARE @IsSubmitted BIT
DECLARE @AppRef NVARCHAR(50)

SELECT TOP 1
    @RecentMaintenanceId = m.Id,
    @IsSubmitted = m.JobCardSubmitted,
    @AppRef = pla.ApplicationReferenceNumber
FROM AllocatedUnitMaintenanceEHCs m
INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
WHERE m.IsDeleted = 0
ORDER BY m.Id DESC

IF @IsSubmitted = 0
BEGIN
    PRINT '1. Navigate to: /PropertyLeaseApplication/MaintenanceJobSheet/' + CAST(@RecentMaintenanceId AS NVARCHAR(10))
    PRINT '2. Add maintenance tasks'
    PRINT '3. Upload before/after images'
    PRINT '4. Sign the job card'
    PRINT '5. Verify workflow routes to UC013'
END
ELSE IF @IsSubmitted = 1
BEGIN
    PRINT '✅ Job card already submitted!'
    PRINT ''
    PRINT 'To retest:'
    PRINT '1. Run the MANUAL RESET below'
    PRINT '2. Refresh MaintenanceJobSheet page'
    PRINT '3. Re-sign the job card'
    PRINT '4. Verify workflow routing'
END

PRINT ''

-- =============================================
-- MANUAL RESET OPTION
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'MANUAL RESET (If Needed):'
PRINT '=========================================='
PRINT ''
PRINT '-- Copy and run this if signature already exists:'
PRINT '/*'
PRINT 'BEGIN TRANSACTION'
PRINT ''
PRINT '-- Soft delete signature'
PRINT 'UPDATE MaintenanceJobCardSignatures'
PRINT 'SET IsDeleted = 1, IsActive = 0, ModifiedDateTime = GETDATE()'
PRINT 'WHERE AllocatedUnitMaintenanceEHCId = ' + CAST(ISNULL(@RecentMaintenanceId, 0) AS NVARCHAR(10))
PRINT '  AND IsDeleted = 0'
PRINT ''
PRINT '-- Reset maintenance flags'
PRINT 'UPDATE AllocatedUnitMaintenanceEHCs'
PRINT 'SET JobCardSubmitted = 0, UnitMaintenanceCompleted = 0,'
PRINT '    JobCardSubmittedDate = NULL, ModifiedDateTime = GETDATE()'
PRINT 'WHERE Id = ' + CAST(ISNULL(@RecentMaintenanceId, 0) AS NVARCHAR(10))
PRINT ''
PRINT '-- Verify'
PRINT 'SELECT * FROM MaintenanceJobCardSignatures WHERE AllocatedUnitMaintenanceEHCId = ' + CAST(ISNULL(@RecentMaintenanceId, 0) AS NVARCHAR(10))
PRINT 'SELECT JobCardSubmitted, UnitMaintenanceCompleted FROM AllocatedUnitMaintenanceEHCs WHERE Id = ' + CAST(ISNULL(@RecentMaintenanceId, 0) AS NVARCHAR(10))
PRINT ''
PRINT 'COMMIT TRANSACTION'
PRINT '*/'
PRINT ''

PRINT '=========================================='
PRINT 'Diagnostic Complete'
PRINT '=========================================='
