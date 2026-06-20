-- ========================================================================
-- VERIFICATION SCRIPT: Auto-Matching CEO Approval
-- ========================================================================
-- Run these queries to verify the auto-matching functionality works

USE CRMPLMDEV_2025
GO

-- ========================================================================
-- 1. CHECK AVAILABLE UNITS FOR YOUR TEST APPLICATION
-- ========================================================================
PRINT '=== 1. AVAILABLE UNITS MATCHING YOUR TEST APP ==='

DECLARE @TestAppId INT = 5216 -- Change this to your test application ID

-- Get the application's preferences
SELECT 
    'Application Preferences' AS Section,
    PLA.Id AS ApplicationId,
    PLA.ApplicationReferenceNumber,
    PLA.PreferredComplexAreaId AS PreferredComplexId,
    PCA.Name AS PreferredComplexName,
    PLA.HumanEHCOptionsId AS PreferredTypologyId,
    HEHO.Name AS PreferredTypologyName
FROM PropertyLeaseApplications PLA
LEFT JOIN PreferredComplexAreas PCA ON PLA.PreferredComplexAreaId = PCA.Id
LEFT JOIN HumanEHCOptions HEHO ON PLA.HumanEHCOptionsId = HEHO.Id
WHERE PLA.Id = @TestAppId

-- Check if matching units exist
SELECT 
    'Available Matching Units' AS Section,
    AAP.Id AS UnitId,
    AAP.SpaceUnitNumber,
    AAP.OfferedComplexId,
    PCA.Name AS ComplexName,
    AAP.HumanEHCOptionId AS TypologyId,
    HEHO.Name AS TypologyName,
    AAP.IsTaken,
    AAP.IsActive
FROM ApplicationAllocatedProperty AAP
LEFT JOIN PreferredComplexAreas PCA ON AAP.OfferedComplexId = PCA.Id
LEFT JOIN HumanEHCOptions HEHO ON AAP.HumanEHCOptionId = HEHO.Id
WHERE AAP.OfferedComplexId = (SELECT PreferredComplexAreaId FROM PropertyLeaseApplications WHERE Id = @TestAppId)
  AND AAP.HumanEHCOptionId = (SELECT HumanEHCOptionsId FROM PropertyLeaseApplications WHERE Id = @TestAppId)
  AND AAP.IsTaken = 0
  AND AAP.IsActive = 1
ORDER BY AAP.Id

-- ========================================================================
-- 2. CHECK CURRENT APPLICATION STATUS
-- ========================================================================
PRINT ''
PRINT '=== 2. CURRENT APPLICATION STATUS ==='

SELECT 
    PLA.Id AS ApplicationId,
    PLA.ApplicationReferenceNumber,
    S.[Key] AS StatusKey,
    S.Name AS StatusName,
    PLA.CreatedDate,
    PLA.ModifiedDate
FROM PropertyLeaseApplications PLA
LEFT JOIN Status S ON PLA.StatusId = S.Id
WHERE PLA.Id = @TestAppId

-- ========================================================================
-- 3. CHECK RISK ASSESSMENT OUTCOME
-- ========================================================================
PRINT ''
PRINT '=== 3. RISK ASSESSMENT OUTCOME ==='

SELECT 
    RAO.Id,
    RAO.PropertyLeaseApplicationId,
    RAO.CEO_OfficialNumber,
    RAO.CEO_Outcome,
    RAO.CEO_DateStamp,
    RAO.CEO_Reason
FROM RiskAssessmentOutcomes RAO
WHERE RAO.PropertyLeaseApplicationId = @TestAppId
ORDER BY RAO.Id DESC

-- ========================================================================
-- 4. CHECK WAITING LIST STATUS
-- ========================================================================
PRINT ''
PRINT '=== 4. WAITING LIST STATUS ==='

SELECT 
    PLWL.Id,
    PLWL.PropertyLeaseApplicationId,
    PLWL.QueueStatus,
    PLWL.DateAdded,
    PLWL.OfferedUnitId,
    PLWL.IsActive
FROM PropertyLeaseWaitingLists PLWL
WHERE PLWL.PropertyLeaseApplicationId = @TestAppId
ORDER BY PLWL.Id DESC

