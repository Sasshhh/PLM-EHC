-- ==========================================================================
-- ROLLBACK UNIT ACCEPTANCE
-- ==========================================================================
-- This script rolls back a unit acceptance so you can test the flow again
--
-- USAGE:
-- Replace @AppId with your application ID
-- ==========================================================================

DECLARE @AppId INT = 5218; -- YOUR APPLICATION ID

BEGIN TRANSACTION;

PRINT '=== Starting Rollback for Application ' + CAST(@AppId AS VARCHAR(10)) + ' ===';

-- 1. GET CURRENT STATE (For Verification)
PRINT '';
PRINT '--- CURRENT STATE ---';
SELECT 
    'Application' AS Entity,
    PLA.Id AS AppId,
    PLA.ApplicationReferenceNumber AS RefNo,
    S.[Name] AS CurrentStatus,
    S.[Key] AS StatusKey
FROM PropertyLeaseApplications PLA
INNER JOIN [Status] S ON PLA.StatusId = S.Id
WHERE PLA.Id = @AppId;

SELECT 
    'MatchedUnit' AS Entity,
    MU.Id AS MatchedUnitId,
    MU.IsAccepted,
    MU.RejectedProperty,
    AAP.SpaceUnitNumber,
    AAP.IsTaken
FROM MatchedUnits MU
INNER JOIN ApplicationAllocatedProperties AAP ON MU.ApplicationAllocatedPropertyId = AAP.Id
WHERE MU.PropertyLeaseApplicationId = @AppId
AND MU.IsDeleted = 0
ORDER BY MU.Id DESC;

PRINT '';
PRINT '--- ROLLING BACK CHANGES ---';

-- 2. ROLLBACK MATCHED UNIT (Set IsAccepted = FALSE)
UPDATE MatchedUnits
SET 
    IsAccepted = 0,
    ModifiedDateTime = GETDATE()
WHERE PropertyLeaseApplicationId = @AppId
AND IsDeleted = 0;

PRINT 'Matched Units: Reset IsAccepted to FALSE';

-- 3. ROLLBACK UNIT ALLOCATION (Set IsTaken = FALSE, Clear Application Link)
DECLARE @UnitId INT;

SELECT TOP 1 @UnitId = ApplicationAllocatedPropertyId
FROM MatchedUnits
WHERE PropertyLeaseApplicationId = @AppId
AND IsDeleted = 0
ORDER BY Id DESC;

IF @UnitId IS NOT NULL
BEGIN
    -- Check if AllocatedByUserId can be NULL, otherwise just set IsTaken
    UPDATE ApplicationAllocatedProperties
    SET 
        IsTaken = 0,
        ModifiedDateTime = GETDATE()
    WHERE Id = @UnitId;

    PRINT 'Unit ' + CAST(@UnitId AS VARCHAR(10)) + ': Reset to Available (IsTaken = FALSE)';
END

-- 4. ROLLBACK APPLICATION STATUS (Back to "Awaited" - s_rcs_awaited)
DECLARE @AwaitedStatusId INT;
DECLARE @CurrentStatusKey VARCHAR(100);

-- Get current status
SELECT @CurrentStatusKey = S.[Key]
FROM PropertyLeaseApplications PLA
INNER JOIN [Status] S ON PLA.StatusId = S.Id
WHERE PLA.Id = @AppId;

PRINT 'Current Status: ' + ISNULL(@CurrentStatusKey, 'NULL');

-- Get Awaited status ID
SELECT @AwaitedStatusId = Id
FROM [Status]
WHERE [Key] = 's_rcs_awaited';

IF @AwaitedStatusId IS NOT NULL
BEGIN
    UPDATE PropertyLeaseApplications
    SET 
        StatusId = @AwaitedStatusId,
        ModifiedDateTime = GETDATE()
    WHERE Id = @AppId;

    PRINT 'Application Status: Reset to Awaited (s_rcs_awaited) from ' + ISNULL(@CurrentStatusKey, 'NULL');
END
ELSE
BEGIN
    PRINT 'WARNING: Could not find s_rcs_awaited status!';
END

-- 5. ROLLBACK WAITING LIST (Set Back to "Offered")
UPDATE PropertyLeaseWaitingLists
SET 
    QueueStatus = 'Offered',
    ModifiedDateTime = GETDATE()
WHERE PropertyLeaseApplicationId = @AppId
AND QueueStatus != 'Waiting'; -- Only reset if it was changed

PRINT 'Waiting List: Reset to Offered';

PRINT '';
PRINT '--- NEW STATE (After Rollback) ---';

-- 6. VERIFY NEW STATE
SELECT 
    'Application' AS Entity,
    PLA.Id AS AppId,
    PLA.ApplicationReferenceNumber AS RefNo,
    S.[Name] AS CurrentStatus,
    S.[Key] AS StatusKey
FROM PropertyLeaseApplications PLA
INNER JOIN [Status] S ON PLA.StatusId = S.Id
WHERE PLA.Id = @AppId;

SELECT 
    'MatchedUnit' AS Entity,
    MU.Id AS MatchedUnitId,
    MU.IsAccepted,
    MU.RejectedProperty,
    AAP.SpaceUnitNumber,
    AAP.IsTaken
FROM MatchedUnits MU
INNER JOIN ApplicationAllocatedProperties AAP ON MU.ApplicationAllocatedPropertyId = AAP.Id
WHERE MU.PropertyLeaseApplicationId = @AppId
AND MU.IsDeleted = 0
ORDER BY MU.Id DESC;

PRINT '';
PRINT '=== Rollback Complete ===';
PRINT 'You can now test the unit acceptance flow again.';

-- COMMIT THE TRANSACTION
COMMIT;

-- ==========================================================================
-- NOTES:
-- ==========================================================================
-- After running this script:
-- 1. Application Status = "Awaited" (Unit Offer Pending)
-- 2. MatchedUnit.IsAccepted = FALSE
-- 3. Unit.IsTaken = FALSE (Available)
-- 4. Waiting List = "Offered"
--
-- Next Steps:
-- 1. Login as the applicant
-- 2. Navigate to the unit details page
-- 3. Click "Accept Unit Offer"
-- 4. Verify the modal shows:
--    - Proper layout (not stretched)
--    - Deposit amount from unit details
-- ==========================================================================
