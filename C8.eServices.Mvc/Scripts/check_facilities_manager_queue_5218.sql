-- =============================================
-- CHECK FACILITIES MANAGER QUEUE FOR APPLICATION 5218
-- =============================================

USE [eServicesDb]
GO

PRINT '=========================================='
PRINT 'UC013 FACILITIES MANAGER QUEUE CHECK'
PRINT '=========================================='
PRINT ''
PRINT 'Application: EHC2026032600001 (ID: 5218)'
PRINT 'Maintenance: ID 2008'
PRINT 'Logged in as: COESolarDev05 (ID: 1287)'
PRINT ''

-- =============================================
-- 1. Check if signature exists (UC012 completed?)
-- =============================================
PRINT '-------------------------------------------'
PRINT '1. Has Maintenance Manager signed job card?'
PRINT '-------------------------------------------'

IF EXISTS (SELECT 1 FROM MaintenanceJobCardSignatures WHERE AllocatedUnitMaintenanceEHCId = 2008 AND IsDeleted = 0)
BEGIN
    SELECT 
        '✅ YES - Job card signed' AS Status,
        sig.Id AS SignatureId,
        sig.OfficialNumber,
        sig.ApprovalAction,
        sig.ApprovalDate,
        c.FirstName + ' ' + c.LastName AS SignedBy
    FROM MaintenanceJobCardSignatures sig
    INNER JOIN Customers c ON sig.SignedByCustomerId = c.Id
    WHERE sig.AllocatedUnitMaintenanceEHCId = 2008
      AND sig.IsDeleted = 0
END
ELSE
BEGIN
    PRINT '❌ NO - Job card NOT signed yet'
    PRINT ''
    PRINT 'ACTION REQUIRED:'
    PRINT '1. Navigate to: http://localhost:3450/PropertyLeaseApplication/MaintenanceJobSheet/2008'
    PRINT '2. Click "Submit Job Card for Sign-Off"'
    PRINT '3. Enter official number, approve, reason, signature'
    PRINT '4. Click "Submit Signature"'
    PRINT ''
    PRINT 'After signing, this queue will appear automatically.'
END

PRINT ''

-- =============================================
-- 2. Check if PropertyFacilitiesManagerReview queue exists
-- =============================================
PRINT '-------------------------------------------'
PRINT '2. Does Facilities Manager Review queue exist?'
PRINT '-------------------------------------------'

IF EXISTS (
    SELECT 1 
    FROM RoundRobinQueues rrq
    INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    WHERE rrq.PropertyLeaseApplicationId = 5218
      AND rt.[Key] = 'r_property_facilities_manager_review'
      AND rrq.IsDeleted = 0
)
BEGIN
    SELECT 
        '✅ YES - Queue exists' AS Status,
        rrq.Id AS QueueId,
        rt.Name AS QueueType,
        rrq.ClerkId AS AssignedToUserId,
        c.FirstName + ' ' + c.LastName AS AssignedToName,
        s.Name AS QueueStatus,
        rrq.IsActive,
        rrq.CreatedDateTime
    FROM RoundRobinQueues rrq
    INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    LEFT JOIN Customers c ON rrq.ClerkId = c.Id
    LEFT JOIN Status s ON rrq.StatusId = s.Id
    WHERE rrq.PropertyLeaseApplicationId = 5218
      AND rt.[Key] = 'r_property_facilities_manager_review'
      AND rrq.IsDeleted = 0
    
    -- Check if assigned to COESolarDev05
    IF EXISTS (
        SELECT 1 FROM RoundRobinQueues rrq
        INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
        WHERE rrq.PropertyLeaseApplicationId = 5218
          AND rt.[Key] = 'r_property_facilities_manager_review'
          AND rrq.ClerkId = 1287
          AND rrq.IsDeleted = 0
    )
    BEGIN
        PRINT ''
        PRINT '✅ Queue is assigned to YOU (COESolarDev05)'
        PRINT '   You should see this in PropertyFacilitiesMaintenanceReviews'
    END
    ELSE
    BEGIN
        PRINT ''
        PRINT '⚠️  Queue exists but NOT assigned to you!'
        PRINT '   Check assigned user above'
    END
END
ELSE
BEGIN
    PRINT '❌ NO - Queue does NOT exist yet'
    PRINT ''
    PRINT 'This queue will be created automatically when:'
    PRINT '1. Maintenance Manager signs the job card (UC012)'
    PRINT '2. SaveMaintenanceSignature runs workflow routing'
    PRINT '3. PropertyFacilitiesManagerReview queue gets created'
    PRINT ''
    PRINT 'Sign the job card first, then check again.'
END

PRINT ''

-- =============================================
-- 3. Show ALL PropertyFacilitiesManagerReview queues for COESolarDev05
-- =============================================
PRINT '-------------------------------------------'
PRINT '3. ALL queues assigned to COESolarDev05:'
PRINT '-------------------------------------------'

