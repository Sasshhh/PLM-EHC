-- ============================================================
-- FIND TEST APPLICATION FOR USER: sash38
-- ============================================================
-- Run this in your PLM database to find a suitable lease
-- for end-to-end termination flow testing.
-- ============================================================

PRINT '=== 1. Find the Customer record for sash38 ==='
SELECT
    c.Id AS CustomerId,
    c.FirstName,
    c.LastName,
    c.Email,
    su.Id AS SystemUserId,
    su.UserName
FROM Customers c
LEFT JOIN SystemUsers su ON su.Email = c.Email
WHERE su.UserName = 'sash38'
   OR c.Email LIKE '%sash38%'
   OR c.FirstName + c.LastName LIKE '%sash%'

PRINT ''
PRINT '=== 2. All Applications for that Customer ==='
SELECT
    pla.Id AS ApplicationId,
    pla.ApplicationReferenceNumber,
    pla.FirstName,
    pla.LastName,
    s.[Name] AS AppStatus,
    s.[Key] AS AppStatusKey,
    pla.CreatedDateTime
FROM PropertyLeaseApplications pla
INNER JOIN Customers c ON c.Id = pla.CustomerId
LEFT JOIN SystemUsers su ON su.Email = c.Email
LEFT JOIN Status s ON s.Id = pla.StatusId
WHERE su.UserName = 'sash38'
   OR c.Email LIKE '%sash38%'
ORDER BY pla.Id DESC

PRINT ''
PRINT '=== 3. All Lease Details for that Customer ==='
SELECT
    ld.Id AS LeaseDetailsId,
    ld.LeaseReferenceNo,
    ld.FirstNames + ' ' + ld.LastName AS TenantName,
    s.[Name] AS CurrentStatus,
    s.[Key] AS StatusKey,
    ld.IsNew,
    ld.IsActive,
    ld.TerminationNotice,
    ld.NoticeDate,
    ld.EndDate,
    pla.Id AS ApplicationId,
    pla.ApplicationReferenceNumber
FROM LeaseDetails ld
INNER JOIN PropertyLeaseApplications pla ON pla.Id = ld.PropertyLeaseApplicationId
INNER JOIN Customers c ON c.Id = pla.CustomerId
LEFT JOIN SystemUsers su ON su.Email = c.Email
LEFT JOIN Status s ON s.Id = ld.StatusId
WHERE su.UserName = 'sash38'
   OR c.Email LIKE '%sash38%'
ORDER BY ld.Id DESC

PRINT ''
PRINT '=== 4. Check for Existing LeaseTermination Records ==='
SELECT
    lt.Id AS TerminationId,
    lt.PropertyLeaseApplicationId,
    ld.LeaseReferenceNo,
    lt.ReasonForTermination,
    lt.TerminationDate,
    lt.EvictionReferenceNumber,
    lt.TerminationReferenceNumber,
    s.[Name] AS TerminationStatus
FROM LeaseTerminations lt
INNER JOIN PropertyLeaseApplications pla ON pla.Id = lt.PropertyLeaseApplicationId
INNER JOIN Customers c ON c.Id = pla.CustomerId
LEFT JOIN SystemUsers su ON su.Email = c.Email
LEFT JOIN LeaseDetails ld ON ld.PropertyLeaseApplicationId = pla.Id AND ld.IsNew = 1 AND ld.IsDeleted = 0
LEFT JOIN Status s ON s.Id = lt.StatusId
WHERE su.UserName = 'sash38'
   OR c.Email LIKE '%sash38%'
ORDER BY lt.Id DESC

PRINT ''
PRINT '=== 5. Check RoundRobin assignments for this customer ==='
SELECT
    rrq.Id,
    rrq.PropertyLeaseApplicationId,
    rt.[Name] AS ResponsibilityType,
    rs.[Name] AS QueueStatus,
    rrq.CreatedDateTime
FROM RoundRobinQueues rrq
INNER JOIN PropertyLeaseApplications pla ON pla.Id = rrq.PropertyLeaseApplicationId
INNER JOIN Customers c ON c.Id = pla.CustomerId
LEFT JOIN SystemUsers su ON su.Email = c.Email
LEFT JOIN ResponsibilityTypes rt ON rt.Id = rrq.ResponsibilityTypeId
LEFT JOIN Status rs ON rs.Id = rrq.StatusId
WHERE su.UserName = 'sash38'
   OR c.Email LIKE '%sash38%'
ORDER BY rrq.Id DESC

PRINT ''
PRINT '=== 6. RECOMMENDED: Set best lease to AwaitingTerminationAppraisal ==='
PRINT 'Once you identify the LeaseDetailsId from result set 3, run:'
PRINT ''
PRINT 'DECLARE @LeaseId INT = ???   -- paste LeaseDetailsId here'
PRINT 'UPDATE LeaseDetails'
PRINT 'SET StatusId = (SELECT Id FROM Status WHERE [Key] = ''s_awaiting_termination_appraisal'')'
PRINT 'WHERE Id = @LeaseId'
PRINT ''
PRINT 'Then log in as Revenue Officer -> Termination -> Terminations -> lease appears.'