-- ========================================================================
-- 5. CHECK MATCHED UNITS (IF AUTO-MATCHED)
-- ========================================================================
PRINT ''
PRINT '=== 5. MATCHED UNITS ==='

SELECT 
    MU.Id,
    MU.PropertyLeaseApplicationId,
    MU.ApplicationAllocatedPropertyId AS UnitId,
    AAP.SpaceUnitNumber,
    MU.IsAccepted,
    MU.RejectedProperty
FROM MatchedUnits MU
LEFT JOIN ApplicationAllocatedProperty AAP ON MU.ApplicationAllocatedPropertyId = AAP.Id
WHERE MU.PropertyLeaseApplicationId = @TestAppId
ORDER BY MU.Id DESC

-- ========================================================================
-- 6. CHECK UNIT ALLOCATION HISTORY
-- ========================================================================
PRINT ''
PRINT '=== 6. UNIT ALLOCATION HISTORY ==='

SELECT 
    UAH.Id,
    UAH.PropertyLeaseApplicationId,
    UAH.Event,
    UAH.ApplicationAllocatedPropertyId AS UnitId,
    UAH.UserId,
    UAH.DateCreated
FROM UnitAllocationHistories UAH
WHERE UAH.PropertyLeaseApplicationId = @TestAppId
ORDER BY UAH.DateCreated DESC

-- ========================================================================
-- HELPER: CREATE TEST UNIT (IF NONE AVAILABLE)
-- ========================================================================
PRINT ''
PRINT '=== HELPER: CREATE TEST UNIT SCRIPT ==='
PRINT 'If no matching units found above, run this to create one:'
PRINT ''

DECLARE @ComplexId INT = (SELECT PreferredComplexAreaId FROM PropertyLeaseApplications WHERE Id = @TestAppId)
DECLARE @TypologyId INT = (SELECT HumanEHCOptionsId FROM PropertyLeaseApplications WHERE Id = @TestAppId)
DECLARE @ComplexName NVARCHAR(255) = (SELECT Name FROM PreferredComplexAreas WHERE Id = @ComplexId)
DECLARE @TypologyName NVARCHAR(255) = (SELECT Name FROM HumanEHCOptions WHERE Id = @TypologyId)

PRINT '/*'
PRINT 'INSERT INTO ApplicationAllocatedProperty ('
PRINT '    SpaceUnitNumber, OfferedComplexId, HumanEHCOptionId, '
PRINT '    IsTaken, IsActive, IsDeleted, CreatedDate'
PRINT ') VALUES ('
PRINT '    ''TEST-UNIT-001'', -- Unit Number'
PRINT '    ' + CAST(@ComplexId AS VARCHAR(10)) + ', -- Complex: ' + @ComplexName
PRINT '    ' + CAST(@TypologyId AS VARCHAR(10)) + ', -- Typology: ' + @TypologyName
PRINT '    0, -- Not Taken'
PRINT '    1, -- Active'
PRINT '    0, -- Not Deleted'
PRINT '    GETDATE() -- Created Now'
PRINT ')'
PRINT '*/'

-- ========================================================================
-- VERIFICATION AFTER CEO APPROVAL
-- ========================================================================
PRINT ''
PRINT '=== AFTER CEO APPROVAL - RUN THIS TO VERIFY ==='
PRINT ''
PRINT 'Expected Results After Auto-Match SUCCESS:'
PRINT '  1. Application Status = ''awaited'' (StatusKey)'
PRINT '  2. Waiting List QueueStatus = ''Offered'''
PRINT '  3. Waiting List OfferedUnitId = [Unit ID]'
PRINT '  4. MatchedUnits record exists with IsAccepted = 0'
PRINT '  5. Unit IsTaken = 1'
PRINT '  6. Unit PropertyLeaseApplicationId = ' + CAST(@TestAppId AS VARCHAR(10))
PRINT '  7. UnitAllocationHistories shows "Unit Auto-Allocated by CEO Approval"'
PRINT ''
PRINT 'Expected Results If NO Match:'
PRINT '  1. Application Status = ''s_added_to_waiting_list'''
PRINT '  2. Waiting List QueueStatus = ''Waiting'''
PRINT '  3. Waiting List OfferedUnitId = NULL'
PRINT '  4. No MatchedUnits record'
PRINT '  5. No Unit allocated'

GO