IF EXISTS (
    SELECT 1 
    FROM RoundRobinQueues rrq
    INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    WHERE rrq.ClerkId = 1287
      AND rt.[Key] = 'r_property_facilities_manager_review'
      AND rrq.IsDeleted = 0
)
BEGIN
    SELECT 
        rrq.Id AS QueueId,
        pla.ApplicationReferenceNumber,
        m.Id AS MaintenanceId,
        m.InspectionType,
        rt2.Name AS DefectType,
        s.Name AS ApplicationStatus,
        rrq.CreatedDateTime AS QueueCreated
    FROM RoundRobinQueues rrq
    INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    INNER JOIN PropertyLeaseApplications pla ON rrq.PropertyLeaseApplicationId = pla.Id
    INNER JOIN AllocatedUnitMaintenanceEHCs m ON m.PropertyLeaseApplicationId = pla.Id
    LEFT JOIN Status s ON pla.StatusId = s.Id
    LEFT JOIN RCSActionTypes rt2 ON m.RCSActionTypeId = rt2.Id
    WHERE rrq.ClerkId = 1287
      AND rt.[Key] = 'r_property_facilities_manager_review'
      AND rrq.IsDeleted = 0
      AND m.JobCardSubmitted = 1
    ORDER BY rrq.CreatedDateTime DESC
    
    PRINT ''
    PRINT '✅ These should appear in PropertyFacilitiesMaintenanceReviews'
END
ELSE
BEGIN
    PRINT '❌ NO queues assigned to COESolarDev05'
    PRINT ''
    PRINT 'This is normal if no maintenance job cards have been signed yet.'
    PRINT 'After signing job card 2008, you will see 1 queue here.'
END

PRINT ''

-- =============================================
-- 4. Check maintenance record state
-- =============================================
PRINT '-------------------------------------------'
PRINT '4. Maintenance Record 2008 State:'
PRINT '-------------------------------------------'

SELECT 
    m.Id AS MaintenanceId,
    pla.ApplicationReferenceNumber,
    m.InspectionType,
    rt.Name AS DefectType,
    m.JobCardSubmitted,
    m.UnitMaintenanceCompleted,
    m.JobCardSubmittedDate,
    CASE 
        WHEN EXISTS (SELECT 1 FROM MaintenanceJobCardSignatures WHERE AllocatedUnitMaintenanceEHCId = m.Id AND IsDeleted = 0)
        THEN '✅ Signed'
        ELSE '❌ Not Signed'
    END AS SignatureStatus
FROM AllocatedUnitMaintenanceEHCs m
INNER JOIN PropertyLeaseApplications pla ON m.PropertyLeaseApplicationId = pla.Id
LEFT JOIN RCSActionTypes rt ON m.RCSActionTypeId = rt.Id
WHERE m.Id = 2008

PRINT ''

-- =============================================
-- SUMMARY
-- =============================================
PRINT '=========================================='
PRINT 'SUMMARY:'
PRINT '=========================================='
PRINT ''

DECLARE @IsSigned BIT = 0
DECLARE @QueueExists BIT = 0

IF EXISTS (SELECT 1 FROM MaintenanceJobCardSignatures WHERE AllocatedUnitMaintenanceEHCId = 2008 AND IsDeleted = 0)
    SET @IsSigned = 1

IF EXISTS (
    SELECT 1 FROM RoundRobinQueues rrq
    INNER JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    WHERE rrq.PropertyLeaseApplicationId = 5218
      AND rt.[Key] = 'r_property_facilities_manager_review'
      AND rrq.IsDeleted = 0
)
    SET @QueueExists = 1

IF @IsSigned = 1 AND @QueueExists = 1
BEGIN
    PRINT '✅ WORKFLOW IS WORKING!'
    PRINT ''
    PRINT 'Maintenance job card is signed and queue exists.'
    PRINT ''
    PRINT 'WHY IS SCREEN EMPTY?'
    PRINT 'Check PropertyFacilitiesMaintenanceReviews controller method.'
    PRINT 'It might be filtering by IsActive or other criteria.'
    PRINT ''
END
ELSE IF @IsSigned = 0
BEGIN
    PRINT '❌ JOB CARD NOT SIGNED YET'
    PRINT ''
    PRINT 'NEXT STEP:'
    PRINT '1. Navigate to: http://localhost:3450/PropertyLeaseApplication/MaintenanceJobSheet/2008'
    PRINT '2. Sign the job card (UC012)'
    PRINT '3. Refresh PropertyFacilitiesMaintenanceReviews page'
    PRINT '4. Queue should appear automatically'
    PRINT ''
END
ELSE IF @IsSigned = 1 AND @QueueExists = 0
BEGIN
    PRINT '⚠️  WORKFLOW ROUTING FAILED!'
    PRINT ''
    PRINT 'Job card is signed but queue was NOT created.'
    PRINT 'SaveMaintenanceSignature method may have errors.'
    PRINT ''
    PRINT 'Check application logs or run workflow routing manually.'
    PRINT ''
END

PRINT '=========================================='
PRINT 'Diagnostic Complete'
PRINT '=========================================='
