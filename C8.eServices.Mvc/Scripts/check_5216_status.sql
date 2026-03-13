-- Check application 5216 current status
SELECT a.Id, a.ApplicationReferenceNumber, a.StatusId, s.Name AS StatusName, s.[Key] AS StatusKey
FROM dbo.PropertyLeaseApplications a
JOIN dbo.Status s ON s.Id = a.StatusId
WHERE a.Id = 5216;

-- Check what Lease Agreement related statuses exist
SELECT Id, Name, [Key] FROM dbo.Status WHERE [Key] LIKE '%Lease%' OR Name LIKE '%Lease%' ORDER BY Name;

-- Check ashkay user
SELECT c.Id AS CustomerId, c.FirstName, c.LastName, su.Id AS SystemUserId, su.UserName
FROM dbo.Customers c
JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId
WHERE su.UserName LIKE '%ashkay%' OR c.FirstName LIKE '%ashkay%';

-- Check RoundRobinQueues columns
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RoundRobinQueues' ORDER BY ORDINAL_POSITION;

-- ============================================================
-- SOURCE OF TRUTH: Unit tables diagnostic
-- ApplicationAllocatedProperties = EHC/PLM source of truth
-- Units = empty for Airport Park, legacy HSD path only
-- ============================================================

-- How many available 2-bed units at Airport Park (correct table)
SELECT
    'ApplicationAllocatedProperties (SOURCE OF TRUTH)' AS UnitTable,
    pca.Name AS Complex,
    ehc.Name AS Typology,
    COUNT(*) AS TotalRows,
    SUM(CASE WHEN aap.IsTaken = 0 AND aap.IsDeleted = 0 THEN 1 ELSE 0 END) AS Available,
    SUM(CASE WHEN aap.IsTaken = 1 THEN 1 ELSE 0 END) AS Taken
FROM dbo.ApplicationAllocatedProperties aap
JOIN dbo.PreferredComplexAreas pca ON pca.Id = aap.OfferedComplexId
JOIN dbo.HumanEHCOptions ehc ON ehc.Id = aap.HumanEHCOptionId
WHERE pca.[Key] = 'p_airportpark' AND aap.HumanEHCOptionId = 8  -- 2 Bedroom
GROUP BY pca.Name, ehc.Name;

-- Units table row count for Airport Park (should be 0 - not used for EHC)
SELECT
    'Units (LEGACY - not used for EHC)' AS UnitTable,
    COUNT(*) AS TotalRows
FROM dbo.Units
WHERE PreferredComplexAreaId = 24;

-- Waiting list queue for Airport Park 2-bed
SELECT
    wl.Id, wl.QueueStatus, wl.DateAdded,
    pla.ApplicationReferenceNumber,
    pca.Name AS PreferredComplex,
    ehc.Name AS Typology
FROM dbo.PropertyLeaseWaitingLists wl
JOIN dbo.PropertyLeaseApplications pla ON pla.Id = wl.PropertyLeaseApplicationId
JOIN dbo.PreferredComplexAreas pca ON pca.Id = wl.PreferredComplexId
JOIN dbo.HumanEHCOptions ehc ON ehc.Id = wl.PreferredTypologyId
WHERE wl.PreferredComplexId = 24 AND wl.PreferredTypologyId = 8
ORDER BY wl.DateAdded;
