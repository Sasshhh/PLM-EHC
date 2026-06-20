-- ==========================================================================
-- VERIFY ROLLBACK FOR APPLICATION 5218
-- ==========================================================================
-- This script verifies the rollback was successful
-- ==========================================================================

DECLARE @AppId INT = 5218;

PRINT '=== VERIFICATION RESULTS FOR APPLICATION ' + CAST(@AppId AS VARCHAR(10)) + ' ===';
PRINT '';

-- 1. CHECK APPLICATION STATUS
PRINT '--- APPLICATION STATUS ---';
SELECT 
    PLA.Id AS AppId,
    PLA.ApplicationReferenceNumber AS RefNo,
    S.[Name] AS CurrentStatus,
    S.[Key] AS StatusKey,
    CASE 
        WHEN S.[Key] = 's_rcs_awaited' THEN '✓ CORRECT (Ready for acceptance)'
        ELSE '✗ WRONG (Should be s_rcs_awaited)'
    END AS VerificationStatus
FROM PropertyLeaseApplications PLA
INNER JOIN [Status] S ON PLA.StatusId = S.Id
WHERE PLA.Id = @AppId;

PRINT '';
PRINT '--- MATCHED UNIT STATUS ---';
-- 2. CHECK MATCHED UNIT
SELECT 
    MU.Id AS MatchedUnitId,
    MU.PropertyLeaseApplicationId,
    MU.IsAccepted,
    CASE 
        WHEN MU.IsAccepted = 0 THEN '✓ CORRECT (Not accepted)'
        ELSE '✗ WRONG (Should be FALSE)'
    END AS AcceptedStatus,
    MU.RejectedProperty,
    AAP.SpaceUnitNumber AS UnitNumber,
    AAP.IsTaken,
    CASE 
        WHEN AAP.IsTaken = 0 THEN '✓ CORRECT (Available)'
        ELSE '✗ WRONG (Should be FALSE/Available)'
    END AS UnitAvailability
FROM MatchedUnits MU
INNER JOIN ApplicationAllocatedProperty AAP ON MU.ApplicationAllocatedPropertyId = AAP.Id
WHERE MU.PropertyLeaseApplicationId = @AppId
AND MU.IsDeleted = 0
ORDER BY MU.Id DESC;

PRINT '';
PRINT '--- WAITING LIST STATUS ---';
-- 3. CHECK WAITING LIST
SELECT 
    PLWL.Id,
    PLWL.PropertyLeaseApplicationId,
    PLWL.QueueStatus,
    CASE 
        WHEN PLWL.QueueStatus = 'Offered' THEN '✓ CORRECT (Unit still offered)'
        WHEN PLWL.QueueStatus = 'Waiting' THEN '⚠ WARNING (Back in queue)'
        ELSE '✗ WRONG (Unexpected status)'
    END AS WaitingListStatus,
    PLWL.DateAdded,
    PLWL.OfferedUnitId
FROM PropertyLeaseWaitingLists PLWL
WHERE PLWL.PropertyLeaseApplicationId = @AppId;

PRINT '';
PRINT '=== SUMMARY ===';
PRINT 'If all statuses show ✓ CORRECT, the rollback was successful.';
PRINT 'You can now test the unit acceptance flow again.';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Rebuild solution (Ctrl+Shift+B)';
PRINT '2. Start debugging (F5)';
PRINT '3. Login as applicant';
PRINT '4. View unit details for application 5218';
PRINT '5. Click "Accept Unit Offer"';
PRINT '6. Verify the modal shows deposit amount and proper layout';
