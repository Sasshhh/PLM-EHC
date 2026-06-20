-- =============================================
-- CHECK WHAT STATUS 99 IS AND WHY RRQ 11578 HAS IT
-- =============================================

USE [eServicesDb]
GO

PRINT '=========================================='
PRINT 'STATUS CHECK FOR RRQ 11578'
PRINT '=========================================='
PRINT ''

-- 1. What is StatusId 99?
PRINT '-------------------------------------------'
PRINT '1. What is StatusId 99?'
PRINT '-------------------------------------------'

SELECT 
    Id,
    [Key],
    Name,
    Description
FROM Status
WHERE Id = 99

PRINT ''

-- 2. What is the "Submitted" status?
PRINT '-------------------------------------------'
PRINT '2. What should the StatusId be (Submitted)?'
PRINT '-------------------------------------------'

SELECT 
    Id,
    [Key],
    Name,
    Description
FROM Status
WHERE [Key] = 'Submitted'

PRINT ''

-- 3. Show the queue record with status details
PRINT '-------------------------------------------'
PRINT '3. RRQ 11578 Details with Status Name:'
PRINT '-------------------------------------------'

SELECT 
    rrq.Id AS QueueId,
    rrq.PropertyLeaseApplicationId,
    rrq.ClerkId,
    c.FirstName + ' ' + c.LastName AS AssignedTo,
    rrq.StatusId,
    s.Name AS StatusName,
    s.[Key] AS StatusKey,
    rt.Name AS QueueType,
    rt.[Key] AS ResponsibilityKey,
    rrq.IsActive,
    rrq.IsDeleted,
    rrq.CreatedDateTime,
    rrq.ModifiedDateTime
FROM RoundRobinQueues rrq
LEFT JOIN Status s ON rrq.StatusId = s.Id
LEFT JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
LEFT JOIN Customers c ON rrq.ClerkId = c.Id
WHERE rrq.Id = 11578

PRINT ''

-- 4. What did SaveMaintenanceSignature set the StatusId to?
PRINT '-------------------------------------------'
PRINT '4. Check SaveMaintenanceSignature Code Expectation:'
PRINT '-------------------------------------------'

PRINT 'SaveMaintenanceSignature should have set:'
PRINT '  StatusId = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted)?.Id'
PRINT ''
PRINT 'If StatusId = 99 and 99 is NOT Submitted,'
PRINT 'then there is a bug in SaveMaintenanceSignature.'
PRINT ''

-- 5. Check if ResponsibilityTypeId 1046 is correct
PRINT '-------------------------------------------'
PRINT '5. Is ResponsibilityTypeId 1046 Correct?'
PRINT '-------------------------------------------'

SELECT 
    Id,
    [Key],
    Name
FROM ResponsibilityTypes
WHERE Id = 1046

SELECT 
    Id,
    [Key],
    Name
FROM ResponsibilityTypes
WHERE [Key] = 'r_property_facilities_manager_review'

PRINT ''

-- 6. What does PropertyFacilitiesMaintenanceReviews look for?
PRINT '-------------------------------------------'
PRINT '6. Controller Filter Criteria:'
PRINT '-------------------------------------------'

DECLARE @UserId INT = 1287
DECLARE @SubmittedStatusId INT = (SELECT Id FROM Status WHERE [Key] = 'Submitted')
DECLARE @FacilitiesReviewId INT = (SELECT Id FROM ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review')

PRINT 'PropertyFacilitiesMaintenanceReviews controller looks for:'
PRINT '  ClerkId = ' + CAST(@UserId AS VARCHAR)
PRINT '  StatusId = ' + CAST(@SubmittedStatusId AS VARCHAR) + ' (Submitted)'
PRINT '  ResponsibilityTypeId = ' + CAST(@FacilitiesReviewId AS VARCHAR) + ' (r_property_facilities_manager_review)'
PRINT ''

PRINT 'Your RRQ 11578 has:'
SELECT 
    'ClerkId = ' + CAST(ClerkId AS VARCHAR) + CASE WHEN ClerkId = @UserId THEN ' ✅' ELSE ' ❌' END AS ClerkIdCheck,
    'StatusId = ' + CAST(StatusId AS VARCHAR) + ' (' + s.Name + ')' + CASE WHEN StatusId = @SubmittedStatusId THEN ' ✅' ELSE ' ❌ WRONG!' END AS StatusIdCheck,
    'ResponsibilityTypeId = ' + CAST(ResponsibilityTypeId AS VARCHAR) + CASE WHEN ResponsibilityTypeId = @FacilitiesReviewId THEN ' ✅' ELSE ' ❌' END AS ResponsibilityCheck
FROM RoundRobinQueues rrq
LEFT JOIN Status s ON rrq.StatusId = s.Id
WHERE rrq.Id = 11578

PRINT ''

-- 7. SOLUTION
PRINT '=========================================='
PRINT 'SOLUTION:'
PRINT '=========================================='
PRINT ''

IF (SELECT StatusId FROM RoundRobinQueues WHERE Id = 11578) != @SubmittedStatusId
BEGIN
    PRINT '❌ PROBLEM IDENTIFIED:'
    PRINT '   RRQ 11578 has WRONG StatusId'
    PRINT ''
    PRINT '✅ QUICK FIX:'
    PRINT '   UPDATE RoundRobinQueues'
    PRINT '   SET StatusId = ' + CAST(@SubmittedStatusId AS VARCHAR) + '  -- Submitted'
    PRINT '   WHERE Id = 11578'
    PRINT ''
    PRINT 'OR'
    PRINT ''
    PRINT '✅ PERMANENT FIX:'
    PRINT '   Fix SaveMaintenanceSignature (line ~17640)'
    PRINT '   Change: StatusId = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted)?.Id'
    PRINT '   This line is setting wrong status in the code'
    PRINT ''
END
ELSE
BEGIN
    PRINT '✅ Status is correct - problem is elsewhere'
END

PRINT '=========================================='
PRINT 'Diagnostic Complete'
PRINT '=========================================='
