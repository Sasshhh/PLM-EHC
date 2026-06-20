-- ==========================================================================
-- CHECK CURRENT STATE FOR APPLICATION 5218
-- ==========================================================================

DECLARE @AppId INT = 5218;

PRINT '=== CURRENT STATE FOR APPLICATION 5218 ===';
PRINT '';

-- Application Status
SELECT 
    'APPLICATION STATUS' AS CheckType,
    PLA.Id,
    PLA.ApplicationReferenceNumber,
    S.[Name] AS StatusName,
    S.[Key] AS StatusKey,
    PLA.ModifiedDateTime AS LastModified
FROM PropertyLeaseApplications PLA
INNER JOIN [Status] S ON PLA.StatusId = S.Id
WHERE PLA.Id = @AppId;

-- Matched Unit Status
SELECT 
    'MATCHED UNIT' AS CheckType,
    MU.Id AS MatchedUnitId,
    MU.IsAccepted,
    MU.RejectedProperty,
    MU.CreatedDateTime,
    AAP.Id AS UnitId,
    AAP.SpaceUnitNumber,
    AAP.IsTaken,
    AAP.PropertyLeaseApplicationId AS UnitLinkedToApp
FROM MatchedUnits MU
LEFT JOIN ApplicationAllocatedProperty AAP ON MU.ApplicationAllocatedPropertyId = AAP.Id
WHERE MU.PropertyLeaseApplicationId = @AppId
AND MU.IsDeleted = 0
ORDER BY MU.Id DESC;

-- Waiting List Status
SELECT 
    'WAITING LIST' AS CheckType,
    PLWL.Id,
    PLWL.QueueStatus,
    PLWL.DateAdded,
    PLWL.OfferedUnitId
FROM PropertyLeaseWaitingLists PLWL
WHERE PLWL.PropertyLeaseApplicationId = @AppId;

-- Available s_rcs_awaited Status
SELECT 
    'TARGET STATUS' AS CheckType,
    Id AS StatusId,
    [Name] AS StatusName,
    [Key] AS StatusKey
FROM [Status]
WHERE [Key] = 's_rcs_awaited';
